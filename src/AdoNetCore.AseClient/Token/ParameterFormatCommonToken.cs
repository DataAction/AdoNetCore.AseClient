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
    /// Refer: p. 285 TDS_PARAMFMT and p. 289 TDS_PARAMFMT2
    /// </summary>
    internal class ParameterFormatCommonToken : IFormatToken
    {
        public TokenType Type { get; private set; }
        public FormatItem[] Formats { get; set; }

        public ParameterFormatCommonToken(TokenType type)
        {
            Type = type;
        }

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"-> {Type}: {Formats.Length} parameters");
            stream.WriteByte((byte)Type);
            var paramCount = (short)Formats.Length;
            using (var ms = new MemoryStream())
            {
                foreach (var format in Formats)
                {
                    format.WriteForParameter(ms, enc, Type);
                }
                ms.Seek(0, SeekOrigin.Begin);
                var formatBytes = ms.ToArray();

                //length
                var remainingLength = 2 + formatBytes.Length;
                if (Type == TokenType.TDS_PARAMFMT)
                {
                    stream.WriteShort((short)remainingLength);
                }
                else
                {
                    stream.WriteUInt((uint)remainingLength);
                }
                stream.WriteShort(paramCount);
                stream.Write(formatBytes, 0, formatBytes.Length);
            }
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previousFormatToken)
        {
            var remainingLength = Type == TokenType.TDS_PARAMFMT
                ? stream.ReadUShort()
                : stream.ReadUInt();

            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                var paramCount = ts.ReadUShort();
                var formats = new List<FormatItem>();

                for (var i = 0; i < paramCount; i++)
                {
                    formats.Add(FormatItem.ReadForParameter(ts, enc, Type));
                }

                Formats = formats.ToArray();
            }
        }
    }
}