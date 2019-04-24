using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId.Extensions
{
    public static class IBayeuxClientExtensions
    {
        public static IDisposable Subscribe<T>(this IBayeuxClient client, ChannelId channelId, long replayId, CancellationToken token, bool throwIfNotConnected) where  T: class, IMessageListener
        {
            var channel = client.GetChannel(channelId).WithReplayId(replayId);
            return client.Subscribe<T>(channel, token, throwIfNotConnected);
        }
    }
}
