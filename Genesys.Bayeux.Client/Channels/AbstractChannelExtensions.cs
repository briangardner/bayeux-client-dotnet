using System.Threading.Tasks;

namespace Genesys.Bayeux.Client.Channels
{
    public static class AbstractChannelExtensions
    {
        public static async Task SubscribeWithListener<TMessageListener>(this AbstractChannel channel, TMessageListener listener) where TMessageListener : IMessageListener
        {
            channel.subscriptions.Add(listener);

            if (channel.subscriptions.Count == 1)
            {
                await channel.SendSubscribe().ConfigureAwait(false);
            }
        }

        public static async Task UnsubscribeListener<TMessageListener>(this AbstractChannel channel,
            TMessageListener listener) where TMessageListener : IMessageListener
        {
            channel.subscriptions.Remove(listener);
            if (channel.subscriptions.Count == 0)
            {
                await channel.SendUnSubscribe().ConfigureAwait(false);
            }
        }

        public static async Task UnsubscribeAll(this AbstractChannel channel)
        {
            foreach(var subscriber in channel.subscriptions)
            {
                await channel.UnsubscribeListener(subscriber).ConfigureAwait(false);
            }
        }
    }
}
