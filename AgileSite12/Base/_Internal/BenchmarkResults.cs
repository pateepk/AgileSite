using System;

namespace CMS.Base.Internal
{
    /// <summary>
    /// Represents the results of the benchmark
    /// </summary>
    public class BenchmarkResults
    {
        #region "Properties"

        /// <summary>
        /// Total seconds for running all iterations
        /// </summary>
        public double TotalRunSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum number of seconds per single run
        /// </summary>
        public double MaxRunSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Minimum seconds per single run
        /// </summary>
        public double MinRunSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Average seconds per single run
        /// </summary>
        public double SecondsPerRun
        {
            get;
            set;
        }


        /// <summary>
        /// Total number of seconds
        /// </summary>
        public double TotalSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Number of benchmark runs
        /// </summary>
        public int Runs
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the results as string representation
        /// </summary>
        public override string ToString()
        {
            var results = String.Format(
                @"Total runs: {0}
Total benchmark time: {1:f5}s
Total run time: {5:f5}s

Average time per run: {2:f5}s
Min run time: {3:f5}s
Max run time: {4:f5}s
"
                , Runs
                , TotalSeconds
                , SecondsPerRun
                , MinRunSeconds
                , MaxRunSeconds
                , TotalRunSeconds
                );

            return results;
        }

        #endregion
    }
}