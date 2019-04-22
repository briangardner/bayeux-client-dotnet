using System;

namespace Genesys.Bayeux.Extensions.ReplayId.Options
{
    public class SqlServerCacheOptions
    {
        public string ConnectionString { get; set; }

        /// <summary>
        /// Schema to use for SQL Server cache.  Only needed if EnableSqlCache = true
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Table to use for SQL Server cache.  Only needed if EnableSqlCache = true
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// Default Expiration of cache entries in seconds. Only needed if EnableSqlCache = true.
        /// </summary>
        public int ExpirationInSeconds { get; set; }

        public void ThrowIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentNullException(nameof(ConnectionString), "Please provide a SQL Connection String for the cache");
            }
            if (string.IsNullOrWhiteSpace(Schema))
            {
                throw new ArgumentNullException(nameof(Schema), "Please provide a database Schema for caching");
            }
            if (string.IsNullOrWhiteSpace(Table))
            {
                throw new ArgumentNullException(nameof(Table), "Please provide a database Table for caching");
            }
            if (ExpirationInSeconds <= 0)
            {
                throw new ArgumentNullException(nameof(ExpirationInSeconds), "Please provide a positive integer for ExpirationInSeconds");
            }

        }
    }
}
