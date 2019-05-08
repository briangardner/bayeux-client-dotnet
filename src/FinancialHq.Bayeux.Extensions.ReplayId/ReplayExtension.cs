using System.Collections.Generic;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Extensions.ReplayId.Extensions;
using FinancialHq.Bayeux.Extensions.ReplayId.Logging;
using FinancialHq.Bayeux.Extensions.ReplayId.Strategies;

namespace FinancialHq.Bayeux.Extensions.ReplayId
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
                    { message.Subscription, message[MessageFields.ReplayIdField] }
                };
                message.GetExt(true)[ExtensionName] = value;
            }
            Log.Debug("Replay ID Extension - Send Meta end");
            return true;
        }
    }
}
