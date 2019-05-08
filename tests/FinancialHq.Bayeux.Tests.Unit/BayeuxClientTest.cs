using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Exceptions;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Listeners;
using FinancialHq.Bayeux.Client.Options;
using FinancialHq.Bayeux.Client.Transport;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Polly;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit
{
    public class BayeuxClientTest
    {
        
        [Fact]
        public async Task Server_response_without_channel()
        {
            var mock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mock.Object);

            mock.Protected().As<IHttpMessageHandlerProtected>()
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(new { }));

            using (var bayeuxClient = CreateHttpBayeuxClient(httpClient))
            {
                await Assert.ThrowsAsync<BayeuxProtocolException>(async () => await bayeuxClient.Start().ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Server_advices_no_reconnect_on_handshake()
        {
            var mock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mock.Object);

            mock.Protected().As<IHttpMessageHandlerProtected>()
                .SetupSequence(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(
                    new
                    {
                        minimumVersion = "1.0",
                        clientId = "nv8g1psdzxpb9yol3z1l6zvk2p",
                        supportedConnectionTypes = new[] { "long-polling", "callback-polling" },
                        advice = new
                        {
                            interval = 0,
                            timeout = 20000,
                            reconnect = "none" // !!!
                        },
                        channel = "/meta/handshake",
                        version = "1.0",
                        successful = true,
                    }))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(
                    new
                    {
                        channel = "/meta/disconnect",
                        successful = true,
                    }));

            using (var bayeuxClient = CreateHttpBayeuxClient(httpClient))
            {
                await bayeuxClient.Start().ConfigureAwait(false);
            }

            mock.Protected().As<IHttpMessageHandlerProtected>()
                .Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                times: Times.Exactly(2));
        }

        [Fact]
        public async Task Reconnections()
        {
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected().As<IHttpMessageHandlerProtected>()
                .SetupSequence(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulHandshakeResponse))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulConnectResponse))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.RehandshakeConnectResponse))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulHandshakeResponse))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ReturnsIndefinitely(() =>
                  Task.Delay(TimeSpan.FromSeconds(5))
                      .ContinueWith(t => TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulHandshakeResponse)))
                ;
            var transport = new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(new HttpLongPollingTransportOptions()
            {
                HttpClient = new HttpClient(mock.Object),
                Uri = Url
            }), new List<IExtension>(),
                Policy.NoOpAsync());
            var bayeuxClient = new BayeuxClient(
                new BayeuxClientContext(transport, new List<IExtension>()),
                new List<IMessageListener>(),
                new Mock<ISubscriberCache>().Object,
                new ReconnectDelayOptions(new List<TimeSpan>(){ TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }));

            using (bayeuxClient)
            {
                await bayeuxClient.Start().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Reconnection_when_started_in_background()
        {
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected().As<IHttpMessageHandlerProtected>()
                .SetupSequence(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("mock raising exception"))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulHandshakeResponse))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulConnectResponse))
                .ReturnsIndefinitely(() =>
                  Task.Delay(TimeSpan.FromSeconds(5))
                      .ContinueWith(t => TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulConnectResponse)))
                ;

            var bayeuxClient = new BayeuxClient(
                new BayeuxClientContext(new HttpLongPollingTransportOptions() { HttpClient = new HttpClient(mock.Object), Uri = Url }.Build(), new List<IExtension>()),
                new List<IMessageListener>(),
                new Mock<ISubscriberCache>().Object,
                new ReconnectDelayOptions(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }));

            using (bayeuxClient)
            {
                bayeuxClient.StartInBackground();
                await Task.Delay(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            }
        }

        // TODO: test ConnectionStateChangedEvents

        const string Url = "http://testing.net/";


        interface IHttpMessageHandlerProtected
        {
            Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        }

        
        private Mock<HttpMessageHandler> MockHttpMessageHandler => new Mock<HttpMessageHandler>(MockBehavior.Strict);
        private HttpClient MockClient => new HttpClient(MockHttpMessageHandler.Object);

        BayeuxClient CreateHttpBayeuxClient(HttpClient httpClient = null, string uri = Url)
        {
            var transport = new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(new HttpLongPollingTransportOptions()
            {
                HttpClient = httpClient,
                Uri = uri
            }),
                new List<IExtension>(),
                Policy.NoOpAsync());
            return new BayeuxClient(
                new BayeuxClientContext(transport, new List<IExtension>()),
                new List<IMessageListener>(),
                new Mock<ISubscriberCache>().Object);
        }
    }
}
