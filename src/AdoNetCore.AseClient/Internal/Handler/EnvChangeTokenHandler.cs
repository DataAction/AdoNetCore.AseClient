using System;
using System.Collections.Generic;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class EnvChangeTokenHandler : ITokenHandler
    {
        //not entirely sure what charsets ASE supports, there are quite a few mentioned in:
        //  * Selecting the Character Set for Your Server - (http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc31654.1600/doc/html/san1360629216676.html)
        //  * https://www.connectionstrings.com/ase-unsupported-charset/
        //  * master.dbo.syscharsets
        //the ones of interest seem to be: iso_1 (or ???), ascii_8, utf-8 (or utf8???), 
        private static readonly Dictionary<string, Func<Encoding>> CharsetMap = new Dictionary<string, Func<Encoding>>(StringComparer.OrdinalIgnoreCase)
        {
            {"iso_1", () => Encoding.GetEncoding("ISO-8859-1")},
            {"iso 8859-1", () => Encoding.GetEncoding("ISO-8859-1")},
            {"iso88591", () => Encoding.GetEncoding("ISO-8859-1")},
            {"ascii_8", () => Encoding.ASCII},
            {"utf-8", () => Encoding.UTF8},
            {"utf8", () => Encoding.UTF8},
        };
        private readonly DbEnvironment _environment;
        private readonly string _clientRequestedCharset;

        public EnvChangeTokenHandler(DbEnvironment environment, string clientRequestedCharset)
        {
            _environment = environment;
            _clientRequestedCharset = clientRequestedCharset;
        }

        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_ENVCHANGE || type == TokenType.TDS_OPTIONCMD;
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case EnvironmentChangeToken t:
                    foreach (var change in t.Changes)
                    {
                        Logger.Instance?.WriteLine($"{t.Type}: {change.Type} - {change.OldValue} -> {change.NewValue}");
                        switch (change.Type)
                        {
                            case EnvironmentChangeToken.ChangeType.TDS_ENV_DB:
                                _environment.Database = change.NewValue;
                                break;
                            case EnvironmentChangeToken.ChangeType.TDS_ENV_PACKSIZE:
                                if (int.TryParse(change.NewValue, out int newPackSize))
                                {
                                    _environment.PacketSize = newPackSize;
                                }
                                break;
                            case EnvironmentChangeToken.ChangeType.TDS_ENV_CHARSET:
                                ApplyNewEncoding(GetNewCharset(change.NewValue));
                                break;
                        }
                    }
                    break;
                case OptionCommandToken o:
                    if (o.Option == OptionCommandToken.OptionType.TDS_OPT_TEXTSIZE)
                    {
                        _environment.TextSize = BitConverter.ToInt32(o.Arguments, 0);
                        Logger.Instance?.WriteLine($"{o.Type}: {o.Option} -> {_environment.TextSize}");
                    }
                    break;
                default:
                    return;
            }
        }

        // If the change token does not specify the new charset
        // then use the client requested charset instead
        private string GetNewCharset(string newValue)
        {
            string newCharset = newValue ?? string.Empty;
            if (newCharset.Equals(string.Empty))
            {
                newCharset = _clientRequestedCharset ?? string.Empty;
            }
            return newCharset;
        }

        private void ApplyNewEncoding(string newCharset)
        {
            if (!newCharset.Equals(string.Empty))
            {
                if (CharsetMap.ContainsKey(newCharset))
                {
                    _environment.Encoding = CharsetMap[newCharset]();
                }
                else
                {
                    try
                    {
                        // save it for later
                        var newEncoding = Encoding.GetEncoding(newCharset);
                        CharsetMap[newCharset] = () => newEncoding;

                        _environment.Encoding = newEncoding;
                    }
                    catch
                    {
                        throw new AseException($"Server environment changed to unsupported charset '{newCharset}'. To add support for this charset, register an EncodingProvider to handle targeting '{newCharset}'.");
                    }
                }
            }
        }
    }
}
