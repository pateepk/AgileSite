using System;
using System.Linq;
using System.Text;

namespace CMS.Membership
{
    /// <summary>
    /// Simple object to persist user informations in cookies
    /// </summary>
    [Serializable]
    internal sealed class ImpersonationCookieData
    {
        /// <summary>
        /// Gets or sets original user GUID
        /// </summary>
        internal Guid OriginalUserId
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets impersonated user GUID
        /// </summary>
        internal Guid ImpersonatedUserId
        {
            get;
            private set;
        }


        /// <summary>
        /// Checks if Impersonation cookie data are valid,
        /// i.e. <see cref="OriginalUserId"/> and <see cref="ImpersonatedUserId"/> are not empty GUIDs.
        /// </summary>
        /// <returns>True, if both <see cref="OriginalUserId"/> and <see cref="ImpersonatedUserId"/> are set to any non-zero value.</returns>
        internal bool IsValid()
        {
            return ((OriginalUserId != Guid.Empty) && (ImpersonatedUserId != Guid.Empty));
        }


        /// <summary>
        /// Creates object with given user GUIDs
        /// </summary>
        internal ImpersonationCookieData(Guid originalUserId, Guid impersonatedUserId)
        {
            OriginalUserId = originalUserId;
            ImpersonatedUserId = impersonatedUserId;
        }
    }
}
