using System.Collections.Generic;
using FinancialHq.Bayeux.Client.Channels;

namespace FinancialHq.Bayeux.Client
{
    public interface ISubscriberCache
    {
        void AddSubscription(IEnumerable<ChannelId> channels);
        void RemoveSubscription(IEnumerable<ChannelId> channels);
        void OnConnected();
    }
}