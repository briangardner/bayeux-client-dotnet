using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public static class AbstractChannelExtensions
    {
        public static DurableChannel WithReplayId(this AbstractChannel channel, int replayId)
        {
            return new DurableChannel(channel, replayId);
        }

    }
}
