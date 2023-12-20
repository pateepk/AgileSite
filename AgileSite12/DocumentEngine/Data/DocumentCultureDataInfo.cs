using System;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Taxonomy;
using CMS.WorkflowEngine;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(DocumentCultureDataInfo), DocumentCultureDataInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Class representing the culture specific data of a document.
    /// </summary>
    /// <remarks>
    /// This class is intended for internal usage only.
    /// </remarks>
    public class DocumentCultureDataInfo : AbstractInfo<DocumentCultureDataInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.DOCUMENTLOCALIZATION;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Variables"

        private ContainerCustomData mDocumentCustomData;
        private EditableItems mDocumentContent;

        #endregion


        #region "Data properties"

        /// <summary>
        /// Document related tree node ID.
        /// </summary>
        [DatabaseField]
        public int DocumentNodeID
        {
            get
            {
                return GetValue("DocumentNodeID", 0);
            }
            set
            {
                SetValue("DocumentNodeID", value);
            }
        }


        /// <summary>
        /// Indicates if redirection to first child document should be performed when accessed.
        /// </summary>
        [DatabaseField]
        public bool DocumentMenuRedirectToFirstChild
        {
            get
            {
                return GetValue("DocumentMenuRedirectToFirstChild", false);
            }
            set
            {
                SetValue("DocumentMenuRedirectToFirstChild", value);
            }
        }


        /// <summary>
        /// URL to which the document is redirected when accessed.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuRedirectUrl
        {
            get
            {
                return GetValue("DocumentMenuRedirectUrl", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuRedirectUrl", value);
            }
        }


        /// <summary>
        /// JavaScript code that is executed upon click on the document in the menus.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuJavascript
        {
            get
            {
                return GetValue("DocumentMenuJavascript", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuJavascript", value);
            }
        }


        /// <summary>
        /// Document name.
        /// </summary>
        [DatabaseField]
        public string DocumentName
        {
            get
            {
                return GetValue("DocumentName", String.Empty);
            }
            set
            {
                SetValue("DocumentName", value);
            }
        }


        /// <summary>
        /// Document name path.
        /// </summary>
        [DatabaseField]
        public string DocumentNamePath
        {
            get
            {
                return GetValue("DocumentNamePath", String.Empty);
            }
            set
            {
                SetValue("DocumentNamePath", value);
            }
        }


        /// <summary>
        /// Document type - contains the document extension(s) that represents.
        /// </summary>
        [DatabaseField]
        public string DocumentType
        {
            get
            {
                return GetValue("DocumentType", String.Empty);
            }
            set
            {
                SetValue("DocumentType", value);
            }
        }


        /// <summary>
        /// Document URL path.
        /// </summary>
        [DatabaseField]
        public string DocumentUrlPath
        {
            get
            {
                return GetValue("DocumentUrlPath", String.Empty);
            }
            set
            {
                SetValue("DocumentUrlPath", value);
            }
        }


        /// <summary>
        /// Document culture.
        /// </summary>
        [DatabaseField]
        public string DocumentCulture
        {
            get
            {
                return GetValue("DocumentCulture", String.Empty);
            }
            set
            {
                SetValue("DocumentCulture", value);
            }
        }


        /// <summary>
        /// Document menu caption.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuCaption
        {
            get
            {
                return GetValue("DocumentMenuCaption", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuCaption", value);
            }
        }


        /// <summary>
        /// Document menu style.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuStyle
        {
            get
            {
                return GetValue("DocumentMenuStyle", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuStyle", value);
            }
        }


        /// <summary>
        /// Document menu item image.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuItemImage
        {
            get
            {
                return GetValue("DocumentMenuItemImage", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemImage", value);
            }
        }


        /// <summary>
        /// Document menu item left image.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuItemLeftImage
        {
            get
            {
                return GetValue("DocumentMenuItemLeftImage", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemLeftImage", value);
            }
        }


        /// <summary>
        /// Document menu item right image.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuItemRightImage
        {
            get
            {
                return GetValue("DocumentMenuItemRightImage", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemRightImage", value);
            }
        }


        /// <summary>
        /// Document menu class.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuClass
        {
            get
            {
                return GetValue("DocumentMenuClass", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuClass", value);
            }
        }


        /// <summary>
        /// Document menu highlighted style.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuStyleHighlighted
        {
            get
            {
                return GetValue("DocumentMenuStyleHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuStyleHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu highlighted class.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuClassHighlighted
        {
            get
            {
                return GetValue("DocumentMenuClassHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuClassHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu item highlighted image.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuItemImageHighlighted
        {
            get
            {
                return GetValue("DocumentMenuItemImageHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemImageHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu item left image highlighted.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuItemLeftImageHighlighted
        {
            get
            {
                return GetValue("DocumentMenuItemLeftImageHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemLeftImageHighlighted", value);
            }
        }


        /// <summary>
        /// Document menu item right image highlighted.
        /// </summary>
        [DatabaseField]
        public string DocumentMenuItemRightImageHighlighted
        {
            get
            {
                return GetValue("DocumentMenuItemRightImageHighlighted", String.Empty);
            }
            set
            {
                SetValue("DocumentMenuItemRightImageHighlighted", value);
            }
        }


        /// <summary>
        /// Indicates whether item is inactive in document menu.
        /// </summary>
        [DatabaseField]
        public bool DocumentMenuItemInactive
        {
            get
            {
                return GetValue("DocumentMenuItemInactive", false);
            }
            set
            {
                SetValue("DocumentMenuItemInactive", value);
            }
        }


        /// <summary>
        /// Document page template for specific culture version.
        /// </summary>
        [DatabaseField]
        public int DocumentPageTemplateID
        {
            get
            {
                return GetValue("DocumentPageTemplateID", 0);
            }
            set
            {
                SetValue("DocumentPageTemplateID", value, value > 0);
            }
        }


        /// <summary>
        /// Document ID.
        /// </summary>
        [DatabaseField]
        public int DocumentID
        {
            get
            {
                return GetValue("DocumentID", 0);
            }
            set
            {
                SetValue("DocumentID", value);
            }
        }


        /// <summary>
        /// Indicates if document name path should be automatically used as a source value for the document URL path.
        /// </summary>
        [DatabaseField]
        public bool DocumentUseNamePathForUrlPath
        {
            get
            {
                return GetValue("DocumentUseNamePathForUrlPath", false);
            }
            set
            {
                SetValue("DocumentUseNamePathForUrlPath", value);
            }
        }


        /// <summary>
        /// Use custom document extensions.
        /// </summary>
        [DatabaseField]
        public bool DocumentUseCustomExtensions
        {
            get
            {
                return GetValue("DocumentUseCustomExtensions", false);
            }
            set
            {
                SetValue("DocumentUseCustomExtensions", value);
            }
        }


        /// <summary>
        /// Document CSS stylesheet ID.
        /// </summary>
        [DatabaseField]
        public int DocumentStylesheetID
        {
            get
            {
                return GetValue("DocumentStylesheetID", 0);
            }
            set
            {
                SetValue("DocumentStylesheetID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates if document inherits stylesheet from the parent.
        /// </summary>
        [DatabaseField]
        public bool DocumentInheritsStylesheet
        {
            get
            {
                return GetValue("DocumentInheritsStylesheet", false);
            }
            set
            {
                SetValue("DocumentInheritsStylesheet", value);
            }
        }


        /// <summary>
        /// Document foreign key ID. Connects page with custom data.
        /// </summary>
        [DatabaseField]
        public int DocumentForeignKeyValue
        {
            get
            {
                return GetValue("DocumentForeignKeyValue", 0);
            }
            set
            {
                SetValue("DocumentForeignKeyValue", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates from when the document should be published.
        /// </summary>
        [DatabaseField]
        public DateTime DocumentPublishFrom
        {
            get
            {
                return GetValue("DocumentPublishFrom", DateTime.MinValue);
            }
            set
            {
                SetValue("DocumentPublishFrom", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Indicates to when the document should be published.
        /// </summary>
        [DatabaseField]
        public DateTime DocumentPublishTo
        {
            get
            {
                return GetValue("DocumentPublishTo", DateTime.MaxValue);
            }
            set
            {
                SetValue("DocumentPublishTo", value, DateTime.MaxValue);
            }
        }


        /// <summary>
        /// Document extensions.
        /// </summary>
        [DatabaseField]
        public string DocumentExtensions
        {
            get
            {
                return GetValue("DocumentExtensions", String.Empty);
            }
            set
            {
                SetValue("DocumentExtensions", value);
            }
        }


        /// <summary>
        /// Document conversion name.
        /// </summary>
        [DatabaseField]
        public string DocumentTrackConversionName
        {
            get
            {
                return GetValue("DocumentTrackConversionName", String.Empty);
            }
            set
            {
                SetValue("DocumentTrackConversionName", value);
            }
        }


        /// <summary>
        /// Document conversion value.
        /// </summary>
        [DatabaseField]
        public string DocumentConversionValue
        {
            get
            {
                return GetValue("DocumentConversionValue", String.Empty);
            }
            set
            {
                SetValue("DocumentConversionValue", value);
            }
        }


        /// <summary>
        /// Document tags.
        /// </summary>
        [DatabaseField]
        public string DocumentTags
        {
            get
            {
                return GetValue("DocumentTags", String.Empty);
            }
            set
            {
                SetValue("DocumentTags", value);
            }
        }


        /// <summary>
        /// Document tag group ID.
        /// </summary>
        [DatabaseField]
        public int DocumentTagGroupID
        {
            get
            {
                return GetValue("DocumentTagGroupID", 0);
            }
            set
            {
                SetValue("DocumentTagGroupID", value, value > 0);
            }
        }


        /// <summary>
        /// Document wild card rule.
        /// </summary>
        [DatabaseField]
        public string DocumentWildcardRule
        {
            get
            {
                return GetValue("DocumentWildcardRule", String.Empty);
            }
            set
            {
                SetValue("DocumentWildcardRule", value);
            }
        }


        /// <summary>
        /// Sum of all ratings.
        /// </summary>
        [DatabaseField]
        public double DocumentRatingValue
        {
            get
            {
                return GetValue<double>("DocumentRatingValue", 0);
            }
            set
            {
                SetValue("DocumentRatingValue", value);
            }
        }


        /// <summary>
        /// Number of ratings.
        /// </summary>
        [DatabaseField]
        public int DocumentRatings
        {
            get
            {
                return GetValue("DocumentRatings", 0);
            }
            set
            {
                SetValue("DocumentRatings", value);
            }
        }


        /// <summary>
        /// Document priority.
        /// </summary>
        [DatabaseField]
        public int DocumentPriority
        {
            get
            {
                return GetValue("DocumentPriority", 0);
            }
            set
            {
                SetValue("DocumentPriority", value);
            }
        }


        /// <summary>
        /// Document published version history ID (latest published document version).
        /// </summary>
        [DatabaseField]
        public int DocumentPublishedVersionHistoryID
        {
            get
            {
                return GetValue("DocumentPublishedVersionHistoryID", 0);
            }
            set
            {

                SetValue("DocumentPublishedVersionHistoryID", value, value > 0);
            }
        }


        /// <summary>
        /// Document checked out version history ID (latest document version).
        /// </summary>
        [DatabaseField]
        public int DocumentCheckedOutVersionHistoryID
        {
            get
            {
                return GetValue("DocumentCheckedOutVersionHistoryID", 0);
            }
            set
            {
                SetValue("DocumentCheckedOutVersionHistoryID", value, value > 0);
            }
        }


        /// <summary>
        /// ID of a user who has checked the document out.
        /// </summary>
        [DatabaseField]
        public int DocumentCheckedOutByUserID
        {
            get
            {
                return GetValue("DocumentCheckedOutByUserID", 0);
            }
            set
            {
                SetValue("DocumentCheckedOutByUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Document workflow step ID.
        /// </summary>
        [DatabaseField]
        public int DocumentWorkflowStepID
        {
            get
            {
                return GetValue("DocumentWorkflowStepID", 0);
            }
            set
            {
                SetValue("DocumentWorkflowStepID", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates whether document is archived.
        /// </summary>
        [DatabaseField]
        public bool DocumentIsArchived
        {
            get
            {
                return GetValue("DocumentIsArchived", false);
            }
            set
            {
                SetValue("DocumentIsArchived", value);
            }
        }


        /// <summary>
        /// Returns string representing workflow action status.
        /// </summary>
        [DatabaseField]
        public string DocumentWorkflowActionStatus
        {
            get
            {
                return GetValue<string>("DocumentWorkflowActionStatus", null);
            }
            set
            {
                SetValue("DocumentWorkflowActionStatus", value);
            }
        }


        /// <summary>
        /// Indicates whether the document will be excluded from search.
        /// </summary>
        [DatabaseField]
        public bool DocumentSearchExcluded
        {
            get
            {
                return GetValue("DocumentSearchExcluded", false);
            }
            set
            {
                SetValue("DocumentSearchExcluded", value);
            }
        }


        /// <summary>
        /// Document hash.
        /// </summary>
        [DatabaseField]
        public string DocumentHash
        {
            get
            {
                return GetValue<string>("DocumentHash", null);
            }
            set
            {
                SetValue("DocumentHash", value);
            }
        }


        /// <summary>
        /// Indicates whether any activity is tracked for this document.
        /// </summary>
        [DatabaseField(ValueType = typeof(bool))]
        public bool? DocumentLogVisitActivity
        {
            get
            {
                var value = GetValue("DocumentLogVisitActivity");
                if (value == null)
                {
                    return null;
                }

                return ValidationHelper.GetValue<bool>(value);
            }
            set
            {
                SetValue("DocumentLogVisitActivity", value);
            }
        }


        /// <summary>
        /// GUID to identify the document within site.
        /// </summary>
        [DatabaseField]
        public Guid DocumentGUID
        {
            get
            {
                return GetValue("DocumentGUID", Guid.Empty);
            }
            set
            {
                SetValue("DocumentGUID", value);
            }
        }


        /// <summary>
        /// Workflow cycle GUID to obtain preview link for document.
        /// </summary>
        [DatabaseField]
        public Guid DocumentWorkflowCycleGUID
        {
            get
            {
                return GetValue("DocumentWorkflowCycleGUID", Guid.Empty);
            }
            set
            {
                SetValue("DocumentWorkflowCycleGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the site map settings in format frequency;priority.
        /// </summary>
        [DatabaseField]
        public string DocumentSitemapSettings
        {
            get
            {
                return GetValue("DocumentSitemapSettings", String.Empty);
            }
            set
            {
                SetValue("DocumentSitemapSettings", value);
            }
        }


        /// <summary>
        /// Indicates whether the document is in the process of translation (submitted to a translation service).
        /// </summary>
        [DatabaseField]
        public bool DocumentIsWaitingForTranslation
        {
            get
            {
                return GetValue("DocumentIsWaitingForTranslation", false);
            }
            set
            {
                SetValue("DocumentIsWaitingForTranslation", value);
            }
        }


        /// <summary>
        /// Indicates whether document can be considered published if published from / to matches the current time.
        /// This property is automatically updated when the object is saved and aggregates the state of other properties which participate on this status.
        /// </summary>
        [DatabaseField]
        internal bool DocumentCanBePublished
        {
            get
            {
                return GetValue("DocumentCanBePublished", true);
            }
            set
            {
                SetValue("DocumentCanBePublished", value);
            }
        }


        /// <summary>
        /// Gets or sets configuration of Page builder document widgets used for MVC support only.
        /// </summary>
        [DatabaseField]
        public string DocumentPageBuilderWidgets
        {
            get
            {
                return GetValue("DocumentPageBuilderWidgets", String.Empty);
            }
            set
            {
                SetValue("DocumentPageBuilderWidgets", value);
            }
        }


        /// <summary>
        /// Gets or sets configuration of page template used for MVC support only.
        /// </summary>
        [DatabaseField]
        public string DocumentPageTemplateConfiguration
        {
            get
            {
                return GetValue("DocumentPageTemplateConfiguration", String.Empty);
            }
            set
            {
                SetValue("DocumentPageTemplateConfiguration", value);
            }
        }


        /// <summary>
        /// Gets or sets configuration of A/B test.
        /// </summary>
        [DatabaseField]
        public string DocumentABTestConfiguration
        {
            get
            {
                return GetValue("DocumentABTestConfiguration", String.Empty);
            }
            set
            {
                SetValue("DocumentABTestConfiguration", value);
            }
        }


        /// <summary>
        /// Document custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public ContainerCustomData DocumentCustomData
        {
            get
            {
                return mDocumentCustomData ?? (mDocumentCustomData = new ContainerCustomData(this, "DocumentCustomData"));
            }
        }


        /// <summary>
        /// Document content.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public EditableItems DocumentContent
        {
            get
            {
                if (mDocumentContent == null)
                {
                    // Load the custom data
                    mDocumentContent = new EditableItems();
                    mDocumentContent.LoadContentXml(GetValue("DocumentContent", String.Empty));
                }
                return mDocumentContent;
            }
            set
            {
                SetValue("DocumentContent", value.GetContentXml());
            }
        }


        /// <summary>
        /// ID of a user who has created the document.
        /// </summary>
        [DatabaseField]
        public int DocumentCreatedByUserID
        {
            get
            {
                return GetValue("DocumentCreatedByUserID", 0);
            }
            set
            {
                SetValue("DocumentCreatedByUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Date and time when the document was created.
        /// </summary>
        [DatabaseField]
        public DateTime DocumentCreatedWhen
        {
            get
            {
                return GetValue("DocumentCreatedWhen", DateTime.MinValue);
            }
            set
            {
                SetValue("DocumentCreatedWhen", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Document group web parts (widgets).
        /// </summary>
        [DatabaseField]
        public string DocumentGroupWebParts
        {
            get
            {
                return GetValue("DocumentGroupWebParts", String.Empty);
            }
            set
            {
                SetValue("DocumentGroupWebParts", value);
            }
        }


        /// <summary>
        /// Indicates if document is checked out/in automatically.
        /// </summary>
        [DatabaseField]
        public bool DocumentCheckedOutAutomatically
        {
            get
            {
                return GetValue("DocumentCheckedOutAutomatically", true);
            }
            set
            {
                SetValue("DocumentCheckedOutAutomatically", value);
            }
        }


        /// <summary>
        /// Date and time when was the document checked out.
        /// </summary>
        [DatabaseField]
        public DateTime DocumentCheckedOutWhen
        {
            get
            {
                return GetValue("DocumentCheckedOutWhen", DateTime.MinValue);
            }
            set
            {
                SetValue("DocumentCheckedOutWhen", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Date and time when was the document last published.
        /// </summary>
        [DatabaseField]
        public DateTime DocumentLastPublished
        {
            get
            {
                return GetValue("DocumentLastPublished", DateTime.MinValue);
            }
            set
            {
                SetValue("DocumentLastPublished", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Document last version name.
        /// </summary>
        [DatabaseField]
        public string DocumentLastVersionName
        {
            get
            {
                return GetValue("DocumentLastVersionName", String.Empty);
            }
            set
            {
                SetValue("DocumentLastVersionName", value);
            }
        }


        /// <summary>
        /// Document last version number.
        /// </summary>
        [DatabaseField]
        public string DocumentLastVersionNumber
        {
            get
            {
                return GetValue("DocumentLastVersionNumber", String.Empty);
            }
            set
            {
                SetValue("DocumentLastVersionNumber", value);
            }
        }


        /// <summary>
        /// Indicates if document is hidden in navigation.
        /// </summary>
        [DatabaseField]
        public bool DocumentMenuItemHideInNavigation
        {
            get
            {
                return GetValue("DocumentMenuItemHideInNavigation", false);
            }
            set
            {
                SetValue("DocumentMenuItemHideInNavigation", value);
            }
        }


        /// <summary>
        /// ID of a user who modified the document.
        /// </summary>
        [DatabaseField]
        public int DocumentModifiedByUserID
        {
            get
            {
                return GetValue("DocumentModifiedByUserID", 0);
            }
            set
            {
                SetValue("DocumentModifiedByUserID", value, value > 0);
            }
        }


        /// <summary>
        /// Date and time when was the document modified.
        /// </summary>
        [DatabaseField]
        public DateTime DocumentModifiedWhen
        {
            get
            {
                return GetValue("DocumentModifiedWhen", DateTime.MinValue);
            }
            set
            {
                SetValue("DocumentModifiedWhen", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// Document page description.
        /// </summary>
        [DatabaseField]
        public string DocumentPageDescription
        {
            get
            {
                return GetValue("DocumentPageDescription", String.Empty);
            }
            set
            {
                SetValue("DocumentPageDescription", value);
            }
        }


        /// <summary>
        /// Document page title.
        /// </summary>
        [DatabaseField]
        public string DocumentPageTitle
        {
            get
            {
                return GetValue("DocumentPageTitle", String.Empty);
            }
            set
            {
                SetValue("DocumentPageTitle", value);
            }
        }


        /// <summary>
        /// Document page key words.
        /// </summary>
        [DatabaseField]
        public string DocumentPageKeyWords
        {
            get
            {
                return GetValue("DocumentPageKeyWords", String.Empty);
            }
            set
            {
                SetValue("DocumentPageKeyWords", value);
            }
        }


        /// <summary>
        /// Indicates if the document is visible in the site map.
        /// </summary>
        [DatabaseField]
        public bool DocumentShowInSiteMap
        {
            get
            {
                return GetValue("DocumentShowInSiteMap", true);
            }
            set
            {
                SetValue("DocumentShowInSiteMap", value);
            }
        }


        /// <summary>
        /// Document SKU description.
        /// </summary>
        [DatabaseField]
        public string DocumentSKUDescription
        {
            get
            {
                return GetValue("DocumentSKUDescription", String.Empty);
            }
            set
            {
                SetValue("DocumentSKUDescription", value);
            }
        }


        /// <summary>
        /// Document SKU name.
        /// </summary>
        [DatabaseField]
        public string DocumentSKUName
        {
            get
            {
                return GetValue("DocumentSKUName", String.Empty);
            }
            set
            {
                SetValue("DocumentSKUName", value);
            }
        }


        /// <summary>
        /// Document SKU short description.
        /// </summary>
        [DatabaseField]
        public string DocumentSKUShortDescription
        {
            get
            {
                return GetValue("DocumentSKUShortDescription", String.Empty);
            }
            set
            {
                SetValue("DocumentSKUShortDescription", value);
            }
        }


        /// <summary>
        /// Document web parts.
        /// </summary>
        [DatabaseField]
        public string DocumentWebParts
        {
            get
            {
                return GetValue("DocumentWebParts", String.Empty);
            }
            set
            {
                SetValue("DocumentWebParts", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="DocumentCultureDataInfo"/>.
        /// </summary>
        public DocumentCultureDataInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Initializes the data from the Data container, can be called only after calling the empty constructor.
        /// </summary>
        /// <param name="dc">Data container with the data</param>
        internal void InitFromDataContainer(IDataContainer dc)
        {
            LoadData(new LoadDataSettings(dc));
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        /// <returns>Returns true if the operation was successful</returns>
        public override bool SetValue(string columnName, object value)
        {
            bool result = base.SetValue(columnName, value);

            switch (columnName.ToLowerInvariant())
            {
                case "documentisarchived":
                case "documentcheckedoutversionhistoryid":
                case "documentpublishedversionhistoryid":
                    InvalidateDocumentCanBePublishedValue();
                    break;
            }

            return result;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            var result = base.TryGetValue(columnName, out value);

            switch (columnName.ToLowerInvariant())
            {
                case "documentcanbepublished":
                    value = value ?? CanBePublished();
                    result = true;
                    break;
            }

            return result;
        }


        /// <summary>
        /// Sets object
        /// </summary>
        protected override void SetObject()
        {
            DocumentCultureDataInfoProvider.SetDocumentCultureInfo(this);
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            // Don't provide automatic code name by base method, let user to specify culture explicitly
            DocumentCulture = null;

            DocumentInheritsStylesheet = true;
            DocumentShowInSiteMap = true;
            DocumentMenuItemHideInNavigation = false;
            DocumentIsArchived = false;
            DocumentUrlPath = String.Empty;
            DocumentWildcardRule = String.Empty;
            DocumentPriority = 0;
        }


        /// <summary>
        /// Method ensures that property <see cref="DocumentCanBePublished"/> is correctly initialized.
        /// </summary>
        internal void EnsureDocumentCanBePublishedValue()
        {
            DocumentCanBePublished = CanBePublished();
        }


        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = new ObjectTypeInfo(typeof(DocumentCultureDataInfoProvider), OBJECT_TYPE, "CMS.Document", "DocumentID", "DocumentModifiedWhen", "DocumentGUID", "DocumentCulture", "DocumentName", null, null, "DocumentNodeID", DocumentNodeDataInfo.OBJECT_TYPE)
            {
                CompositeObjectType = TreeNode.OBJECT_TYPE,
                DependsOn = new List<ObjectDependency>
                {
                    new ObjectDependency("DocumentModifiedByUserID", UserInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentCreatedByUserID", UserInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentCheckedOutByUserID", UserInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentCheckedOutVersionHistoryID", VersionHistoryInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentPublishedVersionHistoryID", VersionHistoryInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentPageTemplateID", PageTemplateInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentTagGroupID", TagGroupInfo.OBJECT_TYPE),
                    new ObjectDependency("DocumentWorkflowStepID", WorkflowStepInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                    new ObjectDependency("DocumentStylesheetID", CssStylesheetInfo.OBJECT_TYPE)
                },
                DependsOnIndirectly = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    CultureInfo.OBJECT_TYPE,
                    CultureSiteInfo.OBJECT_TYPE
                },
                SynchronizationSettings =
                {
                    IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                    LogSynchronization = SynchronizationTypeEnum.None
                },
                LogIntegration = false,
                LogEvents = false,
                TouchCacheDependencies = false,
                SupportsVersioning = false,

                SupportsCloning = false,
                ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },

                SerializationSettings =
                {
                    ExcludedFieldNames =
                    {
                        "DocumentForeignKeyValue",
                        "DocumentNamePath",
                        "DocumentIsWaitingForTranslation",
                        "DocumentWorkflowCycleGUID",

                        #region "Versioning"

                        "DocumentLastPublished",
                        "DocumentLastVersionName",
                        "DocumentLastVersionNumber",
                        "DocumentPublishedVersionHistoryID",
                        "DocumentWorkflowStepID",
                        "DocumentWorkflowActionStatus",
                        "DocumentCheckedOutVersionHistoryID",
                        "DocumentCheckedOutByUserID",
                        "DocumentCheckedOutAutomatically",
                        "DocumentCheckedOutWhen",
                        "DocumentCanBePublished",
                        "DocumentIsArchived"

                        #endregion
                    },
                    StructuredFields = new IStructuredField[]
                    {
                        new StructuredField<PageTemplateInstance>("DocumentWebParts"),
                        new StructuredField<PageTemplateInstance>("DocumentGroupWebParts"),
                        new StructuredField<EditableItems>("DocumentContent")
                    }
                },
                ContinuousIntegrationSettings =
                {
                    ObjectFileNameFields = { "DocumentCulture" }
                }
            };

            // Include time stamp column since the information is used to identify state of culture versions in comparison to the default culture version
            typeInfo.SerializationSettings.ExcludedFieldNames.Remove(typeInfo.TimeStampColumn);

            return typeInfo;
        }


        private bool CanBePublished()
        {
            return !DocumentIsArchived && ((DocumentCheckedOutVersionHistoryID > 0) == (DocumentPublishedVersionHistoryID > 0));
        }


        private void InvalidateDocumentCanBePublishedValue()
        {
            SetValue("DocumentCanBePublished", null);
        }

        #endregion
    }
}