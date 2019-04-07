using System;
using Genesys.Bayeux.Client.Channels;
using Genesys.Bayeux.Client.Messaging;
using System.Reactive;
using System.Threading.Tasks;

namespace Genesys.Bayeux.Client
{
    public interface IMessageListener : IObserver<IMessage, Task>
    {

    }
}