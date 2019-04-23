using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client.Extensions
{
    public interface IExtension
    {
        bool Receive(BayeuxMessage message);
        bool ReceiveMeta(BayeuxMessage message);
        bool Send(BayeuxMessage message);
        bool SendMeta(BayeuxMessage message);
    }
}