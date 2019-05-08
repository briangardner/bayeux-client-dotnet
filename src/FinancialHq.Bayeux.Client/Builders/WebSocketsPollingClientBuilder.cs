﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialHq.Bayeux.Client.Builders
{
    public interface IWebSocketsPollingClientBuilder
    {
        // ReSharper disable once UnusedMember.Global
        IServiceCollection Services { get; }
    }
    public class WebSocketsPollingClientBuilder : IWebSocketsPollingClientBuilder
    {

        public WebSocketsPollingClientBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
        public IServiceCollection Services { get; }
    }
}