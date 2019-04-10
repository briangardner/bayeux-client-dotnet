using System;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Client.Builders
{
    public interface IHttpLongPollingClientBuilder
    {
        IServiceCollection Services { get; }
    }

    public class HttpLongPollingClientBuilder : IHttpLongPollingClientBuilder
    {

        public HttpLongPollingClientBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
        public IServiceCollection Services { get; }
    }
}