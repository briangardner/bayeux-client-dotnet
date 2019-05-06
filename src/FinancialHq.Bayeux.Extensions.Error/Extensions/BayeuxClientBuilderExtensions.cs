using FinancialHq.Bayeux.Client.Builders;
using FinancialHq.Bayeux.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Extensions.Error.Extensions
{
    public static class BayeuxClientBuilderExtensions
    {
        public static IBayeuxClientBuilder AddErrorExtension(this IBayeuxClientBuilder builder)
        {
            builder.Services.AddTransient<IExtension, ErrorExtension>();
            return new BayeuxClientBuilder(builder.Services);
        }
    }
}
