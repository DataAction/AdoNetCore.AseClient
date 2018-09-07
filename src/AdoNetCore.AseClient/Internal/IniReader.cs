using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class IniReader
    {
        private static readonly Regex IniEntryRegex = new Regex(@"^\s*query\s*=\s*(?<drivername>.+),(?<hostname>.+),(?<port>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ServiceRegex = new Regex(@"^\s*\[", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public IniEntry Query(string path, string serviceName)
        {
            if (!File.Exists(path))
            {
                throw new AseException(new AseError { IsError = true, IsFromClient = true, Message = "The path could not be resolved to an .ini file." });
            }

            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        return string.IsNullOrWhiteSpace(serviceName)
                            // If no serviceName is provided, get the first service, if there is precisely one service in the file.
                            // http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc20155.1500/html/newfesd/newfesd182.htm
                            ? QueryFirstService(reader)
                            // Find the service in the file and use that.
                            : QueryServiceByServiceName(reader, serviceName);
                    }
                }
            }
            catch (Exception e)
            {
                throw new AseException(e, new AseError { IsError = true, IsFromClient = true, Message = "The .ini file at the specified path could not be opened." });
            }
        }

        private IniEntry QueryFirstService(StreamReader reader)
        {
            IniEntry result = null;
            while (!reader.EndOfStream)
            {
                var serviceNameLine = reader.ReadLine();

                if (ServiceRegex.IsMatch(serviceNameLine))
                {
                    while (!reader.EndOfStream)
                    {
                        var dataLine = reader.ReadLine().Trim();

                        // If we are still reading data.
                        if (!ServiceRegex.IsMatch(dataLine))
                        {
                            var match = IniEntryRegex.Match(dataLine);

                            if (result != null)
                            {
                                throw new AseException(new AseError { IsError = true, IsFromClient = true, Message = "Getting more than one server with the connection string." });
                            }
                            if (match.Success)
                            {
                                result = new IniEntry(match.Groups["drivername"].Value,
                                    match.Groups["hostname"].Value,
                                    Convert.ToInt32(match.Groups["port"].Value));
                            }
                        }
                    }
                }
            }

            return result;
        }

        private IniEntry QueryServiceByServiceName(StreamReader reader, string serviceName)
        {
            var serviceNameRegex = new Regex($@"^\s*\[{serviceName}\]\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            while (!reader.EndOfStream)
            {
                var serviceNameLine = reader.ReadLine();

                if (serviceNameRegex.IsMatch(serviceNameLine))
                {
                    while (!reader.EndOfStream)
                    {
                        var dataLine = reader.ReadLine().Trim();

                        // If we are still reading data.
                        if (!ServiceRegex.IsMatch(dataLine))
                        {
                            var match = IniEntryRegex.Match(dataLine);

                            if (match.Success)
                            {
                                return new IniEntry(match.Groups["drivername"].Value,
                                    match.Groups["hostname"].Value,
                                    Convert.ToInt32(match.Groups["port"].Value));
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }
    }
}
