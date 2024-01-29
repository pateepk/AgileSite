using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Simple thread handler
    /// </summary>
    public class SessionHandler : SimpleHandler<SessionHandler, SessionEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="session">Session info</param>
        public SessionEventArgs StartEvent(SessionInfo session)
        {
            var e = new SessionEventArgs
            {
                Session = session
            };

            var h = StartEvent(e);

            return h;
        }
    }
}