using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Transport;
using Newtonsoft.Json.Linq;

namespace FinancialHq.Bayeux.Client.Options
{
    public class WebSocketTransportOptions
    {
        public Func<WebSocket> WebSocketFactory { get; set; }
        public Uri Uri { get; set; }

        /// <summary>
        /// Timeout for responses to be received. Must be greater than the expected Connect timeout. (Default is 65 seconds).
        /// </summary>
        public TimeSpan? ResponseTimeout { get; set; }
        
        // ReSharper disable once UnusedMember.Global
        internal WebSocketTransport Build(Func<IEnumerable<JObject>,Task> eventPublisher, IEnumerable<IExtension> extensions = null)
        {
            return new WebSocketTransport(
                WebSocketFactory ?? (SystemClientWebSocket.CreateClientWebSocket),
                Uri ?? throw new Exception("Please set Uri."),
                ResponseTimeout ?? TimeSpan.FromSeconds(65),
                eventPublisher, extensions, new List<IObserver<JObject>>());
        }        
    }
}
