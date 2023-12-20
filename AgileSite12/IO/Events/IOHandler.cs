using CMS.Base;

namespace CMS.IO
{
    /// <summary>
    /// Event handler for IO operations
    /// </summary>
    public class IOHandler : AdvancedHandler<IOHandler, IOEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        public IOHandler StartEvent(string path)
        {
            var e = new IOEventArgs
            {
                Path = path
            };

            return StartEvent(e);
        }
    }
}