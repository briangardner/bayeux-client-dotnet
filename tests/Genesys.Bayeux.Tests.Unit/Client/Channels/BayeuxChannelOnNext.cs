using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using Moq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelOnNext
    {
        [Fact]
        public void Should_Send_Message_To_Listeners()
        {
            var client = ClientMock;
            var listener = MockListener;
            var listener2 = MockListener;
            var message = MockMessage;

            var channel = new BayeuxChannel(client.Object, new ChannelId("/test"));
            channel.Subscribe(listener.Object);
            channel.Subscribe(listener2.Object);
            channel.OnNext(message.Object);

            listener.Verify(x => x.OnNext(message.Object), Times.Once);
            listener2.Verify(x => x.OnNext(message.Object), Times.Once);
        }


        private Mock<IBayeuxClientContext> ClientMock => new Mock<IBayeuxClientContext>();
        private Mock<IMessageListener> MockListener => new Mock<IMessageListener>();
        private Mock<IMessage> MockMessage => new Mock<IMessage>();
    }
}