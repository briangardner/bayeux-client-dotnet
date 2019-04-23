using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Channels
{
    public interface IChannel : ISubject<BayeuxMessage>
    {
        ChannelId ChannelId { get; }

    }

    public abstract class AbstractChannel : IChannel, IUnsubscribe<BayeuxMessage>
    {
        private readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        protected internal readonly IList<IObserver<BayeuxMessage>> observers;

        protected AbstractChannel(IBayeuxClientContext clientContext, ChannelId id)
        {
            observers = new List<IObserver<BayeuxMessage>>();
            ClientContext = clientContext;
            ChannelId = id;

        }

        public IBayeuxClientContext ClientContext { get; }
        public ChannelId ChannelId { get; private set; }

        protected internal async Task SendSubscribe()
        {
            var message = GetSubscribeMessage();
            await ClientContext.RequestMany(new BayeuxMessage[]{ message }, new CancellationToken()).ConfigureAwait(false);
        }

        protected internal async Task SendUnSubscribe()
        {
            await ClientContext.RequestMany(new BayeuxMessage[]{ GetUnsubscribeMessage() }, new CancellationToken()).ConfigureAwait(false);
        }

        public virtual BayeuxMessage GetSubscribeMessage()
        {
            var message = new BayeuxMessage
            {
                {MessageFields.ChannelField, "/meta/subscribe"},
                { MessageFields.SubscriptionField, ChannelId.ToString()}
            };
            return message;
        }

        public virtual BayeuxMessage GetUnsubscribeMessage()
        {
            return new BayeuxMessage
            {
                {MessageFields.ChannelField, "/meta/unsubscribe" } ,
                { MessageFields.SubscriptionField, ChannelId.ToString() }
            };
        }


        public IDisposable Subscribe(IObserver<BayeuxMessage> observer)
        {
            if (observers.Count == 0)
            {
                SendSubscribe().GetAwaiter().GetResult();
            }
            if (!observers.Contains(observer))
                observers.Add(observer);

            return new Unsubscriber<AbstractChannel, BayeuxMessage>(this, observer);
        }

        public async Task UnsubscribeAll()
        {
            observers.Clear();
            await SendUnSubscribe().ConfigureAwait(false);
        }

        public void OnCompleted()
        {
            _logger.Info("Channel {channelId} completed.", ChannelId);
        }

        public void OnError(Exception error)
        {
            _logger.Error(error, "Error in {channelId} channel", ChannelId);
        }

        public void OnNext(BayeuxMessage message)
        {
            if (message.Meta)
            {
                if (ClientContext.Extensions.Any(ext => !ext.ReceiveMeta(message)))
                {
                    return;
                }
            }
            else
            {
                if (ClientContext.Extensions.Any(ext => !ext.Receive(message)))
                {
                    return;
                }
            }
            foreach (var listener in observers)
            {
                try
                {
                    listener.OnNext(message);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error in OnMessage for listener: {listener}.  Message {@message}",ex, listener.GetType().FullName, message);
                }
            }
        }

        public async Task UnsubscribeAsync(IObserver<BayeuxMessage> observer)
        {
            if (observer != null && this.observers.Contains(observer))
                observers.Remove(observer);
            if (observers.Count == 0)
            {
                await SendUnSubscribe().ConfigureAwait(false);
            }
        }
    } 
}