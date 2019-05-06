using FinancialHq.Bayeux.Client.Builders;
using FinancialHq.Bayeux.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Extensions.TimesyncClient.Extensions
{
    public static class BayeuxClientBuilderExtensions
    {
        public static IBayeuxClientBuilder AddTimesyncClient(this IBayeuxClientBuilder builder)
        {
            builder.Services.AddTransient<IExtension, TimesyncClientExtension>();
            return new BayeuxClientBuilder(builder.Services);
        }
    }
}
