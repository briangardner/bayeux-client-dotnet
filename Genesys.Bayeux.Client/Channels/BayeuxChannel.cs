using System;
using System.Collections.Generic;
using System.Text;

namespace Genesys.Bayeux.Client.Channels
{
    public class BayeuxChannel : AbstractChannel
    {
        public BayeuxChannel(IBayeuxClientContext clientContext) : base(clientContext)
        {
        }

        public BayeuxChannel(ChannelId id) : base(id)
        {
        }
    }
}
