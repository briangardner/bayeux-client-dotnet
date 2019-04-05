using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Genesys.Bayeux.Client.Channels;
using static Genesys.Bayeux.Client.BayeuxClient;

namespace Genesys.Bayeux.Client
{
    class Subscriber
    {
        readonly BayeuxClient client;
        readonly ChannelList subscribedChannels = new ChannelList();

        public Subscriber(BayeuxClient client)
        {
            this.client = client;
        }

        public void AddSubscription(IEnumerable<ChannelId> channels) =>
            subscribedChannels.Add(channels);

        public void RemoveSubscription(IEnumerable<ChannelId> channels) =>
            subscribedChannels.Remove(channels);

        public void OnConnected()
        {
            var resubscribeChannels = subscribedChannels.Copy();

            if (resubscribeChannels.Count != 0)
                _ = client.RequestSubscribe(resubscribeChannels, CancellationToken.None, throwIfNotConnected: false);
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
