using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Enums;
using Genesys.Bayeux.Client.Exceptions;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Transport;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client
{
    class BayeuxClientContext : IBayeuxClientContext, IDisposable
    {
        internal static ILog log = LogProvider.GetCurrentClassLogger();

        private readonly IBayeuxTransport _transport;
        private readonly TaskScheduler _eventTaskScheduler;
        protected volatile int currentConnectionState = -1;
        volatile BayeuxConnection _currentConnection;
        public ConcurrentDictionary<string, AbstractChannel> Channels { get; } = new ConcurrentDictionary<string, AbstractChannel>();
        public IEnumerable<IExtension> Extensions { get; }

        public event EventHandler OnNewConnection;

        public event EventHandler<ConnectionStateChangedArgs> ConnectionStateChanged;

        public BayeuxClientContext(IBayeuxTransport transport, IEnumerable<IExtension> extensions, TaskScheduler eventTaskScheduler = null)
        {
            _transport = transport;
            Extensions = extensions;
            _eventTaskScheduler = ChooseEventTaskScheduler(eventTaskScheduler);

        }
        Task IBayeuxClientContext.Open(CancellationToken cancellationToken)
            => _transport.Open(cancellationToken);

        Task IBayeuxClientContext.SetConnectionState(ConnectionState newState)
            => OnConnectionStateChangedAsync(newState);


        public async Task Disconnect(CancellationToken cancellationToken)
        {
            var connection = Interlocked.Exchange(ref _currentConnection, null);
            if (connection != null)
                await connection.Disconnect(cancellationToken).ConfigureAwait(false);
        }

        public bool IsConnected()
        {
            var connection = _currentConnection;
            return connection != null;
        }

        async Task RunInEventTaskScheduler(Action action) =>
            await Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, _eventTaskScheduler).ConfigureAwait(false);


        public Task<JObject> Request(BayeuxMessage request, CancellationToken cancellationToken)
        {
            return RequestMany(new[] { request }, cancellationToken);
        }

        public async Task<JObject> RequestMany(IEnumerable<BayeuxMessage> requests, CancellationToken cancellationToken)
        {
            // https://docs.cometd.org/current/reference/#_messages
            // All Bayeux messages SHOULD be encapsulated in a JSON encoded array so that multiple messages may be transported together
            var enumerable = requests.ToList();
            foreach (var request in enumerable.ToList())
            {
                AddClientId(request);
            }
            log.Debug("Sending Request: {@enumerable}", enumerable);
            var responseObj = await _transport.Request(enumerable, cancellationToken).ConfigureAwait(false);
            log.Debug("Received Response: {@responseObj}", responseObj);
            var response = responseObj.ToObject<BayeuxResponse>();

            if (!response.successful)
                throw new BayeuxRequestException(response.error, responseObj["advice"]?.ToObject<BayeuxAdvice>());

            return responseObj;
        }

        protected internal virtual async Task  OnConnectionStateChangedAsync(ConnectionState state)
        {
            var oldConnectionState = Interlocked.Exchange(ref currentConnectionState, (int)state);

            if (oldConnectionState != (int)state)
                await RunInEventTaskScheduler(() =>
                    ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedArgs(state))).ConfigureAwait(false);
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

        public void SetConnection(BayeuxConnection newConnection)
        {
            _currentConnection = newConnection;
            OnNewConnection?.Invoke(this,null);
        }


        public void Dispose()
        {
            _transport?.Dispose();
        }

        private void AddClientId(BayeuxMessage message)
        {
            if (!message.ContainsKey("clientId") && IsConnected())
            {
                message["clientId"] = _currentConnection.ClientId;
            }
        }
    }
}
