using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client
{
    public class BayeuxConnection
    {
        readonly string clientId;
        readonly IBayeuxClientContext context;

        public BayeuxConnection(
            string clientId,
            IBayeuxClientContext context)
        {
            this.clientId = clientId;
            this.context = context;
        }
        
        public async Task<JObject> Connect(CancellationToken cancellationToken)
        {
            var response = await context.Request(
                new
                {
                    clientId,
                    channel = "/meta/connect",
                    connectionType = "long-polling",
                },
                cancellationToken);

            context.SetConnectionState(BayeuxClient.ConnectionState.Connected);

            return response;
        }

        public Task Disconnect(CancellationToken cancellationToken)
        {
            return context.Request(
                new
                {
                    clientId,
                    channel = "/meta/disconnect",
                },
                cancellationToken);
        }

        public Task DoSubscription(
            IEnumerable<ChannelId> channelsToSubscribe, 
            IEnumerable<ChannelId> channelsToUnsubscribe, 
            CancellationToken cancellationToken)
        {
            return context.RequestMany(
                channelsToSubscribe.Select(channel =>
                    new
                    {
                        clientId,
                        channel = "/meta/subscribe",
                        subscription = channel,
                    })
                    .Concat(channelsToUnsubscribe.Select(channel =>
                    new
                    {
                        clientId,
                        channel = "/meta/unsubscribe",
                        subscription = channel,
                    })),
                cancellationToken);
        }
    }
}
