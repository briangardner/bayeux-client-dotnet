using System;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Client.Builders
{
    public interface IWebSocketsPollingClientBuilder
    {
        IServiceCollection Services { get; }
    }
    public class WebSocketsPollingClientBuilder : IWebSocketsPollingClientBuilder
    {

        public WebSocketsPollingClientBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
        public IServiceCollection Services { get; }
    }
}
