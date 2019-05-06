using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Enums;
using FinancialHq.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace FinancialHq.Bayeux.Client.Connectivity
{
    public class BayeuxConnection
    {
        public readonly string ClientId;
        private readonly IBayeuxClientContext _context;

        public BayeuxConnection(
            string clientId,
            IBayeuxClientContext context)
        {
            ClientId = clientId;
            _context = context;
        }
        
        public async Task<JObject> Connect(CancellationToken cancellationToken)
        {
            var request = new BayeuxMessage
            {
                {MessageFields.ClientIdField, ClientId }, {MessageFields.ChannelField, "/meta/connect"}, { MessageFields.ConnectionTypeField, "long-polling"}
            };
            var response = await _context.Request(request,
                cancellationToken).ConfigureAwait(false);

            await _context.SetConnectionState(ConnectionState.Connected).ConfigureAwait(false);

            return response;
        }

        public Task Disconnect(CancellationToken cancellationToken)
        {
            var request = new BayeuxMessage
            {
                {MessageFields.ClientIdField, ClientId }, {MessageFields.ChannelField, "/meta/disconnect"}
            };
            return _context.Request(
                request,
                cancellationToken);
        }

    }
}
