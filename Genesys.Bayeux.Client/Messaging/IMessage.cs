using System.Collections.Generic;
using Genesys.Bayeux.Client.Channels;

namespace Genesys.Bayeux.Client.Messaging
{
    public interface IMessage : IDictionary<string, object>
    {
        
    }
}