using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Internal
{
    /// <summary>
    /// Wrapper to run multiple benchmarks and compare the results
    /// </summary>
    public class MultiBenchmark : Benchmark
    {
        /// <summary>
        /// Base line benchmark to compare to
        /// </summary>
        public Benchmark BaseLine
        {
            get;
            set;
        }


        /// <summary>
        /// Benchmarks covered by this benchmark
        /// </summary>
        public Benchmark[] Benchmarks
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="benchmarks">Benchmarks to run</param>
        public MultiBenchmark(params Benchmark[] benchmarks)
        {
            Iterations = 0;
            Benchmarks = benchmarks;
        }


        /// <summary>
        /// Runs the benchmarks
        /// </summary>
        public override void Run()
        {
            Results = new BenchmarkResults();

            var allRes = Results;

            foreach (var benchmark in Benchmarks)
            {
                if (Iterations > 0)
                {
                    benchmark.Iterations = Iterations;
                }

                benchmark.Run();

                var res = benchmark.Results;

                if (res.MaxRunSeconds > allRes.MaxRunSeconds)
                {
                    allRes.MaxRunSeconds = res.MaxRunSeconds;
                }
                if (res.MinRunSeconds < allRes.MinRunSeconds)
                {
                    allRes.MinRunSeconds = res.MinRunSeconds;
                }

                allRes.Runs += res.Runs;
                allRes.SecondsPerRun += res.SecondsPerRun;
                allRes.TotalRunSeconds += res.TotalRunSeconds;
                allRes.TotalSeconds += res.TotalSeconds;
            }

            var count = Benchmarks.Length;

            allRes.SecondsPerRun /= count;
        }


        /// <summary>
        /// Gets the benchmark report
        /// </summary>
        /// <param name="baseLine">Base line benchmark</param>
        public override string GetReport(Benchmark baseLine = null)
        {
            var sb = new StringBuilder();

            var ordered = Benchmarks.OrderBy(b => b.Results.SecondsPerRun);

            baseLine = baseLine ?? BaseLine ?? ordered.FirstOrDefault();

            foreach (var benchmark in ordered)
            {
                sb.AppendLine(benchmark.GetReport(baseLine));
            }

            return sb.ToString();
        }
    }
}
