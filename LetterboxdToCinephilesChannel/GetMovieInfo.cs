using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LetterboxdToCinephilesChannel
{
    internal class GetMovieInfo
    {
        public async Task<MovieInfo> GetInfoAsync (TextMessage movie)
        {
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");

            string apiURI = $"http://www.omdbapi.com/?apikey={apiKey}&t={Uri.EscapeDataString(movie.FilmTitle)}&y={movie.FilmYear}&r=JSON";

            MovieInfo info = new MovieInfo ();

            HttpClient httpClient = new HttpClient ();

            var test = await httpClient.GetAsync(apiURI);

            string jsonResponse = await test.Content.ReadAsStringAsync();

            var json = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);

            info = JsonConvert.DeserializeObject<MovieInfo>(jsonResponse);

            Console.WriteLine(jsonResponse);

            return info;
        }

    }

    internal class MovieInfo
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }

        public string Genre { get; set; }
        public string Director { get; set; }
        public string Writer { get; set; }
        public string Actors { get; set; }
        public string Plot { get; set; }
        public string Language { get; set; }
        public string Country { get; set; }
        public string Awards { get; set; }

        public string Poster { get; set; }
        public string Metascore { get; set; }
        public string imdbRating { get; set; }
        public string imdbVotes { get; set; }

        public string imdbID { get; set; }


    }
}
