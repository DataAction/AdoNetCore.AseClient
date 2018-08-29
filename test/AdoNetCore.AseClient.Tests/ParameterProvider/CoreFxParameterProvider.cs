using System;
using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.ParameterProvider
{
    public class CoreFxParameterProvider : IParameterProvider
    {
        public DbParameter GetParameter()
        {
            return new AdoNetCore.AseClient.AseParameter();
        }
        public DbParameter GetParameter(string parameterName, string aseDbType)
        {
            return new AseParameter(parameterName, ParseType(aseDbType));
        }

        public DbParameter GetParameter(string parameterName, object value)
        {
            return new AseParameter(parameterName, value);
        }

        private AseDbType ParseType(string aseDbType)
        {
            if (System.Enum.TryParse(aseDbType, out AseDbType type))
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
