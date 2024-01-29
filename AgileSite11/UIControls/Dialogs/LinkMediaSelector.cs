using System;
using System.Collections;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Link media selector base class.
    /// </summary>
    public abstract class LinkMediaSelector : CMSAdminControl
    {
        #region "Constants"
        
        /// <summary>
        /// Short link to help page regarding media link insertion.
        /// </summary>
        private const string HELP_TOPIC_INSERT_MEDIA_LINK = "media_content_through_editor";


        /// <summary>
        /// Short link to help page regarding link insertion.
        /// </summary>
        private const string HELP_TOPIC_INSERT_LINK_LINK = "links_anchors";

        #endregion


        #region "Private variables"

        private DialogViewModeEnum mViewMode = DialogViewModeEnum.ListView;
        private MediaSourceEnum mSourceType = MediaSourceEnum.MediaLibraries;
        private MediaSource mMediaSource;
        
        private SelectableContentEnum mSelectableContent = SelectableContentEnum.AllContent;
        private SelectablePageTypeEnum mSelectablePageTypes = SelectablePageTypeEnum.All;

        private DialogConfiguration mConfig;
        private Hashtable mParameters;

        private bool mUsePermanentUrls = true;

        #endregion


        #region "Public properties"

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
        /// Selected view mode.
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
        /// Selected source type.
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
        /// Media source.
        /// </summary>
        public MediaSource MediaSource
        {
            get
            {
                if (mMediaSource == null)
                {
                    // Try to get source from the session
                    object ms = SessionHelper.GetValue("MediaSource");
                    if (ms != null)
                    {
                        mMediaSource = ms as MediaSource;
                    }
                }
                return mMediaSource;
            }
            set
            {
                mMediaSource = value;
                SessionHelper.SetValue("MediaSource", value);
            }
        }


        /// <summary>
        /// Dialog parameters collection.
        /// </summary>
        public Hashtable Parameters
        {
            get
            {
                if (mParameters == null)
                {
                    // Try to get parameters from the session
                    object dp = SessionHelper.GetValue("DialogParameters");
                    if (dp != null)
                    {
                        mParameters = (dp as Hashtable);
                    }
                }
                return mParameters;
            }
            set
            {
                mParameters = value;
                SessionHelper.SetValue("DialogParameters", value);
            }
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
        /// Returns current properties (according to OutputFormat).
        /// </summary>
        protected virtual ItemProperties ItemProperties
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Update panel where properties control resides.
        /// </summary>
        protected virtual UpdatePanel PropertiesUpdatePanel
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Indicates whether the permanent URLs should be generated.
        /// </summary>
        protected bool UsePermanentUrls
        {
            get
            {
                return mUsePermanentUrls;
            }
            set
            {
                mUsePermanentUrls = value;
            }
        }


        /// <summary>
        /// Indicates whether the image was recently edited.
        /// </summary>
        protected bool IsEditImage
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IsEditImage"], false);
            }
            set
            {
                ViewState["IsEditImage"] = value;
            }
        }


        /// <summary>
        /// Indicates whether the selected item was recently loaded.
        /// </summary>
        protected bool IsItemLoaded
        {
            get;
            set;
        }
        

        /// <summary>
        /// Indicates whether the asynchronous postback occurs on the page.
        /// </summary>
        protected bool IsInAsyncPostBack
        {
            get
            {
                var scriptManager = ScriptManager.GetCurrent(Page);
                return scriptManager != null && scriptManager.IsInAsyncPostBack;
            }
        }


        /// <summary>
        /// Indicates whether the post back is result of some hidden action.
        /// </summary>
        protected bool IsAction
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets selected item to colorize.
        /// </summary>
        protected Guid ItemToColorize
        {
            get
            {
                return ValidationHelper.GetGuid(ViewState["ItemToColorize"], Guid.Empty);
            }
            set
            {
                ViewState["ItemToColorize"] = value;
            }
        }
                

        /// <summary>
        /// Indicates if properties are displayed in full height mode.
        /// </summary>
        protected bool IsFullDisplay
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IsFullDisplay"], false);
            }
            set
            {
                ViewState["IsFullDisplay"] = value;
            }
        }


        /// <summary>
        /// Indicates whether the control is displayed as part of the copy/move dialog.
        /// </summary>
        protected bool IsCopyMoveLinkDialog
        {
            get
            {
                switch (Config.CustomFormatCode)
                {
                    case "copy":
                    case "move":
                    case "link":
                    case "linkdoc":
                    case "selectpath":
                    case "relationship":
                        return true;

                    default:
                        return false;
                }
            }
        }


        /// <summary>
        /// Indicates whether the control output is link.
        /// </summary>
        protected bool IsLinkOutput
        {
            get
            {
                return
                    ((Config.OutputFormat == OutputFormatEnum.HTMLLink) ||
                    (Config.OutputFormat == OutputFormatEnum.BBLink) ||
                    (Config.OutputFormat == OutputFormatEnum.URL));
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Sets the focus on a search box
        /// </summary>
        protected void SetSearchFocus()
        {
            // Keep focus in search text box
            ScriptHelper.RegisterStartupScript(Page, typeof(string), "SetSearchFocus", ScriptHelper.GetScript("setTimeout('SetSearchFocus();', 200);"));
        }

                
        /// <summary>
        /// Initialize design jQuery scripts.
        /// </summary>
        protected void InitializeDesignScripts()
        {
            ScriptHelper.RegisterStartupScript(Page, typeof(Page), "designScript", ScriptHelper.GetScript(@"
setTimeout('InitializeDesign();',200);
$cmsj(window).unbind('resize').resize(function() { 
    InitializeDesign(); 
});"));
        }

        
        /// <summary>
        /// Initializes its properties according to the URL parameters.
        /// </summary>
        public void InitFromQueryString()
        {
            switch (Config.OutputFormat)
            {
                case OutputFormatEnum.HTMLMedia:
                    SelectableContent = SelectableContentEnum.OnlyMedia;
                    break;

                case OutputFormatEnum.HTMLLink:
                    SelectableContent = SelectableContentEnum.AllContent;
                    break;

                case OutputFormatEnum.BBMedia:
                    SelectableContent = SelectableContentEnum.OnlyImages;
                    break;

                case OutputFormatEnum.BBLink:
                    SelectableContent = SelectableContentEnum.AllContent;
                    break;

                case OutputFormatEnum.URL:
                case OutputFormatEnum.NodeGUID:
                    string content = QueryHelper.GetString("content", "");
                    SelectableContent = CMSDialogHelper.GetSelectableContent(content);
                    break;
            }

            SelectablePageTypes = Config.SelectablePageTypes;
        }


        /// <summary>
        /// Sets context help in dialog depending on current Config.
        /// </summary>
        protected void SetHelp()
        {
            if (IsLiveSite)
            {
                return;
            }

            string helpTopic = String.Empty;
            if ((Config.OutputFormat == OutputFormatEnum.URL) || (Config.OutputFormat == OutputFormatEnum.HTMLLink) || (Config.OutputFormat == OutputFormatEnum.BBLink))
            {
                helpTopic = HELP_TOPIC_INSERT_LINK_LINK;
            }
            else if ((Config.OutputFormat == OutputFormatEnum.BBMedia) || (Config.OutputFormat == OutputFormatEnum.HTMLMedia))
            {
                helpTopic = HELP_TOPIC_INSERT_MEDIA_LINK;
            }

            if (!String.IsNullOrEmpty(helpTopic))
            {
                helpTopic = DocumentationHelper.GetDocumentationTopicUrl(helpTopic);
            }

            object options = new
            {
                helpName = "lnkMediaSelectorHelp",
                helpUrl = helpTopic
            };
            ScriptHelper.RegisterModule(this, "CMS/DialogContextHelpChange", options);
        }


        /// <summary>
        /// Gets JavaScript object representing item being inserted.
        /// </summary>
        /// <param name="properties">Collection of item properties</param>
        protected string GetInsertItem(Hashtable properties)
        {
            string result = "";
            if (properties != null)
            {
                if (properties[DialogParameters.IMG_URL] != null)
                {
                    // Image
                    result = CMSDialogHelper.GetImageItem(properties);
                }
                else if (properties[DialogParameters.AV_URL] != null)
                {
                    // Audio/Video
                    result = CMSDialogHelper.GetAVItem(properties);
                }
                else if (properties[DialogParameters.LINK_URL] != null)
                {
                    // Link
                    result = CMSDialogHelper.GetLinkItem(properties);
                }
                else if (properties[DialogParameters.ANCHOR_NAME] != null)
                {
                    // Anchor
                    result = CMSDialogHelper.GetAnchorItem(properties);
                }
                else if (properties[DialogParameters.EMAIL_TO] != null)
                {
                    // Email
                    result = CMSDialogHelper.GetEmailItem(properties);
                }
                else if (properties[DialogParameters.URL_URL] != null)
                {
                    // Url
                    result = CMSDialogHelper.GetUrlItem(properties);
                }
            }
            return result;
        }


        /// <summary>
        /// Performs actions necessary to select particular item from a list.
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="ext">File extension</param>
        /// <param name="width">File width</param>
        /// <param name="height">File image height</param>
        /// <param name="size">File size</param>
        /// <param name="url">File URL</param>
        /// <param name="permanentUrl">File permanent URL</param>
        /// <param name="nodeId">Node ID</param>
        /// <param name="aliasPath">Node alias path</param>
        protected void SelectMediaItem(string name, string ext, int width, int height, long size, string url, string permanentUrl = null, int nodeId = 0, string aliasPath = "")
        {
            // Create new media item
            MediaItem selectedItem = InitializeMediaItem(name, ext, width, height, size, url, permanentUrl, 0, nodeId, aliasPath);
            SelectMediaItem(selectedItem);
        }


        /// <summary>
        /// Performs actions necessary to select particular item from a list.
        /// </summary>
        /// <param name="item">Media item to select</param>
        protected void SelectMediaItem(MediaItem item)
        {
            if (item != null)
            {
                // Get selected properties from session
                Hashtable selectedParameters = SessionHelper.GetValue("DialogSelectedParameters") as Hashtable ??
                                               new Hashtable();

                // Update selected properties
                selectedParameters = UpdateSelectedProperties(item, selectedParameters);

                // Force media properties control to load selected item
                ItemProperties.LoadSelectedItems(item, selectedParameters);

                // Update properties panel
                PropertiesUpdatePanel.Update();
            }
        }


        /// <summary>
        /// Initializes new media item object using specified details.
        /// </summary>
        /// <param name="name">Name of the file</param>
        /// <param name="ext">Extension of the file</param>
        /// <param name="imageWidth">Width of an image if applicable</param>
        /// <param name="imageHeight">Height of an image if applicable</param>
        /// <param name="size">Size of file</param>
        /// <param name="url">Url of the file</param>
        /// <param name="permanentUrl">Permanent url of the file</param>
        protected static MediaItem InitializeMediaItem(string name, string ext, int imageWidth, int imageHeight, long size, string url, string permanentUrl)
        {
            return InitializeMediaItem(name, ext, imageWidth, imageHeight, size, url, permanentUrl, 0, 0, "");
        }


        /// <summary>
        /// Initializes new media item object using specified details.
        /// </summary>
        /// <param name="name">Name of the file</param>
        /// <param name="ext">Extension of the file</param>
        /// <param name="imageWidth">Width of an image if applicable</param>
        /// <param name="imageHeight">Height of an image if applicable</param>
        /// <param name="url">Url of the file</param>
        /// <param name="permanentUrl">Permanent url of the file</param>
        /// <param name="size">Size of the item</param>
        /// <param name="historyId">ID of the attachment history</param>
        /// <param name="nodeId">ID of node</param>
        /// <param name="aliasPath">Alias path</param>
        protected static MediaItem InitializeMediaItem(string name, string ext, int imageWidth, int imageHeight, long size, string url, string permanentUrl, int historyId, int nodeId, string aliasPath)
        {
            MediaItem selectedItem = new MediaItem();

            // Fill available information
            selectedItem.Name = name;
            selectedItem.Extension = ext;
            selectedItem.Url = url;
            selectedItem.Height = imageHeight;
            selectedItem.Width = imageWidth;
            selectedItem.Size = size;
            selectedItem.PermanentUrl = permanentUrl;
            selectedItem.HistoryID = historyId;
            selectedItem.NodeID = nodeId;
            selectedItem.AliasPath = aliasPath;

            return selectedItem;
        }


        /// <summary>
        /// Clears information on selected item stored in the session when not used anymore.
        /// </summary>
        protected void ClearSelectedItemInfo()
        {
            // Clear unused information
            MediaSource = null;
        }


        /// <summary>
        /// Highlights item specified by its ID.
        /// </summary>
        /// <param name="itemId">String representation of item ID</param>
        protected void ColorizeRow(string itemId)
        {
            // Keep item selected
            ScriptHelper.RegisterStartupScript(Page, typeof(Page), "ColorizeSelectedRow", ScriptHelper.GetScript("function tryColorizeRow(itemId) { if (window.ColorizeRow) { window.ColorizeRow(itemId); } else { setTimeout(\"tryColorizeRow('\" + itemId + \"')\", 500); } }; tryColorizeRow('" + itemId + "');"));
        }
        

        /// <summary>
        /// Returns updated selected item properties.
        /// </summary>
        /// <param name="item">Selected media item</param>
        /// <param name="properties">Pelected properties</param>
        private Hashtable UpdateSelectedProperties(MediaItem item, Hashtable properties)
        {
            if (item != null)
            {
                if (properties == null)
                {
                    properties = new Hashtable();
                }
                switch (Config.OutputFormat)
                {
                    case OutputFormatEnum.HTMLMedia:
                    case OutputFormatEnum.BBMedia:
                        switch (item.MediaType)
                        {
                            case MediaTypeEnum.Image:
                                properties[DialogParameters.IMG_URL] = item.Url;
                                properties[DialogParameters.IMG_WIDTH] = item.Width;
                                properties[DialogParameters.IMG_HEIGHT] = item.Height;
                                properties[DialogParameters.IMG_EXT] = item.Extension;
                                properties[DialogParameters.LAST_TYPE] = MediaTypeEnum.Image;
                                break;

                            case MediaTypeEnum.AudioVideo:
                                properties[DialogParameters.AV_URL] = item.Url;
                                properties[DialogParameters.AV_EXT] = item.Extension;
                                properties[DialogParameters.LAST_TYPE] = MediaTypeEnum.AudioVideo;
                                break;
                        }
                        break;

                    case OutputFormatEnum.HTMLLink:
                    case OutputFormatEnum.BBLink:
                        properties[DialogParameters.LINK_URL] = item.Url;
                        break;

                    case OutputFormatEnum.URL:
                        properties[DialogParameters.URL_URL] = item.Url;
                        properties[DialogParameters.URL_WIDTH] = item.Width;
                        properties[DialogParameters.URL_HEIGHT] = item.Height;
                        properties[DialogParameters.URL_EXT] = item.Extension;
                        break;

                    case OutputFormatEnum.NodeGUID:
                        properties[DialogParameters.URL_URL] = item.Url;
                        properties[DialogParameters.URL_EXT] = item.Extension;
                        break;

                    case OutputFormatEnum.Custom:
                        break;
                }

                return properties;
            }
            return null;
        }

        #endregion
    }
}
