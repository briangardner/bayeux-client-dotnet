using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public static class BayeuxClientExtensions
    {
        public static AbstractChannel GetChannel(this BayeuxClient client, string channelId, long replayId)
        {
            client.Channels.TryGetValue(channelId, out var channel);
            if (channel != null)
            {
                return channel;
            }
            var id = new ChannelId(channelId);
            var newChannel = new DurableChannel(new BayeuxChannel(client, id), replayId );

            client.Channels.Add(channelId, newChannel);
            return newChannel;
        }
    }
}
