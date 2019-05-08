using System;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Builders;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Extensions.ReplayId.Builders;
using FinancialHq.Bayeux.Extensions.ReplayId.Options;
using FinancialHq.Bayeux.Extensions.ReplayId.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinancialHq.Bayeux.Extensions.ReplayId.Extensions
{
    public static class BayeuxClientBuilderExtensions
    {
        public static IReplayIdExtensionBuilder AddReplayIdExtension(this IBayeuxClientBuilder builder, IRetrieveReplayIdStrategy retrieveReplayId )
        {
            builder.Services.AddTransient<IExtension, ReplayExtension>();
            builder.Services.AddTransient<ISubscriberCache, ReplaySubscriberCache>();
            builder.Services.AddSingleton(retrieveReplayId);
            return new ReplayIdExtensionBuilder(builder.Services);
        }
        public static IBayeuxClientBuilder WithDistributedMemoryCache(this IReplayIdExtensionBuilder builder)
        {
            builder.Services.AddDistributedMemoryCache();
            return new BayeuxClientBuilder(builder.Services);
        }

        // ReSharper disable once UnusedMember.Global
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
