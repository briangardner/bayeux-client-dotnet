using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialHq.Bayeux.Salesforce.Messaging
{
    public class MessageEnvelope<TPayload> where TPayload : MessagePayload
    {
        public MessageData<TPayload> Data { get; set; }
        public string Channel { get; set; }
    }
}
