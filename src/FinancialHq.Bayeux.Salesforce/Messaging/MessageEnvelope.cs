namespace FinancialHq.Bayeux.Salesforce.Messaging
{
    public class MessageEnvelope<TPayload> where TPayload : MessagePayload
    {
        public MessageData<TPayload> Data { get; set; }
        public string Channel { get; set; }
    }
}
