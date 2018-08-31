using System;

namespace AdoNetCore.AseClient.Internal
{
    internal static class Constants
    {
        internal static class Sql
        {
            internal static readonly DateTime BigEpoch = new DateTime(1, 1, 1); //I think the epoch is actually year 1 BC ("year 0"), but that's not representable in a DateTime, so we need to keep track of the offset in Microseconds below.
            internal static readonly long BigEpochMicroSeconds = 31622400000000; //this equates to 366 days... 1 leap year since 1 BC?
            internal static readonly DateTime Epoch = new DateTime(1900, 01, 01);
            internal static readonly double TicksPerMillisecond = 0.3;
        }
    }
}
