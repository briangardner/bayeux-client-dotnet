using System;
using Genesys.Bayeux.Client.Enums;

namespace Genesys.Bayeux.Client
{
    public class ConnectionStateChangedArgs : EventArgs
    {
        public ConnectionState ConnectionState { get; private set; }

        public ConnectionStateChangedArgs(ConnectionState state)
        {
            ConnectionState = state;
        }

        public override string ToString() => ConnectionState.ToString();
    }
}
