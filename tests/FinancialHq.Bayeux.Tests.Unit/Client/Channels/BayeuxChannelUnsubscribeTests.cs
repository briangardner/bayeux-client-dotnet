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
    public class BayeuxChannelUnsubscribeTests
    {
        private readonly Mock<IBayeuxClientContext> _clientContextMock;
        private JObject _unsubscribeMessage;
        private readonly ChannelId _channelId = new ChannelId("/dummy");
        public BayeuxChannelUnsubscribeTests()
        {
            _clientContextMock = new Mock<IBayeuxClientContext>();
            _clientContextMock.Setup(client => client.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()))
                .Callback(
                    (object msg, CancellationToken token) => { _unsubscribeMessage = JObject.FromObject(msg); })
                .ReturnsAsync(new JObject());
        }
        [Fact]
        public void Should_Not_Attempt_Unsubscribe_When_Subscribers_Still_Present()
        {
            var subscriber1 = new Mock<IObserver<IMessage>>().Object;
            var subscriber2 = new Mock<IObserver<IMessage>>().Object;

            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            // ReSharper disable once UnusedVariable
            var unsubscriber1 = channel.Subscribe(subscriber1);
            var unsubscriber2 = channel.Subscribe(subscriber2);
            _clientContextMock.Reset();
            unsubscriber2.Dispose();

            _clientContextMock.Verify(client => client.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void Should_Unsubscribe_When_No_Subscribers_Left()
        {
            var subscriber1 = new Mock<IObserver<IMessage>>().Object;

            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            var unsubscriber1 = channel.Subscribe(subscriber1);
            _clientContextMock.Reset();
            unsubscriber1.Dispose();

            _clientContextMock.Verify(client => client.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Unsubscribe_Message_Should_Have_Correct_Channel()
        {
            var subscriber1 = new Mock<IObserver<IMessage>>().Object;
            _clientContextMock.Setup(x => x.IsConnected()).Returns(true);
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            var unsubscriber1 = channel.Subscribe(subscriber1);
            unsubscriber1.Dispose();
            Assert.Equal("/meta/unsubscribe", _unsubscribeMessage[MessageFields.ChannelField]);
        }

        [Fact]
        public void Unsubscribe_Message_Should_Have_Correct_Subscription()
        {
            var subscriber1 = new Mock<IObserver<IMessage>>().Object;
            _clientContextMock.Setup(x => x.IsConnected()).Returns(true);
            var channel = new BayeuxChannel(_clientContextMock.Object, _channelId);
            var unsubscriber1 = channel.Subscribe(subscriber1);
            unsubscriber1.Dispose();
            Assert.Equal(_channelId.ToString(), _unsubscribeMessage[MessageFields.SubscriptionField]);
        }
    }
}
