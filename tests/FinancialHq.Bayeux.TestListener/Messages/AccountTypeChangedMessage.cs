using FinancialHq.Bayeux.Salesforce.Messaging;
using Newtonsoft.Json;

namespace FinancialHq.Bayeux.TestListener.Messages
{
    public class AccountTypeChangedMessage : MessagePayload
    {
        [JsonProperty("ID")]
        public string Id { get; set; }
        [JsonProperty("Account_Type__c")]
        public string AccountType { get; set; }
        [JsonProperty("BD_Transaction_ID__c")]
        public string TransactionId { get; set; }
        [JsonProperty("Client_Account_ID__c")]
        public string ClientAccountId { get; set; }
        [JsonProperty("Other_Account_Tyupe_Description__c")]
        public string OtherAccountTypeDescription { get; set; }
        [JsonProperty("UGMA_UTMA_State__c")]
        public string UgmaUtmaState { get; set; }


    }
}
