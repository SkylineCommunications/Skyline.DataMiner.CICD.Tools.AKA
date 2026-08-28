namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    /// <summary>
    /// Factory for creating <see cref="IUrlShortenerTable"/> instances.
    /// </summary>
    public interface IUrlShortenerTableFactory
    {
        /// <summary>
        /// Creates a table client for the given connection string and table name.
        /// </summary>
        /// <param name="storageConnectionString">The storage connection string.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>The table abstraction.</returns>
        IUrlShortenerTable Create(string storageConnectionString, string tableName);
    }
}
