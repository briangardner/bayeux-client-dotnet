using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client
{
    public interface IMessageListener
    {
        void OnMessage(IChannel channel, IMessage message);
    }
}