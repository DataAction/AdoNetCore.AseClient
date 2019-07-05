using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class CapabilityToken : IToken
    {
        public TokenType Type => TokenType.TDS_CAPABILITY;

        private readonly byte[] _capabilityBytes;

        public CapabilityToken(bool enableServerPacketSize = true)
        {
            var enableServerPacketSizeBit = enableServerPacketSize ? Byte6.REQ_SRVPKTSIZE : Byte6.None;

            _capabilityBytes =  new[] {
                //cap request
                ((byte)Byte0.Unknown),
                ((byte)Byte1.Unknown),
                ((byte)(/*Byte2.REQ_UNUSED1 | Byte2.REQ_UNUSED2 | Byte2.REQ_UNUSED3 | Byte2.REQ_UNUSED4 | Byte2.REQ_UNUSED5 | Byte2.REQ_COMMAND_ENCRYPTION | Byte2.REQ_READONLY |*/ Byte2.REQ_DYNAMIC_SUPPRESS_PARAMFMT)),
                ((byte)(Byte3.REQ_LOGPARAMS | Byte3.REQ_ROWCOUNT_FOR_SELECT | Byte3.DATA_LOBLOCATOR/*| Byte3.REQ_RPC_BATCH*/| Byte3.REQ_LANG_BATCH | Byte3.REQ_DYN_BATCH | Byte3.REQ_GRID | Byte3.REQ_INSTID)),
                ((byte)(Byte4.RPCPARAM_LOB | Byte4.DATA_USECS | Byte4.DATA_BIGDATETIME | Byte4.REQ_UNUSED4 | Byte4.REQ_UNUSED5 | Byte4.MULTI_REQUESTS | Byte4.REQ_MIGRATE | Byte4.REQ_UNUSED8)),
                ((byte)(/*Byte5.REQ_DBRPC2 | */Byte5.REQ_CURINFO3 | Byte5.DATA_XML/* | Byte5.REQ_BLOB_NCHAR_16*/ | Byte5.REQ_LARGEIDENT/* | Byte5.DATA_SINT1 | Byte5.CAP_CLUSTERFAILOVER*/ | Byte5.DATA_UNITEXT)),
                ((byte)(enableServerPacketSizeBit/* | Byte6.CSR_KEYSETDRIVEN*/ | Byte6.CSR_SEMISENSITIVE | Byte6.CSR_INSENSITIVE/* | Byte6.CSR_SENSITIVE*/ | Byte6.CSR_SCROLL | Byte6.DATA_INTERVAL | Byte6.DATA_TIME)),
                ((byte)(Byte7.DATA_DATE | Byte7.BLOB_NCHAR_SCSU | Byte7.BLOB_NCHAR_8 | Byte7.BLOB_NCHAR_16 | Byte7.IMAGE_NCHAR | Byte7.DATA_NLBIN/* | Byte7.CUR_IMPLICIT*/ | Byte7.DATA_UINTN)),
                ((byte)(Byte8.DATA_UINT8 | Byte8.DATA_UINT4 | Byte8.DATA_UINT2 | Byte8.REQ_RESERVED2 | Byte8.WIDETABLE | Byte8.DATA_COLUMNSTATUS | Byte8.OBJECT_BINARY | Byte8.REQ_RESERVED1)),
                ((byte)(Byte9.OBJECT_CHAR/* | Byte9.OBJECT_JAVA1*/ | Byte9.DOL_BULK/* | Byte9.DATA_VOID*/ | Byte9.DATA_INT8 | Byte9.DATA_BITN | Byte9.DATA_FLTN | Byte9.PROTO_DYNPROC)),
                ((byte)(/*Byte10.PROTO_DYNAMIC | */Byte10.DATA_BOUNDARY | Byte10.DATA_SENSITIVITY/* | Byte10.REQ_URGEVT | Byte10.PROTO_BULK*/ | Byte10.PROTO_TEXT/* | Byte10.CON_LOGICAL*/ | Byte10.CON_INBAND)),
                ((byte)(/*Byte11.CON_OOB | */Byte11.CSR_MULTI/* | Byte11.CSR_REL | Byte11.CSR_ABS | Byte11.CSR_LAST | Byte11.CSR_FIRST | Byte11.CSR_PREV*/ | Byte11.DATA_MONEYN)),
                ((byte)(Byte12.DATA_DATETIMEN | Byte12.DATA_INTN | Byte12.DATA_LBIN | Byte12.DATA_LCHAR | Byte12.DATA_DEC | Byte12.DATA_IMAGE | Byte12.DATA_TEXT | Byte12.DATA_NUM)),
                ((byte)(Byte13.DATA_FLT8 | Byte13.DATA_FLT4 | Byte13.DATA_DATE4 | Byte13.DATA_DATE8 | Byte13.DATA_MNY4 | Byte13.DATA_MNY8 | Byte13.DATA_VBIN | Byte13.DATA_BIN)),
                ((byte)(Byte14.DATA_VCHAR | Byte14.DATA_CHAR | Byte14.DATA_BIT | Byte14.DATA_INT4 | Byte14.DATA_INT2 | Byte14.DATA_INT1 | Byte14.REQ_PARAM | Byte14.REQ_MSG)),
                ((byte)(Byte15.REQ_DYNF | Byte15.REQ_CURSOR/* | Byte15.REQ_BCP*/ | Byte15.REQ_MSTMT/* | Byte15.REQ_EVT*/ | Byte15.REQ_RPC | Byte15.REQ_LANG/* | Byte15.NONE*/)),

                //cap response
                ((byte)Byte16.Unknown),
                ((byte)Byte17.Unknown),
                ((byte)Byte18.Unknown),
                ((byte)Byte19.Unknown),
                ((byte)Byte20.Unknown),
                ((byte)Byte21.Unknown),
                ((byte)Byte22.Unknown),
                ((byte)(Byte23.RES_UNUSED1/* | Byte23.DATA_NOLOBLOCATOR | Byte23.RES_UNUSED2 | Byte23.RPCPARAM_NOLOB*/ | Byte23.RES_NO_TDSCONTROL/* | Byte23.DATA_NOUSECS | Byte23.DATA_NOBIGDATETIME | Byte23.RES_FORCE_ROWFMT2*/)),
                ((byte)(/*Byte24.RES_SUPPRESS_DONEINPROC | */Byte24.RES_SUPPRESS_FMT/* | Byte24.RES_NOXNLDATA | Byte24.NONINT_RETURN_VALUE | Byte24.RES_NODATA_XML | Byte24.NO_SRVPKTSIZE | Byte24.RES_NOBLOB_NCHAR_16 | Byte24.RES_NOLARGEIDENT*/)),
                ((byte)(Byte25.None/* | Byte25.DATA_NOSINT1 | Byte25.DATA_NOUNITEXT | Byte25.DATA_NOINTERVAL | Byte25.DATA_NOTIME | Byte25.DATA_NODATE | Byte25.BLOB_NONCHAR_SCSU | Byte25.BLOB_NONCHAR_8 | Byte25.BLOB_NONCHAR_16*/)),
                ((byte)(/*Byte26.IMAGE_NONCHAR | Byte26.DATA_NONLBIN | Byte26.NO_WIDETABLES | Byte26.DATA_NOUINTN | Byte26.DATA_NOUINT8 | Byte26.DATA_NOUINT4 | Byte26.DATA_NOUINT2 | */Byte26.RES_RESERVED1)),
                ((byte)(/*Byte27.OBJECT_NOBINARY | Byte27.DATA_NOCOLUMNSTATUS | Byte27.OBJECT_NOCHAR | Byte27.OBJECT_NOJAVA1 | Byte27.DATA_NOINT8 | Byte27.RES_NOSTRIPBLANKS | */Byte27.RES_NOTDSDEBUG /*| Byte27.DATA_NOBOUNDARY*/)),
                ((byte)(/*Byte28.DATA_NOSENSITIVITY | */Byte28.PROTO_NOBULK/* | Byte28.PROTO_NOTEXT | Byte28.CON_NOINBAND*/ | Byte28.CON_NOOOB/* | Byte28.DATA_NOMONEYN | Byte28.DATA_NODATETIMEN | Byte28.DATA_NOINTN*/)),
                ((byte)(Byte29.None/* | Byte29.DATA_NOLBIN | Byte29.DATA_NOLCHAR | Byte29.DATA_NODEC | Byte29.DATA_NOIMAGE | Byte29.DATA_NOTEXT | Byte29.DATA_NONUM | Byte29.DATA_NOFLT8 | Byte29.DATA_NOFLT4*/)),
                ((byte)(Byte30.None/* | Byte30.DATA_NODATE4 | Byte30.DATA_NODATE8 | Byte30.DATA_NOMNY4 | Byte30.DATA_NOMNY8 | Byte30.DATA_NOVBIN | Byte30.DATA_NOBIN | Byte30.DATA_NOVCHAR | Byte30.DATA_NOCHAR*/)),
                ((byte)(Byte31.None/* | Byte31.DATA_NOBIT | Byte31.DATA_NOINT4 | Byte31.DATA_NOINT2 | Byte31.DATA_NOINT1 | Byte31.RES_NOPARAM | Byte31.RES_NOEED | Byte31.RES_NOMSG | Byte31.NONE*/)),
            };
        }

        public void Write(Stream stream, DbEnvironment env)
        {
            Logger.Instance?.WriteLine($"Write {Type}");
            stream.WriteByte((byte)Type);
            stream.WriteShort((short)_capabilityBytes.Length);
            stream.Write(_capabilityBytes);
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var remainingLength = stream.ReadUShort();
            var capabilityBytes = new byte[remainingLength];
            stream.Read(capabilityBytes, 0, remainingLength);
        }

        public static CapabilityToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new CapabilityToken();
            t.Read(stream, env, previous);
            return t;
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable IdentifierTypo

        [Flags]
        private enum Byte0 : byte
        {
            None = 0,
            Unknown = 0b0000_0001
        }

        [Flags]
        private enum Byte1 : byte
        {
            None = 0,
            Unknown = 0b0000_1110
        }

        [Flags]
        private enum Byte2 : byte
        {
            None = 0,
            REQ_UNUSED1 = 0b1000_0000,
            REQ_UNUSED2 = 0b0100_0000,
            REQ_UNUSED3 = 0b0010_0000,
            REQ_UNUSED4 = 0b0001_0000,
            REQ_UNUSED5 = 0b0000_1000,
            REQ_COMMAND_ENCRYPTION = 0b0000_0100,
            REQ_READONLY = 0b0000_0010,
            REQ_DYNAMIC_SUPPRESS_PARAMFMT = 0b0000_0001
        }

        [Flags]
        private enum Byte3 : byte
        {
            None = 0,
            REQ_LOGPARAMS = 0b1000_0000,
            REQ_ROWCOUNT_FOR_SELECT = 0b0100_0000,
            DATA_LOBLOCATOR = 0b0010_0000,
            REQ_RPC_BATCH = 0b0001_0000,
            REQ_LANG_BATCH = 0b0000_1000,
            REQ_DYN_BATCH = 0b0000_0100,
            REQ_GRID = 0b0000_0010,
            REQ_INSTID = 0b0000_0001
        }

        [Flags]
        private enum Byte4 : byte
        {
            None = 0,
            RPCPARAM_LOB = 0b1000_0000,
            DATA_USECS = 0b0100_0000,
            DATA_BIGDATETIME = 0b0010_0000,
            REQ_UNUSED4 = 0b0001_0000,
            REQ_UNUSED5 = 0b0000_1000,
            MULTI_REQUESTS = 0b0000_0100,
            REQ_MIGRATE = 0b0000_0010,
            REQ_UNUSED8 = 0b0000_0001
        }

        [Flags]
        private enum Byte5 : byte
        {
            None = 0,
            REQ_DBRPC2 = 0b1000_0000,
            REQ_CURINFO3 = 0b0100_0000,
            DATA_XML = 0b0010_0000,
            REQ_BLOB_NCHAR_16 = 0b0001_0000,
            REQ_LARGEIDENT = 0b0000_1000,
            DATA_SINT1 = 0b0000_0100,
            CAP_CLUSTERFAILOVER = 0b0000_0010,
            DATA_UNITEXT = 0b0000_0001
        }

        [Flags]
        private enum Byte6 : byte
        {
            None = 0,
            REQ_SRVPKTSIZE = 0b1000_0000,
            CSR_KEYSETDRIVEN = 0b0100_0000,
            CSR_SEMISENSITIVE = 0b0010_0000,
            CSR_INSENSITIVE = 0b0001_0000,
            CSR_SENSITIVE = 0b0000_1000,
            CSR_SCROLL = 0b0000_0100,
            DATA_INTERVAL = 0b0000_0010,
            DATA_TIME = 0b0000_0001
        }

        [Flags]
        private enum Byte7 : byte
        {
            None = 0,
            DATA_DATE = 0b1000_0000,
            BLOB_NCHAR_SCSU = 0b0100_0000,
            BLOB_NCHAR_8 = 0b0010_0000,
            BLOB_NCHAR_16 = 0b0001_0000,
            IMAGE_NCHAR = 0b0000_1000,
            DATA_NLBIN = 0b0000_0100,
            CUR_IMPLICIT = 0b0000_0010,
            DATA_UINTN = 0b0000_0001
        }

        [Flags]
        private enum Byte8 : byte
        {
            None = 0,
            DATA_UINT8 = 0b1000_0000,
            DATA_UINT4 = 0b0100_0000,
            DATA_UINT2 = 0b0010_0000,
            REQ_RESERVED2 = 0b0001_0000,
            WIDETABLE = 0b0000_1000,
            DATA_COLUMNSTATUS = 0b0000_0100,
            OBJECT_BINARY = 0b0000_0010,
            REQ_RESERVED1 = 0b0000_0001
        }

        [Flags]
        private enum Byte9 : byte
        {
            None = 0,
            OBJECT_CHAR = 0b1000_0000,
            OBJECT_JAVA1 = 0b0100_0000,
            DOL_BULK = 0b0010_0000,
            DATA_VOID = 0b0001_0000,
            DATA_INT8 = 0b0000_1000,
            DATA_BITN = 0b0000_0100,
            DATA_FLTN = 0b0000_0010,
            PROTO_DYNPROC = 0b0000_0001
        }

        [Flags]
        private enum Byte10 : byte
        {
            None = 0,
            PROTO_DYNAMIC = 0b1000_0000,
            DATA_BOUNDARY = 0b0100_0000,
            DATA_SENSITIVITY = 0b0010_0000,
            REQ_URGEVT = 0b0001_0000,
            PROTO_BULK = 0b0000_1000,
            PROTO_TEXT = 0b0000_0100,
            CON_LOGICAL = 0b0000_0010,
            CON_INBAND = 0b0000_0001
        }

        [Flags]
        private enum Byte11 : byte
        {
            None = 0,
            CON_OOB = 0b1000_0000,
            CSR_MULTI = 0b0100_0000,
            CSR_REL = 0b0010_0000,
            CSR_ABS = 0b0001_0000,
            CSR_LAST = 0b0000_1000,
            CSR_FIRST = 0b0000_0100,
            CSR_PREV = 0b0000_0010,
            DATA_MONEYN = 0b0000_0001
        }

        [Flags]
        private enum Byte12 : byte
        {
            None = 0,
            DATA_DATETIMEN = 0b1000_0000,
            DATA_INTN = 0b0100_0000,
            DATA_LBIN = 0b0010_0000,
            DATA_LCHAR = 0b0001_0000,
            DATA_DEC = 0b0000_1000,
            DATA_IMAGE = 0b0000_0100,
            DATA_TEXT = 0b0000_0010,
            DATA_NUM = 0b0000_0001
        }

        [Flags]
        private enum Byte13 : byte
        {
            None = 0,
            DATA_FLT8 = 0b1000_0000,
            DATA_FLT4 = 0b0100_0000,
            DATA_DATE4 = 0b0010_0000,
            DATA_DATE8 = 0b0001_0000,
            DATA_MNY4 = 0b0000_1000,
            DATA_MNY8 = 0b0000_0100,
            DATA_VBIN = 0b0000_0010,
            DATA_BIN = 0b0000_0001
        }

        [Flags]
        private enum Byte14 : byte
        {
            None = 0,
            DATA_VCHAR = 0b1000_0000,
            DATA_CHAR = 0b0100_0000,
            DATA_BIT = 0b0010_0000,
            DATA_INT4 = 0b0001_0000,
            DATA_INT2 = 0b0000_1000,
            DATA_INT1 = 0b0000_0100,
            REQ_PARAM = 0b0000_0010,
            REQ_MSG = 0b0000_0001
        }

        [Flags]
        private enum Byte15 : byte
        {
            None = 0,
            REQ_DYNF = 0b1000_0000,
            REQ_CURSOR = 0b0100_0000,
            REQ_BCP = 0b0010_0000,
            REQ_MSTMT = 0b0001_0000,
            REQ_EVT = 0b0000_1000,
            REQ_RPC = 0b0000_0100,
            REQ_LANG = 0b0000_0010,
            NONE = 0b0000_0001
        }

        [Flags]
        private enum Byte16 : byte
        {
            None = 0,
            Unknown = 0b0000_0010
        }

        [Flags]
        private enum Byte17 : byte
        {
            None = 0,
            Unknown = 0b0000_1110
        }

        [Flags]
        private enum Byte18 : byte
        {
            None = 0,
            Unknown = None
        }

        [Flags]
        private enum Byte19 : byte
        {
            None = 0,
            Unknown = None
        }

        [Flags]
        private enum Byte20 : byte
        {
            None = 0,
            Unknown = None
        }

        [Flags]
        private enum Byte21 : byte
        {
            None = 0,
            Unknown = None
        }

        [Flags]
        private enum Byte22 : byte
        {
            None = 0,
            Unknown = None
        }
        
        [Flags]
        private enum Byte23 : byte
        {
            None = 0,
            RES_UNUSED1 = 0b1000_0000,
            DATA_NOLOBLOCATOR = 0b0100_0000,
            RES_UNUSED2 = 0b0010_0000,
            RPCPARAM_NOLOB = 0b0001_0000,
            RES_NO_TDSCONTROL = 0b0000_1000,
            DATA_NOUSECS = 0b0000_0100,
            DATA_NOBIGDATETIME = 0b0000_0010,
            RES_FORCE_ROWFMT2 = 0b0000_0001
    }

        [Flags]
        private enum Byte24 : byte
        {
            None = 0,
            RES_SUPPRESS_DONEINPROC = 0b1000_0000,
            RES_SUPPRESS_FMT = 0b0100_0000,
            RES_NOXNLDATA = 0b0010_0000,
            NONINT_RETURN_VALUE = 0b0001_0000,
            RES_NODATA_XML = 0b0000_1000,
            NO_SRVPKTSIZE = 0b0000_0100,
            RES_NOBLOB_NCHAR_16 = 0b0000_0010,
            RES_NOLARGEIDENT = 0b0000_0001
        }

        [Flags]
        private enum Byte25 : byte
        {
            None = 0,
            DATA_NOSINT1 = 0b1000_0000,
            DATA_NOUNITEXT = 0b0100_0000,
            DATA_NOINTERVAL = 0b0010_0000,
            DATA_NOTIME = 0b0001_0000,
            DATA_NODATE = 0b0000_1000,
            BLOB_NONCHAR_SCSU = 0b0000_0100,
            BLOB_NONCHAR_8 = 0b0000_0010,
            BLOB_NONCHAR_16 = 0b0000_0001
        }

        [Flags]
        private enum Byte26 : byte
        {
            None = 0,
            IMAGE_NONCHAR = 0b1000_0000,
            DATA_NONLBIN = 0b0100_0000,
            NO_WIDETABLES = 0b0010_0000,
            DATA_NOUINTN = 0b0001_0000,
            DATA_NOUINT8 = 0b0000_1000,
            DATA_NOUINT4 = 0b0000_0100,
            DATA_NOUINT2 = 0b0000_0010,
            RES_RESERVED1 = 0b0000_0001
        }

        [Flags]
        private enum Byte27 : byte
        {
            None = 0,
            OBJECT_NOBINARY = 0b1000_0000,
            DATA_NOCOLUMNSTATUS = 0b0100_0000,
            OBJECT_NOCHAR = 0b0010_0000,
            OBJECT_NOJAVA1 = 0b0001_0000,
            DATA_NOINT8 = 0b0000_1000,
            RES_NOSTRIPBLANKS = 0b0000_0100,
            RES_NOTDSDEBUG = 0b0000_0010,
            DATA_NOBOUNDARY = 0b0000_0001
        }

        [Flags]
        private enum Byte28 : byte
        {
            None = 0,
            DATA_NOSENSITIVITY = 0b1000_0000,
            PROTO_NOBULK = 0b0100_0000,
            PROTO_NOTEXT = 0b0010_0000,
            CON_NOINBAND = 0b0001_0000,
            CON_NOOOB = 0b0000_1000,
            DATA_NOMONEYN = 0b0000_0100,
            DATA_NODATETIMEN = 0b0000_0010,
            DATA_NOINTN = 0b0000_0001
        }

        [Flags]
        private enum Byte29 : byte
        {
            None = 0,
            DATA_NOLBIN = 0b1000_0000,
            DATA_NOLCHAR = 0b0100_0000,
            DATA_NODEC = 0b0010_0000,
            DATA_NOIMAGE = 0b0001_0000,
            DATA_NOTEXT = 0b0000_1000,
            DATA_NONUM = 0b0000_0100,
            DATA_NOFLT8 = 0b0000_0010,
            DATA_NOFLT4 = 0b0000_0001
        }

        [Flags]
        private enum Byte30 : byte
        {
            None = 0,
            DATA_NODATE4 = 0b1000_0000,
            DATA_NODATE8 = 0b0100_0000,
            DATA_NOMNY4 = 0b0010_0000,
            DATA_NOMNY8 = 0b0001_0000,
            DATA_NOVBIN = 0b0000_1000,
            DATA_NOBIN = 0b0000_0100,
            DATA_NOVCHAR = 0b0000_0010,
            DATA_NOCHAR = 0b0000_0001
        }

        [Flags]
        private enum Byte31 : byte
        {
            None = 0,
            DATA_NOBIT = 0b1000_0000,
            DATA_NOINT4 = 0b0100_0000,
            DATA_NOINT2 = 0b0010_0000,
            DATA_NOINT1 = 0b0001_0000,
            RES_NOPARAM = 0b0000_1000,
            RES_NOEED = 0b0000_0100,
            RES_NOMSG = 0b0000_0010,
            NONE = 0b0000_0001
        }

        // ReSharper restore IdentifierTypo
        // ReSharper restore UnusedMember.Local
    }
}
