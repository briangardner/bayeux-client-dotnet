using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Channels
{
    public interface IChannel : ISubject<IMessage>
    {
        ChannelId ChannelId { get; }

    }

    public abstract class AbstractChannel : IChannel, IUnsubscribe<IMessage>
    {
        private readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        protected internal readonly IList<IObserver<IMessage>> observers;

        protected AbstractChannel(IBayeuxClientContext clientContext, ChannelId id)
        {
            observers = new List<IObserver<IMessage>>();
            ClientContext = clientContext;
            ChannelId = id;

        }

        public IBayeuxClientContext ClientContext { get; }
        public ChannelId ChannelId { get; private set; }

        protected internal async Task SendSubscribe()
        {
            var message = GetSubscribeMessage();
            await ClientContext.RequestMany(new JObject[]{ message }, new CancellationToken()).ConfigureAwait(false);
        }

        protected internal async Task SendUnSubscribe()
        {
            await ClientContext.RequestMany(new JObject[]{ GetUnsubscribeMessage() }, new CancellationToken()).ConfigureAwait(false);
        }

        public virtual JObject GetSubscribeMessage()
        {
            var message = new JObject
            {
                {MessageFields.ChannelField, "/meta/subscribe"},
                { MessageFields.SubscriptionField, ChannelId.ToString()}
            };
            return message;
        }

        public virtual JObject GetUnsubscribeMessage()
        {
            return new JObject
            {
                {MessageFields.ChannelField, "/meta/unsubscribe" } ,
                { MessageFields.SubscriptionField, ChannelId.ToString() }
            };
        }


        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            if (observers.Count == 0)
            {
                SendSubscribe().GetAwaiter().GetResult();
            }
            if (!observers.Contains(observer))
                observers.Add(observer);

            return new Unsubscriber<AbstractChannel, IMessage>(this, observer);
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

        public void OnNext(IMessage message)
        {
            foreach (var listener in observers)
            {
                try
                {
                    listener.OnNext(message);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in OnMessage for listener: {listener}.  Message {@message}", listener.GetType().FullName, message);
                }
            }
        }

        public async Task UnsubscribeAsync(IObserver<IMessage> observer)
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