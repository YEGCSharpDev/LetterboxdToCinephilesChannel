using System;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using Microsoft.Data.Sqlite;
using HtmlAgilityPack;

namespace LetterboxdToCinephilesChannel
{
    /// <summary>
    /// Reads RSS feeds, processes movie information, and sends it to a Telegram channel.
    /// </summary>
    internal class RSSReader
    {
        private static string databasePath = "entries.db";
        private static string tableName = "LatestEntries";
        private ChannelCalls calls = new ChannelCalls();
        private GetMovieInfo movieinfo = new GetMovieInfo();

        /// <summary>
        /// Executes the RSS reader process continuously.
        /// </summary>
        internal void Execute()
        {
            InitializeDatabase();

            while (true)
            {
                string Token = Environment.GetEnvironmentVariable("CINEPHILE_TOKEN");
                string ChatId = Environment.GetEnvironmentVariable("CHAT_ID");
                string rss = Environment.GetEnvironmentVariable("RSS_URLS");
                string[] urls = rss.Split(',');

                foreach (string url in urls)
                {
                    string xmlContent = DownloadXmlContent(url);
                    if (xmlContent != null)
                    {
                        TextMessage parsedData = ParseXml(xmlContent);
                        string textExtract = $"{parsedData.FilmTitle}{parsedData.FilmYear}{parsedData.MemberRating}{parsedData.TotalRating}{parsedData.ImgSrc}{parsedData.Review}";

                        if (!EntryExists(textExtract) && !string.IsNullOrWhiteSpace(textExtract))
                        {
                            var movieInfo = movieinfo.GetInfoAsync(parsedData);
                            calls.SendPhotoAsync(parsedData, movieInfo.Result);
                            InsertEntry(textExtract);
                            Console.WriteLine(textExtract);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to download XML from {url}");
                    }
                }

                System.Threading.Thread.Sleep(10 * 60 * 1000); // Sleep for 10 minutes
            }
        }

        /// <summary>
        /// Initializes the SQLite database and creates the necessary table if it doesn't exist.
        /// </summary>
        private static void InitializeDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand($"CREATE TABLE IF NOT EXISTS {tableName} (EntryContent TEXT PRIMARY KEY);", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Inserts a new entry into the database.
        /// </summary>
        /// <param name="entryContent">The content of the entry to be inserted.</param>
        private static void InsertEntry(string entryContent)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand($"INSERT INTO {tableName} (EntryContent) VALUES (@entryContent);", connection))
                {
                    command.Parameters.AddWithValue("@entryContent", entryContent);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Checks if an entry already exists in the database.
        /// </summary>
        /// <param name="entryContent">The content of the entry to check.</param>
        /// <returns>True if the entry exists, false otherwise.</returns>
        private static bool EntryExists(string entryContent)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand($"SELECT COUNT(*) FROM {tableName} WHERE EntryContent = @entryContent;", connection))
                {
                    command.Parameters.AddWithValue("@entryContent", entryContent);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        /// <summary>
        /// Downloads XML content from a given URL.
        /// </summary>
        /// <param name="url">The URL to download the XML from.</param>
        /// <returns>The downloaded XML content as a string, or null if an error occurs.</returns>
        private static string DownloadXmlContent(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading XML from {url}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses XML content and extracts relevant movie information.
        /// </summary>
        /// <param name="xmlContent">The XML content to parse.</param>
        /// <returns>A TextMessage object containing the extracted movie information.</returns>
        private static TextMessage ParseXml(string xmlContent)
        {
            TextMessage message = new TextMessage();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("letterboxd", "https://letterboxd.com");
            nsManager.AddNamespace("tmdb", "https://themoviedb.org");

            XmlNode latestItem = xmlDoc.SelectSingleNode("/rss/channel/item[1]");
            if (latestItem != null)
            {
                message.FilmTitle = latestItem.SelectSingleNode(".//letterboxd:filmTitle", nsManager)?.InnerText;
                message.FilmYear = latestItem.SelectSingleNode(".//letterboxd:filmYear", nsManager)?.InnerText;
                message.MemberRating = string.IsNullOrEmpty(latestItem.SelectSingleNode(".//letterboxd:memberRating", nsManager)?.InnerText) ? string.Empty : latestItem.SelectSingleNode(".//letterboxd:memberRating", nsManager).InnerText;
                if (!string.IsNullOrWhiteSpace(message.MemberRating))
                    message.TotalRating = "5";
                message.Creator = FindCreator(latestItem.LastChild.InnerText);
                string description = latestItem.SelectSingleNode(".//description", nsManager)?.InnerText.Trim();
                string descriptionHtml = WebUtility.HtmlDecode(description);
                ExtractImgUrlAndComments(descriptionHtml, message);
            }

            return message;
        }

        /// <summary>
        /// Extracts image URL and comments from the HTML description.
        /// </summary>
        /// <param name="descriptionHtml">The HTML description to parse.</param>
        /// <param name="message">The TextMessage object to update with extracted information.</param>
        private static void ExtractImgUrlAndComments(string descriptionHtml, TextMessage message)
        {
            if (!string.IsNullOrWhiteSpace(descriptionHtml))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(descriptionHtml);
                message.ImgSrc = doc.DocumentNode.SelectSingleNode("//img")?.Attributes["src"]?.Value;

                var lastPNode = doc.DocumentNode.SelectSingleNode("//p[last()]");
                if (lastPNode != null && !lastPNode.InnerText.Trim().StartsWith("Watched on", StringComparison.OrdinalIgnoreCase))
                {
                    message.Review = lastPNode.InnerText.Trim();
                }
                else
                {
                    message.Review = string.Empty;
                }
            }
        }

        /// <summary>
        /// Finds the creator's name based on the username using environment variable mapping.
        /// </summary>
        /// <param name="username">The username to look up.</param>
        /// <returns>The creator's name if found, or a default value if not found.</returns>
        private static string FindCreator(string username)
        {
            string defaultCreator = "Who?";
            string usernameMappingEnv = Environment.GetEnvironmentVariable("USERNAME_CREATOR_MAPPING");
            Dictionary<string, string> usernameToCreatorMapping = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(usernameMappingEnv))
            {
                string[] mappingPairs = usernameMappingEnv.Split(',');
                foreach (string pair in mappingPairs)
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        usernameToCreatorMapping[keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                }
            }

            return usernameToCreatorMapping.ContainsKey(username)
                ? usernameToCreatorMapping[username]
                : defaultCreator;
        }
    }
}