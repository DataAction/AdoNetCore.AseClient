using System;

namespace AdoNetCore.AseClient.Internal
{
    internal static class Constants
    {
        internal static class Sql
        {
            internal static readonly DateTime Epoch = new DateTime(1900, 01, 01);
            internal static readonly double TicksPerMillisecond = 0.3;
        }
    }
}
