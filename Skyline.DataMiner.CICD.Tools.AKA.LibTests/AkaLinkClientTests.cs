namespace Skyline.DataMiner.CICD.Tools.AKA.LibTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure.Data.Tables;

    using FluentAssertions;

    using Microsoft.Extensions.Logging.Abstractions;

    using Skyline.DataMiner.CICD.Tools.AKA.Lib;

    [TestClass]
    public sealed class AkaLinkClientTests
    {
        [TestMethod]
        public async Task CreateShortUrlAsync_WhenConfigured_InsertsTableEntityAndReturnsPublicUrl()
        {
            var table = new FakeUrlShortenerTable();
            AkaLinkClient client = CreateClient("UseDevelopmentStorage=true", table);

            string? result = await client.CreateShortUrlAsync("https://example.test/long", "AKATool|123", CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().StartWith("https://aka.dataminer.services/q");
            table.CreateIfNotExistsCount.Should().Be(1);
            table.AddedEntities.Should().ContainSingle();
            TableEntity entity = table.AddedEntities.Single();
            entity.PartitionKey.Should().Be(entity.RowKey[0].ToString());
            entity.RowKey.Should().StartWith("q");
            entity["Url"].Should().Be("https://example.test/long");
            entity["Title"].Should().Be("AKATool|123");
            entity["Clicks"].Should().Be(0);
            entity["IsArchived"].Should().Be(false);
            entity["CreatedUtc"].Should().BeOfType<DateTimeOffset>();
            entity["SchedulesPropertyRaw"].Should().Be("[]");
        }

        [TestMethod]
        public async Task CreateShortUrlAsync_WhenUrlHasFragment_InsertsTableEntity()
        {
            var table = new FakeUrlShortenerTable();
            AkaLinkClient client = CreateClient("UseDevelopmentStorage=true", table);
            string longUrl = "https://example.test/app#action=something";

            string? result = await client.CreateShortUrlAsync(longUrl, "AKATool|fragment", CancellationToken.None);

            result.Should().NotBeNull();
            table.AddedEntities.Should().ContainSingle();
            table.AddedEntities.Single()["Url"].Should().Be(longUrl);
        }

        [TestMethod]
        public async Task ListAsync_MapsEntitiesAndSkipsNextIdRow()
        {
            DateTimeOffset created = new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero);
            var table = new FakeUrlShortenerTable
            {
                QueryResults = new List<TableEntity>
                {
                    new TableEntity("1", "KEY")
                    {
                        ["Id"] = 1025,
                    },
                    new TableEntity("q", "qabc123def45")
                    {
                        ["Title"] = "AKATool|123",
                        ["Url"] = "https://example.test/long",
                        ["IsArchived"] = false,
                        ["CreatedUtc"] = created,
                    },
                },
            };
            AkaLinkClient client = CreateClient("UseDevelopmentStorage=true", table);

            IReadOnlyList<ShortUrlInfo> result = await client.ListAsync(CancellationToken.None);

            result.Should().ContainSingle();
            ShortUrlInfo info = result.Single();
            info.ShortUrl.Should().Be("https://aka.dataminer.services/qabc123def45");
            info.AkaUrl.Should().Be("https://aka.dataminer.services/qabc123def45");
            info.Title.Should().Be("AKATool|123");
            info.Vanity.Should().Be("qabc123def45");
            info.RowKey.Should().Be("qabc123def45");
            info.PartitionKey.Should().Be("q");
            info.Created.Should().Be(created);
            info.IsArchived.Should().BeFalse();
        }

        [TestMethod]
        public async Task ArchiveAsync_MergesIsArchived()
        {
            var table = new FakeUrlShortenerTable();
            AkaLinkClient client = CreateClient("UseDevelopmentStorage=true", table);
            var url = new ShortUrlInfo
            {
                PartitionKey = "q",
                RowKey = "qabc123def45",
            };

            bool result = await client.ArchiveAsync(url, CancellationToken.None);

            result.Should().BeTrue();
            table.UpdatedEntities.Should().ContainSingle();
            TableEntity entity = table.UpdatedEntities.Single();
            entity.PartitionKey.Should().Be("q");
            entity.RowKey.Should().Be("qabc123def45");
            entity["IsArchived"].Should().Be(true);
            table.UpdateMode.Should().Be(TableUpdateMode.Merge);
        }

        [TestMethod]
        public async Task CreateShortUrlAsync_WhenStorageConnectionStringMissing_DoesNotUseTable()
        {
            var table = new FakeUrlShortenerTable();
            AkaLinkClient client = CreateClient(null, table);

            string? result = await client.CreateShortUrlAsync("https://example.test/long", "AKATool|123", CancellationToken.None);

            result.Should().BeNull();
            table.CreateIfNotExistsCount.Should().Be(0);
            table.AddedEntities.Should().BeEmpty();
        }

        private static AkaLinkClient CreateClient(string? storageConnectionString, FakeUrlShortenerTable table)
        {
            var options = new AkaLinkOptions
            {
                StorageConnectionString = storageConnectionString ?? String.Empty,
                PublicBaseUrl = "https://aka.dataminer.services",
                UrlsTableName = "UrlsDetails",
                TitleMarker = "AKATool",
            };

            return new AkaLinkClient(
                options,
                NullLogger<AkaLinkClient>.Instance,
                new FakeUrlShortenerTableFactory(table));
        }
    }
}
