using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class providing MVTestInfo management.
    /// </summary>
    public class MVTestInfoProvider : AbstractInfoProvider<MVTestInfo, MVTestInfoProvider>
    {
        #region "Variables"

        // Dictionary used for keeping info about a presence of a multivariate test for a page
        private static Dictionary<string, List<MVTestInfo>> pageSiteMVTests = new Dictionary<string, List<MVTestInfo>>();

        // Table lock for loading
        private static object tableLock = new object();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public MVTestInfoProvider()
            : base(MVTestInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the MVTestInfo objects.
        /// </summary>
        public static ObjectQuery<MVTestInfo> GetMVTests()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns multivariate test with specified ID.
        /// </summary>
        /// <param name="testId">Multivariate test ID</param>        
        public static MVTestInfo GetMVTestInfo(int testId)
        {
            return ProviderObject.GetInfoById(testId);
        }


        /// <summary>
        /// Returns multivariate test with the specified name.
        /// </summary>
        /// <param name="testName">The multivariate test name</param>                
        /// <param name="sitename">The site name</param>
        public static MVTestInfo GetMVTestInfo(string testName, string sitename)
        {
            return ProviderObject.GetMVTestInfoInternal(testName, sitename);
        }


        /// <summary>
        /// Sets (updates or inserts) specified multivariate test.
        /// </summary>
        /// <param name="testObj">The multivariate test to be set</param>
        public static void SetMVTestInfo(MVTestInfo testObj)
        {
            ProviderObject.SetInfo(testObj);
        }


        /// <summary>
        /// Deletes specified multivariate test.
        /// </summary>
        /// <param name="testObj">The multivariate test to be deleted</param>
        public static void DeleteMVTestInfo(MVTestInfo testObj)
        {
            ProviderObject.DeleteInfo(testObj);
        }


        /// <summary>
        /// Deletes multivariate test with specified ID.
        /// </summary>
        /// <param name="testId">The multivariate test ID</param>
        public static void DeleteMVTestInfo(int testId)
        {
            MVTestInfo mvTestObj = GetMVTestInfo(testId);
            DeleteMVTestInfo(mvTestObj);
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            lock (tableLock)
            {
                pageSiteMVTests.Clear();
            }

            base.ClearHashtables(logTasks);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Increases number of conversions in multivariate test and combination.
        /// </summary>
        /// <param name="testName">MVTest name</param>
        /// <param name="combinationName">Combination name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="hits">Hits count</param>
        /// <param name="culture">Culture code</param>
        public static int IncreaseConversions(string testName, string combinationName, string siteName, string culture, int hits)
        {
            return ProviderObject.IncreaseConversionsInternal(testName, combinationName, siteName, culture, hits);
        }


        /// <summary>
        /// Renames multivariate test statistics data when changed code name
        /// </summary>
        /// <param name="oldName">Old code name</param>
        /// <param name="newName">New code name</param>
        /// <param name="siteId">The site id</param>
        public static void RenameMVTestStatistics(string oldName, string newName, int siteId)
        {
            ProviderObject.RenameMVTestStatisticsInternal(oldName, newName, siteId);
        }


        /// <summary>
        /// Moves all MVTests from the document under the oldAlias path to the document under the newAlias path.
        /// </summary>
        /// <param name="newAlias">Document's new alias path</param>
        /// <param name="oldAlias">Document's old alias path</param>
        /// <param name="siteID">Document's siteID</param>
        public static void MoveMVTests(String newAlias, String oldAlias, int siteID)
        {
            ProviderObject.MoveMVTestsInternal(newAlias, oldAlias, siteID);
        }


        /// <summary>
        /// Indicates whether multivariate testing is enabled.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        public static bool MVTestingEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSMVTEnabled");
        }


        /// <summary>
        /// Indicates whether a document has defined any multivariate tests.
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="siteId">The site id</param>
        /// <param name="cultureCode">The culture code</param>
        /// <param name="isLiveSite">Is live site</param>
        public static bool ContainsMVTest(string path, int siteId, string cultureCode, bool isLiveSite)
        {
            return ProviderObject.ContainsMVTestInternal(path, siteId, cultureCode, isLiveSite);
        }


        /// <summary>
        /// Returns status of multivariate test.
        /// </summary>
        /// <param name="testObj">The multivariate test info object</param>
        public static MVTestStatusEnum GetMVTestStatus(MVTestInfo testObj)
        {
            return ProviderObject.GetMVTestStatusInternal(testObj);
        }


        /// <summary>
        /// Returns true if specified multivariate test is running.
        /// </summary>
        /// <param name="testObj">Multivariate test info object</param>
        public static bool MVTestIsRunning(MVTestInfo testObj)
        {
            MVTestStatusEnum status;
            return MVTestIsRunning(testObj, out status);
        }


        /// <summary>
        /// Returns true if specified multivariate test is running.
        /// </summary>
        /// <param name="testObj">Multivariate test info object</param>
        /// <param name="status">Returns current test status</param>
        public static bool MVTestIsRunning(MVTestInfo testObj, out MVTestStatusEnum status)
        {
            status = GetMVTestStatus(testObj);
            return (status == MVTestStatusEnum.Running);
        }


        /// <summary>
        /// Gets the running multivariate test for a page.
        /// </summary>
        /// <param name="testPage">The test page</param>
        /// <param name="siteId">The site id</param>
        /// <param name="cultureCode">The culture code</param>
        public static MVTestInfo GetRunningTest(string testPage, int siteId, string cultureCode)
        {
            bool testExits;
            return GetRunningTest(testPage, siteId, cultureCode, out testExits);
        }


        /// <summary>
        /// Determines whether there is a running multivariate test for the specified document.
        /// </summary>
        /// <param name="testPage">The test page</param>
        /// <param name="siteId">The site id</param>
        /// <param name="cultureCode">The culture code</param>
        public static bool ContainsRunningMVTest(string testPage, int siteId, string cultureCode)
        {
            return (GetRunningTest(testPage, siteId, cultureCode) != null);
        }


        /// <summary>
        /// Combines the specified page template instance with MVT variants.
        /// </summary>
        /// <param name="pi">The page info</param>
        /// <param name="instance">The page template instance</param>
        /// <param name="combinationId">The combination id</param>
        /// <param name="viewMode">The view mode</param>
        public static PageTemplateInstance CombineWithMVT(PageInfo pi, PageTemplateInstance instance, int combinationId, ViewModeEnum viewMode)
        {
            return ProviderObject.CombineWithMVTInternal(pi, instance, combinationId, viewMode);
        }


        /// <summary>
        /// Gets the running multivariate test for the specified page.
        /// </summary>
        /// <param name="testPage">The test page</param>
        /// <param name="siteId">The site id</param>
        /// <param name="cultureCode">The culture code</param>
        /// <param name="mvtTestExists">Returns a value indicating whether a mvt test with the given parameters exists</param>
        public static MVTestInfo GetRunningTest(string testPage, int siteId, string cultureCode, out bool mvtTestExists)
        {
            return ProviderObject.GetRunningTestInternal(testPage, siteId, cultureCode, out mvtTestExists);
        }


        /// <summary>
        /// Returns all conflicting tests (tests running in the same time interval on same page with same culture and are enabled).
        /// </summary>
        /// <param name="info">Info to check conflicts for</param>
        public static IEnumerable<MVTestInfo> GetConflicting(MVTestInfo info)
        {
            return ProviderObject.GetConflictingInternal(info);
        }


        /// <summary>
        /// Gets the running multivariate test for the specified page.
        /// </summary>
        /// <param name="testPage">The test page</param>
        /// <param name="siteId">The site id</param>
        public static List<MVTestInfo> GetMVTests(string testPage, int siteId)
        {
            return ProviderObject.GetMVTestsInternal(testPage, siteId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns multivariate with specified name.
        /// </summary>
        /// <param name="testName">The multivariate test name</param>   
        /// <param name="siteName">The site name</param>
        protected virtual MVTestInfo GetMVTestInfoInternal(string testName, string siteName)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                return GetInfoByCodeName(testName, si.SiteID, true);
            }
            return null;
        }


        /// <summary>
        /// Returns all conflicting tests (tests running in the same time interval on same page with same culture and are enabled).
        /// </summary>
        protected virtual IEnumerable<MVTestInfo> GetConflictingInternal(MVTestInfo info)
        {
            MVTestStatusEnum status = GetMVTestStatusInternal(info);
            if ((status != MVTestStatusEnum.Disabled) && (status != MVTestStatusEnum.Finished))
            {
                var tests = GetMVTests(info.MVTestPage, info.MVTestSiteID);
                if (!DataHelper.DataSourceIsEmpty(tests))
                {
                    DateTime openTo = (info.MVTestOpenTo == DateTimeHelper.ZERO_TIME) ? DateTime.MaxValue : info.MVTestOpenTo;
                    var infoTestRange = new Range<DateTime>(info.MVTestOpenFrom, openTo);
                    foreach (var test in tests)
                    {
                        // The same test
                        if (test.MVTestID == info.MVTestID)
                        {
                            continue;
                        }

                        // Disabled or finished test
                        status = GetMVTestStatusInternal(test);
                        if ((status == MVTestStatusEnum.Disabled) || (status == MVTestStatusEnum.Finished))
                        {
                            continue;
                        }

                        // Different culture
                        if ((test.MVTestCulture != info.MVTestCulture) && !String.IsNullOrEmpty(info.MVTestCulture) && !String.IsNullOrEmpty(test.MVTestCulture))
                        {
                            continue;
                        }

                        // Not disabled and running in intersecting time ranges -> conflict
                        DateTime testOpenTo = (test.MVTestOpenTo == DateTimeHelper.ZERO_TIME) ? DateTime.MaxValue : test.MVTestOpenTo;
                        var testRange = new Range<DateTime>(test.MVTestOpenFrom, testOpenTo);
                        if (testRange.IntersectsWith(infoTestRange))
                        {
                            yield return test;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(MVTestInfo info)
        {
            if (info != null)
            {
                var isNew = (info.MVTestID == 0);

                if (GetConflictingInternal(info).Any())
                {
                    info.MVTestEnabled = false;
                }

                // Save the multivariate test
                base.SetInfo(info);

                string pageSiteCode = GetPageSiteCode(info);
                bool reloadVariants = false;

                lock (tableLock)
                {
                    // Add the new test into the pageSiteMVTests dictionary
                    if (pageSiteMVTests.ContainsKey(pageSiteCode))
                    {
                        MVTestInfo tempTest = pageSiteMVTests[pageSiteCode].Find(t => t.MVTestID.Equals(info.MVTestID));
                        if (tempTest != null)
                        {
                            // Remove the cached test if already exists
                            pageSiteMVTests[pageSiteCode].Remove(tempTest);
                        }

                        // Add the test into the cached array
                        pageSiteMVTests[pageSiteCode].Add(info);

                        if (isNew && (pageSiteMVTests[pageSiteCode].Count == 1))
                        {
                            // This is the first test for the page => Load all web part/zone/widget variants
                            reloadVariants = true;
                        }
                    }
                    else
                    {
                        List<MVTestInfo> pageTests = new List<MVTestInfo>();
                        pageTests.Add(info);
                        pageSiteMVTests.Add(pageSiteCode, pageTests);

                        // This is the first test for the page => Load all web part/zone/widget variants
                        reloadVariants = true;
                    }
                }

                if ((isNew) && (reloadVariants))
                {
                    MVTCombinationInfoProvider.EnsureTestCombination(info);

                    // Set the context manually to allow to load all the MVT variants
                    PortalContext.MVTVariantsEnabled = true;

                    // Check whether this is the first multivariate test for the template. If it is then load all MVT web part/widget/ zone variants.
                    foreach (int templateId in GetTestTemplates(info))
                    {
                        PageTemplateInfo pi = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                        if ((pi != null) && (pi.TemplateInstance != null))
                        {
                            // Reload all MVT variants
                            pi.TemplateInstance.LoadVariants(true, VariantModeEnum.MVT);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(MVTestInfo info)
        {
            base.DeleteInfo(info);

            lock (tableLock)
            {
                pageSiteMVTests.Remove(GetPageSiteCode(info));
            }

            try
            {
                // Check whether this is the first multivariate test for the template. If it is then load all MVT web part/widget/ zone variants.
                foreach (int templateId in GetTestTemplates(info))
                {
                    PageTemplateInfo pi = PageTemplateInfoProvider.GetPageTemplateInfo(templateId);
                    if ((pi != null) && (pi.TemplateInstance != null))
                    {
                        // Reload all MVT variants
                        pi.TemplateInstance.LoadVariants(true, VariantModeEnum.MVT);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception will be thrown if page within MVTest does not exist
                // Log the error
                EventLogProvider.LogException("MVTest", "DELETE", ex);
            }
        }


        /// <summary>
        /// Gets the test templates according to tits culture.
        /// </summary>
        public static List<int> GetTestTemplates(MVTestInfo testObj)
        {
            return ProviderObject.GetTestTemplatesInternal(testObj);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Combines the specified page template instance with MVT variants.
        /// </summary>
        /// <param name="pi">The page info</param>
        /// <param name="instance">The page template instance</param>
        /// <param name="combinationId">The combination id</param>
        /// <param name="viewMode">The view mode</param>
        protected virtual PageTemplateInstance CombineWithMVTInternal(PageInfo pi, PageTemplateInstance instance, int combinationId, ViewModeEnum viewMode)
        {
            // Check whether page info and template instance are defined
            if ((pi != null) && (instance != null))
            {
                bool pageContainsMVTest;
                MVTestInfo candidate = GetRunningTest(pi.NodeAliasPath, pi.NodeSiteID, pi.DocumentCulture, out pageContainsMVTest);
                MVTCombinationInfo mvtCombination = null;

                if ((!pageContainsMVTest) ||
                    ((candidate == null) && (viewMode.IsLiveSite())))
                {
                    // No running test -> do not combine with MVT
                    return instance;
                }
                else
                {
                    // Check whether multivariate test candidate exists
                    if (combinationId == -1)
                    {
                        // Get combination from cookies or select new one
                        mvtCombination = MVTCombinationInfoProvider.GetMVTestCombination(candidate, pi, viewMode);
                        // Keep current test & combination
                        if (candidate != null)
                        {
                            MVTContext.CurrentMVTestName = candidate.MVTestName;
                        }
                        if (mvtCombination != null)
                        {
                            MVTContext.CurrentMVTCombinationName = mvtCombination.MVTCombinationName;
                        }
                    }
                    else if (combinationId > 0)
                    {
                        mvtCombination = MVTCombinationInfoProvider.GetMVTCombinationInfo(combinationId);
                    }

                    // If combination was found
                    if ((mvtCombination != null) && (instance.ParentPageTemplate != null))
                    {
                        // Get variants
                        DataSet variantsDs = MVTVariantInfoProvider.GetMVTVariants(instance.ParentPageTemplate.PageTemplateId, mvtCombination.MVTCombinationID);

                        // Check whether variants exists 
                        if (!DataHelper.DataSourceIsEmpty(variantsDs))
                        {
                            // Loop through all variants & create final XML
                            foreach (DataRow dr in variantsDs.Tables[0].Rows)
                            {
                                var variantId = ValidationHelper.GetInteger(dr["MVTVariantID"], 0);
                                var isZone = string.IsNullOrEmpty(ValidationHelper.GetString(dr["MVTVariantInstanceGUID"], string.Empty));

                                // Get the MVT variant info object
                                MVTVariantInfo variantObj = MVTVariantInfoProvider.GetMVTVariantInfo(variantId);
                                if (variantObj != null)
                                {
                                    // Load the zone instance
                                    if (isZone)
                                    {
                                        // Get the variant instance
                                        WebPartZoneInstance wpzi = variantObj.WebPartZoneInstance;

                                        if (wpzi == null)
                                        {
                                            // Variants not loaded yet, load them now
                                            instance.LoadVariants(true, VariantModeEnum.MVT);
                                            wpzi = variantObj.WebPartZoneInstance;
                                            if (wpzi == null)
                                            {
                                                // Error
                                                return instance;
                                            }
                                        }

                                        // Combine the page template instance with the MVT variants only when there are no CP+MVT variant conflicts
                                        WebPartZoneInstance originalWpzi = instance.GetZone(wpzi.ZoneID);
                                        if ((originalWpzi != null) && (originalWpzi.VariantMode != VariantModeEnum.ContentPersonalization))
                                        {
                                            instance.CombineWith(wpzi);
                                        }
                                    }
                                    // Load the web part/widget instance
                                    else
                                    {
                                        WebPartInstance wpi = variantObj.WebPartInstance;

                                        if (wpi == null)
                                        {
                                            // Variants not loaded yet, load them now
                                            instance.LoadVariants(true, VariantModeEnum.MVT);
                                            wpi = variantObj.WebPartInstance;
                                            if (wpi == null)
                                            {
                                                // Error
                                                return instance;
                                            }
                                        }

                                        // Do not select the widget variant in the Edit mode (all widget variants are being rendered)
                                        if (wpi.IsWidget && viewMode.IsOneOf(ViewModeEnum.Edit, ViewModeEnum.EditDisabled))
                                        {
                                            continue;
                                        }

                                        // Combine the page template instance with the MVT variants only when there are no CP+MVT variant conflicts
                                        WebPartInstance originalWpi = instance.GetWebPart(wpi.InstanceGUID);
                                        if ((originalWpi != null) && (originalWpi.VariantMode != VariantModeEnum.ContentPersonalization))
                                        {
                                            instance.CombineWith(wpi);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return instance;
        }


        /// <summary>
        /// Increases number of conversions in multivariate test and combination.
        /// </summary>
        /// <param name="testName">The multivariate test name</param>
        /// <param name="combinationName">The combination name</param>
        /// <param name="siteName">The site name</param>
        /// <param name="culture">The culture code</param>
        /// <param name="hits">The hits count</param>
        protected virtual int IncreaseConversionsInternal(string testName, string combinationName, string siteName, string culture, int hits)
        {
            // Get multivariate test object
            MVTestInfo mvtest = GetMVTestInfo(testName, siteName);

            // Check whether object is defined
            if (mvtest != null)
            {
                // Get combination  object
                MVTCombinationInfo combination = MVTCombinationInfoProvider.GetMVTCombinationInfo(mvtest.MVTestPage, combinationName, siteName, culture);

                // Check whether combination is defined
                if (combination != null)
                {
                    bool clearCache = false;

                    // Target is for total number of conversions
                    if (mvtest.MVTestTargetConversionType == MVTTargetConversionTypeEnum.Total)
                    {
                        // If hits + current conversion are larger then max 
                        // Log to fill max hits 
                        if ((mvtest.MVTestMaxConversions != 0) && ((mvtest.MVTestConversions + hits) >= mvtest.MVTestMaxConversions))
                        {
                            hits = mvtest.MVTestMaxConversions - mvtest.MVTestConversions;
                            clearCache = true;
                        }
                    }
                    else
                    {
                        // Conversion count for this combination cannot be larger than MaxConversions
                        if ((mvtest.MVTestMaxConversions != 0) && ((combination.MVTCombinationConversions + hits) >= mvtest.MVTestMaxConversions))
                        {
                            hits = mvtest.MVTestMaxConversions - combination.MVTCombinationConversions;
                            clearCache = true;
                        }
                    }

                    if (hits > 0)
                    {
                        using (var context = new CMSActionContext())
                        {
                            context.TouchCacheDependencies = clearCache;
                            mvtest.MVTestConversions += hits;
                            SetMVTestInfo(mvtest);
                            combination.MVTCombinationConversions += hits;
                            MVTCombinationInfoProvider.SetMVTCombinationInfo(combination);
                        }
                    }

                    return hits;
                }
            }
            return 0;
        }


        /// <summary>
        /// Renames multivariate test statistics data when changed code name
        /// </summary>
        /// <param name="oldName">Old code name</param>
        /// <param name="newName">New code name</param>
        /// <param name="siteID">Test site ID </param>
        protected virtual void RenameMVTestStatisticsInternal(string oldName, string newName, int siteID)
        {
            // Test code name change
            if (newName != oldName)
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@OldName", oldName);
                parameters.Add("@NewName", newName);
                parameters.Add("@SiteID", siteID);

                ConnectionHelper.ExecuteQuery("OM.MVTest.RenameMVTestStatistics", parameters);
            }
        }


        /// <summary>
        /// Indicates whether a document has defined any multivariate tests.
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="siteId">The site id</param>
        /// <param name="cultureCode">The culture code</param>
        /// <param name="isLiveSite">Is live site</param>
        protected virtual bool ContainsMVTestInternal(string path, int siteId, string cultureCode, bool isLiveSite)
        {
            bool testExists;
            MVTestInfo testObj = GetRunningTest(path, siteId, cultureCode, out testExists);
            if (isLiveSite)
            {
                return MVTestIsRunning(testObj);
            }

            return testExists;
        }


        /// <summary>
        /// Returns status of multivariate test.
        /// </summary>
        /// <param name="testObj">The multivariate test info object</param>
        protected virtual MVTestStatusEnum GetMVTestStatusInternal(MVTestInfo testObj)
        {
            // Check whether multivariate test is defined and enabled
            if ((testObj != null) && (testObj.MVTestEnabled))
            {
                DateTime currentTime = DateTime.Now;

                // Check whether from/to are valid
                if (((testObj.MVTestOpenFrom == DateTimeHelper.ZERO_TIME) || (testObj.MVTestOpenFrom < currentTime)) &&
                    ((testObj.MVTestOpenTo == DateTimeHelper.ZERO_TIME) || (testObj.MVTestOpenTo > currentTime)))
                {
                    // If MVTest max Conversion not set - return true
                    if (testObj.MVTestMaxConversions != 0)
                    {
                        if (testObj.MVTestTargetConversionType == MVTTargetConversionTypeEnum.Total)
                        {
                            // Check whether count of target count of conversion is lower than current
                            if (testObj.MVTestMaxConversions <= testObj.MVTestConversions)
                            {
                                // Reached maximal count of conversions                                
                                return MVTestStatusEnum.Finished;
                            }
                        }
                        else
                        {
                            return GetMVTStatusByCombinations(testObj);
                        }
                    }

                    return MVTestStatusEnum.Running;
                }
                else
                {
                    // Test has not started yet
                    if ((testObj.MVTestOpenFrom != DateTimeHelper.ZERO_TIME) && (currentTime < testObj.MVTestOpenFrom))
                    {
                        return MVTestStatusEnum.NotRunning;
                    }

                    return MVTestStatusEnum.Finished;
                }
            }

            return MVTestStatusEnum.Disabled;
        }


        /// <summary>
        /// Returns "Running" or "Finished", based only on MVTest combinations.
        /// </summary>
        /// <param name="testObj">MVTestInfo object</param>
        private MVTestStatusEnum GetMVTStatusByCombinations(MVTestInfo testObj)
        {
            SiteInfo site = SiteInfoProvider.GetSiteInfo(testObj.MVTestSiteID);

            var treeProvider = new TreeProvider(MembershipContext.AuthenticatedUser);
            var node = treeProvider.SelectSingleNode(site.SiteName, testObj.MVTestPage, testObj.MVTestCulture);

            if (node != null)
            {
                var combinations = MVTCombinationInfoProvider.GetCombinationsForTemplate(node.DocumentPageTemplateID, true);

                if (!DataHelper.DataSourceIsEmpty(combinations))
                {
                    foreach (var combination in combinations)
                    {
                        if (combination.MVTCombinationConversions >= testObj.MVTestMaxConversions)
                        {
                            // One combination reached maximal conversions count
                            return MVTestStatusEnum.Finished;
                        }
                    }
                }
            }

            return MVTestStatusEnum.Running;
        }


        /// <summary>
        /// Gets the running multivariate test for a page.
        /// </summary>
        /// <param name="testPage">The test page</param>
        /// <param name="siteId">The site id</param>
        /// <param name="cultureCode">The culture code</param>
        /// <param name="mvtTestExists">Returns a value indicating whether a multivariate test with the given parameters exists</param>
        protected virtual MVTestInfo GetRunningTestInternal(string testPage, int siteId, string cultureCode, out bool mvtTestExists)
        {
            MVTestInfo runningTestObj = null;
            mvtTestExists = false;

            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);

            if ((si != null)
                && SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSAnalyticsEnabled")
                && SettingsKeyInfoProvider.GetBoolValue(si.SiteName + ".CMSMVTEnabled"))
            {
                List<MVTestInfo> testObjList = GetMVTests(testPage, siteId);
                if (testObjList != null)
                {
                    // Get running test
                    MVTestStatusEnum status = MVTestStatusEnum.Disabled;

                    mvtTestExists = (testObjList.Count > 0);

                    foreach (MVTestInfo mvTestInfo in testObjList)
                    {
                        if (string.IsNullOrEmpty(mvTestInfo.MVTestCulture) || (mvTestInfo.MVTestCulture == cultureCode))
                        {
                            // Check whether test is running
                            if (MVTestIsRunning(mvTestInfo, out status))
                            {
                                runningTestObj = mvTestInfo;
                                break;
                            }
                        }
                    }
                }
            }

            return runningTestObj;
        }


        /// <summary>
        /// Gets the running multivariate test for a page.
        /// </summary>
        /// <param name="testPage">The test page</param>
        /// <param name="siteId">The site id</param>
        protected virtual List<MVTestInfo> GetMVTestsInternal(string testPage, int siteId)
        {
            List<MVTestInfo> testObjList = null;

            if (!string.IsNullOrEmpty(testPage)
                && (siteId > 0))
            {
                string pageSiteCode = GetPageSiteCode(testPage, siteId);

                lock (tableLock)
                {
                    if (pageSiteMVTests.ContainsKey(pageSiteCode))
                    {
                        testObjList = pageSiteMVTests[pageSiteCode];
                    }
                    else
                    {
                        // Get MVT tests for specific site
                        DataSet ds = GetMVTests().WhereEquals("MVTestPage", testPage)
                                                 .OnSite(siteId)
                                                 .TypedResult;

                        testObjList = new List<MVTestInfo>();

                        // Loop through all MV tests and try find enabled test related to current page info
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            MVTestInfo mvTest = new MVTestInfo(dr);
                            testObjList.Add(mvTest);
                        }

                        pageSiteMVTests.Add(pageSiteCode, testObjList);
                    }
                }
            }

            return testObjList;
        }


        /// <summary>
        /// Returns the where condition selecting tests which are running at the same time as the specified test.
        /// </summary>
        /// <param name="testObj">The multivariate test info object</param>
        /// <param name="parameters">The parameters</param>
        protected virtual string GetRunningConditionInternal(MVTestInfo testObj, out QueryDataParameters parameters)
        {
            if (testObj != null)
            {
                return GetRunningConditionInternal(testObj.MVTestID, testObj.MVTestPage, testObj.MVTestSiteID, testObj.MVTestCulture, testObj.MVTestOpenFrom, testObj.MVTestOpenTo, out parameters);
            }

            parameters = null;
            return string.Empty;
        }


        /// <summary>
        /// Returns the where condition selecting multivariate tests which are running for the same conditions as defined in the method params (testPage, testSiteId, testCulture...).
        /// </summary>
        /// <param name="testId">The test id to be excluded from the where condition</param>
        /// <param name="testPage">The test page</param>
        /// <param name="testSiteId">The test site id</param>
        /// <param name="testCulture">The test culture</param>
        /// <param name="openFrom">The open from</param>
        /// <param name="openTo">The open to</param>
        /// <param name="parameters">The parameters</param>
        /// <returns>String containing actual Where condition</returns>
        protected virtual string GetRunningConditionInternal(int testId, string testPage, int testSiteId, string testCulture, DateTime openFrom, DateTime openTo, out QueryDataParameters parameters)
        {
            var where = "MVTestID <> " + testId;
            where = SqlHelper.AddWhereCondition(where, "MVTestPage = N'" + testPage.Replace("'", "''") + "'");
            where = SqlHelper.AddWhereCondition(where, "MVTestSiteID = " + testSiteId);
            where = SqlHelper.AddWhereCondition(where, "MVTestEnabled = 1");

            if (!string.IsNullOrEmpty(testCulture))
            {
                where = SqlHelper.AddWhereCondition(where, "(MVTestCulture = N'" + testCulture.Replace("'", "''") + "') OR (MVTestCulture IS NULL) OR (MVTestCulture = '')");
            }

            where = SqlHelper.AddWhereCondition(where, "ISNULL(MVTestMaxConversions,0) = 0 OR MVTestMaxConversions > ISNULL(MVTestConversions,0)");

            parameters = new QueryDataParameters();
            parameters.Add("@dateTimeMinValue", DataTypeManager.MIN_DATETIME);
            parameters.Add("@dateTimeMaxValue", DataTypeManager.MAX_DATETIME);
            parameters.Add("@newOpenFrom", (openFrom != DateTimeHelper.ZERO_TIME) ? openFrom : DataTypeManager.MIN_DATETIME);
            parameters.Add("@newOpenTo", (openTo != DateTimeHelper.ZERO_TIME) ? openTo : DataTypeManager.MAX_DATETIME);
            parameters.Add("@now", DateTime.Now);

            where = SqlHelper.AddWhereCondition(where, "(COALESCE(MVTestOpenFrom, @dateTimeMinValue) <= @newOpenTo) AND (COALESCE(MVTestOpenTo, @dateTimeMaxValue) >= @newOpenFrom) AND (MVTestOpenTo IS NULL OR MVTestOpenTo > @Now)");

            return where;
        }


        /// <summary>
        /// Gets the page site code.
        /// </summary>
        /// <param name="testObj">The multivariate test info object</param>
        protected virtual string GetPageSiteCode(MVTestInfo testObj)
        {
            if (testObj != null)
            {
                return GetPageSiteCode(testObj.MVTestPage, testObj.MVTestSiteID);
            }

            return string.Empty;
        }


        /// <summary>
        /// Gets the page site code.
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="siteId">The site id</param>
        protected virtual string GetPageSiteCode(string path, int siteId)
        {
            return siteId + "_" + path;
        }


        /// <summary>
        /// Moves all MVTests from the document under the oldAlias path to the document under the newAlias path.
        /// </summary>
        /// <param name="newAlias">Document's new alias path</param>
        /// <param name="oldAlias">Document's old alias path</param>
        /// <param name="siteID">Document's site ID</param>
        protected virtual void MoveMVTestsInternal(String newAlias, String oldAlias, int siteID)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@newAlias", newAlias);
            parameters.Add("@oldAlias", oldAlias);
            parameters.Add("@siteID", siteID);

            ConnectionHelper.ExecuteQuery("OM.MVTest.MoveTests", parameters);
        }


        /// <summary>
        /// Gets the test templates according to test culture.
        /// </summary>
        /// <param name="testObj">Test object</param>
        protected virtual List<int> GetTestTemplatesInternal(MVTestInfo testObj)
        {
            string siteName = SiteInfoProvider.GetSiteName(testObj.MVTestSiteID);
            List<int> pageTemplateIds = new List<int>();

            if (string.IsNullOrEmpty(testObj.MVTestCulture))
            {
                // All cultures
                TreeProvider tree = new TreeProvider();

                DataSet ds = tree.SelectNodes(siteName, testObj.MVTestPage, TreeProvider.ALL_CULTURES, false);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        // Figure out the template column name
                        bool allCultures = ValidationHelper.GetBoolean(row["NodeTemplateForAllCultures"], false);
                        string colName = (allCultures ? "NodeTemplateID" : "DocumentPageTemplateID");

                        var pageTemplateId = ValidationHelper.GetInteger(row[colName], 0);

                        // If inherited page template, try to get parent page template
                        if (pageTemplateId <= 0)
                        {
                            string aliasPath = ValidationHelper.GetString(row["NodeAliasPath"], "");
                            int nodeId = ValidationHelper.GetInteger(row["NodeID"], 0);

                            // Get the page info
                            PageInfo pi = PageInfoProvider.GetPageInfo(siteName, aliasPath, LocalizationContext.PreferredCultureCode, null, nodeId, SiteInfoProvider.CombineWithDefaultCulture(siteName));
                            if ((pi != null) && (pi.UsedPageTemplateInfo != null))
                            {
                                pageTemplateId = pi.UsedPageTemplateInfo.PageTemplateId;
                            }
                        }

                        if ((pageTemplateId > 0) && (!pageTemplateIds.Contains(pageTemplateId)))
                        {
                            // Select all page templates for all cultures of the document (specified by MVTestPage)
                            pageTemplateIds.Add(pageTemplateId);
                        }
                    }
                }
            }
            else
            {
                PageInfo pi = PageInfoProvider.GetPageInfo(siteName, testObj.MVTestPage, testObj.MVTestCulture, null, SiteInfoProvider.CombineWithDefaultCulture(siteName));

                if (pi != null)
                {
                    int templateId = pi.GetUsedPageTemplateId();

                    // Select the page template for the current culture
                    pageTemplateIds.Add(templateId);
                }
            }

            return pageTemplateIds;
        }

        #endregion


        #region "Enum helper methods"

        /// <summary>
        /// Returns MVTTargetConversionType string.
        /// </summary>
        /// <param name="conversionType">Target conversion type</param>
        public static string GetMVTTargetConversionTypeString(MVTTargetConversionTypeEnum conversionType)
        {
            switch (conversionType)
            {
                case MVTTargetConversionTypeEnum.AnyCombination:
                    return "ANYCOMBINATION";

                default:
                    return "TOTAL";
            }
        }


        /// <summary>
        /// Returns MVTTargetConversionType enum.
        /// </summary>
        /// <param name="conversionType">String representation of target conversion type</param>
        public static MVTTargetConversionTypeEnum GetMVTTargetConversionTypeEnum(string conversionType)
        {
            switch (conversionType.ToLowerCSafe())
            {
                case "anycombination":
                    return MVTTargetConversionTypeEnum.AnyCombination;

                default:
                    return MVTTargetConversionTypeEnum.Total;
            }
        }


        /// <summary>
        /// Returns MVTestStatus string.
        /// </summary>
        /// <param name="statusType">Test status type</param>
        public static string GetMVTestStatusString(MVTestStatusEnum statusType)
        {
            switch (statusType)
            {
                case MVTestStatusEnum.Running:
                    return "RUNNING";

                case MVTestStatusEnum.Finished:
                    return "FINISHED";

                default:
                    return "NONE";
            }
        }


        /// <summary>
        /// Returns MVTestStatus enum.
        /// </summary>
        /// <param name="statusType">String representation of test status type</param>
        public static MVTestStatusEnum GetMVTestStatusEnum(string statusType)
        {
            switch (statusType.ToLowerCSafe())
            {
                case "running":
                    return MVTestStatusEnum.Running;

                case "finished":
                    return MVTestStatusEnum.Finished;

                default:
                    return MVTestStatusEnum.Disabled;
            }
        }

        #endregion
    }
}