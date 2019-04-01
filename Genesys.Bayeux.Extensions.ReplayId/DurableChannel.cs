using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class DurableChannel : ChannelExtension
    {
        public DurableChannel(AbstractChannel channel) : base(channel)
        {
            
        }

        public int ReplayId { get; set; }
    }
}
