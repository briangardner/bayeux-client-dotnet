using FinancialHq.Bayeux.Client.Channels;

namespace FinancialHq.Bayeux.Extensions.ReplayId
{
    public class ReplayChannelId : ChannelId
    {
        public ReplayChannelId(string name, long replayId =-1) : base(name)
        {
            ReplayId = replayId;
        }
        public long ReplayId { get; set; }


    }
}
