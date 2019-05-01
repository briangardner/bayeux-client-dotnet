using System;
using System.Threading;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId.Extensions
{
    public static class BayeuxClientExtensions
    {
        public static IDisposable Subscribe<T>(this IBayeuxClient client,IServiceProvider serviceProvider, ChannelId channelId, long replayId, CancellationToken token, bool throwIfNotConnected) where  T: class, IMessageListener
        {
            var channel = client.GetChannel(channelId).WithReplayId(serviceProvider, replayId);
            return client.Subscribe<T>(channel, token, throwIfNotConnected);
        }
    }
}
