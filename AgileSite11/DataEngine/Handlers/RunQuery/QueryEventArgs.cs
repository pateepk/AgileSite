namespace CMS.Base
{
    /// <summary>
    /// Query event arguments
    /// </summary>
    public class QueryEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Query
        /// </summary>
        public string Query
        {
            get;
            set;
        }
    }
}