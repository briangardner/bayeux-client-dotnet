using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class ReplayExtension : IExtension
    {
        private const string ExtensionName = "replay";

        public bool Receive(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool Send(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta(AbstractChannel channel, BayeuxMessage message)
        {
            if (!ChannelFields.META_SUBSCRIBE.Equals(message.Channel) &&
                !ChannelFields.META_UNSUBSCRIBE.Equals(message.Channel))
            {
                return true;
            }
            var value = new Dictionary<string, object>
            {
                { message.Subscription, message.GetReplayId() }
            };

            message.GetExt(true)[ExtensionName] = value;
            return true;
        }
    }
}
