#if DB_PROVIDERFACTORY
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// The AseClientFactory is a utility class that can decouple an application from any ASE-specific details.
    /// </summary>
    public sealed class AseClientFactory : DbProviderFactory
    {
        /// <summary>
        /// The singleton instance of the factory.
        /// </summary>
        public static readonly AseClientFactory Instance = new AseClientFactory();

        /// <summary>
        /// Private constructor function.
        /// </summary>
        private AseClientFactory()
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="AseCommand"/>.
        /// </summary>
        /// <returns>A new <see cref="AseCommand"/>.</returns>
        public override DbCommand CreateCommand()
        {
            return new AseCommand();
        }

        /// <summary>
        /// Instantiates a new <see cref="AseConnection"/>.
        /// </summary>
        /// <returns>A new <see cref="AseConnection"/>.</returns>
        public override DbConnection CreateConnection()
        {
            return new AseConnection();
        }

        /// <summary>
        /// Instantiates a new <see cref="AseConnectionStringBuilder"/>.
        /// </summary>
        /// <returns>A new <see cref="AseConnectionStringBuilder"/>.</returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new AseConnectionStringBuilder();
        }

#if SYSTEM_DATA_COMMON_EXTENSIONS
        /// <summary>
        /// Instantiates a new <see cref="AseCommandBuilder"/>.
        /// </summary>
        /// <returns>A new <see cref="AseCommandBuilder"/>.</returns>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new AseCommandBuilder();
        }
        /// <summary>
        /// Instantiates a new <see cref="AseDataAdapter"/>.
        /// </summary>
        /// <returns>A new <see cref="AseDataAdapter"/>.</returns>
        public override DbDataAdapter CreateDataAdapter()
        {
            return new AseDataAdapter();
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="AseParameter"/>.
        /// </summary>
        /// <returns>A new <see cref="AseParameter"/>.</returns>
        public override DbParameter CreateParameter()
        {
            return new AseParameter();
        }

        /// <summary>
        /// Instantiates a new <see cref="AseDataSourceEnumerator"/>.
        /// </summary>
        /// <returns>A new <see cref="AseDataSourceEnumerator"/>.</returns>
        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return new AseDataSourceEnumerator();
        }

        /// <summary>
        /// Whether or not the <see cref="CreateDataSourceEnumerator"/> method is implemented. Always False.
        /// </summary>
        public override bool CanCreateDataSourceEnumerator => false;
    }
}
#endif