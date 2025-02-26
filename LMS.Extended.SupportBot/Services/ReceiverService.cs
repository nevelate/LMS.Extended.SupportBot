using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace LMS.Extended.SupportBot.Services
{
    public class ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler, ILogger<ReceiverService> logger) : IReceiverService
    {
        public async Task ReceiveAsync(CancellationToken stoppingToken)
        {
            var receiverOptions = new ReceiverOptions() { DropPendingUpdates = true, AllowedUpdates = [] };

            var me = await botClient.GetMe(stoppingToken);
            logger.LogInformation("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

            // Start receiving updates
            await botClient.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
        }
    }
}
