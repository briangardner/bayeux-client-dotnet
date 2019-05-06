using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Messaging
{
    public class ReplaySubscribeEvent
    {
        private readonly SubscribeEvent _message;

        public ReplaySubscribeEvent(SubscribeEvent message)
        {
            _message = message;
        }

        public int ReplayId { get; set; } = -1;
    }
}
