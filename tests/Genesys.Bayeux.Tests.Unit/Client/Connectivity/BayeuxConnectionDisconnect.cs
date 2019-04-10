using System;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Connectivity;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Connectivity
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
                mock.Setup(x => x.Request(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                    .Callback<object, CancellationToken>(
                        (obj, token) =>
                        {
                            request = obj as JObject;
                        })
                    .ReturnsAsync(new JObject());
                return mock;

            }
        }
    }
}
