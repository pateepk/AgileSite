using System;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.OnlineMarketing.Internal;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class providing ABTestInfo management.
    /// </summary>
    public class ABTestInfoProvider : AbstractInfoProvider<ABTestInfo, ABTestInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ABTestInfoProvider()
            : base(ABTestInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns true if AB testing is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool ABTestingEnabled(string siteName)
        {
            return ProviderObject.ABTestingEnabledInternal(siteName);
        }


        /// <summary>
        /// Determines whether the specified AB test contains variants.
        /// </summary>
        /// <param name="abTest">The AB test</param>
        public static bool ContainsVariants(ABTestInfo abTest)
        {
            return ABCachedObjects.GetVariants(abTest).Any();
        }


        /// <summary>
        /// Returns AB test with specified ID.
        /// </summary>
        /// <param name="abTestId">AB test ID</param>
        public static ABTestInfo GetABTestInfo(int abTestId)
        {
            return ProviderObject.GetInfoById(abTestId);
        }


        /// <summary>
        /// Returns AB test with specified name.
        /// </summary>
        /// <param name="abTestName">AB test name</param>
        /// <param name="siteName">Site name</param>
        public static ABTestInfo GetABTestInfo(string abTestName, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(abTestName, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Returns query of all AB tests.
        /// </summary>
        public static ObjectQuery<ABTestInfo> GetABTests()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified AB test.
        /// </summary>
        /// <param name="abTestObj">AB test to be set</param>
        public static void SetABTestInfo(ABTestInfo abTestObj)
        {
            ProviderObject.SetInfo(abTestObj);
        }


        /// <summary>
        /// Deletes specified AB test.
        /// </summary>
        /// <param name="abTestObj">AB test to be deleted</param>
        public static void DeleteABTestInfo(ABTestInfo abTestObj)
        {
            ProviderObject.DeleteInfo(abTestObj);
        }


        /// <summary>
        /// Deletes AB test with specified ID.
        /// </summary>
        /// <param name="abTestId">AB test ID</param>
        public static void DeleteABTestInfo(int abTestId)
        {
            ABTestInfo abTestObj = GetABTestInfo(abTestId);
            DeleteABTestInfo(abTestObj);
        }


        /// <summary>
        /// Returns ID of the specified test.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="abTestName">Test name</param>
        public static int GetABTestID(string abTestName, string siteName)
        {
            return ProviderObject.GetABTestIDInternal(abTestName, siteName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ABTestInfo info)
        {
            var runMaterialization = info.ChangedColumns().Contains("ABTestOpenFrom") && info.ABTestID > 0;

            HandleCodeNameChange(info);

            base.SetInfo(info);

            if (runMaterialization)
            {
                HandleABVariantDataMaterialization(info);
            }

            CacheHelper.TouchKey("nodeid" + CacheHelper.SEPARATOR + TreePathUtils.GetNodeIdByAliasPath(SiteInfoProvider.GetSiteName(info.ABTestSiteID), info.ABTestOriginalPage));
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ABTestInfo info)
        {
            base.DeleteInfo(info);
            ABVariantColorAssigner.RemoveTest(info);
        }


        /// <summary>
        /// Returns true if AB testing is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual bool ABTestingEnabledInternal(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSABTestingEnabled");
        }


        /// <summary>
        /// Returns ID of the specified test.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="abTestName">Test name</param>
        protected virtual int GetABTestIDInternal(string abTestName, string siteName)
        {
            // Get site ID
            ABTestInfo abti = GetABTestInfo(abTestName, siteName);
            if (abti != null)
            {
                return abti.ABTestID;
            }

            return 0;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Materializes variants of <paramref name="abTestInfo"/> in case A/B test is on content only page, it's OpenFrom column has been changed and it's variants are not materialized.
        /// </summary>
        /// <param name="abTestInfo">A/B test whose variant to materialize.</param>
        private void HandleABVariantDataMaterialization(ABTestInfo abTestInfo)
        {
            if (SiteInfoProvider.GetSiteInfo(abTestInfo.ABTestSiteID).SiteIsContentOnly
                && (!ABTestHelper.HasABTestVariantsMaterialized(abTestInfo)))
            {
                var abTestDocument = ABTestHelper.GetABTestPage(abTestInfo);
                if (abTestDocument == null)
                {
                    return;
                }

                var variants = Service.Resolve<IABTestManager>().GetVariants(abTestDocument).ToList();
                if (!variants.Any())
                {
                    return;
                }

                ABVariantDataInfoProvider.MaterializeVariants(abTestInfo, variants);
            }
        }


        /// <summary>
        /// Renames AB test statistics data when code name changes.
        /// </summary>
        /// <param name="oldName">Old code name</param>
        /// <param name="newName">New code name</param>
        /// <param name="siteID">Test site ID </param>
        private void RenameStatistics(string oldName, string newName, int siteID)
        {
            if (!String.Equals(newName, oldName, StringComparison.InvariantCultureIgnoreCase))
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@OldName", oldName);
                parameters.Add("@NewName", newName);
                parameters.Add("@SiteID", siteID);

                ConnectionHelper.ExecuteQuery("OM.ABTest.RenameABTestStatistics", parameters);
            }
        }


        /// <summary>
        /// Checks for code name change and renames statistics if so.
        /// </summary>
        /// <param name="abTestObj">AB Test</param>
        private void HandleCodeNameChange(ABTestInfo abTestObj)
        {
            if (abTestObj.ChangedColumns().Contains("ABTestName"))
            {
                string originalCodeName = ValidationHelper.GetString(abTestObj.GetOriginalValue("ABTestName"), "");

                if (!String.IsNullOrEmpty(originalCodeName))
                {
                    RenameStatistics(originalCodeName, abTestObj.ABTestName, SiteContext.CurrentSiteID);
                }
            }
        }

        #endregion
    }
}