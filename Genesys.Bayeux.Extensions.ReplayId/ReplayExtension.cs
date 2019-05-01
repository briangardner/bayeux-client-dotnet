using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Extensions.ReplayId.Extensions;
using Genesys.Bayeux.Extensions.ReplayId.Logging;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class ReplayExtension : IExtension
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private const string ExtensionName = "replay";

        public bool Receive(BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(BayeuxMessage message)
        {
            return true;
        }

        public bool Send(BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta(BayeuxMessage message)
        {
            Log.Debug("Replay ID Extension - Send Meta start");
            if (ChannelFields.MetaSubscribe.Equals(message.Channel) ||
                ChannelFields.MetaUnsubscribe.Equals(message.Channel))
            {
                var value = new Dictionary<string, object>
                {
                    { message.Subscription, message.GetReplayId() }
                };
                message.GetExt(true)[ExtensionName] = value;
            }
            Log.Debug("Replay ID Extension - Send Meta end");
            return true;
        }
    }
}
