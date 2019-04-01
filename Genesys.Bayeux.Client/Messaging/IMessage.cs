using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client.Messaging
{
    public interface IMessage : IDictionary<string, object>
    {
        IDictionary<string, object> Advice { get; }
        string Channel { get; }
        string Subscription { get; }
        ChannelId ChannelId { get; }
        string ClientId { get; }
        object Data { get; }
        IDictionary<string, object> DataAsDictionary { get; }
        IDictionary<string, object> Ext { get; }
        string Id { get; }
        string Json { get; }
        bool Meta { get; }
        bool Successful { get; }
    }
}