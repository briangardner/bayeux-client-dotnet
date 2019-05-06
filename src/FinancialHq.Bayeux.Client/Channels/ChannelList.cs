using System.Collections;
using System.Collections.Generic;

namespace FinancialHq.Bayeux.Client.Channels
{
    public class ChannelList
    {
        readonly List<ChannelId> _items;
        private readonly object _syncRoot;

        public ChannelList()
        {
            _items = new List<ChannelId>();
            _syncRoot = ((ICollection)_items).SyncRoot;
        }

        public void Add(IEnumerable<ChannelId> channels)
        {
            lock (_syncRoot)
            {
                _items.AddRange(channels);
            }
        }

        public void Remove(IEnumerable<ChannelId> channels)
        {
            lock (_syncRoot)
            {
                foreach (var channel in channels)
                    _items.Remove(channel);
            }
        }

        public List<ChannelId> Copy()
        {
            lock (_syncRoot)
            {
                return new List<ChannelId>(_items);
            }
        }
    }
}
