using System;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client.Channels
{
    internal class Unsubscriber : IDisposable
    {
        private readonly IObserver<IMessage> _observer;
        private readonly AbstractChannel _channel;

        public Unsubscriber(AbstractChannel channel, IObserver<IMessage> observer)
        {
            _observer = observer;
            _channel = channel;
        }

        public void Dispose()
        {
            _channel.Unsubscribe(_observer);
        }
    }
}
