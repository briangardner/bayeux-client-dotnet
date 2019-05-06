using System;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Extensions.ReplayId.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinancialHQ.Bayeux.TestListener
{
    class Startup : IHostedService
    {
        private ILogger<Startup> _logger;
        private IApplicationLifetime _appLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBayeuxClient _client;

        public Startup(ILogger<Startup> logger, IApplicationLifetime appLifetime, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetService<IBayeuxClient>();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _client.Start(cancellationToken).ConfigureAwait(false);
            //_client.Subscribe<Listener.TestListener>(new ChannelId("/topic/OpportunityAccountTypes"), CancellationToken.None, true);
            _client.Subscribe<Listener.TestListener>(_serviceProvider, new ChannelId("/topic/OpportunityAccountTypes2"), -2,
                CancellationToken.None, true);
            //await _client.Subscribe(new ChannelId("/topic/OpportunityAccountTypes"), cancellationToken).ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Stop(cancellationToken).ConfigureAwait(false);
        }
    }
}
