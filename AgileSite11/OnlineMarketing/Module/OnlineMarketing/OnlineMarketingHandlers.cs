using System;
using System.Collections.Generic;

using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Online marketing event handlers.
    /// </summary>
    internal class OnlineMarketingHandlers
    {
        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            PageInfoEvents.CombinePageTemplateInstance.After += CombinePageTemplateInstance_After;

            PageTemplateEvents.PageTemplateCloneAsAdHoc.After += CloneTemplateVariants;

            WebAnalyticsEvents.ProcessAnalyticsService.Before += LogAbAndMvtContext;

            ActivityEvents.ActivityProcessedInLogService.Execute += AddABVariantNameOrMVTCombinationNameToActivity;

            ApplicationEvents.End.Execute += StoreActivities;

            ContactManagementEvents.DuplicateActivitiesForContact.Execute += DuplicateActivitiesForContact;
        }


        /// <summary>
        /// Duplicates record in Activity table and all related tables if required.
        /// </summary>
        private static void DuplicateActivitiesForContact(object sender, DuplicateActivitiesForContactEventArgs e)
        {
            var contact = e.Contact;
            foreach (var activity in e.Activities)
            {
                // Check license
                if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
                {
                    LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement);
                }

                // Duplicate record in base table
                ActivityInfo duplicate = new ActivityInfo();
                duplicate.ActivityContactID = contact != null ? contact.ContactID : activity.ActivityContactID;
                duplicate.ActivityCreated = activity.ActivityCreated;
                duplicate.ActivityType = activity.ActivityType;
                duplicate.ActivityItemID = activity.ActivityItemID;
                duplicate.ActivityItemDetailID = activity.ActivityItemDetailID;
                duplicate.ActivityValue = activity.ActivityValue;
                duplicate.ActivityURL = activity.ActivityURL;
                duplicate.ActivityTitle = activity.ActivityTitle;
                duplicate.ActivityNodeID = activity.ActivityNodeID;
                duplicate.ActivitySiteID = activity.ActivitySiteID;
                duplicate.ActivityCampaign = activity.ActivityCampaign;
                duplicate.ActivityURLReferrer = activity.ActivityURLReferrer;
                duplicate.ActivityCulture = activity.ActivityCulture;
                duplicate.ActivityABVariantName = activity.ActivityABVariantName;
                duplicate.ActivityMVTCombinationName = activity.ActivityMVTCombinationName;
                ActivityInfoProvider.SetActivityInfo(duplicate);
            }
        }


        /// <summary>
        /// Performs bulk insert of activities currently stored in memory before the application ends to ensure the persistence.
        /// </summary>
        private static void StoreActivities(object sender, EventArgs e)
        {
            Service.Resolve<IActivityQueueProcessor>().InsertActivitiesFromQueueToDB();
        }


       
        /// <summary>
        /// Adds details to <see cref="PredefinedActivityType.PAGE_VISIT"/> or <see cref="PredefinedActivityType.LANDING_PAGE"/> activity. 
        /// </summary>
        private static void AddABVariantNameOrMVTCombinationNameToActivity(object sender, CMSEventArgs<IActivityInfo> cmsEventArgs)
        {
            var activityInfo = cmsEventArgs.Parameter;

            if (activityInfo.ActivityType == PredefinedActivityType.PAGE_VISIT || activityInfo.ActivityType == PredefinedActivityType.LANDING_PAGE)
            {
                activityInfo.ActivityABVariantName = ABTestContext.CurrentABTestVariant != null 
                                                            ? ABTestContext.CurrentABTestVariant.ABVariantName
                                                            : null;

                activityInfo.ActivityMVTCombinationName = MVTContext.CurrentMVTCombinationName;
            }
        }

        
        /// <summary>
        /// Sets AB test and MVTest context from event parameters.
        /// </summary>
        private static void LogAbAndMvtContext(object sender, AnalyticsJSEventArgs e)
        {
            var siteName = SiteContext.CurrentSiteName;
            SetAbAndMvtContext(e.QueryParameters, siteName);
        }


        /// <summary>
        /// Sets up <see cref="ABTestContext"/> and <see cref="MVTContext"/> according to the parameters in given <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">Dictionary containing all required parameters</param>
        /// <param name="siteName">Site name for which should be the context set up</param>
        private static void SetAbAndMvtContext(IDictionary<string, string> parameters, string siteName)
        {
            string abTestName = parameters["ABTestName"];
            if (!string.IsNullOrEmpty(abTestName))
            {
                ABTestContext.CurrentABTest = ABTestInfoProvider.GetABTestInfo(abTestName, siteName);
                string abVariantName = parameters["ABTestVariantName"];
                if (!string.IsNullOrEmpty(abVariantName))
                {
                    ABTestContext.CurrentABTestVariant = ABVariantInfoProvider.GetABVariantInfo(abVariantName, abTestName, siteName);
                }
            }

            string mvTestCombinationName = parameters["MVTestCombinationName"];
            if (!string.IsNullOrEmpty(mvTestCombinationName))
            {
                MVTContext.CurrentMVTCombinationName = mvTestCombinationName;
            }
        }


        /// <summary>
        /// Clones content personalization and MVT variants of the template after template is cloned as ad-hoc.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="pageTemplateClonedEventArgs">Template clone data</param>
        private static void CloneTemplateVariants(object sender, PageTemplateCloneEventArgs pageTemplateClonedEventArgs)
        {
            if ((pageTemplateClonedEventArgs.OriginalPageTemplate == null) || (pageTemplateClonedEventArgs.NewPageTemplate == null))
            {
                throw new ArgumentException("[OnlineMarketingHandlers.CloneTemplateVariants]: Both original and new templates have to be set in order to clone template variants.");
            }

            int originalId = pageTemplateClonedEventArgs.OriginalPageTemplate.PageTemplateId;
            int newId = pageTemplateClonedEventArgs.NewPageTemplate.PageTemplateId;

            if (LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ContentPersonalization))
            {
                if (ContentPersonalizationVariantInfoProvider.ContentPersonalizationEnabled(SiteContext.CurrentSiteName))
                {
                    ContentPersonalizationVariantInfoProvider.CloneTemplateVariants(originalId, newId);
                }
            }

            if (LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.MVTesting))
            {
                if (MVTestInfoProvider.MVTestingEnabled(SiteContext.CurrentSiteName))
                {
                    MVTVariantInfoProvider.CloneTemplateVariants(originalId, newId);
                }
            }
        }


        /// <summary>
        /// Combines the page template instance with MVT and personalization data
        /// </summary>
        private static void CombinePageTemplateInstance_After(object sender, PageInfoEventArgs e)
        {
            // Get parameters
            PageInfo pi = e.PageInfo;
            PageTemplateInstance ti = e.PageTemplateInstance;

            // Get site info
            var si = SiteInfoProvider.GetSiteInfo(pi.NodeSiteID);
            if (si != null)
            {
                ViewModeEnum viewMode = PortalContext.ViewMode;

                // Combine with Content personalization
                if (PortalContext.ContentPersonalizationEnabled)
                {
                    if (!PortalContext.IsDesignMode(viewMode) && (viewMode != ViewModeEnum.EditLive))
                    {
                        ti = ContentPersonalizationVariantInfoProvider.CombineWithPersonalization(pi, ti, PortalContext.ViewMode);
                    }
                }

                // Combine with MVT (if enabled)
                // Check whether MV testing is enabled
                if (AnalyticsHelper.AnalyticsEnabled(si.SiteName)
                    && SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSMVTEnabled")
                    && !SearchCrawler.IsCrawlerRequest()
                    && LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.MVTesting)
                    && ResourceSiteInfoProvider.IsResourceOnSite("cms.mvtest", SiteContext.CurrentSiteName))
                {
                    int combinationId = QueryHelper.GetInteger("combinationid", 0);

                    if (!PortalContext.IsDesignMode(viewMode) && (viewMode != ViewModeEnum.EditLive))
                    {
                        if ((viewMode != ViewModeEnum.LiveSite) && (combinationId > 0))
                        {
                            ti = MVTestInfoProvider.CombineWithMVT(pi, ti, combinationId, PortalContext.ViewMode);
                        }
                        else
                        {
                            ti = MVTestInfoProvider.CombineWithMVT(pi, ti, -1, PortalContext.ViewMode);
                        }
                    }
                }
            }

            // Assign new page template instance
            e.PageTemplateInstance = ti;
        }

        #endregion
    }
}