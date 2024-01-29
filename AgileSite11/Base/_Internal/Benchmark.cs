using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Internal
{
    /// <summary>
    /// Provides support for benchmarking the code
    /// </summary>
    public class Benchmark<TResult> : Benchmark
    {
        #region "Properties"

        /// <summary>
        /// Function to execute within each iteration
        /// </summary>
        public Func<Benchmark<TResult>, TResult> ExecuteFunc
        {
            get;
            set;
        }


        /// <summary>
        /// Results of the benchmarked function
        /// </summary>
        public TResult Result
        {
            get;
            set;
        }

        
        /// <summary>
        /// Performs the setup of the benchmark
        /// </summary>
        public Action<Benchmark<TResult>> SetupFunc
        {
            get;
            set;
        }


        /// <summary>
        /// Performs the tear down of the benchmark
        /// </summary>
        public Action<Benchmark<TResult>> TearDownFunc
        {
            get;
            set;
        }


        /// <summary>
        /// Benchmark name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="executeFunc">Function to execute within each iteration</param>
        public Benchmark(Func<Benchmark<TResult>, TResult> executeFunc)
        {
            ExecuteFunc = executeFunc;
        }


        /// <summary>
        /// Runs the benchmark
        /// </summary>
        public override void Run()
        {
            Result = default(TResult);

            // Setup
            if (SetupFunc != null)
            {
                SetupFunc(this);
            }

            int runs = 0;

            var startTime = DateTime.Now;

            double minRunSeconds = double.MaxValue;
            double maxRunSeconds = 0;

            double totalRunSeconds = 0;

            // Run the benchmark
            for (int i = 0; i < Iterations; i++)
            {
                var runStart = DateTime.Now;

                // Execute the run
                Result = ExecuteFunc(this);

                runs++;

                // Count the run time
                var runEnd = DateTime.Now;
                var runTime = runEnd - runStart;
                var runSeconds = runTime.TotalSeconds;

                if (runSeconds < minRunSeconds)
                {
                    minRunSeconds = runSeconds;
                }
                if (runSeconds > maxRunSeconds)
                {
                    maxRunSeconds = runSeconds;
                }

                totalRunSeconds += runSeconds;
            }

            var endTime = DateTime.Now;

            // Calculate the results
            if (Math.Abs(minRunSeconds - double.MaxValue) < Math.E)
            {
                minRunSeconds = 0;
            }

            var totalTime = endTime - startTime;

            var totalSeconds = totalTime.TotalSeconds;
            var secondsPerRun = totalSeconds / runs;

            // Set up the results
            Results = new BenchmarkResults
            {
                Runs = runs,
                TotalSeconds = totalSeconds,
                SecondsPerRun = secondsPerRun,
                MinRunSeconds = minRunSeconds,
                MaxRunSeconds = maxRunSeconds,
                TotalRunSeconds = totalRunSeconds
            };

            // Tear down
            if (TearDownFunc != null)
            {
                TearDownFunc(this);
            }
        }


        /// <summary>
        /// Gets the results report
        /// </summary>
        /// <param name="baseLine">Base line benchmark</param>
        public override string GetReport(Benchmark baseLine = null)
        {
            if (Iterations <= 1)
            {
                return String.Format("{0}: {1:f5} {2}", (Name ?? "Run time"), Results.TotalRunSeconds, GetBaseLineComparison(baseLine));
            }

            string report = null;

            if (Name != null)
            {
                report += "Benchmark: " + Name;
            }

            report += Results.ToString();

            return report;
        }


        /// <summary>
        /// Gets the comparison of this benchmark to the given base line
        /// </summary>
        /// <param name="baseLine">Base line</param>
        private string GetBaseLineComparison(Benchmark baseLine)
        {
            if ((baseLine == null) || (baseLine == this))
            {
                return null;
            }

            return String.Format("({0:f1} x)", Results.SecondsPerRun / baseLine.Results.SecondsPerRun);
        }

        #endregion
    }


    /// <summary>
    /// Base benchmark class
    /// </summary>
    public abstract class Benchmark
    {
        private int mIterations = 1000;


        /// <summary>
        /// Number of iterations to run. Default 1000
        /// </summary>
        public int Iterations
        {
            get
            {
                return mIterations;
            }
            set
            {
                mIterations = value;
            }
        }


        /// <summary>
        /// Results of the benchmark
        /// </summary>
        public BenchmarkResults Results
        {
            get;
            set;
        }


        /// <summary>
        /// Runs the benchmark
        /// </summary>
        public abstract void Run();


        /// <summary>
        /// Gets the results report
        /// </summary>
        /// <param name="baseLine">Base line benchmark to compare to</param>
        public abstract string GetReport(Benchmark baseLine = null);
    }
}
