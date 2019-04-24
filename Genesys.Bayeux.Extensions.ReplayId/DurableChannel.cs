using System;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class DurableChannel : AbstractChannel
    {
        public DurableChannel(AbstractChannel channel, long replayId) : base(channel.ClientContext, channel.ChannelId)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            this.Observers = channel.Observers;
            ReplayId = replayId;
        }

        public long ReplayId { get; set; }
        public override BayeuxMessage GetSubscribeMessage()
        {
            var msg = base.GetSubscribeMessage();
            msg.Add("replayId", ReplayId);
            return msg;
        }
    }
}
