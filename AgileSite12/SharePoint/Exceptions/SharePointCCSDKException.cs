using System;


namespace CMS.SharePoint
{
    /// <summary>
    /// Thrown when error regarding SharePoint Client Components SDK occurs.
    /// </summary>
    /// <remarks>
    /// Typically thrown when trying to establish connection to SharePoint Online server using SharePointOnlineCredentials
    /// and the SDK is not installed. Thus the "msoidcliL.dll" can not be loaded.
    /// </remarks>
    public class SharePointCCSDKException : Exception
    {
        /// <summary>
        /// Thrown when error regarding SharePoint Client Components SDK occurs.
        /// </summary>
        public SharePointCCSDKException()
        {
        }


        /// <summary>
        /// Thrown when error regarding SharePoint Client Components SDK occurs.
        /// </summary>
        /// <param name="message">Exception message</param>
        public SharePointCCSDKException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Thrown when error regarding SharePoint Client Components SDK occurs.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception that caused the current exception</param>
        public SharePointCCSDKException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
