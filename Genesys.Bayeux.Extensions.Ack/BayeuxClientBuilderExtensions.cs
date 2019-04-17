using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Extensions.Ack
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
