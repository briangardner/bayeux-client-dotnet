using System;
using System.Threading;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Messaging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelSubscribeTests
    {
        private readonly Mock<IBayeuxClientContext> _clientContextMock;
        private readonly ChannelId _channelId = new ChannelId("/dummy");
        public BayeuxChannelSubscribeTests()
        {
            _clientContextMock = new Mock<IBayeuxClientContext>();
            _clientContextMock.Setup(client => client.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new JObject());
        }

        [Fact]
        public void First_Subscription_Should_Trigger_Client_To_Send_Subscribe_Message()
        {
            _clientContextMock.Setup(x => x.IsConnected()).Returns(true);
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId );
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            _clientContextMock.Verify(client => client.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Second_Subscription_Should_Not_Trigger_Client_To_Send_Subscribe_Message()
        {
            _clientContextMock.Setup(x => x.IsConnected()).Returns(true);
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            _clientContextMock.Verify(client => client.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
