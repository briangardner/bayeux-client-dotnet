using System;
using Genesys.Bayeux.Client.Channels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Extensions.ReplayId.Extensions
{
    public static class AbstractChannelExtensions
    {
        public static DurableChannel WithReplayId(this AbstractChannel channel, IServiceProvider serviceProvider, long replayId)
        {
            var cache = serviceProvider.GetService<IDistributedCache>();
            return new DurableChannel(channel,cache, replayId);
        }

    }
}
