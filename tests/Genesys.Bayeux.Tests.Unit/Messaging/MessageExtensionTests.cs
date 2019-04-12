using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Messaging;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Messaging
{
    public class MessageExtensionTests
    {
        [Fact]
        public void Should_Return_Advice_From_Message()
        {
            var message = 
        }

        private Mock<IMutableMessage> TestMessage => new Mock<IMutableMessage>();

        static readonly object successfulHandshakeResponse =
            new
            {
                minimumVersion = "1.0",
                clientId = "nv8g1psdzxpb9yol3z1l6zvk2p",
                supportedConnectionTypes = new[] { "long-polling", "callback-polling" },
                advice = new { interval = 0, timeout = 20000, reconnect = "retry" },
                channel = "/meta/handshake",
                version = "1.0",
                successful = true,
            };
    }
}
