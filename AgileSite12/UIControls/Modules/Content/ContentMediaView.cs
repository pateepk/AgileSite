using System;
using System.Collections;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for content media view control
    /// </summary>
    public abstract class ContentMediaView : MediaView
    {
        #region "Constants"

        private const string SELECTOR_FOLDER = "~/CMSModules/Content/Controls/Dialogs/Selectors/LinkMediaSelector/";

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner media view control
        /// </summary>
        public abstract ContentInnerMediaView InnerMediaControl
        {
            get;
        }


        /// <summary>
        /// Gets or sets a view mode used to display files.
        /// </summary>
        public override DialogViewModeEnum ViewMode
        {
            get
            {
                return base.ViewMode;
            }
            set
            {
                base.ViewMode = value;
                InnerMediaControl.ViewMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the OutputFormat (needed for correct dialog type reckognition).
        /// </summary>
        public OutputFormatEnum OutputFormat
        {
            get
            {
                return InnerMediaControl.OutputFormat;
            }
            set
            {
                InnerMediaControl.OutputFormat = value;
            }
        }


        /// <summary>
        /// Gets or sets text of the information label.
        /// </summary>
        public string InfoText
        {
            get
            {
                return InnerMediaControl.InfoText;
            }
            set
            {
                InnerMediaControl.InfoText = value;
            }
        }


        /// <summary>
        /// Gets currently selected page size.
        /// </summary>
        public int CurrentPageSize
        {
            get
            {
                return InnerMediaControl.CurrentPageSize;
            }
        }


        /// <summary>
        /// Gets currently selected page size.
        /// </summary>
        public int CurrentOffset
        {
            get
            {
                return InnerMediaControl.CurrentOffset;
            }
        }


        /// <summary>
        /// Gets or sets currently selected page.
        /// </summary>
        public int CurrentPage
        {
            get
            {
                return InnerMediaControl.CurrentPage;
            }
            set
            {
                InnerMediaControl.CurrentPage = value;
            }
        }


        /// <summary>
        /// Gets or sets ID of the parent node.
        /// </summary>
        public int AttachmentNodeParentID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a UniGrid control used to display files in LIST view mode.
        /// </summary>
        public UniGrid ListViewControl
        {
            get
            {
                return InnerMediaControl.ListViewControl;
            }
        }


        /// <summary>
        /// Gets the node attachments are related to.
        /// </summary>
        public TreeNode TreeNodeObj
        {
            get
            {
                return mTreeNodeObj;
            }
            set
            {
                mTreeNodeObj = value;
                InnerMediaControl.TreeNodeObj = value;
            }
        }


        /// <summary>
        /// Gets the site attachments are related to.
        /// </summary>
        public SiteInfo SiteObj
        {
            get
            {
                return mSiteObj ?? (mSiteObj = GetSiteObj());
            }
            set
            {
                mSiteObj = value;
            }
        }


        /// <summary>
        /// Indicates whether the content tree is displaying more than max tree nodes.
        /// </summary>
        public bool IsFullListingMode
        {
            get
            {
                return InnerMediaControl.IsFullListingMode;
            }
            set
            {
                InnerMediaControl.IsFullListingMode = value;
            }
        }


        /// <summary>
        /// Decides if absolute URL should be returned. It may be required by configuration or linked node site is different from current site.
        /// </summary>
        private bool IsAbsoluteUrlRequiredInDialog
        {
            get
            {
                var currentSiteID = SiteContext.CurrentSiteID;

                return Config.UseFullURL || (currentSiteID != SiteObj.SiteID) || (currentSiteID != GetCurrentSiteId());
            }
        }

        #endregion


        #region "Private variables"

        private SiteInfo mSiteObj;
        private TreeNode mTreeNodeObj;

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes scripts used by the control.
        /// </summary>
        protected void InitializeControlScripts()
        {
            ScriptHelper.RegisterStartupScript(this, GetType(), "DialogsSelectAction", ScriptHelper.GetScript(@"
function SetSelectAction(argument) {
    // Raise select action
    SetAction('select', argument);
    RaiseHiddenPostBack();
}
function SetParentAction(argument) {
    // Raise select action
    SetAction('parentselect', argument);
    RaiseHiddenPostBack();
}"));
        }

        #endregion


        #region "Inner media view event handlers"

        /// <summary>
        /// Returns argument set according passed DataRow and flag indicating whether the set is obtained for selected item.
        /// </summary>
        /// <param name="data">DataRow with all the item data</param>
        protected string InnerMedia_GetArgumentSet(IDataContainer data)
        {
            // Return required argument set
            return GetArgumentSet(data);
        }


        /// <summary>
        /// Creates URL for node.
        /// If node belongs to current site, a relative URL is returned.
        /// If node belongs to site different from current site, a presentation URL is returned (if presentation URL configuration is available). Otherwise, an absolute URL returned.
        /// </summary>
        /// <param name="node">Tree node to to find URL for.</param>
        public string GetContentItemUrl(TreeNode node)
        {
            // Generate permanent URL.
            if (UsePermanentUrls)
            {
                string url = GetPermanentUrl(node);

                return (IsAbsoluteUrlRequiredInDialog) ? CreateAbsoluteUrl(url) : url;
            }

            // Ensure live site view mode for URLs edited in on-site edit mode
            if (PortalContext.ViewMode.IsEditLive())
            {
                PortalContext.SetRequestViewMode(ViewModeEnum.LiveSite);
            }

            // Create presentation/absolute URL.
            if (IsAbsoluteUrlRequiredInDialog)
            {
                // Use presentation URL if available
                return DocumentURLProvider.GetPresentationUrl(node);
            }

            // Relative URL if node is on current site
            return DocumentURLProvider.GetUrl(node);
        }


        private string InnerMedia_GetListItemUrl(IDataContainer data, bool isPreview)
        {
            // Get set of important information
            string arg = GetArgumentSet(data);

            // Get URL of the list item image
            return GetItemUrl(arg, 0, 0, 0);
        }


        /// <summary>
        /// Creates URL for node.
        /// If node belongs to current site, a relative URL is returned.
        /// If node belongs to site different from current site, a presentation URL is returned (if presentation URL configuration is available). Otherwise, an absolute URL returned.
        /// </summary>
        /// <param name="node">Tree node to to find URL for.</param>
        private string InnerMedia_GetContentItemUrl(TreeNode node)
        {
            return GetContentItemUrl(node);
        }


        private IconParameters InnerMedia_GetThumbsItemUrl(IDataContainer data, bool isPreview, int height, int width, int maxSideSize, string extension)
        {
            var parameters = new IconParameters();
            var arg = GetArgumentSet(data);
            var nodeUrl = "";

            if ((MediaSourceEnum.Content == SourceType) && (data is DataRowContainer))
            {
                var node = TreeNode.New(((DataRowContainer)data).DataRow);
                if (node != null)
                {
                    nodeUrl = GetContentItemUrl(node);
                }
            }

            // If image is requested for preview
            if (!isPreview)
            {
                parameters.Url = (string.IsNullOrEmpty(nodeUrl)) ? GetItemUrl(arg, 0, 0, 0) : nodeUrl;

                return parameters;
            }

            if (ImageHelper.IsImage(extension))
            {
                parameters.Url = (string.IsNullOrEmpty(nodeUrl)) ? GetItemUrl(arg, height, width, maxSideSize) : nodeUrl;

                return parameters;
            }

            string className = (SourceType == MediaSourceEnum.Content) ? data.GetValue("ClassName").ToString().ToLowerCSafe() : "";
            if (className.EqualsCSafe(SystemDocumentTypes.File, true))
            {
                // File isn't image and no preview exists - get default file icon
                parameters.IconClass = UIHelper.GetFileIconClass(extension);
            }
            else if (((SourceType == MediaSourceEnum.DocumentAttachments) || (SourceType == MediaSourceEnum.Attachment) || (SourceType == MediaSourceEnum.MetaFile)) && !String.IsNullOrEmpty(extension))
            {
                // Get file icon for attachment
                parameters.IconClass = UIHelper.GetFileIconClass(extension);
            }
            else
            {
                var dataClass = DataClassInfoProvider.GetDataClassInfo(className);
                parameters.Url = UIHelper.GetDocumentTypeIconUrl(Page, className, "48x48");

                if (dataClass != null)
                {
                    parameters.IconClass = (string)dataClass.GetValue("ClassIconClass");
                }
            }

            // Set font icon size
            if (!string.IsNullOrEmpty(parameters.IconClass))
            {
                parameters.IconSize = FontIconSizeEnum.Dashboard;
            }

            return parameters;
        }


        private void ListViewControl_OnPageChanged(object sender, EventArgs e)
        {
            RaiseListReloadRequired();
        }


        private void UniPagerControl_OnPageChanged(object sender, int pageNumber)
        {
            RaiseListReloadRequired();
        }

        #endregion


        #region "Helper methods"

        private SiteInfo GetSiteObj()
        {
            return (TreeNodeObj != null) ? SiteInfoProvider.GetSiteInfo(TreeNodeObj.NodeSiteID) : SiteContext.CurrentSite;
        }


        /// <summary>
        /// Initializes all nested controls.
        /// </summary>
        protected void SetupControls()
        {
            InitializeControlScripts();

            // Initialize inner view control
            var innerView = InnerMediaControl;

            innerView.ViewMode = ViewMode;
            innerView.DataSource = DataSource;
            innerView.TotalRecords = TotalRecords;
            innerView.SelectableContent = SelectableContent;
            innerView.SelectablePageTypes = SelectablePageTypes;
            innerView.SourceType = SourceType;
            innerView.IsLiveSite = IsLiveSite;
            innerView.NodeParentID = AttachmentNodeParentID;

            innerView.ResizeToHeight = ResizeToHeight;
            innerView.ResizeToMaxSideSize = ResizeToMaxSideSize;
            innerView.ResizeToWidth = ResizeToWidth;

            // Set grid definition according source type
            string gridName;
            if (SourceType == MediaSourceEnum.DocumentAttachments)
            {
                gridName = SELECTOR_FOLDER + "AttachmentsListView.xml";
            }
            else if (SourceType == MediaSourceEnum.MetaFile)
            {
                gridName = SELECTOR_FOLDER + "MetaFileListView.xml";
            }
            else
            {
                string[] linkCustomFormatCodes = { "linkdoc", "relationship", "selectpath" };

                if ((OutputFormat == OutputFormatEnum.HTMLLink) || (OutputFormat == OutputFormatEnum.BBLink) || ((OutputFormat == OutputFormatEnum.Custom) && linkCustomFormatCodes.Contains(Config.CustomFormatCode)))
                {
                    gridName = SELECTOR_FOLDER + "ContentListView_Link.xml";
                }
                else
                {
                    gridName = SELECTOR_FOLDER + "ContentListView.xml";
                }
            }

            innerView.ListViewControl.GridName = gridName;

            innerView.ListViewControl.OnPageChanged -= ListViewControl_OnPageChanged;
            innerView.ListViewControl.OnPageChanged += ListViewControl_OnPageChanged;


            if (innerView.ThumbnailsViewControl.UniPagerControl != null)
            {
                innerView.ThumbnailsViewControl.UniPagerControl.OnPageChanged -= UniPagerControl_OnPageChanged;
                innerView.ThumbnailsViewControl.UniPagerControl.OnPageChanged += UniPagerControl_OnPageChanged;
                innerView.PageSizeDropDownList.SelectedIndexChanged += PageSizeDropDownList_SelectedIndexChanged;
            }

            // Set inner control binding columns
            if (SourceType != MediaSourceEnum.MetaFile)
            {
                if (SourceType == MediaSourceEnum.DocumentAttachments)
                {
                    innerView.FileIdColumn = "AttachmentGUID";
                    innerView.FileNameColumn = "AttachmentName";
                }
                else
                {
                    innerView.FileIdColumn = "NodeGUID";
                    innerView.FileNameColumn = "DocumentName";
                }

                innerView.FileExtensionColumn = "AttachmentExtension";
                innerView.FileSizeColumn = "AttachmentSize";
                innerView.FileWidthColumn = "AttachmentImageWidth";
                innerView.FileHeightColumn = "AttachmentImageHeight";
            }
            else
            {
                innerView.FileIdColumn = "MetaFileID";
                innerView.FileNameColumn = "MetaFileName";
                innerView.FileExtensionColumn = "MetaFileExtension";
                innerView.FileSizeColumn = "MetaFileSize";
                innerView.FileWidthColumn = "MetaFileImageWidth";
                innerView.FileHeightColumn = "MetaFileImageHeight";
            }

            // Register for inner media events
            innerView.GetArgumentSet += InnerMedia_GetArgumentSet;
            innerView.GetListItemUrl += InnerMedia_GetListItemUrl;
            innerView.GetThumbsItemUrl += InnerMedia_GetThumbsItemUrl;
            innerView.GetContentItemUrl += InnerMedia_GetContentItemUrl;
        }


        /// <summary>
        /// Reloads data when page size changed in thumbnails view mode.
        /// </summary>
        protected void PageSizeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            RaiseListReloadRequired();
        }


        /// <summary>
        /// Returns argument set for the passed file data row.
        /// </summary>
        /// <param name="data">Data row object holding all the data on current file</param>
        public string GetArgumentSet(IDataContainer data)
        {
            string className = ValidationHelper.GetString(data.GetValue("ClassName"), String.Empty).ToLowerCSafe();
            string name;

            // Get file name with extension
            switch (SourceType)
            {
                case MediaSourceEnum.DocumentAttachments:
                    name = AttachmentHelper.GetFullFileName(Path.GetFileNameWithoutExtension(data.GetValue("AttachmentName").ToString()), data.GetValue("AttachmentExtension").ToString());
                    break;
                case MediaSourceEnum.MetaFile:
                    name = MetaFileInfoProvider.GetFullFileName(Path.GetFileNameWithoutExtension(data.GetValue("MetaFileName").ToString()), data.GetValue("MetaFileExtension").ToString());
                    break;
                default:
                    name = data.GetValue("DocumentName").ToString();
                    break;
            }

            StringBuilder sb = new StringBuilder();

            // Common information for both content & attachments
            sb.Append("name|" + CMSDialogHelper.EscapeArgument(name));

            // Load attachment info only for file document type
            if (((SourceType != MediaSourceEnum.Content) && (SourceType != MediaSourceEnum.MetaFile)) || className.EqualsCSafe(SystemDocumentTypes.File, true))
            {
                sb.Append("|AttachmentExtension|" + CMSDialogHelper.EscapeArgument(data.GetValue("AttachmentExtension")));
                sb.Append("|AttachmentImageWidth|" + CMSDialogHelper.EscapeArgument(data.GetValue("AttachmentImageWidth")));
                sb.Append("|AttachmentImageHeight|" + CMSDialogHelper.EscapeArgument(data.GetValue("AttachmentImageHeight")));
                sb.Append("|AttachmentSize|" + CMSDialogHelper.EscapeArgument(data.GetValue("AttachmentSize")));
                sb.Append("|AttachmentGUID|" + CMSDialogHelper.EscapeArgument(data.GetValue("AttachmentGUID")));
            }
            else if (SourceType == MediaSourceEnum.MetaFile)
            {
                sb.Append("|MetaFileExtension|" + CMSDialogHelper.EscapeArgument(data.GetValue("MetaFileExtension")));
                sb.Append("|MetaFileImageWidth|" + CMSDialogHelper.EscapeArgument(data.GetValue("MetaFileImageWidth")));
                sb.Append("|MetaFileImageHeight|" + CMSDialogHelper.EscapeArgument(data.GetValue("MetaFileImageHeight")));
                sb.Append("|MetaFileSize|" + CMSDialogHelper.EscapeArgument(data.GetValue("MetaFileSize")));
                sb.Append("|MetaFileGUID|" + CMSDialogHelper.EscapeArgument(data.GetValue("MetaFileGUID")));
                sb.Append("|SiteID|" + CMSDialogHelper.EscapeArgument(data.GetValue("MetaFileSiteID")));
            }
            else
            {
                sb.Append("|AttachmentExtension||AttachmentImageWidth||AttachmentImageHeight||AttachmentSize||AttachmentGUID|");
            }

            // Get source type specific information
            if (SourceType == MediaSourceEnum.Content)
            {
                var siteId = data.GetValue("NodeSiteID").ToInteger(0);
                var siteName = SiteInfoProvider.GetSiteName(siteId);

                sb.Append("|NodeSiteID|" + siteId);
                sb.Append("|SiteName|" + CMSDialogHelper.EscapeArgument(siteName));
                sb.Append("|NodeGUID|" + CMSDialogHelper.EscapeArgument(data.GetValue("NodeGUID")));
                sb.Append("|NodeID|" + CMSDialogHelper.EscapeArgument(data.GetValue("NodeID")));
                sb.Append("|NodeAlias|" + CMSDialogHelper.EscapeArgument(data.GetValue("NodeAlias")));
                sb.Append("|NodeAliasPath|" + CMSDialogHelper.EscapeArgument(data.GetValue("NodeAliasPath")));
                sb.Append("|DocumentUrlPath|" + CMSDialogHelper.EscapeArgument(data.GetValue("DocumentUrlPath")));
                sb.Append("|DocumentExtensions|" + CMSDialogHelper.EscapeArgument(data.GetValue("DocumentExtensions")));
                sb.Append("|ClassName|" + CMSDialogHelper.EscapeArgument(data.GetValue("ClassName")));
                sb.Append("|NodeLinkedNodeID|" + CMSDialogHelper.EscapeArgument(data.GetValue("NodeLinkedNodeID")));
            }
            else if (SourceType != MediaSourceEnum.MetaFile)
            {
                string formGuid = data.ContainsColumn("AttachmentFormGUID") ? data.GetValue("AttachmentFormGUID").ToString() : Guid.Empty.ToString();
                string siteId = data.ContainsColumn("AttachmentSiteID") ? data.GetValue("AttachmentSiteID").ToString() : "0";

                sb.Append("|SiteID|" + CMSDialogHelper.EscapeArgument(siteId));
                sb.Append("|FormGUID|" + CMSDialogHelper.EscapeArgument(formGuid));
                sb.Append("|AttachmentDocumentID|" + CMSDialogHelper.EscapeArgument(data.GetValue("AttachmentDocumentID")));
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns arguments table for the passed argument.
        /// </summary>
        /// <param name="argument">Argument containing information on current media item</param>
        public static Hashtable GetArgumentsTable(string argument)
        {
            Hashtable table = new Hashtable();

            string[] argArr = argument.Split('|');
            try
            {
                // Fill table
                for (int i = 0; i < argArr.Length; i = i + 2)
                {
                    table[argArr[i].ToLowerCSafe()] = CMSDialogHelper.UnEscapeArgument(argArr[i + 1]);
                }
            }
            catch
            {
                throw new Exception("[Media view]: Error loading arguments table.");
            }

            return table;
        }


        /// <summary>
        /// Returns URL of the media item according site settings.
        /// </summary>
        /// <param name="argument">Argument containing information on current media item</param>
        /// <param name="height">Item height in px</param>
        /// <param name="width">Item width in px</param>
        /// <param name="maxSideSize">Maximum dimension for images displayed for thumbnails view</param>
        public string GetItemUrl(string argument, int height, int width, int maxSideSize)
        {
            Hashtable argTable = GetArgumentsTable(argument);
            if (argTable.Count < 2)
            {
                return string.Empty;
            }

            string url;

            // Get image URL
            switch (SourceType)
            {
                case MediaSourceEnum.MetaFile:
                    {
                        url = GetMetaFileUrl(height, width, maxSideSize, argTable);
                    }
                    break;
                default:
                    {
                        url = GetAttachmentUrl(height, width, maxSideSize, argTable);
                    }
                    break;
            }

            return url;
        }


        /// <summary>
        /// Returns URL, resolved if not in media selector, with size parameters added.
        /// </summary>
        /// <param name="url">Media file URL</param>
        /// <param name="height">Height parameter that should be added to the URL</param>
        /// <param name="width">Width parameter that should be added to the URL</param>
        /// <param name="maxSideSize">Max side size parameter that should be added to the URL</param>
        public string AddURLDimensionsAndResolve(string url, int height, int width, int maxSideSize)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            string result = url;

            // If image dimensions are specified
            if (maxSideSize > 0)
            {
                result = URLHelper.AddParameterToUrl(result, "maxsidesize", maxSideSize.ToString());
            }
            if (height > 0)
            {
                result = URLHelper.AddParameterToUrl(result, "height", height.ToString());
            }
            if (width > 0)
            {
                result = URLHelper.AddParameterToUrl(result, "width", width.ToString());
            }

            // Media selector should returns non-resolved URL in all cases
            bool isMediaSelector = (OutputFormat == OutputFormatEnum.URL) && (SelectableContent == SelectableContentEnum.OnlyMedia);

            return (isMediaSelector || ((Config != null) && Config.ContentUseRelativeUrl)) ? result : UrlResolver.ResolveUrl(result);
        }

        #endregion


        #region "Content methods"

        private string GetPermanentUrl(TreeNode node)
        {
            if (node == null)
            {
                return "";
            }

            if (node.NodeIsContentOnly)
            {
                return DocumentURLProvider.GetUrl(node);
            }

            string result;
            bool isLink = ((OutputFormat == OutputFormatEnum.BBLink) || (OutputFormat == OutputFormatEnum.HTMLLink)) ||
                          ((OutputFormat == OutputFormatEnum.URL) && (SelectableContent == SelectableContentEnum.AllContent));

            var documentExtension = DocumentURLProvider.GetExtension(node);
            var nodeAlias = node.NodeAlias;
            var nodeGuid = node.NodeGUID;

            if (String.IsNullOrEmpty(nodeAlias))
            {
                nodeAlias = "default";
            }
            
            if (!node.IsFile() || isLink)
            {
                result = DocumentURLProvider.GetPermanentDocUrl(nodeGuid, nodeAlias, SiteObj.SiteName, null, documentExtension);
            }
            else
            {
                result = AttachmentURLProvider.GetPermanentAttachmentUrl(nodeGuid, nodeAlias, documentExtension);
            }
            return result;
        }

        #endregion


        #region "Attachment methods"

        /// <summary>
        /// Returns URL for the attachment specified by arguments.
        /// </summary>
        /// <param name="attachmentGuid">GUID of the attachment</param>
        /// <param name="attachmentName">Name of the attachment</param>
        /// <param name="attachmentNodeAlias">Attachment node alias</param>
        /// <param name="height">Height of the attachment</param>
        /// <param name="width">Width of the attachment</param>
        /// <param name="maxSideSize">Maximum size of the item if attachment is image</param>
        public string GetAttachmentItemUrl(Guid attachmentGuid, string attachmentName, string attachmentNodeAlias, int height, int width, int maxSideSize)
        {
            string result;

            string safeFileName = URLHelper.GetSafeFileName(attachmentName, SiteObj.SiteName);
            if (UsePermanentUrls || string.IsNullOrEmpty(attachmentNodeAlias))
            {
                result = AttachmentURLProvider.GetAttachmentUrl(attachmentGuid, safeFileName);
            }
            else
            {
                result = AttachmentURLProvider.GetAttachmentUrl(safeFileName, attachmentNodeAlias);
            }

            // Ensure attachment culture (needed for attachment handler to find attachment in displayed culture)
            result = URLHelper.AddParameterToUrl(result, URLHelper.LanguageParameterName, Config.Culture);

            // If current site is different from attachment site make URL absolute (domain included)
            if (IsAbsoluteUrlRequiredInDialog)
            {
                result = CreateAbsoluteUrl(result);
            }

            return AddURLDimensionsAndResolve(result, height, width, maxSideSize);
        }

        #endregion


        #region "MetaFile methods"

        /// <summary>
        /// Returns URL for the metafile specified by arguments.
        /// </summary>
        /// <param name="attachmentGuid">GUID of the metafile</param>
        /// <param name="attachmentName">Name of the metafile</param>
        /// <param name="height">Height parameter that should be added to the URL</param>
        /// <param name="width">Width parameter that should be added to the URL</param>
        /// <param name="maxSideSize">Maximum size of the item if metafile is image</param>
        public string GetMetaFileItemUrl(Guid attachmentGuid, string attachmentName, int height, int width, int maxSideSize)
        {
            string result = MetaFileURLProvider.GetMetaFileUrl(attachmentGuid, attachmentName);

            // If current site is different from attachment site make URL absolute (domain included)
            if (IsAbsoluteUrlRequiredInDialog)
            {
                result = CreateAbsoluteUrl(result);
            }

            return AddURLDimensionsAndResolve(result, height, width, maxSideSize);
        }


        /// <summary>
        /// Transform relative URL to absolute URL.
        /// </summary>
        /// <param name="url">Relative URL which should be transformed to absolute.</param>
        private string CreateAbsoluteUrl(string url)
        {
            return URLHelper.GetAbsoluteUrl(url, SiteObj.DomainName, URLHelper.GetApplicationUrl(SiteObj.DomainName), null);
        }

        #endregion


        #region "Helper methods"


        /// <summary>
        /// Returns URL for the attachment specified by arguments.
        /// </summary>
        /// <param name="height">Height of the attachment</param>
        /// <param name="width">Width of the attachment</param>
        /// <param name="maxSideSize">Maximum size of the item if attachment is image</param>
        /// <param name="argTable">Source attachment data</param>
        private string GetAttachmentUrl(int height, int width, int maxSideSize, Hashtable argTable)
        {
            // Get information from argument
            var attachmentGuid = ValidationHelper.GetGuid(argTable["attachmentguid"], Guid.Empty);
            var attachmentName = argTable["name"].ToString();
            var nodeAliasPath = string.Empty;
            if (TreeNodeObj != null)
            {
                nodeAliasPath = TreeNodeObj.NodeAliasPath;
            }

            // Get item URL
            return GetAttachmentItemUrl(attachmentGuid, attachmentName, nodeAliasPath, height, width, maxSideSize);
        }


        /// <summary>
        /// Returns URL for the metafile specified by arguments.
        /// </summary>
        /// <param name="height">Height parameter that should be added to the URL</param>
        /// <param name="width">Width parameter that should be added to the URL</param>
        /// <param name="maxSideSize">Maximum size of the item if metafile is image</param>
        /// /// <param name="argTable">Source meta file data</param>
        private string GetMetaFileUrl(int height, int width, int maxSideSize, Hashtable argTable)
        {
            var attachmentGuid = ValidationHelper.GetGuid(argTable["metafileguid"], Guid.Empty);
            var attachmentName = argTable["name"].ToString();

            // Get item URL
            return GetMetaFileItemUrl(attachmentGuid, attachmentName, height, width, maxSideSize);
        }

        #endregion
    }
}
