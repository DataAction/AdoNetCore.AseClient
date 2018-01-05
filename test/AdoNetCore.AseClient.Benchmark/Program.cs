using System;
using AdoNetCore.AseClient.Tests.Benchmark;
using BenchmarkDotNet.Running;

namespace AdoNetCore.AseClient.Benchmark
{
    /// <summary>
    /// Uses BenchmarkDotNet to benchmark a number of usages of the IDbProvider that can be compared with the SAP AseClient.
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var version = args[0];
                if (string.Equals(version, "corefx", StringComparison.OrdinalIgnoreCase))
                {
                    var summary = BenchmarkRunner.Run<CoreFxBenchmarks>();

                    Console.WriteLine(summary);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                else if (string.Equals(version, "sap", StringComparison.OrdinalIgnoreCase))
                {
                    var summary = BenchmarkRunner.Run<SapBenchmarks>();

                    Console.WriteLine(summary);
                    Console.ReadLine();
                    Environment.Exit(0);
                }

            }

            Console.WriteLine("Usage: bm [corefx|sap]");
            Environment.Exit(1);
        }
    }

    public class CoreFxBenchmarks : AseClientBenchmarks<CoreFxConnectionProvider>
    {
        public CoreFxBenchmarks() : base(Activator.CreateInstance<Benchmarks<CoreFxConnectionProvider>>())
        {
        }
    }
    public class SapBenchmarks : AseClientBenchmarks<SapConnectionProvider>
    {
        public SapBenchmarks() : base(Activator.CreateInstance<Benchmarks<SapConnectionProvider>>())
        {
        }
    }
}
