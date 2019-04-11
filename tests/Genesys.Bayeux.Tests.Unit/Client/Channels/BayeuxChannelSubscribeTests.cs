using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelSubscribeTests
    {
        private readonly Mock<IBayeuxClientContext> _clientContextMock;
        private JObject _subscribeMessage;
        private ChannelId _channelId = new ChannelId("/dummy");
        public BayeuxChannelSubscribeTests()
        {
            _clientContextMock = new Mock<IBayeuxClientContext>();
            _clientContextMock.Setup(client => client.Request(It.IsAny<JObject>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new JObject())
                .Callback(
                    (JObject msg, CancellationToken token) => { _subscribeMessage = msg; });
        }

        [Fact]
        public void First_Subscription_Should_Trigger_Client_To_Send_Subscribe_Message()
        {
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId );
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            _clientContextMock.Verify(client => client.Request(It.IsAny<JObject>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Second_Subscription_Should_Not_Trigger_Client_To_Send_Subscribe_Message()
        {
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            _clientContextMock.Verify(client => client.Request(It.IsAny<JObject>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Subscribe_Message_Should_Have_Correct_Channel()
        {
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            Assert.Equal("/meta/subscribe", _subscribeMessage[MessageFields.CHANNEL_FIELD]);
        }

        [Fact]
        public void Subscribe_Message_Should_Have_Correct_Subscription()
        {
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            channel.Subscribe(new Mock<IObserver<IMessage>>().Object);
            Assert.Equal(_channelId.ToString(), _subscribeMessage[MessageFields.SUBSCRIPTION_FIELD]);
        }
    }
}
