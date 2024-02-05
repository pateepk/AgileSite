using System;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Media library data source server control.
    /// </summary>
    [ToolboxData("<{0}:MediaLibraryDataSource runat=server />"), Serializable]
    public class MediaLibraryDataSource : CMSBaseDataSource
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets the group ID.
        /// </summary>
        public int GroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the group libraries should be included. (If no group ID is provided.).
        /// </summary>
        public bool ShowGroupLibraries
        {
            get;
            set;
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Gets data source from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (StopProcessing)
            {
                return null;
            }

            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            // Get WHERE condition
            // Get site ID
            string site = DataHelper.GetNotEmpty(SiteName, SiteContext.CurrentSiteName);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(site);
            int siteID = si.SiteID;

            string where = String.Format("(LibrarySiteID = {0})", siteID);

            // WHERE condition for media libraries
            if (GroupID != 0)
            {
                where += String.Format(" AND (LibraryGroupID = {0})", GroupID);
            }
            else
            {
                if (!ShowGroupLibraries)
                {
                    where += " AND (LibraryGroupID IS NULL)";
                }
            }

            // Create WHERE condition
            if (!String.IsNullOrEmpty(WhereCondition))
            {
                where = String.Format("({0}) AND {1}", WhereCondition, where);
            }

            return MediaLibraryInfoProvider.GetMediaLibraries(where, OrderBy, TopN, SelectedColumns).TypedResult;
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            if (!String.IsNullOrEmpty(result))
            {
                result += Environment.NewLine;
            }

            if (GroupID > 0)
            {
                result += MediaLibraryInfo.OBJECT_TYPE_GROUP + "|all";
            }
            else
            {
                result += MediaLibraryInfo.OBJECT_TYPE + "|all";
            }

            return result;
        }
        

        /// <summary>
        /// Gets default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "medialibrarydatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, TopN, SelectedColumns, GroupID, ShowGroupLibraries };
        }

        #endregion
    }
}