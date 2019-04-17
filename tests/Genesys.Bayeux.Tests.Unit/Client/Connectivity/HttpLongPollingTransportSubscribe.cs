using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Connectivity
{
    public class HttpLongPollingTransportSubscribe
    {
        [Fact]
        public void Should_Return_Unsubscriber()
        {
            var observer = MockObserver;
            var transport = new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(Options));
            var unsubscriber = transport.Subscribe(observer.Object);
            Assert.IsAssignableFrom<IDisposable>(unsubscriber);
        }

        private Mock<IObserver<IMessage>> MockObserver => new Mock<IObserver<IMessage>>();
        private Mock<HttpMessageHandler> MockHandler => new Mock<HttpMessageHandler>();
        private HttpLongPollingTransportOptions Options => new HttpLongPollingTransportOptions()
        {
            HttpClient = new HttpClient(MockHandler.Object),
            Uri = "http://localhost"
        };
    }
}
