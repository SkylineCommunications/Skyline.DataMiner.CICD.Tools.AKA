namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;

    /// <summary>
    /// Information about a short URL stored in the URL shortener backend.
    /// </summary>
    public sealed class ShortUrlInfo
    {
        /// <summary>
        /// The destination URL.
        /// </summary>
        public string DestinationUrl { get; set; } = String.Empty;

        /// <summary>
        /// The short URL.
        /// </summary>
        public string ShortUrl { get; set; } = String.Empty;

        /// <summary>
        /// The public aka URL.
        /// </summary>
        public string AkaUrl { get; set; } = String.Empty;

        /// <summary>
        /// The title stored together with the short URL.
        /// </summary>
        public string Title { get; set; } = String.Empty;

        /// <summary>
        /// The vanity / row key of the short URL.
        /// </summary>
        public string Vanity { get; set; } = String.Empty;

        /// <summary>
        /// The row key in the Azure Table.
        /// </summary>
        public string RowKey { get; set; } = String.Empty;

        /// <summary>
        /// The partition key in the Azure Table.
        /// </summary>
        public string PartitionKey { get; set; } = String.Empty;

        /// <summary>
        /// The creation timestamp.
        /// </summary>
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// Indicates whether the short URL has been archived.
        /// </summary>
        public bool? IsArchived { get; set; }
    }
}
