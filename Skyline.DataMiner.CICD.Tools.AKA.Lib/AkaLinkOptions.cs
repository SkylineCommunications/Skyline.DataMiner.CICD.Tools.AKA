namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;

    /// <summary>
    /// Options for connecting to the URL shortener backend.
    /// </summary>
    public sealed class AkaLinkOptions
    {
        /// <summary>
        /// The default Azure Table Storage service URL.
        /// </summary>
        public const string DefaultTableServiceUrl = "https://urldatau44etstg.table.core.windows.net";

        /// <summary>
        /// The Microsoft Entra tenant ID of the app registration.
        /// </summary>
        public string TenantId { get; set; } = String.Empty;

        /// <summary>
        /// The client ID of the app registration.
        /// </summary>
        public string ClientId { get; set; } = String.Empty;

        /// <summary>
        /// The client secret of the app registration.
        /// </summary>
        public string ClientSecret { get; set; } = String.Empty;

        /// <summary>
        /// The Azure Table Storage service URI.
        /// </summary>
        public Uri TableServiceUri { get; set; } = new Uri(DefaultTableServiceUrl);

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
