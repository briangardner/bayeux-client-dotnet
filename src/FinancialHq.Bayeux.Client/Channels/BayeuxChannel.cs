namespace FinancialHq.Bayeux.Client.Channels
{
    public class BayeuxChannel : AbstractChannel
    {
        public BayeuxChannel(IBayeuxClientContext clientContext, ChannelId id) : base(clientContext, id)
        {
        }
    }
}
