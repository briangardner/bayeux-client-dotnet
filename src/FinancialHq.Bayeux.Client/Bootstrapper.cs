using FinancialHq.Bayeux.Client.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Client
{
    public static class Bootstrapper
    {
        public static IBayeuxClientBuilder UseBayeuxClient(this IServiceCollection services)
        {
            services.AddSingleton<IBayeuxClientContext, BayeuxClientContext>();
            services.AddTransient<IBayeuxClient, BayeuxClient>();
            services.AddTransient<ISubscriberCache, SubscriberCache>();
            return new BayeuxClientBuilder(services);
        }
    }
}
