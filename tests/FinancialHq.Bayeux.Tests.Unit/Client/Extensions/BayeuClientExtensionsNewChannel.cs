using System.Collections.Generic;
using System.Net.Http;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Options;
using Moq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Extensions
{
    public class BayeuClientExtensionsNewChannel
    {
        [Fact]
        public void Should_Return_New_Channel_With_Correct_ChannelId()
        {
            var client = new BayeuxClientContext(new HttpLongPollingTransportOptions
            {
                HttpClient = new HttpClient(new Mock<HttpMessageHandler>().Object),
                Uri = "http://localhost"
            }.Build(), new List<IExtension>());

            var testChannel = client.GetChannel("/dummy");
            Assert.Equal("/dummy", testChannel.ChannelId.ToString());
        }
    }
}
