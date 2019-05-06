using System;

namespace FinancialHq.Bayeux.Client.Exceptions
{
    [Serializable]
    internal class BayeuxTransportException : Exception
    {
        public bool TransportClosed { get; }

        public BayeuxTransportException(string message, Exception innerException, bool transportClosed)
            : base(innerException.Message, innerException)
        {
            TransportClosed = transportClosed;
        }
    }
}