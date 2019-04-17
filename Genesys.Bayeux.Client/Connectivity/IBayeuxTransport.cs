using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Connectivity
{
    public interface IBayeuxTransport : IDisposable, IObservable<IMessage>, IUnsubscribe<IMessage>
    {
        Task Open(CancellationToken cancellationToken);
        Task<JObject> Request(IEnumerable<object> requests, CancellationToken cancellationToken);
    }
}