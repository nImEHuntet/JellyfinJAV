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
            var doc = await context.OpenAsync($"https://javtrailers.com/search/{identifier}").ConfigureAwait(false);

            foreach (var n in doc.QuerySelectorAll(".card-container"))
            {
                var code = n.QuerySelector(".card-img-top").GetAttribute("alt").Replace(" jav", string.Empty);
                var id = n.QuerySelector("a").GetAttribute("href").Split('/')[2];

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
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync($"https://javtrailers.com/search/{code}");

            var h3Element = doc.QuerySelector("h3");
            var firstEntry = doc.QuerySelector(".card-container");

            if (firstEntry != null)
            {
                return await LoadVideo(new Uri("https://javtrailers.com" + firstEntry.QuerySelector("a").GetAttribute("href"))).ConfigureAwait(false);

                // var test = await LoadVideo(firstEntry.QuerySelector("a").GetAttribute("href"));
                // return new Video(code: test.ToString(), id: "It should load the video to parse", title: firstEntry.QuerySelector("a").GetAttribute("href"), actresses: new List<string>(), genres: new List<string>(), studio: string.Empty, boxArt: string.Empty, cover: string.Empty, releaseDate: null);
            }
            else if (h3Element != null && h3Element.TextContent.Contains("No videos"))
            {
                return null;

                // return new Video(id: "No Videos", code: doc.ToHtml(), title: string.Empty, actresses: new List<string>(), genres: new List<string>(), studio: string.Empty, boxArt: string.Empty, cover: string.Empty, releaseDate: null);
            }
            else
            {
                return null;
            }
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
                var doc = await context.OpenAsync($"https://javtrailers.com{id}").ConfigureAwait(false);
                return ParseVideoPage(doc);
            }

            var findContentID = await context.OpenAsync($"https://javtrailers.com/search/{id}").ConfigureAwait(false);
            var contentURL = findContentID.QuerySelector(".card-container").QuerySelector("a").GetAttribute("href").Trim();

            if (contentURL != null)
            {
                return await LoadVideo(new Uri($"https://javtrailers.com{contentURL}")).ConfigureAwait(false);
            }

            return null;
        }

        private static Video? ParseVideoPage(IDocument doc)
        {
            static (string, string) SeparateEnglishJapaneseName(string fullName)
            {
                string pattern = @"([\p{IsBasicLatin}\s]+)\s+([\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]+)";

                // Use regex to match the pattern
                Match match = Regex.Match(fullName, pattern);

                // Construct the separated name
                if (match.Success)
                {
                    string englishName = match.Groups[1].Value.Trim();
                    string japaneseName = match.Groups[2].Value.Trim();
                    return (englishName, japaneseName);
                }
                else
                {
                    return (fullName, string.Empty);
                }
        }

            string? id = doc.QuerySelector("p:contains('Content ID:')")?.TextContent?.Trim()?.Replace("Content ID:", string.Empty).Trim();
            string? code = doc.QuerySelector("p:contains('DVD ID:')")?.TextContent?.Trim()?.Replace("DVD ID:", string.Empty).Trim();
            string? title = doc.QuerySelector("h1.lead")?.TextContent?.Replace(code!, string.Empty).Trim();
            var studio = doc.QuerySelector("p:contains('Studio:')")?.QuerySelector("a")?.TextContent?.Trim();
            var boxArt = doc.QuerySelector("#thumbnailContainer img")?.GetAttribute("src");
            var cover = boxArt?.Replace("ps.jpg", "pl.jpg");
            var genres = doc.QuerySelectorAll("p:contains('Categories:') a").Select(a => a.TextContent.Trim()).ToList();
            var actresses = doc.QuerySelectorAll("p:contains('Cast(s):') a").Select(a => SeparateEnglishJapaneseName(a.TextContent.Trim())).SelectMany(tuple => new[] { tuple.Item1, tuple.Item2 });
            var releaseDateText = doc.QuerySelector("p:contains('Release Date:')")?.TextContent?.Trim()?.Replace("Release Date:", string.Empty).Trim();
            var releaseDate = DateTime.TryParseExact(releaseDateText, "dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate) ? parsedDate : (DateTime?)null;

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