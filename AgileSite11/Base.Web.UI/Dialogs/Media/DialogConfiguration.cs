using System;
using System.Collections;

using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.Localization;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Container class for CMS dialogs parameters which determine the behavior of the dialogs.
    /// </summary>
    public class DialogConfiguration
    {
        #region "Variables"

        // Output format
        private OutputFormatEnum mOutputFormat = OutputFormatEnum.HTMLMedia;
        private string mCustomFormatCode = string.Empty;

        // Selectable content
        private SelectableContentEnum mSelectableContent = SelectableContentEnum.AllContent;
        private SelectablePageTypeEnum mSelectablePageTypes = SelectablePageTypeEnum.All;

        // Tabs
        private bool? mHideAttachments;

        // Attachments
        private int mAttachmentDocumentID = -1;
        private Guid mAttachmentFormGUID = Guid.Empty;
        private int mAttachmentParentID;

        // Content tab
        private AvailableSitesEnum mContentSites = AvailableSitesEnum.All;
        private string mContentSelectedSite = string.Empty;
        private string mContentStartingPath = string.Empty;

        // Media libraries tab
        private AvailableSitesEnum mLibSites = AvailableSitesEnum.All;
        private string mLibSelectedSite = string.Empty;
        private AvailableLibrariesEnum mLibGlobalLibraries = AvailableLibrariesEnum.All;
        private AvailableLibrariesEnum mLibGroupLibraries = AvailableLibrariesEnum.All;
        private AvailableGroupsEnum mLibGroups = AvailableGroupsEnum.All;
        private string mLibGlobalLibraryName = string.Empty;
        private string mLibGroupLibraryName = string.Empty;
        private string mLibGroupName = string.Empty;
        private string mLibStartingPath = string.Empty;

        // Image autoresize

        // Dialog dimensions
        private int mDialogWidth = 95;
        private int mDialogHeight = 86;
        private bool mUseRelativeDimensions = true;

        // Additional parameters
        private string mCulture;

        #endregion


        #region "Properties"

        /// <summary>
        /// Culture the dialog works with.
        /// </summary>
        public string Culture
        {
            get
            {
                return mCulture ?? (mCulture = LocalizationContext.PreferredCultureCode);
            }
            set
            {
                mCulture = value;
            }
        }


        /// <summary>
        /// Dialog output format.
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
        /// Indicates whether returned URL should be relative.
        /// </summary>
        public bool ContentUseRelativeUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Dialog selectable content.
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
        /// Dialog selectable page types.
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
        /// Code of the custom output format.
        /// </summary>
        public string CustomFormatCode
        {
            get
            {
                return mCustomFormatCode;
            }
            set
            {
                mCustomFormatCode = value;
            }
        }


        /// <summary>
        /// Indicates if 'Attachments' tab should be hidden.
        /// </summary>
        public bool HideAttachments
        {
            get
            {
                if (mHideAttachments == null)
                {
                    if ((AttachmentFormGUID == Guid.Empty) && (AttachmentDocumentID == 0) && (MetaFileObjectID == 0))
                    {
                        mHideAttachments = true;
                    }
                    else
                    {
                        mHideAttachments = false;
                    }
                }
                return mHideAttachments.Value;
            }
            set
            {
                mHideAttachments = value;
            }
        }


        /// <summary>
        /// Indicates if 'Content' tab should be hidden.
        /// </summary>
        public bool HideContent
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if 'Media libraries' tab should be hidden.
        /// </summary>
        public bool HideLibraries
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if 'E-mail' tab should be hidden.
        /// </summary>
        public bool HideEmail
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if 'Anchor' tab should be hidden.
        /// </summary>
        public bool HideAnchor
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if 'Web' tab should be hidden.
        /// </summary>
        public bool HideWeb
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the existing document the attachment should be uploaded to.
        /// </summary>
        public int AttachmentDocumentID
        {
            get
            {
                if (mAttachmentDocumentID < 0)
                {
                    if (mAttachmentFormGUID == Guid.Empty)
                    {
                        // Take current page document ID
                        PageInfo currentPage = DocumentContext.CurrentPageInfo;
                        if (currentPage != null)
                        {
                            mAttachmentDocumentID = currentPage.DocumentID;
                        }
                    }

                    if (mAttachmentDocumentID < 0)
                    {
                        // Not available
                        mAttachmentDocumentID = 0;
                    }
                }

                return mAttachmentDocumentID;
            }
            set
            {
                mAttachmentDocumentID = value;
            }
        }


        /// <summary>
        /// GUID of the form the temporary attachment should be uploaded to before the document is created.
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
            }
        }


        /// <summary>
        /// Parent ID of the document the attachment should be added to.
        /// </summary>
        public int AttachmentParentID
        {
            get
            {
                if ((mAttachmentParentID == 0) && (DocumentContext.CurrentPageInfo != null))
                {
                    mAttachmentParentID = DocumentContext.CurrentPageInfo.NodeParentID;
                }
                return mAttachmentParentID;
            }
            set
            {
                mAttachmentParentID = value;
            }
        }


        /// <summary>
        /// ID that, together with MetaFileObjectType and MetaFileCategory, specifies object where metafiles should be uploaded to.
        /// </summary>
        public int MetaFileObjectID
        {
            get;
            set;
        }


        /// <summary>
        /// Object type that, together with MetaFileObjectID and MetaFileCategory, specifies object where metafiles should be uploaded to.
        /// </summary>
        public string MetaFileObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object category that, together with MetaFileObjectID and MetaFileObjectType, specifies object where metafiles should be uploaded to.
        /// </summary>
        public string MetaFileCategory
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the selected site on the 'Content' tab.
        /// </summary>
        public string ContentSelectedSite
        {
            get
            {
                return mContentSelectedSite;
            }
            set
            {
                mContentSelectedSite = value;
            }
        }


        /// <summary>
        /// Starting path of the content tree on the 'Content' tab.
        /// </summary>
        public string ContentStartingPath
        {
            get
            {
                return mContentStartingPath;
            }
            set
            {
                mContentStartingPath = value;
            }
        }


        /// <summary>
        /// Indicates which media library sites are available.
        /// </summary>
        public AvailableSitesEnum LibSites
        {
            get
            {
                return mLibSites;
            }
            set
            {
                mLibSites = value;
            }
        }


        /// <summary>
        /// Indicates which content sites are available.
        /// </summary>
        public AvailableSitesEnum ContentSites
        {
            get
            {
                return mContentSites;
            }
            set
            {
                mContentSites = value;
            }
        }


        /// <summary>
        /// Name of the selected site on the 'Media libraries' tab.
        /// </summary>
        public string LibSelectedSite
        {
            get
            {
                return mLibSelectedSite;
            }
            set
            {
                mLibSelectedSite = value;
            }
        }


        /// <summary>
        /// Indicates which global media libraries are available.
        /// </summary>
        public AvailableLibrariesEnum LibGlobalLibraries
        {
            get
            {
                return mLibGlobalLibraries;
            }
            set
            {
                mLibGlobalLibraries = value;
            }
        }


        /// <summary>
        /// Indicates which group media libraries are available.
        /// </summary>
        public AvailableLibrariesEnum LibGroupLibraries
        {
            get
            {
                return mLibGroupLibraries;
            }
            set
            {
                mLibGroupLibraries = value;
            }
        }


        /// <summary>
        /// Indicates which groups are available.
        /// </summary>
        public AvailableGroupsEnum LibGroups
        {
            get
            {
                return mLibGroups;
            }
            set
            {
                mLibGroups = value;
            }
        }


        /// <summary>
        /// Name of the available global media library. Set when only one global media library should be available.
        /// </summary>
        public string LibGlobalLibraryName
        {
            get
            {
                return mLibGlobalLibraryName;
            }
            set
            {
                mLibGlobalLibraryName = value;
            }
        }


        /// <summary>
        /// Name of the available group media library. Set when only one group media library should be available.
        /// </summary>
        public string LibGroupLibraryName
        {
            get
            {
                return mLibGroupLibraryName;
            }
            set
            {
                mLibGroupLibraryName = value;
            }
        }


        /// <summary>
        /// Name of the available group. Set when only one group should be available.
        /// </summary>
        public string LibGroupName
        {
            get
            {
                return mLibGroupName;
            }
            set
            {
                mLibGroupName = value;
            }
        }


        /// <summary>
        /// Starting path of the media library folder tree.
        /// </summary>
        public string LibStartingPath
        {
            get
            {
                return mLibStartingPath;
            }
            set
            {
                mLibStartingPath = value;
            }
        }


        /// <summary>
        /// New image width after it is uploaded.
        /// </summary>
        public int ResizeToWidth
        {
            get;
            set;
        }


        /// <summary>
        /// New image height after it is uploaded.
        /// </summary>
        public int ResizeToHeight
        {
            get;
            set;
        }


        /// <summary>
        /// New image max side size after it is uploaded.
        /// </summary>
        public int ResizeToMaxSideSize
        {
            get;
            set;
        }


        /// <summary>
        /// Width of the dialog.
        /// </summary>
        public int DialogWidth
        {
            get
            {
                return mDialogWidth;
            }
            set
            {
                mDialogWidth = value;
            }
        }


        /// <summary>
        /// Height of the dialog.
        /// </summary>
        public int DialogHeight
        {
            get
            {
                return mDialogHeight;
            }
            set
            {
                mDialogHeight = value;
            }
        }


        /// <summary>
        /// Indicates if dialog width/height are set as relative to the total width/height of the screen.
        /// </summary>
        public bool UseRelativeDimensions
        {
            get
            {
                return mUseRelativeDimensions;
            }
            set
            {
                mUseRelativeDimensions = value;
            }
        }


        /// <summary>
        /// Client id of editor area where content should be inserted.
        /// </summary>
        public string EditorClientID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if absolute URL should be used in dialogs.
        /// </summary>
        public bool UseFullURL
        {
            get;
            set;
        }


        /// <summary>
        /// Additional query parameters appended to URL generated for current config.
        /// </summary>
        public string AdditionalQueryParameters
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true, dialog will not display input fields for specifying size and alt text of image. 
        /// 
        /// Default is false, which means that size inputs will be displayed or hidden according to other settings.
        /// </summary>
        public bool UseSimpleURLProperties { get; set; }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - creates dialog default configuration.
        /// </summary>
        public DialogConfiguration()
        {
        }


        /// <summary>
        /// Constructor - creates dialog configuration from the parameters.
        /// </summary>
        /// <param name="parameters">Collection with dialog configuration parameters</param>
        public DialogConfiguration(Hashtable parameters)
        {
            // Output format
            string output = ValidationHelper.GetString(parameters["dialogs_output"], "html");
            bool link = ValidationHelper.GetBoolean(parameters["dialogs_link"], false);
            OutputFormat = CMSDialogHelper.GetOutputFormat(output, link);

            // Dialog tabs
            HideAttachments = ValidationHelper.GetBoolean(parameters["dialogs_attachments_hide"], false);
            ContentUseRelativeUrl = ValidationHelper.GetBoolean(parameters["dialogs_content_userelativeurl"], true);            
            HideContent = ValidationHelper.GetBoolean(parameters["dialogs_content_hide"], false);
            HideLibraries = ValidationHelper.GetBoolean(parameters["dialogs_libraries_hide"], false);
            HideEmail = ValidationHelper.GetBoolean(parameters["dialogs_email_hide"], false);
            HideAnchor = ValidationHelper.GetBoolean(parameters["dialogs_anchor_hide"], false);
            HideWeb = ValidationHelper.GetBoolean(parameters["dialogs_web_hide"], false);

            // Content tab
            string selectedSite = ValidationHelper.GetString(parameters["dialogs_content_site"], string.Empty);
            ContentSites = GetAvailableSites(selectedSite);
            if (ContentSites == AvailableSitesEnum.OnlySingleSite)
            {
                ContentSelectedSite = selectedSite;
            }

            string startingPath = ValidationHelper.GetString(parameters["dialogs_content_path"], string.Empty);
            ContentStartingPath = String.IsNullOrEmpty(startingPath) ? String.Empty : startingPath.TrimEnd('/');

            // Media libraries tab
            selectedSite = ValidationHelper.GetString(parameters["dialogs_libraries_site"], string.Empty);
            LibSites = GetAvailableSites(selectedSite);
            if (LibSites == AvailableSitesEnum.OnlySingleSite)
            {
                LibSelectedSite = selectedSite;
            }

            LibStartingPath = ValidationHelper.GetString(parameters["dialogs_libraries_path"], string.Empty);

            string items = ValidationHelper.GetString(parameters["dialogs_libraries_global"], string.Empty);
            LibGlobalLibraries = GetAvailableLibraries(items);
            LibGlobalLibraryName = ValidationHelper.GetString(parameters["dialogs_libraries_global_libname"], string.Empty);

            items = ValidationHelper.GetString(parameters["dialogs_groups"], string.Empty);
            LibGroups = GetAvailableGroups(items);
            LibGroupName = ValidationHelper.GetString(parameters["dialogs_groups_name"], string.Empty);

            items = ValidationHelper.GetString(parameters["dialogs_libraries_group"], string.Empty);
            LibGroupLibraries = GetAvailableLibraries(items);
            LibGroupLibraryName = ValidationHelper.GetString(parameters["dialogs_libraries_group_libname"], string.Empty);

            int width;
            int height;
            int maxSideSize;

            // Set image auto resize dimensions
            ImageHelper.GetAutoResizeDimensions(parameters, SiteContext.CurrentSiteName, out width, out height, out maxSideSize);
            ResizeToWidth = width;
            ResizeToHeight = height;
            ResizeToMaxSideSize = maxSideSize;

            // Additional parameters
            EditorClientID = ValidationHelper.GetString(parameters["editor_clientid"], string.Empty);
            AdditionalQueryParameters = ValidationHelper.GetString(parameters["dialogs_additional_query"], string.Empty);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns dialog configuration from current query string.
        /// </summary>        
        public static DialogConfiguration GetDialogConfiguration()
        {
            DialogConfiguration config = new DialogConfiguration();

            // Output format
            string output = QueryHelper.GetString("output", "html");
            bool link = QueryHelper.GetBoolean("link", false);
            config.OutputFormat = CMSDialogHelper.GetOutputFormat(output, link);
            if (config.OutputFormat == OutputFormatEnum.Custom)
            {
                config.CustomFormatCode = output;
            }

            // Selectable content
            string content = QueryHelper.GetString("content", string.Empty);
            config.SelectableContent = CMSDialogHelper.GetSelectableContent(content);

            // Selectable page types
            string pageTypes = QueryHelper.GetString("pagetypes", string.Empty);
            config.SelectablePageTypes = pageTypes.ToEnum<SelectablePageTypeEnum>();

            // Dialog tabs
            config.HideAttachments = QueryHelper.GetBoolean("attachments_hide", false);
            config.HideContent = QueryHelper.GetBoolean("content_hide", false);
            config.HideLibraries = QueryHelper.GetBoolean("libraries_hide", false);
            config.HideEmail = QueryHelper.GetBoolean("email_hide", false);
            config.HideAnchor = QueryHelper.GetBoolean("anchor_hide", false);
            config.HideWeb = QueryHelper.GetBoolean("web_hide", false);

            // Attachments tab
            config.AttachmentDocumentID = QueryHelper.GetInteger("documentid", 0);
            config.AttachmentFormGUID = QueryHelper.GetGuid("formguid", Guid.Empty);
            config.AttachmentParentID = QueryHelper.GetInteger("parentid", 0);
            config.MetaFileObjectID = QueryHelper.GetInteger("objectid", 0);
            config.MetaFileObjectType = QueryHelper.GetString("objecttype", string.Empty);
            config.MetaFileCategory = QueryHelper.GetString("objectcategory", string.Empty);

            // Content tab            
            config.ContentSites = GetAvailableSites(QueryHelper.GetString("content_sites", "all"));
            config.ContentSelectedSite = QueryHelper.GetString("content_site", string.Empty);
            config.ContentStartingPath = QueryHelper.GetString("content_path", string.Empty);
            config.Culture = QueryHelper.GetString("content_culture", string.Empty);
            config.ContentUseRelativeUrl = QueryHelper.GetBoolean("content_userelativeurl", false);

            // Media libraries tab
            config.LibSites = GetAvailableSites(QueryHelper.GetString("libraries_sites", "all"));
            config.LibSelectedSite = QueryHelper.GetString("libraries_site", string.Empty);
            config.LibStartingPath = QueryHelper.GetString("libraries_path", string.Empty);

            string items = QueryHelper.GetString("libraries_global", string.Empty);
            config.LibGlobalLibraries = GetAvailableLibraries(items);
            config.LibGlobalLibraryName = QueryHelper.GetString("libraries_global_name", string.Empty);

            items = QueryHelper.GetString("groups", string.Empty);
            config.LibGroups = GetAvailableGroups(items);
            config.LibGroupName = QueryHelper.GetString("groups_name", string.Empty);

            items = QueryHelper.GetString("libraries_group", string.Empty);
            config.LibGroupLibraries = GetAvailableLibraries(items);
            config.LibGroupLibraryName = QueryHelper.GetString("libraries_group_name", string.Empty);

            // Image auto resize
            config.ResizeToWidth = QueryHelper.GetInteger("autoresize_width", 0);
            config.ResizeToHeight = QueryHelper.GetInteger("autoresize_height", 0);
            config.ResizeToMaxSideSize = QueryHelper.GetInteger("autoresize_maxsidesize", 0);

            // Additional parameters
            config.EditorClientID = QueryHelper.GetString("editor_clientid", string.Empty);
            config.UseFullURL = QueryHelper.GetBoolean("fullurl", false);
            config.UseSimpleURLProperties = QueryHelper.GetBoolean("simple_url_properties", false);
            
            return config;
        }


        /// <summary>
        /// Clones current dialog configuration.
        /// </summary>        
        public DialogConfiguration Clone()
        {
            DialogConfiguration config = new DialogConfiguration();

            // Selectable content
            config.SelectableContent = SelectableContent;
            config.SelectablePageTypes = SelectablePageTypes;

            // Tabs            
            config.HideAttachments = HideAttachments;
            config.HideContent = HideContent;
            config.HideLibraries = HideLibraries;
            config.HideAnchor = HideAnchor;
            config.HideEmail = HideEmail;
            config.HideWeb = HideWeb;

            // Attachments tab
            config.AttachmentDocumentID = AttachmentDocumentID;
            config.AttachmentFormGUID = AttachmentFormGUID;
            config.AttachmentParentID = AttachmentParentID;
            config.MetaFileObjectID = MetaFileObjectID;
            config.MetaFileObjectType = MetaFileObjectType;
            config.MetaFileCategory = MetaFileCategory;

            // Content tab
            config.ContentSites = ContentSites;
            config.ContentSelectedSite = ContentSelectedSite;
            config.ContentStartingPath = ContentStartingPath;
            config.ContentUseRelativeUrl = ContentUseRelativeUrl;

            // Media libraries tab
            config.LibSites = LibSites;
            config.LibGlobalLibraries = LibGlobalLibraries;
            config.LibGlobalLibraryName = LibGlobalLibraryName;
            config.LibGroupLibraries = LibGroupLibraries;
            config.LibGroupLibraryName = LibGroupLibraryName;
            config.LibGroups = LibGroups;
            config.LibGroupName = LibGroupName;
            config.LibSelectedSite = LibSelectedSite;
            config.LibStartingPath = LibStartingPath;

            // Output format
            config.CustomFormatCode = CustomFormatCode;
            config.OutputFormat = OutputFormat;

            // Dialog dimensions
            config.DialogHeight = DialogHeight;
            config.DialogWidth = DialogWidth;

            // Image autoresize
            config.ResizeToHeight = ResizeToHeight;
            config.ResizeToMaxSideSize = ResizeToMaxSideSize;
            config.ResizeToWidth = ResizeToWidth;

            // Additional parameters
            config.EditorClientID = EditorClientID;
            config.UseFullURL = UseFullURL;
            config.AdditionalQueryParameters = AdditionalQueryParameters;
            config.UseSimpleURLProperties = UseSimpleURLProperties;

            return config;
        }

        #endregion


        #region "Private methods"

        private static AvailableLibrariesEnum GetAvailableLibraries(string libraries)
        {
            // Normalize libraries coming from URL
            libraries = libraries.Trim('#');

            switch (libraries)
            {
                case "none":
                    return AvailableLibrariesEnum.None;

                case "current":
                    return AvailableLibrariesEnum.OnlyCurrentLibrary;

                case "single":
                    return AvailableLibrariesEnum.OnlySingleLibrary;

                default:
                    return AvailableLibrariesEnum.All;
            }
        }


        private static AvailableGroupsEnum GetAvailableGroups(string groups)
        {
            // Normalize groups coming from URL
            groups = groups.Trim('#');

            switch (groups)
            {
                case "none":
                    return AvailableGroupsEnum.None;

                case "current":
                    return AvailableGroupsEnum.OnlyCurrentGroup;

                case "single":
                    return AvailableGroupsEnum.OnlySingleGroup;

                default:
                    return AvailableGroupsEnum.All;
            }
        }


        /// <summary>
        /// Transforms string representation of available sites to its enumeration equivalent.
        /// </summary>
        /// <param name="sites">String to transform</param>
        private static AvailableSitesEnum GetAvailableSites(string sites)
        {
            // Normalize groups coming from URL
            sites = sites.Trim('#');

            switch (sites)
            {
                case "current":
                    return AvailableSitesEnum.OnlyCurrentSite;

                case "all":
                case "":
                    return AvailableSitesEnum.All;

                default:
                    return AvailableSitesEnum.OnlySingleSite;
            }
        }

        #endregion
    }
}