using System;
using System.Collections.Generic;
using System.Text;
using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.Extensions;
using Genesys.Bayeux.Extensions.ReplayId.Builders;
using Genesys.Bayeux.Extensions.ReplayId.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Genesys.Bayeux.Extensions.ReplayId
{
    public static class BayeuxClientBuilderExtensions
    {
        public static IReplayIdExtensionBuilder AddReplayIdExtension(this IBayeuxClientBuilder builder)
        {
            builder.Services.AddTransient<IExtension, ReplayExtension>();
            return new ReplayIdExtensionBuilder(builder.Services);
        }
        public static IBayeuxClientBuilder WithDistributedMemoryCache(this IReplayIdExtensionBuilder builder)
        {
            builder.Services.AddDistributedMemoryCache();
            return new BayeuxClientBuilder(builder.Services);
        }

        public static IBayeuxClientBuilder WithSqlServerDistributedCache(this IReplayIdExtensionBuilder builder,
            Action<SqlServerCacheOptions> cacheConfig)
        {
            builder.Services.Configure(cacheConfig)
                .PostConfigure<SqlServerCacheOptions>(options => options.ThrowIfInvalid());
            var provider = builder.Services.BuildServiceProvider();
            var sqlConfig = provider.GetService<IOptions<SqlServerCacheOptions>>();
            builder.Services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = sqlConfig.Value.ConnectionString;
                options.SchemaName = sqlConfig.Value.Schema;
                options.TableName = sqlConfig.Value.Table;
                options.DefaultSlidingExpiration = TimeSpan.FromSeconds(sqlConfig.Value.ExpirationInSeconds);
            });
            return new BayeuxClientBuilder(builder.Services);
        }
    }
}
