using CMS.Base;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Checks whether the propagation of <see cref="UserInfo"/> data to Contact object is allowed.
    /// </summary>
    public class UserContactDataPropagationChecker : IContactDataPropagationChecker
    {
        /// <summary>
        /// Checks whether the propagation of <see cref="UserInfo"/> data to Contact object is allowed.
        /// </summary>
        public bool IsAllowed()
        {
            return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUserContactDataMergingAllowed"], true);
        }
    }
}
