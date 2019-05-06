using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Exceptions;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Logging;
using FinancialHq.Bayeux.Client.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinancialHq.Bayeux.Client.Transport
{
    internal class WebSocketTransport : IBayeuxTransport
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly Func<WebSocket> _webSocketFactory;
        private readonly Uri _uri;
        private readonly TimeSpan _responseTimeout;
        private readonly Func<IEnumerable<JObject>, Task> _eventPublisher;

        private WebSocket _webSocket;
        private Task _receiverLoopTask;
        private CancellationTokenSource _receiverLoopCancel;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<JObject>> _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<JObject>>();
        private long _nextMessageId;

        public WebSocketTransport(Func<WebSocket> webSocketFactory, Uri uri, TimeSpan responseTimeout, Func<IEnumerable<JObject>, Task> eventPublisher, IEnumerable<IExtension> extensions, IList<IObserver<JObject>> observers)
        {
            _webSocketFactory = webSocketFactory;
            _uri = uri;
            _responseTimeout = responseTimeout;
            _eventPublisher = eventPublisher;
            Extensions = extensions;
            Observers = observers;
        }

        public void Dispose()
        {
            ClearPendingRequests();

            _receiverLoopCancel?.Cancel();

            if (_webSocket != null)
            {
                try
                {
                    _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                catch (Exception)
                {
                    // Nothing else to try.
                }

                _webSocket.Dispose();
            }
        }

        public IList<IObserver<JObject>> Observers { get; }

        public IEnumerable<IExtension> Extensions { get; }

        public async Task Open(CancellationToken cancellationToken)
        {
            if (_receiverLoopCancel != null)
            {
                _receiverLoopCancel.Cancel();
                await _receiverLoopTask.ConfigureAwait(false);
            }

            _webSocket?.Dispose();

            _webSocket = _webSocketFactory();

            try
            {
                await _webSocket.ConnectAsync(_uri, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new BayeuxTransportException("WebSocket connect failed.", e, transportClosed: true);
            }

            _receiverLoopCancel = new CancellationTokenSource();
            _receiverLoopTask = StartReceiverLoop(_receiverLoopCancel.Token);
        }

        public async Task StartReceiverLoop(CancellationToken cancelToken)
        {
            Exception fault;

            try
            {
                while (!cancelToken.IsCancellationRequested)
                    await HandleReceivedMessage(await ReceiveMessage(cancelToken).ConfigureAwait(false)).ConfigureAwait(false);

                fault = null;
            }
            catch (OperationCanceledException)
            {
                fault = null;
            }
            catch (WebSocketException e)
            {
                // It is not possible to infer whether the webSocket is closed from webSocket.State,
                // and not clear how to infer it from WebSocketException. So we always assume that it is closed.
                fault = new BayeuxTransportException("WebSocket receive message failed. Connection assumed closed.", e, transportClosed: true);
            }
            catch (Exception e)
            {
                Log.ErrorException("Unexpected exception thrown in WebSocket receiving loop", e);
                fault = new BayeuxTransportException("Unexpected exception. Connection assumed closed.", e, transportClosed: true);
            }

            ClearPendingRequests(fault);
        }

        void ClearPendingRequests(Exception fault = null)
        {
            if (fault == null)
            {
                foreach (var r in _pendingRequests)
                    r.Value.SetCanceled();
            }
            else
            {
                foreach (var r in _pendingRequests)
                    r.Value.SetException(fault);
            }

            _pendingRequests.Clear();
        }

        private async Task<Stream> ReceiveMessage(CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            var stream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await _webSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                stream.Write(buffer.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private async Task HandleReceivedMessage(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var received = JToken.ReadFrom(new JsonTextReader(reader));
                Log.Debug(() => $"Received: {received.ToString(Formatting.None)}");

                var responses = received is JObject ?
                    new[] { (JObject)received } :
                    ((JArray)received).Children().Cast<JObject>();

                var events = new List<JObject>();
                foreach (var response in responses)
                {
                    var messageId = (string)response["id"];
                    if (messageId == null)
                    {
                        events.Add(response);
                    }
                    else
                    {
                        var found = _pendingRequests.TryRemove(messageId, out var requestTask);

                        if (found)
                            requestTask.SetResult(response);
                        else
                            Log.Error($"Request not found for received response with id '{messageId}'");
                    }
                }

                if (events.Count > 0)
                    await _eventPublisher(events).ConfigureAwait(false);
            }
        }

        public async Task<JObject> Request(IEnumerable<BayeuxMessage> requests, CancellationToken cancellationToken)
        {
            var responseTasks = new List<TaskCompletionSource<JObject>>();
            var requestsJArray = JArray.FromObject(requests);
            var messageIds = new List<string>();
            foreach (var request in requestsJArray)
            {
                var messageId = Interlocked.Increment(ref _nextMessageId).ToString();
                request["id"] = messageId;
                messageIds.Add(messageId);

                var responseReceived = new TaskCompletionSource<JObject>();
                _pendingRequests.TryAdd(messageId, responseReceived);
                responseTasks.Add(responseReceived);
            }
            
            var messageStr = JsonConvert.SerializeObject(requestsJArray);
            Log.Debug(() => $"Posting: {messageStr}");
            await SendAsync(messageStr, cancellationToken).ConfigureAwait(false);

            var timeoutTask = Task.Delay(_responseTimeout, cancellationToken);
            Task completedTask = await Task.WhenAny(
                Task.WhenAll(responseTasks.Select(t => t.Task)),
                timeoutTask).ConfigureAwait(false);

            foreach (var id in messageIds)
                _pendingRequests.TryRemove(id, out var _);

            if (completedTask == timeoutTask)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException();
            }
            else
            {
                return await responseTasks.First().Task.ConfigureAwait(false);
            }
        }

        public async Task SendAsync(string message, CancellationToken cancellationToken)
        {
            var bytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            try
            {
                await _webSocket.SendAsync(
                    bytes,
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new BayeuxTransportException("WebSocket send failed.", e, transportClosed: _webSocket.State != WebSocketState.Open);
            }
        }

        public IDisposable Subscribe(IObserver<BayeuxMessage> observer)
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeAsync(IObserver<BayeuxMessage> observer)
        {
            throw new NotImplementedException();
        }
    }
}