using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Transport
{
    public interface IBayeuxTransport : IDisposable, IObservable<BayeuxMessage>, IUnsubscribe<BayeuxMessage>
    {
        IEnumerable<IExtension> Extensions { get; }
        Task Open(CancellationToken cancellationToken);
        Task<JObject> Request(IEnumerable<BayeuxMessage> requests, CancellationToken cancellationToken);
    }
}