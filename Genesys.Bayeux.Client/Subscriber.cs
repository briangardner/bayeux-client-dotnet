using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client
{
    internal class Subscriber
    {
        readonly IBayeuxClientContext _client;
        readonly ChannelList _subscribedChannels = new ChannelList();

        public Subscriber(IBayeuxClientContext client)
        {
            _client = client;
        }

        public void AddSubscription(IEnumerable<ChannelId> channels) =>
            _subscribedChannels.Add(channels);

        public void RemoveSubscription(IEnumerable<ChannelId> channels) =>
            _subscribedChannels.Remove(channels);

        public void OnConnected()
        {
            var resubscribeChannels = _subscribedChannels.Copy();

            foreach (var channelId in resubscribeChannels)
            {
                var channel = _client.GetChannel(channelId.ToString());
                channel.SendSubscribe().GetAwaiter().GetResult();
            }
        }

        
    }
}
