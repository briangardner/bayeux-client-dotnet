﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using Genesys.Bayeux.Client.Transport;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Connectivity
{
    public class HttpLongPollingTransportSubscribe
    {
        [Fact]
        public void Should_Return_Unsubscriber()
        {
            var observer = MockObserver;
            var transport = new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(Options), new List<IExtension>(), Policy.NoOp());
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
