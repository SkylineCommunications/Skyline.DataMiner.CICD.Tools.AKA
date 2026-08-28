namespace Skyline.DataMiner.CICD.Tools.AKA.LibTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Skyline.DataMiner.CICD.Tools.AKA.Lib;

    [TestClass]
    public sealed class AkaLinkCleanupTests
    {
        [TestMethod]
        public void ResolveRetentionDays_FallsBackToDefaultWhenZeroOrNegative()
        {
            AkaLinkCleanup.ResolveRetentionDays(0).Should().Be(30);
            AkaLinkCleanup.ResolveRetentionDays(-5).Should().Be(30);
        }

        [TestMethod]
        public void ResolveRetentionDays_ReturnsPositiveValue()
        {
            AkaLinkCleanup.ResolveRetentionDays(7).Should().Be(7);
        }

        [TestMethod]
        public void SelectUrlsToArchive_OnlySelectsMatchingMarker()
        {
            var now = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
            var urls = new[]
            {
                new ShortUrlInfo { Title = "AKATool|production|old", Created = now.AddDays(-40), IsArchived = false },
                new ShortUrlInfo { Title = "Other|production|old", Created = now.AddDays(-40), IsArchived = false },
            };

            var result = AkaLinkCleanup.SelectUrlsToArchive(urls, now, 30);

            result.Should().ContainSingle();
            result.Single().Title.Should().Be("AKATool|production|old");
        }

        [TestMethod]
        public void SelectUrlsToArchive_OnlySelectsUrlsOlderThanRetention()
        {
            var now = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
            var urls = new[]
            {
                new ShortUrlInfo { Title = "AKATool|production|old", Created = now.AddDays(-40), IsArchived = false },
                new ShortUrlInfo { Title = "AKATool|production|recent", Created = now.AddDays(-5), IsArchived = false },
            };

            var result = AkaLinkCleanup.SelectUrlsToArchive(urls, now, 30);

            result.Should().ContainSingle();
            result.Single().Title.Should().Be("AKATool|production|old");
        }

        [TestMethod]
        public void SelectUrlsToArchive_SkipsArchivedUrls()
        {
            var now = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
            var urls = new[]
            {
                new ShortUrlInfo { Title = "AKATool|production|old", Created = now.AddDays(-40), IsArchived = true },
            };

            var result = AkaLinkCleanup.SelectUrlsToArchive(urls, now, 30);

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void SelectUrlsToArchive_WhenCleanupAll_SelectsAllMatchingUrls()
        {
            var now = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);
            var urls = new[]
            {
                new ShortUrlInfo { Title = "AKATool|production|recent", Created = now.AddDays(-5), IsArchived = false },
                new ShortUrlInfo { Title = "Other|production|old", Created = now.AddDays(-40), IsArchived = false },
            };

            var result = AkaLinkCleanup.SelectUrlsToArchive(urls, now, 30, cleanupAll: true);

            result.Should().ContainSingle();
            result.Single().Title.Should().Be("AKATool|production|recent");
        }

        [TestMethod]
        public void BuildSummary_IncludesCountsAndUrls()
        {
            var now = new DateTimeOffset(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
            string summary = AkaLinkCleanup.BuildSummary(
                now,
                "AKATool",
                30,
                listedCount: 10,
                selectedCount: 2,
                archivedUrls: new[] { "https://aka.dataminer.services/q1", "https://aka.dataminer.services/q2" },
                errors: new[] { "something failed" });

            summary.Should().Contain("UTC: 2026-08-01T12:00:00.0000000Z");
            summary.Should().Contain("Marker: AKATool");
            summary.Should().Contain("Retention: 30 days");
            summary.Should().Contain("Listed: 10");
            summary.Should().Contain("Selected: 2");
            summary.Should().Contain("Archived: 2");
            summary.Should().Contain("Errors: 1");
            summary.Should().Contain("https://aka.dataminer.services/q1");
            summary.Should().Contain("something failed");
        }
    }
}
