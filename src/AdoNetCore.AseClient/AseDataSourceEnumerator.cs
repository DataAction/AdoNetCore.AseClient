using System;
using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Implementation of a data source enumerator.
    /// </summary>
    internal sealed class AseDataSourceEnumerator : DbDataSourceEnumerator
    {
        public override DataTable GetDataSources()
        {
            throw new NotImplementedException();
        }
    }
}
