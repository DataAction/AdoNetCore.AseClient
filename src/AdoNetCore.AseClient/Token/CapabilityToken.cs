using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class CapabilityToken : IToken
    {
        //todo: create fields to represent capabilities
        public TokenType Type => TokenType.TDS_CAPABILITY;

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

        //from .net 4 client
        private readonly byte[] _capabilityBytes = {
            //cap request
            0x01, 0x0e, 0x01, 0xef, 0xff, 0x69, 0xb7, 0xfd, 0xff, 0xaf, 0x65, 0x41, 0xff, 0xff, 0xff, 0xd6,
            //cap response
            0x02, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x88, 0x40, 0x00, 0x01, 0x02, 0x48, 0x00, 0x00, 0x00
        };

        /* Info from ribo:
CAPABILITY Token (0xE2); variable length.
  Length [2]:                   32
  Type [1]:                     CAP_REQUEST
  Mask [14]:                    
0xEF (11101111): REQ_UNUSED, REQ_UNUSED, DATA_LOBLOCATOR, (REQ_RPC_BATCH), REQ_LANG_BATCH, REQ_DYN_BATCH, REQ_GRID, REQ_INSTID 
0xFF (11111111): RPCPARAM_LOB, DATA_USECS, DATA_BIGDATETIME, REQ_UNUSED, REQ_UNUSED, MULTI_REQUESTS, REQ_MIGRATE, REQ_UNUSED 
0x69 (01101001): (REQ_DBRPC2), REQ_CURINFO3, DATA_XML, (REQ_BLOB_NCHAR_16), REQ_LARGEIDENT, (DATA_SINT1), (CAP_CLUSTERFAILOVER), DATA_UNITEXT 
0xB7 (10110111): REQ_SRVPKTSIZE, (CSR_KEYSETDRIVEN), CSR_SEMISENSITIVE, CSR_INSENSITIVE, (CSR_SENSITIVE), CSR_SCROLL, DATA_INTERVAL, DATA_TIME 
0xFD (11111101): DATA_DATE, BLOB_NCHAR_SCSU, BLOB_NCHAR_8, BLOB_NCHAR_16, IMAGE_NCHAR, DATA_NLBIN, (CUR_IMPLICIT), DATA_UINTN 
0xFF (11111111): DATA_UINT8, DATA_UINT4, DATA_UINT2, REQ_RESERVED2, WIDETABLE, DATA_COLUMNSTATUS, OBJECT_BINARY, REQ_RESERVED1 
0xAF (10101111): OBJECT_CHAR, (OBJECT_JAVA1), DOL_BULK, (DATA_VOID), DATA_INT8, DATA_BITN, DATA_FLTN, PROTO_DYNPROC 
0x65 (01100101): (PROTO_DYNAMIC), DATA_BOUNDARY, DATA_SENSITIVITY, (REQ_URGEVT), (PROTO_BULK), PROTO_TEXT, (CON_LOGICAL), CON_INBAND 
0x41 (01000001): (CON_OOB), CSR_MULTI, (CSR_REL), (CSR_ABS), (CSR_LAST), (CSR_FIRST), (CSR_PREV), DATA_MONEYN 
0xFF (11111111): DATA_DATETIMEN, DATA_INTN, DATA_LBIN, DATA_LCHAR, DATA_DEC, DATA_IMAGE, DATA_TEXT, DATA_NUM 
0xFF (11111111): DATA_FLT8, DATA_FLT4, DATA_DATE4, DATA_DATE8, DATA_MNY4, DATA_MNY8, DATA_VBIN, DATA_BIN 
0xFF (11111111): DATA_VCHAR, DATA_CHAR, DATA_BIT, DATA_INT4, DATA_INT2, DATA_INT1, REQ_PARAM, REQ_MSG 
0xD6 (11010110): REQ_DYNF, REQ_CURSOR, (REQ_BCP), REQ_MSTMT, (REQ_EVT), REQ_RPC, REQ_LANG, (NONE) 

  Type [1]:                     CAP_RESPONSE
  Mask [14]:                    
0x88 (10001000): RES_UNUSED, (DATA_NOLOBLOCATOR), (RES_UNUSED), (RPCPARAM_NOLOB), RES_NO_TDSCONTROL, (DATA_NOUSECS), (DATA_NOBIGDATETIME), (RES_FORCE_ROWFMT2) 
0x40 (01000000): (RES_SUPPRESS_DONEINPROC), RES_SUPPRESS_FMT, (RES_NOXNLDATA), (NONINT_RETURN_VALUE), (RES_NODATA_XML), (NO_SRVPKTSIZE), (RES_NOBLOB_NCHAR_16), (RES_NOLARGEIDENT) 
0x00 (00000000): (DATA_NOSINT1), (DATA_NOUNITEXT), (DATA_NOINTERVAL), (DATA_NOTIME), (DATA_NODATE), (BLOB_NONCHAR_SCSU), (BLOB_NONCHAR_8), (BLOB_NONCHAR_16) 
0x01 (00000001): (IMAGE_NONCHAR), (DATA_NONLBIN), (NO_WIDETABLES), (DATA_NOUINTN), (DATA_NOUINT8), (DATA_NOUINT4), (DATA_NOUINT2), RES_RESERVED1 
0x02 (00000010): (OBJECT_NOBINARY), (DATA_NOCOLUMNSTATUS), (OBJECT_NOCHAR), (OBJECT_NOJAVA1), (DATA_NOINT8), (RES_NOSTRIPBLANKS), RES_NOTDSDEBUG, (DATA_NOBOUNDARY) 
0x48 (01001000): (DATA_NOSENSITIVITY), PROTO_NOBULK, (PROTO_NOTEXT), (CON_NOINBAND), CON_NOOOB, (DATA_NOMONEYN), (DATA_NODATETIMEN), (DATA_NOINTN) 
0x00 (00000000): (DATA_NOLBIN), (DATA_NOLCHAR), (DATA_NODEC), (DATA_NOIMAGE), (DATA_NOTEXT), (DATA_NONUM), (DATA_NOFLT8), (DATA_NOFLT4) 
0x00 (00000000): (DATA_NODATE4), (DATA_NODATE8), (DATA_NOMNY4), (DATA_NOMNY8), (DATA_NOVBIN), (DATA_NOBIN), (DATA_NOVCHAR), (DATA_NOCHAR) 
0x00 (00000000): (DATA_NOBIT), (DATA_NOINT4), (DATA_NOINT2), (DATA_NOINT1), (RES_NOPARAM), (RES_NOEED), (RES_NOMSG), (NONE) 
         */

        public static CapabilityToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new CapabilityToken();
            t.Read(stream, env, previous);
            return t;
        }
    }
}
