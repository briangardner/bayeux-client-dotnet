using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Channels
{
    public abstract class ChannelExtension : AbstractChannel
    {
        protected AbstractChannel channel;
        protected ChannelExtension(AbstractChannel channel) : base(channel.ChannelId)
        {
            this.channel = channel;
        }
    }
}
