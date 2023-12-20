namespace CMS.Base
{
    /// <summary>
    /// Get URL event arguments
    /// </summary>
    public class URLEventArgs : CMSEventArgs
    {
        /// <summary>
        /// URL
        /// </summary>
        public string URL
        {
            get;
            set;
        }
    }
}