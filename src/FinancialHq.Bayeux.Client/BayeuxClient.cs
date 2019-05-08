using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Connectivity;
using FinancialHq.Bayeux.Client.Listeners;
using FinancialHq.Bayeux.Client.Logging;
using FinancialHq.Bayeux.Client.Options;

namespace FinancialHq.Bayeux.Client
{
    public class BayeuxClient : IDisposable, IBayeuxClient
    {
        internal static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IBayeuxClientContext _context;
        private readonly IEnumerable<IMessageListener> _messageListeners;
        private readonly ISubscriberCache _subscriberCache;
        private readonly ConnectLoop _connectLoop;


        /// <param name="context"></param>
        /// <param name="messageListeners">Collection of Listeners</param>
        /// <param name="subscriberCache"></param>
        /// <param name="delayOptions">
        /// When a request results in network errors, reconnection trials will be delayed based on the 
        /// values passed here. The last element of the collection will be re-used indefinitely.
        /// </param>
        public BayeuxClient(
            IBayeuxClientContext context,
            IEnumerable<IMessageListener> messageListeners,
            ISubscriberCache subscriberCache,
            ReconnectDelayOptions delayOptions = null)
        {
            _context = context;
            _messageListeners = messageListeners;
            _connectLoop = new ConnectLoop("long-polling", delayOptions?.ReconnectDelays, _context);
            _subscriberCache = subscriberCache;
            _context.OnNewConnection += OnNewConnection;
        }

        

        // TODO: add a new method to Start without failing when first connection has failed.

        /// <inheritdoc />
        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling.
        /// Handshake does not support re-negotiation; it fails at first unsuccessful response.
        /// </summary>
        public Task Start(CancellationToken cancellationToken = default) =>
            _connectLoop.Start(cancellationToken);

        /// <inheritdoc />
        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling in the background with reconnects as needed.
        /// This method does not fail.
        /// </summary>
        public void StartInBackground() =>
            _connectLoop.StartInBackground();

        public async Task Stop(CancellationToken cancellationToken = default)
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
            _subscriberCache.OnConnected();
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

        public AbstractChannel GetChannel(ChannelId channelId)
        {
            return _context.GetChannel(channelId.ToString());
        }

        public void AddChannel(AbstractChannel channel)
        {
            _context.AddChannel(channel.ChannelId.ToString(), channel);
        }

        public IDisposable Subscribe<T>(ChannelId channelId, CancellationToken cancellationToken, bool throwIfNotConnected) where T : class, IMessageListener
        {
            var channel = _context.GetChannel(channelId.ToString());
            _context.AddChannel(channel.ToString(), channel);
            return Subscribe<T>(channel, cancellationToken, throwIfNotConnected);
        }

        public IDisposable Subscribe<T>(AbstractChannel channel, CancellationToken cancellationToken, bool throwIfNotConnected) where T : class, IMessageListener
        {
            var listener = GetListener<T>();
            _subscriberCache.AddSubscription(new []{ channel.ChannelId });
            return channel.Subscribe(listener);
        }

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Subscribe(ChannelId channel, CancellationToken cancellationToken = default) =>
            Subscribe(new[] { channel }, cancellationToken);

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Unsubscribe(ChannelId channel, CancellationToken cancellationToken = default) =>
            Unsubscribe(new[] { channel }, cancellationToken);

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Subscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default) =>
            SubscribeImpl(channels, cancellationToken, throwIfNotConnected: true);

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        public Task Unsubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default) =>
            UnsubscribeImpl(channels, cancellationToken, throwIfNotConnected: true);

        #endregion

        protected T GetListener<T>() where T : IMessageListener
        {
            foreach (var item in _messageListeners)
            {
                if (item.GetType() != typeof(T))
                {
                    continue;
                }

                Log.Debug("Found Listener match");
                return (T)item;
            }

            throw new ApplicationException($"{typeof(T).Name} hander is not found in the registry");
        }

        private Task SubscribeImpl(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected)
        {
            var channelIds = channels as ChannelId[] ?? channels.ToArray();
            _subscriberCache.AddSubscription(channelIds);
            return RequestSubscribe(channelIds, cancellationToken, throwIfNotConnected);
        }

        private Task UnsubscribeImpl(IEnumerable<ChannelId> channels, CancellationToken cancellationToken, bool throwIfNotConnected)
        {
            var channelIds = channels as ChannelId[] ?? channels.ToArray();
            _subscriberCache.RemoveSubscription(channelIds);
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
