using System;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing ConversionInfo management.
    /// </summary>
    public class ConversionInfoProvider : AbstractInfoProvider<ConversionInfo, ConversionInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ConversionInfo objects.
        /// </summary>
        public static ObjectQuery<ConversionInfo> GetConversions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns conversion with specified ID.
        /// </summary>
        /// <param name="conversionId">Conversion ID</param>        
        public static ConversionInfo GetConversionInfo(int conversionId)
        {
            return ProviderObject.GetInfoById(conversionId);
        }


        /// <summary>
        /// Returns conversion with specified name.
        /// </summary>
        /// <param name="conversionName">Conversion name</param>        
        /// <param name="siteName">Site name</param>
        public static ConversionInfo GetConversionInfo(string conversionName, string siteName)
        {
            return ProviderObject.GetConversionInfoInternal(conversionName, siteName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified conversion.
        /// </summary>
        /// <param name="conversionObj">Conversion to be set</param>
        public static void SetConversionInfo(ConversionInfo conversionObj)
        {
            ProviderObject.SetInfo(conversionObj);
        }


        /// <summary>
        /// Deletes specified conversion.
        /// </summary>
        /// <param name="conversionObj">Conversion to be deleted</param>
        public static void DeleteConversionInfo(ConversionInfo conversionObj)
        {
            ProviderObject.DeleteInfo(conversionObj);
        }


        /// <summary>
        /// Deletes conversion with specified ID.
        /// </summary>
        /// <param name="conversionId">Conversion ID</param>
        public static void DeleteConversionInfo(int conversionId)
        {
            ConversionInfo conversionObj = GetConversionInfo(conversionId);
            DeleteConversionInfo(conversionObj);
        }


        /// <summary>
        /// Renames conversion statistics data when changed code name
        /// </summary>
        /// <param name="oldName">Old code name</param>
        /// <param name="newName">New code name</param>
        /// <param name="siteID">Conversion site ID </param>
        public static void RenameConversionStatistics(string oldName, string newName, int siteID)
        {
            if ((newName != oldName))
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@OldName", oldName);
                parameters.Add("@NewName", newName);
                parameters.Add("@SiteID", siteID);

                ConnectionHelper.ExecuteQuery("Analytics.Conversion.RenameConversionStatistics", parameters);
            }
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns conversion with specified name.
        /// </summary>
        /// <param name="conversionName">Conversion name</param>  
        /// <param name="siteName">Site name</param>
        protected virtual ConversionInfo GetConversionInfoInternal(string conversionName, string siteName)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                return GetInfoByCodeName(conversionName, si.SiteID, true);
            }
            return null;
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ConversionInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            RemoveAnalyticsData(info);
            RemoveConversionFromDocuments(info);

            base.DeleteInfo(info);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Removes conversion from document nodes.
        /// </summary>
        /// <param name="conversionObj">Conversion object</param>  
        private void RemoveConversionFromDocuments(ConversionInfo conversionObj)
        {
            var treeNodes = DocumentHelper.GetDocuments()
                                          .PublishedVersion()
                                          .AllCultures()
                                          .OnSite(conversionObj.ConversionSiteID)
                                          .WhereEquals("DocumentTrackConversionName", conversionObj.ConversionName);

            foreach (var treeNode in treeNodes)
            {
                treeNode.DocumentTrackConversionName = null;
                treeNode.DocumentConversionValue = null;
                treeNode.SubmitChanges(false);
            }
        }


        /// <summary>
        /// Removes analytics data.
        /// </summary>
        /// <param name="conversionObj">Conversion object</param>  
        private void RemoveAnalyticsData(ConversionInfo conversionObj)
        {
            var where = new WhereCondition()
                .WhereEquals("StatisticsObjectName", conversionObj.ConversionName)
                .Where(w => w
                    .WhereEquals("StatisticsCode", "conversion")
                    .Or()
                    .WhereStartsWith("StatisticsCode", "abconversion;")
                    .Or()
                    .WhereStartsWith("StatisticsCode", "mvtconversion;"));

            DateTime fromDate = DateTimeHelper.ZERO_TIME;
            DateTime toDate = DateTimeHelper.ZERO_TIME;

            // Remove data for deleted conversion
            StatisticsInfoProvider.RemoveAnalyticsData(fromDate, toDate, conversionObj.ConversionSiteID, where.ToString(expand: true));
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor using ID and codename Hashtables.
        /// </summary>
        public ConversionInfoProvider()
            : base(ConversionInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion
    }
}