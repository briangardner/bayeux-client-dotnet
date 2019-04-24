using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Extensions.ReplayId
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
