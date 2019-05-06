using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Connectivity;
using FinancialHq.Bayeux.Client.Enums;
using FinancialHq.Bayeux.Client.Messaging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Connectivity
{
    public class ConnectLoopStart
    {
        [Fact]
        public async Task Should_Set_Context_Connection_Status_To_Connecting()
        {
            var contextMock = MockClientContext;
            var connectLoop = new ConnectLoop("none", new List<TimeSpan>(), contextMock.Object);
            contextMock.SetupSequence(x => x.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JObject.FromObject(TestMessages.SuccessfulHandshakeResponse))
                .ReturnsAsync(JObject.FromObject(TestMessages.SuccessfulConnectResponse))
                .ReturnsIndefinitely(() =>
                    Task.Delay(TimeSpan.FromSeconds(5))
                        .ContinueWith(t => JObject.FromObject(TestMessages.SuccessfulConnectResponse)));
            
            await connectLoop.Start(CancellationToken.None).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            contextMock.Verify(x => x.SetConnectionState(ConnectionState.Connecting), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Should_Set_Context_Connection_Status_To_Connected()
        {
            var contextMock = MockClientContext;
            var connectLoop = new ConnectLoop("none", new List<TimeSpan>(), contextMock.Object);
            contextMock.SetupSequence(x => x.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(JObject.FromObject(TestMessages.SuccessfulHandshakeResponse))
                .ReturnsAsync(JObject.FromObject(TestMessages.SuccessfulConnectResponse))
                .ReturnsIndefinitely(() =>
                    Task.Delay(TimeSpan.FromSeconds(5))
                        .ContinueWith(t => JObject.FromObject(TestMessages.SuccessfulConnectResponse)));

            await connectLoop.Start(CancellationToken.None).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            contextMock.Verify(x => x.SetConnectionState(ConnectionState.Connected), Times.AtLeastOnce);
        }

        private Mock<IBayeuxClientContext> MockClientContext => new Mock<IBayeuxClientContext>();
    }
}
