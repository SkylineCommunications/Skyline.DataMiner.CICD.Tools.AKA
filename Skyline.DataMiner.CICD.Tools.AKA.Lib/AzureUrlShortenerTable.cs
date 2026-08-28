namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure.Data.Tables;

    /// <summary>
    /// Azure Table Storage implementation of <see cref="IUrlShortenerTable"/>.
    /// </summary>
    public sealed class AzureUrlShortenerTable : IUrlShortenerTable
    {
        private readonly TableClient tableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureUrlShortenerTable"/> class.
        /// </summary>
        /// <param name="tableClient">The Azure Table client.</param>
        public AzureUrlShortenerTable(TableClient tableClient)
        {
            this.tableClient = tableClient;
        }

        /// <inheritdoc />
        public async Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            await tableClient.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task AddEntityAsync(TableEntity entity, CancellationToken cancellationToken)
        {
            await tableClient.AddEntityAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TableEntity>> QueryEntitiesAsync(CancellationToken cancellationToken)
        {
            var entities = new List<TableEntity>();
            await foreach (TableEntity entity in tableClient.QueryAsync<TableEntity>(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                entities.Add(entity);
            }

            return entities;
        }

        /// <inheritdoc />
        public async Task UpdateEntityAsync(TableEntity entity, Azure.ETag ifMatch, TableUpdateMode mode, CancellationToken cancellationToken)
        {
            await tableClient.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken).ConfigureAwait(false);
        }
    }
}
