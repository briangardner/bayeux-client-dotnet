using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Connectivity
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
            var request = new JObject()
            {
                {MessageFields.CLIENT_ID_FIELD, clientId }, {MessageFields.CHANNEL_FIELD, "/meta/connect"}, { MessageFields.CONNECTION_TYPE_FIELD, "long-polling"}
            };
            var response = await context.Request(request,
                cancellationToken).ConfigureAwait(false);

            await context.SetConnectionState(BayeuxClient.ConnectionState.Connected).ConfigureAwait(false);

            return response;
        }

        public Task Disconnect(CancellationToken cancellationToken)
        {
            var request = new JObject()
            {
                {MessageFields.CLIENT_ID_FIELD, clientId }, {MessageFields.CHANNEL_FIELD, "/meta/disconnect"}
            };
            return context.Request(
                request,
                cancellationToken);
        }

    }
}
