namespace Skyline.DataMiner.CICD.Tools.AKA.LibTests
{
    using FluentAssertions;

    [TestClass]
    public sealed class AkaLinkTitleBuilderTests
    {
        [TestMethod]
        [DataRow(null, "production")]
        [DataRow("", "production")]
        [DataRow("Production", "production")]
        [DataRow("prod", "production")]
        [DataRow("staging", "staging")]
        [DataRow("sandbox", "sandbox")]
        [DataRow("FeatureX", "featurex")]
        public void ResolveEnvironment_NormalizesEnvironment(string? environment, string expected)
        {
            string result = AkaLinkTitleBuilder.ResolveEnvironment(environment);

            result.Should().Be(expected);
        }

        [TestMethod]
        public void ResolveMarker_ReturnsDefaultWhenEmpty()
        {
            AkaLinkTitleBuilder.ResolveMarker(null).Should().Be(AkaLinkTitleBuilder.DefaultMarker);
            AkaLinkTitleBuilder.ResolveMarker("").Should().Be(AkaLinkTitleBuilder.DefaultMarker);
            AkaLinkTitleBuilder.ResolveMarker("  ").Should().Be(AkaLinkTitleBuilder.DefaultMarker);
        }

        [TestMethod]
        public void BuildTitle_IncludesMarkerEnvironmentAndIdentifiers()
        {
            string result = AkaLinkTitleBuilder.BuildTitle("AKATool", "staging", "id1", "id2");

            result.Should().Be("AKATool|staging|id1|id2");
        }

        [TestMethod]
        public void BuildTitle_FallsBackToDefaultMarker()
        {
            string result = AkaLinkTitleBuilder.BuildTitle(null, "production", "id");

            result.Should().StartWith(AkaLinkTitleBuilder.DefaultMarker);
        }

        [TestMethod]
        public void IsReusable_ReturnsTrueForMatchingActiveUrl()
        {
            var url = new ShortUrlInfo
            {
                Title = "AKATool|production|id",
                DestinationUrl = "https://example.test/long",
                AkaUrl = "https://aka.dataminer.services/abc",
            };

            AkaLinkTitleBuilder.IsReusable(url, "AKATool|production|id", "https://example.test/long").Should().BeTrue();
        }

        [TestMethod]
        public void IsReusable_ReturnsFalseForArchivedUrl()
        {
            var url = new ShortUrlInfo
            {
                Title = "AKATool|production|id",
                DestinationUrl = "https://example.test/long",
                AkaUrl = "https://aka.dataminer.services/abc",
                IsArchived = true,
            };

            AkaLinkTitleBuilder.IsReusable(url, "AKATool|production|id", "https://example.test/long").Should().BeFalse();
        }

        [TestMethod]
        public void HasMarker_DetectsMarker()
        {
            var url = new ShortUrlInfo { Title = "AKATool|production|id" };

            AkaLinkTitleBuilder.HasMarker(url).Should().BeTrue();
            AkaLinkTitleBuilder.HasMarker(url, "Other").Should().BeFalse();
        }
    }
}
