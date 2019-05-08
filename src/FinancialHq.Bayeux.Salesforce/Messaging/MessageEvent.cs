using System;

namespace FinancialHq.Bayeux.Salesforce.Messaging
{
    public class MessageEvent
    {
        public long ReplayId { get; set; }
        public string Type { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
