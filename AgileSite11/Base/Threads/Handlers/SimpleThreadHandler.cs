namespace CMS.Base
{
    /// <summary>
    /// Simple thread handler
    /// </summary>
    public class SimpleThreadHandler : SimpleHandler<SimpleThreadHandler, ThreadEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="thread">Handled thread</param>
        public ThreadEventArgs StartEvent(CMSThread thread)
        {
            var e = new ThreadEventArgs
            {
                Thread = thread
            };

            var h = StartEvent(e);

            return h;
        }
    }
}