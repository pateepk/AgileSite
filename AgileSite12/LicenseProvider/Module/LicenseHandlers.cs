using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.Core;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// License module event handlers
    /// </summary>
    internal class LicenseHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            RequestEvents.Begin.Execute += CheckApplicationValidityAndLicense;

            WebFarmHelper.RegisterTask<ClearLicenseLimitationCacheWebFarmTask>(true);
        }


        /// <summary>
        /// Checks the application validity and license for external application
        /// </summary>
        private static void CheckApplicationValidityAndLicense(object sender, EventArgs eventArgs)
        {
            // Check the application validity
            LicenseHelper.CheckApplicationValidity();

            CheckLicenseForCurrentDomainInExternalApplication();
        }


        /// <summary>
        /// Checks whether a license for the current domain exists and is valid.
        /// When the license is not valid, respond with HTTP503 status code.
        /// </summary>
        /// <remarks>
        /// Prevents an external application (e.g. MVC) with registered CMSApplication module from running without license.
        /// </remarks>
        private static void CheckLicenseForCurrentDomainInExternalApplication()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                return;
            }

            var domain = RequestContext.CurrentDomain;

            // Check license for the current domain
            var licenseValidationResult = LicenseHelper.ValidateLicenseForDomain(domain);
            if (licenseValidationResult != LicenseValidationEnum.Valid)
            {
                var message = $"You don't have a valid license for the domain '{domain}'. License status: {LicenseHelper.GetValidationResultString(licenseValidationResult)}";
                CoreServices.EventLog.LogEvent("W", "Licensing", "INVALIDLICENSE", message);

                throw new HttpException(503, "You don't have a valid license for current domain. See event log for more details.");
            }
        }
    }
}
