using System;

namespace Genesys.Bayeux.Client.Exceptions
{
    public class BayeuxProtocolException : Exception
    {
        public BayeuxProtocolException(string message) : base(message)
        {
        }
    }
}