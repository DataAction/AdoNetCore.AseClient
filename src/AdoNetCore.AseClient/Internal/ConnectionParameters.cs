using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    internal class ConnectionParameters : IConnectionParameters
    {
        //Cache the current process details, expensive call
        private static readonly Process CurrentProcess = Process.GetCurrentProcess();
        private static readonly Regex DsUrlRegex = new Regex(
            @"^file://(?<path>.+" + Regex.Escape(new string(new[] { Path.DirectorySeparatorChar })) + ")?(?<filename>[^" + Regex.Escape(new string(new[] { Path.DirectorySeparatorChar })) + "?]+)(?:[?](?<servicename>.+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Dictionary<string, Action<ConnectionStringItem, ConnectionParameters>> PropertyMap = new Dictionary<string, Action<ConnectionStringItem, ConnectionParameters>>(StringComparer.OrdinalIgnoreCase)
        {
            {"Server", ParseServer},
            {"Data Source", ParseServer},
            {"DataSource", ParseServer},
            {"Address", ParseServer},
            {"Addr", ParseServer},
            {"Network Address", ParseServer},
            {"Server Name", ParseServer},
            {"Port", ParsePort},
            {"Server Port", ParsePort},
            {"Db", ParseDatabase},
            {"Database", ParseDatabase},
            {"Initial Catalog", ParseDatabase},
            {"UID", ParseUsername},
            {"User ID", ParseUsername},
            {"UserID", ParseUsername},
            {"User", ParseUsername},
            {"Pwd", ParsePassword},
            {"Password", ParsePassword},
            {"Charset", ParseCharset},
            {"Pooling", ParsePooling},
            {"Max Pool Size", ParseMaxPoolSize},
            {"Min Pool Size", ParseMinPoolSize},
            {"ApplicationName", ParseApplicationName},
            {"Application Name", ParseApplicationName},
            {"ClientHostName", ParseClientHostName},
            {"ClientHostProc", ParseClientHostProc},
            {"Ping Server", ParsePingServer},
            {"LoginTimeOut", ParseLoginTimeout},
            {"Connect Timeout", ParseLoginTimeout},
            {"Connection Timeout", ParseLoginTimeout},
            {"ConnectionIdleTimeout", ParseConnectionIdleTimeout},
            {"Connection IdleTimeout", ParseConnectionIdleTimeout},
            {"Connection Idle Timeout", ParseConnectionIdleTimeout},
            {"ConnectionLifetime", ParseConnectionLifetime},
            {"Connection Lifetime", ParseConnectionLifetime},
            {"PacketSize", ParsePacketSize},
            {"Packet Size", ParsePacketSize},
            {"TextSize", ParseTextSize},
            {"UseAseDecimal", ParseUseAseDecimal},
            {"EncryptPassword", ParseEncryptPassword},
            {"AnsiNull", ParseAnsiNull},
            {"EnableServerPacketSize", ParseEnableServerPacketSize},
        };

        public static ConnectionParameters Parse(string connectionString)
        {
            var connectionStringTokeniser = new ConnectionStringTokeniser();

            var result = new ConnectionParameters();

            string dsUrl = null;
            foreach (var item in connectionStringTokeniser.Tokenise(connectionString))
            {
                if (item.PropertyName.Equals("DSURL", StringComparison.OrdinalIgnoreCase)
                    || item.PropertyName.Equals("Directory Service URL", StringComparison.OrdinalIgnoreCase))
                {
                    dsUrl = item.PropertyValue;
                }
                else if (PropertyMap.ContainsKey(item.PropertyName))
                {
                    PropertyMap[item.PropertyName](item, result);
                }
            }

            ProcessDsUrl(dsUrl, result);

            ValidateConnectionParameters(result);

            return result;
        }

        private static void ProcessDsUrl(string dsUrl, ConnectionParameters result)
        {
            if (!string.IsNullOrWhiteSpace(dsUrl))
            {
                // file://[path]<filename>[?][servicename]
                var match = DsUrlRegex.Match(dsUrl);
                if (match.Success)
                {
                    var path = match.Groups["path"].Value;
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        var sybaseEnvironmentVariablePath = @"%SYBASE%\ini";
                        var resolvedSybasePath = Environment.ExpandEnvironmentVariables(sybaseEnvironmentVariablePath);

                        if (!string.Equals(sybaseEnvironmentVariablePath, resolvedSybasePath))
                        {
                            path = resolvedSybasePath;
                        }
                    }

                    // If we got a path...
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var filename = match.Groups["filename"].Value;
                        var servicename = match.Groups["servicename"].Value;

                        if (string.IsNullOrWhiteSpace(servicename))
                        {
                            servicename = result.Server;
                        }

                        var fullpath = Path.Combine(path, filename);

                        var iniReader = new IniReader();
                        var iniEntry = iniReader.Query(fullpath, servicename);

                        if (iniEntry != null)
                        {
                            result.Server = iniEntry.HostName;
                            result.Port = iniEntry.Port;
                        }
                    }
                }
            }
        }

        private static void ParseServer(ConnectionStringItem item, ConnectionParameters result)
        {
            if (string.IsNullOrWhiteSpace(item.PropertyValue))
            {
                return;
            }
            var parts = item.PropertyValue.Split(',', ':');

            result.Server = parts[0];

            if (parts.Length > 1)
            {
                result.Port = Convert.ToInt32(parts[1]);
            }
        }

        private static void ParsePort(ConnectionStringItem item, ConnectionParameters result)
        {
            result.Port = Convert.ToInt32(item.PropertyValue);
        }

        private static void ParseDatabase(ConnectionStringItem item, ConnectionParameters result)
        {
            result.Database = item.PropertyValue;
        }

        private static void ParseUsername(ConnectionStringItem item, ConnectionParameters result)
        {
            result.Username = item.PropertyValue;
        }

        private static void ParsePassword(ConnectionStringItem item, ConnectionParameters result)
        {
            result.Password = item.PropertyValue;
        }

        private static void ParseCharset(ConnectionStringItem item, ConnectionParameters result)
        {
            result.Charset = item.PropertyValue;
        }

        private static void ParsePooling(ConnectionStringItem item, ConnectionParameters result)
        {
            result.Pooling = Convert.ToBoolean(item.PropertyValue);
        }

        private static void ParseMaxPoolSize(ConnectionStringItem item, ConnectionParameters result)
        {
            result.MaxPoolSize = Convert.ToInt16(item.PropertyValue);
        }

        private static void ParseMinPoolSize(ConnectionStringItem item, ConnectionParameters result)
        {
            result.MinPoolSize = Convert.ToInt16(item.PropertyValue);
        }

        private static void ParseApplicationName(ConnectionStringItem item, ConnectionParameters result)
        {
            result.ApplicationName = item.PropertyValue;
        }

        private static void ParseClientHostName(ConnectionStringItem item, ConnectionParameters result)
        {
            result.ClientHostName = item.PropertyValue;
        }

        private static void ParseClientHostProc(ConnectionStringItem item, ConnectionParameters result)
        {
            result.ClientHostProc = item.PropertyValue;
        }

        private static void ParsePingServer(ConnectionStringItem item, ConnectionParameters result)
        {
            result.PingServer = Convert.ToBoolean(item.PropertyValue);
        }

        private static void ParseLoginTimeout(ConnectionStringItem item, ConnectionParameters result)
        {
            result.LoginTimeout = Convert.ToInt32(item.PropertyValue);
        }

        private static void ParseConnectionIdleTimeout(ConnectionStringItem item, ConnectionParameters result)
        {
            result.ConnectionIdleTimeout = Convert.ToInt16(item.PropertyValue);
        }

        private static void ParseConnectionLifetime(ConnectionStringItem item, ConnectionParameters result)
        {
            result.ConnectionLifetime = Convert.ToInt16(item.PropertyValue);
        }

        private static void ParsePacketSize(ConnectionStringItem item, ConnectionParameters result)
        {
            result.PacketSize = Convert.ToUInt16(item.PropertyValue);
        }

        private static void ParseTextSize(ConnectionStringItem item, ConnectionParameters result)
        {
            result.TextSize = Convert.ToInt32(item.PropertyValue);
        }

        private static void ParseUseAseDecimal(ConnectionStringItem item, ConnectionParameters result)
        {
            if (bool.TryParse(item.PropertyValue, out var parsedBool))
            {
                result.UseAseDecimal = parsedBool;
            }
            else if (int.TryParse(item.PropertyValue, out var parsedInt))
            {
                result.UseAseDecimal = parsedInt != 0;
            }
            else
            {
                result.UseAseDecimal = Convert.ToBoolean(item.PropertyValue);
            }
        }

        private static void ParseEncryptPassword(ConnectionStringItem item, ConnectionParameters result)
        {
            if (bool.TryParse(item.PropertyValue, out var parsedBool))
            {
                result.EncryptPassword = parsedBool;
            }
            else if (int.TryParse(item.PropertyValue, out var parsedInt))
            {
                result.EncryptPassword = parsedInt != 0;
            }
            else
            {
                result.EncryptPassword = Convert.ToBoolean(item.PropertyValue);
            }
        }

        private static void ParseAnsiNull(ConnectionStringItem item, ConnectionParameters result)
        {
            result.AnsiNull = Convert.ToInt32(item.PropertyValue) == 1;
        }

        private static void ParseEnableServerPacketSize(ConnectionStringItem item, ConnectionParameters result)
        {
            if (int.TryParse(item.PropertyValue?.Trim(), out var intValue))
            {
                result.EnableServerPacketSize = intValue != 0;
            }
            else if (bool.TryParse(item.PropertyValue?.Trim(), out var boolValue))
            {
                result.EnableServerPacketSize = boolValue;
            }
        }
        

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ValidateConnectionParameters(ConnectionParameters result)
        {
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
        public string Charset { get; private set; }
        public bool Pooling { get; private set; } = true;
        public short MaxPoolSize { get; private set; } = 100;
        public short MinPoolSize { get; private set; }
        public int LoginTimeout { get; private set; } = 15; //login timeout in seconds
        public short ConnectionIdleTimeout { get; private set; } //how long a connection may be idle before being dropped/replaced. 0 = indefinite
        public short ConnectionLifetime { get; private set; } //how long a connection may live before being dropped/replaced. 0 = indefinite
        public bool PingServer { get; private set; } = true; //in pooling, ping the server before returning from the pool
        public ushort PacketSize { get; private set; } = 512;
        public int TextSize { get; private set; } = 32768;
        public bool UseAseDecimal { get; private set; }

        public bool EncryptPassword { get; private set; }
        public bool AnsiNull { get; private set; }
        public bool EnableServerPacketSize { get; private set; } = true;
    }
}
