using CMS.DataEngine;

using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(INewsletterLicenseCheckerService), typeof(NewsletterLicenseCheckerService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods to check newsletter licence. 
    /// </summary>
    internal interface INewsletterLicenseCheckerService
    {
        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null);


        /// <summary>
        /// Checks the license for insert for a new newsletter or for edit in other cases.
        /// </summary>
        /// <param name="newsletter">Newsletter</param>
        void CheckLicense(NewsletterInfo newsletter);


        /// <summary>
        /// Clear license newsletter hashtable.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        void ClearLicNewsletter(bool logTasks);


        /// <summary>
        /// Checks if newsletter A/B testing is available for current URL.
        /// </summary>
        bool IsABTestingAvailable();


        /// <summary>
        /// Checks if newsletter tracking (open e-mail, click through and bounces) is available for current URL.
        /// </summary>
        bool IsTrackingAvailable();


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature to check</param>
        /// <param name="action">Action, if action is Insert limitations are not checked under administration interface</param>
        bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action);


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature to check</param>
        /// <param name="action">Action</param>
        /// <param name="siteCheck">If true limitations are not applied under URLs in Site manager, CMS Desk, CMSModules and CMSPages/Logon</param>
        bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action, bool siteCheck);
    }
}