using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.DI;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Options;
using FinancialHq.Bayeux.Extensions.Ack.Extensions;
using FinancialHq.Bayeux.Extensions.Error.Extensions;
using FinancialHq.Bayeux.Extensions.ReplayId.Extensions;
using FinancialHq.Bayeux.Extensions.TimesyncClient.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.DI
{
    public class BayeuxClientBuilderExtensionsAddExensions
    {

        [Fact]
        public void Should_Resolve_Extensions()
        {
            var provider = GetServiceProvider();
            var extensions = provider.GetService<IEnumerable<IExtension>>();
            Assert.NotEmpty(extensions);
        }

        [Fact]
        public void Should_Resolve_DistributedCache()
        {
            var provider = GetServiceProvider();
            var cache = provider.GetService<IDistributedCache>();
            Assert.IsAssignableFrom<MemoryDistributedCache>(cache);
        }

        private IServiceProvider GetServiceProvider()
        {
            var collection = new ServiceCollection();
            var bayeuxClientBuilder = collection.AddBayeuxClient();
            bayeuxClientBuilder.UseHttpLongPolling(new HttpLongPollingTransportOptions()
            {
                HttpClient = MockHttpClient,
                Uri = "test"
            });
            bayeuxClientBuilder.AddTimesyncClient();
            bayeuxClientBuilder.AddAckExtension();
            bayeuxClientBuilder.AddErrorExtension();
            bayeuxClientBuilder.AddReplayIdExtension().WithDistributedMemoryCache();
            return collection.BuildServiceProvider();

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
