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
    using Newtonsoft.Json;

    /// <summary>A web scraping client for r18.com.</summary>
    public static class R18Client
    {
        private static readonly IDictionary<string, string> CensoredWords = new Dictionary<string, string>
        {
            { "S***e", "Slave" },
            { "S*********l", "School Girl" },
            { "S********l", "Schoolgirl" },
            { "Sch**l", "School" },
            { "F***e", "Force" },
            { "F*****g", "Forcing" },
            { "P****h", "Punish" },
            { "M****t", "Molest" },
            { "S*****t", "Student" },
            { "T*****e", "Torture" },
            { "D**g", "Drug" },
            { "H*******e", "Hypnotize" },
            { "C***d", "Child" },
            { "V*****e", "Violate" },
            { "Y********l", "Young Girl" },
            { "A*****t", "Assault" },
            { "D***king", "Drinking" },
            { "D***k", "Drunk" },
            { "V*****t", "Violent" },
            { "S******g", "Sleeping" },
            { "R**e", "Rape" },
            { "R****g", "Raping" },
            { "S**t", "Scat" },
            { "K****r", "Killer" },
            { "H*******m", "Hypnotism" },
            { "G*******g", "Gangbang" },
            { "C*ck", "Cock" },
            { "K*ds", "Kids" },
            { "K****p", "Kidnap" },
            { "A****p", "Asleep" },
            { "U*********s", "Unconscious" },
            { "D******e", "Disgrace" },
            { "P********t", "Passed Out" },
            { "M************n", "Mother And Son" },
        };

        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly IBrowsingContext Context = BrowsingContext.New();

        /// <summary>Searches for a video by jav code.</summary>
        /// <param name="searchCode">The jav code. Ex: ABP-001.</param>
        /// <returns>A list of every matched video.</returns>
        public static async Task<IEnumerable<VideoResult>>? Search(string searchCode)
        {
            HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            HttpClient.DefaultRequestHeaders.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("en-US"));
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.164 Safari/537.36");
            HttpClient.DefaultRequestHeaders.Host = "r18.dev";
            var videos = new List<VideoResult>();
            var jsonResponse = await HttpClient.GetAsync($"https://r18.dev/videos/vod/movies/detail/-/dvd_id={searchCode}/json");
            string? contentId;

            if (jsonResponse.IsSuccessStatusCode)
            {
                string json = await jsonResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
#pragma warning disable CS8600
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
#pragma warning restore CS8600
                contentId = jsonObj!["content_id"];
                string large2Url = jsonObj!["images"]["jacket_image"]["large2"];
                videos.Add(new VideoResult
                {
                    Code = searchCode.ToUpper(),
                    Id = contentId,
                    Cover = new Uri(large2Url),
                });

                return videos;
            }
            else
            {
                return null;
            }
        }

        /// <summary>Searches for a video by jav code, and returns the first result.</summary>
        /// <param name="searchCode">The jav code. Ex: ABP-001.</param>
        /// <returns>The parsed video.</returns>
        public static async Task<Video?> SearchFirst(string searchCode)
        {
            var results = await Search(searchCode)!.ConfigureAwait(false);

            if (results.Any())
            {
                return await LoadVideo(results.First().Id).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>Loads a video by id.</summary>
        /// <param name="id">The r18.dev unique video identifier.</param>
        /// <returns>The parsed video.</returns>
        public static async Task<Video?> LoadVideo(string id)
        {
            var response = await HttpClient.GetAsync($"https://r18.dev/videos/vod/movies/detail/-/id={id}/").ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string? html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var doc = await Context.OpenAsync(req => req.Content(html)).ConfigureAwait(false);

            string? code = doc.QuerySelector("#dvd-id")?.TextContent.Trim();
            string? title = (doc.QuerySelector("#title")?.TextContent.Trim()?.Substring(doc.QuerySelector("#dvd-id")?.TextContent.Trim()?.Length ?? 0).TrimStart(':', ' ') ?? string.Empty).Trim();
            var actresses = doc.QuerySelectorAll(".performer")
                                   ?.Select(n => n.TextContent.Trim()).ToArray()
                                    ?? Array.Empty<string>();
            var genres = doc.QuerySelectorAll(".category")
                    ?.SelectMany(n => n.QuerySelectorAll("a"))
                    .Select(a => a.TextContent.Trim())
                    .Where(genre => NotSaleGenre(genre))
                    .ToArray() ?? Array.Empty<string>();
            string? studio = doc.QuerySelector("#studio")?.TextContent.Trim();
            string? cover = doc.QuerySelector("#jacket")?.GetAttribute("src");
            string? boxArt = cover?.Replace("pl.jpg", "ps.jpg");
            string dateString = doc!.QuerySelector("#release-date")!.TextContent;
            DateTime releaseDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", null);

            if (title is null || code is null)
            {
                return null;
            }

            title = NormalizeTitle(title, actresses);

            return new Video(
                    id: id,
                    code: code,
                    title: title,
                    actresses: actresses,
                    genres: genres,
                    studio: studio,
                    boxArt: boxArt,
                    cover: cover,
                    releaseDate: releaseDate);
        }

        private static string NormalizeActress(string actress)
        {
            var rx = new Regex(@"^(.+?)( ?\(.+\))?$");
            var match = rx.Match(actress);

            if (!match.Success)
            {
                return actress;
            }

            return match.Groups[1].Value;
        }

        private static string NormalizeTitle(string title, IEnumerable<string> actresses)
        {
            if (actresses.Count() != 1)
            {
                return title;
            }

            string? name = actresses.ElementAt(0);
            var rx = new Regex($"^({name} - )?(.+?)( ?-? {name})?$");
            var match = rx.Match(title);

            if (!match.Success)
            {
                return title;
            }

            return match.Groups[2].Value;
        }

        private static bool NotSaleGenre(string? genre)
        {
            var rx = new Regex(@"\bsale\b", RegexOptions.IgnoreCase);
            var match = rx.Match(genre ?? string.Empty);

            return !match.Success;
        }
    }
}