using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Options;
using Moq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Extensions
{
    public class BayeuxClientContextExtensionsGetChannel
    {
        [Fact]
        public void Should_Return_Channel_If_Already_Exists()
        {
            var client = new BayeuxClientContext(new HttpLongPollingTransportOptions
            {
                HttpPost = new Mock<IHttpPost>().Object,
                Uri = "http://localhost"
            }.Build());
            var channelId = new ChannelId("/dummy");
            var channel = new BayeuxChannel(new Mock<IBayeuxClientContext>().Object, channelId);
            client.Channels.TryAdd("/dummy", channel);

            var testChannel = client.GetChannel("/dummy");
            Assert.Equal(channel, testChannel);
        }

        [Fact]
        public void Should_Return_New_Channel()
        {
            var client = new BayeuxClientContext(new HttpLongPollingTransportOptions
            {
                HttpPost = new Mock<IHttpPost>().Object,
                Uri = "http://localhost"
            }.Build());

            var testChannel = client.GetChannel("/dummy");
            Assert.NotNull(testChannel);
        }
    }
}
