using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    public class TokenParser : ITokenParser
    {
        public IToken[] Parse(Stream stream, Encoding enc)
        {
            return ParseInternal(stream, enc).ToArray();
        }

        private IEnumerable<IToken> ParseInternal(Stream stream, Encoding enc)
        {
            IToken previous = null;
            while (stream.Position < stream.Length)
            {
                var tokenType = (TokenType)stream.ReadByte();
                Console.WriteLine($"Hit token type {tokenType}");

                if (readers.ContainsKey(tokenType))
                {
                    var t = readers[tokenType](stream, enc, previous);
                    previous = t;
                    yield return t;
                }
                else
                {
                    var t = new CatchAllToken(tokenType);
                    t.Read(stream, enc, previous);
                    previous = t;
                    yield return t;
                    //throw new InvalidOperationException($"Unexpected token type {tokenType}");
                }
            }
        }

        private static Dictionary<TokenType, Func<Stream, Encoding, IToken, IToken>> readers = new Dictionary<TokenType, Func<Stream, Encoding, IToken, IToken>>
        {
            //{TokenType.TDS_ENVCHANGE, EnvironmentChangeToken.Create}
        };
    }
}
