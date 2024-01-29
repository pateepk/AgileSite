using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.WorkflowEngine;
using CMS.PortalEngine;
using CMS.DataEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Documents data source control.
    /// </summary>
    [ToolboxData("<{0}:AttachmentsDataSource runat=server></{0}:AttachmentsDataSource>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class AttachmentsDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private int mDocumentVersionHistoryID;
        private bool mCheckPermissions;
        private bool mGetBinary;
        private string mCultureCode;
        private string mPath;
        private Guid mAttachmentGUID = Guid.Empty;
        private Guid mAttachmentGroupGUID = Guid.Empty;
        private Guid mAttachmentFormGUID = Guid.Empty;
        private bool mCombineWithDefaultCulture;

        #endregion


        #region "Properties"

        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Tree node.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether select also binary content of the attachments.
        /// </summary>
        public bool GetBinary
        {
            get
            {
                return mGetBinary;
            }
            set
            {
                mGetBinary = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Attachment GUID (To be able to select only one particular attachment).
        /// </summary>
        public Guid AttachmentGUID
        {
            get
            {
                return mAttachmentGUID;
            }
            set
            {
                mAttachmentGUID = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Group GUID (document field GUID) of the grouped attachments.
        /// </summary>
        public Guid AttachmentGroupGUID
        {
            get
            {
                return mAttachmentGroupGUID;
            }
            set
            {
                mAttachmentGroupGUID = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Form GUID of the temporary attachments.
        /// </summary>
        public Guid AttachmentFormGUID
        {
            get
            {
                return mAttachmentFormGUID;
            }
            set
            {
                mAttachmentFormGUID = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// ID of version history.
        /// </summary>
        public int DocumentVersionHistoryID
        {
            get
            {
                return mDocumentVersionHistoryID;
            }
            set
            {
                mDocumentVersionHistoryID = value;
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
                return mCultureCode;
            }
            set
            {
                mCultureCode = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if the document should be selected eventually from the default culture.
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get
            {
                return mCombineWithDefaultCulture;
            }
            set
            {
                mCombineWithDefaultCulture = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Gets or sets the alias path.
        /// </summary>
        public string Path
        {
            get
            {
                return mPath;
            }
            set
            {
                mPath = value;
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Allows you to specify whether to check permissions of the current user. If the value is 'false' (default value) no permissions are checked. Otherwise, only nodes for which the user has read permission are displayed.
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
                FilterChanged = true;
            }
        }


        /// <summary>
        /// Indicates if latest version of the document should be get
        /// </summary>
        protected bool GetLatestVersion
        {
            get
            {
                return !IsLiveSite || (PortalContext.ViewMode != ViewModeEnum.LiveSite);
            }
        }


        /// <summary>
        /// Indicates if the columns and order by expression should be generated automatically.
        /// </summary>
        public bool AutomaticColumns
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets data source from DB.
        /// </summary>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        /// <returns>Datasource from DB</returns>
        protected override object GetDataSourceFromDB(int offset, int maxRecords, ref int totalRecords)
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

            if (TreeProvider == null)
            {
                TreeProvider = new TreeProvider(MembershipContext.AuthenticatedUser);
            }

            // Init default parameters to be used
            var parameters = 
                new ObjectQuerySettings()
                    .With(p =>
                    {
                        p.Offset = offset;
                        p.MaxRecords = maxRecords;
                    })
                    .OrderBy(OrderBy)
                    .Columns(SelectedColumns)
                    .BinaryData(GetBinary)
                    .TopN(TopN);

            // Grouped attachments
            if (AttachmentGroupGUID != Guid.Empty)
            {
                parameters.Where("AttachmentGroupGUID", QueryOperator.Equals, AttachmentGroupGUID);
            }
            // Unsorted attachments
            else
            {
                parameters.Where("AttachmentIsUnsorted", QueryOperator.Equals, true);
            }

            // Temporary attachments
            if ((AttachmentFormGUID != Guid.Empty) && (DocumentVersionHistoryID == 0))
            {
                parameters.Where("AttachmentFormGUID", QueryOperator.Equals, AttachmentFormGUID);
            }
            // Else document attachments
            else
            {
                if (DocumentVersionHistoryID == 0)
                {
                    // Ensure current site name
                    if (string.IsNullOrEmpty(SiteName))
                    {
                        SiteName = SiteContext.CurrentSiteName;
                    }
                    // Ensure current culture
                    if (string.IsNullOrEmpty(CultureCode))
                    {
                        CultureCode = LocalizationContext.PreferredCultureCode;
                    }


                    // Path not specified
                    if (Path == null)
                    {
                        return null;
                    }

                    Node = TreeProvider.SelectSingleNode(SiteName, MacroResolver.ResolveCurrentPath(Path), CultureCode, CombineWithDefaultCulture, null, !GetLatestVersion, CheckPermissions, false);
                    if (Node != null)
                    {
                        parameters.Where("AttachmentDocumentID", QueryOperator.Equals, Node.DocumentID);

                        // Get attachments for latest version if not live site
                        if (GetLatestVersion)
                        {
                            WorkflowManager wm = WorkflowManager.GetInstance(TreeProvider);
                            WorkflowInfo wi = wm.GetNodeWorkflow(Node);
                            if (wi != null)
                            {
                                DocumentVersionHistoryID = Node.DocumentCheckedOutVersionHistoryID;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (AttachmentGUID == Guid.Empty)
            {
                // Ensure additional where condition
                parameters.Where(WhereCondition);
            }
            else
            {
                parameters.NewWhere().Where("AttachmentGUID", QueryOperator.Equals, AttachmentGUID);

                // Get site ID from the document or use site ID from SiteName property or current site ID if document is not available
                if (DocumentVersionHistoryID == 0)
                {
                    int siteId;
                    if (Node != null)
                    {
                        siteId = Node.NodeSiteID;
                    }
                    else
                    {
                        siteId = SiteInfoProvider.GetSiteID(SiteName);
                        if (siteId == 0)
                        {
                            siteId = SiteContext.CurrentSiteID;
                        }
                    }

                    parameters.Where("AttachmentSiteID", QueryOperator.Equals, siteId);
                }
            }

            IDataQuery query;
            DataSet ds;

            // Get attachments for published document
            if (DocumentVersionHistoryID == 0)
            {
                if (AutomaticColumns)
                {
                    parameters.OrderBy("AttachmentOrder, AttachmentName, AttachmentID");
                    parameters.Columns("AttachmentID, AttachmentGUID, AttachmentImageWidth, AttachmentImageHeight, AttachmentExtension, AttachmentName, AttachmentSize, AttachmentOrder, AttachmentTitle, AttachmentDescription");
                }

                query = 
                    AttachmentInfoProvider.GetAttachments()
                        // Attachments data source returns only main attachments
                        .ExceptVariants()
                        .WithSettings(parameters);

                ds = query.Result;
            }
            else
            {
                if (AutomaticColumns)
                {
                    parameters.OrderBy("AttachmentOrder, AttachmentName, AttachmentHistoryID");
                    parameters.Columns("AttachmentHistoryID, AttachmentGUID, AttachmentImageWidth, AttachmentImageHeight, AttachmentExtension, AttachmentName, AttachmentSize, AttachmentOrder, AttachmentTitle, AttachmentDescription");
                }

                // Make sure the proper columns are used for history
                parameters.ReplaceColumn("AttachmentID", "AttachmentHistoryID");

                query = AttachmentHistoryInfoProvider.GetAttachmentHistories()
                                                     .InVersionExceptVariants(DocumentVersionHistoryID)
                                                     .WithSettings(parameters);

                ds = query.Result;

                // Ensure consistent ID column name
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    if (ds.Tables[0].Columns.Contains("AttachmentHistoryID"))
                    {
                        ds.Tables[0].Columns["AttachmentHistoryID"].ColumnName = "AttachmentID";
                    }
                }
            }

            totalRecords = query.TotalRecords;

            return ds;
        }


        /// <summary>
        /// Gets related data.
        /// </summary>
        protected override object GetRelatedData()
        {
            return Node;
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

            if (Node != null)
            {
                result += "documentid|" + Node.DocumentID + "|attachments";
            }
            else
            {
                result += "cms.attachment|all";
            }

            return result;
        }


        /// <summary>
        /// Gets the default cache item name.
        /// </summary>
        /// <returns></returns>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "attachmentsdatasource", CacheHelper.BaseCacheKey, ClientID, SiteName, Path, TopN, CultureCode, CombineWithDefaultCulture, CheckPermissions, GetLatestVersion, WhereCondition, OrderBy, AttachmentGroupGUID, AttachmentGUID, AttachmentFormGUID, GetBinary, DocumentVersionHistoryID, SelectedColumns };
        }


        /// <summary>
        /// Determines whether the attachment data source instance has data.
        /// </summary>
        public bool HasData()
        {
            return !DataHelper.DataSourceIsEmpty(DataSource);
        }

        #endregion
    }
}