using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ShutdownReproApp
{
    public class SlowConsumer: IConsumer<RequestMessage>
    {
        private readonly ILogger<SlowConsumer> _logger;

        public SlowConsumer(ILogger<SlowConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RequestMessage> context)
        {
            _logger.LogInformation("Consumed RequestMessage. Sleeping for 5 seconds.");
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("Sending ResponseMessage");
            try
            {
                await context.RespondAsync(new ResponseMessage(context.Message.MyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing response");
                throw;
            }
        }
    }
}
