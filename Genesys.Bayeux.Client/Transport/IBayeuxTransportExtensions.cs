﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client.Transport
{
    internal static class IBayeuxTransportExtensions
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
