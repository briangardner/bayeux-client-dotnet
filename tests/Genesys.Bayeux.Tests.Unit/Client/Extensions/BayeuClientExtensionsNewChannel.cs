using System.Collections.Generic;
using System.Net.Http;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Options;
using Moq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Extensions
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
