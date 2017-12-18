using System;
using BenchmarkDotNet.Running;

namespace AdoNetCore.AseClient.Benchmark.SAP
{
    /// <summary>
    /// Uses BenchmarkDotNet to benchmark a number of usages of the IDbProvider that can be compared with the SAP AseClient.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            var summary = BenchmarkRunner.Run<AseClientBenchmarks>();

            Console.WriteLine(summary);
            Console.ReadLine();
        }
    }
}
