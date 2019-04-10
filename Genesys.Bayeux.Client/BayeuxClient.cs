﻿using Genesys.Bayeux.Client.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using static Genesys.Bayeux.Client.Logging.LogProvider;

namespace Genesys.Bayeux.Client
{
    public class BayeuxClient : IDisposable, IBayeuxClientContext, IObserver<JObject>
    {
        internal static readonly ILog log;
        public ConcurrentDictionary<string, AbstractChannel> Channels { get; } = new ConcurrentDictionary<string, AbstractChannel>();

        static BayeuxClient()
        {
            LogProvider.LogProviderResolvers.Add(
                new Tuple<IsLoggerAvailable, CreateLogProvider>(() => true, () => new TraceSourceLogProvider()));

            log = LogProvider.GetLogger(typeof(BayeuxClient).Namespace);
        }

        readonly IBayeuxTransport _transport;
        readonly TaskScheduler _eventTaskScheduler;
        readonly Subscriber _subscriber;
        readonly ConnectLoop _connectLoop;

        volatile BayeuxConnection _currentConnection;

        /// <param name="eventTaskScheduler">
        /// <para>
        /// TaskScheduler for invoking events. Usually, you will be good by providing null. If you decide to 
        /// your own TaskScheduler, please make sure that it guarantees ordered execution of events.
        /// </para>
        /// <para>
        /// If null is provided, SynchronizationContext.Current will be used. This means that WPF and 
        /// Windows Forms applications will run events appropriately. If SynchronizationContext.Current
        /// is null, then a new TaskScheduler with ordered execution will be created.
        /// </para>
        /// </param>
        /// <param name="delayOptions">
        /// When a request results in network errors, reconnection trials will be delayed based on the 
        /// values passed here. The last element of the collection will be re-used indefinitely.
        /// </param>
        public BayeuxClient(
            IBayeuxTransport transport,
            ReconnectDelayOptions delayOptions = null,
            TaskScheduler eventTaskScheduler = null)
        {
            this._transport = transport;
            transport.Observers.Add(this);
            this._eventTaskScheduler = ChooseEventTaskScheduler(eventTaskScheduler);
            this._connectLoop = new ConnectLoop("long-polling", delayOptions?.ReconnectDelays, this);
            this._subscriber = new Subscriber(this);
        }

        static TaskScheduler ChooseEventTaskScheduler(TaskScheduler eventTaskScheduler)
        {
            if (eventTaskScheduler != null)
                return eventTaskScheduler;
            
            if (SynchronizationContext.Current != null)
            {
                log.Info($"Using current SynchronizationContext for events: {SynchronizationContext.Current}");
                return TaskScheduler.FromCurrentSynchronizationContext();
            }
            else
            {
                log.Info("Using a new TaskScheduler with ordered execution for events.");
                return new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            }
        }

        // TODO: add a new method to Start without failing when first connection has failed.

        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling.
        /// Handshake does not support re-negotiation; it fails at first unsuccessful response.
        /// </summary>
        public Task Start(CancellationToken cancellationToken = default(CancellationToken)) =>
            _connectLoop.Start(cancellationToken);

        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling in the background with reconnects as needed.
        /// This method does not fail.
        /// </summary>
        public void StartInBackground() =>
            _connectLoop.StartInBackground();

        public async Task Stop(CancellationToken cancellationToken = default(CancellationToken))
        {
            _connectLoop.Dispose();

            var connection = Interlocked.Exchange(ref _currentConnection, null);
            if (connection != null)
                await connection.Disconnect(cancellationToken);
        }

        public void Dispose()
        {
            _ = Stop();
        }

        public enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
            DisconnectedOnError,
        }

        public class ConnectionStateChangedArgs : EventArgs
        {
            public ConnectionState ConnectionState { get; private set; }

            public ConnectionStateChangedArgs(ConnectionState state)
            {
                ConnectionState = state;
            }

            public override string ToString() => ConnectionState.ToString();
        }

        protected volatile int currentConnectionState = -1;

        public event EventHandler<ConnectionStateChangedArgs> ConnectionStateChanged;

        protected internal virtual async Task OnConnectionStateChangedAsync(ConnectionState state)
        {
            var oldConnectionState = Interlocked.Exchange(ref currentConnectionState, (int) state);

            if (oldConnectionState != (int)state)
                await RunInEventTaskScheduler(() =>
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedArgs(state))).ConfigureAwait(false);
        }

        internal void SetNewConnection(BayeuxConnection newConnection)
        {
            _currentConnection = newConnection;
            _subscriber.OnConnected();
        }

        // On defining .NET events
        // https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/event
        // https://stackoverflow.com/questions/3880789/why-should-we-use-eventhandler
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines
        public class EventReceivedArgs : EventArgs
        {
            readonly JObject ev;

            public EventReceivedArgs(JObject ev)
            {
                this.ev = ev;
            }

            // https://docs.cometd.org/current/reference/#_code_data_code
            // The data message field is an arbitrary JSON encoded *object*
            public JObject Data { get => (JObject) ev["data"]; }

            public string Channel { get => (string) ev["channel"]; }

            public JObject Message { get => ev; }

            public override string ToString() => ev.ToString();
        }


        #region Public subscription methods

        /// <summary>
        /// Adds a subscription. Subscribes immediately if this BayeuxClient is connected.
        /// This is equivalent to <see cref="Subscribe(IEnumerable{ChannelId}, CancellationToken)"/>, but does not await result and does not throw an exception if disconnected.
        /// </summary>
        /// <param name="channels"></param>
        public void AddSubscriptions(params ChannelId[] channels) =>
            SubscribeImpl(channels, CancellationToken.None, throwIfNotConnected: false);

        /// <summary>
        /// Removes subscriptions. Unsubscribes immediately if this BayeuxClient is connected.
        /// This is equivalent to <see cref="Unsubscribe(IEnumerable{ChannelId}, CancellationToken)"/>, but does not await result and does not throw an exception if disconnected.
        /// </summary>
        /// <param name="channels"></param>
        public void RemoveSubscriptions(params ChannelId[] channels) =>
            UnsubscribeImpl(channels, CancellationToken.None, throwIfNotConnected: false);


        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Subscribe(ChannelId channel, CancellationToken cancellationToken = default(CancellationToken)) =>
            Subscribe(new[] { channel }, cancellationToken);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Unsubscribe(ChannelId channel, CancellationToken cancellationToken = default(CancellationToken)) =>
            Unsubscribe(new[] { channel }, cancellationToken);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Subscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default(CancellationToken)) =>
            SubscribeImpl(channels, cancellationToken, throwIfNotConnected: true);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Unsubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default(CancellationToken)) =>
            UnsubscribeImpl(channels, cancellationToken, throwIfNotConnected: true);

        #endregion

        Task SubscribeImpl(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected)
        {
            _subscriber.AddSubscription(channels);
            return RequestSubscribe(channels, cancellationToken, throwIfNotConnected);
        }

        Task UnsubscribeImpl(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected)
        {
            _subscriber.RemoveSubscription(channels);
            return RequestUnsubscribe(channels, cancellationToken, throwIfNotConnected);
        }

        internal Task RequestSubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected) =>
            RequestSubscription(channels, Enumerable.Empty<ChannelId>(), cancellationToken, throwIfNotConnected);

        Task RequestUnsubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected) =>
            RequestSubscription(Enumerable.Empty<ChannelId>(), channels, cancellationToken, throwIfNotConnected);

        async Task RequestSubscription(
            IEnumerable<ChannelId> channelsToSubscribe,
            IEnumerable<ChannelId> channelsToUnsubscribe,
            CancellationToken cancellationToken,
            bool throwIfNotConnected)
        {
            var connection = _currentConnection;
            if (connection == null)
            {
                if (throwIfNotConnected)
                    throw new InvalidOperationException("Not connected. Operation will be effective on next connection.");
            }
            else
            {
                var messages = channelsToSubscribe.Select(ch => this.GetChannel(ch.ToString()).GetSubscribeMessage())
                    .Concat(channelsToUnsubscribe.Select(ch => this.GetChannel(ch.ToString()).GetUnsubscribeMessage()));
                await RequestMany(messages,cancellationToken).ConfigureAwait(false);
            }
        }

        internal Task<JObject> Request(object request, CancellationToken cancellationToken)
        {
            Trace.Assert(!(request is System.Collections.IEnumerable), "Use method RequestMany");
            return RequestMany(new[] { request }, cancellationToken);
        }

        internal async Task<JObject> RequestMany(IEnumerable<object> requests, CancellationToken cancellationToken)
        {
            // https://docs.cometd.org/current/reference/#_messages
            // All Bayeux messages SHOULD be encapsulated in a JSON encoded array so that multiple messages may be transported together
            var responseObj = await _transport.Request(requests, cancellationToken);

            var response = responseObj.ToObject<BayeuxResponse>();

            if (!response.successful)
                throw new BayeuxRequestException(response.error);

            return responseObj;
        }

        /*async Task PublishEvents(IEnumerable<JObject> events) =>
            await RunInEventTaskScheduler(() =>
            {
                var observable = events.ToObservable();
                observable.Subscribe((ev) =>
                {
                    var channel = this.GetChannel((string)ev["channel"]);
                    channel.OnNext(ev.ToObject<IMessage>());
                });
            }).ConfigureAwait(false);*/

        async Task RunInEventTaskScheduler(Action action) =>
            await Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _eventTaskScheduler).ConfigureAwait(false);

        Task IBayeuxClientContext.Open(CancellationToken cancellationToken)
            => _transport.Open(cancellationToken);

        Task<JObject> IBayeuxClientContext.Request(object request, CancellationToken cancellationToken)
            => Request(request, cancellationToken);

        Task<JObject> IBayeuxClientContext.RequestMany(IEnumerable<object> requests, CancellationToken cancellationToken)
            => RequestMany(requests, cancellationToken);

        Task IBayeuxClientContext.SetConnectionState(ConnectionState newState)
            => OnConnectionStateChangedAsync(newState);

        void IBayeuxClientContext.SetConnection(BayeuxConnection newConnection)
            => SetNewConnection(newConnection);

        public void OnCompleted()
        {
            //do nothing
        }

        public void OnError(Exception error)
        {
            log.Log(LogLevel.Error, () => "Error with message", error);
        }

        public void OnNext(JObject value)
        {
            var channel = this.GetChannel((string) value["channel"]);
            channel.OnNext(value.ToObject<IMessage>());
        }
    }
}
