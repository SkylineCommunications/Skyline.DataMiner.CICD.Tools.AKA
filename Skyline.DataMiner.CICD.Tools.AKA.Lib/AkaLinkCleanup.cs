namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Helpers for cleaning up short URLs created by the AKA tool/library.
    /// </summary>
    public static class AkaLinkCleanup
    {
        private const int DefaultRetentionDays = 30;

        /// <summary>
        /// Selects the short URLs that should be archived based on the marker and retention period.
        /// </summary>
        /// <param name="urls">The URLs to inspect.</param>
        /// <param name="now">The current timestamp.</param>
        /// <param name="retentionDays">The retention period in days.</param>
        /// <param name="marker">The title marker. Defaults to <see cref="AkaLinkTitleBuilder.DefaultMarker"/>.</param>
        /// <param name="cleanupAll">When <c>true</c>, all matching URLs are selected regardless of age.</param>
        /// <returns>The URLs that should be archived.</returns>
        public static IReadOnlyList<ShortUrlInfo> SelectUrlsToArchive(
            IEnumerable<ShortUrlInfo> urls,
            DateTimeOffset now,
            int retentionDays,
            string? marker = null,
            bool cleanupAll = false)
        {
            if (urls == null)
            {
                throw new ArgumentNullException(nameof(urls));
            }

            TimeSpan retention = TimeSpan.FromDays(ResolveRetentionDays(retentionDays));
            DateTimeOffset cutoff = now - retention;
            string resolvedMarker = AkaLinkTitleBuilder.ResolveMarker(marker);

            IEnumerable<ShortUrlInfo> matchingUrls = urls.Where(url => IsUnarchivedWithMarker(url, resolvedMarker));
            return cleanupAll
                ? matchingUrls.ToList()
                : matchingUrls.Where(url => url.Created.HasValue && url.Created.Value < cutoff).ToList();
        }

        /// <summary>
        /// Resolves the retention days, falling back to the default when the value is not positive.
        /// </summary>
        /// <param name="retentionDays">The requested retention.</param>
        /// <returns>The resolved retention days.</returns>
        public static int ResolveRetentionDays(int retentionDays)
        {
            return retentionDays <= 0 ? DefaultRetentionDays : retentionDays;
        }

        /// <summary>
        /// Builds a human-readable summary of a cleanup run.
        /// </summary>
        /// <param name="timestamp">The timestamp of the cleanup run.</param>
        /// <param name="marker">The title marker.</param>
        /// <param name="retentionDays">The retention days used.</param>
        /// <param name="listedCount">The number of URLs listed.</param>
        /// <param name="selectedCount">The number of URLs selected for archival.</param>
        /// <param name="archivedUrls">The URLs that were archived.</param>
        /// <param name="errors">The errors that occurred.</param>
        /// <param name="cleanupAll">Whether all matching URLs were selected.</param>
        /// <returns>The summary text.</returns>
        public static string BuildSummary(
            DateTimeOffset timestamp,
            string? marker,
            int retentionDays,
            int listedCount,
            int selectedCount,
            IReadOnlyCollection<string> archivedUrls,
            IReadOnlyCollection<string> errors,
            bool cleanupAll = false)
        {
            string utcTimestamp = timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
            int resolvedRetentionDays = ResolveRetentionDays(retentionDays);
            string resolvedMarker = AkaLinkTitleBuilder.ResolveMarker(marker);

            var builder = new StringBuilder();
            if (selectedCount == 0 && archivedUrls.Count == 0 && errors.Count == 0)
            {
                builder.AppendLine($"UTC: {utcTimestamp}");
                builder.AppendLine("AKA cleanup: no short URLs archived.");
            }
            else
            {
                builder.AppendLine($"UTC: {utcTimestamp}");
                builder.AppendLine("AKA cleanup summary");
            }

            builder.AppendLine($"Marker: {resolvedMarker}");
            builder.AppendLine($"Cleanup all: {cleanupAll}");
            builder.AppendLine($"Retention: {resolvedRetentionDays} days");
            builder.AppendLine($"Listed: {listedCount}");
            builder.AppendLine($"Selected: {selectedCount}");
            builder.AppendLine($"Archived: {archivedUrls.Count}");
            builder.AppendLine($"Errors: {errors.Count}");

            if (archivedUrls.Count > 0)
            {
                builder.AppendLine("Archived URLs:");
                foreach (string archivedUrl in archivedUrls)
                {
                    builder.AppendLine($"- {archivedUrl}");
                }
            }

            if (errors.Count > 0)
            {
                builder.AppendLine("Errors:");
                foreach (string error in errors)
                {
                    builder.AppendLine($"- {error}");
                }
            }

            return builder.ToString().TrimEnd();
        }

        private static bool IsUnarchivedWithMarker(ShortUrlInfo url, string marker)
        {
            return url.IsArchived != true &&
                   !String.IsNullOrWhiteSpace(url.Title) &&
                   url.Title!.StartsWith(marker, StringComparison.OrdinalIgnoreCase);
        }
    }
}
