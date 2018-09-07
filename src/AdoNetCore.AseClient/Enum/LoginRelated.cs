// ReSharper disable InconsistentNaming
namespace AdoNetCore.AseClient.Enum
{
    internal enum LInt2 : byte
    {
        TDS_INT2_LSB_HI = 2,
        TDS_INT2_LSB_LO = 3,
    }

    internal enum LInt4 : byte
    {
        TDS_INT4_LSB_HI = 0,
        TDS_INT4_LSB_LO = 1,
    }

    internal enum LChar : byte
    {
        TDS_CHAR_ASCII = 6,
        TDS_CHAR_EBCDIC = 7,
    }

    internal enum LFlt : byte
    {
        TDS_FLT_IEEE_HI = 4,
        TDS_FLT_VAXD = 5,
        TDS_FLT_IEEE_LO = 10,
        TDS_FLT_ND5000 = 11,
    }

    internal enum LDt : byte
    {
        TDS_TWO_I4_LSB_HI = 8,
        TDS_TWO_I4_LSB_LO = 9,
    }

    internal enum LUseDb : byte
    {
        TRUE = 1,
        FALSE = 0
    }

    internal enum LDmpLd : byte
    {
        TRUE = 1,
        FALSE = 0
    }

    internal enum LInterfaceSpare : byte
    {
        TDS_LDEFSQL = 0,
        TDS_LXSQL = 1,
        TDS_LSQL = 2,
        TDS_LSQL2_1 = 3,
        TDS_LSQL2_2 = 4,
        TDS_LOG_SUCCEED = 5,
        TDS_LOG_FAIL = 6,
        TDS_LOG_NEG = 7,
        TDS_LOG_SECSESS_ACK = 0x08
    }

    internal enum LType : byte
    {
        TDS_NONE = 0x00, //added for our own use
        TDS_LSERVER = 0x01,
        TDS_LREMUSER = 0x02,
        TDS_LINTERNAL_RPC = 0x04
    }

    internal enum LNoShort : byte
    {
        TDS_CVT_SHORT = 1,
        TDS_NOCVT_SHORT = 0
    }

    internal enum LFlt4 : byte
    {
        TDS_FLT4_IEEE_HI = (12),
        TDS_FLT4_IEEE_LO = (13),
        TDS_FLT4_VAXF = (14),
        TDS_FLT4_ND50004 = (15)
    }

    internal enum LDate4 : byte
    {
        TDS_TWO_I2_LSB_HI = (16),
        TDS_TWO_I2_LSB_LO = (17)
    }

    internal enum LSetLang : byte
    {
        TDS_NOTIFY = (1),
        TDS_NO_NOTIFY = (0)
    }

    internal enum LSecLogin : byte
    {
        TDS_SEC_LOG_ENCRYPT = (0x01),
        TDS_SEC_LOG_CHALLENGE = (0x02),
        TDS_SEC_LOG_LABELS = (0x04),
        TDS_SEC_LOG_APPDEFINED = (0x08),
        TDS_SEC_LOG_SECSESS = (0x10),
        TDS_SEC_LOG_ENCRYPT2 = (0x20)
    }

    internal enum LSecBulk : byte
    {
        TDS_SEC_BULK_LABELED = (0x01)
    }

    internal enum LHaLogin : byte
    {
        TDS_HA_LOG_SESSION = (0x01),
        TDS_HA_LOG_RESUME = (0x02),
        TDS_HA_LOG_FAILOVERSRV = (0x04),
        TDS_HA_LOG_REDIRECT = (0x08),
        TDS_HA_LOG_MIGRATE = (0x10)
    }

    internal enum LSetCharset : byte
    {
        TDS_NOTIFY = (1),
        TDS_NO_NOTIFY = (0)
    }
}
