using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Client.Extensions
{
    public interface IExtension
    {
        bool Receive(BayeuxMessage message);
        bool ReceiveMeta(BayeuxMessage message);
        bool Send(BayeuxMessage message);
        bool SendMeta(BayeuxMessage message);
    }
}