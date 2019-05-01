using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FinancialHq.Salesforce.ApiClient;
using Genesys.Bayeux.Client;
using Genesys.Bayeux.Client.DI;
using Genesys.Bayeux.Client.Options;
using Genesys.Bayeux.Extensions.Ack;
using Genesys.Bayeux.Extensions.Ack.Extensions;
using Genesys.Bayeux.Extensions.Error;
using Genesys.Bayeux.Extensions.Error.Extensions;
using Genesys.Bayeux.Extensions.ReplayId;
using Genesys.Bayeux.Extensions.ReplayId.Extensions;
using Genesys.Bayeux.Extensions.TimesyncClient;
using Genesys.Bayeux.Extensions.TimesyncClient.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Genesys.Bayeux.TestListener
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(hostConfig => { hostConfig.SetBasePath(Directory.GetCurrentDirectory()); })
                .ConfigureAppConfiguration((hostingContext, appConfig) =>
                {
                    //appConfig.AddJsonFile("applicationsettings.json", false);
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .WriteTo.Console()
                        .WriteTo.File("log.json", rollingInterval: RollingInterval.Day)
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        //.Enrich.FromLogContext()
                        .CreateLogger();
                    loggingBuilder.AddSerilog();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddTransient<IMessageListener, Listener.TestListener>();

                    services.AddSalesforceApi(options =>
                    {
                        options.ApiVersion = "45";
                        options.SalesforceLoginUri = "https://cs14.salesforce.com";
                        options.TokenCachingInMinutes = 60;
                    }).UseUserNamePasswordFlow(options =>
                    {
                        options.ClientId =
                            "3MVG9sLbBxQYwWqucek6Tyk.w7jSQLDtDmNF4tV3SuPVEbTOwTYMSkMsgPnmD8A7U0zTX3kV7OC5IRFcb3le3";
                        options.ClientSecret = "62F49832C0AE27B651F2195A96E575DE036C238365453A6E87C9EA7A42F472D0";
                        options.UserName = "stpapi@usafinancial.com.vnext";
                        options.Password = "f7by@68K@@ASk7GNnUH2oqdljPo2NILPd5qnKsCs";
                    });
                    var provider = services.BuildServiceProvider();
                    var salesforceApiClient = provider.GetService<ISalesforceApiClient>();
                    salesforceApiClient.Client.Timeout = new TimeSpan(0, 0, 2, 10, 0);
                    services.UseBayeuxClient()
                        .AddAckExtension()
                        .AddErrorExtension()
                        .AddTimesyncClient()
                        .AddReplayIdExtension().WithDistributedMemoryCache()
                        .UseHttpLongPolling(new HttpLongPollingTransportOptions()
                        {
                            HttpClient = salesforceApiClient.Client,
                            Uri = "https://cs14.salesforce.com/cometd/42.0"
                        }, reconnectDelayOptions: options => options.ReconnectDelays = new List<TimeSpan>()
                        {
                            new TimeSpan(0, 0, 0, 5),
                            new TimeSpan(0, 0, 0, 15),
                            new TimeSpan(0, 0, 0, 30),
                        });
                    services.AddHostedService<Startup>();
                })
                .UseConsoleLifetime();
            await hostBuilder.RunConsoleAsync().ConfigureAwait(false);

        }
    }
}
