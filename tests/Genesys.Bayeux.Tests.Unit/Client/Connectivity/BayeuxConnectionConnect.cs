﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Enums;
using Genesys.Bayeux.Client.Messaging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Connectivity
{
    public class BayeuxConnectionConnect
    {
        private readonly string _clientId = Guid.NewGuid().ToString();
        JObject request = null;
        [Fact]
        public async Task Subscribe_Request_Should_Include_ClientId()
        {
            
            var context = MockContext;
            var connection = new BayeuxConnection(_clientId, context.Object);

            await connection.Connect(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(_clientId.ToString(), request[MessageFields.ClientIdField]);
        }

        [Fact]
        public async Task Subscribe_Request_Should_Include_Connect_Channel()
        {

            var context = MockContext;
            var connection = new BayeuxConnection(_clientId, context.Object);

            await connection.Connect(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("/meta/connect", request[MessageFields.ChannelField]);
        }

        [Fact]
        public async Task Subscribe_Request_Should_Include_ConnectionType()
        {

            var context = MockContext;
            var connection = new BayeuxConnection(_clientId, context.Object);

            await connection.Connect(CancellationToken.None).ConfigureAwait(false);
            Assert.Equal("long-polling", request[MessageFields.ConnectionTypeField]);
        }

        [Fact]
        public async Task Should_Set_ConnectionState_To_Connected()
        {
            var context = MockContext;
            var connection = new BayeuxConnection(_clientId, context.Object);

            await connection.Connect(CancellationToken.None).ConfigureAwait(false);
            context.Verify(x => x.SetConnectionState(ConnectionState.Connected), Times.Once);
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
                            request = obj as JObject;
                        })
                    .ReturnsAsync(new JObject());
                return mock;

            }
        }
    }
}
