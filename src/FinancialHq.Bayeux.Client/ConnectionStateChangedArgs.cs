using System;
using FinancialHq.Bayeux.Client.Enums;

namespace FinancialHq.Bayeux.Client
{
    public class ConnectionStateChangedArgs : EventArgs
    {
        public ConnectionState ConnectionState { get; }

        public ConnectionStateChangedArgs(ConnectionState state)
        {
            ConnectionState = state;
        }

        public override string ToString() => ConnectionState.ToString();
    }
}
