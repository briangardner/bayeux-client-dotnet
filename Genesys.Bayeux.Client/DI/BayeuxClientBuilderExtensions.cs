using System;
using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.Logging;
using Genesys.Bayeux.Client.Options;
using Genesys.Bayeux.Client.Transport;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace Genesys.Bayeux.Client.DI
{
    public static class BayeuxClientBuilderExtensions
    {
        internal static ILog logger = LogProvider.GetCurrentClassLogger();
        public static IHttpLongPollingClientBuilder UseHttpLongPolling(this IBayeuxClientBuilder builder,
            HttpLongPollingTransportOptions httpLongPollingTransportOptions,
            Action<ReconnectDelayOptions> reconnectDelayOptions = null,
            Action<RetryPolicy> httpRetryPolicy = null )
        {
            if(builder == null) throw new ArgumentNullException(nameof(builder));
            if(httpLongPollingTransportOptions == null) throw new ArgumentNullException(nameof(httpLongPollingTransportOptions));

            if (httpRetryPolicy != null)
            {
                builder.Services.Configure(httpRetryPolicy);
            }
            else
            {
                var retryPolicy = Policy.Handle<Exception>()
                    .WaitAndRetryAsync(5,retryCount =>
                    {
                        const int maxWait = 5 * 60; // max wait of 5 minutes
                        var wait = Math.Pow(2, retryCount) < maxWait ? Math.Pow(2, retryCount) : maxWait;
                        return TimeSpan.FromSeconds(wait);
                    }, (exception, timeSpan, context) =>
                    {
                        logger.WarnException("Retrying Http call. Waiting {wait} seconds.",exception, timeSpan.Seconds);
                    });
                builder.Services.AddTransient<Policy>((provider) => retryPolicy);
            }
            if (reconnectDelayOptions != null)
            {
                builder.Services.Configure(reconnectDelayOptions);
            }

            
            builder.Services.Configure<HttpLongPollingTransportOptions>(options =>
                {
                    options.HttpClient = httpLongPollingTransportOptions.HttpClient;
                    options.Uri = httpLongPollingTransportOptions.Uri;
                });
            builder.Services.AddTransient<IBayeuxTransport, HttpLongPollingTransport>();
            return new HttpLongPollingClientBuilder(builder.Services);
        }

        // ReSharper disable once UnusedMember.Global
        public static IWebSocketsPollingClientBuilder UseWebSockets(this IBayeuxClientBuilder builder, Action<WebSocketTransportOptions> webSocketTransportOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (webSocketTransportOptions == null) throw new ArgumentNullException(nameof(webSocketTransportOptions));

            builder.Services.Configure(webSocketTransportOptions);
            builder.Services.AddTransient<IBayeuxTransport, WebSocketTransport>();
            return new WebSocketsPollingClientBuilder(builder.Services);
        }

        public static IBayeuxClientBuilder AddBayeuxClient(this IServiceCollection services)
        {
            if(services == null) throw new ArgumentNullException(nameof(services));
            AddServices(services);
            return new BayeuxClientBuilder(services);
        }

        private static void AddServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IBayeuxClientContext, BayeuxClientContext>();
            serviceCollection.AddTransient<IBayeuxClient, BayeuxClient>();
        }
    }
}
