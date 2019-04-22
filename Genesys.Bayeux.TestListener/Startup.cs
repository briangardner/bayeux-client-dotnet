using System;
using System.Threading;
using System.Threading.Tasks;
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
            _client.StartInBackground();
            await Task.CompletedTask.ConfigureAwait(false);

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.Stop(cancellationToken).ConfigureAwait(false);
            //throw new NotImplementedException();
        }
    }
}
