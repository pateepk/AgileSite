using System.Data;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class providing PageTemplateScopeInfo management.
    /// </summary>
    public class PageTemplateScopeInfoProvider : AbstractInfoProvider<PageTemplateScopeInfo, PageTemplateScopeInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the PageTemplateScopeInfo structure for the specified pageTemplateScope.
        /// </summary>
        /// <param name="pageTemplateScopeId">PageTemplateScope id</param>
        public static PageTemplateScopeInfo GetPageTemplateScopeInfo(int pageTemplateScopeId)
        {
            return ProviderObject.GetPageTemplateScopeInfoInternal(pageTemplateScopeId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified pageTemplateScope.
        /// </summary>
        /// <param name="pageTemplateScope">PageTemplateScope to set</param>
        public static void SetPageTemplateScopeInfo(PageTemplateScopeInfo pageTemplateScope)
        {
            ProviderObject.SetInfo(pageTemplateScope);
        }


        /// <summary>
        /// Deletes specified pageTemplateScope.
        /// </summary>
        /// <param name="infoObj">PageTemplateScope object</param>
        public static void DeletePageTemplateScopeInfo(PageTemplateScopeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified pageTemplateScope.
        /// </summary>
        /// <param name="pageTemplateScopeId">PageTemplateScope id</param>
        public static void DeletePageTemplateScopeInfo(int pageTemplateScopeId)
        {
            PageTemplateScopeInfo infoObj = GetPageTemplateScopeInfo(pageTemplateScopeId);
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns all page template scopes.
        /// </summary>
        public static ObjectQuery<PageTemplateScopeInfo> GetTemplateScopes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all templates matching scope criteria.
        /// </summary>
        /// <param name="path">Page template path</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="classNames">Class names</param>
        /// <param name="level">Level</param>
        /// <param name="siteName">Site name</param>
        public static InfoDataSet<PageTemplateInfo> GetScopeTemplates(string path, string cultureCode, string classNames, int level, string siteName)
        {
            string where = GetScopeWhereCondition(path, cultureCode, classNames, level, siteName, "PageTemplateID");

            return PageTemplateInfoProvider.GetTemplates()
                .Where(where)
                .TypedResult;
        }


        /// <summary>
        /// Returns where condition for template scope.
        /// </summary>
        /// <param name="path">Page template path</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="classNames">Class names separated by semicolon</param>
        /// <param name="level">Scope level</param>
        /// <param name="templateIdColumnName">ID column name</param>
        /// <param name="siteName">Site name</param>
        public static string GetScopeWhereCondition(string path, string cultureCode, string classNames, int level, string siteName, string templateIdColumnName)
        {
            // Resolve class names
            DataSet dsClasses = null;
            string classesWhere = string.Empty;
            if (!string.IsNullOrEmpty(classNames))
            {
                // Create where condition
                string[] classes = classNames.Split(';');
                if (classes.Length > 1)
                {
                    // Get classes
                    dsClasses = DataClassInfoProvider.GetClasses().WhereIn("ClassName", classes).Column("ClassID");
                }
                // Only 1 class
                else if (classes.Length == 1)
                {
                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(classes[0]);
                    if (dci != null)
                    {
                        classesWhere = " OR PageTemplateScopeClassID = " + dci.ClassID;
                    }
                }
            }

            // Different number of classes than 1
            if (string.IsNullOrEmpty(classesWhere))
            {
                // Create class where condition
                if (!DataHelper.DataSourceIsEmpty(dsClasses))
                {
                    foreach (DataRow dr in dsClasses.Tables[0].Rows)
                    {
                        int classID = ValidationHelper.GetInteger(dr["ClassID"], 0);
                        if (classID > 0)
                        {
                            classesWhere += " OR PageTemplateScopeClassID = " + classID;
                        }
                    }
                }
            }
            // Resolve culture code
            string cultureWhere = string.Empty;
            CultureInfo culture = CultureInfoProvider.GetCultureInfo(cultureCode);
            if (culture != null)
            {
                cultureWhere = " OR PageTemplateScopeCultureID = " + culture.CultureID;
            }

            // Resolve site name
            string siteWhere = string.Empty;
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site != null)
            {
                siteWhere = " OR PageTemplateScopeSiteID = " + site.SiteID;
            }

            // Handle SQL injection
            path = SqlHelper.EscapeQuotes(path);

            // Complete query
            StringBuilder sb = new StringBuilder();
            sb.Append("(PageTemplateForAllPages = 1 OR PageTemplateForAllPages IS NULL)");
            sb.Append(" OR " + templateIdColumnName);
            sb.Append(" IN (SELECT PageTemplateScopeTemplateID FROM CMS_PageTemplateScope WHERE");
            sb.Append(" (PageTemplateScopeSiteID IS NULL " + siteWhere + ")");
            sb.Append(" AND (PageTemplateScopeCultureID IS NULL " + cultureWhere + " )");
            sb.Append(" AND (PageTemplateScopeClassID IS NULL " + classesWhere + ")");
            sb.Append(" AND (PageTemplateScopeLevels IS NULL OR PageTemplateScopeLevels LIKE N'%{" + level + "}%' )");
            sb.Append(" AND	(PageTemplateScopePath = '/' OR N'" + path + "' LIKE PageTemplateScopePath OR N'" + path + "' LIKE PageTemplateScopePath + '/%' ))");

            return sb.ToString();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the PageTemplateScopeInfo structure for the specified pageTemplateScope.
        /// </summary>
        /// <param name="pageTemplateScopeId">PageTemplateScope id</param>
        protected virtual PageTemplateScopeInfo GetPageTemplateScopeInfoInternal(int pageTemplateScopeId)
        {
            if (pageTemplateScopeId > 0)
            {
                return GetObjectQuery().WhereEquals("PageTemplateScopeID", pageTemplateScopeId).TopN(1).FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(PageTemplateScopeInfo info)
        {
            if (info != null)
            {
                // Delete object from database
                base.DeleteInfo(info);
            }
        }

        #endregion
    }
}