using System;
using System.Collections.Generic;
using System.Reactive.Linq;
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

    public abstract class AbstractChannel : IChannel
    {
        private readonly IBayeuxClientContext _clientContext;
        private readonly ILog _logger;
        protected internal readonly IList<IObserver<IMessage>> observers;

        protected AbstractChannel(IBayeuxClientContext clientContext, ChannelId id)
        {
            observers = new List<IObserver<IMessage>>();
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

        internal virtual JObject GetSubscribeMessage()
        {
            var message = new JObject
            {
                {"channel", "/meta/subscribe"},
                { "subscription", ChannelId.ToString()}
            };
            return message;
        }

        internal virtual dynamic GetUnsubscribeMessage()
        {
            return new JObject
            {
                {"channel", "/meta/unsubscribe" } ,
                { "subscription", ChannelId.ToString() }
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

            return new Unsubscriber(this, observer);
        }

        public async Task Unsubscribe(IObserver<IMessage> observer)
        {
            if (observer != null && this.observers.Contains(observer))
                observers.Remove(observer);
            if (observers.Count == 0)
            {
                await SendUnSubscribe().ConfigureAwait(false);
            }
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
                    _logger.Error(ex, "Error in OnMessage for listener: {listener}", listener.GetType().FullName);
                }
            }
        }
    } 
}