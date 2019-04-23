using System;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class DurableChannel : ChannelExtension
    {
        public DurableChannel(AbstractChannel channel, long replayId) : base(channel.ClientContext, channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            ReplayId = replayId;
        }

        public long ReplayId { get; set; }

        public override BayeuxMessage GetSubscribeMessage()
        {
            var msg = base.GetSubscribeMessage();
            msg.Add("replayid", ReplayId);
            return msg;
        }
    }
}
