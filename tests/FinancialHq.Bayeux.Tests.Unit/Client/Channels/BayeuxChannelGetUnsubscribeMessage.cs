using System;
using System.Collections.Generic;
using System.Text;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using Moq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelGetUnsubscribeMessage
    {
        [Fact]
        public void Should_Have_Channel_Field_Meta_Unsubscribe()
        {
            var client = ClientMock;
            var channel = new BayeuxChannel(client.Object, new ChannelId("/test"));
            var msg = channel.GetUnsubscribeMessage();
            Assert.Equal("/meta/unsubscribe", msg.Channel);
        }

        [Fact]
        public void Should_Have_Correct_Subscription_Field()
        {
            var client = ClientMock;
            var channel = new BayeuxChannel(client.Object, new ChannelId("/test"));
            var msg = channel.GetUnsubscribeMessage();
            Assert.Equal("/test", msg.Subscription);
        }

        private static Mock<IBayeuxClientContext> ClientMock => new Mock<IBayeuxClientContext>();
    }
}
