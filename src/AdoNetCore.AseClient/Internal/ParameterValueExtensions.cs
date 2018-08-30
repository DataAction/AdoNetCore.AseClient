using System;
using System.Collections.Generic;
using System.Text;

namespace AdoNetCore.AseClient.Internal
{
    internal static class ParameterValueExtensions
    {
        internal static object AsSendableValue(this object value, AseDbType aseDbType)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            switch (value)
            {
                case string s:
                    return s.AsSendable();
                case char c:
                    return c.AsSendable();
                case byte[] b:
                    return b.AsSendable(aseDbType);
                default:
                    return value;
            }
        }

        private static string AsSendable(this string value)
        {
            return value.HandleTerminator().HandleEmpty();
        }

        private static string HandleTerminator(this string value)
        {
            var iTerminator = value.IndexOf('\0');
            return iTerminator < 0
                ? value
                : value.Substring(0, iTerminator);
        }

        private static string HandleEmpty(this string value)
        {
            return string.Equals(value, string.Empty)
                ? " "
                : value;
        }

        private static char AsSendable(this char value)
        {
            return value == '\0'
                ? ' '
                : value;
        }

        private static byte[] AsSendable(this byte[] value, AseDbType aseDbType)
        {
            return value.Length == 0 && aseDbType != AseDbType.Image
                ? new byte[] { 0 }
                : value;
        }
    }
}
