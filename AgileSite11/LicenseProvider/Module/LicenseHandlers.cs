using System;
using System.Linq;
using System.Text;

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

            string domain = RequestContext.CurrentDomain;

            // Check license for the current domain
            LicenseValidationEnum licenseValidationResult = LicenseHelper.ValidateLicenseForDomain(domain);
            if (licenseValidationResult != LicenseValidationEnum.Valid)
            {
                string message = String.Format("You don't have a valid license for the domain '{0}'. License status: {1}",
                    domain,
                    CoreServices.Localization.GetString(LicenseHelper.GetValidationResultString(licenseValidationResult)));
                CoreServices.EventLog.LogEvent("W", "Licensing", "INVALIDLICENSE", message);

                RequestHelper.Respond503();
            }
        }
    }
}
