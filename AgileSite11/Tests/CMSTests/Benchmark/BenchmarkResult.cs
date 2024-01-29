using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMS.Tests
{
    /// <summary>
    /// Benchmark result class
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// Gets the underlaying results.
        /// </summary>
        public readonly TimeSpan[] Results;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="results">Results of the benchmark action runs</param>
        public BenchmarkResult(IEnumerable<TimeSpan> results)
        {
            Results = results.ToArray();
        }


        /// <summary>
        /// Gets the average result time in milliseconds.
        /// </summary>
        public double Average
        {
            get
            {
                return Results.Average(r => r.TotalMilliseconds);
            }
        }


        /// <summary>
        /// Gets the shortest result time in milliseconds.
        /// </summary>
        public double Min
        {
            get
            {
                return Results.Min(r => r.TotalMilliseconds);
            }
        }


        /// <summary>
        /// Gets the longest result time in milliseconds.
        /// </summary>
        public double Max
        {
            get
            {
                return Results.Max(r => r.TotalMilliseconds);
            }
        }


        /// <summary>
        ///Writes the formatted information about the results using the specified writer.
        /// Uses the System.Console.Out writer if no writer is specified.
        /// </summary>
        /// <param name="writer">Writer</param>
        public void Write(TextWriter writer = null)
        {
            if (writer == null)
            {
                writer = Console.Out;
            }

            var count = Results.Count();

            writer.WriteLine("Average ({0}): {1:F} ms", count, Average);
            writer.WriteLine("Min: {0:F} ms", Min);
            writer.WriteLine("Max: {0:F} ms", Max);
            writer.WriteLine();
            writer.WriteLine("Results ({0}):", count);
            foreach (var result in Results)
            {
                writer.WriteLine("{0:F} ms", result.TotalMilliseconds);
            }
        }
    }
}
