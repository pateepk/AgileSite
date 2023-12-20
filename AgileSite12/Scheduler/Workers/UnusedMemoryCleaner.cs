using System;

namespace CMS.Scheduler
{
    /// <summary>
    /// Class for cleaning unused memory.
    /// </summary>
    public class UnusedMemoryCleaner : ITask
    {
        /// <summary>
        /// Cleans unused memory.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Collect the memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                return (e.Message);
            }
            return string.Empty;
        }
    }
}