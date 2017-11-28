using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Packet
{
    public class NormalPacket:IPacket
    {
        public BufferType Type => BufferType.TDS_BUF_NORMAL;

        public void Write(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }

        public void Read(Stream stream, Encoding enc)
        {
            throw new NotImplementedException();
        }
    }
}
