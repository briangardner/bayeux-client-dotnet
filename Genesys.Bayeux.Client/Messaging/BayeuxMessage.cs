using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client.Messaging
{
    public class BayeuxMessage : Dictionary<string, object>, IMessage
    {
        public IDictionary<string, object> Advice { get; }
        public string Channel { get; }
        public string Subscription { get; }
        public ChannelId ChannelId { get; }
        public string ClientId { get; }
        public object Data { get; }
        public IDictionary<string, object> DataAsDictionary { get; }
        public IDictionary<string, object> Ext { get; }
        public string Id { get; }
        public string Json { get; }
        public bool Meta { get; }
        public bool Successful { get; }
    }
}
