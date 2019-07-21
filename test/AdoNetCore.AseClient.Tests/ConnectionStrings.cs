using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests
{
    public static class ConnectionStrings
    {
        public static string Default => Pooled;
        public static string NonPooled => $"{Prefix}; Pooling=false; LoginTimeOut=1;";
        public static string Pooled => $"{Prefix}; Pooling=true; LoginTimeOut=1; Max Pool Size=32;";
        public static string EncryptPassword1 => $"{Prefix}; Pooling=true; LoginTimeOut=1; Max Pool Size=32; EncryptPassword=1;";
        public static string EncryptPassword2 => $"{Prefix}; Pooling=true; LoginTimeOut=1; Max Pool Size=32; EncryptPassword=2;";
        public static string Pooled10 => $"{Prefix}; Pooling=true; LoginTimeOut=1; Max Pool Size=10;";
        public static string Pooled100 => $"{Prefix}; Pooling=true; LoginTimeOut=1; Max Pool Size=100;";
        public static string PooledUtf8 => Pooled;
        public static string Cp850 => $"{PrefixNoCharSet}; charset=cp850;";
        public static string BigPacketSize => $"{Prefix}; Pooling=true; LoginTimeOut=1; PacketSize=2048;";
        public static string BigTextSize => $"{Prefix}; Pooling=true; LoginTimeOut=1; TextSize=131072;";
        public static string AseDecimalOn => $"{Prefix}; Pooling=true; LoginTimeOut=1; UseAseDecimal=1;";
        public static string NonPooledUnique => $"{NonPooled}; UniqueID={{{Guid.NewGuid()}}}";
        public static string PooledUnique => $"{Pooled}; UniqueID={{{Guid.NewGuid()}}}";
        public static string BadPass => $"Data Source={Server}; Port={Port}; Uid={User}; Pwd=XXXXXXXX; db={Database};";
        public static string EnableServerPacketSize => $"{Prefix}; EnableServerPacketSize=0;";

        public static string AnsiNullOn => $"{Prefix}; AnsiNull=1";
        public static string NamedParametersOff => $"{Prefix}; NamedParameters=false";
        public static string AnsiNullOff => $"{Prefix}; AnsiNull=0";
        public static string Tls => $"Data Source={TlsHostname}; Port={TlsPort}; Uid={User}; Pwd={Pass}; db={Database}; charset={Charset}; Encryption=ssl";

        private static IDictionary<string, string> _loginDetails;
        private static IDictionary<string, string> LoginDetails => _loginDetails ?? (_loginDetails = Load());
        private static string Server => LoginDetails["Server"];
        private static string Port => LoginDetails["Port"];
        private static string TlsPort => LoginDetails["TlsPort"];
        public static string TlsHostname => LoginDetails["TlsHostname"];
        public static string TlsTrustedText => LoginDetails["TlsTrustedText"];
        private static string Database => LoginDetails["Database"];
        private static string User => LoginDetails["User"];
        private static string Pass => LoginDetails["Pass"];
        private static string Charset => LoginDetails.ContainsKey("Charset") ? LoginDetails["Charset"] : "utf8";
        private static string Prefix => $"Data Source={Server}; Port={Port}; Uid={User}; Pwd={Pass}; db={Database}; charset={Charset}";
        private static string PrefixNoCharSet => $"Data Source={Server}; Port={Port}; Uid={User}; Pwd={Pass}; db={Database}";
        

        private const string DocoLocation = "https://github.com/DataAction/AdoNetCore.AseClient/wiki/Running-the-integration-tests";
        private static IDictionary<string, string> Load()
        {
            string fileText;

            try
            {
#if NET_FRAMEWORK
                fileText = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "DatabaseLoginDetails.json"));
#else
                fileText = File.ReadAllText("DatabaseLoginDetails.json");
#endif
            }
            catch (Exception e)
            {
                throw new AssertionException($"Failed to load a 'DatabaseLoginDetails.json' file. This file must be defined at the root of the test project. See {DocoLocation} for details.", e);
            }

            var results = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileText);

            Assert.IsTrue(results.ContainsKey("Server"), "The specified 'DatabaseLoginDetails.json' file does not define a property called 'Server'");
            Assert.IsTrue(results.ContainsKey("Port"), "The specified 'DatabaseLoginDetails.json' file does not define a property called 'Port'");
            Assert.IsTrue(results.ContainsKey("Database"), "The specified 'DatabaseLoginDetails.json' file does not define a property called 'Database'");
            Assert.IsTrue(results.ContainsKey("User"), "The specified 'DatabaseLoginDetails.json' file does not define a property called 'User'");
            Assert.IsTrue(results.ContainsKey("Pass"), "The specified 'DatabaseLoginDetails.json' file does not define a property called 'Pass'");

            return results;
        }
    }
}
