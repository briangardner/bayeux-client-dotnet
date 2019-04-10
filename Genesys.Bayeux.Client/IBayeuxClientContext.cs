using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Connectivity;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client
{
    public interface IBayeuxClientContext
    {
        Task Open(CancellationToken cancellationToken);
        Task<JObject> Request(object request, CancellationToken cancellationToken);
        Task<JObject> RequestMany(IEnumerable<object> requests, CancellationToken cancellationToken);
        Task SetConnectionState(BayeuxClient.ConnectionState newState);
        void SetConnection(BayeuxConnection newConnection);
    }
}