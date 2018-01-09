using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests
{
    /// <summary>
    /// Utility class to load the test connection strings from a ConnectionStrings.json that the developer must define in the root of the test project.
    /// </summary>
    /// <seealso cref="http://github.com/DataAction/AdoNetCore.AseClient/wiki/Running-the-integration-tests"/>
    internal static class ConnectionStringLoader
    {
        private const string DocoLocation = "https://github.com/DataAction/AdoNetCore.AseClient/wiki/Running-the-integration-tests";

        public static IDictionary<string, string> Load()
        {
            string fileText;

            try
            {
                fileText = File.ReadAllText("ConnectionStrings.json");
            }
            catch (Exception e)
            {
                throw new AssertionException($"Failed to load a 'ConnectionStrings.json' file. This file must be defined at the root of the test project. See {DocoLocation} for details.", e);
            }

            var results = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileText);

            Assert.IsTrue(results.ContainsKey("default"), "The specified 'ConnectionStrings.json' file does not define a connection string called 'default'.");
            Assert.IsTrue(results.ContainsKey("pooled"), "The specified 'ConnectionStrings.json' file does not define a connection string called 'pooled'.");
            Assert.IsTrue(results.ContainsKey("big-packetsize"), "The specified 'ConnectionStrings.json' file does not define a connection string called 'big-packetsize'.");
            Assert.IsTrue(results.ContainsKey("badpass"), "The specified 'ConnectionStrings.json' file does not define a connection string called 'badpass'.");

            return results;
        }
    }
}
