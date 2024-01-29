using CMS.Base;
using CMS.DataEngine;
using CMS.LicenseProvider;

namespace CMS.DataProtection
{
    /// <summary>
    /// Provides helper methods for license checking.
    /// </summary>
    internal class InternalLicenseHelper
    {
        /// <summary>
        /// Checks if current license allows <see cref="FeatureEnum.DataProtection"/> feature and if it doesn't the method throws <see cref="LicenseException"/>.
        /// </summary>
        /// <exception cref="LicenseException">Thrown if insufficient license found.</exception>
        public static void ThrowIfInsufficientLicense()
        {
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(FeatureEnum.DataProtection))
            {
                // Report error in Event log and throw exception without redirecting to error page
                using (var actionContext = new CMSActionContext { AllowLicenseRedirect = false })
                {
                    LicenseHelper.ReportFailedLicenseCheck(FeatureEnum.DataProtection, domain: null, throwError: true);
                }
            }
        }
    }
}
