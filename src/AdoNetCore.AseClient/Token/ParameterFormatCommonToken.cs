using System;
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
        [Flags]
        public enum ParameterStatus : byte
        {
            /// <summary>
            /// This is a return parameter. It is like a parameter passed by reference.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_PARAM_RETURN = 0x01,
            /// <summary>
            /// This parameter will have a columnstatus byte in its corresponding TDS_PARAM token. Note that it will be a protocol error for this bit to be set when the TDS_DATA_COLUMNSTATUS capability bit is off.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_PARAM_COLUMNSTATUS = 0x08,
            /// <summary>
            /// This parameter can be NULL
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_PARAM_NULLALLOWED = 0x20
        }

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

            var nullableStatus = item.IsNullable ? ParameterStatus.TDS_PARAM_NULLALLOWED : 0x00;
            var outputStatus = item.IsOutput ? ParameterStatus.TDS_PARAM_RETURN : 0x00;
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
            throw new NotImplementedException();
        }
    }
}