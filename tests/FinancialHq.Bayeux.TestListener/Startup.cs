using System;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.Channels;
using FinancialHq.Bayeux.Extensions.ReplayId.Extensions;
using FinancialHq.Bayeux.TestListener.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinancialHQ.Bayeux.TestListener
{
    class Startup : IHostedService
    {
        private readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        private readonly IBayeuxClient _client;

        public Startup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetService<IBayeuxClient>();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info("StartAsync called");
            await _client.Start(cancellationToken).ConfigureAwait(false);
            //_client.Subscribe<Listener.TestListener>(new ChannelId("/topic/OpportunityAccountTypes"), CancellationToken.None, true);
            _client.Subscribe<Listener.TestListener>(_serviceProvider, new ChannelId("/topic/OpportunityAccountTypes2"), -2,
                CancellationToken.None, true);
            //await _client.Subscribe(new ChannelId("/topic/OpportunityAccountTypes"), cancellationToken).ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info("StopAsync called");
            await _client.Stop(cancellationToken).ConfigureAwait(false);
        }
    }
}
