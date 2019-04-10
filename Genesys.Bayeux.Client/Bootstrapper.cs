using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Client
{
    public static class Bootstrapper
    {
        public static IBayeuxClientBuilder UseBayeuxClient(this IServiceCollection services)
        {
            services.AddTransient<IBayeuxClientContext, BayeuxClient>();

            return new BayeuxClientBuilder(services);
        }
    }
}
