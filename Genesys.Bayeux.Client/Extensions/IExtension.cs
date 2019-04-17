using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client.Extensions
{
    public interface IExtension
    {
        bool Receive(AbstractChannel channel, BayeuxMessage message);
        bool ReceiveMeta(AbstractChannel channel, BayeuxMessage message);
        bool Send(AbstractChannel channel, BayeuxMessage message);
        bool SendMeta(AbstractChannel channel, BayeuxMessage message);
    }
}