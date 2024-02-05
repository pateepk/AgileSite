using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document handler
    /// </summary>
    public class LogDocumentChangeHandler : AdvancedHandler<LogDocumentChangeHandler, LogDocumentChangeEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Log document change settings</param>
        public LogDocumentChangeHandler StartEvent(LogDocumentChangeSettings settings)
        {
            LogDocumentChangeEventArgs e = new LogDocumentChangeEventArgs()
            {
                Settings = settings
            };
            
            return StartEvent(e);
        }
    }
}