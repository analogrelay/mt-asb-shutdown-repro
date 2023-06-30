using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ShutdownReproApp
{
    public class RequesterService: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RequesterService> _logger;

        public RequesterService(IServiceScopeFactory scopeFactory, ILogger<RequesterService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (!stoppingToken.IsCancellationRequested)
            {
                // Fire and forget, on purpose!
                _ = RunRequestCycleAsync(_scopeFactory.CreateScope().ServiceProvider);
                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
            _logger.LogInformation("Stopping publish loop.");
        }

        async Task RunRequestCycleAsync(IServiceProvider services)
        {
            var requestClient = services.GetRequiredService<IRequestClient<RequestMessage>>();
            var messageId = Guid.NewGuid().ToString("N");
            _logger.LogInformation("Publishing RequestMessage: {MyId}", messageId);
            var response = await requestClient.GetResponse<ResponseMessage>(new RequestMessage(messageId));
            _logger.LogInformation("Received ResponseMessage: {MyId}", response.Message.MyId);
        }
    }
}
