namespace Skyline.DataMiner.CICD.Tools.AKA.LibTests
{
    using System;

    using Azure.Core;

    using Skyline.DataMiner.CICD.Tools.AKA.Lib;

    internal sealed class FakeUrlShortenerTableFactory : IUrlShortenerTableFactory
    {
        private readonly FakeUrlShortenerTable table;

        public FakeUrlShortenerTableFactory(FakeUrlShortenerTable table)
        {
            this.table = table;
        }

        public Uri? TableServiceUri { get; private set; }

        public string? TableName { get; private set; }

        public TokenCredential? Credential { get; private set; }

        public IUrlShortenerTable Create(Uri tableServiceUri, string tableName, TokenCredential credential)
        {
            TableServiceUri = tableServiceUri;
            TableName = tableName;
            Credential = credential;
            return table;
        }
    }
}
