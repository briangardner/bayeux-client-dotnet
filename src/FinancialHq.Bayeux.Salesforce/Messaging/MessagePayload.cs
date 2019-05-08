using System;

namespace FinancialHq.Bayeux.Salesforce.Messaging
{
    public class MessagePayload
    {
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedById { get; set; }
    }
}
