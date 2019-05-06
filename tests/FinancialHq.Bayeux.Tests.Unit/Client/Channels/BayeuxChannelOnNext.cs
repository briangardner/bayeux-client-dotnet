using System.Collections.Generic;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Listeners;
using FinancialHq.Bayeux.Client.Messaging;
using Moq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelOnNext
    {
        [Fact]
        public void Should_Send_Message_To_Listeners_When_ChannelId_Same()
        {
            var client = ClientMock;
            var listener = MockListener;
            var listener2 = MockListener;
            var message = MockMessage;

            var channel = new BayeuxChannel(client.Object, new ChannelId("/test"));
            channel.Subscribe(listener.Object);
            channel.Subscribe(listener2.Object);
            channel.OnNext(message);

            listener.Verify(x => x.OnNext(message), Times.Once);
            listener2.Verify(x => x.OnNext(message), Times.Once);
        }

        [Fact]
        public void Should_Not_Send_Message_To_Listeners_When_ChannelId_Same()
        {
            var client = ClientMock;
            var listener = MockListener;
            var listener2 = MockListener;
            var message = MockMessage;

            var channel = new BayeuxChannel(client.Object, new ChannelId("/test2"));
            channel.Subscribe(listener.Object);
            channel.Subscribe(listener2.Object);
            channel.OnNext(message);

            listener.Verify(x => x.OnNext(message), Times.Never);
            listener2.Verify(x => x.OnNext(message), Times.Never);
        }

        [Fact]
        public void Should_Call_Extensions_ReceiveMeta_For_Meta_Messages()
        {
            var client = ClientMock;
            var listener = MockListener;
            var mockExtension = new Mock<IExtension>();
            var message = MockMessage;
            message.Channel = "/meta/handshake";
            client.Setup(x => x.Extensions).Returns(new List<IExtension> {mockExtension.Object});

            var channel = new BayeuxChannel(client.Object, new ChannelId("/meta/handshake"));
            
            channel.Subscribe(listener.Object);
            channel.OnNext(message);

            mockExtension.Verify(x => x.ReceiveMeta(message), Times.Once);
        }

        [Fact]
        public void Should_Call_Extensions_Receive_For_NonMeta_Messages()
        {
            var client = ClientMock;
            var listener = MockListener;
            var mockExtension = new Mock<IExtension>();
            var message = MockMessage;
            client.Setup(x => x.Extensions).Returns(new List<IExtension> { mockExtension.Object });

            var channel = new BayeuxChannel(client.Object, new ChannelId("/test"));

            channel.Subscribe(listener.Object);
            channel.OnNext(message);

            mockExtension.Verify(x => x.Receive(message), Times.Once);
        }


        private static Mock<IBayeuxClientContext> ClientMock => new Mock<IBayeuxClientContext>();
        private static Mock<IMessageListener> MockListener => new Mock<IMessageListener>();
        private static BayeuxMessage MockMessage => new BayeuxMessage()
        {
            Channel = "/test"
        };
    }
}