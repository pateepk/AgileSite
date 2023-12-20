using System;

using CMS.Helpers;
using CMS.Base;


namespace CMS.MembershipProvider
{
    /// <summary>
    /// Membership helper.
    /// </summary>
    /// <exclude />
    internal static class CMSMembershipHelper
    {
        private static string mADDefaultMapUserName;

        /// <summary>
        /// Returns the field name used for user name mapping.
        /// </summary>
        /// <remarks>
        /// If not set the "sAMAccountName" is used.
        /// </remarks>
        internal static string ADDefaultMapUserNameInternal
        {
            get
            {
                return mADDefaultMapUserName ?? (mADDefaultMapUserName = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSADDefaultMapUserName"], "sAMAccountName"));
            }
        }
    }
}