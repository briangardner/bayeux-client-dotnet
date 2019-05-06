using System;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Extensions.Error.Logging;

namespace FinancialHq.Bayeux.Extensions.Error
{
    public class ErrorExtension : IExtension
    {

        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        public bool Receive(BayeuxMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(BayeuxMessage message)
        {
            Log.Debug("Error Extension - Receive Meta start");
            if (message.Successful)
            {
                return true;
            }

            if (message.ContainsKey("exception"))
            {
                throw new Exception($"Exception in ReceiveMeta: {message["exception"]}");
            }

            if (message.ContainsKey("error"))
            {
                throw new Exception($"Error in ReceiveMeta: {message["error"]}");
            }
            Log.Debug("Error Extension - Receive Meta end");
            return true;
        }

        public bool Send(BayeuxMessage message)
        {
            return true;
        }

        public bool SendMeta( BayeuxMessage message)
        {
            return true;
        }
    }
}
