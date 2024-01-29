using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Log object change handler
    /// </summary>
    public class LogObjectChangeHandler : AdvancedHandler<LogObjectChangeHandler, LogObjectChangeEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LogObjectChangeHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public LogObjectChangeHandler(LogObjectChangeHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="settings">Log object change settings</param>
        public LogObjectChangeHandler StartEvent(LogObjectChangeSettings settings)
        {
            LogObjectChangeEventArgs e = new LogObjectChangeEventArgs()
                                    {
                                        Settings = settings
                                    };

            return StartEvent(e);
        }
    }
}