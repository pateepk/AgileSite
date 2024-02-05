using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Request license key values
    /// </summary>
    [RegisterAllProperties]
    public class LicenseContext : AbstractContext<LicenseContext>
    {
        #region "Variables"

        private LicenseKeyInfo mCurrentLicenseKeyInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns license key info for current domain.
        /// </summary>
        public static LicenseKeyInfo CurrentLicenseInfo
        {
            get
            {
                return Current.mCurrentLicenseKeyInfo ?? (Current.mCurrentLicenseKeyInfo = LicenseKeyInfoProvider.GetLicenseKeyInfo(RequestContext.CurrentDomain));
            }
            set
            {
                Current.mCurrentLicenseKeyInfo = value;
            }
        }

        #endregion
    }
}