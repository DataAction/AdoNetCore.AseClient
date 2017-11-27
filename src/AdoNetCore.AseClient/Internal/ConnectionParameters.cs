using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdoNetCore.AseClient.Internal
{
    public class ConnectionParameters
    {
        //Cache the current process details, expensive call
        private static readonly Process CurrentProcess = Process.GetCurrentProcess();

        private static readonly Dictionary<string, Action<ConnectionParameters, string>> _parsers = new Dictionary<string, Action<ConnectionParameters, string>>(StringComparer.OrdinalIgnoreCase)
        {
            {"Data Source", (p, v) => { /*todo: need to parse Data Source values that contain the port*/p.Server = v; } },
            {"Port", (p, v) => { p.Port = Convert.ToInt32(v); } },
            {"Database", (p, v) => { p.Database = v; } },
            {"Uid", (p, v) => { p.Username = v; } },
            {"Pwd", (p, v) => { p.Password = v; } },
            {"Charset", (p, v) => { p.Charset= v; } },
            {"Pooling", (p, v) => { p.Pooling = Convert.ToBoolean(v); } },
            {"ApplicationName", (p, v) => { p.ApplicationName = v; } },
            {"ClientHostName", (p, v) => { p.ClientHostName = v; } },
        };

        public static ConnectionParameters Parse(string connectionString)
        {
            var parameters = new ConnectionParameters();
            
            //todo: this implementation may be too naiive - how do we handle for values which contain ';' or '=' ?
            foreach (var item in connectionString.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = item.Split(new[] {'='}, 2).ToArray();
                if (pair.Length == 2 && _parsers.ContainsKey(pair[0]))
                {
                    _parsers[pair[0]](parameters, pair[1]);
                }
            }

            return parameters;
        }

        public string Server { get; private set; }
        public int Port { get; private set; } = 5000;
        public string Database{ get; private set; }
        public string Username { get; private set; }
        public string Password{ get; private set; }
        public string ProcessId { get; private set; } = CurrentProcess.Id.ToString();
        public string ApplicationName { get; private set; } = CurrentProcess.ProcessName;
        public string ClientHostName { get; private set; } = Environment.MachineName;
        public string Charset { get; private set; } = "iso_1";
        public bool Pooling { get; private set; } = true;
    }
}
