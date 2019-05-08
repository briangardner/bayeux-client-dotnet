using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Listeners;

namespace FinancialHq.Bayeux.Client
{
    public interface IBayeuxClient
    {
        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling.
        /// Handshake does not support re-negotiation; it fails at first unsuccessful response.
        /// </summary>
        Task Start(CancellationToken cancellationToken = default);

        /// <summary>
        /// Does the Bayeux handshake, and starts long-polling in the background with reconnects as needed.
        /// This method does not fail.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        void StartInBackground();

        Task Stop(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a subscription. Subscribes immediately if this BayeuxClient is connected.
        /// This is equivalent to <see cref="BayeuxClient.Subscribe(System.Collections.Generic.IEnumerable{FinancialHq.Bayeux.Client.Channels.ChannelId},System.Threading.CancellationToken)"/>, but does not await result and does not throw an exception if disconnected.
        /// </summary>
        /// <param name="channels"></param>
        // ReSharper disable once UnusedMember.Global
        void AddSubscriptions(params ChannelId[] channels);

        /// <summary>
        /// Removes subscriptions. Unsubscribes immediately if this BayeuxClient is connected.
        /// This is equivalent to <see cref="BayeuxClient.Unsubscribe(System.Collections.Generic.IEnumerable{FinancialHq.Bayeux.Client.Channels.ChannelId},System.Threading.CancellationToken)"/>, but does not await result and does not throw an exception if disconnected.
        /// </summary>
        /// <param name="channels"></param>
        // ReSharper disable once UnusedMember.Global
        void RemoveSubscriptions(params ChannelId[] channels);

        AbstractChannel GetChannel(ChannelId channelId);
        void AddChannel(AbstractChannel channel);

        // ReSharper disable once UnusedMember.Global
        IDisposable Subscribe<T>(ChannelId channelId, CancellationToken cancellationToken, bool throwIfNotConnected) where T : class, IMessageListener;
        IDisposable Subscribe<T>(AbstractChannel channel, CancellationToken cancellationToken, bool throwIfNotConnected) where T : class, IMessageListener;

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        // ReSharper disable once UnusedMember.Global
        Task Subscribe(ChannelId channel, CancellationToken cancellationToken = default);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        // ReSharper disable once UnusedMember.Global
        Task Unsubscribe(ChannelId channel, CancellationToken cancellationToken = default);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        // ReSharper disable once UnusedMember.Global
        Task Subscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default);

        /// <exception cref="InvalidOperationException">If the Bayeux connection is not currently connected.</exception>
        // ReSharper disable once UnusedMember.Global
        Task Unsubscribe(IEnumerable<ChannelId> channels, CancellationToken cancellationToken = default);
    }
}