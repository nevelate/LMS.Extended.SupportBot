using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Extended.SupportBot.Services
{
    public class PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting polling service");
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            // Make sure we receive updates until Cancellation Requested
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create new IServiceScope on each iteration. This way we can leverage benefits
                    // of Scoped TReceiverService and typed HttpClient - we'll grab "fresh" instance each time
                    using var scope = serviceProvider.CreateScope();
                    var receiver = scope.ServiceProvider.GetRequiredService<IReceiverService>();

                    await receiver.ReceiveAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError("Polling failed with exception: {Exception}", ex);
                    // Cooldown if something goes wrong
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
