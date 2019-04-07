using System;
using System.Collections.Generic;
using System.Text;

namespace Genesys.Bayeux.Client.Channels
{
    public class BayeuxChannel : AbstractChannel
    {
        public BayeuxChannel(IBayeuxClientContext clientContext, ChannelId id) : base(clientContext, id)
        {
        }
    }
}
