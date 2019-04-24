using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.DI;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Options;
using Genesys.Bayeux.Extensions.Ack;
using Genesys.Bayeux.Extensions.Ack.Extensions;
using Genesys.Bayeux.Extensions.Error;
using Genesys.Bayeux.Extensions.Error.Extensions;
using Genesys.Bayeux.Extensions.ReplayId;
using Genesys.Bayeux.Extensions.ReplayId.Extensions;
using Genesys.Bayeux.Extensions.TimesyncClient;
using Genesys.Bayeux.Extensions.TimesyncClient.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.DI
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
