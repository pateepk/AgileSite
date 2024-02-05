namespace CMS.Base
{
    /// <summary>
    /// Events raised within thread processing
    /// </summary>
    public class ThreadEvents
    {
        /// <summary>
        /// Fires when the thread gets initialized. Runs from the original thread context.
        /// </summary>
        public static ThreadHandler Init = new ThreadHandler
            {
                Name = "ThreadEvents.Init",
                Debug = false
            };


        /// <summary>
        /// Fires when the thread runs. Runs from the new thread context.
        /// </summary>
        public static ThreadHandler Run = new ThreadHandler
            {
                Name = "ThreadEvents.Run",
                Debug = false
            };


        /// <summary>
        /// Fires when the finalizes its execution. Runs from the new thread context.
        /// </summary>
        public static SimpleThreadHandler Finalize = new SimpleThreadHandler
            {
                Name = "ThreadEvents.Finalize",
                Debug = false
            };
    }
}
