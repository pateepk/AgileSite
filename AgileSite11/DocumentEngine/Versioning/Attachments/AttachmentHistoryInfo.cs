using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(AttachmentHistoryInfo), AttachmentHistoryInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(AttachmentHistoryInfo), AttachmentHistoryInfo.OBJECT_TYPE_VARIANT)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// AttachmentHistory data container class.
    /// </summary>
    public class AttachmentHistoryInfo : AbstractInfo<AttachmentHistoryInfo>, IAttachment
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.attachmenthistory";

        
        /// <summary>
        /// Object type for attachment history variants.
        /// </summary>
        public const string OBJECT_TYPE_VARIANT = "cms.attachmenthistoryvariant";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AttachmentHistoryInfoProvider), OBJECT_TYPE, "CMS.AttachmentHistory", "AttachmentHistoryID", "AttachmentLastModified", "AttachmentHistoryGUID", null, "AttachmentName", "AttachmentBinary", "AttachmentSiteID", "AttachmentDocumentID", DocumentCultureDataInfo.OBJECT_TYPE)
        {
            ModuleName = "cms.content",
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentExtension",
            OrderColumn = "AttachmentOrder",
            SizeColumn = "AttachmentSize",
            TouchCacheDependencies = true,
            TypeCondition = new TypeCondition().WhereIsNull("AttachmentVariantParentID"),
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
            },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },
            HasExternalColumns = true,
            SupportsVersioning = false,
            AllowRestore = false,
            // Exclude the attachment history from being deleted as a dependency together with a page
            // Dependency is handled explicitly when a version history of a document is deleted or when 
            // recycle bin is purged for deleted site.
            DeleteAsDependency = false
        };


        /// <summary>
        /// Type information for attachment history variant.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_VARIANT = new ObjectTypeInfo(typeof(AttachmentHistoryInfoProvider), OBJECT_TYPE_VARIANT, "CMS.AttachmentHistory", "AttachmentHistoryID", "AttachmentLastModified", "AttachmentHistoryGUID", null, "AttachmentName", "AttachmentBinary", "AttachmentSiteID", "AttachmentVariantParentID", OBJECT_TYPE)
        {
            MacroCollectionName = "AttachmentHistoryVariant",
            OriginalTypeInfo = TYPEINFO,

            ModuleName = "cms.content",
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentExtension",
            SizeColumn = "AttachmentSize",
            TypeCondition = new TypeCondition().WhereIsNotNull("AttachmentVariantParentID"),
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
            },
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("AttachmentDocumentID", DocumentCultureDataInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            TouchCacheDependencies = true,
            HasExternalColumns = true,
            SupportsVersioning = false,
            AllowRestore = false,
            // Exclude the attachment history from being deleted as a dependency together with a page
            // Dependency is handled explicitly when a version history of a document is deleted or when 
            // recycle bin is purged for deleted site.
            DeleteAsDependency = false
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Attachment custom data.
        /// </summary>
        protected ContainerCustomData mAttachmentCustomData;


        /// <summary>
        /// Extracted content from attachment binary used for search indexing.
        /// </summary>
        protected XmlData mAttachmentSearchContent;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Attachment variant parent ID.
        /// </summary>
        public virtual int AttachmentVariantParentID
        {
            get
            {
                return GetIntegerValue("AttachmentVariantParentID", 0);
            }
            set
            {
                SetValue("AttachmentVariantParentID", value);
            }
        }


        /// <summary>
        /// Text identification of used variant definition.
        /// </summary>
        public virtual string AttachmentVariantDefinitionIdentifier
        {
            get
            {
                return GetStringValue("AttachmentVariantDefinitionIdentifier", string.Empty);
            }
            set
            {
                SetValue("AttachmentVariantDefinitionIdentifier", value);
            }
        }


        /// <summary>
        /// Size of the attachment.
        /// </summary>
        public virtual int AttachmentSize
        {
            get
            {
                return GetIntegerValue("AttachmentSize", 0);
            }
            set
            {
                SetValue("AttachmentSize", value);
            }
        }


        /// <summary>
        /// Extension of the attachment.
        /// </summary>
        public virtual string AttachmentExtension
        {
            get
            {
                return GetStringValue("AttachmentExtension", "");
            }
            set
            {
                SetValue("AttachmentExtension", value);
            }
        }


        /// <summary>
        /// Name of the attachment.
        /// </summary>
        public virtual string AttachmentName
        {
            get
            {
                return GetStringValue("AttachmentName", "");
            }
            set
            {
                SetValue("AttachmentName", value);
            }
        }


        /// <summary>
        /// MIME type of the attachment.
        /// </summary>
        public virtual string AttachmentMimeType
        {
            get
            {
                return GetStringValue("AttachmentMimeType", "");
            }
            set
            {
                SetValue("AttachmentMimeType", value);
            }
        }


        /// <summary>
        /// Binary data of the attachment.
        /// </summary>
        public virtual byte[] AttachmentBinary
        {
            get
            {
                return ValidationHelper.GetBinary(GetValue("AttachmentBinary"), null);
            }
            set
            {
                SetValue("AttachmentBinary", value, null);
            }
        }


        /// <summary>
        /// Related document Document ID.
        /// </summary>
        public virtual int AttachmentDocumentID
        {
            get
            {
                return GetIntegerValue("AttachmentDocumentID", 0);
            }
            set
            {
                SetValue("AttachmentDocumentID", value);
            }
        }


        /// <summary>
        /// Width of the attachment image.
        /// </summary>
        public virtual int AttachmentImageWidth
        {
            get
            {
                return GetIntegerValue("AttachmentImageWidth", 0);
            }
            set
            {
                SetValue("AttachmentImageWidth", value, 0);
            }
        }


        /// <summary>
        /// Height of the attachment image.
        /// </summary>
        public virtual int AttachmentImageHeight
        {
            get
            {
                return GetIntegerValue("AttachmentImageHeight", 0);
            }
            set
            {
                SetValue("AttachmentImageHeight", value, 0);
            }
        }


        /// <summary>
        /// GUID of the attachment.
        /// </summary>
        public virtual Guid AttachmentGUID
        {
            get
            {
                return GetGuidValue("AttachmentGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentGUID", value);
            }
        }


        /// <summary>
        /// GUID of the attachment history (unique across whole attachment history).
        /// </summary>
        public virtual Guid AttachmentHistoryGUID
        {
            get
            {
                return GetGuidValue("AttachmentHistoryGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentHistoryGUID", value);
            }
        }


        /// <summary>
        /// Indicates if attachment is unsorted attachment.
        /// </summary>
        public virtual bool AttachmentIsUnsorted
        {
            get
            {
                return GetBooleanValue("AttachmentIsUnsorted", false);
            }
            set
            {
                SetValue("AttachmentIsUnsorted", value, (object)false);
            }
        }


        /// <summary>
        /// Stores the attachment order.
        /// </summary>
        public virtual int AttachmentOrder
        {
            get
            {
                return GetIntegerValue("AttachmentOrder", 0);
            }
            set
            {
                SetValue("AttachmentOrder", value, 0);
            }
        }


        /// <summary>
        /// Holds the GUID of the document field (group) under which the grouped attachment belongs.
        /// </summary>
        public virtual Guid AttachmentGroupGUID
        {
            get
            {
                return GetGuidValue("AttachmentGroupGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentGroupGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// History ID of the attachment.
        /// </summary>
        public virtual int AttachmentHistoryID
        {
            get
            {
                return GetIntegerValue("AttachmentHistoryID", 0);
            }
            set
            {
                SetValue("AttachmentHistoryID", value);
            }
        }


        /// <summary>
        /// Attachment hash.
        /// </summary>
        public virtual string AttachmentHash
        {
            get
            {
                return GetStringValue("AttachmentHash", string.Empty);
            }
            set
            {
                SetValue("AttachmentHash", value, null);
            }
        }


        /// <summary>
        /// Attachment title.
        /// </summary>
        public virtual string AttachmentTitle
        {
            get
            {
                return GetStringValue("AttachmentTitle", string.Empty);
            }
            set
            {
                SetValue("AttachmentTitle", value, null);
            }
        }


        /// <summary>
        /// Attachment description.
        /// </summary>
        public virtual string AttachmentDescription
        {
            get
            {
                return GetStringValue("AttachmentDescription", string.Empty);
            }
            set
            {
                SetValue("AttachmentDescription", value, null);
            }
        }


        /// <summary>
        /// Attachment custom data.
        /// </summary>
        public ContainerCustomData AttachmentCustomData
        {
            get
            {
                return mAttachmentCustomData ?? (mAttachmentCustomData = new ContainerCustomData(this, "AttachmentCustomData"));
            }
        }


        /// <summary>
        /// Attachment last modified.
        /// </summary>
        public virtual DateTime AttachmentLastModified
        {
            get
            {
                return GetDateTimeValue("AttachmentLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AttachmentLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Attachment site ID.
        /// </summary>
        public virtual int AttachmentSiteID
        {
            get
            {
                return GetIntegerValue("AttachmentSiteID", 0);
            }
            set
            {
                SetValue("AttachmentSiteID", value);
            }
        }


        /// <summary>
        /// Extracted content from attachment binary used for search indexing.
        /// </summary>
        public virtual XmlData AttachmentSearchContent
        {
            get
            {
                if (mAttachmentSearchContent == null)
                {
                    // Load the settings data
                    mAttachmentSearchContent = new XmlData();
                    mAttachmentSearchContent.LoadData(ValidationHelper.GetString(GetValue("AttachmentSearchContent"), null));
                }
                return mAttachmentSearchContent;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (AttachmentVariantParentID > 0)
                {
                    return TYPEINFO_VARIANT;
                }

                return TYPEINFO;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AttachmentHistoryInfoProvider.DeleteAttachmentHistory(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AttachmentHistoryInfoProvider.SetAttachmentHistory(this);
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            if (TypeInfo == TYPEINFO_VARIANT)
            {
                // Attachment GUID for variant can be changed between two workflow cycles
                return new WhereCondition().WhereEquals(TypeInfo.ParentIDColumn, ObjectParentID)
                                           .WhereEquals("AttachmentVariantDefinitionIdentifier", AttachmentVariantDefinitionIdentifier);
            }

            return base.GetExistingWhereCondition();
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentially modified are cleared</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            if (AttachmentVariantParentID <= 0)
            {
                RemoveVariants();
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        private void RemoveVariants()
        {
            AttachmentHistoryInfoProvider.GetAttachmentHistories()
                             .VariantsForAttachments(AttachmentHistoryID)
                             .ForEachObject(attachmentVariant => attachmentVariant.Delete());
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AttachmentHistory structure.
        /// </summary>
        public AttachmentHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates the AttachmentHistory object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the workflow info data</param>
        public AttachmentHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
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


        #region "Methods"


        /// <summary>
        /// Returns clone object of current <see cref="AttachmentHistoryInfo"/>
        /// </summary>
        /// <param name="clear">If <c>true</c>, the attachment is cleared to allow create new object</param>
        public override AttachmentHistoryInfo Clone(bool clear)
        {
            var clone = base.Clone(clear);

            if (clear)
            {
                clone.AttachmentHash = null;
            }

            return clone;
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        /// <exception cref="InvalidOperationException">Thrown when cloned instance of an attachment is variant.</exception>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            if (this.IsVariant())
            {
                ProcessVariant(settings, result, originalObject);
            }
            else
            {
                ProcessMainAttachment();
            }
        }


        private void ProcessVariant(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            base.InsertAsCloneInternal(settings, result, originalObject);
        }


        private void ProcessMainAttachment()
        {
            var lastVersionId = GetLastVersionId();

            AttachmentHistoryInfoProvider.SetAttachmentHistory(this);
            VersionAttachmentInfoProvider.SetVersionAttachmentInfo(lastVersionId, AttachmentHistoryID);
        }


        private int GetLastVersionId()
        {
            var treeProvider = new TreeProvider();
            var versionManager = VersionManager.GetInstance(treeProvider);
            var lastVersionId = versionManager.GetLatestVersionHistoryId(AttachmentGUID, new SiteInfoIdentifier(AttachmentSiteID));
            return lastVersionId;
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            if (AttachmentDocumentID > 0)
            {
                var treeProvider = new TreeProvider();
                var node = treeProvider.SelectSingleDocument(AttachmentDocumentID);
                if (node != null)
                {
                    return DocumentSecurityHelper.IsAuthorizedPerDocument(node, GetPermissionToCheck(permission), userInfo, siteName);
                }
            }

            // For the attachment which is not assigned (newly created) to a document there is not available document which can be used. 
            // Base implementation uses only module permission but this can cause false negative result - e.g. permissions through 
            // module 'cms.content' have a 'deny modify' but PageType permissions for a specific user have an 'allow modify'.
            return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Ensures the attachment history GUID
        /// </summary>
        internal new void EnsureGUID()
        {
            base.EnsureGUID();
        }

        #endregion


        #region "External columns handling"

        /// <summary>
        /// Returns path to externally stored binary data
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            var settings = new AttachmentHistoryBinarySettings
            {
                StoragePath = a => a.GetBinaryFilePath(),
                DependencyColumns = new List<string>
                {
                    "AttachmentSiteID", 
                    "AttachmentDocumentID"
                }
            };

            RegisterExternalColumn("AttachmentBinary", settings);
        }



        /// <summary>
        /// Gets the binary file path for storing the data in file system
        /// </summary>
        internal string GetBinaryFilePath()
        {
            return String.Format(
                "{0}/{1}/{2}",
                AttachmentHistoryInfoProvider.GetAttachmentHistoriesFolder(ObjectSiteName),
                AttachmentDocumentID,
                AttachmentHelper.GetFullFileName(AttachmentHistoryGUID.ToString(), AttachmentExtension)
            );
        }

        #endregion


        #region "Ordering methods"

        /// <summary>
        /// Creates where condition for sibling objects
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            var where = new WhereCondition();

            if (AttachmentVariantParentID > 0)
            {
                // Attachment variants, not sorted, but technically have siblings, always in the same version
                where.WhereEquals("AttachmentVariantParentID", AttachmentVariantParentID);
            }
            else
            {
                where.WhereNull("AttachmentVariantParentID");

                // For main attachments the siblings are attachments in the same version history ID as the latest version
                where.WhereIn(
                    "AttachmentHistoryID",
                    VersionAttachmentInfoProvider.GetVersionAttachments()
                        .Column("AttachmentHistoryID")
                        .WhereEquals(
                            "VersionHistoryID",
                            // Latest version in which the current attachment participates
                            VersionAttachmentInfoProvider.GetVersionAttachments()
                                .Column("VersionHistoryID")
                                .WhereEquals("AttachmentHistoryID", AttachmentHistoryID)
                                .TopN(1)
                                .OrderByDescending("VersionHistoryID")
                        )
                );

                if (AttachmentGroupGUID != Guid.Empty)
                {
                    // Grouped attachments
                    where.WhereEquals("AttachmentGroupGUID", AttachmentGroupGUID);
                }
                else
                {
                    // Field or unsorted attachments
                    where.WhereNull("AttachmentGroupGUID");

                    if (AttachmentIsUnsorted)
                    {
                        // Unsorted attachments
                        where.WhereTrue("AttachmentIsUnsorted");
                    }
                    else
                    {
                        // Field attachments
                        where.WhereEqualsOrNull("AttachmentIsUnsorted", false);
                    }
                }
            }

            return where;
        }

        #endregion
    }
}