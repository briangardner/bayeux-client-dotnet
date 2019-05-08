using System;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Extensions.ReplayId.Strategies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Extensions
{
    public static class AbstractChannelExtensions
    {
        public static DurableChannel WithReplayId(this AbstractChannel channel, IServiceProvider serviceProvider, long replayId)
        {
            var cache = serviceProvider.GetService<IDistributedCache>();           
            var replayIdStrategy = serviceProvider.GetService<IRetrieveReplayIdStrategy>();           
            var newChannel = new DurableChannel(channel, cache, replayId, replayIdStrategy);
            return newChannel;
        }

    }
}
