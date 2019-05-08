using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FinancialHq.Bayeux.Salesforce.Messaging
{
    public class MessageData<TPayload> where TPayload : MessagePayload
    {
        public string Schema { get; set; }
        public TPayload Payload { get; set; }

        [JsonProperty("sobject")]
        private TPayload SObject
        {
            set => Payload = value;
        }
        public MessageEvent Event { get; set; }
    }
}
