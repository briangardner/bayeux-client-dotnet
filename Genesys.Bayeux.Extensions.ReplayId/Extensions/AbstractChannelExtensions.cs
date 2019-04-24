using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId.Extensions
{
    public static class AbstractChannelExtensions
    {
        public static DurableChannel WithReplayId(this AbstractChannel channel, long replayId)
        {
            return new DurableChannel(channel, replayId);
        }

    }
}
