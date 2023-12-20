using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Base
{
    /// <summary>
    /// Wrapper for a Parallel class, providing additional operations to make threads compatible with CMS API (such as CMSThread class)
    /// </summary>
    public class CMSParallel
    {
        /// <summary>
        /// Executes a for each operation on an System.Collections.IEnumerable{TSource} in which iterations may run in parallel. Wraps the operation into CMSThread to ensure proper context.
        /// </summary>
        /// <param name="source">Parallel sources</param>
        /// <param name="body">Executing body</param>
        /// <param name="options">Options for executing</param>
        /// <param name="count">Number of the items that will be executed. If set, optimizes the performance in case there is none or one items in the source.</param>
        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body, ParallelOptions options = null, int count = int.MaxValue)
        {
            // For zero items, do not even execute
            if (count == 0)
            {
                return new ParallelLoopResult();
            }

            // For single item, execute within normal synchronous action to improve performance
            if (count == 1)
            {
                body(source.First());

                return new ParallelLoopResult();
            }

            // Wrap the body to ensure context
            body = CMSThread.Wrap(body, true);

            if (options == null)
            { 
                options = new ParallelOptions();
            }

            // Execute
            return Parallel.ForEach(source, options, body);
        } 


        /// <summary>
        /// Executes a for each operation on an System.Collections.IEnumerable{TSource} in which iterations may run in parallel. Wraps the operation into CMSThread to ensure proper context.
        /// </summary>
        /// <param name="source">Parallel sources</param>
        /// <param name="body">Executing body</param>
        /// <param name="options">Options for executing</param>
        public static ParallelLoopResult ForEach<TSource>(TSource[] source, Action<TSource> body, ParallelOptions options)
        {
            return ForEach(source, body, options, source.Length);
        }


        /// <summary>
        /// Executes a for each operation on an System.Collections.IEnumerable{TSource} in which iterations may run in parallel. Wraps the operation into CMSThread to ensure proper context.
        /// </summary>
        /// <param name="source">Parallel sources</param>
        /// <param name="body">Executing body</param>
        /// <param name="options">Options for executing</param>
        public static ParallelLoopResult ForEach<TSource>(ICollection<TSource> source, Action<TSource> body, ParallelOptions options)
        {
            return ForEach(source, body, options, source.Count);
        }

        
        /// <summary>
        /// Executes a for each operation on an System.Collections.IEnumerable{TSource} in which iterations may run in parallel. Wraps the operation into CMSThread to ensure proper context.
        /// </summary>
        /// <param name="source">Parallel sources</param>
        /// <param name="body">Executing body</param>
        /// <param name="options">Options for executing</param>
        public static ParallelLoopResult ForEach<TSource>(IList<TSource> source, Action<TSource> body, ParallelOptions options)
        {
            return ForEach(source, body, options, source.Count);
        }
    }
}
