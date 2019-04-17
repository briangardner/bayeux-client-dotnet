using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client.Extensions
{
    static class BayeuxClientExtensions
    {
        public static AbstractChannel GetChannel(this BayeuxClient client, string channelId)
        {
            client.Channels.TryGetValue(channelId, out var channel);
            if (channel != null)
            {
                return channel;
            }
            var newChannelId = new ChannelId(channelId);
            var newChannel = client.NewChannel(newChannelId);
            client.Channels.TryAdd(channelId, newChannel);
            return newChannel;
        }

        private static AbstractChannel NewChannel(this IBayeuxClientContext client, ChannelId channelId)
        {
            return new BayeuxChannel(client, channelId);
        }

    }
}
