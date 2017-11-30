using System;
using System.Collections.Generic;
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
                    default:
                        throw new InvalidOperationException($"Unsupported data type {Type}");
                }
            }

            public void Read(Stream stream)
            {
                switch (Type)
                {
                    case TdsDataType.TDS_INT4:
                        Value = stream.ReadInt();
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported data type {Type}");
                }
                Console.WriteLine($"  <- {Type}: {Value}");
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
            var parameters = new List<Parameter>();
            foreach(var format in previous.Formats)
            {
                var p = new Parameter
                {
                    Type = format.DataType
                };
                p.Read(stream);
                parameters.Add(p);
            }
            Parameters = parameters.ToArray();
        }

        public static ParametersToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new ParametersToken();
            t.Read(stream, enc, previous);
            return t;
        }
    }
}