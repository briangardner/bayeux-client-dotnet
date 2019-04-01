using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public class DurableMessage : MessageExtension
    {
        public DurableMessage(IMutableMessage message) : base(message)
        {
        }

        public long ReplayId {
            get
            {
                TryGetValue(MessageFields.REPLAY_ID_FIELD, out var obj);
                if (obj != null)
                {
                    return (long)obj;
                }
                return 0;
            }
            set => this[MessageFields.REPLAY_ID_FIELD] = value;
        }
    }
}
