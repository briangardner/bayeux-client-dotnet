using System;

namespace FinancialHq.Bayeux.Client.Exceptions
{
    public class BayeuxProtocolException : Exception
    {
        public BayeuxProtocolException(string message) : base(message)
        {
        }
    }
}