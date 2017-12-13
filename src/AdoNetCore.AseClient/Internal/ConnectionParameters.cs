using System;
using System.Diagnostics;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class ConnectionParameters : IConnectionParameters
    {
        //Cache the current process details, expensive call
        private static readonly Process CurrentProcess = Process.GetCurrentProcess();

        public static ConnectionParameters Parse(string connectionString)
        {
            var connectionStringTokeniser = new ConnectionStringTokeniser();
            
            var result = new ConnectionParameters();

            foreach (var item in connectionStringTokeniser.Tokenise(connectionString))
            {
                if (item.PropertyName.Equals("Data Source", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("DataSource", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(item.PropertyValue))
                    {
                        continue;
                    }
                    var parts = item.PropertyValue.Split(',', ':');

                    result.Server = parts[0];

                    if (parts.Length > 1)
                    {
                        result.Port = Convert.ToInt32(parts[1]);
                    }
                }
                else if (item.PropertyName.Equals("Port", StringComparison.OrdinalIgnoreCase))
                {
                    result.Port = Convert.ToInt32(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("Db", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("Database", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("Initial Catalog", StringComparison.OrdinalIgnoreCase))
                {
                    result.Database = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("Uid", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("User Id", StringComparison.OrdinalIgnoreCase))
                {
                    result.Username = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("Pwd", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("Password", StringComparison.OrdinalIgnoreCase))
                {
                    result.Password = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("Charset", StringComparison.OrdinalIgnoreCase))
                {
                    result.Charset = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("Pooling", StringComparison.OrdinalIgnoreCase))
                {
                    result.Pooling = Convert.ToBoolean(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("Max Pool Size", StringComparison.OrdinalIgnoreCase))
                {
                    result.MaxPoolSize = Convert.ToInt16(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("Min Pool Size", StringComparison.OrdinalIgnoreCase))
                {
                    result.MinPoolSize = Convert.ToInt16(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("ApplicationName", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("Application Name", StringComparison.OrdinalIgnoreCase))
                {
                    result.ApplicationName = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("ClientHostName", StringComparison.OrdinalIgnoreCase))
                {
                    result.ClientHostName = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("ClientHostProc", StringComparison.OrdinalIgnoreCase))
                {
                    result.ClientHostProc = item.PropertyValue;
                }
                else if (item.PropertyName.Equals("Ping Server", StringComparison.OrdinalIgnoreCase))
                {
                    result.PingServer = Convert.ToBoolean(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("LoginTimeOut", StringComparison.OrdinalIgnoreCase))
                {
                    result.LoginTimeout = Convert.ToInt32(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("ConnectionIdleTimeout", StringComparison.OrdinalIgnoreCase))
                {
                    result.ConnectionIdleTimeout = Convert.ToInt16(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("ConnectionLifetime", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("Connection Lifetime", StringComparison.OrdinalIgnoreCase))
                {
                    result.ConnectionLifetime = Convert.ToInt16(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("PacketSize", StringComparison.OrdinalIgnoreCase))
                {
                    result.PacketSize = Convert.ToUInt16(item.PropertyValue);
                }
                else if (item.PropertyName.Equals("TextSize", StringComparison.OrdinalIgnoreCase))
                {
                    result.TextSize = Convert.ToInt32(item.PropertyValue);
                }
            }

            if (string.IsNullOrWhiteSpace(result.Server))
            {
                throw new ArgumentException("Data Source not specified");
            }

            if (result.Port <= 0 || result.Port > ushort.MaxValue)
            {
                throw new ArgumentException("Valid port not specified");
            }

            if (string.IsNullOrWhiteSpace(result.Username))
            {
                throw new ArgumentException("Uid not specified"); // TODO - is this mandatory? What about Integrated Security?
            }

            if (string.IsNullOrWhiteSpace(result.Database))
            {
                throw new ArgumentException("Database not specified");
            }

            if (result.LoginTimeout < 1)
            {
                throw new ArgumentException("Login timeout must be at least 1 second");
            }

            if (result.ConnectionIdleTimeout < 0)
            {
                throw new ArgumentException("ConnectionIdleTimeout must be at least 0 seconds");
            }

            if (result.ConnectionLifetime < 0)
            {
                throw new ArgumentException("Connection Lifetime must be at least 0 seconds");
            }

            if (result.PacketSize < 256)
            {
                throw new ArgumentException("PacketSize must be at least 256 (bytes)");
            }

            if (result.Pooling && result.MaxPoolSize <= 0)
            {
                throw new ArgumentException("Max Pool Size must be at least 1 when Pooling is enabled");
            }

            if (result.MinPoolSize > result.MaxPoolSize)
            {
                throw new ArgumentException("Min Pool Size must be at most the same as Max Pool Size");
            }

            return result;
        }

        public string Server { get; private set; } = string.Empty;
        public int Port { get; private set; } = 5000;
        public string Database { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public string ProcessId { get; private set; } = CurrentProcess.Id.ToString();
        public string ApplicationName { get; private set; } = CurrentProcess.ProcessName;
        public string ClientHostName { get; private set; } = Environment.MachineName;
        public string ClientHostProc { get; private set; } = string.Empty;
        public string Charset { get; private set; } = "iso_1";
        public bool Pooling { get; private set; } = true;
        public short MaxPoolSize { get; private set; } = 100;
        public short MinPoolSize { get; private set; }
        public int LoginTimeout { get; private set; } = 15; //login timeout in seconds
        public short ConnectionIdleTimeout { get; private set; } //how long a connection may be idle before being dropped/replaced. 0 = indefinite
        public short ConnectionLifetime { get; private set; } //how long a connection may live before being dropped/replaced. 0 = indefinite
        public bool PingServer { get; private set; } = true; //in pooling, ping the server before returning from the pool
        public ushort PacketSize { get; private set; } = 512;
        public int TextSize { get; private set; } = 32768;
    }
}
