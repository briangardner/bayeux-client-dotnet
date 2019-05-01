using System;
using System.Collections.Generic;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Extensions.ReplayId.Extensions;
using Genesys.Bayeux.Extensions.ReplayId.Logging;
using Microsoft.Extensions.Caching.Distributed;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    internal class ReplaySubscriberCache : ISubscriberCache
    {
        private readonly ILog _log = LogProvider.GetCurrentClassLogger();
        private readonly IBayeuxClientContext _client;
        private readonly IDistributedCache _distributedCache;
        private readonly IServiceProvider _serviceProvider;
        readonly ChannelList _subscribedChannels = new ChannelList();

        public ReplaySubscriberCache(IBayeuxClientContext client, IDistributedCache distributedCache, IServiceProvider serviceProvider)
        {
            _client = client;
            _distributedCache = distributedCache;
            _serviceProvider = serviceProvider;
        }
        public void AddSubscription(IEnumerable<ChannelId> channels) =>
            _subscribedChannels.Add(channels);

        public void RemoveSubscription(IEnumerable<ChannelId> channels) =>
            _subscribedChannels.Remove(channels);

        public void OnConnected()
        {
            var resubscribeChannels = _subscribedChannels.Copy();

            foreach (var channelId in resubscribeChannels)
            {
                long replayId = -1;
                var cachedReplayId = _distributedCache.GetString(channelId.ToString());
                if (long.TryParse(cachedReplayId, out var result))
                {
                    replayId = result;
                }
                _log.Debug("Resubscribing to {channelId} with ReplayId {replayId}", channelId.ToString(), replayId);
                var channel = _client.GetChannel(channelId.ToString()).WithReplayId(_serviceProvider,replayId);
                channel.SendSubscribe().GetAwaiter().GetResult();
            }
        }
    }
}
