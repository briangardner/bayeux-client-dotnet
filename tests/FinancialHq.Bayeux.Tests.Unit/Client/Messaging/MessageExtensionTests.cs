using System.Collections.Generic;
using FinancialHq.Bayeux.Client.Messaging;
using Newtonsoft.Json;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Messaging
{
    public class MessageExtensionTests
    {
        [Fact]
        public void Should_Return_Advice_Interval_From_Message()
        {
            var message = TestMessage;
            var advice = message.Advice;
            Assert.Equal(0L, advice["interval"]);
        }

        [Fact]
        public void Should_Return_Advice_Timeout_From_Message()
        {
            var message = TestMessage;
            var advice = message.Advice;
            Assert.Equal(20000L, advice["timeout"]);
        }

        [Fact]
        public void Should_Return_Advice_Reconnect_From_Message()
        {
            var message = TestMessage;
            var advice = message.Advice;
            Assert.Equal("retry", advice["reconnect"]);
        }

        [Fact]
        public void Should_Return_Message_Channel()
        {
            var message = TestMessage;
            var channel = message.Channel;
            Assert.Equal("/meta/handshake", channel);
        }

        [Fact]
        public void Should_Return_Subscription_Channel()
        {
            var message = TestSubscribeMessage;
            var channel = message.Subscription;
            Assert.Equal("/foo/**", channel);
        }

        [Fact]
        public void Should_Return_ClientId()
        {
            var message = TestSubscribeMessage;
            var clientId = message.ClientId;
            Assert.Equal("Un1q31d3nt1f13r", clientId);
        }

        [Fact]
        public void Should_Return_Message_Id()
        {
            var message = TestSubscribeMessage;
            var clientId = message.Id;
            Assert.Equal("123", clientId);
        }

        [Fact]
        public void Should_Return_Message_Data()
        {
            var message = TestEventMessage;
            var data = message.Data;
            Assert.NotNull(data);
        }

        [Fact]
        public void Should_Return_Message_DataAsDictionary()
        {
            var message = TestEventMessage;
            var data = message.DataAsDictionary;
            Assert.IsAssignableFrom<Dictionary<string,object>>(data);
        }

        [Fact]
        public void Should_Return_Message_Ext_Data()
        {
            var message = TestEventMessage;
            var ext = message.Ext;
            Assert.IsAssignableFrom<Dictionary<string, object>>(ext);
        }

        [Fact]
        public void Should_Return_Message_as_JSON()
        {
            var message = TestEventMessage;
            Assert.Equal(JsonConvert.SerializeObject(message), message.Json);
        }

        [Fact]
        public void Should_Return_Meta_True_For_Meta_Message()
        {
            var message = TestMessage;
            Assert.True(message.Meta);
        }

        [Fact]
        public void Should_Return_Meta_False_For_Event_Message()
        {
            var message = TestEventMessage;
            Assert.False(message.Meta);
        }

        private static BayeuxMessage TestMessage => new BayeuxMessage(JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(TestMessages.SuccessfulHandshakeResponse)));
        private static BayeuxMessage TestSubscribeMessage => new BayeuxMessage(JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(TestMessages.SubscribeRequestMessage)));
        private static BayeuxMessage TestEventMessage => new BayeuxMessage(JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(TestMessages.EventMessage)));


    }
}
