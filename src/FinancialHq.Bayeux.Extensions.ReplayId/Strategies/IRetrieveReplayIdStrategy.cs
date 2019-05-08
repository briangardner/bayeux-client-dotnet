using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Strategies
{
    public interface IRetrieveReplayIdStrategy
    {
        long GetReplayId(IMessage message);
    }
}