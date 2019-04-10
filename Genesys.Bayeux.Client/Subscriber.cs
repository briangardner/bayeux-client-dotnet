using System.Collections;
using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Extensions;

namespace Genesys.Bayeux.Client
{
    class Subscriber
    {
        readonly BayeuxClient _client;
        readonly ChannelList _subscribedChannels = new ChannelList();

        public Subscriber(BayeuxClient client)
        {
            this._client = client;
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

        class ChannelList
        {
            readonly List<ChannelId> items;
            readonly object syncRoot;

            public ChannelList()
            {
                items = new List<ChannelId>();
                syncRoot = ((ICollection)items).SyncRoot;
            }

            public void Add(IEnumerable<ChannelId> channels)
            {
                lock (syncRoot)
                {
                    items.AddRange(channels);
                }
            }

            public void Remove(IEnumerable<ChannelId> channels)
            {
                lock (syncRoot)
                {
                    foreach (var channel in channels)
                        items.Remove(channel);
                }
            }

            public List<ChannelId> Copy()
            {
                lock (syncRoot)
                {
                    return new List<ChannelId>(items);
                }
            }
        }
    }
}
