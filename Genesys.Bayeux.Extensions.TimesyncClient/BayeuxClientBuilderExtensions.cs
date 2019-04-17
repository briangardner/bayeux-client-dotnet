using System;
using System.Transactions;
using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Extensions.TimesyncClient
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
