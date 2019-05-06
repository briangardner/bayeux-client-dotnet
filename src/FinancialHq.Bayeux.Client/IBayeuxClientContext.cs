using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Client.Connectivity;
using FinancialHq.Bayeux.Client.Enums;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace FinancialHq.Bayeux.Client
{
    public interface IBayeuxClientContext
    {
        event EventHandler OnNewConnection;
        void AddChannel(string channelId, AbstractChannel channel);
        ConcurrentDictionary<string, AbstractChannel> Channels { get; }
        IEnumerable<IExtension> Extensions { get; }
        Task Open(CancellationToken cancellationToken);
        Task<JObject> Request(BayeuxMessage request, CancellationToken cancellationToken);
        Task<JObject> RequestMany(IEnumerable<BayeuxMessage> requests, CancellationToken cancellationToken);
        Task SetConnectionState(ConnectionState newState);
        void SetConnection(BayeuxConnection newConnection);
        Task Disconnect(CancellationToken cancellationToken);
        bool IsConnected();
    }
}