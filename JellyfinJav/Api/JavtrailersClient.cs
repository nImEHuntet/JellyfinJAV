namespace JellyfinJav.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection.Metadata;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using MediaBrowser.Controller.Entities;
    using Microsoft.Extensions.Logging;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>A web scraping client for Javtrailers.com.</summary>
    public static class JavtrailersClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly IBrowsingContext Context = BrowsingContext.New();

        /// <summary>Searches by the specified identifier.</summary>
        /// <param name="identifier">The identifier to search for.</param>
        /// <returns>An array of tuples representing each video returned during the search.</returns>
        public static async Task<IEnumerable<VideoResult>> Search(string identifier)
        {
            var videos = new List<VideoResult>();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync($"https://www.av01.tv/en/search/videos?search_query={identifier}").ConfigureAwait(false);

            foreach (var n in doc.QuerySelectorAll(".card-container"))
            {
                var code = n?.QuerySelector(".card-img-top")?.GetAttribute("alt")?.Replace(" jav", string.Empty);
                var id = n?.QuerySelector("a")?.GetAttribute("href")?.Split('/')[2];

                if (code is not null && id is not null)
                {
                    videos.Add(new VideoResult
                    {
                        Code = code,
                        Id = id,
                    });
                }
            }

            return videos;
        }

        /// <summary>Loads a specific JAV by url.</summary>
        /// <param name="url">The JAV url.</param>
        /// <returns>The parsed video, or null if no video at <c>url</c> exists.</returns>
        public static async Task<Video?> LoadVideo(Uri url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url.ToString()).ConfigureAwait(false);
            return ParseVideoPage(doc);
        }

        /// <summary>Searches for a specific JAV code, and returns the first result.</summary>
        /// <param name="code">The JAV to search for.</param>
        /// <returns>The first result of the search, or null if nothing was found.</returns>
        public static async Task<Video?> SearchFirst(string code)
        {
           var santizedSearch = code.Replace("-", string.Empty);
           var config = Configuration.Default.WithDefaultLoader();
           var context = BrowsingContext.New(config);
           var doc = await context.OpenAsync($"https://sextb.net/search/{santizedSearch}");
           var h3Element = doc.QuerySelector("h3");
           var matchedEntry = string.Empty;

           foreach (var n in doc.QuerySelectorAll(".well.well-sm "))
            {
                var id = n.QuerySelector("a")?.GetAttribute("href");
                if (id!.Contains(santizedSearch))
                {
                    matchedEntry = id;
                }
            }

           if (h3Element != null && h3Element.TextContent.Contains("No videos"))
            {
                // return null;

                return new Video(id: $"I'm Looking for: {code} {santizedSearch}", code: $"But I got mE: {matchedEntry!} h3: {h3Element.TextContent}", title: doc.ToHtml(), actresses: new List<string>(), genres: new List<string>(), studio: string.Empty, boxArt: string.Empty, cover: string.Empty, releaseDate: null);
            }

           if (matchedEntry.Contains("/video/"))
            {
                // return await LoadVideo(new Uri(https://www.av01.tv" + matchedEntry)).ConfigureAwait(false);

                return new Video(id: $"I'm Looking for: {code} {santizedSearch}", code: $"But I got {matchedEntry!} {h3Element}", title: doc.ToHtml(), actresses: new List<string>(), genres: new List<string>(), studio: string.Empty, boxArt: string.Empty, cover: string.Empty, releaseDate: null);
            }

           return new Video(id: $"I'm Looking for: {code} {santizedSearch}", code: $"But I got {matchedEntry!} {h3Element}", title: doc.ToHtml(), actresses: new List<string>(), genres: new List<string>(), studio: string.Empty, boxArt: string.Empty, cover: string.Empty, releaseDate: null);

            // return new Video(id: $"I'm gonna Look for: {code}", code: $"But I got {matchedEntry!}", title: doc.ToHtml(), actresses: new List<string>(), genres: new List<string>(), studio: string.Empty, boxArt: string.Empty, cover: string.Empty, releaseDate: null);
        }

        /// <summary>Loads a specific JAV by id.</summary>
        /// <param name="id">The Javtrailers spcific JAV identifier.</param>
        /// <returns>The parsed video, or null if no video with <c>id</c> exists.</returns>
        public static async Task<Video?> LoadVideo(string id)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            if (id.Contains("/video/"))
            {
                var doc = await context.OpenAsync($"https://www.av01.tv{id}").ConfigureAwait(false);
                return ParseVideoPage(doc);
            }

            var findContentID = await context.OpenAsync($"https://www.av01.tv/en/search/videos?search_query={id}").ConfigureAwait(false);
            var contentURL = findContentID?.QuerySelector(".well.well-sm a[href]")?.GetAttribute("href")?.Trim();

            if (contentURL != null)
            {
                return await LoadVideo(new Uri($"https://www.av01.tv{contentURL}")).ConfigureAwait(false);
            }

            return null;
        }

        private static Video? ParseVideoPage(IDocument doc)
        {
            string? id = doc.QuerySelector("span.info_item[aria-label='ID']")?.TextContent.Trim();
            string? code = doc.GetElementById("DVD-ID")?.TextContent.Trim();
            string? title = "Test";
            var studio = "Test";
            var boxArt = "Test";
            var cover = boxArt?.Replace("ps.jpg", "pl.jpg");
            var genres = new List<string> { "apple", "banana", "orange", "grape", "kiwi" };
            var actresses = new List<string> { "apple", "banana", "orange", "grape", "kiwi" };
            var releaseDate = DateTime.Now;

            return new Api.Video(
                id: id!,
                code: code!,
                title: title!,
                actresses: actresses,
                genres: genres,
                studio: studio,
                boxArt: boxArt,
                cover: cover,
                releaseDate: releaseDate); // TODO
        }

        private static string ReverseName(in string name)
        {
            return string.Join(" ", name.Split(' ').Reverse());
        }
    }
}