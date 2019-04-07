﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Channels
{
    public interface IChannel :  IObservable<IMessage>
    {
        ChannelId ChannelId { get; }

    }

    public abstract class AbstractChannel : IChannel
    {
        private readonly IBayeuxClientContext _clientContext;
        private readonly ILog _logger;
        protected internal readonly IList<IObserver<IMessage,Task>> observers;

        protected AbstractChannel(IBayeuxClientContext clientContext, ChannelId id)
        {
            observers = new List<IObserver<IMessage, Task>>();
            _clientContext = clientContext;
            LogProvider.LogProviderResolvers.Add(
                new Tuple<LogProvider.IsLoggerAvailable, LogProvider.CreateLogProvider>(() => true, () => new TraceSourceLogProvider()));

            _logger = LogProvider.GetLogger(typeof(AbstractChannel).Namespace);
            ChannelId = id;

        }

        public IBayeuxClientContext ClientContext => _clientContext;
        public ChannelId ChannelId { get; private set; }

        protected internal async Task SendSubscribe()
        {
            await _clientContext.Request(GetSubscribeMessage(), new CancellationToken());
        }

        protected internal async Task SendUnSubscribe()
        {
            await _clientContext.Request(GetUnsubscribeMessage(), new CancellationToken());
        }

        protected virtual JObject GetSubscribeMessage()
        {
            var message = new JObject
            {
                {"channel", "/meta/subscribe"},
                { "subscription", ChannelId.ToString()}
            };
            return message;
        }

        protected virtual dynamic GetUnsubscribeMessage()
        {
            return new JObject
            {
                {"channel", "/meta/unsubscribe" } ,
                { "subscription", ChannelId.ToString() }
            };
        }

        public async Task NotifyMessageListeners(IMessage message)
        {
            foreach (var listener in observers)
            {
                try
                {
                    await listener.OnNext(message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in OnMessage for listener: {listener}", listener.GetType().FullName);
                }
            }
        }

        public IDisposable Subscribe(IObserver<IMessage, Task> observer)
        {
            if (observers.Count == 0)
            {
                SendSubscribe().GetAwaiter().GetResult();
            }
            if (!observers.Contains(observer))
                observers.Add(observer);
            
            return new Unsubscriber(this, observer);
        }

    } 
}