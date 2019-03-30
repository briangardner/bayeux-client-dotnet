using System;
using System.Collections.Generic;
using System.Text;

namespace Genesys.Bayeux.Client
{
    interface IBayeuxChannel
    {
        void AddListener(IBayeuxChannelListener listener);
        void RemoveListener(IBayeuxChannelListener listener);
    }
}
