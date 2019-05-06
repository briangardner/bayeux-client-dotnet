using System;
using FinancialHq.Bayeux.Client.Listeners;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.TestListener.Logging;

namespace FinancialHQ.Bayeux.TestListener.Listener
{
    class TestListener : IMessageListener
    {
        private readonly ILog _log = LogProvider.GetCurrentClassLogger();
        public void OnCompleted()
        {
            
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnNext(BayeuxMessage value)
        {
            _log.Info("Message Received: {$message}", value);
        }
    }
}
