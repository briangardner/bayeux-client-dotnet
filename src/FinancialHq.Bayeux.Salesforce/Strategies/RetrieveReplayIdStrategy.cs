using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Extensions.ReplayId.Strategies;
using FinancialHq.Bayeux.Salesforce.Messaging;

namespace FinancialHq.Bayeux.Salesforce.Strategies
{
    public class RetrieveReplayIdStrategy : IRetrieveReplayIdStrategy
    {
        public long GetReplayId(IMessage message)
        {
            var msgdata = message.GetMessageData<MessagePayload>();
            return msgdata?.Data?.Event?.ReplayId ?? 0;
        }
    }
}
