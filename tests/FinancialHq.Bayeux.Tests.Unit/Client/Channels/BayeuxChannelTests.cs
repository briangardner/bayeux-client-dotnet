using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using Moq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Channels
{
    public class BayeuxChannelTests
    {
        [Fact]
        public void Should_Set_ChannelId()
        {
            var channelId = new ChannelId("/test");
            var channel = new BayeuxChannel(new Mock<IBayeuxClientContext>().Object, channelId);
            Assert.Equal(channelId, channel.ChannelId);
        }
    }
}
