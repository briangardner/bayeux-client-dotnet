using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Extensions.ReplayId.Messaging
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
