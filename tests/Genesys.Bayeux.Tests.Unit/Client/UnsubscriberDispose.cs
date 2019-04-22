using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Transport;
using Moq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client
{
    public class UnsubscriberDispose
    {
        [Fact]
        public void Dispose_Should_Unsubscribe_Observer_From_Observable()
        {
            var transport = MockTransport;
            var observer = MockObserver;
            var unsubscriber =
                new Unsubscriber<IBayeuxTransport, IMessage>(transport.Object, observer.Object);
            unsubscriber.Dispose();
            transport.Verify(x => x.UnsubscribeAsync(observer.Object), Times.Once);
        }

        private Mock<IBayeuxTransport> MockTransport => new Mock<IBayeuxTransport>();
        private Mock<IObserver<IMessage>> MockObserver => new Mock<IObserver<IMessage>>();
    }
}
