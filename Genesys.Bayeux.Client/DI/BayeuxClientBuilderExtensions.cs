using System;
using Genesys.Bayeux.Client.Builders;
using Genesys.Bayeux.Client.Connectivity;
using Genesys.Bayeux.Client.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Genesys.Bayeux.Client.DI
{
    public static class BayeuxClientBuilderExtensions
    {
        public static IHttpLongPollingClientBuilder UseHttpLongPolling(this IBayeuxClientBuilder builder,
            HttpLongPollingTransportOptions httpLongPollingTransportOptions,
            Action<ReconnectDelayOptions> reconnectDelayOptions = null)
        {
            if(builder == null) throw new ArgumentNullException(nameof(builder));
            if(httpLongPollingTransportOptions == null) throw new ArgumentNullException(nameof(httpLongPollingTransportOptions));

            if (reconnectDelayOptions != null)
            {
                builder.Services.Configure(reconnectDelayOptions);
            }
            builder.Services.Configure<HttpLongPollingTransportOptions>(options =>
                {
                    options.HttpClient = httpLongPollingTransportOptions.HttpClient;
                    options.HttpPost = httpLongPollingTransportOptions.HttpPost;
                    options.Uri = httpLongPollingTransportOptions.Uri;
                });
            builder.Services.AddTransient<IBayeuxTransport, HttpLongPollingTransport>();
            return new HttpLongPollingClientBuilder(builder.Services);
        }

        public static IWebSocketsPollingClientBuilder UseWebSockets(this IBayeuxClientBuilder builder, Action<WebSocketTransportOptions> webSocketTransportOptions)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (webSocketTransportOptions == null) throw new ArgumentNullException(nameof(webSocketTransportOptions));

            builder.Services.Configure(webSocketTransportOptions);
            builder.Services.AddTransient<IBayeuxTransport, WebSocketTransport>();
            return new WebSocketsPollingClientBuilder(builder.Services);
        }
    }
}
