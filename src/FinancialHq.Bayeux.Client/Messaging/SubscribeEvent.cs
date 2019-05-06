using FinancialHq.Bayeux.Client.Channels;

namespace FinancialHq.Bayeux.Client.Messaging
{
    public class SubscribeEvent
    {
        public ChannelId Channel { get; set; }
    }
}
