using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client.Channels
{
    internal class Unsubscriber : IDisposable
    {
        private readonly IObserver<IMessage, Task> _observer;
        private readonly AbstractChannel _channel;

        public Unsubscriber(AbstractChannel channel, IObserver<IMessage, Task> observer)
        {
            _observer = observer;
            _channel = channel;
        }
        public void Dispose()
        {
            if (_observer != null && _channel.observers.Contains(_observer))
                _channel.observers.Remove(_observer);
            if (_channel.observers.Count == 0)
            {
                _channel.SendUnSubscribe().GetAwaiter().GetResult();
            }
        }
    }
}
