using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Provides methods to check availibility of personalization feature.
    /// </summary>
    internal class PersonalizationAvailabilityChecker : IFeatureAvailabilityChecker
    {
        /// <summary>
        /// Indicates if feature is enabled.
        /// </summary>
        public bool IsFeatureEnabled()
        {
            return SettingsKeyInfoProvider.GetBoolValue(SiteContext.CurrentSiteName + ".CMSContentPersonalizationEnabled");
        }


        /// <summary>
        /// Indicates if license requirements for feature are met.
        /// </summary>
        public bool IsFeatureAvailable()
        {
            return LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.ContentPersonalization);
        }
    }
}
