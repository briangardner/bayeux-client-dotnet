using Genesys.Bayeux.Client.Enums;
using Genesys.Bayeux.Client.Exceptions;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Genesys.Bayeux.Client.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.Bayeux.Client.Connectivity
{
    internal class ConnectLoop : IDisposable
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private const int ReadTimeOut = 120 * 1000;

        private readonly string _connectionType;
        private readonly ReconnectDelays _reconnectDelays;
        private readonly IBayeuxClientContext _context;

        private readonly CancellationTokenSource _pollCancel = new CancellationTokenSource();

        private BayeuxConnection _currentConnection;
        private BayeuxAdvice _lastAdvice = new BayeuxAdvice();
        private bool _transportFailed;
        private bool _transportClosed;
        private bool _startInBackground;


        public ConnectLoop(
            string connectionType,
            IEnumerable<TimeSpan> reconnectDelays,
            IBayeuxClientContext context)
        {
            _connectionType = connectionType;
            _reconnectDelays = new ReconnectDelays(reconnectDelays);
            _context = context;
        }

        private readonly BooleanLatch _startLatch = new BooleanLatch();

        public async Task Start(CancellationToken cancellationToken)
        {
            if (_startLatch.AlreadyRun())
                throw new Exception("Already started.");

            await _context.Open(cancellationToken).ConfigureAwait(false);
            await Handshake(cancellationToken).ConfigureAwait(false);

            // A way to test the re-handshake with a real server is to put some delay here, between the first handshake response,
            // and the first try to connect. That will cause an "Invalid client id" response, with an advice of reconnect=handshake.
            // This can also be tested with a fake server in unit tests.

            LoopPolling();
        }

        public void StartInBackground()
        {
            if (_startLatch.AlreadyRun())
                throw new Exception("Already started.");

            _startInBackground = true;

            LoopPolling();
        }

        private async void LoopPolling()
        {
            try
            {
                while (!_pollCancel.IsCancellationRequested)
                    await Poll().ConfigureAwait(false);

                await _context.SetConnectionState(ConnectionState.Disconnected).ConfigureAwait(false);
                Log.Warn("Long-polling stopped.");
            }
            catch (OperationCanceledException ex)
            {
                await _context.SetConnectionState(ConnectionState.Disconnected).ConfigureAwait(false);
                Log.WarnException("Long-polling stopped.", ex);
            }
            catch (Exception e)
            {
                Log.ErrorException("Long-polling stopped on unexpected exception.", e);
                await _context.SetConnectionState(ConnectionState.DisconnectedOnError).ConfigureAwait(false);
                throw; // unobserved exception
            }
        }

        public void Dispose()
        {
            _pollCancel.Cancel();
        }

        private async Task Poll()
        {
            _reconnectDelays.ResetIfLastSucceeded();

            try
            {
                if (_startInBackground)
                {
                    _startInBackground = false;
                    await _context.Open(_pollCancel.Token).ConfigureAwait(false);
                    await Handshake(_pollCancel.Token).ConfigureAwait(false);
                }
                else if (_transportFailed)
                {
                    _transportFailed = false;

                    if (_transportClosed)
                    {
                        _transportClosed = false;
                        Log.Info($"Re-opening transport due to previously failed request.");
                        await _context.Disconnect(_pollCancel.Token).ConfigureAwait(false);
                        await _context.Open(_pollCancel.Token).ConfigureAwait(false);
                    }

                    Log.Info($"Re-handshaking due to previously failed request.");
                    await Handshake(_pollCancel.Token).ConfigureAwait(false);
                }
                else switch (_lastAdvice.reconnect)
                    {
                        case MessageFields.ReconnectNoneValue:
                            Log.Info("Long-polling stopped on server request.");
                            Dispose();
                            break;

                        // https://docs.cometd.org/current/reference/#_the_code_long_polling_code_response_messages
                        // interval: the number of milliseconds the client SHOULD wait before issuing another long poll request

                        // usual sample advice:
                        // {"interval":0,"timeout":20000,"reconnect":"retry"}
                        // another sample advice, when too much time without polling:
                        // [{"advice":{"interval":0,"reconnect":"handshake"},"channel":"/meta/connect","error":"402::Unknown client","successful":false}]

                        case MessageFields.ReconnectHandshakeValue:
                            Log.Info($"Re-handshaking after {_lastAdvice.interval} ms on server request.");
                            await Task.Delay(_lastAdvice.interval).ConfigureAwait(false);
                            await Handshake(_pollCancel.Token).ConfigureAwait(false);
                            break;

                        case MessageFields.ReconnectRetryValue:
                        default:
                            if (_lastAdvice.interval > 0)
                                Log.Info($"Re-connecting after {_lastAdvice.interval} ms on server request.");

                            await Task.Delay(_lastAdvice.interval).ConfigureAwait(false);
                            await Connect(_pollCancel.Token).ConfigureAwait(false);
                            break;
                    }
            }
            catch (HttpRequestException e)
            {
                await _context.SetConnectionState(ConnectionState.Connecting).ConfigureAwait(false);
                _transportFailed = true;

                var reconnectDelay = _reconnectDelays.GetNext();
                Log.WarnException($"HTTP request failed. Rehandshaking after {reconnectDelay}", e);
                await Task.Delay(reconnectDelay).ConfigureAwait(false);
            }
            catch (BayeuxTransportException e)
            {
                _transportFailed = true;
                _transportClosed = e.TransportClosed;

                await _context.SetConnectionState(ConnectionState.Connecting).ConfigureAwait(false);

                var reconnectDelay = _reconnectDelays.GetNext();
                Log.WarnException($"Request transport failed. Retrying after {reconnectDelay}", e);
                await Task.Delay(reconnectDelay).ConfigureAwait(false);
            }
            catch (BayeuxRequestException e)
            {
                _transportFailed = true;
                _currentConnection = null;
                var reconnectDelay = _reconnectDelays.GetNext();
                Log.WarnException($"Request failed: {e.Message}. Retrying after {reconnectDelay}", e);
                await _context.SetConnectionState(ConnectionState.Connecting).ConfigureAwait(false);
                await Task.Delay(reconnectDelay).ConfigureAwait(false);
            }
        }

        private async Task Handshake(CancellationToken cancellationToken)
        {
            Log.Debug("Begin Handshake");
            await _context.SetConnectionState(ConnectionState.Connecting).ConfigureAwait(false);
            var handshakeRequest = new BayeuxMessage
            {
                {"channel",  "/meta/handshake"},
                {"version", "1.0" },
                {"supportedConnectionTypes", new[] {_connectionType}},
                {"timeout",  _lastAdvice.timeout ?? ReadTimeOut}
            };
            
            var response = await _context.Request(handshakeRequest, 
                cancellationToken).ConfigureAwait(false);

            _currentConnection = new BayeuxConnection((string)response[MessageFields.ClientIdField], _context);
            await _context.SetConnectionState(ConnectionState.Connected).ConfigureAwait(false);
            _context.SetConnection(_currentConnection);
            Log.Debug("Handshake Successful");
            ObtainAdvice(response);
            Log.Debug("Advice {@advice}", _lastAdvice);
        }

        private async Task Connect(CancellationToken cancellationToken)
        {
            var connectResponse = await _currentConnection.Connect(cancellationToken).ConfigureAwait(false);
            ObtainAdvice(connectResponse);
        }


        private void ObtainAdvice(JObject response)
        {
            var adviceToken = response[MessageFields.AdviceField];
            if (adviceToken != null)
                _lastAdvice = adviceToken.ToObject<BayeuxAdvice>();
        }

    }

}
