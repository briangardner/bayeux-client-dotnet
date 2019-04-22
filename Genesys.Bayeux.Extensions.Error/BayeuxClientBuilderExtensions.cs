using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Extensions.Error
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
