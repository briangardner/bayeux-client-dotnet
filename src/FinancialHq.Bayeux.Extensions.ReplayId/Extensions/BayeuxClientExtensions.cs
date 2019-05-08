using System;
using System.Threading;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Listeners;
using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Extensions
{
    public static class BayeuxClientExtensions
    {
        
        public static IDisposable Subscribe<T>(this IBayeuxClient client,IServiceProvider serviceProvider, ChannelId channelId, long replayId, CancellationToken token, bool throwIfNotConnected) where  T: class, IMessageListener
        {
            var channel = client.GetChannel(channelId).WithReplayId(serviceProvider, replayId);
            client.AddChannel(channel);
            return client.Subscribe<T>(channel, token, throwIfNotConnected);
        }
    }
}
