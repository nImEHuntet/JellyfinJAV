namespace JellyfinJav.Providers.JavtrailersProvider
{
    using MediaBrowser.Controller.Entities.Movies;
    using MediaBrowser.Controller.Providers;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Providers;

    /// <summary>External ID for a Javtrailers video.</summary>
    public class JavTrailersExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName => "Javtrailers";

        /// <inheritdoc />
        public string Key => "Javtrailers";

        /// <inheritdoc />
        public string UrlFormatString => "https://javtrailers.com/video/{0}";

        /// <inheritdoc />
        public ExternalIdMediaType? Type => ExternalIdMediaType.Movie;

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item)
        {
            return item is Movie;
        }
    }
}