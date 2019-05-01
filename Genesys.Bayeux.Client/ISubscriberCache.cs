using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client
{
    public interface ISubscriberCache
    {
        void AddSubscription(IEnumerable<ChannelId> channels);
        void RemoveSubscription(IEnumerable<ChannelId> channels);
        void OnConnected();
    }
}