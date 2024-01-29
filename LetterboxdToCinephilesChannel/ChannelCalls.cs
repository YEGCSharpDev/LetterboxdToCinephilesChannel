using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LetterboxdToCinephilesChannel
{
    internal class ChannelCalls
    {

        internal async void SendPhotoAsync(TextMessage parsedData)
        {

            try
            {
                TelegramBotClient botClient;
                string Token = Environment.GetEnvironmentVariable("CINEPHILE_TOKEN");
                string ChatId = Environment.GetEnvironmentVariable("CHAT_ID");

                if (string.IsNullOrEmpty(Token))
                {
                    Console.WriteLine("Telegram bot token is missing. Set the TELEGRAM_BOT_TOKEN environment variable.");
                    return;
                }
                string caption = !string.IsNullOrEmpty(parsedData.MemberRating) ?
                $"**{EscapeForMarkdown(parsedData.FilmTitle)}\\({EscapeForMarkdown(parsedData.FilmYear)}\\) ** \n{EscapeForMarkdown(parsedData.MemberRating)}\\/{EscapeForMarkdown(parsedData.TotalRating)}\n{EscapeForMarkdown(parsedData.Review)}\n \\- `{EscapeForMarkdown(parsedData.Creator)}`" : $"**{EscapeForMarkdown(parsedData.FilmTitle)}\\({EscapeForMarkdown(parsedData.FilmYear)}\\) **\n{EscapeForMarkdown(parsedData.Review)}\n \\- `{EscapeForMarkdown(parsedData.Creator)}`";

                botClient = new TelegramBotClient(Token);

                ReceiverOptions receiverOptions = new()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };

                using CancellationTokenSource cts = new();

                Message message = await botClient.SendPhotoAsync(
                chatId: ChatId,
                photo: InputFile.FromUri(parsedData.ImgSrc),
                caption: caption,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cts.Token);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        string EscapeForMarkdown(string input)
        {
            // List of special characters in Markdown that need to be escaped
            string[] specialCharacters = { "_", "*", "`", "[", "]", "(", ")", "{", "}", "#", "+", "-", ".", "!" };

            // Escape each special character with a backslash
            foreach (var character in specialCharacters)
            {
                input = input.Replace(character, "\\" + character);
            }

            return input;
        }

    }
}
