using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client
{
    public interface IBayeuxClient
    {
        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling.
        /// Handshake does not support re-negotiation; it fails at first unsuccessful response.
        /// </summary>
        Task Start(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling in the background with reconnects as needed.
        /// This method does not fail.
        /// </summary>
        void StartInBackground();

        Task Stop(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Adds a subscription. Subscribes immediately if this BayeuxClient is connected.
        /// This is equivalent to <see cref="BayeuxClient.Subscribe(System.Collections.Generic.IEnumerable{Genesys.Bayeux.Client.Channels.ChannelId},System.Threading.CancellationToken)"/>, but does not await result and does not throw an exception if disconnected.
        /// </summary>
        /// <param name="channels"></param>
        void AddSubscriptions(params ChannelId[] channels);

        /// <summary>
        /// Removes subscriptions. Unsubscribes immediately if this BayeuxClient is connected.
        /// This is equivalent to <see cref="BayeuxClient.Unsubscribe(System.Collections.Generic.IEnumerable{Genesys.Bayeux.Client.Channels.ChannelId},System.Threading.CancellationToken)"/>, but does not await result and does not throw an exception if disconnected.
        /// </summary>
        /// <param name="channels"></param>
        void RemoveSubscriptions(params ChannelId[] channels);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        Task Subscribe(ChannelId channel, CancellationToken cancellationToken = default(CancellationToken));

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        Task Unsubscribe(ChannelId channel, CancellationToken cancellationToken = default(CancellationToken));

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        Task Subscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default(CancellationToken));

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        Task Unsubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default(CancellationToken));
    }
}