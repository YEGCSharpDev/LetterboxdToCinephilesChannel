using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LetterboxdToCinephilesChannel
{
    internal class ChannelCalls
    {

        internal async void SendMessageAsync(string message)
        {
        TelegramBotClient botClient;
        string Token = Environment.GetEnvironmentVariable("CINEPHILE_TOKEN");
        string ChatId = Environment.GetEnvironmentVariable("CHAT_ID");

        if (string.IsNullOrEmpty(Token))
        {
            Console.WriteLine("Telegram bot token is missing. Set the TELEGRAM_BOT_TOKEN environment variable.");
            return;
        }

        botClient = new TelegramBotClient(Token);

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        using CancellationTokenSource cts = new();

        Message sentMessage = await botClient.SendTextMessageAsync(ChatId,message,cancellationToken: cts.Token);
        }

        internal async void SendPhotoAsync(TextMessage parsedData)
        {
            TelegramBotClient botClient;
            string Token = Environment.GetEnvironmentVariable("CINEPHILE_TOKEN");
            string ChatId = Environment.GetEnvironmentVariable("CHAT_ID");

            if (string.IsNullOrEmpty(Token))
            {
                Console.WriteLine("Telegram bot token is missing. Set the TELEGRAM_BOT_TOKEN environment variable.");
                return;
            }
            string caption = $"**{parsedData.FilmTitle} : {parsedData.FilmYear}**  \n{parsedData.MemberRating}  \n{parsedData.Review}";
            botClient = new TelegramBotClient(Token);

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            using CancellationTokenSource cts = new();

            //Message sentMessage = await botClient.SendPhotoAsync(ChatId, message, cancellationToken: cts.Token);

            Message message = await botClient.SendPhotoAsync(
            chatId: ChatId,
            photo: InputFile.FromUri(parsedData.ImgSrc),
            caption: caption,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cts.Token);
        }

    }
}
