namespace AdoNetCore.AseClient.Interface
{
    public interface IInternalConnection
    {
        void ChangeDatabase(string databaseName);
        string Database { get; }
        //todo: add things related to command/query execution
    }
}