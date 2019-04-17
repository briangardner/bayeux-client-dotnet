using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Exceptions;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Connectivity
{
    internal class HttpLongPollingTransport : IBayeuxTransport
    {
        private static readonly ILog Log = BayeuxClient.Log;

        readonly IHttpPost _httpPost;
        readonly string _url;
        private readonly IList<IObserver<IMessage>> _observers;

        public HttpLongPollingTransport(IOptions<HttpLongPollingTransportOptions> options)
        {
            _httpPost = options.Value.HttpClient != null ? new HttpClientHttpPost(options.Value.HttpClient) : options.Value.HttpPost;
            _url = options.Value.Uri;
            _observers = new List<IObserver<IMessage>>();
        }

        public void Dispose() { }

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
            var events = new List<IMessage>();

            foreach (var token in tokens)
            {
                JObject message = (JObject)token;
                var channel = (string)message[MessageFields.ChannelField];

                if (channel == null)
                    throw new BayeuxProtocolException("No 'channel' field in message.");



                if (channel.StartsWith("/meta/"))
                    responseObj = message;
                else
                    events.Add(new BayeuxMessage(message.ToObject<Dictionary<string,object>>()));
            }

            var observable = events.ToObservable();
            foreach (var observer in _observers)
            {
                observable.Subscribe(observer);
            }

            return responseObj;
        }


        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<HttpLongPollingTransport,IMessage>(this, observer);
        }

        public async Task UnsubscribeAsync(IObserver<IMessage> observer)
        {
            if (observer != null && _observers.Contains(observer))
            {
                _observers.Remove(observer);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
