using System;
using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Client.Listeners
{
    public interface IMessageListener : IObserver<BayeuxMessage>
    {

    }
}