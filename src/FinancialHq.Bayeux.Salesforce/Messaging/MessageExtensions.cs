using FinancialHq.Bayeux.Client.Messaging;
using Newtonsoft.Json;

namespace FinancialHq.Bayeux.Salesforce.Messaging
{
    public static class MessageExtensions
    {
        public static MessageEnvelope<T> GetMessageData<T>(this IMessage message) where T : MessagePayload
        {
            return JsonConvert.DeserializeObject<MessageEnvelope<T>>(JsonConvert.SerializeObject(message));
        }
    }
}
