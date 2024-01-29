using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing PersonalizationInfo management.
    /// </summary>
    public class PersonalizationInfoProvider : AbstractInfoProvider<PersonalizationInfo, PersonalizationInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Undefined dashboard constant.
        /// </summary>
        public const string UNDEFINEDDASHBOARD = "undefineddashboardname";

        #endregion


        #region "Variables"

        /// <summary>
        /// User personalization indexed by document ID [userId + "_" + documentId] -> [PersonalizationInfo]
        /// </summary>
        private static readonly CMSStatic<ProviderInfoDictionary<string>> mUserPersonalizations = new CMSStatic<ProviderInfoDictionary<string>>();


        /// <summary>
        /// User personalization indexed by dashboard name [userId + "_" + dashboardName + "_" + siteId] -> [PersonalizationInfo]
        /// </summary>
        private static readonly CMSStatic<ProviderInfoDictionary<string>> mUserPersonalizationByName = new CMSStatic<ProviderInfoDictionary<string>>();

        #endregion


        #region "Properties"

        /// <summary>
        /// User personalization indexed by document ID [userId + "_" + documentId] -> [PersonalizationInfo]
        /// </summary>
        private static ProviderInfoDictionary<string> UserPersonalizations
        {
            get
            {
                return mUserPersonalizations;
            }
            set
            {
                mUserPersonalizations.Value = value;
            }
        }


        /// <summary>
        /// User personalization indexed by dashboard name [userId + "_" + dashboardName + "_" + siteId] -> [PersonalizationInfo]
        /// </summary>
        private static ProviderInfoDictionary<string> UserPersonalizationByName
        {
            get
            {
                return mUserPersonalizationByName;
            }
            set
            {
                mUserPersonalizationByName.Value = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns all Personalization records.
        /// </summary>
        public static DataSet GetPersonalizationInfos()
        {
            return GetPersonalizationInfos(null, null, 0, null);
        }


        /// <summary>
        /// Returns the personalization records based on given parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Get only top N items</param>
        /// <param name="columns">Columns to get</param>
        public static DataSet GetPersonalizationInfos(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("cms.personalization.selectall", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns the PersonalizationInfo structure for the specified personalization.
        /// </summary>
        /// <param name="personalizationId">Personalization id</param>
        public static PersonalizationInfo GetPersonalizationInfo(int personalizationId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ID", personalizationId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("cms.personalization.select", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new PersonalizationInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns PersonalizationInfo according to user ID and Dashboard name.
        /// </summary>
        /// <param name="userId">ID of the user to be selected</param>
        /// <param name="dashboardName">Name of the specified dashboard</param>
        /// <param name="siteName">Site name</param>
        public static PersonalizationInfo GetDashBoardPersonalization(int userId, string dashboardName, string siteName)
        {
            // Fill hashtables
            LoadPersonalizationHashtables();

            int siteId = 0;
            string siteIdWhere = "IS NULL";
            if (!String.IsNullOrEmpty(siteName))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    siteId = si.SiteID;
                    siteIdWhere = " = " + siteId;
                }
            }

            // Try to get from the hashtable
            string key = userId + "_" + dashboardName + "_" + siteId;

            BaseInfo result = UserPersonalizationByName[key];
            if (result == null)
            {
                // Get the data
                DataSet ds = GetPersonalizationInfos("PersonalizationUserID = " + userId + " AND PersonalizationSiteID " + siteIdWhere + " AND PersonalizationDashboardName = N'" + SqlHelper.EscapeQuotes(dashboardName) + "'", null, 1, null);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    result = new PersonalizationInfo(ds.Tables[0].Rows[0]);
                }
                else
                {
                    result = InfoHelper.EmptyInfo;
                }

                // Store the result
                UserPersonalizationByName[key] = result;
            }

            // Return the result
            return result as PersonalizationInfo;
        }


        /// <summary>
        /// Returns PersonalizationInfo according to user ID and document ID.
        /// </summary>
        /// <param name="userId">ID of the user to be selected</param>
        /// <param name="documentId">ID of the specified document</param>
        public static PersonalizationInfo GetUserPersonalization(int userId, int documentId)
        {
            // Fill hashtables
            LoadPersonalizationHashtables();

            // Try to get from the hashtable
            string key = userId + "_" + documentId;

            BaseInfo result = UserPersonalizations[key];
            if (result == null)
            {
                // Get the data
                DataSet ds = GetPersonalizationInfos("PersonalizationUserID = " + userId + " AND PersonalizationDocumentID = " + documentId, null, 1, null);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    result = new PersonalizationInfo(ds.Tables[0].Rows[0]);
                }
                else
                {
                    result = InfoHelper.EmptyInfo;
                }

                // Store the result
                UserPersonalizations[key] = result;
            }

            // Return the result
            return result as PersonalizationInfo;
        }


        /// <summary>
        /// Sets (updates or inserts) specified personalization.
        /// </summary>
        /// <param name="personalization">Personalization to set</param>
        public static void SetPersonalizationInfo(PersonalizationInfo personalization)
        {
            if (personalization != null)
            {
                // Fill hashtables
                LoadPersonalizationHashtables();

                // Check whether pesonalziation is se to document or dashboard
                if ((personalization.PersonalizationDocumentID < 1) && (String.IsNullOrEmpty(personalization.PersonalizationDashboardName) || (CMSString.Compare(UNDEFINEDDASHBOARD, personalization.PersonalizationDashboardName, true) == 0)))
                {
                    throw new Exception("[PersonalizationInfoProvider.SetPersonalizationInfoProvider] Personalization must be bounded to the some page or dasboard.");
                }

                // Save the web parts
                personalization.WebParts = personalization.TemplateInstance.GetZonesXML();

                if (personalization.PersonalizationID > 0)
                {
                    personalization.Generalized.UpdateData();
                }
                else
                {
                    personalization.Generalized.InsertData();
                }

                // Update hashtable
                UserPersonalizations.Update(personalization.PersonalizationUserID + "_" + personalization.PersonalizationDocumentID, personalization);
                UserPersonalizationByName.Update(personalization.PersonalizationUserID + "_" + personalization.PersonalizationDashboardName + "_" + personalization.PersonalizationSiteID, personalization);
            }
            else
            {
                throw new Exception("[PersonalizationInfoProvider.SetPersonalizationInfo]: No PersonalizationInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified personalization.
        /// </summary>
        /// <param name="infoObj">Personalization object</param>
        public static void DeletePersonalizationInfo(PersonalizationInfo infoObj)
        {
            if (infoObj != null)
            {
                // Create temporary strings
                int userId = infoObj.PersonalizationUserID;
                int documentId = infoObj.PersonalizationDocumentID;
                string dashboardName = infoObj.PersonalizationDashboardName;

                // Delete record from DB
                infoObj.Generalized.DeleteData();

                // Delete record from hashtables
                if (UserPersonalizations != null)
                {
                    UserPersonalizations.Delete(userId + "_" + documentId);
                }
                if (UserPersonalizationByName != null)
                {
                    UserPersonalizationByName.Delete(userId + "_" + dashboardName + "_" + infoObj.PersonalizationSiteID);
                }
            }
        }


        /// <summary>
        /// Deletes specified personalization.
        /// </summary>
        /// <param name="personalizationId">Personalization id</param>
        public static void DeletePersonalizationInfo(int personalizationId)
        {
            PersonalizationInfo infoObj = GetPersonalizationInfo(personalizationId);
            DeletePersonalizationInfo(infoObj);
        }


        /// <summary>
        /// Gets the personalized user instance for given page info.
        /// </summary>
        /// <param name="pageInfo">Page info</param>
        /// <param name="userId">User ID</param>
        /// <param name="dashboardName">Dashboard name</param>
        /// <param name="siteName">Site name - is used for dashboard personalization only</param>
        public static PageTemplateInstance GetPersonalizedTemplateInstance(PageInfo pageInfo, int userId, string dashboardName = null, string siteName = null)
        {
            PageTemplateInstance result = null;

            // Try to get data from cache
            using (var cs = new CachedSection<PageTemplateInstance>(ref result, 1440, true, null, "userpersonalization", userId, pageInfo.DocumentID, dashboardName, siteName))
            {
                if (cs.LoadData)
                {
                    PersonalizationInfo userPersonalization;
                    WidgetZoneTypeEnum zoneType = WidgetZoneTypeEnum.User;

                    // Load user or dashboard personalization
                    if (String.IsNullOrEmpty(dashboardName))
                    {
                        userPersonalization = GetUserPersonalization(userId, pageInfo.DocumentID);
                    }
                    else
                    {
                        userPersonalization = GetDashBoardPersonalization(userId, dashboardName, siteName);
                        zoneType = WidgetZoneTypeEnum.Dashboard;
                    }

                    // Get the data
                    if (userPersonalization != null)
                    {
                        // Combine with the user personalization
                        result = pageInfo.TemplateInstance;
                        if (result != null)
                        {
                            result = result.Clone();
                            result.CombineWith(userPersonalization.TemplateInstance, zoneType);

                            // Prepare the cache dependencies (depending on template data / personalization data / document data)
                            string[] dependencies = {
                                "",
                                userPersonalization.TypeInfo.ObjectType + "|byid|" + userPersonalization.PersonalizationID,
                                "nodeid|" + pageInfo.NodeID
                            };

                            if (pageInfo.UsedPageTemplateInfo != null)
                            {
                                dependencies[0] = "cms.pagetemplate|byid|" + pageInfo.UsedPageTemplateInfo.PageTemplateId;
                            }

                            // Add to the cache
                            cs.CacheDependency = CacheHelper.GetCacheDependency(dependencies);
                        }
                    }
                    else
                    {
                        // Do not cache
                        cs.CacheMinutes = 0;
                    }

                    cs.Data = result;
                }
            }

            return result;
        }


        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static readonly object tableLock = new object();


        /// <summary>
        /// Load personalization to hashtables.
        /// </summary>
        private static void LoadPersonalizationHashtables()
        {
            if (ProviderHelper.LoadTables(UserPersonalizations, UserPersonalizationByName))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(UserPersonalizations, UserPersonalizationByName))
                    {
                        // Prepare the tables
                        ProviderInfoDictionary<string> personalizationTable = new ProviderInfoDictionary<string>(PersonalizationInfo.OBJECT_TYPE, "PersonalizationUserID;PersonalizationDocumentID");
                        ProviderInfoDictionary<string> personalizationTableByName = new ProviderInfoDictionary<string>(PersonalizationInfo.OBJECT_TYPE, "PersonalizationUserID;PersonalizationDashboardName;PersonalizationSiteID");

                        if (ProviderHelper.LoadHashTables(PersonalizationInfo.OBJECT_TYPE, LoadHashtableEnum.None) != LoadHashtableEnum.None)
                        {
                            // Get the result
                            DataSet ds = GetPersonalizationInfos();

                            if (!DataHelper.DataSourceIsEmpty(ds))
                            {
                                var rows = ds.Tables[0].Rows;

                                foreach (DataRow row in rows)
                                {
                                    // Create new personalization info
                                    var pi = new PersonalizationInfo(row);

                                    personalizationTable[pi.PersonalizationUserID + "_" + pi.PersonalizationDocumentID] = pi;
                                    personalizationTableByName[pi.PersonalizationUserID + "_" + pi.PersonalizationDashboardName + "_" + pi.PersonalizationSiteID] = pi;
                                }
                            }
                        }

                        UserPersonalizations = personalizationTable;
                        UserPersonalizationByName = personalizationTableByName;
                    }
                }
            }
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            lock (tableLock)
            {
                if (UserPersonalizations != null)
                {
                    UserPersonalizations.Invalidate(logTasks);
                }

                if (UserPersonalizationByName != null)
                {
                    UserPersonalizationByName.Invalidate(logTasks);
                }
            }
        }

        #endregion
    }
}