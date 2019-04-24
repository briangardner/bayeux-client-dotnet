using System;
using Genesys.Bayeux.Client.Connectivity;

namespace Genesys.Bayeux.Client.Exceptions
{
    public class BayeuxRequestException : Exception
    {
        public string BayeuxError { get; }
        public BayeuxAdvice Advice { get; set; }

        public BayeuxRequestException(string error, BayeuxAdvice advice) : base(error)
        {
            BayeuxError = error;
            Advice = advice;
        }
    }
}