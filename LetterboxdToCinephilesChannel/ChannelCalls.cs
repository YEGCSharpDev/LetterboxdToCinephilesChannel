using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static LetterboxdToCinephilesChannel.GetMovieInfo;

namespace LetterboxdToCinephilesChannel
{
    /// <summary>
    /// Handles sending messages to a Telegram channel.
    /// </summary>
    internal class ChannelCalls
    {
        /// <summary>
        /// Sends a photo to a Telegram channel with the provided parsed data and movie information.
        /// Implements rate limiting and retries in case of failure.
        /// </summary>
        /// <param name="parsedData">The parsed data from the XML feed.</param>
        /// <param name="info">The movie information obtained from the API.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task SendPhotoAsync(TextMessage parsedData, MovieInfo info)
        {
            try
            {
                string Token = Environment.GetEnvironmentVariable("CINEPHILE_TOKEN");
                string ChatId = Environment.GetEnvironmentVariable("CHAT_ID");

                if (string.IsNullOrEmpty(Token))
                {
                    Console.WriteLine("Telegram bot token is missing. Set the TELEGRAM_BOT_TOKEN environment variable.");
                    return;
                }

                string finalcaption = PrepCaption(parsedData, info);

                TelegramBotClient botClient = new TelegramBotClient(Token);

                ReceiverOptions receiverOptions = new()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };

                using CancellationTokenSource cts = new();

                const int maxRetries = 3;
                int retries = 0;

                while (retries < maxRetries)
                {
                    try
                    {
                        Message message = await botClient.SendPhotoAsync(
                            chatId: ChatId,
                            photo: InputFile.FromUri(parsedData.ImgSrc),
                            caption: finalcaption,
                            parseMode: ParseMode.MarkdownV2,
                            cancellationToken: cts.Token);

                        Console.WriteLine("Message sent successfully.");
                        break; // Exit the loop if the message is sent successfully
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Attempt {retries + 1} failed: {ex.Message}");
                        retries++;

                        if (retries < maxRetries)
                        {
                            int delayTime = ParseRetryAfterTime(ex.Message);
                            Console.WriteLine($"Waiting {delayTime} seconds before retrying...");
                            await Task.Delay(delayTime * 1000); // Convert seconds to milliseconds
                        }
                        else
                        {
                            Console.WriteLine("Max retries reached. Moving on to the next message.");
                            break; // Exit the loop after max retries
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendPhotoAsync: {ex.Message}");
                // Log the error or perform any necessary error handling
            }
        }

        /// <summary>
        /// Parses the retry-after time from an error message.
        /// </summary>
        /// <param name="errorMessage">The error message to parse.</param>
        /// <returns>The number of seconds to wait before retrying, or a default value of 10 seconds.</returns>
        private int ParseRetryAfterTime(string errorMessage)
        {
            const int defaultDelay = 10; // Default delay in seconds

            try
            {
                if (errorMessage.Contains("Too Many Requests: retry after"))
                {
                    string[] parts = errorMessage.Split("retry after");
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int waitTime))
                    {
                        return waitTime + 1; // Add 1 second plus the error message specification
                    }
                }
            }
            catch
            {
                // If any error occurs during parsing, return the default delay
            }

            return defaultDelay;
        }

        /// <summary>
        /// Escapes special characters in a string for Markdown formatting.
        /// </summary>
        /// <param name="input">The input string to escape.</param>
        /// <returns>The escaped string.</returns>
        private string EscapeForMarkdown(string input)
        {
            // List of special characters in Markdown that need to be escaped
            string[] specialCharacters = { "_", "*", "`", "[", "]", "(", ")", "{", "}", "#", "+", "-", ".", "!", ":", "/" };

            // Escape each special character with a backslash
            foreach (var character in specialCharacters)
            {
                input = input.Replace(character, "\\" + character);
            }

            return input;
        }

        /// <summary>
        /// Prepares genre information by adding hashtags to each genre.
        /// </summary>
        /// <param name="input">The input genre string.</param>
        /// <returns>A string with hashtags added to each genre.</returns>
        private string PrepGenre(string input)
        {
            // split genre into a string array
            string[] genreSplit = input.Split(',');
            string preppedGenre = string.Empty;

            // add hashtag to each of them
            foreach (var character in genreSplit)
            {
                preppedGenre = preppedGenre + " " + "#" + (character).Trim();
            }

            return preppedGenre;
        }

        /// <summary>
        /// Prepares the caption for the Telegram message.
        /// </summary>
        /// <param name="message">The parsed message data.</param>
        /// <param name="movie">The movie information.</param>
        /// <returns>A formatted string to be used as the message caption.</returns>
        private string PrepCaption(TextMessage message, MovieInfo movie)
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