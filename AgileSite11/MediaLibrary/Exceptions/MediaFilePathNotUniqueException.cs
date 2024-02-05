using System;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Media file path not unique exception.
    /// </summary>
    public class MediaFilePathNotUniqueException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message</param>
        public MediaFilePathNotUniqueException(string message = null)
            :base (message)
        {
        }
    }
}
