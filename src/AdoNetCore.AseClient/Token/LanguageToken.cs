using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class LanguageToken : IToken
    {
        public TokenType Type => TokenType.TDS_LANGUAGE;

        public string CommandText { get; set; }
        public bool HasParameters { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            Logger.Instance?.WriteLine($"Write {Type}: '{CommandText}'");
            stream.WriteByte((byte)Type);
            var commandText = enc.GetBytes(CommandText);
            stream.WriteInt(1 + commandText.Length);
            stream.WriteByte((byte)(HasParameters ? 1 : 0));
            stream.Write(commandText, 0, commandText.Length);
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            var remainingLength = stream.ReadInt();
            var status = stream.ReadByte();
            HasParameters = (status & 1) > 0;
            CommandText = stream.ReadString(remainingLength - 1, enc);
        }
    }
}
