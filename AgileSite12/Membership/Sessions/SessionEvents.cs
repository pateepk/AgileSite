using CMS.Base;

namespace CMS.Membership
{
    /// <summary>
    /// Global CMS session events
    /// </summary>
    public class SessionEvents
    {
        /// <summary>
        /// Fires when the session is updated
        /// </summary>
        public static AdvancedHandler UpdateSession = new AdvancedHandler { Name = "SessionEvents.UpdateSession" };

        /// <summary>
        /// Fires when the session data is updated
        /// </summary>
        public static SessionHandler UpdateSessionData = new SessionHandler { Name = "SessionEvents.UpdateSessionData" };
    }
}