using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class ConnectionStringTokeniser
    {
        private static readonly Regex TokenRegex = new Regex(
            @"^(\s|;)*" + // Ignore leading semi-colons and whitespace.
            @"(" + // Begin property section.
            @"(?<property>" + // Begin property capture.
            @"(?<name>[^=]+)" + // Get the property name
            @"\s*" + // Ignore any whitespace
            @"=" + // A literal equals
            @"\s*" + // Ignore any whitespace
            @"((?<value>[""][^""]+[""])|(?<value>['][^']+['])|(?<value>([^;]|$)+))" + // Get the property value - which can be enclosed in single or double quotes
            @")" + // End of property capture.
            @"(\s|;)*" + // Ignore leading semi-colons and whitespace.
            @"((;*)|$)" + // Match a semi-colon or the end of the line
            @")+" // End property section
            , RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public IEnumerable<ConnectionStringItem> Tokenise(string connectionString)
        {

            foreach (System.Text.RegularExpressions.Match item in TokenRegex.Matches(connectionString))
            {
                if (item.Success)
                {
                    var nameGroupCaptures = item.Groups["name"].Captures;
                    var valueGroupCaptures = item.Groups["value"].Captures;

                    var matchCount = Math.Min(nameGroupCaptures.Count, valueGroupCaptures.Count);

                    for (int i = 0; i < matchCount; i++)
                    {
                        var name = nameGroupCaptures[i].Value.Trim();
                        var value = valueGroupCaptures[i].Value.Trim();

                        if ((value.StartsWith("'") && value.EndsWith("'")) ||
                            (value.StartsWith("\"") && value.EndsWith("\"")))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }

                        yield return new ConnectionStringItem(name, value);
                    }
                }
                else
                {
                    throw new ArgumentException("Badly formatted connection string encountered");
                }
            }
        }
    }
}