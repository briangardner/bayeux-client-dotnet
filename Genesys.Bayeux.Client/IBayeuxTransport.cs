using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genesys.Bayeux.Client
{
    public interface IBayeuxTransport : IDisposable
    {
        IList<IObserver<JObject>> Observers { get; }
        Task Open(CancellationToken cancellationToken);
        Task<JObject> Request(IEnumerable<object> requests, CancellationToken cancellationToken);
    }
}