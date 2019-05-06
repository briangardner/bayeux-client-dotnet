using System.Linq;
using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Client.Transport
{
    internal static class BayeuxTransportExtensions
    {
        internal static bool ExtendReceive(this IBayeuxTransport transport, BayeuxMessage message)
        {
            if (message.Meta)
            {
                if (transport.Extensions.Any(extension => !extension.ReceiveMeta(message)))
                {
                    return false;
                }
            }
            else
            {
                if (transport.Extensions.Any(extension => !extension.Receive(message)))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool ExtendSend(this IBayeuxTransport transport, BayeuxMessage message)
        {
            if (message.Meta)
            {
                if (transport.Extensions.Any(extension => !extension.SendMeta(message)))
                {
                    return false;
                }
            }
            else
            {
                if (transport.Extensions.Any(extension => !extension.Send(message)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
