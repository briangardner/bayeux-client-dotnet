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

        AbstractChannel GetChannel(ChannelId channelId);
        void AddChannel(AbstractChannel channel);

        // ReSharper disable once UnusedMember.Global
        IDisposable Subscribe<T>(ChannelId channelId, CancellationToken cancellationToken, bool throwIfNotConnected) where T : class, IMessageListener;
        IDisposable Subscribe<T>(AbstractChannel channel, CancellationToken cancellationToken, bool throwIfNotConnected) where T : class, IMessageListener;

    }
}