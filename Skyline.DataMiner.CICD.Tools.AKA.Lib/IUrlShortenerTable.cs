namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure;
    using Azure.Data.Tables;

    /// <summary>
    /// Abstraction over the Azure Table that stores short URL details.
    /// </summary>
    public interface IUrlShortenerTable
    {
        /// <summary>
        /// Creates the table if it does not exist yet.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task CreateIfNotExistsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new entity to the table.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task AddEntityAsync(TableEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Queries all entities from the table.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The list of entities.</returns>
        Task<IReadOnlyList<TableEntity>> QueryEntitiesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Updates an entity in the table.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="ifMatch">The ETag.</param>
        /// <param name="mode">The update mode.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateEntityAsync(TableEntity entity, ETag ifMatch, TableUpdateMode mode, CancellationToken cancellationToken);
    }
}
