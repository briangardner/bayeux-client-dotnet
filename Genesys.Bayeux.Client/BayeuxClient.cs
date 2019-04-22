﻿using Genesys.Bayeux.Client.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Options;

namespace Genesys.Bayeux.Client
{
    public class BayeuxClient : IDisposable, IBayeuxClient
    {
        internal static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IBayeuxClientContext _context;
        private readonly Subscriber _subscriber;
        private readonly ConnectLoop _connectLoop;


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
            IBayeuxClientContext context,
            ReconnectDelayOptions delayOptions = null)
        {
            _context = context;
            _connectLoop = new ConnectLoop("long-polling", delayOptions?.ReconnectDelays, _context);
            _subscriber = new Subscriber(_context);
            _context.OnNewConnection += OnNewConnection;
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
            await _context.Disconnect(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Stop().GetAwaiter().GetResult();
        }

        private void OnNewConnection(object sender, EventArgs e)
        {
            _subscriber.OnConnected();
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

        private Task SubscribeImpl(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected)
        {
            var channelIds = channels as ChannelId[] ?? channels.ToArray();
            _subscriber.AddSubscription(channelIds);
            return RequestSubscribe(channelIds, cancellationToken, throwIfNotConnected);
        }

        private Task UnsubscribeImpl(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected)
        {
            var channelIds = channels as ChannelId[] ?? channels.ToArray();
            _subscriber.RemoveSubscription(channelIds);
            return RequestUnsubscribe(channelIds, cancellationToken, throwIfNotConnected);
        }

        private Task RequestSubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected) =>
            RequestSubscription(channels, Enumerable.Empty<ChannelId>(), cancellationToken, throwIfNotConnected);

        private Task RequestUnsubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected) =>
            RequestSubscription(Enumerable.Empty<ChannelId>(), channels, cancellationToken, throwIfNotConnected);

        private async Task RequestSubscription(
            IEnumerable<ChannelId> channelsToSubscribe,
            IEnumerable<ChannelId> channelsToUnsubscribe,
            CancellationToken cancellationToken,
            bool throwIfNotConnected)
        {
            if (!_context.IsConnected())
            {
                if (throwIfNotConnected)
                    throw new InvalidOperationException("Not connected. Operation will be effective on next connection.");
            }
            else
            {
                var messages = channelsToSubscribe.Select(ch => _context.GetChannel(ch.ToString()).GetSubscribeMessage())
                    .Concat(channelsToUnsubscribe.Select(ch => _context.GetChannel(ch.ToString()).GetUnsubscribeMessage()));
                await _context.RequestMany(messages,cancellationToken).ConfigureAwait(false);
            }
        }


        
       
    }
}
