using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    /// <summary>
    /// Refer: p. 285 TDS_PARAMFMT
    /// </summary>
    internal class ParameterFormatToken : IToken
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

        public class Parameter
        {
            private string _name;
            public string Name
            {
                get => _name;
                set => _name = value.StartsWith("@") ? value : $"@{value}";
            }

            public bool IsNullable { get; set; }
            public bool IsOutput { get; set; }
            //public int UserType { get; set; }
            public TdsDataType DataType { get; set; }
            public int Length { get; set; }
            public int Precision { get; set; }
            public int Scale { get; set; }

            public void Write(Stream stream, Encoding enc)
            {
                Console.WriteLine($"  -> {Name}: {DataType}");
                if (string.IsNullOrWhiteSpace(Name) || string.Equals("@", Name))
                {
                    stream.WriteByte(0);
                }
                else
                {
                    stream.WriteBytePrefixedString(Name, enc);
                }

                var nullableStatus = IsNullable ? ParameterStatus.TDS_PARAM_NULLALLOWED : 0x00;
                var outputStatus = IsOutput ? ParameterStatus.TDS_PARAM_RETURN : 0x00;
                stream.WriteByte((byte)(nullableStatus | outputStatus));

                stream.WriteInt(0); //we don't currently do anything with user types
                stream.WriteByte((byte)DataType);

                switch (DataType)
                {
                    case TdsDataType.TDS_INT4:
                        //int4 is fixed-length so don't write anything
                        break;
                    default:
                        throw new InvalidOperationException($"{DataType} not yet supported");
                }

                //locale
                stream.WriteByte(0);
            }
        }

        public TokenType Type => TokenType.TDS_PARAMFMT;

        public Parameter[] Parameters { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"-> {Type}: {Parameters.Length} parameters");
            stream.WriteByte((byte)Type);
            var paramCount = (short)Parameters.Length;
            using (var ms = new MemoryStream())
            {
                foreach (var parameter in Parameters)
                {
                    parameter.Write(ms, enc);
                }
                ms.Seek(0, SeekOrigin.Begin);
                var formatBytes = ms.ToArray();

                stream.WriteShort((short)(2 + formatBytes.Length));
                stream.WriteShort(paramCount);
                stream.Write(formatBytes, 0, formatBytes.Length);
            }
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            throw new NotImplementedException();
        }
    }
}
