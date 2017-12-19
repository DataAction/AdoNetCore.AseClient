using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IInternalConnection : IDisposable
    {
        /// <summary>
        /// Marks when the connection was 'officially' created
        /// * 'officially' being whatever the factory decides is appropriate, such as "after login is complete"
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Marks when the connection's socket was last active
        /// </summary>
        DateTime LastActive { get; }

        /// <summary>
        /// Try to send a packet to the server and read the response. Report the result back
        /// </summary>
        /// <returns>true if the ping succeeded, false otherwise</returns>
        bool Ping();

        /// <summary>
        /// Change the current connection/session to the desired database
        /// </summary>
        void ChangeDatabase(string databaseName);

        /// <summary>
        /// Get the current connection/session's database
        /// Note: this value is cached, and is whatever the server last reported
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Get the current connection/session's data source (server name/address)
        /// </summary>
        string DataSource { get; }

        /// <summary>
        /// Get the current connection/session's server version
        /// </summary>
        string ServerVersion { get; }

        /// <summary>
        /// Internal implementation of <see cref="IDbCommand.ExecuteNonQuery"/>
        /// </summary>
        int ExecuteNonQuery(AseCommand command, AseTransaction transaction);
        
        /// <summary>
        /// Internal implementation of <see cref="IDbCommand.ExecuteNonQuery"/>,
        /// but the result is wrapped in a Task to allow the caller to check IsCanceled
        /// </summary>
        Task<int> ExecuteNonQueryTaskRunnable(AseCommand command, AseTransaction transaction);


        /// <summary>
        /// Internal implementation of <see cref="IDbCommand.ExecuteReader()"/>
        /// </summary>
        DbDataReader ExecuteReader(CommandBehavior behavior, AseCommand command, AseTransaction transaction);

        /// <summary>
        /// Internal implementation of <see cref="IDbCommand.ExecuteReader()"/>,
        /// but the result is wrapped in a Task to allow the caller to check IsCanceled
        /// </summary>
        Task<DbDataReader> ExecuteReaderTaskRunnable(CommandBehavior behavior, AseCommand command, AseTransaction transaction);

        /// <summary>
        /// Internal implementation of <see cref="IDbCommand.ExecuteScalar"/>
        /// </summary>
        object ExecuteScalar(AseCommand command, AseTransaction transaction);

        /// <summary>
        /// Cancel the currently running command
        /// </summary>
        void Cancel();

        /// <summary>
        /// Set @@textsize, useful if the caller needs to select out very large text data
        /// </summary>
        /// <param name="textSize">The new maximum in bytes</param>
        void SetTextSize(int textSize);

        /// <summary>
        /// Indicates if this connection is doomed to destruction
        /// </summary>
        bool IsDoomed { get; set; }

        /// <summary>
        /// Indicates if this connection has already been disposed
        /// </summary>
        bool IsDisposed { get; }
    }
}
