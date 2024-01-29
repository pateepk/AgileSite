using System.Web;

namespace CMS.Base
{
    /// <summary>
    /// Event arguments for GetVaryByCustomString event
    /// </summary>
    public class GetVaryByCustomStringEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Result value
        /// </summary>
        public string Result
        {
            get;
            set;
        }


        /// <summary>
        /// Request context
        /// </summary>
        public HttpContext Context
        {
            get;
            set;
        }


        /// <summary>
        /// Custom string
        /// </summary>
        public string Custom
        {
            get;
            set;
        }
    }
}