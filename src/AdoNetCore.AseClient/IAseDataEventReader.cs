using System;
using System.Data;

namespace AdoNetCore.AseClient
{
    public interface IAseDataCallbackReader
    {
        bool GetBoolean(int i);
        byte GetByte(int i);
        long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        char GetChar(int i);
        long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length);
        string GetDataTypeName(int ordinal);
        DateTime GetDateTime(int i);
        TimeSpan GetTimeSpan(int i);
        decimal GetDecimal(int i);
        double GetDouble(int i);
        Type GetFieldType(int i);
        float GetFloat(int i);
        Guid GetGuid(int i);
        short GetInt16(int i);
        int GetInt32(int i);
        long GetInt64(int i);
        ushort GetUInt16(int i);
        uint GetUInt32(int i);
        ulong GetUInt64(int i);
        string GetString(int i);
        //AseDecimal GetAseDecimal(int ordinal);
        string GetName(int i);
        int GetOrdinal(string name);
        object GetValue(int i);
        int GetValues(object[] values);
        bool IsDBNull(int i);
        int FieldCount { get; }
        int VisibleFieldCount { get; }
        object this[int ordinal] { get; }
        object this[string name] { get; }
        DataTable GetSchemaTable();
        int Depth { get; }
        bool ContainsListCollection { get; }
    }
}
