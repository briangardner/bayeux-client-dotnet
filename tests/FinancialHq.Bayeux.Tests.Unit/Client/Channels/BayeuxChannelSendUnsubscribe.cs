using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Messaging;
using Moq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelSendUnsubscribe
    {
        [Fact]
        public async Task Should_Make_Unsubscription_Request()
        {
            var client = ClientMock;
            var channel = new BayeuxChannel(client.Object, new ChannelId("/test"));
            await channel.SendUnSubscribe().ConfigureAwait(false);
            client.Verify(x => x.Request(It.Is<BayeuxMessage>(msg => msg.Channel == "/meta/unsubscribe"), It.IsAny<CancellationToken>()), Times.Once);
        }

        private static Mock<IBayeuxClientContext> ClientMock => new Mock<IBayeuxClientContext>();
    }
}
