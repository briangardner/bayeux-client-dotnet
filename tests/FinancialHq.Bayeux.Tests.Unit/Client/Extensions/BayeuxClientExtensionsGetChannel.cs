using System.Collections.Generic;
using System.Net.Http;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Options;
using Moq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Extensions
{
    public class BayeuxClientContextExtensionsGetChannel
    {
        [Fact]
        public void Should_Return_Channel_If_Already_Exists()
        {
            var client = new BayeuxClientContext(new HttpLongPollingTransportOptions
            {
                HttpClient = new HttpClient(new Mock<HttpMessageHandler>().Object),
                Uri = "http://localhost"
            }.Build(), new List<IExtension>());
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
                HttpClient = new HttpClient(new Mock<HttpMessageHandler>().Object),
                Uri = "http://localhost"
            }.Build(), new List<IExtension>());

            var testChannel = client.GetChannel("/dummy");
            Assert.NotNull(testChannel);
        }
    }
}
