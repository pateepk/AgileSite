using System;
using System.Data;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Blog comments data source server control.
    /// </summary>
    [ToolboxData("<{0}:BlogCommentDataSource runat=server />"), Serializable]
    public class BlogCommentDataSource : CMSBaseDataSource
    {
        private bool mSelectOnlyApproved = true;
        private bool mUseDocumentFilter;
        private ICMSDataProperties mProperties = new CMSDataProperties();


        /// <summary>
        /// Gets or sets if only approved comments should be selected.
        /// </summary>
        public bool SelectOnlyApproved
        {
            get
            {
                return mSelectOnlyApproved;
            }
            set
            {
                mSelectOnlyApproved = value;
            }
        }


        #region "Document (Blog posts) filter properties"

        /// <summary>
        /// Data properties
        /// </summary>
        protected CMSAbstractDataProperties Properties
        {
            get
            {
                return (CMSAbstractDataProperties)mProperties;
            }
        }


        /// <summary>
        /// Indicates if the comments should be retrieved according to document filter settings.
        /// </summary>
        public bool UseDocumentFilter
        {
            get
            {
                return mUseDocumentFilter;
            }
            set
            {
                mUseDocumentFilter = value;
            }
        }


        /// <summary>
        /// Path of the documents to be displayed. /% selects all documents.
        /// </summary>
        public string Path
        {
            get
            {
                return Properties.Path;
            }
            set
            {
                Properties.Path = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Culture code, such as en-us.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return Properties.CultureCode;
            }
            set
            {
                Properties.CultureCode = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if the documents from the default culture version should be alternatively used.
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get
            {
                return Properties.CombineWithDefaultCulture;
            }
            set
            {
                Properties.CombineWithDefaultCulture = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if only published documents should be displayed.
        /// </summary>
        public bool SelectOnlyPublished
        {
            get
            {
                return Properties.SelectOnlyPublished;
            }
            set
            {
                Properties.SelectOnlyPublished = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Relative level of child documents that should be selected. -1 selects all child documents.
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return Properties.MaxRelativeLevel;
            }
            set
            {
                Properties.MaxRelativeLevel = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Gets or sets the where condition for blog posts.
        /// </summary>
        public string DocumentsWhereCondition
        {
            get
            {
                return Properties.WhereCondition;
            }
            set
            {
                Properties.WhereCondition = value;
            }
        }

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public BlogCommentDataSource()
        {
            PropagateProperties(mProperties);
        }


        #region "Methods, events, handlers"

        /// <summary>
        /// Propagates given settings
        /// </summary>
        /// <param name="properties">Settings</param>
        protected void PropagateProperties(ICMSDataProperties properties)
        {
            base.PropagateProperties(properties);
            mProperties = properties;
        }


        /// <summary>
        /// Gets datasource from DB.
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

            // Create WHERE condition
            var where = new WhereCondition();
            var documentsWhere = new WhereCondition();
            // Document filter should be used
            if (UseDocumentFilter)
            {
                documentsWhere.Where(TreeProvider.GetCompleteWhereCondition(SiteName, Path, CultureCode, CombineWithDefaultCulture, DocumentsWhereCondition, SelectOnlyPublished, MaxRelativeLevel));
            }
            else if (!string.IsNullOrEmpty(SiteName) && (SiteName != TreeProvider.ALL_SITES))
            {
                documentsWhere.WhereEquals("NodeSiteID", SiteInfoProvider.GetSiteID(SiteName));
            }

            var tree = new TreeProvider();
            var subQuery = tree.SelectNodes()
                               .All()
                               .Column("DocumentID")
                               .Where(documentsWhere);

            where.WhereIn("CommentPostDocumentID", subQuery);

            if (SelectOnlyApproved)
            {
                where.WhereTrue("CommentApproved");
            }

            if (!string.IsNullOrEmpty(WhereCondition))
            {
                where.Where(WhereCondition);
            }

            var data = BlogCommentInfoProvider.GetBlogComments()
                                                .TopN(TopN)
                                                .Columns(SelectedColumns)
                                                .Where(where)
                                                .OrderBy(OrderBy)
                                                .TypedResult;

            // Ensure SiteName column if set to a specific site
            if ((SiteName != TreeProvider.ALL_SITES) && !DataHelper.DataSourceIsEmpty(data))
            {
                DataTable table = data.Tables[0];
                DataHelper.EnsureColumn(table, "SiteName", typeof(string));
                DataHelper.ChangeStringValues(table, "SiteName", SiteName, null);
            }

            return data;
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

            result += "blog.comment|all";

            return result;
        }


        /// <summary>
        /// Gets cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "blogcommentdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, TopN, SiteName, SelectOnlyApproved, UseDocumentFilter, SelectedColumns, DocumentsWhereCondition, CultureCode, MaxRelativeLevel, CombineWithDefaultCulture };
        }

        #endregion
    }
}