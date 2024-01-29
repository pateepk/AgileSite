using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object event arguments for log object change event
    /// </summary>
    public class LogObjectChangeEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Log object change settings
        /// </summary>
        public LogObjectChangeSettings Settings
        {
            get;
            set;
        }
    }
}