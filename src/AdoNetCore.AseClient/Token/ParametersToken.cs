using System.Collections.Generic;
using System.IO;
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

        public void Write(Stream stream, DbEnvironment env)
        {
            Logger.Instance?.WriteLine($"-> {Type}: {Parameters.Length} parameters");
            stream.WriteByte((byte)Type);
            foreach (var parameter in Parameters)
            {
                ValueWriter.Write(parameter.Value, stream, parameter.Format, env.Encoding);
            }
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var parameters = new List<Parameter>();
            foreach(var format in previous.Formats)
            {
                var p = new Parameter
                {
                    Format = format,
                    Value = ValueReader.Read(stream, format, env)
                };
                parameters.Add(p);
            }
            Parameters = parameters.ToArray();
        }

        public static ParametersToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new ParametersToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}