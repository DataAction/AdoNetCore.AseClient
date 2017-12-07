using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class CapabilityToken : IToken
    {
        //todo: create fields to represent capabilities
        public TokenType Type => TokenType.TDS_CAPABILITY;

        public void Write(Stream stream, Encoding enc)
        {
            Logger.Instance?.WriteLine($"Write {Type}");
            stream.WriteByte((byte)Type);
            stream.WriteShort((short)_capabilityBytes.Length);
            stream.Write(_capabilityBytes);
        }

        public void Read(Stream stream, Encoding enc, IFormatToken previous)
        {
            var remainingLength = stream.ReadUShort();
            var capabilityBytes = new byte[remainingLength];
            stream.Read(capabilityBytes, 0, remainingLength);
        }

        //from .net 4 client
        private readonly byte[] _capabilityBytes = {
            //cap request
            0x01, 0x0e, 0x01, 0xef, 0xff, 0x69, 0xb7, 0xfd, 0xff, 0xaf, 0x65, 0x41, 0xff, 0xff, 0xff, 0xd6,
            //cap response
            0x02, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x88, 0x40, 0x00, 0x01, 0x02, 0x48, 0x00, 0x00, 0x00
        };

        public static CapabilityToken Create(Stream stream, Encoding enc, IFormatToken previous)
        {
            var t = new CapabilityToken();
            t.Read(stream, enc, previous);
            return t;
        }

        /*public enum RequestByte1 : byte
        {
            TDS_REQ_LANG = 0x01, //1 Language requests
            TDS_REQ_RPC = 0x02, //2 RPC requests
            TDS_REQ_EVT = 0x04, //3 Registered procedure event notification
            TDS_REQ_MSTMT = 0x08, //4 Support multiple commands per request
            TDS_REQ_BCP = 0x10, //5 Bulk copy requests
            TDS_REQ_CURSOR = 0x20, //6 Cursor command requests
            TDS_REQ_DYNF = 0x40, //7 Dynamic SQL requests
            TDS_REQ_MSG = 0x80, //8 TDS_MSG requests
        }

        public enum RequestByte2 : byte
        {
            TDS_REQ_PARAM = 0x01, //9 RPC requests will use the TDS_DBRPC token and TDS_PARAMFMT/TDS_PARAM to send parameters.
            TDS_DATA_INT1 = 0x02, //10 Support 1 byte unsigned integers
            TDS_DATA_INT2 = 0x04, //11 Support 2 byte integers
            TDS_DATA_INT4 = 0x08, //12 Support 4 byte integers
            TDS_DATA_BIT = 0x10, //13 Support bit data types
            TDS_DATA_CHAR = 0x20, //14 Support fixed length character data types
            TDS_DATA_VCHAR = 0x40, //15 Support variable length character data types
            TDS_DATA_BIN = 0x80, //16 Support fixed length character data types
        }
        public enum RequestByte3 : byte
        {
            TDS_DATA_VBIN = 0x01, //17 Support variable length binary data types
            TDS_DATA_MNY8 = 0x02, //18 Support 8 byte money data types
            TDS_DATA_MNY4 = 0x04, //19 Support 4 byte money data types
            TDS_DATA_DATE8 = 0x08, //20 Support 8 byte date/time data types
            TDS_DATA_DATE4 = 0x10, //21 Support 4 byte date/time data types
            TDS_DATA_FLT4 = 0x20, //22 Support 4 byte floating point data types
            TDS_DATA_FLT8 = 0x40, //23 Support 8 byte floating point data types
            TDS_DATA_NUM = 0x80, //24 Support numeric data types
        }
        public enum Byte4 : byte
        {
            TDS_DATA_TEXT = 0x01, //25 Support text data types
            TDS_DATA_IMAGE = 0x02, //26 Support image data types
            TDS_DATA_DEC = 0x04, //27 Support decimal data types
            TDS_DATA_LCHAR = 0x08, //28 Support long variable length character data types
            TDS_DATA_LBIN = 0x10, //29 Support long variable length binary data types.
            TDS_DATA_INTN = 0x20, //30 Support NULL integers
            TDS_DATA_DATETIMEN = 0x40, //31 Support NULL date/time
            TDS_DATA_MONEYN = 0x80, //32 Support NULL money
        }
        public enum RequestByte5 : byte
        {
            TDS_CSR_PREV = 0x01, //33 Obsolete, will not be used.
            TDS_CSR_FIRST = 0x02, //34 Obsolete, will not be used.
            TDS_CSR_LAST = 0x04, //35 Obsolete, will not be used.
            TDS_CSR_ABS = 0x08, //36 Obsolete, will not be used.
            TDS_CSR_REL = 0x10, //37 Obsolete, will not be used.
            TDS_CSR_MULTI = 0x20, //38 This is possibly obsolete.
            TDS_CON_OOB = 0x40, //39 Support expedited attentions
            TDS_CON_INBAND = 0x80, //40 Support non-expedited attentions
        }
        public enum RequestByte6 : byte
        {
            TDS_CON_LOGICAL = 0x01, //41 Support logical logout (not supported in this release)
            TDS_PROTO_TEXT = 0x02, //42 Support tokenized text and image (not supported in this release)
            TDS_PROTO_BULK = 0x04, //43 Support tokenized bulk copy (not supported this release)
            TDS_REQ_URGEVT = 0x08, //44 Use new event notification protocol
            TDS_DATA_SENSITIVITY = 0x10, //45 Support sensitivity security data types
            TDS_DATA_BOUNDARY = 0x20, //46 Support boundary security data types
            TDS_PROTO_DYNAMIC = 0x40, //47 Use DESCIN/DESCOUT dynamic protocol
            TDS_PROTO_DYNPROC = 0x80, //48 Pre-pend “create proc” to dynamic prepare statements
        }
        public enum RequestByte7 : byte
        {
            TDS_DATA_FLTN = 0x01, //49 Support NULL floats
            TDS_DATA_BITN = 0x02, //50 Support NULL bits
            TDS_DATA_INT8 = 0x04, //51 Support 8 byte integers
            TDS_DATA_VOID = 0x08, //52 ?
            TDS_DOL_BULK = 0x10, //53 ?
            TDS_OBJECT_JAVA1 = 0x20, //54 Support Serialized Java Objects
            TDS_OBJECT_CHAR = 0x40, //55 Support Streaming character data
            RESERVED = 0x80, //56 Reserved for future use
        }
        public enum RequestByte8 : byte
        {
            TDS_OBJECT_BINARY = 0x01, //57 Streaming Binary data
            TDS_DATA_COLUMNSTATUS = 0x02, //58 Indicates that a one-byte status field can follow any length or data (etc.) for every column within a row using TDS_ROW or TDS_PARAMS. Note that when this capability is on, the ROWFMT* and PARAMFMT* tokens indicate in their status byte fields whether a particular column will contain the columnstatus byte.
            TDS_WIDETABLE = 0x04, //59 The client may send requests using the CURDECLARE2, DYNAMIC2, PARAMFMT2 tokens.
            RESERVED = 0x08, //60 Reserved
            TDS_DATA_UINT2 = 0x10, //61 Support for unsigned 2-byte integers
            TDS_DATA_UINT4 = 0x20, //62 Support for unsigned 4-byte integers
            TDS_DATA_UINT8 = 0x40, //63 Support for unsigned 8-byte integers
            TDS_DATA_UINTN = 0x80, //64 Support for NULL unsigned integers
        }
        public enum RequestByte9 : byte
        {
            TDS_CUR_IMPLICIT = 0x01, //65 Support for TDS_CUR_DOPT_IMPLICIT cursor declare option.
            TDS_DATA_NLBIN = 0x02, //66 Support for LONGBINARY data containing UTF-16 encoded data (usertypes 34 and 35)
            TDS_IMAGE_NCHAR = 0x04, //67 Support for IMAGE data containing UTF-16 encoded data (usertype 36).
            TDS_BLOB_NCHAR_16 = 0x08, //68 Support for BLOB subtype 0x05 (unichar) with serialization type 0.
            TDS_BLOB_NCHAR_8 = 0x10, //69 Support for BLOB subtype 0x05 (unichar) with serialization type 1.
            TDS_BLOB_NCHAR_SCSU = 0x20, //70 Support for BLOB subtype 0x05 (unichar) with serialization type2.
            TDS_DATA_DATE = 0x40, //71 Support for Date
            TDS_DATA_TIME = 0x80, //72 Support for Time.
        }
        public enum RequestByte10 : byte
        {
            TDS_DATA_INTERVAL = 0x01, //73 Support for Interval
            TDS_CSR_SCROLL = 0x02, //74 Support for Scrollable Cursor. This bit must be on for the following four capability bits to have meaning.
            TDS_CSR_SENSITIVE = 0x04, //75 Support for Scrollable Sensitive Cursor
            TDS_CSR_INSENSITIVE = 0x08, //76 Support for Scrollable Insensitive Cursor
            TDS_CSR_SEMISENSITIVE = 0x10, //77 Support for Scrollable Semi-sensitive Cursor
            TDS_CSR_KEYSETDRIVEN = 0x20, //78 Support for Scrollable Keyset-driven Cursor
            TDS_REQ_SRVPKTSIZE = 0x40, //79 Support for server specified packet size
            TDS_DATA_UNITEXT = 0x80, //80 Support for Unicode UTF-16 Text.
        }

        public enum RequestByte11 : byte
        {
            TDS_CAP_CLUSTERFAILOVER = 0x01, //81 Support Cluster Failover Extensions.
            TDS_DATA_SINT1 = 0x02, //82 Support for 1 byte signed integer
            TDS_REQ_LARGEIDENT = 0x04, //83 Support for large identifiers
            TDS_REQ_BLOB_NCHAR_16 = 0x08, //84 Support for BLOB subtype 0x05 (unichar) with serialization type 0. Replaces
            TDS_BLOB_NCHAR_16 = 0x10, // Added to deal with ASE coding issue in old servers.
            TDS_DATA_XML = 0x20, //85 Support for XML datatype.
            TDS_REQ_CURINFO3 = 0x40, //86 Support for TDS_CURINFO3 token.
            TDS_REQ_DBRPC2 = 0x80, //87 Support for TDS_DBRPC2 token.
        }

        public enum RequestByte12 : byte
        {
            TDS_REQ_MIGRATE = 0x01, // 89 Client can be migrated to another server
        }*/
    }
}
