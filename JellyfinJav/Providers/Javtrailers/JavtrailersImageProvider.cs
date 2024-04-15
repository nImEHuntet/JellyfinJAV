namespace JellyfinJav.Providers.JavtrailersProvider
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using JellyfinJav.Api;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Providers;

    /// <summary>The provider for Javtrailers video covers.</summary>
    public class JavtrailersImageProvider : IRemoteImageProvider, IHasOrder
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <inheritdoc />
        public string Name => "Javtrailers";

        /// <inheritdoc />
        public int Order => 100;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancelToken)
        {
            var id = item.GetProviderId("Javtrailers");
            if (string.IsNullOrEmpty(id))
            {
                return Array.Empty<RemoteImageInfo>();
            }

            var video = await JavtrailersClient.LoadVideo(id).ConfigureAwait(false);

            return new RemoteImageInfo[]
            {
                new RemoteImageInfo
                {
                    ProviderName = this.Name,
                    Type = ImageType.Primary,
                    Url = video?.Cover,
                },
            };
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancelToken)
        {
            var httpResponse = await HttpClient.GetAsync(url, cancelToken).ConfigureAwait(false);
            await Utility.CropThumb(httpResponse).ConfigureAwait(false);
            return httpResponse;
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[] { ImageType.Primary };
        }

        /// <inheritdoc />
        public bool Supports(BaseItem item) => item is Movie;
    }
}