using System;
using Genesys.Bayeux.Client.Channels;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class DurableChannel : ChannelExtension
    {
        public DurableChannel(AbstractChannel channel, int replayId) : base(channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            ReplayId = replayId;
        }

        public int ReplayId { get; set; }

        protected override JObject GetSubscribeMessage()
        {
            var msg = base.GetSubscribeMessage();
            msg.Add("replayid", ReplayId);
            return msg;
        }
    }
}
