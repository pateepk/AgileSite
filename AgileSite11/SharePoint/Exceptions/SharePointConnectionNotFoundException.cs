using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Thrown when SharePoint connection can not be found.
    /// </summary>
    /// <seealso cref="SharePointConnectionInfo"/>
    public class SharePointConnectionNotFoundException : Exception
    {
        /// <summary>
        /// Thrown when SharePoint connection can not be found.
        /// </summary>
        public SharePointConnectionNotFoundException()
        {
        }


        /// <summary>
        /// Thrown when SharePoint connection can not be found.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointConnectionNotFoundException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Thrown when SharePoint connection can not be found.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointConnectionNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
