using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FinancialHq.Salesforce.ApiClient;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genesys.Bayeux.TestListener
{
    class Startup : IHostedService
    {
        private ILogger<Startup> _logger;
        private IApplicationLifetime _appLifetime;
        private IServiceProvider _serviceProvider;
        private IBayeuxClient _client;

        public Startup(ILogger<Startup> logger, IApplicationLifetime appLifetime, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
            _client = _serviceProvider.GetService<IBayeuxClient>();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
           
            _client.AddSubscriptions(new ChannelId( "/topic/OpportunityAccountTypes"));

            /*client.EventReceived += (e, args) =>
                _logger.LogInformation($"Event received on channel {args.Channel} with data\n{args.Data}");*/
            var firstConnection = true;
            _client.StartInBackground();

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Stop(cancellationToken).ConfigureAwait(false);
            //throw new NotImplementedException();
        }
    }
}
