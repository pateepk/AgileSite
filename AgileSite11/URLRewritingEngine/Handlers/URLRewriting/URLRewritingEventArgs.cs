using CMS.Base;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// URL Rewriting event arguments
    /// </summary>
    public class URLRewritingEventArgs : CMSEventArgs
    {
        /// <summary>
        /// URL Rewriting parameters
        /// </summary>
        public URLRewritingParams Parameters
        {
            get;
            set;
        }
    }
}