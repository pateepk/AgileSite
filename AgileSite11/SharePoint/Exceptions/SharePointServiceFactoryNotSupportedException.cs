using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Indicates the desired SharePoint service factory does not have an implementation.
    /// </summary>
    public class SharePointServiceFactoryNotSupportedException : Exception
    {
        /// <summary>
        /// Indicates the desired SharePoint service factory does not have an implementation.
        /// </summary>
        public SharePointServiceFactoryNotSupportedException()
        {
        }


        /// <summary>
        /// Indicates the desired SharePoint service factory does not have an implementation.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointServiceFactoryNotSupportedException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Indicates the desired SharePoint service factory does not have an implementation.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointServiceFactoryNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
