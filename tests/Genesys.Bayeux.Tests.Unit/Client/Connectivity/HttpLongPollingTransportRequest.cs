using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Exceptions;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using Genesys.Bayeux.Client.Transport;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Genesys.Bayeux.Tests.Unit.Client.Connectivity
{
    public class HttpLongPollingTransportRequest
    {
        private const string FakeUrl = "http://localhost/testing123";

        [Fact]
        public async Task Should_Post_To_Url()
        {
            var post = MockHttpPost;
                post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(MetaResponse.ToString())
                });

            var transport = new HttpLongPollingTransport(GetOptions(post.Object));
            await transport.Request(new List<object>(), CancellationToken.None).ConfigureAwait(false);
            post.Verify(x => x.PostAsync(FakeUrl, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Return_Meta_Response()
        {
            var post = MockHttpPost;
            post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(MetaResponse.ToString())
                });

            var transport = new HttpLongPollingTransport(GetOptions(post.Object));
            var result = await transport.Request(new List<object>(), CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(MetaResponse.ToString(), result.ToString());
        }

        [Fact]
        public async Task Should_Notify_Observers_of_Events()
        {
            var post = MockHttpPost;
            var observer = MockObserver;
            var content = new JArray {MetaResponse, EventResponse, EventResponse};
            post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content.ToString())
                });
            var transport = new HttpLongPollingTransport(GetOptions(post.Object));
            transport.Subscribe(observer.Object);
            var result = await transport.Request(new List<object>(), CancellationToken.None).ConfigureAwait(false);
            observer.Verify(x => x.OnNext(It.IsAny<IMessage>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Should_Notify_Observers_of_Events_On_Second_Call()
        {
            var post = MockHttpPost;
            var observer = MockObserver;
            var content = new JArray { MetaResponse, EventResponse, EventResponse };
            post.SetupSequence(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content.ToString())
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content.ToString())
                });
            var transport = new HttpLongPollingTransport(GetOptions(post.Object));
            transport.Subscribe(observer.Object);
            await transport.Request(new List<object>(), CancellationToken.None).ConfigureAwait(false);
            await transport.Request(new List<object>(), CancellationToken.None).ConfigureAwait(false);
            observer.Verify(x => x.OnNext(It.IsAny<IMessage>()), Times.Exactly(4));
        }

        [Fact]
        public async Task Should_Throw_BayeuxProtocolException_When_Message_Missing_Channel()
        {
            var post = MockHttpPost;
            var observer = MockObserver;
            var content = new JArray { MetaResponse, ErrorEventResponse };
            post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content.ToString())
                });
            var transport = new HttpLongPollingTransport(GetOptions(post.Object));
            await Assert.ThrowsAsync<BayeuxProtocolException>(async () =>
                 await transport.Request(new List<object>(), CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private Mock<IObserver<IMessage>> MockObserver => new Mock<IObserver<IMessage>>();

        private IOptions<HttpLongPollingTransportOptions> GetOptions(IHttpPost httpPost)
        {
            return new OptionsWrapper<HttpLongPollingTransportOptions>(new HttpLongPollingTransportOptions()
            {
                HttpPost = httpPost,
                Uri = FakeUrl
            });
        }

        private JToken MetaResponse => new JObject()
        {
            {MessageFields.ChannelField, "/meta/testing" }, {MessageFields.ClientIdField, "123"}
        };

        private JToken EventResponse => new JObject()
        {
            {MessageFields.ChannelField, "/some/testing" }, {MessageFields.ClientIdField, "123"}, {"messageId", Guid.NewGuid().ToString()}
        };

        private JToken ErrorEventResponse => new JObject()
        {
            {MessageFields.ChannelField, "123"}, {"messageId", Guid.NewGuid().ToString()}
        };


        private Mock<IHttpPost> MockHttpPost
        {
            get
            {
                var mock = new Mock<IHttpPost>();
                return mock;
            }
        }
    }
}
