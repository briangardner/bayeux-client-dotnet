using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client.Messaging
{
    public class SubscribeEvent
    {
        public ChannelId Channel { get; set; }
    }
}
