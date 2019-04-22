using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.DI;
using Genesys.Bayeux.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.DI
{
    public class BayeuxClientBuilderExtensionsAddBayeuxClient
    {
        [Fact]
        public void Should_Resolve_BayeuxClient()
        {
            var provider = GetServiceProvider();
            var client = provider.GetService<IBayeuxClientContext>();
            Assert.NotNull(client);
        }

        private IServiceProvider GetServiceProvider()
        {
            var collection = new ServiceCollection();
            return collection.AddBayeuxClient().UseHttpLongPolling(new HttpLongPollingTransportOptions()
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
