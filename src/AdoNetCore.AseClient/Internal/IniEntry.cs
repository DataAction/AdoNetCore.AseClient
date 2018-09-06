namespace AdoNetCore.AseClient.Internal
{
    internal sealed class IniEntry
    {
        public string DriverName { get; private set; }
        public string HostName { get; private set; }
        public int Port { get; private set; }

        public IniEntry(string driverName, string hostName, int port)
        {
            DriverName = driverName;
            HostName = hostName;
            Port = port;
        }
    }
}
