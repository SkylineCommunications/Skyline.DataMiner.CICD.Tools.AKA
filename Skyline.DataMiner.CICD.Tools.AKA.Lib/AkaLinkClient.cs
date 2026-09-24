namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure;
    using Azure.Core;
    using Azure.Data.Tables;
    using Azure.Identity;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Default implementation of <see cref="IAkaLinkClient"/> backed by Azure Table Storage.
    /// </summary>
    public sealed class AkaLinkClient : IAkaLinkClient
    {
        private const string DefaultPublicBaseUrl = "https://aka.dataminer.services";
        private const int MaxCreateAttempts = 5;

        private readonly AkaLinkOptions options;
        private readonly ILogger<AkaLinkClient> logger;
        private readonly IUrlShortenerTableFactory tableFactory;
        private readonly Lazy<TokenCredential> credential;

        /// <summary>
        /// Initializes a new instance of the <see cref="AkaLinkClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public AkaLinkClient(AkaLinkOptions options, ILogger<AkaLinkClient> logger)
            : this(options, logger, new AzureUrlShortenerTableFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AkaLinkClient"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="tableFactory">The table factory.</param>
        public AkaLinkClient(AkaLinkOptions options, ILogger<AkaLinkClient> logger, IUrlShortenerTableFactory tableFactory)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tableFactory = tableFactory ?? throw new ArgumentNullException(nameof(tableFactory));
            credential = new Lazy<TokenCredential>(
                () => new ClientSecretCredential(this.options.TenantId, this.options.ClientId, this.options.ClientSecret),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc />
        public async Task<string?> CreateShortUrlAsync(string longUrl, string title, CancellationToken cancellationToken = default)
        {
            if (!TryResolveConfiguration(out UrlShortenerTableConfiguration? configuration))
            {
                return null;
            }

            if (!IsSupportedAbsoluteUrl(longUrl))
            {
                logger.LogWarning("Cannot create short URL because {LongUrl} is not an absolute URL.", longUrl);
                return null;
            }

            try
            {
                IUrlShortenerTable table = tableFactory.Create(configuration!.TableServiceUri, configuration.UrlsTableName, credential.Value);
                await table.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

                for (int attempt = 1; attempt <= MaxCreateAttempts; attempt++)
                {
                    string rowKey = CreateRowKey();
                    TableEntity entity = CreateShortUrlEntity(rowKey, longUrl, title);
                    try
                    {
                        await table.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
                        return BuildShortUrl(configuration.PublicBaseUrl, rowKey);
                    }
                    catch (RequestFailedException ex) when (ex.Status == 409 && attempt < MaxCreateAttempts)
                    {
                        logger.LogWarning(ex, "Generated short URL row key {RowKey} already exists. Retrying with a new key.", rowKey);
                    }
                }

                logger.LogWarning("Failed to create a unique short URL row after {AttemptCount} attempts.", MaxCreateAttempts);
                return null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to create short URL for {LongUrl} in Azure Table Storage.", longUrl);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ShortUrlInfo>> ListAsync(CancellationToken cancellationToken = default)
        {
            if (!TryResolveConfiguration(out UrlShortenerTableConfiguration? configuration))
            {
                return Array.Empty<ShortUrlInfo>();
            }

            try
            {
                IUrlShortenerTable table = tableFactory.Create(configuration!.TableServiceUri, configuration.UrlsTableName, credential.Value);
                await table.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
                IReadOnlyList<TableEntity> entities = await table.QueryEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return entities
                    .Where(entity => !String.Equals(entity.RowKey, "KEY", StringComparison.Ordinal))
                    .Select(entity => ToShortUrlInfo(entity, configuration.PublicBaseUrl))
                    .ToList();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to list short URLs from Azure Table Storage.");
                return Array.Empty<ShortUrlInfo>();
            }
        }

        /// <inheritdoc />
        public async Task<bool> ArchiveAsync(ShortUrlInfo url, CancellationToken cancellationToken = default)
        {
            if (!TryResolveConfiguration(out UrlShortenerTableConfiguration? configuration))
            {
                return false;
            }

            string? partitionKey = url.PartitionKey;
            string? rowKey = String.IsNullOrWhiteSpace(url.RowKey) ? url.Vanity : url.RowKey;
            if (String.IsNullOrWhiteSpace(partitionKey) || String.IsNullOrWhiteSpace(rowKey))
            {
                logger.LogWarning("Cannot archive short URL because PartitionKey or RowKey is missing. PartitionKey: {PartitionKey}, RowKey: {RowKey}", partitionKey, rowKey);
                return false;
            }

            try
            {
                IUrlShortenerTable table = tableFactory.Create(configuration!.TableServiceUri, configuration.UrlsTableName, credential.Value);
                await table.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
                var entity = new TableEntity(partitionKey, rowKey)
                {
                    ["IsArchived"] = true,
                };

                await table.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Merge, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to archive short URL {ShortUrl} in Azure Table Storage.", url.ShortUrl ?? url.AkaUrl ?? url.RowKey);
                return false;
            }
        }

        private bool TryResolveConfiguration(out UrlShortenerTableConfiguration? configuration)
        {
            configuration = null;

            if (String.IsNullOrWhiteSpace(options.TenantId)
                || String.IsNullOrWhiteSpace(options.ClientId)
                || String.IsNullOrWhiteSpace(options.ClientSecret))
            {
                logger.LogWarning("URL shortener is disabled because the Azure app tenant ID, client ID, or client secret is not configured.");
                return false;
            }

            Uri? tableServiceUri = options.TableServiceUri;
            if (tableServiceUri == null
                || !tableServiceUri.IsAbsoluteUri
                || !String.Equals(tableServiceUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("URL shortener is disabled because Table service URI {TableServiceUri} is not a valid HTTPS URI.", tableServiceUri);
                return false;
            }

            string publicBaseUrl = options.PublicBaseUrl;
            if (String.IsNullOrWhiteSpace(publicBaseUrl) || !Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out Uri? _))
            {
                logger.LogWarning("URL shortener is disabled because public base URL {PublicBaseUrl} is invalid.", publicBaseUrl);
                return false;
            }

            string tableName = String.IsNullOrWhiteSpace(options.UrlsTableName) ? "UrlsDetails" : options.UrlsTableName.Trim();
            configuration = new UrlShortenerTableConfiguration(tableServiceUri, tableName, publicBaseUrl.TrimEnd('/'));
            return true;
        }

        private static TableEntity CreateShortUrlEntity(string rowKey, string longUrl, string title)
        {
            return new TableEntity(rowKey[0].ToString(CultureInfo.InvariantCulture), rowKey)
            {
                ["Url"] = longUrl,
                ["Title"] = title,
                ["Clicks"] = 0,
                ["IsArchived"] = false,
                ["CreatedUtc"] = DateTimeOffset.UtcNow,
                ["SchedulesPropertyRaw"] = "[]",
            };
        }

        private static ShortUrlInfo ToShortUrlInfo(TableEntity entity, string publicBaseUrl)
        {
            string shortUrl = BuildShortUrl(publicBaseUrl, entity.RowKey);
            return new ShortUrlInfo
            {
                DestinationUrl = GetString(entity, "Url"),
                ShortUrl = shortUrl,
                AkaUrl = shortUrl,
                Title = GetString(entity, "Title"),
                Vanity = entity.RowKey,
                RowKey = entity.RowKey,
                PartitionKey = entity.PartitionKey,
                Created = GetDateTimeOffset(entity, "CreatedUtc") ?? entity.Timestamp,
                IsArchived = GetBoolean(entity, "IsArchived"),
            };
        }

        private static string CreateRowKey()
        {
            return "q" + Guid.NewGuid().ToString("N").Substring(0, 12);
        }

        private static string BuildShortUrl(string publicBaseUrl, string rowKey)
        {
            return $"{publicBaseUrl.TrimEnd('/')}/{rowKey}";
        }

        private static bool IsSupportedAbsoluteUrl(string value)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
            {
                return true;
            }

            int fragmentIndex = value.IndexOf('#');
            if (fragmentIndex <= 0)
            {
                return false;
            }

            string urlWithoutFragment = value.Substring(0, fragmentIndex);
            return Uri.IsWellFormedUriString(urlWithoutFragment, UriKind.Absolute);
        }

        private static string GetString(TableEntity entity, string propertyName)
        {
            return entity.TryGetValue(propertyName, out object? value) ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? String.Empty : String.Empty;
        }

        private static bool? GetBoolean(TableEntity entity, string propertyName)
        {
            if (!entity.TryGetValue(propertyName, out object? value) || value == null)
            {
                return null;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            return Boolean.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out bool parsed) ? parsed : (bool?)null;
        }

        private static DateTimeOffset? GetDateTimeOffset(TableEntity entity, string propertyName)
        {
            if (!entity.TryGetValue(propertyName, out object? value) || value == null)
            {
                return null;
            }

            switch (value)
            {
                case DateTimeOffset dateTimeOffset:
                    return dateTimeOffset;
                case DateTime dateTime:
                    return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
                case string text when DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset parsed):
                    return parsed;
                default:
                    return null;
            }
        }

        private sealed class UrlShortenerTableConfiguration
        {
            public UrlShortenerTableConfiguration(Uri tableServiceUri, string urlsTableName, string publicBaseUrl)
            {
                TableServiceUri = tableServiceUri;
                UrlsTableName = urlsTableName;
                PublicBaseUrl = publicBaseUrl;
            }

            public Uri TableServiceUri { get; }

            public string UrlsTableName { get; }

            public string PublicBaseUrl { get; }
        }
    }
}
