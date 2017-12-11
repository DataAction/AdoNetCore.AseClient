using System;
using System.Data;
using System.Threading;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IInternalConnection : IDisposable
    {
        DateTime Created { get; }

        DateTime LastActive { get; set; }

        bool Ping();

        void ChangeDatabase(string databaseName, CancellationToken? token = null);

        string Database { get; }

        int ExecuteNonQuery(AseCommand command, AseTransaction transaction);

        AseDataReader ExecuteReader(CommandBehavior behavior, AseCommand command, AseTransaction transaction);

        object ExecuteScalar(AseCommand command, AseTransaction transaction);

        void SetTextSize(int textSize);

        bool IsDoomed { get; set; }
        bool IsDisposed { get; }
    }
}
