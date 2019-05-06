using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client.Logging;
using FinancialHq.Bayeux.Client.Messaging;

namespace FinancialHq.Bayeux.Client.Channels
{
    public interface IChannel : ISubject<BayeuxMessage>
    {
        // ReSharper disable once UnusedMember.Global
        ChannelId ChannelId { get; }

    }

    public abstract class AbstractChannel : IChannel, IUnsubscribe<BayeuxMessage>
    {
        private readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        public IList<IObserver<BayeuxMessage>> Observers { get; protected set; }

        protected AbstractChannel(IBayeuxClientContext clientContext, ChannelId id)
        {
            Observers = new List<IObserver<BayeuxMessage>>();
            ClientContext = clientContext;
            ChannelId = id;

        }

        public IBayeuxClientContext ClientContext { get; }
        public ChannelId ChannelId { get; }

        public async Task SendSubscribe()
        {
            _logger.Info("Sending subscribe request.");
            var message = GetSubscribeMessage();
            await ClientContext.Request(message, new CancellationToken()).ConfigureAwait(false);
        }

        protected internal async Task SendUnSubscribe()
        {
            _logger.Info("Sending unsubscribe request.");
            var message = GetUnsubscribeMessage();
            await ClientContext.Request(message, new CancellationToken()).ConfigureAwait(false);
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
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            if (Observers.Count == 1)
            {
                SendSubscribe().GetAwaiter().GetResult();
            }

            return new Unsubscriber<AbstractChannel, BayeuxMessage>(this, observer);
        }

        public void OnCompleted()
        {
            _logger.Info("Channel {channelId} completed.", ChannelId);
        }

        public void OnError(Exception error)
        {
            _logger.Error(error, "Error in {channelId} channel", ChannelId);
        }

        public virtual void OnNext(BayeuxMessage message)
        {
            if (!message.ChannelId.Equals(ChannelId))
            {
                _logger.Debug("Skipping Message.  Message ChannelId {msgChannel} does not match ChannelId {channel}", message.ChannelId, ChannelId);
                return;
            }
            if (message.Meta)
            {
                foreach (var ext in ClientContext.Extensions)
                {
                    if (ext.ReceiveMeta(message)) continue;
                    _logger.Debug("Skipping Message. {ext} ReceiveMeta was false", ext.GetType().Name);
                    return;
                }
            }
            else
            {
                foreach (var ext in ClientContext.Extensions)
                {
                    if (ext.Receive(message)) continue;
                    _logger.Debug("Skipping Message. {ext} Receive was false", ext.GetType().Name);
                    return;
                }
            }
            foreach (var listener in Observers)
            {
                try
                {
                    listener.OnNext(message);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error in OnMessage for listener: {listener}.  Message {@message}",ex, listener.GetType().FullName, message);
                    throw;
                }
            }
        }

        public async Task UnsubscribeAsync(IObserver<BayeuxMessage> observer)
        {
            if (observer != null && Observers.Contains(observer))
                Observers.Remove(observer);
            if (Observers.Count == 0)
            {
                await SendUnSubscribe().ConfigureAwait(false);
            }
        }
    } 
}