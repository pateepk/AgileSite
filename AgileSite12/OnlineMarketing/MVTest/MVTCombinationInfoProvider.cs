using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing
{
    using TypedDataSet = InfoDataSet<MVTCombinationInfo>;


    /// <summary>
    /// Class providing MVTCombinationInfo management.
    /// </summary>
    public class MVTCombinationInfoProvider : AbstractInfoProvider<MVTCombinationInfo, MVTCombinationInfoProvider>
    {
        #region "Private constants"

        /// <summary>
        /// Default prefix for new combinations.
        /// </summary>
        protected const string COMBINATION_NAME_PREFIX = "Combination_";


        /// <summary>
        /// The maximum allowed length of the automatically generated combination custom names.
        /// </summary>
        protected const int COMBINATION_CUSTOMNAME_MAX_LENGHT = 50;

        #endregion


        #region "Variables"

        // Random generator for choosing a combination.
        private static Random mRandomGenerator;


        /// <summary>
        /// Dictionary containing combination infos with keys: templateID_combinationName 
        /// </summary>
        private static CMSStatic<ProviderInfoDictionary<string>> mTemplateCombinations = new CMSStatic<ProviderInfoDictionary<string>>();


        // Table lock for loading
        private static readonly object tableLock = new object();

        
        private static readonly object randomGeneratorLock = new object();

        #endregion


        #region "Private properties"

        /// <summary>
        /// Dictionary containing combination infos with keys: templateID_combinationName 
        /// </summary>
        private static ProviderInfoDictionary<string> TemplateCombinations
        {
            get
            {
                return mTemplateCombinations.Value;
            }
            set
            {
                mTemplateCombinations.Value = value;
            }
        }


        /// <summary>
        /// Gets random generator for choosing a combination.
        /// </summary>
        private static Random RandomGenerator
        {
            get
            {
                if (mRandomGenerator == null)
                {
                    mRandomGenerator = new Random();
                }
                return mRandomGenerator;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public MVTCombinationInfoProvider()
            : base(MVTCombinationInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.None,
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the MVTCombinationInfo objects.
        /// </summary>
        public static ObjectQuery<MVTCombinationInfo> GetMVTCombinations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns MVT combination with the specified ID.
        /// </summary>
        /// <param name="combinationId">Combination ID</param>        
        public static MVTCombinationInfo GetMVTCombinationInfo(int combinationId)
        {
            return ProviderObject.GetInfoById(combinationId);
        }


        /// <summary>
        /// Returns MVT combination with the specified combination name and template id.
        /// </summary>
        /// <param name="combinationName">Code name of the combination.</param>
        /// <param name="pageTemplateId">The page template id.</param>
        public static MVTCombinationInfo GetMVTCombinationInfo(int pageTemplateId, string combinationName)
        {
            return ProviderObject.GetMVTCombinationInfoInternal(pageTemplateId, combinationName);
        }


        /// <summary>
        /// Returns MVT combination with the specified parameters.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combinationName">Combination name</param>
        /// <param name="siteName">Site name</param>
        public static MVTCombinationInfo GetMVTCombinationInfo(string aliasPath, string combinationName, string siteName, string culture)
        {
            return ProviderObject.GetMVTCombinationInfoInternal(aliasPath, combinationName, siteName, culture);
        }


        /// <summary>
        /// Returns the default MVT combination for the specified page template.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        public static MVTCombinationInfo GetDefaultCombinationInfo(int pageTemplateId)
        {
            return ProviderObject.GetDefaultCombinationInfoInternal(pageTemplateId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified MVT combination.
        /// </summary>
        /// <param name="combinationObj">MVT combination to be set</param>
        public static void SetMVTCombinationInfo(MVTCombinationInfo combinationObj)
        {
            ProviderObject.SetInfo(combinationObj);
        }


        /// <summary>
        /// Deletes specified MVT combination.
        /// </summary>
        /// <param name="combinationObj">MVT combination to be deleted</param>
        public static void DeleteMVTCombinationInfo(MVTCombinationInfo combinationObj)
        {
            ProviderObject.DeleteInfo(combinationObj);
        }


        /// <summary>
        /// Deletes MVT combination with the specified ID.
        /// </summary>
        /// <param name="combinationId">MVT combination ID</param>
        public static void DeleteMVTCombinationInfo(int combinationId)
        {
            MVTCombinationInfo mvtCombinationObj = GetMVTCombinationInfo(combinationId);
            DeleteMVTCombinationInfo(mvtCombinationObj);
        }


        /// <summary>
        /// Deletes MVT combinations using the specified condition.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        public static void DeleteMVTCombinationInfo(string whereCondition)
        {
            ProviderObject.DeleteMVTCombinationInfoInternal(whereCondition);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Checks whether the specified MVT combination is assigned to a specified multivariate test and the multivariate test is running.
        /// </summary>
        /// <param name="combinationName">Combination name</param>
        /// <param name="testName">Test name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture code</param>
        public static bool IsMVTCombinationValid(string combinationName, string testName, string siteName, string culture)
        {
            return ProviderObject.IsMVTCombinationValidInternal(combinationName, testName, siteName, culture);
        }


        /// <summary>
        /// Returns dataset of MVT combinations for the specified page template.
        /// </summary>
        /// <param name="pageTemplateId">PageTemplate id parameter</param>
        /// <param name="enabledOnly">Indicates whether only enabled combinations should be loaded</param>
        public static TypedDataSet GetCombinationsForTemplate(int pageTemplateId, bool enabledOnly)
        {
            return ProviderObject.GetCombinationsForTemplateInternal(pageTemplateId, enabledOnly);
        }


        /// <summary>
        /// Gets the combinations which do not contain the specified webpart (instanceGuid).
        /// </summary>
        /// <param name="pageTemplateId">The template id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="documentId">The document id</param>
        public static TypedDataSet GetCombinationsWithoutWebpart(int pageTemplateId, Guid instanceGuid, int documentId)
        {
            return ProviderObject.GetCombinationsWithoutWebpartInternal(pageTemplateId, instanceGuid, documentId);
        }


        /// <summary>
        /// Gets the combinations which do not contain the specified zone.
        /// </summary>
        /// <param name="pageTemplateId">The template id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="documentId">The document id</param>
        public static TypedDataSet GetCombinationsWithoutZone(int pageTemplateId, string zoneId, int documentId)
        {
            return ProviderObject.GetCombinationsWithoutZoneInternal(pageTemplateId, zoneId, documentId);
        }


        /// <summary>
        /// Gets the new combination number.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        public static int GetNewCombinationNumber(int pageTemplateId, int documentId)
        {
            return ProviderObject.GetNewCombinationNumberInternal(pageTemplateId, documentId);
        }


        /// <summary>
        /// Ensures that the multivariate test has a default combination created.
        /// </summary>
        /// <param name="testObj">The multivariate test obj</param>
        public static void EnsureTestCombination(MVTestInfo testObj)
        {
            ProviderObject.EnsureTestCombinationInternal(testObj);
        }


        /// <summary>
        /// Ensures that the page template has a default combination created.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        public static void EnsureTestCombination(int pageTemplateId)
        {
            ProviderObject.EnsureTestCombinationInternal(pageTemplateId);
        }


        /// <summary>
        /// Disables/enables all combinations where the MVT variant is used.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="variantId">The MVT variant id</param>
        /// <param name="enable">True = Enable the combinations; False = Disable the combinations</param>
        public static void EnableCombinations(int pageTemplateId, int variantId, bool enable)
        {
            ProviderObject.EnableCombinationsInternal(pageTemplateId, variantId, enable);
        }


        /// <summary>
        /// Returns the combination for the specified multivariate test object.
        /// If any combination for this multivariate test is stored in a browser cookie then this combination will be used,
        /// otherwise will be chosen a random combination for this test.
        /// </summary>
        /// <param name="testObj">The multivariate test object</param>
        /// <param name="culture">Required culture</param>
        /// <param name="viewMode">The view mode</param>
        public static MVTCombinationInfo GetMVTestCombination(MVTestInfo testObj, string culture, ViewModeEnum viewMode)
        {
            return ProviderObject.GetMVTestCombinationInternal(testObj, culture, viewMode);
        }


        /// <summary>
        /// Returns the combination for the specified multivariate test object.
        /// If any combination for this multivariate test is stored in a browser cookie then this combination will be used,
        /// otherwise will be chosen a random combination for this test.
        /// </summary>
        /// <param name="testObj">The multivariate test info object</param>
        /// <param name="pi">The page info object</param>
        /// <param name="viewMode">The view mode</param>
        public static MVTCombinationInfo GetMVTestCombination(MVTestInfo testObj, PageInfo pi, ViewModeEnum viewMode)
        {
            return ProviderObject.GetMVTestCombinationInternal(testObj, pi, viewMode);
        }


        /// <summary>
        /// Combines the old custom name with the new variant.
        /// </summary>
        /// <param name="oldCombinationCustomName">The old combination custom name</param>
        /// <param name="newVariantName">The new variant name</param>
        /// <param name="newCombinationNumber">The new combination number</param>
        public static string GetNewCombinationCustomName(string oldCombinationCustomName, string newVariantName, int newCombinationNumber)
        {
            return ProviderObject.GetNewCombinationCustomNameInternal(oldCombinationCustomName, newVariantName, newCombinationNumber);
        }


        /// <summary>
        /// Gets the formated name of the combination (i.e.: number=2 returns "Combination_002").
        /// </summary>
        /// <param name="combinationNumber">The number used in the formated combination name</param>
        public static string GetCombinationName(int combinationNumber)
        {
            return ProviderObject.GetCombinationNameInternal(combinationNumber);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(MVTCombinationInfo info)
        {
            bool isNew = false;

            if (info != null)
            {
                isNew = (info.MVTCombinationID == 0);
                info.Generalized.CheckUnique = false;
            }

            base.SetInfo(info);

            CacheHelper.TouchKey("om.mvtcombination|bytemplateid|" + info.MVTCombinationPageTemplateID);

            // Enable all combination variants if the combination was enabled
            if ((info.MVTCombinationEnabled) && (info.MVTCombinationEnabled != info.MVTCombinationEnabledOriginal))
            {
                MVTVariantInfoProvider.EnableWebPartVariants(info.MVTCombinationPageTemplateID, info.MVTCombinationID, true);
            }

            lock (tableLock)
            {
                // Load the hashtable
                LoadTemplateCombinations();

                if (isNew)
                {
                    // Add new combination to the hashtable
                    TemplateCombinations.Add(GetTemplateCombinationCode(info), info);
                }
                else
                {
                    // Replace the edited combination in the hashtable
                    TemplateCombinations[GetTemplateCombinationCode(info)] = info;
                }
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(MVTCombinationInfo info)
        {
            base.DeleteInfo(info);

            lock (tableLock)
            {
                // Load the hashtable
                LoadTemplateCombinations();

                TemplateCombinations.Remove(GetTemplateCombinationCode(info));
            }
        }


        /// <summary>
        /// Deletes MVT combinations using the specified condition.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        protected virtual void DeleteMVTCombinationInfoInternal(string whereCondition)
        {
            if (whereCondition == null)
            {
                throw new ArgumentNullException("whereCondition");
            }

            BulkDelete(new WhereCondition(whereCondition), new BulkDeleteSettings { ObjectType = PredefinedObjectType.DOCUMENTMVTCOMBINATION });
            BulkDelete(new WhereCondition(whereCondition), new BulkDeleteSettings { ObjectType = PredefinedObjectType.MVTCOMBINATION });
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            lock (tableLock)
            {
                if (TemplateCombinations != null)
                {
                    TemplateCombinations.Invalidate(false);
                }
            }
        }


        /// <summary>
        /// Checks whether MVT combination is assigned to a specified multivariate test and the multivariate test is running.
        /// </summary>
        /// <param name="combinationName">Combination name</param>
        /// <param name="testName">Test name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture code</param>
        protected virtual bool IsMVTCombinationValidInternal(string combinationName, string testName, string siteName, string culture)
        {
            // Get MV Test info object
            MVTestInfo test = MVTestInfoProvider.GetMVTestInfo(testName, siteName);

            // Check whether object is defined
            if (test != null)
            {
                // Check whether MVTest is running                
                if (MVTestInfoProvider.MVTestIsRunning(test))
                {
                    // Check if MVT combination is defined
                    MVTCombinationInfo combination = GetMVTCombinationInfo(test.MVTestPage, combinationName, siteName, culture);
                    if ((combination != null))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns dataset of MVT combinations for the specified page template.
        /// </summary>
        /// <param name="pageTemplateId">Page template id</param>
        /// <param name="enabledOnly">Indicates whether only enabled should be loaded</param>
        protected virtual TypedDataSet GetCombinationsForTemplateInternal(int pageTemplateId, bool enabledOnly)
        {
            TypedDataSet ds = null;

            if (pageTemplateId > 0)
            {
                // Cache list of template combinations
                using (var cs = new CachedSection<TypedDataSet>(ref ds, 1440, enabledOnly, null, "OnlineMarketingTemplateCombinations", pageTemplateId))
                {
                    // Check whether data should be loaded
                    if (cs.LoadData)
                    {
                        // Set where condition
                        var whereCondition = new WhereCondition().WhereEquals("MVTCombinationPageTemplateID", pageTemplateId);
                        if (enabledOnly)
                        {
                            whereCondition.WhereEquals("MVTCombinationEnabled", 1);
                        }

                        // Get combinations data
                        ds = GetMVTCombinations().Where(whereCondition)
                                                 .TypedResult;

                        // Cache data if it is required
                        if (cs.Cached)
                        {
                            // Prepare cache dependency
                            cs.CacheDependency = CacheHelper.GetCacheDependency("om.mvtcombination|bytemplateid|" + pageTemplateId);
                        }

                        cs.Data = ds;
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Returns MVT combination with the specified parameters.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="culture">Culture code</param>
        /// <param name="combinationName">Combination name</param>
        /// <param name="siteName">Site name</param>
        protected virtual MVTCombinationInfo GetMVTCombinationInfoInternal(string aliasPath, string combinationName, string siteName, string culture)
        {
            PageInfo pi = PageInfoProvider.GetPageInfo(siteName, aliasPath, culture, null, SiteInfoProvider.CombineWithDefaultCulture(siteName));

            if ((pi != null) && (pi.UsedPageTemplateInfo != null))
            {
                // Get collection key
                string templateCombinationCode = GetTemplateCombinationCode(pi.UsedPageTemplateInfo.PageTemplateId, combinationName);

                // Combination object
                MVTCombinationInfo combinationObj;

                lock (tableLock)
                {
                    // Load the hashtable
                    ProviderObject.LoadTemplateCombinations();
                    combinationObj = TemplateCombinations[templateCombinationCode] as MVTCombinationInfo;
                }

                return combinationObj;
            }

            return null;
        }


        /// <summary>
        /// Returns the default MVT combination for the specified page template.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        protected virtual MVTCombinationInfo GetDefaultCombinationInfoInternal(int pageTemplateId)
        {
            MVTCombinationInfo defaultCombinationInfo = null;

            // Data set for combinations
            PageTemplateInfo pti = PageTemplateInfoProvider.GetPageTemplateInfo(pageTemplateId);
            if (pti != null)
            {
                DataSet ds = GetCombinationsForTemplate(pti.PageTemplateId, true);

                // Choose variant for user
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (ValidationHelper.GetBoolean(row["MVTCombinationIsDefault"], false))
                        {
                            defaultCombinationInfo = new MVTCombinationInfo(row);
                            break;
                        }
                    }
                }
            }

            return defaultCombinationInfo;
        }


        /// <summary>
        /// Gets the combinations which do not contain the specified webpart (instanceGuid).
        /// </summary>
        /// <param name="pageTemplateId">The template id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="documentId">The document id</param>
        protected virtual TypedDataSet GetCombinationsWithoutWebpartInternal(int pageTemplateId, Guid instanceGuid, int documentId)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@MVTCombinationPageTemplateID", pageTemplateId);
            parameters.Add("@MVTVariantInstanceGUID", instanceGuid);
            parameters.Add("@MVTVariantDocumentID", documentId);
            parameters.EnsureDataSet<MVTCombinationInfo>();

            return ConnectionHelper.ExecuteQuery("OM.MVTCombination.GetCombinationsWithoutWebpart", parameters).As<MVTCombinationInfo>();
        }


        /// <summary>
        /// Gets the combinations which do not contain the specified zone.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="documentId">The document id</param>
        protected virtual TypedDataSet GetCombinationsWithoutZoneInternal(int pageTemplateId, string zoneId, int documentId)
        {
            var parameters = new QueryDataParameters();
            parameters.Add("@MVTCombinationPageTemplateID", pageTemplateId);
            parameters.Add("@MVTVariantZoneID", zoneId);
            parameters.Add("@MVTVariantDocumentID", documentId);
            parameters.EnsureDataSet<MVTCombinationInfo>();

            return ConnectionHelper.ExecuteQuery("OM.MVTCombination.GetCombinationsWithoutZone", parameters).As<MVTCombinationInfo>();
        }


        /// <summary>
        /// Gets a new combination number.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        protected virtual int GetNewCombinationNumberInternal(int pageTemplateId, int documentId)
        {
            int max = 0;

            var whereCondition = new WhereCondition().WhereEquals("MVTCombinationPageTemplateID", pageTemplateId);

            if (documentId > 0)
            {
                whereCondition.Where(w => w.WhereNull("MVTCombinationDocumentID")
                                           .Or()
                                           .WhereEquals("MVTCombinationDocumentID", documentId));
            }

            DataSet ds = GetMVTCombinations().Column(new AggregatedColumn(AggregationType.Max, "CAST(REPLACE (MVTCombinationName, '" + COMBINATION_NAME_PREFIX + "', '') as int)").As("MaxNumber"))
                                             .Where(whereCondition);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                max = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["MaxNumber"], 0);
            }

            max++;
            return max;
        }


        /// <summary>
        /// Ensures that the multivariate test has a default combination created.
        /// </summary>
        /// <param name="testObj">The multivariate test obj</param>
        protected virtual void EnsureTestCombinationInternal(MVTestInfo testObj)
        {
            if (testObj != null)
            {
                foreach (int templateId in MVTestInfoProvider.GetTestTemplates(testObj))
                {
                    // Generate (if does not exist) a default combination for all selected page templates
                    EnsureTestCombination(templateId);
                }
            }
        }


        /// <summary>
        /// Ensures that the page template has a default combination created.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        protected virtual void EnsureTestCombinationInternal(int pageTemplateId)
        {
            if (pageTemplateId > 0)
            {
                int combinationsCount = GetMVTCombinations().WhereEquals("MVTCombinationPageTemplateID", pageTemplateId)
                                                            .Count;
                if (combinationsCount == 0)
                {
                    MVTCombinationInfo combinationInfo = new MVTCombinationInfo();
                    combinationInfo.MVTCombinationName = GetCombinationName(1);
                    combinationInfo.MVTCombinationCustomName = "1: " + ResHelper.GetString("mvtcombination.defaultpage");
                    combinationInfo.MVTCombinationPageTemplateID = pageTemplateId;
                    combinationInfo.MVTCombinationEnabled = true;
                    combinationInfo.MVTCombinationIsDefault = true;

                    SetMVTCombinationInfo(combinationInfo);
                }
            }
        }


        /// <summary>
        /// Disables all combinations where the variant is used.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="variantId">The variant id</param>
        /// <param name="enable">True = Enable; false = Disable</param>
        protected virtual void EnableCombinationsInternal(int pageTemplateId, int variantId, bool enable)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@MVTVariantID", variantId);
            parameters.Add("@enabled", enable);

            ConnectionHelper.ExecuteQuery("OM.MVTCombination.EnableCombinations", parameters);

            // Clear the mvtcombinations hash table due to possible changes in the database caused by the 'EnableCombinations()' method.
            ClearHashtables(true);
        }


        /// <summary>
        /// Returns the combination for the specified multivariate test object.
        /// If any combination for this multivariate test is stored in a browser cookie then this combination will be used,
        /// otherwise will be chosen a random combination for this test.
        /// </summary>
        /// <param name="testObj">The multivariate test object</param>
        /// <param name="culture">Required culture</param>
        /// <param name="viewMode">The view mode</param>
        protected virtual MVTCombinationInfo GetMVTestCombinationInternal(MVTestInfo testObj, string culture, ViewModeEnum viewMode)
        {
            if (testObj != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(testObj.MVTestSiteID);
                if (si != null)
                {
                    PageInfo pi = PageInfoProvider.GetPageInfo(si.SiteName, testObj.MVTestPage, culture, null, SiteInfoProvider.CombineWithDefaultCulture(si.SiteName));
                    return GetMVTestCombination(testObj, pi, viewMode);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns the combination for the specified multivariate test object.
        /// If any combination for this multivariate test is stored in a browser cookie then this combination will be used,
        /// otherwise will be chosen a random combination for this test.
        /// </summary>
        /// <param name="testObj">The multivariate test object</param>
        /// <param name="pi">The page info object</param>
        /// <param name="viewMode">The view mode</param>
        protected virtual MVTCombinationInfo GetMVTestCombinationInternal(MVTestInfo testObj, PageInfo pi, ViewModeEnum viewMode)
        {
            // Check whether page info is defined
            if (pi?.UsedPageTemplateInfo == null)
            {
                return null;
            }

            string cookiename;
            string combinationName;

            // Try get combination name for live site page
            if (testObj != null)
            {
                cookiename = CookieName.GetMVTCookieName(testObj.MVTestName);

                // Get combination from cookies
                combinationName = CookieHelper.GetValue(cookiename);
            }
            // Try get combination name for non-livesite view mode (edit/design...)
            else
            {
                // Get a template cookie name (used just in administration when no test is running)
                cookiename = CookieName.GetNoMVTCookieName(pi.GetUsedPageTemplateId());

                // Get combination from cookies
                combinationName = CookieHelper.GetValue(cookiename);
            }

            // Check whether combination is defined
            if (!String.IsNullOrEmpty(combinationName))
            {
                MVTCombinationInfo combination = GetMVTCombinationInfo(pi.NodeAliasPath, combinationName, SiteInfoProvider.GetSiteName(pi.NodeSiteID), pi.DocumentCulture);

                // Check whether variant is defined and is assigned to specified MVT test
                if (combination != null)
                {
                    // Return only enabled combination in live site, other view modes can return disabled combinations as well
                    if (((viewMode.IsLiveSite()) && combination.MVTCombinationEnabled)
                        || (viewMode != ViewModeEnum.LiveSite))
                    {
                        // Move cookies expiration to next 30 days
                        CookieHelper.SetValue(cookiename, combination.MVTCombinationName, null, DateTime.Now.AddDays(30), false);
                        return combination;
                    }
                }
            }

            // Data set for combinations
            DataSet ds = GetCombinationsForTemplate(pi.UsedPageTemplateInfo.PageTemplateId, true);

            // Choose variant for user
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                int randomCombination;

                // Generate random combination
                lock (randomGeneratorLock)
                {
                    randomCombination = RandomGenerator.Next(0, ds.Tables[0].Rows.Count);
                }

                // Get combination and set it to cookies
                MVTCombinationInfo combination = GetMVTCombinationInfo(Convert.ToInt32(ds.Tables[0].Rows[randomCombination]["MVTCombinationID"]));

                if (combination != null)
                {
                    CookieHelper.SetValue(cookiename, combination.MVTCombinationName, DateTime.Now.AddDays(30));
                    return combination;
                }
            }

            return null;
        }


        /// <summary>
        /// Load all combinations from DB to the dictionary.
        /// </summary>
        protected virtual void LoadTemplateCombinations()
        {
            if (ProviderHelper.LoadTables(TemplateCombinations))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(TemplateCombinations))
                    {
                        // Prepare the tables
                        var templateCombinationInfos = new ProviderInfoDictionary<string>(MVTCombinationInfo.OBJECT_TYPE, "MVTCombinationPageTemplateID;MVTCombinationName");

                        foreach (var combination in GetMVTCombinations())
                        {
                            templateCombinationInfos[GetTemplateCombinationCode(combination.MVTCombinationPageTemplateID, combination.MVTCombinationName)] = combination;
                        }

                        TemplateCombinations = templateCombinationInfos;
                    }
                }
            }
        }


        /// <summary>
        /// Gets the template combination code.
        /// </summary>
        /// <param name="combinationObj">The combination obj</param>
        protected virtual string GetTemplateCombinationCode(MVTCombinationInfo combinationObj)
        {
            return GetTemplateCombinationCode(combinationObj.MVTCombinationPageTemplateID, combinationObj.MVTCombinationName);
        }


        /// <summary>
        /// Gets the template combination site code.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="combinationName">Name of the MVT combination</param>
        protected virtual string GetTemplateCombinationCode(int pageTemplateId, string combinationName)
        {
            if (string.IsNullOrEmpty(combinationName))
            {
                combinationName = string.Empty;
            }

            return pageTemplateId + "_" + combinationName.ToLowerCSafe();
        }


        /// <summary>
        /// Returns MVT combination with the specified combination name and template id.
        /// </summary>
        /// <param name="combinationName">Code name of the combination.</param>
        /// <param name="pageTemplateId">The page template id.</param>
        protected virtual MVTCombinationInfo GetMVTCombinationInfoInternal(int pageTemplateId, string combinationName)
        {
            // Combination object
            MVTCombinationInfo combinationObj;

            lock (tableLock)
            {
                string templateCombinationCode = GetTemplateCombinationCode(pageTemplateId, combinationName);

                // Load the hashtable
                ProviderObject.LoadTemplateCombinations();

                // Get the variant
                combinationObj = TemplateCombinations[templateCombinationCode] as MVTCombinationInfo;
            }

            return combinationObj;
        }


        /// <summary>
        /// Combines the old custom name with the new variant.
        /// </summary>
        /// <param name="oldCombinationCustomName">The old combination custom name</param>
        /// <param name="newVariantName">The new variant name</param>
        /// <param name="newCombinationNumber">The new combination number</param>
        protected string GetNewCombinationCustomNameInternal(string oldCombinationCustomName, string newVariantName, int newCombinationNumber)
        {
            string baseCombinationCustomName = string.Empty;

            // Prepare the base custom name (i.e.: "3: RedText, YellowButton" returns "RedText, YellowButton,")
            if (!string.IsNullOrEmpty(oldCombinationCustomName))
            {
                string[] parts = oldCombinationCustomName.Split(new char[] { ':' }, 2);
                if (parts.Length == 1)
                {
                    // Old custom name is in a format "RedButton"
                    baseCombinationCustomName = oldCombinationCustomName.Trim() + ", ";
                }
                else if (parts.Length == 2)
                {
                    // Old custom name is in a format "1: RedButton"
                    if (ValidationHelper.IsInteger(parts[0].Trim()))
                    {
                        baseCombinationCustomName = parts[1].TrimStart(null) + ", ";
                    }
                    // Old custom name is in a format "ABC: RedButton"
                    else
                    {
                        baseCombinationCustomName = oldCombinationCustomName + ", ";
                    }
                }
            }

            // Generate the new combination custom name
            var newCombinationCustomName = newCombinationNumber + ": " + baseCombinationCustomName + newVariantName;

            // Shorten the custom name if exceeds the length limitation
            if (newCombinationCustomName.Length >= COMBINATION_CUSTOMNAME_MAX_LENGHT)
            {
                newCombinationCustomName = TextHelper.LimitLength(newCombinationCustomName, COMBINATION_CUSTOMNAME_MAX_LENGHT);
            }

            return newCombinationCustomName;
        }


        /// <summary>
        /// Gets the formated name of the combination (i.e.: number=2 returns "Combination_002").
        /// </summary>
        /// <param name="combinationNumber">The number used in the formated combination name</param>
        protected virtual string GetCombinationNameInternal(int combinationNumber)
        {
            return COMBINATION_NAME_PREFIX + String.Format("{0:000}", combinationNumber);
        }

        #endregion
    }
}