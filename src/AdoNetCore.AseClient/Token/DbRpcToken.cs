using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class DbRpcToken : IToken
    {
        public enum DbRpcOptions : short
        {
            // ReSharper disable once InconsistentNaming
            TDS_RPC_UNUSED = 0x0000,
            // ReSharper disable once InconsistentNaming
            TDS_RPC_RECOMPILE = 0x0001,
            // ReSharper disable once InconsistentNaming
            TDS_RPC_PARAMS = 0x0002
        }

        public TokenType Type => TokenType.TDS_DBRPC;

        public string ProcedureName { get; set; }
        public bool HasParameters { get; set; }

        public void Write(Stream stream, Encoding enc)
        {
            Console.WriteLine($"-> {Type}: {ProcedureName}");

            var rpcNameBytes = enc.GetBytes(ProcedureName);
            var rpcLength = (byte) rpcNameBytes.Length;

            stream.WriteByte((byte)Type);
            stream.WriteShort((short)(rpcLength + 3));
            stream.WriteByte(rpcLength);
            stream.Write(rpcNameBytes, 0, rpcLength);
            stream.WriteShort((short) (HasParameters
                ? DbRpcOptions.TDS_RPC_PARAMS
                : DbRpcOptions.TDS_RPC_UNUSED));
        }

        public void Read(Stream stream, Encoding enc, IToken previous)
        {
            throw new NotImplementedException();
        }
    }
}
