using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class ClientCapabilityToken : IToken
    {
        public TokenType Type => TokenType.TDS_CAPABILITY;

        private readonly byte[] _capabilityBytes;

        public ClientCapabilityToken(bool enableServerPacketSize = true)
        {
            var packetSizeRequestBit = enableServerPacketSize ? Request4.REQ_SRVPKTSIZE : Request4.None;

            _capabilityBytes =  new[] {
                //cap request
                ((byte)TDS_CAPABILITY_TYPE.TDS_CAP_REQUEST),
                ((byte)14), // Request capability bits length
                ((byte)(/*Request0.REQ_UNUSED1 | Request0.REQ_UNUSED2 | Request0.REQ_UNUSED3 | Request0.REQ_UNUSED4 | Request0.REQ_UNUSED5 | Request0.REQ_COMMAND_ENCRYPTION | Request0.REQ_READONLY |*/ Request0.REQ_DYNAMIC_SUPPRESS_PARAMFMT)),
                ((byte)(Request1.REQ_LOGPARAMS | Request1.REQ_ROWCOUNT_FOR_SELECT | Request1.DATA_LOBLOCATOR/*| Request1.REQ_RPC_BATCH*/| Request1.REQ_LANG_BATCH | Request1.REQ_DYN_BATCH | Request1.REQ_GRID | Request1.REQ_INSTID)),
                ((byte)(Request2.RPCPARAM_LOB | Request2.DATA_USECS | Request2.DATA_BIGDATETIME | Request2.REQ_UNUSED4 | Request2.REQ_UNUSED5 | Request2.MULTI_REQUESTS | Request2.REQ_MIGRATE | Request2.REQ_UNUSED8)),
                ((byte)(/*Request3.REQ_DBRPC2 | */Request3.REQ_CURINFO3 | Request3.DATA_XML/* | Request3.REQ_BLOB_NCHAR_16*/ | Request3.REQ_LARGEIDENT/* | Request3.DATA_SINT1 | Request3.CAP_CLUSTERFAILOVER*/ | Request3.DATA_UNITEXT)),
                ((byte)(packetSizeRequestBit/* | Request4.CSR_KEYSETDRIVEN | Request4.CSR_SEMISENSITIVE | Request4.CSR_INSENSITIVE | Request4.CSR_SENSITIVE | Request4.CSR_SCROLL | Request4.DATA_INTERVAL*/ | Request4.DATA_TIME)),
                ((byte)(Request5.DATA_DATE | Request5.BLOB_NCHAR_SCSU | Request5.BLOB_NCHAR_8 | Request5.BLOB_NCHAR_16 | Request5.IMAGE_NCHAR | Request5.DATA_NLBIN/* | Request5.CUR_IMPLICIT*/ | Request5.DATA_UINTN)),
                ((byte)(Request6.DATA_UINT8 | Request6.DATA_UINT4 | Request6.DATA_UINT2 | Request6.REQ_RESERVED2 | Request6.WIDETABLE | Request6.REQ_RESERVED1 | Request6.OBJECT_BINARY | Request6.DATA_COLUMNSTATUS)),
                ((byte)(Request7.OBJECT_CHAR/* | Request7.OBJECT_JAVA1 | Request7.DOL_BULK | Request7.DATA_VOID*/ | Request7.DATA_INT8 | Request7.DATA_BITN | Request7.DATA_FLTN/* | Request7.PROTO_DYNPROC*/)),
                ((byte)(Request8.None/*Request8.PROTO_DYNAMIC | Request8.DATA_BOUNDARY | Request8.DATA_SENSITIVITY | Request8.REQ_URGEVT | Request8.PROTO_BULK | Request8.PROTO_TEXT | Request8.CON_LOGICAL | Request8.CON_INBAND*/)),
                ((byte)(/*Request9.CON_OOB | Request9.CSR_MULTI | Request9.CSR_REL | Request9.CSR_ABS | Request9.CSR_LAST | Request9.CSR_FIRST | Request9.CSR_PREV |*/ Request9.DATA_MONEYN)),
                ((byte)(Request10.DATA_DATETIMEN | Request10.DATA_INTN | Request10.DATA_LBIN | Request10.DATA_LCHAR | Request10.DATA_DEC | Request10.DATA_IMAGE | Request10.DATA_TEXT | Request10.DATA_NUM)),
                ((byte)(Request11.DATA_FLT8 | Request11.DATA_FLT4 | Request11.DATA_DATE4 | Request11.DATA_DATE8 | Request11.DATA_MNY4 | Request11.DATA_MNY8 | Request11.DATA_VBIN | Request11.DATA_BIN)),
                ((byte)(Request12.DATA_VCHAR | Request12.DATA_CHAR | Request12.DATA_BIT | Request12.DATA_INT4 | Request12.DATA_INT2 | Request12.DATA_INT1 | Request12.REQ_PARAM | Request12.REQ_MSG)),
                ((byte)(Request13.REQ_DYNF /*| Request13.REQ_CURSOR | Request13.REQ_BCP*/ | Request13.REQ_MSTMT/* | Request13.REQ_EVT*/ | Request13.REQ_RPC | Request13.REQ_LANG/* | Request13.NONE*/)),

                //cap response
                ((byte)TDS_CAPABILITY_TYPE.TDS_CAP_RESPONSE),
                ((byte)14), // Response capability bits length
                ((byte)Response0.RES_UNUSED),
                ((byte)Response1.RES_UNUSED),
                ((byte)Response2.RES_UNUSED),
                ((byte)Response3.RES_UNUSED),
                ((byte)Response4.RES_UNUSED),
                ((byte)(Response5.RES_UNUSED1/* | Response5.DATA_NOLOBLOCATOR | Response5.RES_UNUSED2 | Response5.RPCPARAM_NOLOB*/ | Response5.RES_NO_TDSCONTROL/* | Response5.DATA_NOUSECS | Response5.DATA_NOBIGDATETIME | Response5.RES_FORCE_ROWFMT2*/)),
                ((byte)(/*Response6.RES_SUPPRESS_DONEINPROC | */Response6.RES_SUPPRESS_FMT/* | Response6.RES_NOXNLDATA | Response6.NONINT_RETURN_VALUE | Response6.RES_NODATA_XML | Response6.NO_SRVPKTSIZE | Response6.RES_NOBLOB_NCHAR_16 | Response6.RES_NOLARGEIDENT*/)),
                ((byte)(/*Response7.DATA_NOSINT1 | Response7.DATA_NOUNITEXT |*/ Response7.DATA_NOINTERVAL /*| Response7.DATA_NOTIME | Response7.DATA_NODATE | Response7.BLOB_NONCHAR_SCSU | Response7.BLOB_NONCHAR_8 | Response7.BLOB_NONCHAR_16*/)),
                ((byte)(/*Response8.IMAGE_NONCHAR | Response8.DATA_NONLBIN | Response8.NO_WIDETABLES | Response8.DATA_NOUINTN | Response8.DATA_NOUINT8 | Response8.DATA_NOUINT4 | Response8.DATA_NOUINT2 | */Response8.RES_RESERVED1)),
                ((byte)(/*Response9.OBJECT_NOBINARY | Response9.DATA_NOCOLUMNSTATUS | Response9.OBJECT_NOCHAR | Response9.OBJECT_NOJAVA1 | Response9.DATA_NOINT8 | Response9.RES_NOSTRIPBLANKS | */Response9.RES_NOTDSDEBUG | Response9.DATA_NOBOUNDARY)),
                ((byte)(Response10.DATA_NOSENSITIVITY | Response10.PROTO_NOBULK/* | Response10.PROTO_NOTEXT | Response10.CON_NOINBAND*/ | Response10.CON_NOOOB/* | Response10.DATA_NOMONEYN | Response10.DATA_NODATETIMEN | Response10.DATA_NOINTN*/)),
                ((byte)(Response11.None/* | Response11.DATA_NOLBIN | Response11.DATA_NOLCHAR | Response11.DATA_NODEC | Response11.DATA_NOIMAGE | Response11.DATA_NOTEXT | Response11.DATA_NONUM | Response11.DATA_NOFLT8 | Response11.DATA_NOFLT4*/)),
                ((byte)(Response12.None/* | Response12.DATA_NODATE4 | Response12.DATA_NODATE8 | Response12.DATA_NOMNY4 | Response12.DATA_NOMNY8 | Response12.DATA_NOVBIN | Response12.DATA_NOBIN | Response12.DATA_NOVCHAR | Response12.DATA_NOCHAR*/)),
                ((byte)(Response13.None/* | Response13.DATA_NOBIT | Response13.DATA_NOINT4 | Response13.DATA_NOINT2 | Response13.DATA_NOINT1 | Response13.RES_NOPARAM | Response13.RES_NOEED | Response13.RES_NOMSG | Response13.NONE*/)),
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
            throw new NotSupportedException();
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable IdentifierTypo

        // ReSharper disable InconsistentNaming
        private enum TDS_CAPABILITY_TYPE : byte
        {
            TDS_CAP_REQUEST = 1,
            TDS_CAP_RESPONSE = 2
        }
        // ReSharper restore InconsistentNaming

        [Flags]
        private enum Request0 : byte
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
        private enum Request1 : byte
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
        private enum Request2 : byte
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
        private enum Request3 : byte
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
        private enum Request4 : byte
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
        private enum Request5 : byte
        {
            None = 0,
            DATA_DATE = 0b1000_0000,
            /// <summary>
            /// Support for BLOB subtype 0x05 (unichar) with serialization type2.
            /// </summary>
            BLOB_NCHAR_SCSU = 0b0100_0000,
            /// <summary>
            /// Support for BLOB subtype 0x05 (unichar) with serialization type 1.
            /// </summary>
            BLOB_NCHAR_8 = 0b0010_0000,
            /// <summary>
            /// Support for BLOB subtype 0x05 (unichar) with serialization type 0.
            /// </summary>
            BLOB_NCHAR_16 = 0b0001_0000,
            /// <summary>
            /// Support for IMAGE data containing UTF-16 encoded data (usertype 36).
            /// </summary>
            IMAGE_NCHAR = 0b0000_1000,
            /// <summary>
            /// Support for LONGBINARY data containing UTF-16 encoded data (usertypes 34 and 35)
            /// </summary>
            DATA_NLBIN = 0b0000_0100,
            /// <summary>
            /// Support for TDS_CUR_DOPT_IMPLICIT cursor declare option.
            /// </summary>
            CUR_IMPLICIT = 0b0000_0010,
            /// <summary>
            /// Support for NULL unsigned integers
            /// </summary>
            DATA_UINTN = 0b0000_0001
        }

        [Flags]
        private enum Request6 : byte
        {
            None = 0,
            /// <summary>
            /// Support for unsigned 8-byte integers
            /// </summary>
            DATA_UINT8 = 0b1000_0000,
            /// <summary>
            /// Support for unsigned 4-byte integers
            /// </summary>
            DATA_UINT4 = 0b0100_0000,
            /// <summary>
            /// Support for unsigned 2-byte integers
            /// </summary>
            DATA_UINT2 = 0b0010_0000,
            /// <summary>
            /// Reserved
            /// </summary>
            REQ_RESERVED2 = 0b0001_0000,
            /// <summary>
            /// The client may send requests using the CURDECLARE2, DYNAMIC2, PARAMFMT2 tokens.
            /// </summary>
            WIDETABLE = 0b0000_1000,
            /// <summary>
            /// Reserved for future use
            /// </summary>
            REQ_RESERVED1 = 0b0000_0100,
            /// <summary>
            /// Streaming Binary data
            /// </summary>
            OBJECT_BINARY = 0b0000_0010,
            /// <summary>
            /// Indicates that a one-byte status field proceeds any length or data (etc.) for every
            /// column within a row using TDS_ROW or TDS_PARAMS
            /// </summary>
            DATA_COLUMNSTATUS = 0b0000_0001
        }

        [Flags]
        private enum Request7 : byte
        {
            None = 0,
            /// <summary>
            /// Support Streaming character data
            /// </summary>
            OBJECT_CHAR = 0b1000_0000,
            /// <summary>
            /// Support Serialized Java Objects
            /// </summary>
            OBJECT_JAVA1 = 0b0100_0000,
            /// <summary>
            /// ?
            /// </summary>
            DOL_BULK = 0b0010_0000,
            /// <summary>
            /// ?
            /// </summary>
            DATA_VOID = 0b0001_0000,
            /// <summary>
            /// Support 8 byte integers
            /// </summary>
            DATA_INT8 = 0b0000_1000,
            /// <summary>
            /// Support NULL bits
            /// </summary>
            DATA_BITN = 0b0000_0100,
            /// <summary>
            /// Support NULL floats
            /// </summary>
            DATA_FLTN = 0b0000_0010,
            /// <summary>
            /// Pre-pend “create proc” to dynamic prepare statements
            /// </summary>
            PROTO_DYNPROC = 0b0000_0001
        }

        [Flags]
        private enum Request8 : byte
        {
            None = 0,
            /// <summary>
            /// Use DESCIN/DESCOUT dynamic protocol
            /// </summary>
            PROTO_DYNAMIC = 0b1000_0000,
            /// <summary>
            /// Support boundary security data types
            /// </summary>
            DATA_BOUNDARY = 0b0100_0000,
            /// <summary>
            /// Support sensitivity security data types
            /// </summary>
            DATA_SENSITIVITY = 0b0010_0000,
            /// <summary>
            /// Use new event notification protocol
            /// </summary>
            REQ_URGEVT = 0b0001_0000,
            /// <summary>
            /// Support tokenized bulk copy (not supported this release - TDS 5.0)
            /// </summary>
            PROTO_BULK = 0b0000_1000,
            /// <summary>
            /// Support tokenized text and image (not supported in this release - TDS 5.0)
            /// </summary>
            PROTO_TEXT = 0b0000_0100,
            /// <summary>
            /// Support logical logout (not supported in this release - TDS 5.0)
            /// </summary>
            CON_LOGICAL = 0b0000_0010,
            /// <summary>
            /// Support non-expedited attentions
            /// </summary>
            CON_INBAND = 0b0000_0001 
        }

        [Flags]
        private enum Request9 : byte
        {
            None = 0,
            /// <summary>
            /// Support expedited attentions.
            /// </summary>
            CON_OOB = 0b1000_0000,
            /// <summary>
            /// Support multi-row fetch cursor commands
            /// </summary>
            CSR_MULTI = 0b0100_0000,
            /// <summary>
            /// Support fetch specified relative row cursor commands
            /// </summary>
            CSR_REL = 0b0010_0000,
            /// <summary>
            /// Support fetch specified absolute row cursor commands
            /// </summary>
            CSR_ABS = 0b0001_0000,
            /// <summary>
            /// Support fetch last row cursor commands
            /// </summary>
            CSR_LAST = 0b0000_1000,
            /// <summary>
            /// Support fetch first row cursor commands
            /// </summary>
            CSR_FIRST = 0b0000_0100,
            /// <summary>
            /// Support fetch previous cursor commands
            /// </summary>
            CSR_PREV = 0b0000_0010,
            /// <summary>
            /// Support NULL money
            /// </summary>
            DATA_MONEYN = 0b0000_0001
        }

        [Flags]
        private enum Request10 : byte
        {
            None = 0,
            /// <summary>
            /// Support NULL date/time
            /// </summary>
            DATA_DATETIMEN = 0b1000_0000,
            /// <summary>
            /// Support NULL integers
            /// </summary>
            DATA_INTN = 0b0100_0000,
            /// <summary>
            /// Support long variable length binary data types.
            /// </summary>
            DATA_LBIN = 0b0010_0000,
            /// <summary>
            /// Support long variable length character data types
            /// </summary>
            DATA_LCHAR = 0b0001_0000,
            /// <summary>
            /// Support decimal data types
            /// </summary>
            DATA_DEC = 0b0000_1000,
            /// <summary>
            /// Support image data types
            /// </summary>
            DATA_IMAGE = 0b0000_0100,
            /// <summary>
            /// Support text data types
            /// </summary>
            DATA_TEXT = 0b0000_0010,
            /// <summary>
            /// Support numeric data types
            /// </summary>
            DATA_NUM = 0b0000_0001
        }

        [Flags]
        private enum Request11 : byte
        {
            None = 0,
            /// <summary>
            /// Support 8 byte floating point data types
            /// </summary>
            DATA_FLT8 = 0b1000_0000,
            /// <summary>
            /// Support 4 byte floating point data types
            /// </summary>
            DATA_FLT4 = 0b0100_0000,
            /// <summary>
            /// Support 4 byte date/time data types
            /// </summary>
            DATA_DATE4 = 0b0010_0000,
            /// <summary>
            /// Support 8 byte date/time data types
            /// </summary>
            DATA_DATE8 = 0b0001_0000,
            /// <summary>
            /// Support 4 byte money data types
            /// </summary>
            DATA_MNY4 = 0b0000_1000,
            /// <summary>
            /// Support 8 byte money data types
            /// </summary>
            DATA_MNY8 = 0b0000_0100,
            /// <summary>
            /// Support variable length binary data types
            /// </summary>
            DATA_VBIN = 0b0000_0010,
            /// <summary>
            /// Support fixed length binary data types
            /// </summary>
            DATA_BIN = 0b0000_0001
        }

        [Flags]
        private enum Request12 : byte
        {
            None = 0,
            /// <summary>
            /// Support variable length character data types
            /// </summary>
            DATA_VCHAR = 0b1000_0000,
            /// <summary>
            /// Support fixed length character data types
            /// </summary>
            DATA_CHAR = 0b0100_0000,
            /// <summary>
            /// Support bit data types
            /// </summary>
            DATA_BIT = 0b0010_0000,
            /// <summary>
            /// Support 4 byte integers
            /// </summary>
            DATA_INT4 = 0b0001_0000,
            /// <summary>
            /// Support 2 byte integers
            /// </summary>
            DATA_INT2 = 0b0000_1000,
            /// <summary>
            /// Support 1 byte unsigned integers
            /// </summary>
            DATA_INT1 = 0b0000_0100,
            /// <summary>
            /// RPC requests will use the TDS_DBRPC token and TDS_PARAMFMT/TDS_PARAM to send parameters.
            /// </summary>
            REQ_PARAM = 0b0000_0010,
            /// <summary>
            /// TDS_MSG requests.
            /// </summary>
            REQ_MSG = 0b0000_0001
        }

        [Flags]
        private enum Request13 : byte
        {
            None = 0,
            /// <summary>
            /// Dynamic SQL requests.
            /// </summary>
            REQ_DYNF = 0b1000_0000,
            /// <summary>
            /// Cursor command requests.
            /// </summary>
            REQ_CURSOR = 0b0100_0000,
            /// <summary>
            /// Bulk copy requests.
            /// </summary>
            REQ_BCP = 0b0010_0000,
            /// <summary>
            /// Support multiple commands per request.
            /// </summary>
            REQ_MSTMT = 0b0001_0000,
            /// <summary>
            /// Registered procedure event notifications.
            /// </summary>
            REQ_EVT = 0b0000_1000,
            /// <summary>
            /// RPC Requests.
            /// </summary>
            REQ_RPC = 0b0000_0100,
            /// <summary>
            /// Language requests.
            /// </summary>
            REQ_LANG = 0b0000_0010,
            NONE = 0b0000_0001
        }

        [Flags]
        private enum Response0 : byte
        {
            None = 0,
            RES_UNUSED = None
        }

        [Flags]
        private enum Response1 : byte
        {
            None = 0,
            RES_UNUSED = None
        }

        [Flags]
        private enum Response2 : byte
        {
            None = 0,
            RES_UNUSED = None
        }

        [Flags]
        private enum Response3 : byte
        {
            None = 0,
            RES_UNUSED = None
        }

        [Flags]
        private enum Response4 : byte
        {
            None = 0,
            RES_UNUSED = None
        }
        
        [Flags]
        private enum Response5 : byte
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
        private enum Response6 : byte
        {
            None = 0,
            RES_SUPPRESS_DONEINPROC = 0b1000_0000,
            RES_SUPPRESS_FMT = 0b0100_0000,
            RES_NOXNLDATA = 0b0010_0000,
            NONINT_RETURN_VALUE = 0b0001_0000,
            RES_NODATA_XML = 0b0000_1000,
            NO_SRVPKTSIZE = 0b0000_0100, // You would think this tells the server not to respond with TDS_ENV_PACKSIZE tokens, but it doesn't seem to.
            RES_NOBLOB_NCHAR_16 = 0b0000_0010,
            RES_NOLARGEIDENT = 0b0000_0001
        }

        [Flags]
        private enum Response7 : byte
        {
            None = 0,
            DATA_NOSINT1 = 0b1000_0000,
            DATA_NOUNITEXT = 0b0100_0000,
            DATA_NOINTERVAL = 0b0010_0000,
            DATA_NOTIME = 0b0001_0000,
            DATA_NODATE = 0b0000_1000,
            /// <summary>
            /// No Support for BLOB subtype 0x05/2.
            /// </summary>
            BLOB_NONCHAR_SCSU = 0b0000_0100,
            /// <summary>
            /// No Support for BLOB subtype 0x05/1.
            /// </summary>
            BLOB_NONCHAR_8 = 0b0000_0010,
            /// <summary>
            /// No Support for BLOB subtype 0x05/0.
            /// </summary>
            BLOB_NONCHAR_16 = 0b0000_0001
        }

        [Flags]
        private enum Response8 : byte
        {
            None = 0,
            /// <summary>
            /// No Support for IMAGE data containing UTF-16 encoded data (usertype 36).
            /// </summary>
            IMAGE_NONCHAR = 0b1000_0000,
            /// <summary>
            /// No Support for LONGBINARY data containing UTF-16 encoded data (usertypes 34 and 35)
            /// </summary>
            DATA_NONLBIN = 0b0100_0000,
            /// <summary>
            /// Client cannot process the ORDERBY2, PARAMFMT2, and ROWFMT2 tokens required to support tables
            /// with a LARGE number of columns. The server should not send them.
            /// </summary>
            NO_WIDETABLES = 0b0010_0000,
            /// <summary>
            /// No Support for NULL unsigned integers
            /// </summary>
            DATA_NOUINTN = 0b0001_0000,
            /// <summary>
            /// No Support for unsigned 8-byte integers
            /// </summary>
            DATA_NOUINT8 = 0b0000_1000,
            /// <summary>
            /// No Support for unsigned 4-byte integers
            /// </summary>
            DATA_NOUINT4 = 0b0000_0100,
            /// <summary>
            /// No Support for unsigned 2-byte integers
            /// </summary>
            DATA_NOUINT2 = 0b0000_0010,
            /// <summary>
            /// Reserved for future use
            /// </summary>
            RES_RESERVED1 = 0b0000_0001
        }

        [Flags]
        private enum Response9 : byte
        {
            None = 0,
            /// <summary>
            /// No Streaming Binary data
            /// </summary>
            OBJECT_NOBINARY = 0b1000_0000,
            /// <summary>
            /// No Support for 0-length non-null strings
            /// </summary>
            DATA_NOZEROLEN = 0b0100_0000,
            /// <summary>
            /// No Support Streaming character data
            /// </summary>
            OBJECT_NOCHAR = 0b0010_0000,
            /// <summary>
            /// No Support Serialized Java Objects
            /// </summary>
            OBJECT_NOJAVA1 = 0b0001_0000,
            /// <summary>
            /// No support for 8 byte integers
            /// </summary>
            DATA_NOINT8 = 0b0000_1000,
            /// <summary>
            /// Do not strip blank from fixed length character data
            /// </summary>
            RES_NOSTRIPBLANKS = 0b0000_0100,
            /// <summary>
            /// No support for TDS_DEBUG token. Use image data instead.
            /// </summary>
            RES_NOTDSDEBUG = 0b0000_0010,
            /// <summary>
            /// No support for the security boundary data type
            /// </summary>
            DATA_NOBOUNDARY = 0b0000_0001
        }

        [Flags]
        private enum Response10 : byte
        {
            None = 0,
            /// <summary>
            /// No support for the security sensitivity data type
            /// </summary>
            DATA_NOSENSITIVITY = 0b1000_0000,
            /// <summary>
            /// No support for tokenized bulk copy
            /// </summary>
            PROTO_NOBULK = 0b0100_0000,
            /// <summary>
            /// No support for tokenized text and image.
            /// </summary>
            PROTO_NOTEXT = 0b0010_0000,
            /// <summary>
            /// No support for non-expedited attentions
            /// </summary>
            CON_NOINBAND = 0b0001_0000,
            /// <summary>
            /// No support for expedited attentions
            /// </summary>
            CON_NOOOB = 0b0000_1000,
            /// <summary>
            /// No support for nullable money data types
            /// </summary>
            DATA_NOMONEYN = 0b0000_0100,
            /// <summary>
            /// No support for nullable date/time data types
            /// </summary>
            DATA_NODATETIMEN = 0b0000_0010,
            /// <summary>
            /// No support for nullable integers
            /// </summary>
            DATA_NOINTN = 0b0000_0001
        }

        [Flags]
        private enum Response11 : byte
        {
            None = 0,
            /// <summary>
            /// No support for long variable length binary data types
            /// </summary>
            DATA_NOLBIN = 0b1000_0000,
            /// <summary>
            /// No support for long variable length character data types
            /// </summary>
            DATA_NOLCHAR = 0b0100_0000,
            /// <summary>
            /// No support for decimal data types
            /// </summary>
            DATA_NODEC = 0b0010_0000,
            /// <summary>
            /// No support for image data types
            /// </summary>
            DATA_NOIMAGE = 0b0001_0000,
            /// <summary>
            /// No support for text data types
            /// </summary>
            DATA_NOTEXT = 0b0000_1000,
            /// <summary>
            /// No support for numeric data types
            /// </summary>
            DATA_NONUM = 0b0000_0100,
            /// <summary>
            /// No support for 8 byte float data types
            /// </summary>
            DATA_NOFLT8 = 0b0000_0010,
            /// <summary>
            /// No support for 4 byte float data types
            /// </summary>
            DATA_NOFLT4 = 0b0000_0001
        }

        [Flags]
        private enum Response12 : byte
        {
            None = 0,
            /// <summary>
            /// No support for 4 byte date/time data types
            /// </summary>
            DATA_NODATE4 = 0b1000_0000,
            /// <summary>
            /// No support for 8 byte date/time data types
            /// </summary>
            DATA_NODATE8 = 0b0100_0000,
            /// <summary>
            /// No support for 4 byte money data types
            /// </summary>
            DATA_NOMNY4 = 0b0010_0000,
            /// <summary>
            /// No support for 8 byte money data types
            /// </summary>
            DATA_NOMNY8 = 0b0001_0000,
            /// <summary>
            /// No support for variable length binary data types
            /// </summary>
            DATA_NOVBIN = 0b0000_1000,
            /// <summary>
            /// No support for fixed length binary data types
            /// </summary>
            DATA_NOBIN = 0b0000_0100,
            /// <summary>
            /// No support for variable length character data types
            /// </summary>
            DATA_NOVCHAR = 0b0000_0010,
            /// <summary>
            /// No support for fixed length character data types
            /// </summary>
            DATA_NOCHAR = 0b0000_0001
        }

        [Flags]
        private enum Response13 : byte
        {
            None = 0,
            /// <summary>
            /// No support for bit data types
            /// </summary>
            DATA_NOBIT = 0b1000_0000,
            /// <summary>
            /// No support for 4 byte integers
            /// </summary>
            DATA_NOINT4 = 0b0100_0000,
            /// <summary>
            /// No support for 2 byte integers
            /// </summary>
            DATA_NOINT2 = 0b0010_0000,
            /// <summary>
            /// No support for 1 byte integers
            /// </summary>
            DATA_NOINT1 = 0b0001_0000,
            /// <summary>
            /// No support for TDS_PARAM/TDS_PARAMFMT for return parameter. use TDS_RETURNVALUE to return parameters to this client.
            /// </summary>
            RES_NOPARAM = 0b0000_1000,
            /// <summary>
            /// No support for TDS_EED token
            /// </summary>
            RES_NOEED = 0b0000_0100,
            /// <summary>
            /// No support for TDS_MSG results
            /// </summary>
            RES_NOMSG = 0b0000_0010,
            NONE = 0b0000_0001
        }

        // ReSharper restore IdentifierTypo
        // ReSharper restore UnusedMember.Local
    }
}
