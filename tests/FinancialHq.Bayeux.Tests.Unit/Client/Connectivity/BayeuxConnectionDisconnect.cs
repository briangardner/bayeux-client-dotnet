using System;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Connectivity;
using FinancialHq.Bayeux.Client.Messaging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Connectivity
{
    public class BayeuxConnectionDisconnect
    {
        private readonly string _clientId = Guid.NewGuid().ToString();
        JObject request = null;
        [Fact]
        public async Task Subscribe_Request_Should_Include_ClientId()
        {

            var context = MockContext;
            var connection = new BayeuxConnection(_clientId, context.Object);

            await connection.Disconnect(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(_clientId.ToString(), request["clientId"]);
        }

        [Fact]
        public async Task Subscribe_Request_Should_Include_Disconnect_Channel()
        {

            var context = MockContext;
            var connection = new BayeuxConnection(_clientId, context.Object);

            await connection.Disconnect(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("/meta/disconnect", request["channel"]);
        }


        private Mock<IBayeuxClientContext> MockContext
        {
            get
            {
                var mock = new Mock<IBayeuxClientContext>();
                mock.Setup(x => x.Request(It.IsAny<BayeuxMessage>(), It.IsAny<CancellationToken>()))
                    .Callback<object, CancellationToken>(
                        (obj, token) =>
                        {
                            request = JObject.FromObject(obj);
                        })
                    .ReturnsAsync(new JObject());
                return mock;

            }
        }
    }
}
