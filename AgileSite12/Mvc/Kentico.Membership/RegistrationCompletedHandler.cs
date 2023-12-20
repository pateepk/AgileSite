using CMS.Base;
using CMS.Membership;

namespace Kentico.Membership.Internal
{
    /// <summary>
    /// Represents a handler that handles the user registration event.
    /// </summary>
    public class RegistrationCompletedHandler : SimpleHandler<RegistrationCompletedHandler, RegistrationCompletedEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="userInfo"><see cref="UserInfo" /> representing registered user.</param>
        public RegistrationCompletedEventArgs StartEvent(UserInfo userInfo)
        {
            var eventArgs = new RegistrationCompletedEventArgs(userInfo);

            StartEvent(eventArgs);

            return eventArgs;
        }
    }
}
