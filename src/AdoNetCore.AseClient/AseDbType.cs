using System.Data;
// ReSharper disable UnusedMember.Global

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Specifies ASE datatypes. 
    /// </summary>
    public enum AseDbType
    {
        BigInt = DbType.Int64,
        Binary = DbType.Binary,
        Bit = DbType.Boolean,
        Char = DbType.AnsiStringFixedLength,
        Date = DbType.Date,
        DateTime = DbType.DateTime,
        Decimal = DbType.Decimal,
        Double = DbType.Double,
        //Float = DbType.Single, // NOTE - this was missing from the docs: http://infocenter.sybase.com/help/topic/com.sybase.help.sdk_12.5.1.adonet/html/adonet/Asadbtype_apiref.htm
        Integer = DbType.Int32,
        Image = DbType.Binary,
        LongVarChar = DbType.AnsiString, // NOTE - this was missing from the docs: http://infocenter.sybase.com/help/topic/com.sybase.help.sdk_12.5.1.adonet/html/adonet/Asadbtype_apiref.htm
        Money = DbType.Currency,
        NChar = DbType.AnsiStringFixedLength,
        Numeric = DbType.VarNumeric,
        NVarChar = DbType.AnsiString,
        Real = DbType.Single,
        SmallDateTime = DbType.DateTime,
        SmallInt = DbType.Int16, // NOTE - this was missing from the docs: http://infocenter.sybase.com/help/topic/com.sybase.help.sdk_12.5.1.adonet/html/adonet/Asadbtype_apiref.htm
        SmallMoney = DbType.Currency,
        Text = DbType.AnsiString,
        Unitext = DbType.String,
        Time = DbType.Time,
        TimeStamp = DbType.Binary,
        TinyInt = DbType.Byte,
        UniChar = DbType.StringFixedLength,
        UniVarChar = DbType.String,
        UnsignedBigInt = DbType.UInt64,
        UnsignedInt = DbType.UInt32,
        UnsignedSmallInt = DbType.UInt16,
        VarBinary = DbType.Binary,
        VarChar = DbType.AnsiString
    }
}