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
                    WriteFormatItem(format, ms, enc);
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

        private void WriteFormatItem(FormatItem item, Stream stream, Encoding enc)
        {
            Console.WriteLine($"  -> {item.ParameterName}: {item.DataType}");
            if (string.IsNullOrWhiteSpace(item.ParameterName) || string.Equals("@", item.ParameterName))
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteBytePrefixedString(item.ParameterName, enc);
            }

            var nullableStatus = item.IsNullable ? ParameterFormatItemStatus.TDS_PARAM_NULLALLOWED : 0x00;
            var outputStatus = item.IsOutput ? ParameterFormatItemStatus.TDS_PARAM_RETURN : 0x00;
            var status = nullableStatus | outputStatus;
            if (Type == TokenType.TDS_PARAMFMT)
            {
                stream.WriteByte((byte) status);
            }
            else
            {
                stream.WriteUInt((uint) status);
            }

            stream.WriteInt(0); //we don't currently do anything with user types
            stream.WriteByte((byte)item.DataType);

            switch (item.DataType)
            {
                case TdsDataType.TDS_INT4:
                    //int4 is fixed-length so don't write anything
                    break;
                default:
                    throw new InvalidOperationException($"{item.DataType} not yet supported");
            }

            //locale
            stream.WriteByte(0);
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