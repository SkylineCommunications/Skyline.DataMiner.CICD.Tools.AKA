namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Client that creates, lists and archives short URLs.
    /// </summary>
    public interface IAkaLinkClient
    {
        /// <summary>
        /// Creates a short URL for the given long URL and title.
        /// </summary>
        /// <param name="longUrl">The destination URL.</param>
        /// <param name="title">The title to store with the short URL.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created short URL, or <c>null</c> when creation failed.</returns>
        Task<string?> CreateShortUrlAsync(string longUrl, string title, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists all short URLs from the backend.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The list of short URLs.</returns>
        Task<IReadOnlyList<ShortUrlInfo>> ListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Archives the given short URL.
        /// </summary>
        /// <param name="url">The short URL to archive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><c>true</c> when the URL was archived.</returns>
        Task<bool> ArchiveAsync(ShortUrlInfo url, CancellationToken cancellationToken = default);
    }
}
