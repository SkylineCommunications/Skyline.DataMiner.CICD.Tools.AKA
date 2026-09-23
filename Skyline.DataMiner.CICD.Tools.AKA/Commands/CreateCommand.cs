namespace Skyline.DataMiner.CICD.Tools.AKA.Commands
{
    using Skyline.DataMiner.CICD.Tools.AKA.Lib;

    internal class CreateCommand : Command
    {
        public CreateCommand()
            : base(name: "create", description: "Creates a short aka link for the given URL.")
        {
            AddOption(new Option<string>(
                aliases: ["--url", "-u"],
                description: "The long URL to shorten.")
            {
                IsRequired = true,
            });

            AddOption(new Option<string>(
                aliases: ["--tenant-id"],
                getDefaultValue: () => Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? String.Empty,
                description: "The Microsoft Entra tenant ID. Defaults to AZURE_TENANT_ID."));

            AddOption(new Option<string>(
                aliases: ["--client-id"],
                getDefaultValue: () => Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? String.Empty,
                description: "The Azure app client ID. Defaults to AZURE_CLIENT_ID."));

            AddOption(new Option<string>(
                aliases: ["--client-secret"],
                getDefaultValue: () => Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? String.Empty,
                description: "The Azure app client secret. Defaults to AZURE_CLIENT_SECRET."));

            AddOption(new Option<string>(
                aliases: ["--title", "-t"],
                description: "The title to store with the short URL. When omitted, a title based on the marker and environment is generated."));

            AddOption(new Option<string>(
                aliases: ["--marker", "-m"],
                getDefaultValue: () => AkaLinkTitleBuilder.DefaultMarker,
                description: "The title marker used to identify links created by this tool."));

            AddOption(new Option<string>(
                aliases: ["--environment", "-e"],
                getDefaultValue: () => "production",
                description: "The environment name included in the generated title."));

            AddOption(new Option<string>(
                aliases: ["--table-name"],
                getDefaultValue: () => "UrlsDetails",
                description: "The name of the Azure Table that contains the URL details."));

            AddOption(new Option<string>(
                aliases: ["--public-base-url"],
                getDefaultValue: () => "https://aka.dataminer.services",
                description: "The public base URL used to build short links."));

            AddOption(new Option<bool>(
                aliases: ["--reuse", "-r"],
                getDefaultValue: () => true,
                description: "When true, an existing short URL with the same title and destination is reused."));
        }
    }

    internal class CreateCommandHandler(
        ILogger<CreateCommandHandler> logger,
        ILogger<AkaLinkClient> akaLogger) : ICommandHandler
    {
        /*
         * Automatic binding with System.CommandLine.NamingConventionBinder
         * The property names need to match with the command line argument names.
         */

        public required string Url { get; set; }

        public string TenantId { get; set; } = String.Empty;

        public string ClientId { get; set; } = String.Empty;

        public string ClientSecret { get; set; } = String.Empty;

        public string? Title { get; set; }

        public string Marker { get; set; } = AkaLinkTitleBuilder.DefaultMarker;

        public string Environment { get; set; } = "production";

        public string TableName { get; set; } = "UrlsDetails";

        public string PublicBaseUrl { get; set; } = "https://aka.dataminer.services";

        public bool Reuse { get; set; } = true;

        public int Invoke(InvocationContext context)
        {
            return (int)ExitCodes.NotImplemented;
        }

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            logger.LogDebug("Starting {method}...", nameof(CreateCommand));

            try
            {
                if (String.IsNullOrWhiteSpace(TenantId)
                    || String.IsNullOrWhiteSpace(ClientId)
                    || String.IsNullOrWhiteSpace(ClientSecret))
                {
                    logger.LogError("Azure app credentials are incomplete. Configure AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET or pass the corresponding command options.");
                    return (int)ExitCodes.UnexpectedException;
                }

                string title = ResolveTitle();
                logger.LogInformation("Creating short URL for {Url} with title '{Title}'.", Url, title);

                var options = new AkaLinkOptions
                {
                    TenantId = TenantId,
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    PublicBaseUrl = PublicBaseUrl,
                    UrlsTableName = TableName,
                    TitleMarker = Marker,
                };

                var client = new AkaLinkClient(options, akaLogger);

                if (Reuse)
                {
                    string? existing = await TryGetExistingShortUrlAsync(client, title, Url, context.GetCancellationToken()).ConfigureAwait(false);
                    if (!String.IsNullOrWhiteSpace(existing))
                    {
                        Console.WriteLine(existing);
                        return (int)ExitCodes.Ok;
                    }
                }

                string? shortUrl = await client.CreateShortUrlAsync(Url, title, context.GetCancellationToken()).ConfigureAwait(false);
                if (String.IsNullOrWhiteSpace(shortUrl))
                {
                    logger.LogError("Failed to create a short URL for {Url}.", Url);
                    return (int)ExitCodes.UnexpectedException;
                }

                logger.LogInformation("Created short URL: {ShortUrl}", shortUrl);
                Console.WriteLine(shortUrl);
                return (int)ExitCodes.Ok;
            }
            catch (OperationCanceledException) when (context.GetCancellationToken().IsCancellationRequested)
            {
                logger.LogWarning("Operation was cancelled.");
                return (int)ExitCodes.UnexpectedException;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed the create command.");
                return (int)ExitCodes.UnexpectedException;
            }
            finally
            {
                logger.LogDebug("Finished {method}.", nameof(CreateCommand));
            }
        }

        private string ResolveTitle()
        {
            if (!String.IsNullOrWhiteSpace(Title))
            {
                return Title;
            }

            string environment = AkaLinkTitleBuilder.ResolveEnvironment(Environment);
            return AkaLinkTitleBuilder.BuildTitle(Marker, environment, Guid.NewGuid().ToString("N"));
        }

        private static async Task<string?> TryGetExistingShortUrlAsync(IAkaLinkClient client, string title, string longUrl, CancellationToken cancellationToken)
        {
            IReadOnlyList<ShortUrlInfo> existingUrls = await client.ListAsync(cancellationToken).ConfigureAwait(false);
            ShortUrlInfo? existingUrl = existingUrls.FirstOrDefault(url => AkaLinkTitleBuilder.IsReusable(url, title, longUrl));

            if (existingUrl == null)
            {
                return null;
            }

            return String.IsNullOrWhiteSpace(existingUrl.AkaUrl) ? existingUrl.ShortUrl : existingUrl.AkaUrl;
        }
    }
}
