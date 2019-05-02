using System;
using Genesys.Bayeux.Client.Logging;

namespace Genesys.Bayeux.Client.Channels
{
    internal class Unsubscriber<TPublisher, TType> : IDisposable
        where TType:class
        where TPublisher:IUnsubscribe<TType>
    {
        private readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        private readonly IObserver<TType> _observer;
        private readonly TPublisher _publisher;

        public Unsubscriber(TPublisher publisher, IObserver<TType> observer)
        {
            _observer = observer;
            _publisher = publisher;
        }

        public void Dispose()
        {
            _logger.Info("Unsubscribing {publisher} from {observer}", _publisher.GetType(), _observer.GetType());
            _publisher.UnsubscribeAsync(_observer).GetAwaiter().GetResult();
        }
    }
}
