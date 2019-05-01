using System.Collections;
using System.Collections.Generic;

namespace Genesys.Bayeux.Client.Channels
{
    public class ChannelList
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
