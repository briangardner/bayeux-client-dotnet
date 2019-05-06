using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Exceptions;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Messaging;
using FinancialHq.Bayeux.Client.Options;
using FinancialHq.Bayeux.Client.Transport;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Polly;
using Xunit;

namespace FinancialHq.Bayeux.Tests.Unit.Client.Connectivity
{
    public class HttpLongPollingTransportRequest
    {
        private const string FakeUrl = "http://localhost/testing123";

        //[Fact]
        //public async Task Should_Post_To_Url()
        //{
        //    var post = MockHttpPost;
        //        post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        //        {
        //            Content = new StringContent(MetaResponse.ToString())
        //        });

        //    var transport = new HttpLongPollingTransport(GetOptions(post.Object), new List<IExtension>());
        //    await transport.Request(new List<BayeuxMessage>(), CancellationToken.None).ConfigureAwait(false);
        //    post.Verify(x => x.PostAsync(FakeUrl, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        //}

        //[Fact]
        //public async Task Should_Return_Meta_Response()
        //{
        //    var post = MockHttpPost;
        //    post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        //        {
        //            Content = new StringContent(MetaResponse.ToString())
        //        });

        //    var transport = new HttpLongPollingTransport(GetOptions(post.Object), new List<IExtension>());
        //    var result = await transport.Request(new List<BayeuxMessage>(), CancellationToken.None).ConfigureAwait(false);
        //    Assert.Equal(MetaResponse.ToString(), result.ToString());
        //}

        //[Fact]
        //public async Task Should_Notify_Observers_of_Events()
        //{
        //    var post = MockHttpPost;
        //    var observer = MockObserver;
        //    var content = new JArray {MetaResponse, EventResponse, EventResponse};
        //    post.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        //        {
        //            Content = new StringContent(content.ToString())
        //        });
        //    var transport = new HttpLongPollingTransport(GetOptions(post.Object), new List<IExtension>());
        //    transport.Subscribe(observer.Object);
        //    var result = await transport.Request(new List<BayeuxMessage>(), CancellationToken.None).ConfigureAwait(false);
        //    observer.Verify(x => x.OnNext(It.IsAny<IMessage>()), Times.Exactly(2));
        //}

        //[Fact]
        //public async Task Should_Notify_Observers_of_Events_On_Second_Call()
        //{
        //    var post = MockHttpPost;
        //    var observer = MockObserver;
        //    var content = new JArray { MetaResponse, EventResponse, EventResponse };
        //    post.SetupSequence(x => x.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        //        {
        //            Content = new StringContent(content.ToString())
        //        })
        //        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
        //        {
        //            Content = new StringContent(content.ToString())
        //        });
        //    var transport = new HttpLongPollingTransport(GetOptions(post.Object), new List<IExtension>());
        //    transport.Subscribe(observer.Object);
        //    await transport.Request(new List<BayeuxMessage>(), CancellationToken.None).ConfigureAwait(false);
        //    await transport.Request(new List<BayeuxMessage>(), CancellationToken.None).ConfigureAwait(false);
        //    observer.Verify(x => x.OnNext(It.IsAny<IMessage>()), Times.Exactly(4));
        //}

        [Fact]
        public async Task Should_Throw_BayeuxProtocolException_When_Message_Missing_Channel()
        {
            var content = new JArray { MetaResponse, ErrorEventResponse };
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content.ToString())
                })
                .Verifiable();
            var client = new HttpClient(handler.Object);
            var transport = new HttpLongPollingTransport(GetOptions(client), new List<IExtension>(), Policy.NoOpAsync());
            await Assert.ThrowsAsync<BayeuxProtocolException>(async () =>
                 await transport.Request(new List<BayeuxMessage>(), CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private Mock<HttpMessageHandler> MockHttpMessageHandler => new Mock<HttpMessageHandler>(MockBehavior.Strict);

        private Mock<IObserver<IMessage>> MockObserver => new Mock<IObserver<IMessage>>();

        private IOptions<HttpLongPollingTransportOptions> GetOptions(HttpClient httpClient)
        {
            return new OptionsWrapper<HttpLongPollingTransportOptions>(new HttpLongPollingTransportOptions()
            {
                HttpClient = httpClient,
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


    }
}
