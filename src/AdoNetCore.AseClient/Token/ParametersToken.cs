using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    /// <summary>
    /// Refer: p. 291 TDS_PARAMS
    /// </summary>
    internal class ParametersToken : IToken
    {
        public class Parameter
        {
            public TdsDataType Type { get; set; }
            public object Value { get; set; }

            public void Write(Stream stream)
            {
                Console.WriteLine($"  -> {Type}: {Value}");
                switch (Type)
                {
                    case TdsDataType.TDS_INT4:
                        stream.WriteInt((int)Value);
                        break;
                }
            }
        }

        public TokenType Type => TokenType.TDS_PARAMS;

        public Parameter[] Parameters { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"-> {Type}: {Parameters.Length} parameters");
            stream.WriteByte((byte)Type);
            foreach (var parameter in Parameters)
            {
                parameter.Write(stream);
            }
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            throw new NotImplementedException();
        }
    }
}