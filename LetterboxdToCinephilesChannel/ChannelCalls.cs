using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LetterboxdToCinephilesChannel
{
    internal class ChannelCalls
    {
        /// <summary>
        /// Generic bot message 
        /// </summary>
        /// <param name="parsedData"></param>
        /// <param name="info"></param>
        internal async void SendPhotoAsync(TextMessage parsedData, MovieInfo info)
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
                string finalcaption = PrepCaption(parsedData, info);

                botClient = new TelegramBotClient(Token);

                ReceiverOptions receiverOptions = new()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };

                using CancellationTokenSource cts = new();

                Message message = await botClient.SendPhotoAsync(
                chatId: ChatId,
                photo: InputFile.FromUri(parsedData.ImgSrc),
                caption: finalcaption,
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
            string[] specialCharacters = { "_", "*", "`", "[", "]", "(", ")", "{", "}", "#", "+", "-", ".", "!", ":","/" };

            // Escape each special character with a backslash
            foreach (var character in specialCharacters)
            {
                input = input.Replace(character, "\\" + character);
            }

            return input;
        }

        string PrepGenre(string input)
        {
            // split genre into a string array
            string[] genreSplit = input.Split(',');
            string preppedGenre = string.Empty;

            // add hastag to each of them
            foreach (var character in genreSplit)
            {
                preppedGenre = preppedGenre + " " + "#" + (character).Trim();
                
            }

            return preppedGenre;
        }

        string PrepCaption(TextMessage message, MovieInfo movie)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"**Title**\\: {EscapeForMarkdown(message.FilmTitle)}\\({EscapeForMarkdown(message.FilmYear)}\\)\n");
            stringBuilder.Append($"**Language**\\: {EscapeForMarkdown(movie.Language)}\n");
            stringBuilder.Append($"**IMDB Rating**\\: {EscapeForMarkdown(movie.imdbRating)}\n");
            stringBuilder.Append($"**IMdb URL**\\: {EscapeForMarkdown("https://www.imdb.com/title/")}{movie.imdbID} \n");
            stringBuilder.Append($"**Genre**\\: {EscapeForMarkdown(PrepGenre(movie.Genre))} \n");

            stringBuilder.Append($"**Story Line**\\: {EscapeForMarkdown(movie.Plot)}\n");
            if (movie.Awards != "N/A")
            {
                stringBuilder.Append($"**Awards**\\: {EscapeForMarkdown(movie.Awards)}\n");
            }
            if (!string.IsNullOrEmpty(message.MemberRating))
            {
                stringBuilder.Append($"**{EscapeForMarkdown(message.Creator)}\\'s Rating**\\: {EscapeForMarkdown(message.MemberRating)}\\/{EscapeForMarkdown(message.TotalRating)} \n");

            }
            if (!string.IsNullOrEmpty(message.Review))
            {
                stringBuilder.Append($"**Review**\\: {EscapeForMarkdown(message.Review)}\n\n");

            }

            stringBuilder.Append($"\\- `{EscapeForMarkdown(message.Creator)}`");

            return stringBuilder.ToString();
        }

    }
}
