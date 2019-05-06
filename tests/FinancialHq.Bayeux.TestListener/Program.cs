using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FinancialHq.Salesforce.ApiClient;
using FinancialHq.Bayeux.Client;
using FinancialHq.Bayeux.Client.DI;
using FinancialHq.Bayeux.Client.Listeners;
using FinancialHq.Bayeux.Client.Options;
using FinancialHq.Bayeux.Extensions.Ack.Extensions;
using FinancialHq.Bayeux.Extensions.Error.Extensions;
using FinancialHq.Bayeux.Extensions.ReplayId.Extensions;
using FinancialHq.Bayeux.Extensions.TimesyncClient.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FinancialHQ.Bayeux.TestListener
{
    internal class Program
    {
        // ReSharper disable once UnusedParameter.Local
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
