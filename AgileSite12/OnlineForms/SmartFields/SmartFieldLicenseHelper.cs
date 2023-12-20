using CMS.DataEngine;
using CMS.LicenseProvider;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Licensing helper class for Smart field feature.
    /// </summary>
    public static class SmartFieldLicenseHelper
    {
        /// <summary>
        /// Performs license check.
        /// </summary>
        /// <returns>True when license is available, false otherwise.</returns>
        public static bool HasLicense()
        {
            return LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.BizForms) && LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.FullContactManagement);
        }
    }
}
