namespace Skyline.DataMiner.CICD.Tools.AKA.LibTests
{
    using Skyline.DataMiner.CICD.Tools.AKA.Lib;

    internal sealed class FakeUrlShortenerTableFactory : IUrlShortenerTableFactory
    {
        private readonly FakeUrlShortenerTable table;

        public FakeUrlShortenerTableFactory(FakeUrlShortenerTable table)
        {
            this.table = table;
        }

        public IUrlShortenerTable Create(string storageConnectionString, string tableName)
        {
            return table;
        }
    }
}
