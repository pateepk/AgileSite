using System;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Board messages data source server control.
    /// </summary>
    [ToolboxData("<{0}:BoardMessagesDataSource runat=server />"), Serializable]
    public class BoardMessagesDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private bool mSelectOnlyApproved = true;
        private string mBoardName;
        private bool mUseDocumentFilter;
        private int mGroupID;
        private bool mShowGroupMessages;

        /// <summary>
        /// Data properties variable.
        /// </summary>
        private ICMSDataProperties mProperties = new CMSDataProperties();

        #endregion


        #region "Properties"

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
        /// Gets or sets select only approved messages.
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


        /// <summary>
        /// Gets or sets message board name.
        /// </summary>
        public string BoardName
        {
            get
            {
                return mBoardName;
            }
            set
            {
                mBoardName = value;
            }
        }


        /// <summary>
        /// Gets or sets the group ID.
        /// </summary>
        public int GroupID
        {
            get
            {
                return mGroupID;
            }
            set
            {
                mGroupID = value;
            }
        }


        /// <summary>
        /// Indicates if the group messages should be included. (If no group ID is provided.).
        /// </summary>
        public bool ShowGroupMessages
        {
            get
            {
                return mShowGroupMessages;
            }
            set
            {
                mShowGroupMessages = value;
            }
        }

        #endregion


        #region "Document properties"

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
        /// Gets or sets the alias path of the board document.
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
            }
        }


        /// <summary>
        /// Gets or sets the culture code of the board document.
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
            }
        }


        /// <summary>
        /// Gets or sets the where condition for board documents.
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


        /// <summary>
        /// Gets or sets combine with default culture for board document.
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
            }
        }


        /// <summary>
        /// Gets or sets select only published for documents.
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
            }
        }


        /// <summary>
        /// Gets or sets max relative level for board documents.
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
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoardMessagesDataSource()
        {
            PropagateProperties(mProperties);
        }

        #endregion


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
            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            // Get site ID
            int siteId = 0;
            if (!String.IsNullOrEmpty(SiteName))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteName);

                if (si != null)
                {
                    siteId = si.SiteID;
                }
            }

            // Create WHERE condition
            var where = new WhereCondition().WhereEquals("BoardSiteID", siteId);

            if (!string.IsNullOrEmpty(BoardName))
            {
                where.WhereEquals("BoardName", BoardName);
            }

            // Document filter should be used
            if (UseDocumentFilter)
            {
                var documentsWhere = new WhereCondition().Where(TreeProvider.GetCompleteWhereCondition(SiteName, Path, CultureCode, CombineWithDefaultCulture, DocumentsWhereCondition, SelectOnlyPublished, MaxRelativeLevel));
                var tree = new TreeProvider();
                var subQuery = tree.SelectNodes()
                                   .All()
                                   .Column("DocumentID")
                                   .Where(documentsWhere);

                where.WhereIn("BoardDocumentID", subQuery);
            }

            // Add approved message condition
            if (SelectOnlyApproved)
            {
                where.WhereTrue("MessageApproved");
            }

            // WHERE condition for group boards
            if (GroupID != 0)
            {
                where.WhereEquals("BoardGroupID", GroupID);
            }
            else if (!ShowGroupMessages)
            {
                where.WhereNull("BoardGroupID");
            }

            // Add external where condition
            if (!String.IsNullOrEmpty(WhereCondition))
            {
                where.Where(WhereCondition);
            }

            // Get the data
            return ConnectionHelper.ExecuteQuery("Board.Message.SelectAllWithBoard", null, where.ToString(true), OrderBy, TopN, SelectedColumns);
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

            result += "board.message|all";

            return result;
        }


        /// <summary>
        /// Gets cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "boardmessagesdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, TopN, SiteName, SelectOnlyApproved, UseDocumentFilter, SelectedColumns, BoardName, ShowGroupMessages, GroupID };
        }
        
        #endregion
    }
}