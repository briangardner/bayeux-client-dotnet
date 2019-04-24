using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Exceptions;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Transport
{
    internal class HttpLongPollingTransport : IBayeuxTransport
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        readonly IHttpPost _httpPost;
        readonly string _url;
        private readonly IList<IObserver<BayeuxMessage>> _observers;
        public IEnumerable<IExtension> Extensions { get; }

        public HttpLongPollingTransport(IOptions<HttpLongPollingTransportOptions> options, IEnumerable<IExtension> extensions)
        {
            _httpPost = options.Value.HttpClient != null ? new HttpClientHttpPost(options.Value.HttpClient) : options.Value.HttpPost;
            _url = options.Value.Uri;
            _observers = new List<IObserver<BayeuxMessage>>();
            Extensions = extensions;
        }

        public void Dispose() { }

        public Task Open(CancellationToken cancellationToken)
            => Task.FromResult(0);

        public async Task<JObject> Request(IEnumerable<BayeuxMessage> requests, CancellationToken cancellationToken)
        {
            List<BayeuxMessage> requestsToSend = new List<BayeuxMessage>();
            foreach(var msg in requests)
            {
                if (this.ExtendSend(msg))
                {
                    requestsToSend.Add(msg);
                }
            }
            
            var messageStr = JsonConvert.SerializeObject(requestsToSend);
            Log.Debug(() => $"Posting: {messageStr}");

            var httpResponse = await _httpPost.PostAsync(_url, messageStr, cancellationToken).ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var responseStr = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                Log.Debug(() => $"Received: {responseStr}");
            }

            httpResponse.EnsureSuccessStatusCode();

            var responseToken = JToken.ReadFrom(new JsonTextReader(new StreamReader(await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))));
            Log.Debug(() => $"Received Response Token: {responseToken.ToString(Formatting.None)}");
            
            IEnumerable<JToken> tokens = responseToken is JArray ?
                (IEnumerable<JToken>)responseToken :
                new[] { responseToken };
            // https://docs.cometd.org/current/reference/#_delivery
            // Event messages MAY be sent to the client in the same HTTP response 
            // as any other message other than a /meta/handshake response.
            JObject responseObj = null;
            var events = new List<BayeuxMessage>();

            foreach (var token in tokens)
            {
                BayeuxMessage message = token.ToObject<BayeuxMessage>();
                
                var channel = message.ChannelId;

                if (channel == null)
                    throw new BayeuxProtocolException("No 'channel' field in message.");

                if (channel.IsMeta())
                    responseObj = JObject.FromObject(message);
                else
                {
                    if (this.ExtendReceive(message))
                    {
                        events.Add(message);
                    }
                }
                    
            }

            var observable = events.ToObservable();
            foreach (var observer in _observers)
            {
                observable.Subscribe(observer);
            }

            return responseObj;
        }


        public IDisposable Subscribe(IObserver<BayeuxMessage> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<HttpLongPollingTransport,BayeuxMessage>(this, observer);
        }

        public async Task UnsubscribeAsync(IObserver<BayeuxMessage> observer)
        {
            if (observer != null && _observers.Contains(observer))
            {
                _observers.Remove(observer);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
