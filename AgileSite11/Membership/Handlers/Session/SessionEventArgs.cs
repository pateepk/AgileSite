using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Session event arguments
    /// </summary>
    public class SessionEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Session
        /// </summary>
        public SessionInfo Session
        {
            get;
            set;
        }
    }
}