using System;
using System.Diagnostics;

namespace CMS.Tests
{
    /// <summary>
    /// Benchmark class
    /// </summary>
    public class Benchmark
    {
        /// <summary>
        /// Gets the benchmark action.
        /// </summary>
        public readonly Action<Stopwatch> BenchmarkAction;


        private Benchmark(Action<Stopwatch> benchmarkAction)
        {
            BenchmarkAction = benchmarkAction;
        }


        /// <summary>
        /// Creates a new benchmark with the specified action to be benchmarked.
        /// </summary>
        /// <param name="action">Action to be benchmarked</param>
        public static Benchmark This(Action action)
        {
            // Wrap action in benchmark action
            var benchmarkAction = new Action<Stopwatch>(w =>
            {
                w.Start();
                action();
                w.Stop();
            });

            return new Benchmark(benchmarkAction);
        }


        /// <summary>
        /// Creates a new benchmark with the specified benchmark action.
        /// </summary>
        /// <param name="benchmarkAction">Benchmark action</param>
        public static Benchmark This(Action<Stopwatch> benchmarkAction)
        {
            return new Benchmark(benchmarkAction);
        }


        /// <summary>
        /// Executes the benchmarked action a specified number of times and returns the result.
        /// Runs the action once prior to benchmarking.
        /// </summary>
        /// <param name="times">Number of times the benchmarked action to be run</param>
        public BenchmarkResult RunWithWarmup(int times)
        {
            // Warm up
            BenchmarkAction(new Stopwatch());

            // Benchmark
            return Run(times);
        }


        /// <summary>
        /// Executes the benchmarked action a specified number of times and returns the result.
        /// </summary>
        /// <param name="times">Number of times the benchmarked action to be run</param>
        public BenchmarkResult Run(int times)
        {
            var results = new TimeSpan[times];
            var watch = new Stopwatch();
            for (int i = 0; i < times; i++)
            {
                // Reset the watch
                watch.Reset();

                // Compact the heap
                GC.Collect();

                // Wait for the finalize queue to empty
                GC.WaitForPendingFinalizers();

                // Benchmark the action
                BenchmarkAction(watch);
                results[i] = watch.Elapsed;
            }

            return new BenchmarkResult(results);
        }
    }
}
