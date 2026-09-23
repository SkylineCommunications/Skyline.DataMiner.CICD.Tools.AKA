namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;

    using Azure.Core;
    using Azure.Data.Tables;

    /// <summary>
    /// Default factory that creates Azure Table Storage backed tables.
    /// </summary>
    public sealed class AzureUrlShortenerTableFactory : IUrlShortenerTableFactory
    {
        /// <inheritdoc />
        public IUrlShortenerTable Create(Uri tableServiceUri, string tableName, TokenCredential credential)
        {
            return new AzureUrlShortenerTable(new TableClient(tableServiceUri, tableName, credential));
        }
    }
}
