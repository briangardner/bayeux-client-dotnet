using System;

namespace Genesys.Bayeux.Client.Channels
{
    internal class Unsubscriber<TPublisher, TType> : IDisposable
        where TType:class
        where TPublisher:IUnsubscribe<TType>
    {
        private readonly IObserver<TType> _observer;
        private readonly TPublisher _publisher;

        public Unsubscriber(TPublisher publisher, IObserver<TType> observer)
        {
            _observer = observer;
            _publisher = publisher;
        }

        public void Dispose()
        {
            _publisher.UnsubscribeAsync(_observer).GetAwaiter().GetResult();
        }
    }
}
