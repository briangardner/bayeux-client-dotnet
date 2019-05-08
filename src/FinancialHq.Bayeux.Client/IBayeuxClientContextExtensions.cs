using FinancialHq.Bayeux.Client.Channels;

namespace FinancialHq.Bayeux.Client
{
    public static class BayeuxClientContextExtensions
    {
        public static AbstractChannel GetChannel(this IBayeuxClientContext context, string channelId)
        {
            context.Channels.TryGetValue(channelId, out var channel);
            if (channel != null)
            {
                return channel;
            }
            var newChannelId = new ChannelId(channelId);
            var newChannel = context.NewChannel(newChannelId);
            return newChannel;
        }

        private static AbstractChannel NewChannel(this IBayeuxClientContext client, ChannelId channelId)
        {
            return new BayeuxChannel(client, channelId);
        }

    }
}
