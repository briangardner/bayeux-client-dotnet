using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.DI;
using Genesys.Bayeux.Client.Options;
using Genesys.Bayeux.Client.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.DI
{
    public class BayeuxClientBuilderExtensionsUseHttpLongPolling
    {
        [Fact]
        public void Can_Resolve_HttpLongPollingTransport()
        {
            var provider = GetServiceProvider();
            var transport = provider.GetService<IBayeuxTransport>();
            Assert.NotNull(transport);
        }

        [Fact]
        public void Can_Resolve_HttpLongPollingTransportOptions()
        {
            var provider = GetServiceProvider();
            var transport = provider.GetService<IOptions<HttpLongPollingTransportOptions>>();
            Assert.NotNull(transport);
        }

        private IServiceProvider GetServiceProvider()
        {
            var collection = new BayeuxClientBuilder(new ServiceCollection());
            return collection.UseHttpLongPolling(new HttpLongPollingTransportOptions()
            {
                HttpClient = MockHttpClient,
                Uri = "test"
            }).Services.BuildServiceProvider();
        }

        private HttpClient MockHttpClient => new HttpClient(MockHandler.Object);
        private Mock<HttpMessageHandler> MockHandler
        {
            get
            {
                var handler = new Mock<HttpMessageHandler>();
                handler.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                    .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK);
                    }))
                    .Callback<HttpRequestMessage, CancellationToken>((r, c) =>
                    {
                    });
                return handler;
            }
        }
    }
}
