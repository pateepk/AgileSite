using System;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Media file data source server control.
    /// </summary>
    [ToolboxData("<{0}:MediaFileDataSource runat=server />"), Serializable]
    public class MediaFileDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private bool mCheckPermissions = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        public string FilePath
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the allowed file extensions.
        /// </summary>
        public string FileExtensions
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the source filter name.
        /// </summary>
        public string LibraryName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the group files should be included.
        /// </summary>
        public bool ShowGroupFiles
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set the group id (for permission checking).
        /// </summary>
        public int GroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if user permissions to file should be checked.
        /// </summary>
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
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
            string where = string.Empty;

            // WHERE condition for media library
            if (!string.IsNullOrEmpty(LibraryName))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteName);
                int siteId = (si != null) ? si.SiteID : 0;
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo(LibraryName, siteId, GroupID);
                if (library != null)
                {
                    where += "FileLibraryID = " + library.LibraryID;
                }
                else
                {
                    // Given library wasn't found
                    return null;
                }
            }
            else
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteName);
                if (si != null)
                {
                    where += "FileSiteID = " + si.SiteID;
                }
            }

            // WHERE condition for file path
            if (!String.IsNullOrEmpty(FilePath))
            {
                where = SqlHelper.AddWhereCondition(where, String.Format("FilePath LIKE N'{0}'", SqlHelper.EscapeQuotes(FilePath)));
            }

            // WHERE condition for file extension
            if (!String.IsNullOrEmpty(FileExtensions))
            {
                string[] extensions = FileExtensions.Split(';');

                if (extensions.Length > 0)
                {
                    where = SqlHelper.AddWhereCondition(where, new WhereCondition().WhereIn("FileExtension", extensions).ToString(true));
                }
            }

            // Add custom WHERE condition

            string groupWhere = ShowGroupFiles ? string.Empty : "LibraryGroupID IS NULL";

            // Check permissions
            string accessWhere = CheckPermissions ? MediaLibraryInfoProvider.CombineSecurityWhereCondition(string.Empty, GroupID) : string.Empty;
            accessWhere = SqlHelper.AddWhereCondition(accessWhere, groupWhere);

            where = accessWhere == string.Empty ? SqlHelper.AddWhereCondition(where, WhereCondition) : SqlHelper.AddWhereCondition(SqlHelper.AddWhereCondition(where, WhereCondition), String.Format("FileLibraryID IN (SELECT LibraryID FROM Media_Library WHERE {0})", accessWhere));

            return MediaFileInfoProvider.GetMediaFiles(where, OrderBy, TopN, SelectedColumns).TypedResult;
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            if (result != null)
            {
                result += "\n";
            }

            result += "media.file|all";

            return result;
        }

        
        /// <summary>
        /// Gets default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "mediafiledatasource", CacheHelper.BaseCacheKey, SiteName, GroupID, LibraryName, FilePath, FileExtensions, WhereCondition, OrderBy, TopN, ShowGroupFiles, CheckPermissions };
        }

        #endregion
    }
}