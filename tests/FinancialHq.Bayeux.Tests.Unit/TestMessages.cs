using System.Net;
using System.Net.Http;
using Moq;
using Newtonsoft.Json;

namespace FinancialHq.Bayeux.Tests.Unit
{
    public class TestMessages
    {
        public static HttpResponseMessage BuildBayeuxResponse(params object[] messages) =>
            new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(messages)),
            };

        public static HttpRequestMessage MatchSubscriptionRequest() => MatchRequestContains("/meta/subscribe");
        public static HttpRequestMessage MatchHandshakeRequest() => MatchRequestContains("/meta/handshake");
        public static HttpRequestMessage MatchConnectRequest() => MatchRequestContains("/meta/connect");

        public static HttpRequestMessage MatchRequestContains(string s) =>
            Match.Create((HttpRequestMessage request) =>
                request.Content.ReadAsStringAsync().Result.Contains(s));

        public static readonly object SuccessfulHandshakeResponse =
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

        public static readonly object SuccessfulConnectResponse =
            new
            {
                channel = "/meta/connect",
                successful = true,
            };

        public static readonly object SuccessfulSubscriptionResponse =
            new
            {
                channel = "/meta/subscribe",
                successful = true,
            };

        public static readonly object RehandshakeConnectResponse =
            new
            {
                channel = "/meta/connect",
                successful = false,
                error = "402::Unknown client",
                advice = new { interval = 0, reconnect = "handshake" },
            };

        public static readonly object SubscribeRequestMessage =
            new
            {
                id = "123",
                channel = "/meta/subscribe",
                clientId = "Un1q31d3nt1f13r",
                subscription = "/foo/**"
            };
        public static readonly object EventMessage =
            new
            {
                channel= "/some/channel",
                clientId= "Un1q31d3nt1f13r",
                data = new {
                    fruit= "Apple",
                    size= "Large",
                    color= "Red"
                },
                id= "123",
                ext = new { ext = new {ack = true }}
            };
    }
}
