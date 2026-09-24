namespace Skyline.DataMiner.CICD.Tools.AKA.Lib
{
    using System;

    using Azure.Core;

    /// <summary>
    /// Factory for creating <see cref="IUrlShortenerTable"/> instances.
    /// </summary>
    public interface IUrlShortenerTableFactory
    {
        /// <summary>
        /// Creates a table client for the given service URI, table name, and token credential.
        /// </summary>
        /// <param name="tableServiceUri">The Azure Table Storage service URI.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="credential">The Azure token credential.</param>
        /// <returns>The table abstraction.</returns>
        IUrlShortenerTable Create(Uri tableServiceUri, string tableName, TokenCredential credential);
    }
}
