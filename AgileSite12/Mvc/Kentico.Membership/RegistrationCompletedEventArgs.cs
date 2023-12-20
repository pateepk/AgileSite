using CMS.Base;
using CMS.Membership;

namespace Kentico.Membership.Internal
{
    /// <summary>
    /// Event arguments used for <see cref="MembershipEvents.RegistrationCompleted"/> event.
    /// </summary>
    public class RegistrationCompletedEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Returns an <see cref="UserInfo" /> of the user who completed the registration process.
        /// </summary>
        public UserInfo User
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="RegistrationCompletedEventArgs"/>.
        /// </summary>
        public RegistrationCompletedEventArgs()
        {

        }


        /// <summary>
        /// Initializes a new instance of <see cref="RegistrationCompletedEventArgs"/> with given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">Registered user.</param>
        public RegistrationCompletedEventArgs(UserInfo user)
        {
            User = user;
        }
    }
}
