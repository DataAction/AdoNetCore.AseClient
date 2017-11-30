using System;
using System.Data;

namespace AdoNetCore.AseClient.Interface
{
    internal interface IInternalConnection : IDisposable
    {
        void ChangeDatabase(string databaseName);
        string Database { get; }

        int ExecuteNonQuery(AseCommand command);

        AseDataReader ExecuteReader(CommandBehavior behavior, AseCommand command);
        object ExecuteScalar(AseCommand command);
    }
}