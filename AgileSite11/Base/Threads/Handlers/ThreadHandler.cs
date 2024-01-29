namespace CMS.Base
{
    /// <summary>
    /// Thread handler
    /// </summary>
    public class ThreadHandler : AdvancedHandler<ThreadHandler, ThreadEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="thread">Handled thread</param>
        public ThreadHandler StartEvent(CMSThread thread)
        {
            var e = new ThreadEventArgs()
            {
                Thread = thread
            };

            var h = StartEvent(e);

            return h;
        }
    }
}