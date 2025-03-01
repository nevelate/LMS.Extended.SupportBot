using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace LMS.Extended.SupportBot.Services
{
    public class UpdateHandler(ITelegramBotClient bot, IOptions<BotConfiguration> options, ILogger<UpdateHandler> logger) : IUpdateHandler
    {
        private readonly long adminId = options.Value.AdminId;
        private ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup("LMS Extended on Google Play", "About") { ResizeKeyboard = true };

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            logger.LogInformation("HandleError: {Exception}", exception);
            // Cooldown in case of network connection error
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await (update switch
            {
                { Message: { } message } => OnMessage(message),
                _ => UnknownUpdateHandlerAsync(update)
            });
        }

        private async Task OnMessage(Message msg)
        {
            logger.LogInformation("Receive message type: {MessageType}", msg.Type);
            if (msg.Text is not { } messageText)
                return;

            await (messageText switch
            {
                "/start" => SendStartMessage(msg),
                "LMS Extended on Google Play" => bot.SendMessage(msg.Chat, "https://play.google.com/store/apps/details?id=com.nevelate.lmse", replyMarkup: keyboard),
                "About" => SendAbout(msg),
                _ => SendMessage(msg)
            });
        }

        async Task SendMessage(Message msg)
        {
            if (msg.Chat.Id == adminId)
            {
                if (msg.ReplyToMessage != null && msg.ReplyToMessage.Chat.Id != adminId)
                    await bot.CopyMessage(msg.ReplyToMessage.ForwardFromChat.Id, adminId, msg.Id);
            }
            else
            {
                await bot.ForwardMessage(adminId, msg.Chat.Id, msg.Id);
                await bot.SendMessage(msg.Chat, "Your message has been sent to the administrator. You will receive a response soon.", replyMarkup: keyboard);
            }
        }

        async Task SendStartMessage(Message msg)
        {
            if (msg.Chat.Id == adminId) await bot.SendMessage(msg.Chat, "Welcome admin!", replyMarkup: keyboard);
            else await bot.SendMessage(msg.Chat, "Welcome to **LMS Extended Support Bot**\\! Official support bot of [LMS Extended](https://play.google.com/store/apps/details?id=com.nevelate.lmse)\\. Here you can send suggestions, questions and bug reports", parseMode: ParseMode.MarkdownV2, replyMarkup: keyboard);
        }

        async Task SendAbout(Message msg)
        {
            const string about = """
              <b>About:</b>
              LMS Extended Support Bot by <a href="https://github.com/nevelate">nevelate</a>
              Source code - <a href="https://github.com/nevelate/LMS.Extended.SupportBot">LMS.Extended.SupportBot</a>
            """;
            await bot.SendMessage(msg.Chat, about, parseMode: ParseMode.Html, replyMarkup: keyboard);
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }
    }
}
