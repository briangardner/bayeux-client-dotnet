using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Messaging;

namespace Genesys.Bayeux.Client.Channels
{
    public interface IChannel
    {
        ChannelId ChannelId { get; }

    }

    public abstract class AbstractChannel : IChannel
    {
        private readonly ILog _logger;
        protected internal IList<IMessageListener> subscriptions ;

        protected AbstractChannel()
        {
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

        protected internal abstract Task SendSubscribe();
        protected internal abstract Task SendUnSubscribe();

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