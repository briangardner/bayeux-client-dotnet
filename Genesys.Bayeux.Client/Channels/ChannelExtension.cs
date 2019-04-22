namespace Genesys.Bayeux.Client.Channels
{
    public abstract class ChannelExtension : AbstractChannel 
    {
        protected AbstractChannel channel;
        protected ChannelExtension(IBayeuxClientContext clientContext, AbstractChannel channel) : base(clientContext, channel.ChannelId)
        {
            this.channel = channel;
        }
    }
}
