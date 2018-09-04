#if NET_FRAMEWORK
using System;
using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.ParameterProvider
{
    public class SapParameterProvider : IParameterProvider
    {
        public DbParameter GetParameter()
        {

            return new Sybase.Data.AseClient.AseParameter();
        }

        public DbParameter GetParameter(string parameterName, string aseDbType)
        {
            return new Sybase.Data.AseClient.AseParameter(parameterName, ParseType(aseDbType));
        }

        public DbParameter GetParameter(string parameterName, object value)
        {
            return new Sybase.Data.AseClient.AseParameter(parameterName, value);
        }

        private Sybase.Data.AseClient.AseDbType ParseType(string aseDbType)
        {
            if (System.Enum.TryParse(aseDbType, out Sybase.Data.AseClient.AseDbType type))
            {
                return type;
            }
            else
            {
                throw new ArgumentException(nameof(aseDbType));
            }
        }
    }
}
#endif
