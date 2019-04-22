using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Enums;
using Genesys.Bayeux.Client.Extensions;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client
{
    public interface IBayeuxClientContext
    {
        event EventHandler OnNewConnection;
        ConcurrentDictionary<string, AbstractChannel> Channels { get; }
        IEnumerable<IExtension> Extensions { get; }
        Task Open(CancellationToken cancellationToken);
        Task<JObject> Request(JObject request, CancellationToken cancellationToken);
        Task<JObject> RequestMany(IEnumerable<JObject> requests, CancellationToken cancellationToken);
        Task SetConnectionState(ConnectionState newState);
        void SetConnection(BayeuxConnection newConnection);
        Task Disconnect(CancellationToken cancellationToken);
        bool IsConnected();
    }
}