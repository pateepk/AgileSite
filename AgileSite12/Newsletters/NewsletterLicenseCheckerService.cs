using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Newsletters
{
    using IntDictionary = SafeDictionary<string, int?>;

    /// <summary>
    /// Provides methods to check newsletter licence. 
    /// </summary>
    internal class NewsletterLicenseCheckerService : INewsletterLicenseCheckerService
    {
        /// <summary>
        /// License limitation newsletter table
        /// </summary>
        private static CMSStatic<IntDictionary> mLicNews = new CMSStatic<IntDictionary>(() => new IntDictionary());


        /// <summary>
        /// License limitation newsletter table
        /// </summary>
        private static IntDictionary LicNews
        {
            get
            {
                return mLicNews;
            }
        }


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature to check</param>
        /// <param name="action">Action, if action is Insert limitations are not checked under administration interface</param>
        public bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            return LicenseVersionCheck(domain, feature, action, (action != ObjectActionEnum.Insert));
        }


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature to check</param>
        /// <param name="action">Action</param>
        /// <param name="siteCheck">If true limitations are not applied under URLs in Site manager, CMS Desk, CMSModules and CMSPages/Logon</param>
        public bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action, bool siteCheck)
        {
            // Parse domain name to remove port etc.
            if (domain != null)
            {
                domain = LicenseKeyInfoProvider.ParseDomainName(domain);
            }

            // Get version limitations
            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(domain, feature, siteCheck);


            if (versionLimitations == 0)
            {
                return true;
            }

            if (feature == FeatureEnum.Newsletters)
            {
                if (LicNews[domain] == null)
                {
                    LicNews[domain] = NewsletterInfoProvider.GetNewsletters().OnSite(LicenseHelper.GetSiteIDbyDomain(domain)).GetCount();
                }

                try
                {
                    // Try add
                    if (action == ObjectActionEnum.Insert)
                    {
                        if (versionLimitations < ValidationHelper.GetInteger(LicNews[domain], -1) + 1)
                        {
                            return false;
                        }
                    }

                    // Get status
                    if (action == ObjectActionEnum.Edit)
                    {
                        if (versionLimitations < ValidationHelper.GetInteger(LicNews[domain], 0))
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    ClearLicNewsletter(true);
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null)
        {
            domainName = domainName ?? RequestContext.CurrentDomain;

            if (!LicenseVersionCheck(domainName, FeatureEnum.Newsletters, action))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Newsletters);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks the license for insert for a new newsletter or for edit in other cases.
        /// </summary>
        /// <param name="newsletter">Newsletter</param>
        public void CheckLicense(NewsletterInfo newsletter)
        {
            var action = ObjectActionEnum.Edit;

            if ((newsletter != null) && (newsletter.NewsletterID <= 0))
            {
                action = ObjectActionEnum.Insert;
            }

            CheckLicense(action);
        }


        /// <summary>
        /// Clear license newsletter hashtable.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        public void ClearLicNewsletter(bool logTasks)
        {
            LicNews.Clear();

            if (logTasks)
            {
                WebFarmHelper.CreateTask(new ClearLicenseLimitationCacheWebFarmTask());
            }
        }


        /// <summary>
        /// Checks if newsletter tracking (open e-mail, click through and bounces) is available for current URL.
        /// </summary>
        public bool IsTrackingAvailable()
        {
            return (string.IsNullOrEmpty(RequestContext.CurrentDomain) || LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.NewsletterTracking));
        }


        /// <summary>
        /// Checks if newsletter A/B testing is available for current URL.
        /// </summary>
        public bool IsABTestingAvailable()
        {
            return (string.IsNullOrEmpty(RequestContext.CurrentDomain) || LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.NewsletterABTesting));
        }
    }
}
