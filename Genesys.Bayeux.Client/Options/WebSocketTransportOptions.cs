using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Transport;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Options
{
    public class WebSocketTransportOptions
    {
        public Func<WebSocket> WebSocketFactory { get; set; }
        public Uri Uri { get; set; }

        /// <summary>
        /// Timeout for responses to be received. Must be greater than the expected Connect timeout. (Default is 65 seconds).
        /// </summary>
        public TimeSpan? ResponseTimeout { get; set; }
        
        internal WebSocketTransport Build(Func<IEnumerable<JObject>,Task> eventPublisher)
        {
            return new WebSocketTransport(
                WebSocketFactory ?? (() => SystemClientWebSocket.CreateClientWebSocket()),
                Uri ?? throw new Exception("Please set Uri."),
                ResponseTimeout ?? TimeSpan.FromSeconds(65),
                eventPublisher);
        }        
    }
}
