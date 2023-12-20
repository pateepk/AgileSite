using System;

using CMS.Base;
using CMS.Membership;


namespace Kentico.Membership.Internal
{
    /// <summary>
    /// Encapsulates helper methods for user registration.
    /// </summary>
    public class RegistrationHelper : AbstractHelper<RegistrationHelper>
    {
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="user">User to be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="user"/> already exists. Method is to be used only for creating a new user.</exception>
        /// <remarks>Rises <see cref="MembershipEvents.RegistrationCompleted"/> event when registration proccess has been completed and user is <see cref="UserInfo.Enabled"/>.</remarks>
        public static void RegisterUser(UserInfo user)
        {
            HelperObject.RegisterUserInternal(user);
        }


        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="user">User to be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="user"/> already exists. Method is to be used only for creating a new user.</exception>
        /// <remarks>Rises <see cref="MembershipEvents.RegistrationCompleted"/> event when registration proccess has been completed and user is <see cref="UserInfo.Enabled"/>.</remarks>
        protected virtual void RegisterUserInternal(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.UserID != 0)
            {
                throw new ArgumentException($"User with ID '{user.UserID}' already exists.");
            }

            UserInfoProvider.SetUserInfo(user);

            if (MembershipEvents.RegistrationCompleted.IsBound && user.Enabled)
            {
                MembershipEvents.RegistrationCompleted.StartEvent(user);
            }
        }
    }
}
