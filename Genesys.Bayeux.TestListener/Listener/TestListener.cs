using System;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.TestListener.Logging;

namespace Genesys.Bayeux.TestListener.Listener
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

        public void OnNext(IMessage value)
        {
            _log.Info("Message Received: {$message}", value);
        }
    }
}
