using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Exceptions;
using Genesys.Bayeux.Client.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit
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
            var bayeuxClient = new BayeuxClient(
                new HttpLongPollingTransportOptions{ HttpClient = new HttpClient(mock.Object), Uri = Url }.Build(), 
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
                new HttpLongPollingTransportOptions() { HttpClient = new HttpClient(mock.Object), Uri = Url }.Build(),
                new ReconnectDelayOptions(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }));

            using (bayeuxClient)
            {
                bayeuxClient.StartInBackground();
                await Task.Delay(TimeSpan.FromSeconds(20)).ConfigureAwait(false);
            }
        }

        // TODO: test ConnectionStateChangedEvents

        [Fact]
        public async Task Automatic_subscription()
        {
            var mock = new Mock<HttpMessageHandler>();
            var mockProtected = mock.Protected().As<IHttpMessageHandlerProtected>();

            int subscriptionCount = 0;

            mockProtected
                .Setup(h => h.SendAsync(TestMessages.MatchSubscriptionRequest(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                    Task.Run(() => subscriptionCount++)
                        .ContinueWith(t => TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulSubscriptionResponse)));

            mockProtected
                .Setup(h => h.SendAsync(TestMessages.MatchHandshakeRequest(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulHandshakeResponse));

            mockProtected
                .Setup(h => h.SendAsync(TestMessages.MatchConnectRequest(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                    Task.Delay(TimeSpan.FromSeconds(5))
                        .ContinueWith(t => TestMessages.BuildBayeuxResponse(TestMessages.SuccessfulConnectResponse)));

            var bayeuxClient = new BayeuxClient(
                new HttpLongPollingTransportOptions() { HttpClient = new HttpClient(mock.Object), Uri = Url }.Build(),
                new ReconnectDelayOptions(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }));

            using (bayeuxClient)
            {
                bayeuxClient.AddSubscriptions(new ChannelId("/mychannel"));
                await bayeuxClient.Start().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }

            Assert.Equal(1, subscriptionCount);
        }

        



        const string Url = "http://testing.net/";



        interface IHttpMessageHandlerProtected
        {
            Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        }

        [Fact]
        public async Task Subscribe_throws_exception_when_not_connected()
        {
            var httpPoster = new Mock<IHttpPost>();
            var bayeuxClient = new BayeuxClient(new HttpLongPollingTransportOptions() { HttpPost = httpPoster.Object, Uri = "none" }.Build());
            await Assert.ThrowsAsync<InvalidOperationException>( async () => await bayeuxClient.Subscribe(new ChannelId("dummy")).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task Unsubscribe_throws_exception_when_not_connected()
        {
            var httpPoster = new Mock<IHttpPost>();
            var bayeuxClient = new BayeuxClient(new HttpLongPollingTransportOptions() { HttpPost = httpPoster.Object, Uri = "none" }.Build());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await bayeuxClient.Unsubscribe(new ChannelId("dummy")).ConfigureAwait(false)).ConfigureAwait(false) ;
        }

        [Fact]
        public void AddSubscriptions_succeeds_when_not_connected()
        {
            var httpPoster = new Mock<IHttpPost>();
            var bayeuxClient = new BayeuxClient(new HttpLongPollingTransportOptions() { HttpPost = httpPoster.Object, Uri = "none" }.Build());
            bayeuxClient.AddSubscriptions(new ChannelId("/dummy"));
        }

        [Fact]
        public void RemoveSubscriptions_succeeds_when_not_connected()
        {
            var httpPoster = new Mock<IHttpPost>();
            var bayeuxClient = new BayeuxClient(new HttpLongPollingTransportOptions() { HttpPost = httpPoster.Object, Uri = "none" }.Build());
            bayeuxClient.RemoveSubscriptions(new ChannelId("/dummy"));
        }

        BayeuxClient CreateHttpBayeuxClient(HttpClient httpClient = null, string uri = Url)
        {
            return new BayeuxClient(new HttpLongPollingTransportOptions()
            {
                HttpClient = httpClient,
                Uri = uri,
            }.Build());
        }
    }
}
