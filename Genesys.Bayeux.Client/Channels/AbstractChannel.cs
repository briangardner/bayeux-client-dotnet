using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Channels
{
    public interface IChannel
    {
        ChannelId ChannelId { get; }

    }

    public abstract class AbstractChannel : IChannel
    {
        private readonly IBayeuxClientContext _clientContext;
        private readonly ILog _logger;
        protected internal IList<IMessageListener> subscriptions ;

        protected AbstractChannel(IBayeuxClientContext clientContext)
        {
            _clientContext = clientContext;
            LogProvider.LogProviderResolvers.Add(
                new Tuple<LogProvider.IsLoggerAvailable, LogProvider.CreateLogProvider>(() => true, () => new TraceSourceLogProvider()));

            _logger = LogProvider.GetLogger(typeof(AbstractChannel).Namespace);

            subscriptions = new List<IMessageListener>();
        }
        protected AbstractChannel(ChannelId id)
        {
            ChannelId = id;
        }
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
            return new
            {
                channel = "/meta/unsubscribe",
                subscription = ChannelId,
            };
        }

        public void NotifyMessageListeners(IMessage message)
        {
            foreach (var listener in subscriptions)
            {
                try
                {
                    listener.OnMessage(this, message);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in OnMessage for listener: {listener}", listener.GetType().FullName);
                }
            }
        }
        
    } 
}