namespace AdoNetCore.AseClient.Interface
{
    internal interface IConnectionParameters
    {
        string Server { get; }
        int Port { get; }
        string Database { get; }
        string Username { get; }
        string Password { get; }
        string ProcessId { get; }
        string ApplicationName { get; }
        string ClientHostName { get; }
        string ClientHostProc { get; }
        string Charset { get; }
        bool Pooling { get; }
        short MaxPoolSize { get; }
        short MinPoolSize { get; }
        int LoginTimeout { get; } //login timeout in seconds
        short ConnectionIdleTimeout { get; } //how long a connection may be idle before being dropped/replaced. 0 = indefinite
        short ConnectionLifetime { get; } //how long a connection may live before being dropped/replaced. 0 = indefinite
        bool PingServer { get; } //in pooling, ping the server before returning from the pool
        ushort PacketSize { get; }
        int TextSize { get; }
        bool UseAseDecimal { get; }
        bool EncryptPassword { get; }
        bool Encryption { get; }
        string TrustedFile { get; }
        bool AnsiNull { get; }
    }
}
