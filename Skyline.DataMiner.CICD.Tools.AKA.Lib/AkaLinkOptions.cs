namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    /// <summary>
    /// Options for connecting to the URL shortener backend.
    /// </summary>
    public sealed class AkaLinkOptions
    {
        /// <summary>
        /// The Azure Storage connection string.
        /// </summary>
        public string StorageConnectionString { get; set; } = String.Empty;

        /// <summary>
        /// The public base URL used to build short links.
        /// </summary>
        public string PublicBaseUrl { get; set; } = "https://aka.dataminer.services";

        /// <summary>
        /// The name of the Azure Table that contains the URL details.
        /// </summary>
        public string UrlsTableName { get; set; } = "UrlsDetails";

        /// <summary>
        /// The marker used in the title to identify links created by this tool/library.
        /// </summary>
        public string TitleMarker { get; set; } = AkaLinkTitleBuilder.DefaultMarker;
    }
}
