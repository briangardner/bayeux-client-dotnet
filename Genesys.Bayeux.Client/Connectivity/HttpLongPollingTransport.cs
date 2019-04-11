﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Connectivity
{
    public interface IHttpPost
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, string jsonContent, CancellationToken cancellationToken);
    }

    public class HttpClientHttpPost : IHttpPost
    {
        readonly HttpClient httpClient;

        public HttpClientHttpPost(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, string jsonContent, CancellationToken cancellationToken)
        {
            return httpClient.PostAsync(
                requestUri,
                new StringContent(jsonContent, Encoding.UTF8, "application/json"),
                cancellationToken);
        }
    }

    class HttpLongPollingTransport : IBayeuxTransport
    {
        private static readonly ILog Log = BayeuxClient.log;

        readonly IHttpPost _httpPost;
        readonly string _url;
        readonly Func<IEnumerable<JObject>,Task> eventPublisher;

        public HttpLongPollingTransport(IOptions<HttpLongPollingTransportOptions> options)
        {
            _httpPost = options.Value.HttpClient != null ? new HttpClientHttpPost(options.Value.HttpClient) : options.Value.HttpPost;
            _url = options.Value.Uri;
            Observers = new List<IObserver<JObject>>();
        }

        public void Dispose() { }

        public IList<IObserver<JObject>> Observers { get; }

        public Task Open(CancellationToken cancellationToken)
            => Task.FromResult(0);

        public async Task<JObject> Request(IEnumerable<object> requests, CancellationToken cancellationToken)
        {
            var messageStr = JsonConvert.SerializeObject(requests);
            Log.Debug(() => $"Posting: {messageStr}");

            var httpResponse = await _httpPost.PostAsync(_url, messageStr, cancellationToken).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var responseStr = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                Log.Debug(() => $"Received: {responseStr}");
            }

            httpResponse.EnsureSuccessStatusCode();

            var responseToken = JToken.ReadFrom(new JsonTextReader(new StreamReader(await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))));
            Log.Debug(() => $"Received: {responseToken.ToString(Formatting.None)}");
            
            IEnumerable<JToken> tokens = responseToken is JArray ?
                (IEnumerable<JToken>)responseToken :
                new[] { responseToken };

            // https://docs.cometd.org/current/reference/#_delivery
            // Event messages MAY be sent to the client in the same HTTP response 
            // as any other message other than a /meta/handshake response.
            JObject responseObj = null;
            var events = new List<JObject>();

            foreach (var token in tokens)
            {
                JObject message = (JObject)token;
                var channel = (string)message[MessageFields.CHANNEL_FIELD];

                if (channel == null)
                    throw new BayeuxProtocolException("No 'channel' field in message.");



                if (channel.StartsWith("/meta/"))
                    responseObj = message;
                else
                    events.Add(message);
            }

            var observable = events.ToObservable();
            foreach (var observer in Observers)
            {
                observable.Subscribe(observer);
            }

            return responseObj;
        }

    }
}
