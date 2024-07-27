using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LetterboxdToCinephilesChannel
{
    /// <summary>
    /// Handles retrieving movie information from an external API.
    /// </summary>
    internal class GetMovieInfo
    {
        /// <summary>
        /// Asynchronously retrieves movie information from an external API.
        /// </summary>
        /// <param name="movie">The movie data to query.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the movie information.</returns>
        public async Task<MovieInfo> GetInfoAsync(TextMessage movie)
        {
            int maxRetries = 3;
            int currentRetry = 0;
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");
            string apiURI = $"http://www.omdbapi.com/?apikey={apiKey}&t={Uri.EscapeDataString(movie.FilmTitle)}&y={movie.FilmYear}&r=JSON";

            using (HttpClient httpClient = new HttpClient())
            {
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(apiURI);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var info = JsonConvert.DeserializeObject<MovieInfo>(jsonResponse);

                            if (info != null && !string.IsNullOrWhiteSpace(info.Title))
                            {
                                Console.WriteLine(jsonResponse);
                                return ReplaceNullWithNA(info);
                            }
                            else
                            {
                                Console.WriteLine($"Attempt {currentRetry + 1} failed. API returned null or empty response.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Attempt {currentRetry + 1} failed. Status code: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Attempt {currentRetry + 1} failed. Error: {ex.Message}");
                    }

                    currentRetry++;
                    if (currentRetry < maxRetries)
                    {
                        Console.WriteLine($"Waiting 30 seconds before retry...");
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }

                Console.WriteLine("All retry attempts failed. Returning placeholder MovieInfo.");
                return CreatePlaceholderMovieInfo(movie.FilmTitle, movie.FilmYear);
            }
        }

        /// <summary>
        /// Creates a placeholder MovieInfo object with default values.
        /// </summary>
        /// <param name="filmTitle">The title of the film.</param>
        /// <param name="filmYear">The year of the film.</param>
        /// <returns>A MovieInfo object with placeholder data.</returns>
        private MovieInfo CreatePlaceholderMovieInfo(string filmTitle, string filmYear)
        {
            return new MovieInfo
            {
                Title = filmTitle,
                Year = filmYear,
                Rated = "N/A",
                Released = "N/A",
                Runtime = "N/A",
                Genre = "N/A",
                Director = "N/A",
                Writer = "N/A",
                Actors = "N/A",
                Plot = "Information not available",
                Language = "N/A",
                Country = "N/A",
                Awards = "N/A",
                Poster = "N/A",
                Metascore = "N/A",
                imdbRating = "N/A",
                imdbVotes = "N/A",
                imdbID = "N/A"
            };
        }

        /// <summary>
        /// Replaces null values in a MovieInfo object with "N/A".
        /// </summary>
        /// <param name="info">The MovieInfo object to process.</param>
        /// <returns>The MovieInfo object with null values replaced by "N/A".</returns>
        private MovieInfo ReplaceNullWithNA(MovieInfo info)
        {
            info.Title = info.Title ?? "N/A";
            info.Year = info.Year ?? "N/A";
            info.Rated = info.Rated ?? "N/A";
            info.Released = info.Released ?? "N/A";
            info.Runtime = info.Runtime ?? "N/A";
            info.Genre = info.Genre ?? "N/A";
            info.Director = info.Director ?? "N/A";
            info.Writer = info.Writer ?? "N/A";
            info.Actors = info.Actors ?? "N/A";
            info.Plot = info.Plot ?? "N/A";
            info.Language = info.Language ?? "N/A";
            info.Country = info.Country ?? "N/A";
            info.Awards = info.Awards ?? "N/A";
            info.Poster = info.Poster ?? "N/A";
            info.Metascore = info.Metascore ?? "N/A";
            info.imdbRating = info.imdbRating ?? "N/A";
            info.imdbVotes = info.imdbVotes ?? "N/A";
            info.imdbID = info.imdbID ?? "N/A";

            return info;
        }

        /// <summary>
        /// Represents detailed information about a movie.
        /// </summary>
        internal class MovieInfo
        {
            /// <summary>
            /// Gets or sets the title of the movie.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the year of the movie's release.
            /// </summary>
            public string Year { get; set; }

            /// <summary>
            /// Gets or sets the movie's rating (e.g., PG, R).
            /// </summary>
            public string Rated { get; set; }

            /// <summary>
            /// Gets or sets the movie's release date.
            /// </summary>
            public string Released { get; set; }

            /// <summary>
            /// Gets or sets the movie's runtime.
            /// </summary>
            public string Runtime { get; set; }

            /// <summary>
            /// Gets or sets the movie's genre(s).
            /// </summary>
            public string Genre { get; set; }

            /// <summary>
            /// Gets or sets the movie's director(s).
            /// </summary>
            public string Director { get; set; }

            /// <summary>
            /// Gets or sets the movie's writer(s).
            /// </summary>
            public string Writer { get; set; }

            /// <summary>
            /// Gets or sets the movie's main actors.
            /// </summary>
            public string Actors { get; set; }

            /// <summary>
            /// Gets or sets the movie's plot summary.
            /// </summary>
            public string Plot { get; set; }

            /// <summary>
            /// Gets or sets the movie's language(s).
            /// </summary>
            public string Language { get; set; }

            /// <summary>
            /// Gets or sets the movie's country of origin.
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// Gets or sets the awards received by the movie.
            /// </summary>
            public string Awards { get; set; }

            /// <summary>
            /// Gets or sets the URL of the movie's poster.
            /// </summary>
            public string Poster { get; set; }

            /// <summary>
            /// Gets or sets the movie's Metascore rating.
            /// </summary>
            public string Metascore { get; set; }

            /// <summary>
            /// Gets or sets the movie's IMDb rating.
            /// </summary>
            public string imdbRating { get; set; }

            /// <summary>
            /// Gets or sets the number of IMDb votes for the movie.
            /// </summary>
            public string imdbVotes { get; set; }

            /// <summary>
            /// Gets or sets the movie's IMDb ID.
            /// </summary>
            public string imdbID { get; set; }
        }
    }
}