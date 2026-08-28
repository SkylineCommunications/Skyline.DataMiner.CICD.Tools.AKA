namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;

    /// <summary>
    /// Helpers for building and recognizing titles used by the AKA tool/library.
    /// </summary>
    public static class AkaLinkTitleBuilder
    {
        /// <summary>
        /// The default title marker used to identify links created by this tool/library.
        /// </summary>
        public const string DefaultMarker = "AKATool";

        /// <summary>
        /// Builds a short URL title from the marker, environment and any extra identifiers.
        /// </summary>
        /// <param name="marker">The title marker. Defaults to <see cref="DefaultMarker"/> when empty.</param>
        /// <param name="environment">The environment name.</param>
        /// <param name="identifiers">Optional extra identifiers.</param>
        /// <returns>The formatted title.</returns>
        public static string BuildTitle(string? marker, string? environment, params string[] identifiers)
        {
            string resolvedMarker = ResolveMarker(marker);
            string resolvedEnvironment = ResolveEnvironment(environment);

            if (identifiers == null || identifiers.Length == 0)
            {
                return $"{resolvedMarker}|{resolvedEnvironment}";
            }

            return $"{resolvedMarker}|{resolvedEnvironment}|{String.Join("|", identifiers!)}";
        }

        /// <summary>
        /// Normalizes an environment name.
        /// </summary>
        /// <param name="environment">The raw environment name.</param>
        /// <returns>The normalized environment name.</returns>
        public static string ResolveEnvironment(string? environment)
        {
            if (String.IsNullOrWhiteSpace(environment))
            {
                return "production";
            }

            string normalizedEnvironment = environment!.Trim();
            if (String.Equals(normalizedEnvironment, "prod", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(normalizedEnvironment, "production", StringComparison.OrdinalIgnoreCase))
            {
                return "production";
            }

            if (String.Equals(normalizedEnvironment, "staging", StringComparison.OrdinalIgnoreCase))
            {
                return "staging";
            }

            if (String.Equals(normalizedEnvironment, "sandbox", StringComparison.OrdinalIgnoreCase))
            {
                return "sandbox";
            }

            return normalizedEnvironment.ToLowerInvariant();
        }

        /// <summary>
        /// Resolves the title marker, falling back to <see cref="DefaultMarker"/>.
        /// </summary>
        /// <param name="marker">The raw marker.</param>
        /// <returns>The resolved marker.</returns>
        public static string ResolveMarker(string? marker)
        {
            return String.IsNullOrWhiteSpace(marker) ? DefaultMarker : marker!.Trim();
        }

        /// <summary>
        /// Determines whether an existing short URL can be reused for the same title and destination.
        /// </summary>
        /// <param name="url">The existing short URL.</param>
        /// <param name="title">The requested title.</param>
        /// <param name="destinationUrl">The requested destination URL.</param>
        /// <returns><c>true</c> when the URL is reusable.</returns>
        public static bool IsReusable(ShortUrlInfo url, string title, string destinationUrl)
        {
            return url.IsArchived != true &&
                String.Equals(url.Title!, title, StringComparison.OrdinalIgnoreCase) &&
                String.Equals(url.DestinationUrl!, destinationUrl, StringComparison.Ordinal) &&
                (!String.IsNullOrWhiteSpace(url.AkaUrl) || !String.IsNullOrWhiteSpace(url.ShortUrl));
        }

        /// <summary>
        /// Checks whether the title of a short URL starts with the given marker.
        /// </summary>
        /// <param name="url">The short URL.</param>
        /// <param name="marker">The marker. Defaults to <see cref="DefaultMarker"/> when empty.</param>
        /// <returns><c>true</c> when the title starts with the marker.</returns>
        public static bool HasMarker(ShortUrlInfo url, string? marker = null)
        {
            string resolvedMarker = ResolveMarker(marker);
            return !String.IsNullOrWhiteSpace(url.Title) &&
                url.Title!.StartsWith(resolvedMarker, StringComparison.OrdinalIgnoreCase);
        }
    }
}
