using System;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Extensions.Error
{
    public class ErrorExtension : IExtension
    {
        public bool Receive(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(AbstractChannel channel, BayeuxMessage message)
        {
            if (message.Successful)
            {
                return true;
            }

            if (message.ContainsKey("exception"))
            {
                throw new Exception($"Exception in ReceiveMeta: {message["exception"]}");
            }

            if (message.ContainsKey("error"))
            {
                throw new Exception($"Error in ReceiveMeta: {message["error"]}");
            }

            return true;
        }

        public bool Send(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta(AbstractChannel channel, BayeuxMessage message)
        {
            return true;
        }
    }
}
