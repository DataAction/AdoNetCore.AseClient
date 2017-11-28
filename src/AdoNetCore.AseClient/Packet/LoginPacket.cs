using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Packet
{
    public class LoginPacket : IPacket
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ProcessId { get; set; }
        public string ApplicationName { get; set; }
        public string ServerName { get; set; }
        public string Language { get; set; }
        public string Charset { get; set; }
        public string ClientLibrary { get; set; }

        public CapabilityToken Capability { get; set; }

        public LInt2 LInt2 { get; set; } = LInt2.TDS_INT2_LSB_LO;
        public LInt4 LInt4 { get; set; } = LInt4.TDS_INT4_LSB_LO;
        public LChar LChar { get; set; } = LChar.TDS_CHAR_ASCII;
        public LFlt LFlt { get; set; } = LFlt.TDS_FLT_IEEE_LO;
        public LDt LDt { get; set; } = LDt.TDS_TWO_I4_LSB_LO;
        public LInterfaceSpare LInterfaceSpare { get; set; } = LInterfaceSpare.TDS_LDEFSQL;
        public LType LType { get; set; } = LType.TDS_NONE;

        public LNoShort LNoShort { get; set; } = LNoShort.TDS_NOCVT_SHORT;
        public LFlt4 LFlt4 { get; set; } = LFlt4.TDS_FLT4_IEEE_LO;
        public LDate4 LDate4 { get; set; } = LDate4.TDS_TWO_I2_LSB_LO;
        public LSetLang LSetLang { get; set; } = LSetLang.TDS_NOTIFY;
        public LSetCharset LSetCharset { get; set; } = LSetCharset.TDS_NOTIFY;

        public int PacketSize { get; set; }

        public LoginPacket(string hostname, string username, string password, string processId, string applicationName, string serverName, string language, string charset, string clientLibrary, int packetSize, CapabilityToken capability)
        {
            Capability = capability;
            Hostname = hostname ?? string.Empty;
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
            ProcessId = processId ?? string.Empty;
            ApplicationName = applicationName ?? string.Empty;
            ServerName = serverName ?? string.Empty;
            Language = language ?? string.Empty;
            Charset = charset ?? string.Empty;
            ClientLibrary = clientLibrary ?? string.Empty;
            PacketSize = packetSize;
        }

        private int TDS_MAXNAME = 30;
        private int TDS_PROGNLEN = 10;
        private int TDS_RPLEN = 15 * 16 + 12 + 1;
        private int TDS_PKTLEN = 6;

        public BufferType Type => BufferType.TDS_BUF_LOGIN;
        public void Write(Stream stream, Encoding enc)
        {
            stream.WritePaddedString(Hostname, TDS_MAXNAME, enc); //lhostname
            stream.WritePaddedString(Username, TDS_MAXNAME, enc); //lussername
            stream.WritePaddedString(Password, TDS_MAXNAME, enc); //lpw
            stream.WritePaddedString(ProcessId, TDS_MAXNAME, enc); //lhostproc

            stream.Write(new byte[]
            {
                (byte) LInt2,
                (byte) LInt4,
                (byte) LChar,
                (byte) LFlt,
                (byte) LDt,
                (byte) LUseDb.TRUE, //lusedb
                (byte) LDmpLd.FALSE, //ldmpld
                (byte) LInterfaceSpare,
                (byte) LType,
                0, 0, 0, 0, //lbufsize
                0, 0, 0, //lspare
            });

            stream.WritePaddedString(ApplicationName, TDS_MAXNAME, enc); //lappname
            stream.WritePaddedString(ServerName, TDS_MAXNAME, enc); //lservname

            //spec's a bit weird when it comes to this bit... following ADO.net driver
            //lrempw, lrempwlen
            stream.WriteWeirdPasswordString(Password, TDS_RPLEN, enc);

            //ltds version
            stream.Write(new byte[] { 5, 0, 0, 0 });
            stream.WritePaddedString(ClientLibrary, TDS_PROGNLEN, enc);
            stream.Write(new byte[] { 0x0f, 0x07, 0x00, 0x0d }); //lprogvers //doesn't matter what this value is really

            stream.Write(new []
            {
                (byte) LNoShort,
                (byte) LFlt4,
                (byte) LDate4
            });

            stream.WritePaddedString(Language, TDS_MAXNAME, enc);//llanguage

            stream.Write(new byte[]
            {
                (byte)LSetLang,
                0,0,0,0,8,0,0,0,0,0,0,0,0//loldsecure, lseclogin, lsecbulk, lhalogin, lhasessionid, lsecspare
            });

            stream.WritePaddedString(Charset, TDS_MAXNAME, enc); //lcharset
            stream.WriteByte((byte)LSetCharset); //lsetcharset
            stream.WritePaddedString(PacketSize.ToString(), TDS_PKTLEN, enc);//lpacketsize
            //ldummy
            stream.Write(new byte[]
            {
                //0xDE, 0xAD, 0xBE, 0xEF
                0x00, 0x00, 0x00, 0x00
            });

            Capability.Write(stream, enc);
        }

        public void Read(Stream stream, Encoding enc)
        {
            throw new System.NotImplementedException();
        }
    }
}
