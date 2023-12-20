using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;


namespace CMS.OnlineMarketing
{
    using TypedDataSet = InfoDataSet<ABVariantInfo>;

    /// <summary>
    /// Class providing ABVariantInfo management.
    /// </summary>
    public class ABVariantInfoProvider : AbstractInfoProvider<ABVariantInfo, ABVariantInfoProvider>
    {
        #region "Hashtables"

        /// <summary>
        /// Variants indexed by ABVariantID.
        /// </summary>
        private static readonly CMSStatic<ProviderInfoDictionary<int>> mVariantsById = new CMSStatic<ProviderInfoDictionary<int>>();


        /// <summary>
        /// Variants indexed by key in format [ABVariantName]_[ABVariantSiteID]_[ABVariantTestID].
        /// </summary>
        private static readonly CMSStatic<ProviderInfoDictionary<string>> mVariantsByNameSiteTest = new CMSStatic<ProviderInfoDictionary<string>>();


        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static readonly object tableLock = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Variants indexed by ABVariantID.
        /// </summary>
        private static ProviderInfoDictionary<int> VariantsById
        {
            get
            {
                return mVariantsById;
            }
            set
            {
                mVariantsById.Value = value;
            }
        }


        /// <summary>
        /// Variants indexed by key in format [ABVariantName]_[ABVariantSiteID]_[ABVariantTestID].
        /// </summary>
        private static ProviderInfoDictionary<string> VariantsByNameSiteTest
        {
            get
            {
                return mVariantsByNameSiteTest;
            }
            set
            {
                mVariantsByNameSiteTest.Value = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns variants for AB test.
        /// </summary>
        /// <param name="abTest">AB test info object</param>
        public static TypedDataSet GetVariantsForTest(ABTestInfo abTest)
        {
            return ProviderObject.GetVariantsForTestInternal(abTest);
        }


        /// <summary>
        /// Checks whether AB variant is assigned to the specified AB test and AB test is running.
        /// </summary>
        /// <param name="abTestName">AB test name</param>
        /// <param name="abVariantName">AB variant name</param>
        /// <param name="siteName">Site name</param>
        /// <returns>Returns true if variant is valid</returns>
        public static bool IsABVariantValid(string abVariantName, string abTestName, string siteName)
        {
            // Get AB Test info object
            ABTestInfo abTest = ABTestInfoProvider.GetABTestInfo(abTestName, siteName);
            // Check whether object is defined
            if (abTest != null)
            {
                // Check whether AB test is running
                if (ABTestStatusEvaluator.ABTestIsRunning(abTest))
                {
                    // Check if AB variant is defined
                    ABVariantInfo variant = GetABVariantInfo(abVariantName, abTestName, siteName);
                    if ((variant != null))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns AB variant with specified ID.
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        public static ABVariantInfo GetABVariantInfo(int variantId)
        {
            return ProviderObject.GetABVariantInfoInternal(variantId);
        }


        /// <summary>
        /// Returns cached AB variant with specified name.
        /// </summary>
        /// <param name="variantName">Variant name</param>
        /// <param name="abTestName">AB test name</param>
        /// <param name="siteName">Site name</param>
        public static ABVariantInfo GetABVariantInfo(string variantName, string abTestName, string siteName)
        {
            return ProviderObject.GetABVariantInfoInternal(variantName, abTestName, siteName);
        }


        /// <summary>
        /// Returns a query for all the AB variants.
        /// </summary>
        public static ObjectQuery<ABVariantInfo> GetVariants()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified Variant.
        /// </summary>
        /// <param name="variantObj">Variant to be set</param>
        public static void SetABVariantInfo(ABVariantInfo variantObj)
        {
            ProviderObject.SetInfo(variantObj);
        }


        /// <summary>
        /// Deletes specified Variant.
        /// </summary>
        /// <param name="variantObj">Variant to be deleted</param>
        public static void DeleteABVariantInfo(ABVariantInfo variantObj)
        {
            ProviderObject.DeleteInfo(variantObj);
        }


        /// <summary>
        /// Deletes Variant with specified ID.
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        public static void DeleteABVariantInfo(int variantId)
        {
            ABVariantInfo variantObj = GetABVariantInfo(variantId);
            DeleteABVariantInfo(variantObj);
        }


        /// <summary>
        /// Returns list of variants which do not have translations into all cultures for multi-cultural test or specific translation for single culture test.
        /// Returns empty list if no variants were found.
        /// </summary>
        /// <exception cref="ArgumentNullException">abTest is null</exception>
        /// <param name="abTest">AB Test</param>
        public static IList<ABVariantInfo> GetVariantsWithMissingTranslations(ABTestInfo abTest)
        {
            if (abTest == null)
            {
                throw new ArgumentNullException("abTest");
            }

            var missingTranslationVariants = new List<ABVariantInfo>();

            // If site has not more than one culture, do not check
            var siteCultures = CultureSiteInfoProvider.GetSiteCultures(SiteContext.CurrentSiteName).Items.Count;
            if (siteCultures <= 1)
            {
                return missingTranslationVariants;
            }

            var variants = ABCachedObjects.GetVariants(abTest);

            // If test is not multi-cultural, check if specific translation of all variants exists
            if (!String.IsNullOrEmpty(abTest.ABTestCulture))
            {
                foreach (var variant in variants)
                {
                    if (new TreeProvider().SelectSingleNode(SiteContext.CurrentSiteName, variant.ABVariantPath, abTest.ABTestCulture, false) == null)
                    {
                        missingTranslationVariants.Add(variant);
                    }
                }
            }
            else
            {
                // Get variants where number of cultures is different from number of nodes
                foreach (var variant in variants)
                {
                    var nodeCultures = DocumentHelper.GetDocuments()
                                                       .PublishedVersion()
                                                       .All()
                                                       .OnCurrentSite()
                                                       .AllCultures()
                                                       .Column("DocumentCulture")
                                                       .WhereEquals("NodeAliasPath", variant.ABVariantPath)
                                                       .Count();
                    if (nodeCultures != siteCultures)
                    {
                        missingTranslationVariants.Add(variant);
                    }
                }
            }

            return missingTranslationVariants;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            lock (tableLock)
            {
                if (VariantsById != null)
                {
                    VariantsById.Invalidate(logTasks);
                }

                if (VariantsByNameSiteTest != null)
                {
                    VariantsByNameSiteTest.Invalidate(logTasks);
                }
            }
        }


        /// <summary>
        /// Returns Variant with specified ID.
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        protected virtual ABVariantInfo GetABVariantInfoInternal(int variantId)
        {
            if (variantId <= 0)
            {
                return null;
            }

            // Ensure the hashtables
            LoadVariants();

            ABVariantInfo result;
            BaseInfo existing;

            // Get Variant from hashtable
            if (VariantsById.TryGetValue(variantId, out existing))
            {
                result = (ABVariantInfo)existing;
            }
            else
            {
                // Get Variant from database
                result = GetInfoById(variantId);
                if (result != null)
                {
                    VariantsById[result.ABVariantID] = result;
                    VariantsByNameSiteTest[result.ABVariantName + "_" + result.ABVariantSiteID + "_" + result.ABVariantTestID] = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns cached variant with specified name.
        /// </summary>
        /// <param name="variantName">Variant name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="abTestName">AB test name</param>
        protected virtual ABVariantInfo GetABVariantInfoInternal(string variantName, string abTestName, string siteName)
        {
            if (string.IsNullOrEmpty(variantName))
            {
                return null;
            }

            // Ensure the hashtables
            LoadVariants();

            ABVariantInfo result;
            BaseInfo existing;

            // Get site ID
            int siteId = SiteInfoProvider.GetSiteID(siteName);
            int abTestID = ABTestInfoProvider.GetABTestID(abTestName, siteName);

            // Get Variant from hashtable
            if (VariantsByNameSiteTest.TryGetValue(variantName + "_" + siteId + "_" + abTestID, out existing))
            {
                result = (ABVariantInfo)existing;
            }
            else
            {
                // Get Variant from database
                result = GetABVariantInfoFromDBInternal(variantName, abTestName, siteName);
                if (result != null)
                {
                    // Update Variant in hashtables
                    VariantsById[result.ABVariantID] = result;
                    VariantsByNameSiteTest[result.ABVariantName + "_" + result.ABVariantSiteID + "_" + abTestID] = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Returns Variant with specified name from database.
        /// </summary>
        /// <param name="abTestName">ABTest name</param>
        /// <param name="variantName">Variant name</param>
        /// <param name="siteName">Site name</param>
        protected virtual ABVariantInfo GetABVariantInfoFromDBInternal(string variantName, string abTestName, string siteName)
        {
            if (string.IsNullOrEmpty(variantName))
            {
                return null;
            }

            // Get site WHERE condition
            string where = SiteInfoProvider.GetSiteWhereCondition(ABVariantInfo.TYPEINFO.CodeNameColumn, variantName, ABVariantInfo.TYPEINFO.SiteIDColumn, siteName);

            // Add condition to A/B test
            if (!String.IsNullOrEmpty(abTestName))
            {
                ABTestInfo ti = ABTestInfoProvider.GetABTestInfo(abTestName, siteName);
                if (ti != null)
                {
                    where = SqlHelper.AddWhereCondition(where, "ABVariantTestID = " + ti.ABTestID);
                }
            }

            // Get data from database
            return GetVariants().Where(new WhereCondition(where)).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ABVariantInfo info)
        {
            if (info == null)
            {
                throw new Exception("[ABVariantInfoProvider.SetABVariantInfo]: No ABVariantInfo object set.");
            }

            // Ensure the hashtables
            LoadVariants();

            RemoveInvalidCacheRecords(info);

            CheckUniqueDisplayName(info);

            HandleCodeNameChange(info);

            base.SetInfo(info);

            // Updates or adds AB variant into hashtables
            VariantsById.Update(info.ABVariantID, info);
            VariantsByNameSiteTest.Update(info.ABVariantName + "_" + info.ABVariantSiteID + "_" + info.ABVariantTestID, info);
        }


        /// <summary>
        /// Deletes the object from the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ABVariantInfo info)
        {
            if (info == null)
            {
                return;
            }

            // Ensure the hashtables
            LoadVariants();

            // Delete Variant
            base.DeleteInfo(info);

            // Deletes variants from hashtables
            VariantsById.Delete(info.ABVariantID);
            VariantsByNameSiteTest.Delete(info.ABVariantName + "_" + info.ABVariantSiteID + "_" + info.ABVariantTestID);
            ABVariantColorAssigner.RemoveVariant(info);
        }


        /// <summary>
        /// Get variants for test.
        /// </summary>
        protected virtual TypedDataSet GetVariantsForTestInternal(ABTestInfo abTest)
        {
            TypedDataSet variants = GetVariants()
                                   .WhereEquals("ABVariantTestID", abTest.ABTestID)
                                   .TypedResult;

            return variants;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads Variants into hashtables.
        /// </summary>
        private static void LoadVariants()
        {
            if (ProviderHelper.LoadTables(VariantsById, VariantsByNameSiteTest))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(VariantsById, VariantsByNameSiteTest))
                    {
                        // Prepare the tables
                        var infosById = new ProviderInfoDictionary<int>(ABVariantInfo.OBJECT_TYPE, "ABVariantID");
                        var infosByNameSiteTest = new ProviderInfoDictionary<string>(ABVariantInfo.OBJECT_TYPE, "ABVariantName;ABVariantSiteID;ABVariantTestID");

                        if (ProviderHelper.LoadHashTables(ABVariantInfo.OBJECT_TYPE, LoadHashtableEnum.All) != LoadHashtableEnum.None)
                        {
                            // Get the data
                            DataSet ds = GetVariants();

                            // Fill in the temporary tables
                            if (!DataHelper.DataSourceIsEmpty(ds))
                            {
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    ABVariantInfo variantObj = new ABVariantInfo(dr);

                                    infosById[variantObj.ABVariantID] = variantObj;
                                    infosByNameSiteTest[variantObj.ABVariantName + "_" + variantObj.ABVariantSiteID + "_" + variantObj.ABVariantTestID] = variantObj;
                                }
                            }
                        }

                        // Initialize original tables
                        VariantsById = infosById;
                        VariantsByNameSiteTest = infosByNameSiteTest;
                    }
                }
            }
        }


        /// <summary>
        /// Removes invalid cache records.
        /// This happens when code name changes.
        /// </summary>
        /// <param name="variantObj">AB Variant</param>
        private void RemoveInvalidCacheRecords(ABVariantInfo variantObj)
        {
            // Remove variant from hashtables when code name changes
            var originalName = ValidationHelper.GetString(variantObj.GetOriginalValue("ABVariantName"), "");
            if (!string.IsNullOrEmpty(originalName) && (originalName != variantObj.ABVariantName))
            {
                VariantsByNameSiteTest.Remove(originalName + "_" + variantObj.ABVariantSiteID + "_" + variantObj.ABVariantTestID);
            }
        }


        /// <summary>
        /// Checks whether the object has unique display name.
        /// </summary>
        /// <param name="variant">AB variant</param>
        /// <exception cref="InfoObjectException">Display name is not unique</exception>
        private void CheckUniqueDisplayName(ABVariantInfo variant)
        {
            // Check for another variants with the same display name
            var nameNotUnique = ABCachedObjects.GetVariants(ABTestInfoProvider.GetABTestInfo(variant.ABVariantTestID))
                                               .Any(v => (v.ABVariantID != variant.ABVariantID) &&
                                                         (v.ABVariantDisplayName.EqualsCSafe(variant.ABVariantDisplayName, true)));


            if (nameNotUnique)
            {
                string niceObjectType = TypeHelper.GetNiceObjectTypeName(variant.TypeInfo.ObjectType);
                string message = String.Format(
                    CoreServices.Localization.GetString("general.displaynamenotunique"),
                    niceObjectType.ToLowerCSafe(),
                    HTMLHelper.HTMLEncode(variant.ABVariantDisplayName)
                );
                throw new InfoObjectException(variant, message);
            }
        }


        /// <summary>
        /// Renames AB variant statistics data when code name changes.
        /// </summary>
        /// <param name="oldName">Old code name</param>
        /// <param name="newName">New code name</param>
        /// <param name="siteID">Test site ID </param>
        /// <param name="abTestName">ABTest name</param>
        private void RenameABVariantStatistics(string oldName, string newName, int siteID, string abTestName)
        {
            // Variant code name change
            if (newName.ToLowerCSafe() != oldName.ToLowerCSafe())
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@OldName", oldName);
                parameters.Add("@NewName", newName);
                parameters.Add("@SiteID", siteID);
                parameters.Add("@ABTestName", abTestName);

                ConnectionHelper.ExecuteQuery("OM.ABVariant.RenameABVariantStatistics", parameters);
            }
        }


        /// <summary>
        /// Checks for code name change and renames statistics if so.
        /// </summary>
        /// <param name="variantObj">AB Variant</param>
        private void HandleCodeNameChange(ABVariantInfo variantObj)
        {
            // Check whether variant name changed and if so, rename statistics
            if (variantObj.ChangedColumns().Contains("ABVariantName"))
            {
                string originalCodeName = ValidationHelper.GetString(variantObj.GetOriginalValue("ABVariantName"), "");
                ABTestInfo abTest = ABTestInfoProvider.GetABTestInfo(variantObj.ABVariantTestID);

                if (!String.IsNullOrEmpty(originalCodeName) && (abTest != null))
                {
                    RenameABVariantStatistics(originalCodeName, variantObj.ABVariantName, SiteContext.CurrentSiteID, abTest.ABTestName);
                }
            }
        }

        #endregion
    }
}