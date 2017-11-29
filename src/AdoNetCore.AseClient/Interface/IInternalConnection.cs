using System;

namespace AdoNetCore.AseClient.Interface
{
    public interface IInternalConnection : IDisposable
    {
        void ChangeDatabase(string databaseName);
        string Database { get; }

        int ExecuteNonQuery(AseCommand command);
    }
}