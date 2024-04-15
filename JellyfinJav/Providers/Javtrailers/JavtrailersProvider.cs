namespace JellyfinJav.Providers.JavtrailersrProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using JellyfinJav.Api;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Providers;
    using Microsoft.Extensions.Logging;

    /// <summary>The provider for Javtrailers videos.</summary>
    public class JavtrailersProvider : IRemoteMetadataProvider<Movie, MovieInfo>, IHasOrder
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly ILibraryManager libraryManager;
        private readonly ILogger<JavtrailersProvider> logger;

        /// <summary>Initializes a new instance of the <see cref="JavtrailersProvider"/> class.</summary>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager" />.</param>
        /// <param name="logger">Instance of the <see cref="ILogger" />.</param>
        public JavtrailersProvider(
            ILibraryManager libraryManager,
            ILogger<JavtrailersProvider> logger)
        {
            this.libraryManager = libraryManager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public string Name => "[Testing] Javtrailers";

        /// <inheritdoc />
        public int Order => 100;

        /// <inheritdoc />
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancelToken)
        {
            var originalTitle = Utility.GetVideoOriginalTitle(info, this.libraryManager);

            this.logger.LogInformation("[JellyfinJav] Javtrailers - Scanning: " + originalTitle);

            Api.Video? result;
            if (info.ProviderIds.ContainsKey("Javtrailers"))
            {
                result = await JavtrailersClient.LoadVideo(info.ProviderIds["Javtrailers"]).ConfigureAwait(false);
                this.logger.LogInformation("[JellyfinJav] Javtrailers - Scanning Video: " + result);
            }
            else
            {
                var code = Utility.ExtractCodeFromFilename(originalTitle);
                this.logger.LogInformation("[JellyfinJav] Javtrailers - Am Code Null?: " + code);
                if (code is null)
                {
                    this.logger.LogInformation("[JellyfinJav] Javtrailers - Yes I am");
                    return new MetadataResult<Movie>();
                }

                result = await JavtrailersClient.SearchFirst(code).ConfigureAwait(false);
            }

            if (!result.HasValue)
            {
                this.logger.LogInformation("[JellyfinJav] Javtrailers - Oh Noes!" + result);
                return new MetadataResult<Movie>();
            }

            this.logger.LogInformation("[JellyfinJav] Javtrailers - Found metadata: " + result);
            return new MetadataResult<Movie>
            {
                Item = new Movie
                {
                    OriginalTitle = originalTitle,
                    Name = Utility.CreateVideoDisplayName(result.Value),
                    ProviderIds = new Dictionary<string, string> { { "Javtrailers", result.Value.Id } },
                    Studios = new[] { result.Value.Studio },
                    Genres = result.Value.Genres.ToArray(),
                },
                People = CreateActressList(result.Value),
                HasMetadata = true,
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo info, CancellationToken cancelToken)
        {
            return from video in await JavtrailersClient.Search(info.Name).ConfigureAwait(false)
                   select new RemoteSearchResult
                   {
                       Name = video.Code,
                       ProviderIds = new Dictionary<string, string>
                       {
                           { "Javtrailers", video.Id },
                       },
                   };
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancelToken)
        {
            return await HttpClient.GetAsync(url, cancelToken).ConfigureAwait(false);
        }

        private static string NormalizeActressName(string name)
        {
            if (Plugin.Instance?.Configuration.ActressNameOrder == ActressNameOrder.FirstLast)
            {
                return string.Join(" ", name.Split(' ').Reverse());
            }

            return name;
        }

        private static List<PersonInfo> CreateActressList(Api.Video video)
        {
            if (Plugin.Instance?.Configuration.EnableActresses == false)
            {
                return new List<PersonInfo>();
            }

            return (from actress in video.Actresses
                    select new PersonInfo
                    {
                        Name = NormalizeActressName(actress),
                        Type = PersonType.Actor,
                    }).ToList();
        }
    }
}