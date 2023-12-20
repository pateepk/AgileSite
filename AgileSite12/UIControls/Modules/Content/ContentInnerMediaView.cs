using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Web.UI;
using CMS.Globalization;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for inner media view for content dialog
    /// </summary>
    public abstract class ContentInnerMediaView : CMSUserControl
    {
        #region "Constants"

        private const double MaxThumbImgWidth = 160.0;
        private const double MaxThumbImgHeight = 95.0;

        private const string CONTENT_FOLDER = "~/CMSModules/Content/";
        private const string CONTENT_UPLOADER_FILE = CONTENT_FOLDER + "Controls/Attachments/DirectFileUploader/DirectFileUploader.ascx";

        #endregion


        #region "Private variables"

        private OutputFormatEnum mOutputFormat = OutputFormatEnum.HTMLMedia;
        private DialogConfiguration mConfig;
        private DialogViewModeEnum mViewMode = DialogViewModeEnum.ListView;

        private SelectableContentEnum mSelectableContent = SelectableContentEnum.AllContent;
        private SelectablePageTypeEnum mSelectablePageTypes = SelectablePageTypeEnum.All;

        private MediaSourceEnum mSourceType = MediaSourceEnum.Content;

        private string mFileIdColumn = "";
        private string mFileNameColumn = "";
        private string mFileExtensionColumn = "";
        private string mFileSizeColumn = "";
        private string mFileWidthColumn = "";
        private string mFileHeightColumn = "";
        private string mAllowedExtensions = "";
        private string mInfoText = "";
        private string mGMTTooltip;

        private string siteName = String.Empty;
        private int mVersionHistoryId = -1;
        private int mLastSiteId;
        private int mObjectSiteID = -1;
        private int mUpdateIconPanelWidth = 16;

        /// <summary>
        /// Specifies if the update column is visible
        /// </summary>
        protected bool columnUpdateVisible;

        private SafeDictionary<int, IDataContainer> mFileData;
        private static DocumentAttachment mEmptyAttachment;

        #endregion


        #region "Events & delegates"

        /// <summary>
        /// Delegate for an event occurring when argument set is required.
        /// </summary>
        /// <param name="data">DataRow holding information on currently processed file</param>    
        public delegate string OnGetArgumentSet(IDataContainer data);

        /// <summary>
        /// Event occurring when argument set is required.
        /// </summary>
        public event OnGetArgumentSet GetArgumentSet;

        /// <summary>
        /// Delegate for the event fired when URL for list image is required.
        /// </summary>
        /// <param name="data">DataRow holding information on currently processed file</param>   
        /// <param name="isPreview">Indicates whether the image is generated as part of preview</param>
        public delegate string OnGetListItemUrl(IDataContainer data, bool isPreview);


        /// <summary>
        /// Event occurring when URL for list item image is required.
        /// </summary>
        public event OnGetListItemUrl GetListItemUrl;


        /// <summary>
        /// Delegate for the event fired when URL for content item based on node is required.
        /// </summary>
        /// <param name="node">Tree node to create presentation URL for.</param>
        public delegate string OnGetContentItemUrl(TreeNode node);


        /// <summary>
        /// Event occurring when URL for content item is required.
        /// </summary>
        public event OnGetContentItemUrl GetContentItemUrl;


        /// <summary>
        /// Delegate for the event fired when URL for thumbnails image is required.
        /// </summary>
        /// <param name="data">DataRow holding information on currently processed file</param>  
        /// <param name="isPreview">Indicates whether the image is generated as part of preview</param>
        /// <param name="width">Width of preview image</param>
        /// <param name="maxSideSize">Maximum size of the preview image. If full-size required parameter gets zero value</param>
        /// <param name="height">Height of preview image</param>
        /// <param name="extension">File extension</param>
        public delegate IconParameters OnGetThumbsItemUrl(IDataContainer data, bool isPreview, int height, int width, int maxSideSize, string extension);

        /// <summary>
        /// Event occurring when URL for thumbnails image is required.
        /// </summary>
        public event OnGetThumbsItemUrl GetThumbsItemUrl;

        /// <summary>
        /// Delegate for the event occurring when information on file import status is required.
        /// </summary>
        /// <param name="type">Type of the required information</param>
        /// <param name="parameter">Parameter related</param>
        public delegate object OnGetInformation(string type, object parameter);

        /// <summary>
        /// Event occurring when information on file import status is required.
        /// </summary>
        public event OnGetInformation GetInformation;

        /// <summary>
        /// Delegate for the event occurring when permission modify is required.
        /// </summary>
        /// <param name="data">DataRow holding information on currently processed file</param>
        public delegate bool OnGetModifyPermission(IDataContainer data);

        /// <summary>
        /// Event occurring when permission modify is required.
        /// </summary>
        public event OnGetModifyPermission GetModifyPermission;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets a UniGrid control used to display files in LIST view mode.
        /// </summary>
        public abstract UniGrid ListViewControl
        {
            get;
        }


        /// <summary>
        /// Gets a repeater control used to display files in THUMBNAILS view mode.
        /// </summary>
        public abstract BasicRepeater ThumbnailsViewControl
        {
            get;
        }


        /// <summary>
        /// Gets a page size drop-down list control used THUMBNAILS view mode.
        /// </summary>
        public abstract DropDownList PageSizeDropDownList
        {
            get;
        }


        /// <summary>
        /// Gets currently selected page size.
        /// </summary>
        public abstract int CurrentPageSize
        {
            get;
        }


        /// <summary>
        /// Gets currently selected page size.
        /// </summary>
        public abstract int CurrentOffset
        {
            get;
        }


        /// <summary>
        /// Gets or sets currently selected page.
        /// </summary>
        public abstract int CurrentPage
        {
            get;
            set;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the OutputFormat (needed for correct dialog type recognition).
        /// </summary>
        public OutputFormatEnum OutputFormat
        {
            get
            {
                return mOutputFormat;
            }
            set
            {
                mOutputFormat = value;
            }
        }


        /// <summary>
        /// Gets current dialog configuration.
        /// </summary>
        public DialogConfiguration Config
        {
            get
            {
                return mConfig ?? (mConfig = DialogConfiguration.GetDialogConfiguration());
            }
        }


        /// <summary>
        /// Gets or sets a view mode used to display files.
        /// </summary>
        public DialogViewModeEnum ViewMode
        {
            get
            {
                return mViewMode;
            }
            set
            {
                mViewMode = value;
            }
        }


        /// <summary>
        /// Gets or sets source of the data for view controls.
        /// </summary>
        public DataSet DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets total records count for pagination.
        /// </summary>
        public int TotalRecords
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the content which can be selected.
        /// </summary>
        public SelectableContentEnum SelectableContent
        {
            get
            {
                return mSelectableContent;
            }
            set
            {
                mSelectableContent = value;
            }
        }


        /// <summary>
        /// Page types which can be selected
        /// </summary>
        public SelectablePageTypeEnum SelectablePageTypes
        {
            get
            {
                return mSelectablePageTypes;
            }
            set
            {
                mSelectablePageTypes = value;
            }
        }


        /// <summary>
        /// Gets or sets name of the column holding information on the file identifier.
        /// </summary>
        public string FileIdColumn
        {
            get
            {
                return mFileIdColumn;
            }
            set
            {
                mFileIdColumn = value;
            }
        }

        /// <summary>
        /// Gets or sets name of the column holding information on file name.
        /// </summary>
        public string FileNameColumn
        {
            get
            {
                return mFileNameColumn;
            }
            set
            {
                mFileNameColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets name of the column holding information on file extension.
        /// </summary>
        public string FileExtensionColumn
        {
            get
            {
                return mFileExtensionColumn;
            }
            set
            {
                mFileExtensionColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets name of the column holding information on file width.
        /// </summary>
        public string FileWidthColumn
        {
            get
            {
                return mFileWidthColumn;
            }
            set
            {
                mFileWidthColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets name of the column holding information on file height.
        /// </summary>
        public string FileHeightColumn
        {
            get
            {
                return mFileHeightColumn;
            }
            set
            {
                mFileHeightColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets name of the column holding information on file size.
        /// </summary>
        public string FileSizeColumn
        {
            get
            {
                return mFileSizeColumn;
            }
            set
            {
                mFileSizeColumn = value;
            }
        }


        /// <summary>
        /// Gets or sets text of the information label.
        /// </summary>
        public string InfoText
        {
            get
            {
                return mInfoText;
            }
            set
            {
                mInfoText = value;
            }
        }


        /// <summary>
        /// Gets the node attachments are related to.
        /// </summary>
        public TreeNode TreeNodeObj
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets information on source type.
        /// </summary>
        public MediaSourceEnum SourceType
        {
            get
            {
                return mSourceType;
            }
            set
            {
                mSourceType = value;
            }
        }


        /// <summary>
        /// Height of attachment.
        /// </summary>
        public int ResizeToHeight
        {
            get;
            set;
        }


        /// <summary>
        /// Width of attachment.
        /// </summary>
        public int ResizeToWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Max side size of attachment.
        /// </summary>
        public int ResizeToMaxSideSize
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if full listing mode is enabled. This mode enables navigation to child and parent folders/documents from current view.
        /// </summary>
        public bool IsFullListingMode
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IsFullListingMode"], false);
            }
            set
            {
                ViewState["IsFullListingMode"] = value;
            }
        }


        /// <summary>
        /// Indicates whether the control is displayed as part of the copy/move dialog.
        /// </summary>
        public bool IsCopyMoveLinkDialog
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the related node version history.
        /// </summary>
        private int VersionHistoryID
        {
            get
            {
                if (mVersionHistoryId < 0)
                {
                    mVersionHistoryId = 0;

                    // Load the version history
                    if (TreeNodeObj != null)
                    {
                        // Get the node workflow
                        WorkflowManager wm = WorkflowManager.GetInstance(TreeNodeObj.TreeProvider);
                        WorkflowInfo wi = wm.GetNodeWorkflow(TreeNodeObj);
                        if (wi != null)
                        {
                            // Ensure the document version
                            VersionManager vm = VersionManager.GetInstance(TreeNodeObj.TreeProvider);
                            VersionHistoryID = vm.EnsureVersion(TreeNodeObj, TreeNodeObj.IsPublished);
                        }
                    }
                }

                return mVersionHistoryId;
            }
            set
            {
                mVersionHistoryId = value;
            }
        }

        #endregion


        #region "Attachment properties"

        /// <summary>
        /// Empty attachment object to fake attachment data
        /// </summary>
        private static DocumentAttachment EmptyAttachment
        {
            get
            {
                return mEmptyAttachment ?? (mEmptyAttachment = new DocumentAttachment());
            }
        }


        /// <summary>
        /// Gets all allowed extensions.
        /// </summary>
        public string AllowedExtensions
        {
            get
            {
                if (mAllowedExtensions == "")
                {
                    mAllowedExtensions = QueryHelper.GetString("allowedextensions", "");
                    if (mAllowedExtensions == "")
                    {
                        mAllowedExtensions = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSUploadExtensions");
                    }
                }
                return mAllowedExtensions;
            }
            private set
            {
                mAllowedExtensions = value;
            }
        }


        /// <summary>
        /// Gets or sets ID of the parent node.
        /// </summary>
        public int NodeParentID
        {
            get;
            set;
        }

        #endregion


        #region "Mass actions methods"

        /// <summary>
        /// Ensures given file name in the way it is usable as ID.
        /// </summary>
        /// <param name="fileName">Name of the file to ensure</param>
        private static string EnsureFileName(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                char[] specialChars = "#;&,.+*~':\"!^$[]()=>|/\\-%@`{}".ToCharArray();
                foreach (char specialChar in specialChars)
                {
                    fileName = fileName.Replace(specialChar, '_');
                }
                return fileName.Replace(" ", "").ToLowerCSafe();
            }

            return fileName;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes all the necessary JavaScript blocks.
        /// </summary>
        protected void InitializeControlScripts(string colorizeElementId)
        {
            // Dialog for editing image and non-image
            string urlImage;
            string urlMeta;

            if (SourceType == MediaSourceEnum.MediaLibraries)
            {
                const string MEDIA_LIBRARY_FOLDER = "~/CMSModules/MediaLibrary/";

                if (IsLiveSite)
                {
                    const string MEDIA_PAGES_FOLDER = MEDIA_LIBRARY_FOLDER + "CMSPages/";

                    var editorUrl = AuthenticationHelper.IsAuthenticated() ? MEDIA_PAGES_FOLDER + "ImageEditor.aspx" : MEDIA_PAGES_FOLDER + "PublicImageEditor.aspx";

                    urlImage = ResolveUrl(editorUrl);
                    urlMeta = ApplicationUrlHelper.ResolveDialogUrl(MEDIA_PAGES_FOLDER + "MetaDataEditor.aspx");
                }
                else
                {
                    urlImage = ResolveUrl(MEDIA_LIBRARY_FOLDER + "Controls/MediaLibrary/ImageEditor.aspx");
                    urlMeta = ApplicationUrlHelper.ResolveDialogUrl(MEDIA_LIBRARY_FOLDER + "Dialogs/MetaDataEditor.aspx");
                }
            }
            else
            {
                if (IsLiveSite)
                {
                    const string LIVE_SELECTORS_FOLDER = "~/CMSFormControls/LiveSelectors/";

                    var editorUrl = AuthenticationHelper.IsAuthenticated() ? LIVE_SELECTORS_FOLDER + "ImageEditor.aspx" : LIVE_SELECTORS_FOLDER + "PublicImageEditor.aspx";

                    urlImage = ResolveUrl(editorUrl);
                    urlMeta = ApplicationUrlHelper.ResolveDialogUrl(CONTENT_FOLDER + "Attachments/CMSPages/MetaDataEditor.aspx");
                }
                else
                {
                    urlImage = ResolveUrl(CONTENT_FOLDER + "CMSDesk/Edit/ImageEditor.aspx");
                    urlMeta = ApplicationUrlHelper.ResolveDialogUrl(CONTENT_FOLDER + "Attachments/Dialogs/MetaDataEditor.aspx");
                }
            }

            if (SourceType == MediaSourceEnum.MetaFile)
            {
                urlMeta = ApplicationUrlHelper.ResolveDialogUrl("~/CMSModules/AdminControls/Controls/MetaFiles/MetaDataEditor.aspx");
            }

            string script = String.Format(
    @"
var attemptNo = 0;

function ColorizeRow(itemId) {{
    if (itemId != null)
    {{
        var hdnField = document.getElementById('{0}');
        if (hdnField != null)
        {{
            // If some item was previously selected
            if ((hdnField.value != null) && (hdnField.value != ''))
            {{
                // Get selected item and reset its selection
                var lastColorizedElem = document.getElementById(hdnField.value);
                if (lastColorizedElem != null)
                {{
                    ColorizeElement(lastColorizedElem, true);
                }}
             }}            
             // Update field value
             hdnField.value = itemId;
        }}
        // Colorize currently selected item
        var elem = document.getElementById(itemId);
        if (elem != null)
        {{
            ColorizeElement(elem, false);
            attemptNo = 0;
        }}
        else
        {{
            if(attemptNo < 1)
            {{
                setTimeout('ColorizeRow(\'' + itemId + '\')', 300);
                attemptNo = attemptNo + 1;
            }}
            else
            {{
                attemptNo = 0;
            }}
        }}
     }}
 }}

 function ColorizeLastRow() {{
    var hdnField = document.getElementById('{0}');
    if (hdnField != null)
    {{
        // If some item was previously selected
        if ((hdnField.value != null) && (hdnField.value != ''))
        {{               
            // Get selected item and reset its selection
            var lastColorizedElem = document.getElementById(hdnField.value);
            if (lastColorizedElem != null)
            {{
                ColorizeElement(lastColorizedElem, false);
            }}
        }}
    }}
}}

function ColorizeElement(elem, clear) {{
    if(!clear){{
        elem.className += ' Selected';
    }}
    else {{
        elem.className = elem.className.replace(' Selected','');
    }}
}}

function ClearColorizedRow()
{{
    var hdnField = document.getElementById('{0}');
    if (hdnField != null) 
    {{
        // If some item was previously selected
        if ((hdnField.value != null) && (hdnField.value != '')) 
        {{               
            // Get selected item and reset its selection
            var lastColorizedElem = document.getElementById(hdnField.value);
            if (lastColorizedElem != null) 
            {{   
                ColorizeElement(lastColorizedElem, false);

                // Update field value
                hdnField.value = '';                                    
            }}
        }}
    }}                                
}}

function EditImage(param) {{
    var form = '';
    if (param.indexOf('?') != 0) {{ 
        param = '?' + param; 
    }}
    modalDialog('{1}' + param, 'imageEditorDialog', 905, 670, undefined, true); 
    return false; 
}}

function Edit(param) {{
    var form = '';
    if (param.indexOf('?') != 0) {{ 
        param = '?' + param; 
    }}
    modalDialog('{2}' + param, 'editorDialog', 700, 400, undefined, true); 
    return false; 
}}
",
                colorizeElementId,
                urlImage,
                urlMeta
            );

            ScriptHelper.RegisterStartupScript(this, GetType(), "DialogsScript", script, true);
        }


        /// <summary>
        /// Loads the external file data for displayed items
        /// </summary>
        protected void LoadFileData()
        {
            // Get the data for attachments
            if (SourceType == MediaSourceEnum.Content)
            {
                var dt = DataHelper.GetDataTable(DataSource);
                if (dt != null)
                {
                    var documentIds = DataHelper.GetIntegerValues(dt, "DocumentID");
                    var versionHistoryIds = DataHelper.GetIntegerValues(dt, "DocumentCheckedOutVersionHistoryID");

                    mFileData = DocumentHelper.GetPrimaryAttachmentsForDocuments(documentIds, !IsLiveSite, versionHistoryIds);
                }
            }
        }


        /// <summary>
        /// Returns the sitename according to item info.
        /// </summary>
        /// <param name="data">Row containing information on the current item</param>
        /// <param name="isMediaFile">Indicates whether the file is media file</param>
        private string GetSiteName(IDataContainer data, bool isMediaFile)
        {
            int siteId = 0;
            string result = "";

            if (isMediaFile)
            {
                if (data.ContainsColumn("FileSiteID"))
                {
                    // Imported media file
                    siteId = ValidationHelper.GetInteger(data.GetValue("FileSiteID"), 0);
                }
                else
                {
                    // Not imported yet
                    siteId = RaiseOnSiteIdRequired();
                }
            }
            else
            {
                if (data.ContainsColumn("NodeSiteID"))
                {
                    // Content file
                    result = SiteInfoProvider.GetSiteName(ValidationHelper.GetInteger(data.GetValue("NodeSiteID"), 0));
                }
                else
                {
                    if (data.ContainsColumn("AttachmentSiteID"))
                    {
                        // Non-versioned attachment
                        siteId = ValidationHelper.GetInteger(data.GetValue("AttachmentSiteID"), 0);
                    }
                    else if (TreeNodeObj != null)
                    {
                        // Versioned attachment
                        siteId = TreeNodeObj.NodeSiteID;
                    }
                    else if (data.ContainsColumn("MetaFileSiteID"))
                    {
                        // Metafile
                        siteId = ValidationHelper.GetInteger(data.GetValue("MetaFileSiteID"), 0);
                    }
                }
            }

            if (result == "")
            {
                if (String.IsNullOrEmpty(siteName) || (mLastSiteId != siteId))
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
                    if (si != null)
                    {
                        mLastSiteId = si.SiteID;
                        result = si.SiteName;
                        siteName = result;
                    }
                }
                else
                {
                    result = siteName;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the object site ID
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectID">Object ID</param>
        private int GetObjectSiteID(string objectType, int objectID)
        {
            if (mObjectSiteID == -1)
            {
                BaseInfo info = ProviderHelper.GetInfoById(objectType, objectID);

                mObjectSiteID = info != null ? info.Generalized.ObjectSiteID : SiteContext.CurrentSiteID;
            }

            return mObjectSiteID;
        }


        /// <summary>
        /// Initializes attachment update control according current attachment data.
        /// </summary>
        /// <param name="dfuElem">Direct file uploader</param>
        /// <param name="data">Data container holding attachment data</param>
        private void GetAttachmentUpdateControl(ref DirectFileUploader dfuElem, IDataContainer data)
        {
            if (dfuElem != null)
            {
                string refreshType = CMSDialogHelper.GetMediaSource(SourceType);
                Guid formGuid = Guid.Empty;
                int documentId = ValidationHelper.GetInteger(data.GetValue("AttachmentDocumentID"), 0);

                // If attachment is related to the workflow 'AttachmentFormGUID' information isn't present
                if (data.ContainsColumn("AttachmentFormGUID"))
                {
                    formGuid = ValidationHelper.GetGuid(data.GetValue("AttachmentFormGUID"), Guid.Empty);
                }

                if (SourceType == MediaSourceEnum.MetaFile)
                {
                    dfuElem.ObjectID = Config.MetaFileObjectID;
                    dfuElem.ObjectType = Config.MetaFileObjectType;
                    dfuElem.Category = Config.MetaFileCategory;

                    dfuElem.SiteID = GetObjectSiteID(Config.MetaFileObjectType, Config.MetaFileObjectID);

                    dfuElem.SourceType = MediaSourceEnum.MetaFile;
                    dfuElem.MetaFileID = ValidationHelper.GetInteger(data.GetValue("MetaFileID"), 0);
                }
                else
                {
                    dfuElem.SourceType = MediaSourceEnum.DocumentAttachments;
                    dfuElem.FormGUID = formGuid;
                    dfuElem.DocumentID = documentId;
                    if (TreeNodeObj != null)
                    {
                        // if attachment node exists
                        dfuElem.NodeParentNodeID = TreeNodeObj.NodeParentID;
                        dfuElem.NodeClassName = TreeNodeObj.NodeClassName;
                    }
                    else
                    {
                        // if attachment node doesn't exist
                        dfuElem.NodeParentNodeID = NodeParentID;
                        dfuElem.NodeClassName = SystemDocumentTypes.File;
                    }
                    dfuElem.CheckPermissions = true;
                    dfuElem.AttachmentGUID = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);
                    dfuElem.ResizeToWidth = ResizeToWidth;
                    dfuElem.ResizeToHeight = ResizeToHeight;
                    dfuElem.ResizeToMaxSideSize = ResizeToMaxSideSize;
                    dfuElem.AllowedExtensions = AllowedExtensions;
                }

                dfuElem.ParentElemID = refreshType;
                dfuElem.ID = "dfuElem";
                dfuElem.ForceLoad = true;
                dfuElem.ControlGroup = "MediaView";
                dfuElem.ShowIconMode = true;
                dfuElem.InsertMode = false;
                dfuElem.IncludeNewItemInfo = true;
                dfuElem.IsLiveSite = IsLiveSite;

                // Setting of the direct single mode
                dfuElem.UploadMode = MultifileUploaderModeEnum.DirectSingle;
                dfuElem.MaxNumberToUpload = 1;
            }
        }


        /// <summary>
        /// Initializes upload control.
        /// When data is null, the control can be rendered as disabled only.
        /// </summary>
        /// <param name="dfuElem">Upload control to initialize</param>
        /// <param name="data">Data row with data on related media file</param>
        public void GetLibraryUpdateControl(ref DirectFileUploader dfuElem, IDataContainer data)
        {
            if (dfuElem != null)
            {
                if (data != null)
                {
                    string dataSiteName = GetSiteName(data, true);

                    int fileId = ValidationHelper.GetInteger(data.GetValue("FileID"), 0);
                    string fileName = EnsureFileName(Path.GetFileName(ValidationHelper.GetString(data.GetValue("FilePath"), "")));
                    string folderPath = Path.GetDirectoryName(ValidationHelper.GetString(data.GetValue("FilePath"), ""));
                    int libraryId = ValidationHelper.GetInteger(data.GetValue("FileLibraryID"), 0);

                    AllowedExtensions = SettingsKeyInfoProvider.GetValue(dataSiteName + ".CMSMediaFileAllowedExtensions");

                    // Initialize library info
                    dfuElem.LibraryID = libraryId;
                    dfuElem.MediaFileID = fileId;
                    dfuElem.MediaFileName = fileName;
                    dfuElem.LibraryFolderPath = folderPath;
                }

                // Initialize general info
                dfuElem.CheckPermissions = true;
                dfuElem.SourceType = MediaSourceEnum.MediaLibraries;
                dfuElem.ID = "dfuElemLib";
                dfuElem.ForceLoad = true;
                dfuElem.DisplayInline = true;
                dfuElem.ControlGroup = "MediaView";
                dfuElem.ResizeToWidth = ResizeToWidth;
                dfuElem.ResizeToHeight = ResizeToHeight;
                dfuElem.ResizeToMaxSideSize = ResizeToMaxSideSize;
                dfuElem.AllowedExtensions = AllowedExtensions;
                dfuElem.ShowIconMode = true;
                dfuElem.InsertMode = false;
                dfuElem.ParentElemID = "LibraryUpdate";
                dfuElem.IncludeNewItemInfo = true;
                dfuElem.RaiseOnClick = true;
                dfuElem.IsLiveSite = IsLiveSite;

                // Setting of the direct single mode
                dfuElem.UploadMode = MultifileUploaderModeEnum.DirectSingle;
                dfuElem.Width = 16;
                dfuElem.Height = 16;
                dfuElem.MaxNumberToUpload = 1;
            }
        }


        /// <summary>
        /// Returns correct ID for the item (for colorizing the item when selected).
        /// </summary>
        /// <param name="dataItem">Container.DataItem</param>
        protected string GetID(object dataItem)
        {
            var dr = dataItem as DataRowView;
            if (dr != null)
            {
                IDataContainer data = new DataRowContainer(dr);

                data = GetCompleteData(data);

                return GetColorizeID(data);
            }

            return "";
        }


        /// <summary>
        /// Returns correct ID for the given item (for colorizing the item when selected).
        /// </summary>
        /// <param name="data">Item to get the ID of</param>
        protected string GetColorizeID(IDataContainer data)
        {
            string id = data.ContainsColumn(FileIdColumn) ?
                            ValidationHelper.GetString(data.GetValue(FileIdColumn), "") :
                            EnsureFileName(ValidationHelper.GetString(data.GetValue("FileName"), ""));

            if (String.IsNullOrEmpty(id))
            {
                // Content file
                id = data.GetValue("NodeGUID").ToString();
            }

            return id.ToLowerCSafe();
        }


        /// <summary>
        /// Gets file name according available columns.
        /// </summary>
        /// <param name="data">DataRow containing data</param>
        private string GetFileName(IDataContainer data)
        {
            string fileName = "";

            if (data != null)
            {
                if (data.ContainsColumn("FileExtension"))
                {
                    fileName = String.Concat(data.GetValue("FileName"), data.GetValue("FileExtension"));
                }
                else
                {
                    fileName = data.GetValue(FileNameColumn).ToString();
                }
            }

            return fileName;
        }


        /// <summary>
        /// Ensures correct format of extension being displayed.
        /// </summary>
        /// <param name="extension">Extension to normalize</param>
        private static string NormalizeExtenison(string extension)
        {
            if (!string.IsNullOrEmpty(extension))
            {
                if (extension.Trim() != "<dir>")
                {
                    extension = "." + extension.ToLowerCSafe().TrimStart('.');
                }
                else
                {
                    extension = HTMLHelper.HTMLEncode("<DIR>");
                }
            }

            return extension;
        }


        /// <summary>
        /// Gets title text.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="isContentFile">If true, the file is a content file</param>
        private string GetTitle(IDataContainer data, bool isContentFile)
        {
            string title = null;
            switch (SourceType)
            {
                case MediaSourceEnum.Attachment:
                case MediaSourceEnum.DocumentAttachments:
                    title = ValidationHelper.GetString(data.GetValue("AttachmentTitle"), null);
                    break;

                case MediaSourceEnum.Content:
                    if (isContentFile)
                    {
                        title = ValidationHelper.GetString(data.GetValue("AttachmentTitle"), null);
                    }
                    break;

                case MediaSourceEnum.MediaLibraries:
                    title = ValidationHelper.GetString(data.GetValue("FileTitle"), null);
                    break;

                case MediaSourceEnum.MetaFile:
                    title = ValidationHelper.GetString(data.GetValue("MetaFileTitle"), null);
                    break;
            }

            return title;
        }


        /// <summary>
        /// Gets description text.
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="isContentFile">If true, the file is a content file</param>
        private string GetDescription(IDataContainer data, bool isContentFile)
        {
            string desc = null;
            switch (SourceType)
            {
                case MediaSourceEnum.Attachment:
                case MediaSourceEnum.DocumentAttachments:
                    desc = ValidationHelper.GetString(data.GetValue("AttachmentDescription"), null);
                    break;

                case MediaSourceEnum.Content:
                    if (isContentFile)
                    {
                        desc = ValidationHelper.GetString(data.GetValue("AttachmentDescription"), null);
                    }
                    break;

                case MediaSourceEnum.MediaLibraries:
                    desc = ValidationHelper.GetString(data.GetValue("FileDescription"), null);
                    break;

                case MediaSourceEnum.MetaFile:
                    desc = ValidationHelper.GetString(data.GetValue("MetaFileDescription"), null);
                    break;
            }

            return desc;
        }

        #endregion


        #region "List view methods"

        /// <summary>
        /// Generates SetAction script based on parameters.
        /// </summary>
        /// <param name="actionParameter">Action name</param>
        /// <param name="fileName">File name</param>
        private string GenerateSetActionScript(string actionParameter, string fileName)
        {
            const string setActionScript = "SetAction('{0}', {1}); RaiseHiddenPostBack(); return false;";
           
            return  String.Format(setActionScript, actionParameter, ScriptHelper.GetString(fileName));
        }


        /// <summary>
        /// Returns panel with image according extension of the processed file.
        /// </summary>
        /// <param name="ext">Extension of the file used to determine icon</param>
        /// <param name="className">Class name</param>
        /// <param name="item">Control inserted as a file name</param>
        /// <param name="previewUrl">Preview URL</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="isSelectable">Determines whether it can be selected</param>
        /// <param name="generateTooltip">Determines if tooltip should be generated</param>
        /// <param name="title">File title</param>
        /// <param name="description">File description</param>
        /// <param name="name">File name without extension</param>
        private Panel GetListItem(string ext, string className, string previewUrl, int width, int height, Control item, bool isSelectable, bool generateTooltip, string title, string description, string name)
        {
            var pnl = new Panel
            {
                CssClass = "DialogListItem" + (isSelectable ? "" : "Unselectable")
            };
            pnl.Controls.Add(new LiteralControl("<div class=\"DialogListItemNameRow\">"));

            // Process media library folder
            if ((SourceType == MediaSourceEnum.MediaLibraries) && ext.ToLowerCSafe() == "<dir>")
            {
                className = "icon-folder";
            }

            // Prepare image
            if (className.EqualsCSafe(SystemDocumentTypes.File, true) || (className == ""))
            {
                var docImg = new Label();
                docImg.Text = UIHelper.GetFileIcon(Page, ext);

                // Tooltip
                if (generateTooltip)
                {
                    UIHelper.EnsureTooltip(docImg, previewUrl, width, height, title, name, ext, description, null, 300);
                }
                pnl.Controls.Add(docImg);
            }
            else
            {
                string icon;
                var ci = DataClassInfoProvider.GetDataClassInfo(className);
                if (ci != null)
                {
                    var iconClass = ValidationHelper.GetString(ci.GetValue("ClassIconClass"), String.Empty);
                    icon = UIHelper.GetDocumentTypeIcon(Page, className, iconClass);
                }
                else
                {
                    icon = UIHelper.GetAccessibleIconTag(className);
                }

                pnl.Controls.Add(new LiteralControl(icon));
            }

            if ((isSelectable) && (item is LinkButton))
            {
                // Create clickabe compelte panel
                pnl.Attributes["onclick"] = ((LinkButton)item).Attributes["onclick"];
                ((LinkButton)item).Attributes["onclick"] = null;
            }

            // Add file name                  
            pnl.Controls.Add(new LiteralControl(String.Format("<span class=\"DialogListItemName\" {0}>", (!isSelectable ? "style=\"cursor:default;\"" : ""))));
            pnl.Controls.Add(item);
            pnl.Controls.Add(new LiteralControl("</span></div>"));
            return pnl;
        }


        /// <summary>
        /// Item data bound handler for grid view
        /// </summary>
        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var dr = (e.Row.DataItem as DataRowView);
                if (dr != null)
                {
                    IDataContainer data = new DataRowContainer(dr);

                    // Apply file data if necessary
                    data = GetCompleteData(data);

                    e.Row.Attributes["id"] = GetColorizeID(data);
                }
            }
        }


        /// <summary>
        /// Item data bound handler for list view
        /// </summary>
        protected object ListViewControl_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            object result = null;
            string argument;
            TreeNode node = null;

            sourceName = sourceName.ToLowerCSafe();

            // Prepare the data
            IDataContainer data = null;
            if (parameter is DataRowView)
            {
                var sourceData = (DataRowView)parameter;
                data = new DataRowContainer(sourceData);
                node = TreeNode.New(sourceData.Row);
            }
            else
            {
                // Get the data row view from parameter
                GridViewRow gvr = (parameter as GridViewRow);
                if (gvr != null)
                {
                    DataRowView dr = (DataRowView)gvr.DataItem;
                    if (dr != null)
                    {
                        data = new DataRowContainer(dr);
                        node = TreeNode.New(dr.Row);
                    }
                }
            }

            if (data == null)
            {
                return parameter;
            }

            // Apply file data if necessary
            data = GetCompleteData(data);

            CMSGridActionButton btnAction;
            DirectFileUploader dfuElem;

            string fileName;
            bool isLibraryFolder = (SourceType == MediaSourceEnum.MediaLibraries) && IsFullListingMode && (data.GetValue(FileExtensionColumn).ToString().ToLowerCSafe() == "<dir>");
            string className = ValidationHelper.GetString(data.GetValue("ClassName"), "");
            string classDisplayName = ValidationHelper.GetString(data.GetValue("ClassDisplayName"), "");
            bool isContent = (SourceType == MediaSourceEnum.Content);

            // Current item is CMS.File
            var isContentFile = className.EqualsCSafe(SystemDocumentTypes.File, true);
            // CMS.File has attachment defined (view action, size, ...) should be available for this object
            var fileDocumentWithAttachment = isContentFile && (ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty) != Guid.Empty);
            // Object which does not have attachment defined (view action will not be available)
            var isContentItem = isContent && !fileDocumentWithAttachment;
            // CMS.File with attachment defined, attachment, meta file or media file object
            var isAttachment = !isContent || fileDocumentWithAttachment;
            var ext = HTMLHelper.HTMLEncode(isAttachment ? data.GetValue(FileExtensionColumn).ToString().TrimStart('.') : "");

            Func<string> getUrl = () => (isAttachment && !isContentFile) ? RaiseOnGetAttachmentUrl(data, false) : RaiseOnGetContentItemUrl(node);

            int siteId = GetSiteID(data);
            var siteInfo = GetSiteInfo(siteId);
            bool siteIsRunning = (siteInfo.Status == SiteStatusEnum.Running);

            UserInfo userInfo = MembershipContext.AuthenticatedUser;

            switch (sourceName)
            {
                #region "Select"

                case "select":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction == null)
                    {
                        break;
                    }

                    // Get file name
                    fileName = GetFileName(data);

                    if ((SourceType == MediaSourceEnum.MediaLibraries) && ((RaiseOnFileIsNotInDatabase(fileName) == null) && (DisplayMode == ControlDisplayModeEnum.Simple)))
                    {
                        // If folders are displayed as well show SELECT button icon
                        if (isLibraryFolder && !IsCopyMoveLinkDialog)
                        {
                            btnAction.IconCssClass = "icon-arrow-crooked-right";
                            btnAction.ToolTip = GetString("dialogs.list.actions.showsubfolders");
                            btnAction.OnClientClick = String.Format("SetAction('morefolderselect', {0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(fileName));
                        }
                        else if (isLibraryFolder && IsCopyMoveLinkDialog)
                        {
                            btnAction.IconCssClass = "icon-chevron-right";
                            btnAction.ToolTip = GetString("general.select");
                            btnAction.OnClientClick = String.Format("SetAction('copymoveselect', {0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(fileName));
                        }
                        else
                        {
                            // If media file not imported yet - display warning sign
                            btnAction.IconCssClass = "icon-exclamation-triangle";
                            btnAction.IconStyle = GridIconStyle.Warning;
                            btnAction.ToolTip = GetString("media.file.import");
                            btnAction.OnClientClick = String.Format("SetAction('importfile', {0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(data.GetValue("FileName").ToString()));
                        }
                    }
                    else
                    {
                        var isSelectable = IsSelectable(data, ext, isContentFile);
                        if (!isSelectable)
                        {
                            // Disable the selection if not allowed
                            btnAction.Enabled = false;
                        }
                        else
                        {
                            argument = RaiseOnGetArgumentSet(data);

                            // Initialize command
                            btnAction.OnClientClick = String.Format("ColorizeRow({0}); SetSelectAction({1}); return false;", ScriptHelper.GetString(GetColorizeID(data)), ScriptHelper.GetString(argument + "|URL|" + getUrl()));

                            result = btnAction;
                        }
                    }
                    break;

                #endregion


                #region "Select sub docs"

                case "selectsubdocs":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction != null)
                    {
                        int nodeId = ValidationHelper.GetInteger(data.GetValue("NodeID"), 0);

                        if (IsFullListingMode)
                        {
                            // Check if item is selectable, if not remove select action button
                            // Initialize command
                            btnAction.OnClientClick = String.Format("SetParentAction('{0}'); return false;", nodeId);
                        }
                        else
                        {
                            btnAction.Visible = false;
                        }
                    }
                    break;

                #endregion


                #region "Select sub folders"

                case "selectsubfolders":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction == null)
                    {
                        break;
                    }

                    string folderName = ValidationHelper.GetString(data.GetValue("FileName"), "");

                    if (IsCopyMoveLinkDialog || (IsFullListingMode && (DisplayMode == ControlDisplayModeEnum.Default) && (ValidationHelper.GetString(data.GetValue(FileExtensionColumn), "") == "<dir>")))
                    {
                        // Initialize command
                        btnAction.OnClientClick = String.Format("SetLibParentAction({0}); return false;", ScriptHelper.GetString(folderName));
                    }
                    else
                    {
                        btnAction.Visible = false;
                    }
                    break;

                #endregion


                #region "View"

                case "view":
                    {
                        // Action is visible only for attachments or CMS.File document with defined attachment
                        btnAction = sender as CMSGridActionButton;
                        if (btnAction == null)
                        {
                            break;
                        }

                        if (isContentItem || isLibraryFolder || !siteIsRunning)
                        {
                            btnAction.Visible = false;
                            break;
                        }

                        string url = getUrl();
                        if (String.IsNullOrEmpty(url))
                        {
                            btnAction.Enabled = false;
                            break;
                        }

                        var finalUrl = url;
                        // Add latest version requirement for live site
                        if (IsLiveSite)
                        {
                            // Check version history ID
                            int versionHistoryId = VersionHistoryID;
                            if (versionHistoryId > 0)
                            {
                                // Add requirement for latest version of files for current document
                                string newparams = "latestforhistoryid=" + versionHistoryId;
                                newparams += "&hash=" + ValidationHelper.GetHashString("h" + versionHistoryId, new HashSettings(""));

                                finalUrl = URLHelper.AppendQuery(url, newparams);
                            }
                        }

                        finalUrl = UrlResolver.ResolveUrl(finalUrl);
                        btnAction.OnClientClick = String.Format("window.open({0}); return false;", ScriptHelper.GetString(finalUrl));
                    }
                    break;

                #endregion


                #region "Edit"

                case "edit":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction == null)
                    {
                        break;
                    }

                    if (isContentItem || isLibraryFolder || !siteIsRunning)
                    {
                        btnAction.Visible = false;
                        break;
                    }

                    Guid guid;
                    if (SourceType == MediaSourceEnum.MediaLibraries)
                    {
                        guid = ValidationHelper.GetGuid(data.GetValue("FileGUID"), Guid.Empty);

                        btnAction.ScreenReaderDescription = String.Format("{0}|MediaFileGUID={1}&sitename={2}", ext, guid, GetSiteName(data, true));
                        btnAction.PreRender += img_PreRender;
                        btnAction.IconCssClass = "icon-edit";
                        btnAction.IconStyle = GridIconStyle.Allow;
                    }
                    else
                    {
                        string nodeIdQuery = "";
                        if (SourceType == MediaSourceEnum.Content)
                        {
                            nodeIdQuery = "&nodeId=" + data.GetValue("NodeID");
                        }

                        // Get the node workflow
                        VersionHistoryID = ValidationHelper.GetInteger(data.GetValue("DocumentCheckedOutVersionHistoryID"), 0);
                        guid = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);
                        btnAction.ScreenReaderDescription = String.Format("{0}|AttachmentGUID={1}&sitename={2}{3}&versionHistoryId={4}", ext, guid, GetSiteName(data, false), nodeIdQuery, VersionHistoryID);
                        btnAction.PreRender += img_PreRender;
                        btnAction.IconCssClass = "icon-edit";
                        btnAction.IconStyle = GridIconStyle.Allow;
                    }

                    break;

                #endregion


                #region "External edit"

                case "extedit":
                    {
                        // Prepare the data
                        DataRowView dr = parameter as DataRowView;
                        if (dr != null)
                        {
                            data = new DataRowContainer(dr);
                        }

                        // Create placeholder
                        PlaceHolder plcUpd = new PlaceHolder();
                        plcUpd.ID = "plcUdateColumn";

                        Panel pnlBlock = new Panel();
                        pnlBlock.ID = "pnlBlock";

                        pnlBlock.CssClass = "TableCell";
                        plcUpd.Controls.Add(pnlBlock);

                        if (siteIsRunning)
                        {
                            if (ExternalEditHelper.LoadExternalEditControl(pnlBlock, GetFileType(SourceType), siteInfo.SiteName, data, IsLiveSite, TreeNodeObj) != null)
                            {
                                columnUpdateVisible = true;
                            }

                            return plcUpd;
                        }
                    }
                    break;

                #endregion


                #region "Edit library ui"

                case "editlibraryui":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction == null)
                    {
                        break;
                    }

                    // Get file name
                    fileName = GetFileName(data);

                    bool notInDatabase = ((RaiseOnFileIsNotInDatabase(fileName) == null) && (DisplayMode == ControlDisplayModeEnum.Simple));

                    if (isLibraryFolder && notInDatabase)
                    {
                        btnAction.Visible = false;
                    }
                    else if (notInDatabase)
                    {
                        btnAction.Enabled = false;
                    }
                    else
                    {
                        btnAction.OnClientClick = String.Format("$cmsj('#hdnFileOrigName').attr('value', {0}); SetAction('editlibraryui', {1}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(EnsureFileName(fileName)), ScriptHelper.GetString(fileName));
                    }
                    break;

                #endregion


                #region "Delete"

                case "delete":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction == null)
                    {
                        break;
                    }

                    // Get file name
                    fileName = GetFileName(data);

                    btnAction.ToolTip = GetString("general.delete");

                    if (((RaiseOnFileIsNotInDatabase(fileName) == null) && (DisplayMode == ControlDisplayModeEnum.Simple)))
                    {
                        if (isLibraryFolder)
                        {
                            btnAction.Visible = false;
                        }
                        else
                        {
                            btnAction.OnClientClick = String.Format("if(DeleteMediaFileConfirmation() == false){{return false;}} SetAction('deletefile',{0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(fileName));
                        }
                    }
                    else
                    {
                        btnAction.OnClientClick = String.Format("if(DeleteMediaFileConfirmation() == false){{return false;}} SetAction('deletefile',{0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(fileName));
                    }
                    break;

                #endregion


                #region "Name"

                case "name":
                    {
                        // Is current item CMS.File or attachment ?

                        // Get name and extension                
                        string fileNameColumn = FileNameColumn;
                        if (isContentItem)
                        {
                            fileNameColumn = "DocumentName";
                        }
                        string name = data.GetValue(fileNameColumn).ToString();

                        if ((SourceType == MediaSourceEnum.DocumentAttachments) || (SourceType == MediaSourceEnum.MetaFile))
                        {
                            name = Path.GetFileNameWithoutExtension(name);
                        }

                        // Width & height
                        int width = (data.ContainsColumn(FileWidthColumn) ? ValidationHelper.GetInteger(data.GetValue(FileWidthColumn), 0) : 0);
                        int height = (data.ContainsColumn(FileHeightColumn) ? ValidationHelper.GetInteger(data.GetValue(FileHeightColumn), 0) : 0);

                        string cmpltExt = (isContentItem ? "" : "." + ext);
                        fileName = (cmpltExt != "") ? name.Replace(cmpltExt, "") : name;

                        // Check if item is selectable
                        var isSelectable = IsSelectable(data, ext, isContentFile);
                        if (!isSelectable)
                        {
                            var ltlName = new LiteralControl(fileName);

                            // Get final panel
                            result = GetListItem(ext, className, "", width, height, ltlName, false, false, GetTitle(data, isContentFile), GetDescription(data, isContentFile), name);
                        }
                        else
                        {
                            // Make a file name link
                            LinkButton lnkBtn = new LinkButton
                            {
                                ID = "n",
                                Text = HTMLHelper.HTMLEncode(fileName)
                            };

                            fileName = GetFileName(data);

                            // Try to get imported file row
                            bool isImported = true;
                            IDataContainer importedData;
                            if (DisplayMode == ControlDisplayModeEnum.Simple)
                            {
                                importedData = RaiseOnFileIsNotInDatabase(fileName);

                                // Apply file data if necessary
                                importedData = GetCompleteData(importedData);

                                isImported = (importedData != null);
                            }
                            else
                            {
                                importedData = data;
                            }

                            if (isImported)
                            {
                                // Update WIDTH
                                if (importedData.ContainsColumn(FileWidthColumn))
                                {
                                    width = ValidationHelper.GetInteger(importedData[FileWidthColumn], 0);
                                }

                                // Update HEIGHT
                                if (importedData.ContainsColumn(FileHeightColumn))
                                {
                                    height = ValidationHelper.GetInteger(importedData[FileHeightColumn], 0);
                                }
                            }
                            else
                            {
                                importedData = data;
                            }

                            argument = RaiseOnGetArgumentSet(data);

                            string url = getUrl();
                            string previewUrl = (isAttachment && !isContentFile) ? RaiseOnGetAttachmentUrl(importedData, true) : url;

                            if (!String.IsNullOrEmpty(previewUrl))
                            {
                                // Add chset
                                string chset = Guid.NewGuid().ToString();
                                previewUrl = URLHelper.AddParameterToUrl(previewUrl, "chset", chset);
                            }

                            // Add latest version requirement for live site
                            if (!String.IsNullOrEmpty(previewUrl) && IsLiveSite)
                            {
                                int versionHistoryId = VersionHistoryID;
                                if (versionHistoryId > 0)
                                {
                                    // Add requirement for latest version of files for current document
                                    string newparams = "latestforhistoryid=" + versionHistoryId;
                                    newparams += "&hash=" + ValidationHelper.GetHashString("h" + versionHistoryId, new HashSettings(""));

                                    //url = URLHelper.AppendQuery(url, newparams);
                                    previewUrl = URLHelper.AppendQuery(previewUrl, newparams);
                                }
                            }

                            if ((SourceType != MediaSourceEnum.MediaLibraries) || (isImported || (DisplayMode != ControlDisplayModeEnum.Simple)))
                            {
                                // Initialize command
                                lnkBtn.Attributes["onclick"] = String.Format("ColorizeRow({0}); SetSelectAction({1}); return false;", ScriptHelper.GetString(GetColorizeID(data)), ScriptHelper.GetString(String.Format("{0}|URL|{1}", argument, url)));
                            }
                            else
                            {
                                string finalScript;

                                if (isLibraryFolder && !IsCopyMoveLinkDialog)
                                {
                                    finalScript = GenerateSetActionScript("morefolderselect", fileName);
                                }
                                else if (isLibraryFolder && IsCopyMoveLinkDialog)
                                {
                                    finalScript = GenerateSetActionScript("copymoveselect", fileName);
                                }
                                else
                                {
                                    finalScript = String.Format("ColorizeRow({0}); ", ScriptHelper.GetString(GetColorizeID(data)));
                                    finalScript += GenerateSetActionScript("importfile", fileName);
                                }

                                lnkBtn.Attributes["onclick"] = finalScript;
                            }

                            // Get final panel
                            result = GetListItem(ext, className, previewUrl, width, height, lnkBtn, true, isImported && siteIsRunning, GetTitle(data, isContentFile), GetDescription(data, isContentFile), name);
                        }
                    }
                    break;

                #endregion


                #region "Type"

                case "type":
                    {
                        // Show class name
                        if (isContent && (!fileDocumentWithAttachment || (OutputFormat == OutputFormatEnum.HTMLLink) || (OutputFormat == OutputFormatEnum.BBLink)))
                        {
                            return HTMLHelper.HTMLEncode(ResHelper.LocalizeString(classDisplayName));
                        }

                        // Show extension
                        result = NormalizeExtenison(data.GetValue(FileExtensionColumn).ToString());
                    }
                    break;

                #endregion


                #region "Size"

                case "size":
                    {
                        if (!isAttachment)
                        {
                            break;
                        }

                        long size = 0;
                        if (data.GetValue(FileExtensionColumn).ToString() != "<dir>")
                        {
                            if (data.ContainsColumn(FileSizeColumn))
                            {
                                size = ValidationHelper.GetLong(data.GetValue(FileSizeColumn), 0);
                            }
                            else if (data.ContainsColumn("Size"))
                            {
                                var importedData = RaiseOnFileIsNotInDatabase(GetFileName(data));
                                size = ValidationHelper.GetLong((importedData != null) ? importedData["FileSize"] : data.GetValue("Size"), 0);
                            }
                        }
                        else
                        {
                            return "";
                        }
                        result = DataHelper.GetSizeString(size);
                    }
                    break;

                #endregion


                #region "Attachment modified"

                case "attachmentmodified":
                case "attachmentmodifiedtooltip":
                    {
                        result = data.GetValue("AttachmentLastModified").ToString();

                        if (sourceName.EqualsCSafe("attachmentmodified", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = TimeZoneHelper.ConvertToUserTimeZone(ValidationHelper.GetDateTime(result, DateTimeHelper.ZERO_TIME), true, userInfo, siteInfo);
                        }
                        else
                        {
                            result = GetGMTTooltipString(userInfo, siteInfo);
                        }
                    }
                    break;

                #endregion


                #region "Attachment update"

                case "attachmentupdate":
                    {
                        // Dynamically load uploader control
                        dfuElem = LoadDirectFileUploader();

                        var updatePanel = new Panel();
                        updatePanel.ID = "updatePanel";
                        updatePanel.PreRender += (senderObject, args) => updatePanel.Width = mUpdateIconPanelWidth;
                        updatePanel.Style.Add("margin", "0 auto");

                        // Initialize update control
                        GetAttachmentUpdateControl(ref dfuElem, data);

                        dfuElem.DisplayInline = true;
                        updatePanel.Controls.Add(dfuElem);

                        Guid formGUID = ValidationHelper.GetGuid(data.GetValue("AttachmentFormGUID"), Guid.Empty);

                        // Setup external edit if form GUID is empty
                        if (formGUID == Guid.Empty)
                        {
                            ExternalEditHelper.LoadExternalEditControl(updatePanel, FileTypeEnum.Attachment, null, data, IsLiveSite, TreeNodeObj);
                            mUpdateIconPanelWidth = 32;
                        }

                        result = updatePanel;
                    }
                    break;

                #endregion


                #region "Library update"

                case "libraryupdate":
                    {
                        Panel updatePanel = new Panel();

                        updatePanel.Style.Add("margin", "0 auto");
                        updatePanel.PreRender += (senderObject, args) => updatePanel.Width = mUpdateIconPanelWidth;

                        // Get info on imported file
                        fileName = GetFileName(data);

                        IDataContainer existingData = RaiseOnFileIsNotInDatabase(fileName);
                        bool hasModifyPermission = RaiseOnGetModifyPermission(data);

                        // Dynamically load uploader control
                        dfuElem = LoadDirectFileUploader();
                        if (dfuElem != null)
                        {
                            // Initialize update control
                            GetLibraryUpdateControl(ref dfuElem, existingData);

                            updatePanel.Controls.Add(dfuElem);
                        }
                        if (hasModifyPermission && (existingData != null))
                        {
                            dfuElem.Enabled = true;
                        }
                        else
                        {
                            updatePanel.Visible = !isLibraryFolder || !hasModifyPermission;
                            dfuElem.Enabled = false;
                        }

                        if (existingData != null)
                        {
                            string dataSiteName = GetSiteName(existingData, true);

                            // Setup external edit
                            var ctrl = ExternalEditHelper.LoadExternalEditControl(updatePanel, FileTypeEnum.MediaFile, dataSiteName, existingData, IsLiveSite);

                            if (ctrl != null)
                            {
                                mUpdateIconPanelWidth = 32;
                            }
                        }

                        result = updatePanel;
                    }
                    break;

                #endregion


                #region "Attachment/MetaFile delete"

                case "metafiledelete":
                case "attachmentdelete":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction != null)
                    {
                        // Initialize DELETE button
                        btnAction.OnClientClick = String.Format("if(DeleteConfirmation() == false){{return false;}} SetAction('{1}', '{0}'); RaiseHiddenPostBack(); return false;", data.GetValue(FileIdColumn), sourceName.ToLowerCSafe());
                    }
                    break;

                #endregion


                #region "Attachment moveup"

                case "attachmentmoveup":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction != null)
                    {
                        // Get attachment ID
                        Guid attachmentGuid = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);
                        btnAction.OnClientClick = String.Format("SetAction('attachmentmoveup', '{0}'); RaiseHiddenPostBack(); return false;", attachmentGuid);
                    }
                    break;

                #endregion


                #region "Attachment movedown"

                case "attachmentmovedown":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction != null)
                    {
                        // Get attachment ID
                        Guid attachmentGuid = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);
                        btnAction.OnClientClick = String.Format("SetAction('attachmentmovedown', '{0}'); RaiseHiddenPostBack(); return false;", attachmentGuid);
                    }
                    break;

                #endregion


                #region "Attachment edit"

                case "attachmentedit":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction != null)
                    {
                        // Get file extension
                        string attExtension = ValidationHelper.GetString(data.GetValue("AttachmentExtension"), "").ToLowerCSafe();
                        Guid attGuid = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);

                        btnAction.ScreenReaderDescription = String.Format("{0}|AttachmentGUID={1}&sitename={2}&versionHistoryId={3}", attExtension, attGuid, GetSiteName(data, false), VersionHistoryID);
                        btnAction.PreRender += img_PreRender;
                    }
                    break;

                #endregion


                #region "Library extension"

                case "extension":
                    {
                        result = data.ContainsColumn("FileExtension") ? data.GetValue("FileExtension").ToString() : data.GetValue("Extension").ToString();
                        result = NormalizeExtenison(result.ToString());
                    }
                    break;

                #endregion


                #region "Modified"

                case "modified":
                case "modifiedtooltip":
                    {
                        if (sourceName.EqualsCSafe("modified", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = data.ContainsColumn("FileModifiedWhen") ? data.GetValue("FileModifiedWhen").ToString() : data.GetValue("Modified").ToString();
                            result = TimeZoneHelper.ConvertToUserTimeZone(ValidationHelper.GetDateTime(result, DateTimeHelper.ZERO_TIME), true, userInfo, siteInfo);
                        }
                        else
                        {
                            result = GetGMTTooltipString(userInfo, siteInfo);
                        }
                    }
                    break;

                #endregion


                #region "Document modified when"

                case "documentmodifiedwhen":
                case "filemodifiedwhen":
                case "documentmodifiedwhentooltip":
                case "filemodifiedwhentooltip":
                    {
                        if (sourceName.EqualsCSafe("documentmodifiedwhen", StringComparison.InvariantCultureIgnoreCase) || sourceName.EqualsCSafe("filemodifiedwhen", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = TimeZoneHelper.ConvertToUserTimeZone(ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME), true, userInfo, siteInfo);
                        }
                        else
                        {
                            result = GetGMTTooltipString(userInfo, siteInfo);
                        }
                    }
                    break;

                #endregion


                #region "MetaFile edit"

                case "metafileedit":
                    btnAction = sender as CMSGridActionButton;
                    if (btnAction != null)
                    {
                        // Get file extension
                        string metaExtension = ValidationHelper.GetString(data.GetValue("MetaFileExtension"), string.Empty).ToLowerCSafe();
                        Guid metaGuid = ValidationHelper.GetGuid(data.GetValue("MetaFileGUID"), Guid.Empty);

                        btnAction.ScreenReaderDescription = String.Format("{0}|metafileguid={1}", metaExtension, metaGuid);
                        btnAction.PreRender += img_PreRender;
                    }
                    break;

                #endregion


                #region "MetaFile update"

                case "metafileupdate":
                    {
                        // Dynamically load uploader control
                        dfuElem = Page.LoadUserControl(CONTENT_UPLOADER_FILE) as DirectFileUploader;

                        Panel updatePanel = new Panel();
                        updatePanel.ID = "updatePanel";
                        updatePanel.Width = mUpdateIconPanelWidth;
                        updatePanel.Style.Add("margin", "0 auto");

                        // Initialize update control
                        GetAttachmentUpdateControl(ref dfuElem, data);

                        dfuElem.DisplayInline = true;
                        updatePanel.Controls.Add(dfuElem);

                        result = updatePanel;
                    }
                    break;

                #endregion


                #region "MetaFile modified"

                case "metafilemodified":
                case "metafilemodifiedtooltip":
                    {
                        if (sourceName.EqualsCSafe("metafilemodified", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = data.GetValue("MetaFileLastModified").ToString();
                            result = TimeZoneHelper.ConvertToUserTimeZone(ValidationHelper.GetDateTime(result, DateTimeHelper.ZERO_TIME), true, userInfo, siteInfo);
                        }
                        else
                        {
                            result = GetGMTTooltipString(userInfo, siteInfo);
                        }
                    }
                    break;

                #endregion
            }

            return result;
        }


        /// <summary>
        /// Returns SiteInfo for given siteId. If not found, current site info is returned.
        /// </summary>
        /// <param name="siteID"></param>
        private SiteInfo GetSiteInfo(int siteID)
        {
            var siteInfo = SiteContext.CurrentSite;
            if (siteID > 0)
            {
                siteInfo = SiteInfoProvider.GetSiteInfo(siteID);
            }

            return siteInfo;
        }



        /// <summary>
        /// Finds site ID in node, attachment, meta file and media library object.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int GetSiteID(IDataContainer data)
        {
            int siteId;
            if (data.ContainsColumn("NodeSiteID"))
            {
                siteId = ValidationHelper.GetInteger(data.GetValue("NodeSiteID"), 0);
            }
            else if (data.ContainsColumn("AttachmentSiteID"))
            {
                siteId = ValidationHelper.GetInteger(data.GetValue("AttachmentSiteID"), 0);
            }
            else if (data.ContainsColumn("MetaFileSiteID"))
            {
                siteId = ValidationHelper.GetInteger(data.GetValue("MetaFileSiteID"), 0);
            }
            else
            {
                siteId = ValidationHelper.GetInteger(data.GetValue("FileSiteID"), 0);
            }
            return siteId;
        }


        /// <summary>
        /// Returns true if the item is selectable
        /// </summary>
        /// <param name="data">Item data</param>
        /// <param name="ext">File extension</param>
        /// <param name="isContentFile">True if the item is a content file</param>
        private bool IsSelectable(IDataContainer data, string ext, bool isContentFile)
        {
            // Check if item is selectable, if not remove select action button
            bool isSelectable = CMSDialogHelper.IsItemSelectable(SelectableContent, ext, isContentFile);

            // Check if the page type can be selected
            if (isSelectable)
            {
                isSelectable = IsPageTypeSelectable(data);
            }

            return isSelectable;
        }


        /// <summary>
        /// Returns true if the given page is selectable based on its page type
        /// </summary>
        /// <param name="data">Page data</param>
        public bool IsPageTypeSelectable(IDataContainer data)
        {
            var pageTypeSelectable = true;

            if ((SourceType == MediaSourceEnum.Content) && (SelectablePageTypes != SelectablePageTypeEnum.All))
            {
                var isContentOnly = data.GetValue("NodeIsContentOnly").ToBoolean(false);
                var wantsContentOnly = (SelectablePageTypes == SelectablePageTypeEnum.ContentOnly);

                pageTypeSelectable = (isContentOnly == wantsContentOnly);
            }

            return pageTypeSelectable;
        }


        /// <summary>
        /// Gets the resource type based on the media source
        /// </summary>
        /// <param name="sourceType">Media source type</param>
        private FileTypeEnum GetFileType(MediaSourceEnum sourceType)
        {
            switch (sourceType)
            {
                case MediaSourceEnum.Attachment:
                case MediaSourceEnum.Content:
                case MediaSourceEnum.DocumentAttachments:
                    return FileTypeEnum.Attachment;


                case MediaSourceEnum.MediaLibraries:
                    return FileTypeEnum.MediaFile;

                case MediaSourceEnum.MetaFile:
                    return FileTypeEnum.MetaFile;
            }

            return FileTypeEnum.Unknown;
        }


        /// <summary>
        /// Loads the direct file uploader control
        /// </summary>
        private DirectFileUploader LoadDirectFileUploader()
        {
            var dfuElem = Page.LoadUserControl(CONTENT_UPLOADER_FILE) as DirectFileUploader;

            return dfuElem;
        }


        private string GetGMTTooltipString(IUserInfo userInfo, ISiteInfo siteInfo)
        {
            if (String.IsNullOrEmpty(mGMTTooltip))
            {
                mGMTTooltip = TimeZoneHelper.GetUTCLongStringOffset(userInfo, siteInfo);
            }

            return mGMTTooltip;
        }


        private void img_PreRender(object sender, EventArgs e)
        {
            var img = (CMSAccessibleButton)sender;

            string[] args = img.ScreenReaderDescription.Split('|');

            img.ScreenReaderDescription = GetString("general.edit");

            if (args.Length == 2)
            {
                string refreshType = CMSDialogHelper.GetMediaSource(SourceType);
                int parentId = QueryHelper.GetInteger("parentId", 0);
                Guid formGuid = QueryHelper.GetGuid("formGuid", Guid.Empty);
                string query = String.Format("?clientid={0}{1}&refresh=1&refaction=0{2}&{3}", HTMLHelper.HTMLEncode(refreshType), ((parentId > 0) ? "&parentId=" + parentId : ""), ((formGuid != Guid.Empty) ? "&formGuid=" + formGuid : ""), args[1]);
                // Get validation hash for current image
                query = URLHelper.AddUrlParameter(query, "hash", QueryHelper.GetHash(query));

                img.OnClientClick = ImageHelper.IsSupportedByImageEditor(args[0]) ?
                    String.Format("if(!($cmsj(this).hasClass('Edited'))){{ EditImage(\"{0}\"); }} return false;", query) :
                    String.Format("if(!($cmsj(this).hasClass('Edited'))){{ Edit(\"{0}\"); }} return false;", query);

                img.ToolTip = GetString("general.edit");
            }
        }

        #endregion


        #region "Thumbnails view methods"

        /// <summary>
        /// Setups the UniPager
        /// </summary>
        /// <param name="pager">Pager to setup</param>
        protected void SetupPager(UniPager pager)
        {
            pager.DisplayFirstLastAutomatically = false;
            pager.DisplayPreviousNextAutomatically = false;
            pager.HidePagerForSinglePage = true;
            pager.PagerMode = UniPagerMode.PostBack;
        }


        /// <summary>
        /// Item data bound handler for thumbnails view
        /// </summary>
        protected void ThumbnailsViewControl_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            #region "Load the item data"

            var dr = (DataRowView)e.Item.DataItem;

            IDataContainer dataWithoutAttachment = new DataRowContainer(dr);

            // Apply file data if necessary
            var data = GetCompleteData(dataWithoutAttachment);

            string fileNameColumn = FileNameColumn;
            string className = "";

            bool isContent = (SourceType == MediaSourceEnum.Content);

            bool isContentFile = isContent && data.GetValue("ClassName").ToString().EqualsCSafe(SystemDocumentTypes.File, true);
            bool notAttachment = isContent && !(isContentFile && (data.GetValue("AttachmentGUID") != DBNull.Value));
            if (notAttachment)
            {
                className = ResHelper.LocalizeString(DataClassInfoProvider.GetDataClassInfo((int)data.GetValue("NodeClassID")).ClassDisplayName);

                fileNameColumn = "DocumentName";
            }

            // Get information on file
            string fileName = HTMLHelper.HTMLEncode(data.GetValue(fileNameColumn).ToString(""));
            string ext = HTMLHelper.HTMLEncode(notAttachment ? className : data.GetValue(FileExtensionColumn).ToString("").TrimStart('.'));
            string argument = RaiseOnGetArgumentSet(data);

            // Get full media library file name
            bool isInDatabase = true;
            string fullFileName = GetFileName(data);

            IDataContainer importedMediaData = null;

            if (SourceType == MediaSourceEnum.MediaLibraries)
            {
                importedMediaData = RaiseOnFileIsNotInDatabase(fullFileName);

                // Apply file data if necessary
                importedMediaData = GetCompleteData(importedMediaData);

                isInDatabase = (importedMediaData != null);
            }

            bool libraryFolder = ((SourceType == MediaSourceEnum.MediaLibraries) && IsFullListingMode && (data.GetValue(FileExtensionColumn).ToString().ToLowerCSafe() == "<dir>"));
            bool libraryUiFolder = libraryFolder && !((DisplayMode == ControlDisplayModeEnum.Simple) && isInDatabase);

            int width = 0;
            int height = 0;
            // Get thumb preview image dimensions
            int[] thumbImgDimension = { 0, 0 };
            if (ImageHelper.IsSupportedByImageEditor(ext))
            {
                // Width & height
                if (data.ContainsColumn(FileWidthColumn))
                {
                    width = ValidationHelper.GetInteger(data.GetValue(FileWidthColumn), 0);
                }
                else if ((importedMediaData != null) && (importedMediaData.ContainsColumn(FileWidthColumn)))
                {
                    width = ValidationHelper.GetInteger(importedMediaData.GetValue(FileWidthColumn), 0);
                }

                if (data.ContainsColumn(FileHeightColumn))
                {
                    height = ValidationHelper.GetInteger(data.GetValue(FileHeightColumn), 0);
                }
                else if ((importedMediaData != null) && (importedMediaData.ContainsColumn(FileHeightColumn)))
                {
                    height = ValidationHelper.GetInteger(importedMediaData.GetValue(FileHeightColumn), 0);
                }

                thumbImgDimension = CMSDialogHelper.GetThumbImageDimensions(height, width, MaxThumbImgHeight, MaxThumbImgWidth);
            }

            // Preview parameters
            IconParameters previewParameters;
            if ((SourceType == MediaSourceEnum.MediaLibraries) && isInDatabase)
            {
                string mediaExtension = ext;
                if (importedMediaData != null)
                {
                    mediaExtension = importedMediaData.GetValue((importedMediaData.ContainsColumn("FileExtension") ? "FileExtension" : "Extension")).ToString();
                }

                previewParameters = RaiseOnGetThumbsItemUrl(importedMediaData, true, thumbImgDimension[0], thumbImgDimension[1], 0, mediaExtension);
            }
            else
            {
                previewParameters = RaiseOnGetThumbsItemUrl(dataWithoutAttachment, true, thumbImgDimension[0], thumbImgDimension[1], 0, ext);
            }

            // Item parameters
            IconParameters selectUrlParameters = RaiseOnGetThumbsItemUrl(dataWithoutAttachment, false, 0, 0, 0, ext);
            bool isSelectable = IsSelectable(data, ext, isContentFile);

            #endregion


            #region "Standard controls and actions"

            // Load file name
            Label lblName = e.Item.FindControl("lblFileName") as Label;
            if (lblName != null)
            {
                lblName.Text = fileName;
            }

            // Initialize SELECT button
            var btnWarning = e.Item.FindControl("btnWarning") as CMSAccessibleButton;
            if (btnWarning != null)
            {
                // If media file not imported yet - display warning sign
                if (isSelectable && (SourceType == MediaSourceEnum.MediaLibraries) && ((DisplayMode == ControlDisplayModeEnum.Simple) && !isInDatabase && !libraryFolder))
                {
                    btnWarning.ToolTip = GetString("media.file.import");
                    btnWarning.OnClientClick = String.Format("ColorizeRow({0}); SetAction('importfile',{1}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(GetColorizeID(data)), ScriptHelper.GetString(fullFileName));
                }
                else
                {
                    PlaceHolder plcWarning = e.Item.FindControl("plcWarning") as PlaceHolder;
                    if (plcWarning != null)
                    {
                        plcWarning.Visible = false;
                    }
                }
            }

            // Initialize SELECTSUBDOCS button
            var btnSelectSubDocs = e.Item.FindControl("btnSelectSubDocs") as CMSAccessibleButton;
            if (btnSelectSubDocs != null)
            {
                if (IsFullListingMode && (SourceType == MediaSourceEnum.Content))
                {
                    int nodeId = ValidationHelper.GetInteger(data.GetValue("NodeID"), 0);

                    btnSelectSubDocs.ToolTip = GetString("dialogs.list.actions.showsubdocuments");

                    // Check if item is selectable, if not remove select action button
                    btnSelectSubDocs.OnClientClick = String.Format("SetParentAction('{0}'); return false;", nodeId);
                }
                else
                {
                    PlaceHolder plcSelectSubDocs = e.Item.FindControl("plcSelectSubDocs") as PlaceHolder;
                    if (plcSelectSubDocs != null)
                    {
                        plcSelectSubDocs.Visible = false;
                    }
                }
            }

            // Initialize VIEW button
            var btnView = e.Item.FindControl("btnView") as CMSAccessibleButton;
            if (btnView != null)
            {
                if (!notAttachment && !libraryFolder)
                {
                    if (String.IsNullOrEmpty(selectUrlParameters.Url))
                    {
                        btnView.OnClientClick = "return false;";
                        btnView.Attributes["style"] = "cursor:default;";
                        btnView.Enabled = false;
                    }
                    else
                    {
                        btnView.ToolTip = GetString("dialogs.list.actions.view");
                        btnView.OnClientClick = String.Format("javascript: window.open({0}); return false;", ScriptHelper.GetString(UrlResolver.ResolveUrl(selectUrlParameters.Url)));
                    }
                }
                else
                {
                    btnView.Visible = false;
                }
            }

            // Initialize EDIT button
            var btnContentEdit = e.Item.FindControl("btnContentEdit") as CMSAccessibleButton;
            if (btnContentEdit != null)
            {
                btnContentEdit.ToolTip = GetString("general.edit");

                Guid guid;

                if (SourceType == MediaSourceEnum.MediaLibraries && !libraryFolder && !libraryUiFolder)
                {
                    // Media files coming from FS
                    if (!data.ContainsColumn("FileGUID"))
                    {
                        if ((DisplayMode == ControlDisplayModeEnum.Simple) && !isInDatabase)
                        {
                            btnContentEdit.Attributes["style"] = "cursor: default;";
                            btnContentEdit.Enabled = false;
                        }
                        else
                        {
                            btnContentEdit.OnClientClick = String.Format("$cmsj('#hdnFileOrigName').attr('value', {0}); SetAction('editlibraryui', {1}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(EnsureFileName(fileName)), ScriptHelper.GetString(fileName));
                        }
                    }
                    else
                    {
                        guid = ValidationHelper.GetGuid(data.GetValue("FileGUID"), Guid.Empty);
                        btnContentEdit.ScreenReaderDescription = String.Format("{0}|MediaFileGUID={1}&sitename={2}", ext, guid, GetSiteName(data, true));
                        btnContentEdit.PreRender += img_PreRender;
                    }
                }
                else if (SourceType == MediaSourceEnum.MetaFile)
                {
                    // If MetaFiles being displayed set EDIT action
                    string metaExtension = ValidationHelper.GetString(data.GetValue("MetaFileExtension"), string.Empty).ToLowerCSafe();
                    Guid metaGuid = ValidationHelper.GetGuid(data.GetValue("MetaFileGUID"), Guid.Empty);

                    btnContentEdit.ScreenReaderDescription = String.Format("{0}|metafileguid={1}", metaExtension, metaGuid);
                    btnContentEdit.PreRender += img_PreRender;
                }
                else if (!notAttachment && !libraryFolder && !libraryUiFolder)
                {
                    string nodeid = "";
                    if (SourceType == MediaSourceEnum.Content)
                    {
                        nodeid = "&nodeId=" + data.GetValue("NodeID");

                        // Get the node workflow
                        VersionHistoryID = ValidationHelper.GetInteger(data.GetValue("DocumentCheckedOutVersionHistoryID"), 0);
                    }

                    guid = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);
                    btnContentEdit.ScreenReaderDescription = String.Format("{0}|AttachmentGUID={1}&sitename={2}{3}{4}", ext, guid, GetSiteName(data, false), nodeid, ((VersionHistoryID > 0) ? "&versionHistoryId=" + VersionHistoryID : ""));
                    btnContentEdit.PreRender += img_PreRender;
                }
                else
                {
                    btnContentEdit.Visible = false;
                }
            }

            #endregion


            #region "Special actions"

            // If attachments being displayed show additional actions
            if (SourceType == MediaSourceEnum.DocumentAttachments)
            {
                // Initialize EDIT button
                var btnEdit = e.Item.FindControl("btnEdit") as CMSAccessibleButton;
                if (btnEdit != null)
                {
                    if (!notAttachment)
                    {
                        btnEdit.ToolTip = GetString("general.edit");

                        // Get file extension
                        string extension = ValidationHelper.GetString(data.GetValue("AttachmentExtension"), "").ToLowerCSafe();
                        Guid guid = ValidationHelper.GetGuid(data.GetValue("AttachmentGUID"), Guid.Empty);

                        btnEdit.ScreenReaderDescription = String.Format("{0}|AttachmentGUID={1}&sitename={2}&versionHistoryId={3}", extension, guid, GetSiteName(data, false), VersionHistoryID);
                        btnEdit.PreRender += img_PreRender;
                    }
                }

                // Initialize UPDATE button
                var dfuElem = e.Item.FindControl("dfuElem") as DirectFileUploader;
                if (dfuElem != null)
                {
                    GetAttachmentUpdateControl(ref dfuElem, data);
                }

                // Setup external edit
                var ctrl = ExternalEditHelper.LoadExternalEditControl(e.Item.FindControl("plcExtEdit"), FileTypeEnum.Attachment, null, data, IsLiveSite, TreeNodeObj, true);
                if (ctrl != null)
                {
                    ctrl.CssClass = null;
                }

                // Initialize DELETE button
                var btnDelete = e.Item.FindControl("btnDelete") as CMSAccessibleButton;
                if (btnDelete != null)
                {
                    btnDelete.ToolTip = GetString("general.delete");

                    // Initialize command
                    btnDelete.OnClientClick = String.Format("if(DeleteConfirmation() == false){{return false;}} SetAction('attachmentdelete','{0}'); RaiseHiddenPostBack(); return false;", data.GetValue("AttachmentGUID"));
                }

                var plcContentEdit = e.Item.FindControl("plcContentEdit") as PlaceHolder;
                if (plcContentEdit != null)
                {
                    plcContentEdit.Visible = false;
                }
            }
            else if ((SourceType == MediaSourceEnum.MediaLibraries) && !data.ContainsColumn("FileGUID") && ((DisplayMode == ControlDisplayModeEnum.Simple) && !libraryFolder && !libraryUiFolder))
            {
                // Initialize DELETE button
                var btnDelete = e.Item.FindControl("btnDelete") as CMSAccessibleButton;
                if (btnDelete != null)
                {
                    btnDelete.ToolTip = GetString("general.delete");
                    btnDelete.OnClientClick = String.Format("if(DeleteMediaFileConfirmation() == false){{return false;}} SetAction('deletefile',{0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(fullFileName));
                }

                // Hide attachment specific actions
                PlaceHolder plcAttachmentUpdtAction = e.Item.FindControl("plcAttachmentUpdtAction") as PlaceHolder;
                if (plcAttachmentUpdtAction != null)
                {
                    plcAttachmentUpdtAction.Visible = false;
                }
            }
            else
            {
                PlaceHolder plcAttachmentActions = e.Item.FindControl("plcAttachmentActions") as PlaceHolder;
                if (plcAttachmentActions != null)
                {
                    plcAttachmentActions.Visible = false;
                }
            }

            #endregion


            #region "Library update action"

            if ((SourceType == MediaSourceEnum.MediaLibraries) && (DisplayMode == ControlDisplayModeEnum.Simple))
            {
                // Initialize UPDATE button
                var dfuElemLib = e.Item.FindControl("dfuElemLib") as DirectFileUploader;
                if (dfuElemLib != null)
                {
                    Panel pnlDisabledUpdate = (e.Item.FindControl("pnlDisabledUpdate") as Panel);
                    if (pnlDisabledUpdate != null)
                    {
                        bool hasModifyPermission = RaiseOnGetModifyPermission(data);
                        if (isInDatabase && hasModifyPermission)
                        {
                            GetLibraryUpdateControl(ref dfuElemLib, importedMediaData);

                            pnlDisabledUpdate.Visible = false;
                        }
                        else
                        {
                            pnlDisabledUpdate.Controls.Clear();

                            var disabledIcon = new CMSAccessibleButton
                            {
                                EnableViewState = false,
                                Enabled = false,
                                IconCssClass = "icon-arrow-up-line",
                                IconOnly = true
                            };

                            pnlDisabledUpdate.Controls.Add(disabledIcon);

                            dfuElemLib.Visible = false;
                        }
                    }
                }

                // Setup external edit
                if (isInDatabase)
                {
                    ExternalEditHelper.LoadExternalEditControl(e.Item.FindControl("plcExtEditMfi"), FileTypeEnum.MediaFile, GetSiteName(data, true), importedMediaData, IsLiveSite, null, true);
                }
            }
            else if (((SourceType == MediaSourceEnum.Content) && (DisplayMode == ControlDisplayModeEnum.Default) && !notAttachment && !libraryFolder && !libraryUiFolder))
            {
                // Setup external edit
                if (data.ContainsColumn("AttachmentGUID"))
                {
                    LoadExternalEditControl(e.Item, FileTypeEnum.Attachment);
                }
            }
            else if (((SourceType == MediaSourceEnum.MediaLibraries) && (DisplayMode == ControlDisplayModeEnum.Default) && !libraryFolder && !libraryUiFolder))
            {
                // Setup external edit
                if (data.ContainsColumn("FileGUID"))
                {
                    LoadExternalEditControl(e.Item, FileTypeEnum.MediaFile);
                }
            }
            else
            {
                var plcLibraryUpdtAction = e.Item.FindControl("plcLibraryUpdtAction") as PlaceHolder;
                if (plcLibraryUpdtAction != null)
                {
                    plcLibraryUpdtAction.Visible = false;
                }
            }

            if ((SourceType == MediaSourceEnum.MediaLibraries) && libraryFolder && IsFullListingMode)
            {
                // Initialize SELECT SUB-FOLDERS button
                var btn = e.Item.FindControl("imgSelectSubFolders") as CMSAccessibleButton;
                if (btn != null)
                {
                    btn.Visible = true;
                    btn.ToolTip = GetString("dialogs.list.actions.showsubfolders");
                    btn.OnClientClick = String.Format("SetLibParentAction({0}); return false;", ScriptHelper.GetString(fileName));
                }
            }
            else
            {
                var plcSelectSubFolders = e.Item.FindControl("plcSelectSubFolders") as PlaceHolder;
                if (plcSelectSubFolders != null)
                {
                    plcSelectSubFolders.Visible = false;
                }
            }

            #endregion


            #region "File image"

            // Selectable area
            Panel pnlItemInageContainer = e.Item.FindControl("pnlThumbnails") as Panel;
            if (pnlItemInageContainer != null)
            {
                if (isSelectable)
                {
                    if ((DisplayMode == ControlDisplayModeEnum.Simple) && !isInDatabase)
                    {
                        if (libraryFolder || libraryUiFolder)
                        {
                            pnlItemInageContainer.Attributes["onclick"] = String.Format("SetAction('morefolderselect', {0}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(fileName));
                        }
                        else
                        {
                            pnlItemInageContainer.Attributes["onclick"] = String.Format("ColorizeRow({0}); SetAction('importfile', {1}); RaiseHiddenPostBack(); return false;", ScriptHelper.GetString(GetColorizeID(data)), ScriptHelper.GetString(fullFileName));
                        }
                    }
                    else
                    {
                        pnlItemInageContainer.Attributes["onclick"] = String.Format("ColorizeRow({0}); SetSelectAction({1}); return false;", ScriptHelper.GetString(GetColorizeID(data)), ScriptHelper.GetString(String.Format("{0}|URL|{1}", argument, selectUrlParameters.Url)));
                    }
                    pnlItemInageContainer.Attributes["style"] = "cursor:pointer;";
                }
                else
                {
                    pnlItemInageContainer.Attributes["style"] = "cursor:default;";
                }
            }

            // Image area
            Image imgFile = e.Item.FindControl("imgFile") as Image;
            if (imgFile != null)
            {
                string chset = Guid.NewGuid().ToString();
                var previewUrl = previewParameters.Url;
                previewUrl = URLHelper.AddParameterToUrl(previewUrl, "chset", chset);

                // Add latest version requirement for live site
                int versionHistoryId = VersionHistoryID;
                if (IsLiveSite && (versionHistoryId > 0))
                {
                    // Add requirement for latest version of files for current document
                    string newparams = String.Format("latestforhistoryid={0}&hash={1}", versionHistoryId, ValidationHelper.GetHashString("h" + versionHistoryId, new HashSettings("")));

                    previewUrl += "&" + newparams;
                }

                if (String.IsNullOrEmpty(previewParameters.IconClass))
                {
                    imgFile.ImageUrl = previewUrl;
                    imgFile.AlternateText = TextHelper.LimitLength(fileName, 10);
                    imgFile.Attributes["title"] = fileName.Replace("\"", "\\\"");

                    // Ensure tooltip - only text description
                    if (isInDatabase)
                    {
                        UIHelper.EnsureTooltip(imgFile, previewUrl, width, height, GetTitle(data, isContentFile), fileName, ext, GetDescription(data, isContentFile), null, 300);
                    }
                }
                else
                {
                    var imgIcon = e.Item.FindControl(("imgFileIcon")) as Label;
                    if ((imgIcon != null) && imgIcon.Controls.Count < 1)
                    {
                        className = ValidationHelper.GetString(data.GetValue("ClassName"), String.Empty);
                        var icon = UIHelper.GetDocumentTypeIcon(null, className, previewParameters.IconClass, previewParameters.IconSize);

                        imgIcon.Controls.Add(new LiteralControl(icon));

                        // Ensure tooltip - only text description
                        if (isInDatabase)
                        {
                            UIHelper.EnsureTooltip(imgIcon, previewUrl, width, height, GetTitle(data, isContentFile), fileName, ext, GetDescription(data, isContentFile), null, 300);
                        }

                        imgFile.Visible = false;
                    }
                }
            }

            #endregion


            // Display only for ML UI
            if ((DisplayMode == ControlDisplayModeEnum.Simple) && !libraryFolder)
            {
                PlaceHolder plcSelectionBox = e.Item.FindControl("plcSelectionBox") as PlaceHolder;
                if (plcSelectionBox != null)
                {
                    plcSelectionBox.Visible = true;

                    // Multiple selection check-box
                    CMSCheckBox chkSelected = e.Item.FindControl("chkSelected") as CMSCheckBox;
                    if (chkSelected != null)
                    {
                        chkSelected.ToolTip = GetString("general.select");
                        chkSelected.InputAttributes["alt"] = fullFileName;

                        HiddenField hdnItemName = e.Item.FindControl("hdnItemName") as HiddenField;
                        if (hdnItemName != null)
                        {
                            hdnItemName.Value = fullFileName;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the file data for the given data record
        /// </summary>
        /// <param name="data">Source data</param>
        private IDataContainer GetCompleteData(IDataContainer data)
        {
            if (data == null)
            {
                return null;
            }

            if (SourceType == MediaSourceEnum.Content)
            {
                var documentId = ValidationHelper.GetInteger(data.GetValue("DocumentID"), 0);

                var fileData = ((mFileData != null) ? mFileData[documentId] : null) ?? EmptyAttachment;

                return new AggregatedDataContainer(data, fileData);
            }

            // File data is placed directly in original data for other source types
            return data;
        }

        #endregion


        #region "Raise events methods"

        /// <summary>
        /// Fires specific action and returns result provided by the parent control.
        /// </summary>
        /// <param name="data">Data related to the action</param>
        private string RaiseOnGetArgumentSet(IDataContainer data)
        {
            if (GetArgumentSet != null)
            {
                return GetArgumentSet(data);
            }
            return "";
        }


        /// <summary>
        /// Fires specific action and returns result provided by the parent control.
        /// </summary>
        /// <param name="data">Data related to the action</param>
        /// <param name="isPreview">Indicates whether the URL is required for preview item</param>
        private string RaiseOnGetAttachmentUrl(IDataContainer data, bool isPreview)
        {
            if (GetListItemUrl != null)
            {
                return GetListItemUrl(data, isPreview);
            }
            return "";
        }



        /// <summary>
        /// Fires action to find node presentation URL.
        /// </summary>
        /// <param name="node">Tree node to find presentation URL for.</param>
        private string RaiseOnGetContentItemUrl(TreeNode node)
        {
            if (GetContentItemUrl != null)
            {
                return GetContentItemUrl(node);
            }
            return "";
        }


        /// <summary>
        /// Fires specific action and returns result provided by the parent control.
        /// </summary>
        /// <param name="data">Data related to the action</param>
        /// <param name="isPreview">Indicates whether the image is required as part of preview</param>
        /// <param name="width">Maximum width of the preview image</param>
        /// <param name="maxSideSize">Maximum size of the preview image. If full-size required parameter gets zero value</param>
        /// <param name="extension">File extension</param>
        /// <param name="height">Maximum height of the preview image</param>
        private IconParameters RaiseOnGetThumbsItemUrl(IDataContainer data, bool isPreview, int height, int width, int maxSideSize, string extension)
        {
            if (GetThumbsItemUrl != null)
            {
                return GetThumbsItemUrl(data, isPreview, height, width, maxSideSize, extension);
            }
            return null;
        }


        /// <summary>
        /// Raises event when information on import status of specified file is required.
        /// </summary>
        /// <param name="fileName">Name of the file (including extension)</param>
        private IDataContainer RaiseOnFileIsNotInDatabase(string fileName)
        {
            if (GetInformation != null)
            {
                object result = GetInformation("fileisnotindatabase", fileName);
                if (result != null)
                {
                    // Ensure the data container
                    return (result is DataRow ? new DataRowContainer((DataRow)result) : (result is DataRowView ? new DataRowContainer((DataRowView)result) : (IDataContainer)result));
                }
                return null;
            }
            return null;
        }


        /// <summary>
        /// Raises event when ID of the current site is required.
        /// </summary>
        private int RaiseOnSiteIdRequired()
        {
            if (GetInformation != null)
            {
                return (int)GetInformation("siteidrequired", null);
            }

            return 0;
        }


        /// <summary>
        /// Raises event when modify permission is required.
        /// </summary>
        /// <param name="data">Data container</param>
        private bool RaiseOnGetModifyPermission(IDataContainer data)
        {
            if (GetModifyPermission != null)
            {
                return GetModifyPermission(data);
            }
            return true;
        }

        #endregion


        #region "External edit methods"

        /// <summary>
        /// Loads the external edit control and sets visibility of other controls
        /// </summary>
        /// <param name="repeaterItem">Repeater item</param>
        /// <param name="type">Source type</param>
        private void LoadExternalEditControl(RepeaterItem repeaterItem, FileTypeEnum type)
        {
            var plcAttachmentActions = repeaterItem.FindControl("plcAttachmentActions") as PlaceHolder;
            var plcAttachmentUpdtAction = repeaterItem.FindControl("plcAttachmentUpdtAction") as PlaceHolder;
            var plcLibraryUpdtAction = repeaterItem.FindControl("plcLibraryUpdtAction") as PlaceHolder;
            var plcExt = repeaterItem.FindControl("plcExtEdit") as PlaceHolder;
            var plcExtMfi = repeaterItem.FindControl("plcExtEditMfi") as PlaceHolder;
            var pnlDisabledUpdate = (repeaterItem.FindControl("pnlDisabledUpdate") as Panel);
            var dfuLib = repeaterItem.FindControl("dfuElemLib") as DirectFileUploader;
            var dfu = repeaterItem.FindControl("dfuElem") as DirectFileUploader;
            var btnEdit = repeaterItem.FindControl("btnEdit") as WebControl;
            var btnDelete = repeaterItem.FindControl("btnDelete") as WebControl;

            if ((plcAttachmentActions != null) && (plcLibraryUpdtAction != null) && (plcAttachmentUpdtAction != null) && (plcExt != null)
                && (plcExtMfi != null) && (pnlDisabledUpdate != null) && (dfuLib != null) && (dfu != null) && (btnEdit != null) && (btnDelete != null))
            {
                var data = new DataRowContainer((DataRowView)repeaterItem.DataItem);

                plcAttachmentActions.Visible = true;
                plcAttachmentUpdtAction.Visible = false;
                pnlDisabledUpdate.Visible = false;
                dfuLib.Visible = false;
                dfu.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                plcExt.Visible = false;
                plcExtMfi.Visible = false;

                plcLibraryUpdtAction.Visible = (type == FileTypeEnum.MediaFile);

                ExternalEditHelper.LoadExternalEditControl(plcExt, type, null, data, IsLiveSite, TreeNodeObj, true);
            }
        }

        #endregion
    }
}
