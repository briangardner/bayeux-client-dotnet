using System;
using System.Collections.Generic;
using System.Text;
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
            return newChannel;
        }

        public static AbstractChannel NewChannel(this BayeuxClient client, ChannelId channelId)
        {
            return new BayeuxChannel(client, channelId);
        } 

        public static void Subscribe(this BayeuxClient client, AbstractChannel channel)
        {

        }
    }
}
