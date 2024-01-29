using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// GetMediaData event arguments.
    /// </summary>
    public class GetMediaDataEventArgs : CMSEventArgs
    {
        /// <summary>
        /// URL of media object
        /// </summary>
        public string Url
        {
            get;
            set;
        }


        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Media source
        /// </summary>
        public MediaSource MediaSource
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if event was handled.
        /// </summary>
        public bool EventHandled
        {
            get;
            set;
        }
    }
}
