using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client
{
    public static class BayeuxClientExtensions
    {
        public static AbstractChannel GetChannel(this BayeuxClient client, string channelId)
        {
            client.Channels.TryGetValue(channelId, out var channel);
            return channel;
        }

        
    }
}
