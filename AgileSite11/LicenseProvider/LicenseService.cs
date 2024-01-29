using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.LicenseProvider
{
    internal class LicenseService : ILicenseService
    {
        /// <summary>
        /// Checks the license based on feature and perform actions based on given arguments
        /// </summary>
        /// <param name="feature">Feature to check</param>
        /// <param name="domain">Domain to check. If null, function tries to get domain from HttpContext</param>
        /// <param name="throwError">Indicates whether throw error after false check</param>
        public bool CheckLicense(FeatureEnum feature, string domain = null, bool throwError = true)
        {
            domain = domain ?? RequestContext.CurrentDomain;
            
            // No check for unknown domain
            if (!String.IsNullOrEmpty(domain))
            {
                // Check feature
                bool check = LicenseKeyInfoProvider.IsFeatureAvailable(domain, feature);
                if (!check)
                {
                    LicenseHelper.ReportFailedLicenseCheck(feature, domain, throwError);

                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Checks whether the given <paramref name="feature"/> is available on given <paramref name="domain"/>.
        /// </summary>
        /// <remarks>
        /// If the method is called outside the request and <paramref name="domain"/> is <c>null</c>, check is performed against the best license available on the instance.
        /// </remarks>
        /// <param name="feature">Feature to be checked</param>
        /// <param name="domain">Domain the <paramref name="feature"/> is checked against. If <c>null</c>, the domain will be obtained from the context</param>
        /// <returns><c>true</c> if the <paramref name="feature"/> is available on given <paramref name="domain"/>; otherwise, <c>false</c></returns>
        public bool IsFeatureAvailable(FeatureEnum feature, string domain = null)
        {
            domain = domain ?? RequestContext.CurrentDomain;

            return !string.IsNullOrEmpty(domain) ? 
                LicenseKeyInfoProvider.IsFeatureAvailable(domain, feature) : 
                LicenseHelper.IsFeatureAvailableInBestLicense(feature);
        }
    }
}