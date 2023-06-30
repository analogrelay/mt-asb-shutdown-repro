using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShutdownReproApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, builder) => builder.AddUserSecrets<Program>())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IHostedService, RequesterService>();
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        // By default, sagas are in-memory, but should be changed to a durable
                        // saga repository.
                        x.SetInMemorySagaRepositoryProvider();

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddConsumers(entryAssembly);
                        x.AddSagaStateMachines(entryAssembly);
                        x.AddSagas(entryAssembly);
                        x.AddActivities(entryAssembly);

                        // Change this to ASB to see the issue
                        // x.UsingInMemory((context, cfg) =>
                        // {
                        //     cfg.ConfigureEndpoints(context);
                        // });
                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            var config = context.GetRequiredService<IConfiguration>();
                            cfg.Host(config["AzureServiceBus:ConnectionString"]);
                            cfg.ConfigureEndpoints(context);
                        });
                    });
                });
    }
}
