namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using Azure.Data.Tables;

    /// <summary>
    /// Default factory that creates Azure Table Storage backed tables.
    /// </summary>
    public sealed class AzureUrlShortenerTableFactory : IUrlShortenerTableFactory
    {
        /// <inheritdoc />
        public IUrlShortenerTable Create(string storageConnectionString, string tableName)
        {
            return new AzureUrlShortenerTable(new TableClient(storageConnectionString, tableName));
        }
    }
}
