using System.Collections.Generic;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    internal class TokenParser : ITokenParser
    {
        private IFormatToken _previousFormatToken;
        
        public long LastStartPosition { get; private set; }

        public IEnumerable<IToken> Parse(Stream stream, DbEnvironment env, out bool streamExceeded)
        {
            var tokens = new List<IToken>();
            streamExceeded = false;
            while (stream.Position < stream.Length)
            {
                LastStartPosition = stream.Position;
                var tokenType = (TokenType)stream.ReadByte();

                if (Readers.ContainsKey(tokenType))
                {
                    var t = Readers[tokenType](stream, env, _previousFormatToken, ref streamExceeded);
                    if (streamExceeded)
                        return tokens;

                    if (t is IFormatToken token)
                    {
                        Logger.Instance?.WriteLine("**Set new format token**");
                        _previousFormatToken = token;
                    }

                    tokens.Add(t);
                }
                else
                {
                    Logger.Instance?.WriteLine($"!!! Hit unknown token type {tokenType} !!!");
                    var t = new CatchAllToken(tokenType);
                    t.Read(stream, env, _previousFormatToken, ref streamExceeded);
                    if (streamExceeded)
                        return tokens;
                    tokens.Add(t);
                }

                if (tokenType == TokenType.TDS_DONE)
                    _previousFormatToken = null;
            }
            LastStartPosition = stream.Length;
            return tokens;
        }

        private delegate IToken ReadersMethodDelegate(Stream stream, DbEnvironment env, IFormatToken previous, ref bool streamExceeded);
        private static readonly Dictionary<TokenType, ReadersMethodDelegate> Readers = new Dictionary<TokenType, ReadersMethodDelegate>
        {
            {TokenType.TDS_ENVCHANGE, EnvironmentChangeToken.Create},
            {TokenType.TDS_EED, EedToken.Create },
            {TokenType.TDS_LOGINACK, LoginAckToken.Create },
            {TokenType.TDS_DONE, DoneToken.Create },
            {TokenType.TDS_CAPABILITY, CapabilityToken.Create },
            {TokenType.TDS_RETURNSTATUS, ReturnStatusToken.Create },
            {TokenType.TDS_DONEINPROC, DoneInProcToken.Create },
            {TokenType.TDS_DONEPROC, DoneProcToken.Create },
            {TokenType.TDS_ROWFMT2, RowFormat2Token.Create },
            {TokenType.TDS_CONTROL, ControlToken.Create },
            {TokenType.TDS_ROW, RowToken.Create },
            {TokenType.TDS_PARAMFMT, ParameterFormatToken.Create },
            {TokenType.TDS_PARAMFMT2, ParameterFormat2Token.Create },
            {TokenType.TDS_PARAMS, ParametersToken.Create },
            {TokenType.TDS_OPTIONCMD, OptionCommandToken.Create }
        };
    }
}
