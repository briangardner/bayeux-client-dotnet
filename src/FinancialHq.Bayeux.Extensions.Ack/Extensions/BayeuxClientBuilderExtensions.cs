using FinancialHq.Bayeux.Client.Builders;
using FinancialHq.Bayeux.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Extensions.Ack.Extensions
{
    public static class BayeuxClientBuilderExtensions
    {
        public static IBayeuxClientBuilder AddAckExtension(this IBayeuxClientBuilder builder)
        {
            builder.Services.AddTransient<IExtension, AckExtension>();
            return new BayeuxClientBuilder(builder.Services);
        }
    }
}
