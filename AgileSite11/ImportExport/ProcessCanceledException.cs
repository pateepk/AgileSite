using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Exception for the cancelation of the process.
    /// </summary>
    public class ProcessCanceledException : Exception
    {
        /// <summary>
        /// Constructor - Creates process canceled exception.
        /// </summary>
        public ProcessCanceledException()
        {
        }


        /// <summary>
        /// Constructor - Creates process canceled exception.
        /// </summary>
        /// <param name="message">Exception message</param>
        public ProcessCanceledException(string message)
            : base(message)
        {
        }
    }
}