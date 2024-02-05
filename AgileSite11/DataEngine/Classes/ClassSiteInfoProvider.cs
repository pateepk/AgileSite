using System;
using System.Linq;

namespace CMS.DataEngine
{
    using TypedDataSet = InfoDataSet<ClassSiteInfo>;

    /// <summary>
    /// Class providing ClassSiteInfo management.
    /// </summary>
    public class ClassSiteInfoProvider : AbstractInfoProvider<ClassSiteInfo, ClassSiteInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all bindings between classes and sites.
        /// </summary>
        public static ObjectQuery<ClassSiteInfo> GetClassSites()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the bindings between classes and sites.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>        
        /// <param name="columns">Data columns to return</param>
        [Obsolete("Use method GetClassSites() instead.")]
        public static TypedDataSet GetClassSites(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return ProviderObject.GetClassSitesInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns the ClassSiteInfo structure for the specified classSite.
        /// </summary>
        /// <param name="classId">ClassID</param>
        /// <param name="siteId">SiteID</param>        
        public static ClassSiteInfo GetClassSiteInfo(int classId, int siteId)
        {
            return ProviderObject.GetClassSiteInfoInternal(classId, siteId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified classSite.
        /// </summary>
        /// <param name="classSite">ClassSite to set</param>
        public static void SetClassSiteInfo(ClassSiteInfo classSite)
        {
            ProviderObject.SetInfo(classSite);
        }


        /// <summary>
        /// Deletes specified classSite.
        /// </summary>
        /// <param name="infoObj">ClassSite object</param>
        public static void DeleteClassSiteInfo(ClassSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes site bindings of the specified class.
        /// </summary>
        /// <param name="classId">ID of the class.</param>
        public static void DeleteClassSiteInfos(int classId)
        {
            ProviderObject.DeleteClassSiteInfosInternal(classId);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Deletes specified classSite.
        /// </summary>
        /// <param name="classId">ClassID</param>
        /// <param name="siteId">SiteID</param>
        public static void RemoveClassFromSite(int classId, int siteId)
        {
            ClassSiteInfo infoObj = GetClassSiteInfo(classId, siteId);
            DeleteClassSiteInfo(infoObj);
        }


        /// <summary>
        /// Adds the class to the specified site.
        /// </summary>
        /// <param name="classId">ClassID</param>
        /// <param name="siteId">SiteID</param>
        public static void AddClassToSite(int classId, int siteId)
        {
            // Create new binding
            ClassSiteInfo infoObj = new ClassSiteInfo();
            infoObj.SiteID = siteId;
            infoObj.ClassID = classId;

            // Save to the database
            SetClassSiteInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns the bindings between classes and sites.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>        
        /// <param name="columns">Data columns to return</param>
        [Obsolete("Use method GetClassSites() instead.")]
        protected virtual TypedDataSet GetClassSitesInternal(string where, string orderBy, int topN, string columns)
        {
            // Get the data
            return GetClassSites().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns the ClassSiteInfo structure for the specified classSite.
        /// </summary>
        /// <param name="classId">ClassID</param>
        /// <param name="siteId">SiteID</param>
        protected virtual ClassSiteInfo GetClassSiteInfoInternal(int classId, int siteId)
        {
            var condition = new WhereCondition()
                .WhereEquals("SiteID", siteId)
                .WhereEquals("ClassID", classId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Deletes site bindings of the specified class.
        /// </summary>
        /// <param name="classId">ID of the class.</param>
        protected virtual void DeleteClassSiteInfosInternal(int classId)
        {
            ProviderObject.GetDeleteQuery().WhereEquals("ClassID", classId).Execute();
        }

        #endregion
    }
}