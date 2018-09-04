using System.Data.Common;

namespace AdoNetCore.AseClient.Tests.ParameterProvider
{
    public interface IParameterProvider
    {
        DbParameter GetParameter();
        DbParameter GetParameter(string parameterName, string aseDbType);
        DbParameter GetParameter(string parameterName, object value);
    }
}
