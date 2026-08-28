namespace Skyline.DataMiner.CICD.Tools.AKA.LibTests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure;
    using Azure.Data.Tables;

    using Skyline.DataMiner.CICD.Tools.AKA.Lib;

    internal sealed class FakeUrlShortenerTable : IUrlShortenerTable
    {
        public int CreateIfNotExistsCount { get; private set; }

        public List<TableEntity> AddedEntities { get; } = new List<TableEntity>();

        public List<TableEntity> UpdatedEntities { get; } = new List<TableEntity>();

        public IReadOnlyList<TableEntity> QueryResults { get; init; } = new List<TableEntity>();

        public TableUpdateMode? UpdateMode { get; private set; }

        public Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            CreateIfNotExistsCount++;
            return Task.CompletedTask;
        }

        public Task AddEntityAsync(TableEntity entity, CancellationToken cancellationToken)
        {
            AddedEntities.Add(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TableEntity>> QueryEntitiesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(QueryResults);
        }

        public Task UpdateEntityAsync(TableEntity entity, ETag ifMatch, TableUpdateMode mode, CancellationToken cancellationToken)
        {
            UpdatedEntities.Add(entity);
            UpdateMode = mode;
            return Task.CompletedTask;
        }
    }
}
