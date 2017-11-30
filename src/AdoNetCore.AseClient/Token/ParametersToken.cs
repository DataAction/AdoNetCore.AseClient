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
            public FormatItem Format { get; set; }
            public object Value { get; set; }
        }

        public TokenType Type => TokenType.TDS_PARAMS;

        public Parameter[] Parameters { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"-> {Type}: {Parameters.Length} parameters");
            stream.WriteByte((byte)Type);
            foreach (var parameter in Parameters)
            {
                ValueWriter.Write(parameter.Value, stream, parameter.Format);
            }
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            var parameters = new List<Parameter>();
            foreach(var format in previous.Formats)
            {
                var p = new Parameter
                {
                    Format = format,
                    Value = ValueReader.Read(stream, format)
                };
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