using System;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Extensions.ReplayId.Extensions;
using Genesys.Bayeux.Extensions.ReplayId.Logging;
using Microsoft.Extensions.Caching.Distributed;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class DurableChannel : AbstractChannel
    {
        private readonly IDistributedCache _cache;
        private readonly ILog _log = LogProvider.GetCurrentClassLogger();

        public  DurableChannel(AbstractChannel channel, IDistributedCache cache, long replayId) : base(channel.ClientContext, channel.ChannelId)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            _cache = cache;

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

        public override void OnNext(BayeuxMessage message)
        {
            try
            {
                base.OnNext(message);
                _cache.SetString(ChannelId.ToString(), message.GetReplayId().ToString());
            }
            catch (Exception ex)
            {
                _log.ErrorException("Error processing message.", ex, message);
                throw;
            }

        }
    }
}
