using System;

namespace CMS.Base
{
    /// <summary>
    /// System event arguments
    /// </summary>
    public class SystemEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Raised exception
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the exception is allowed to be logged
        /// </summary>
        public bool LogException
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public SystemEventArgs()
        {
            LogException = true;
        }
    }
}