using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Search;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;

using SystemIO = System.IO;

[assembly: RegisterObjectType(typeof(AttachmentInfo), AttachmentInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(AttachmentInfo), AttachmentInfo.OBJECT_TYPE_VARIANT)]
[assembly: RegisterObjectType(typeof(AttachmentInfo), AttachmentInfo.OBJECT_TYPE_TEMPORARY)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Attachment info data container class.
    /// </summary>
    public class AttachmentInfo : AbstractInfo<AttachmentInfo>, IAttachment
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.attachment";


        /// <summary>
        /// Object type for temporary attachments.
        /// </summary>
        public const string OBJECT_TYPE_TEMPORARY = "cms.temporaryattachment";


        /// <summary>
        /// Object type for attachment variants.
        /// </summary>
        public const string OBJECT_TYPE_VARIANT = "cms.attachmentvariant";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AttachmentInfoProvider), OBJECT_TYPE, "CMS.Attachment", "AttachmentID", "AttachmentLastModified", "AttachmentGuid", null, "AttachmentName", "AttachmentBinary", "AttachmentSiteID", "AttachmentDocumentID", DocumentCultureDataInfo.OBJECT_TYPE)
        {
            ModuleName = "cms.content",
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentExtension",
            OrderColumn = "AttachmentOrder",
            SearchContentColumn = "AttachmentSearchContent",
            SizeColumn = "AttachmentSize",
            SynchronizationSettings =
                {
                    LogSynchronization = SynchronizationTypeEnum.None,
                    IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },
            ContainsMacros = false,
            TypeCondition = new TypeCondition().WhereIsNull("AttachmentFormGUID").WhereIsNull("AttachmentVariantParentID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "AttachmentName" },
                SeparatedFields = new List<SeparatedField>
                {
                    new SeparatedField("AttachmentBinary")
                    {
                        IsBinaryField = true,
                        FileName = "file",
                        FileExtensionFieldName = "AttachmentExtension"
                    }
                }
            },
            SupportsVersioning = false,
            AllowRestore = false,
            // Delete attachments as dependency object using API to delete physical file from file system if configured in settings
            DeleteObjectWithAPI = true
        };


        /// <summary>
        /// Type information for temporary attachment.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_TEMPORARY = new ObjectTypeInfo(typeof(AttachmentInfoProvider), OBJECT_TYPE_TEMPORARY, "CMS.Attachment", "AttachmentID", "AttachmentLastModified", "AttachmentGuid", null, "AttachmentName", "AttachmentBinary", "AttachmentSiteID", null, null)
        {
            MacroCollectionName = "TemporaryAttachment",
            ModuleName = "cms.content",
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentExtension",
            OrderColumn = "AttachmentOrder",
            SearchContentColumn = "AttachmentSearchContent",
            SizeColumn = "AttachmentSize",
            SynchronizationSettings =
                {
                    LogSynchronization = SynchronizationTypeEnum.None,
                    IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None },
            ContainsMacros = false,
            TypeCondition = new TypeCondition().WhereIsNotNull("AttachmentFormGUID", Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")),
            SupportsVersioning = false,
            AllowRestore = false
        };


        /// <summary>
        /// Type information for attachment variant.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_VARIANT = new ObjectTypeInfo(typeof(AttachmentInfoProvider), OBJECT_TYPE_VARIANT, "CMS.Attachment", "AttachmentID", "AttachmentLastModified", "AttachmentGuid", null, "AttachmentName", "AttachmentBinary", "AttachmentSiteID", "AttachmentVariantParentID", OBJECT_TYPE)
        {
            MacroCollectionName = "AttachmentVariant",
            OriginalTypeInfo = TYPEINFO,

            ModuleName = "cms.content",
            MimeTypeColumn = "AttachmentMimeType",
            ExtensionColumn = "AttachmentExtension",
            SearchContentColumn = "AttachmentSearchContent",
            SizeColumn = "AttachmentSize",
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
            },
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("AttachmentDocumentID", DocumentCultureDataInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },
            ContainsMacros = false,
            TypeCondition = new TypeCondition().WhereIsNotNull("AttachmentVariantParentID"),
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "AttachmentName", "AttachmentVariantDefinitionIdentifier" },
                SeparatedFields = new List<SeparatedField>
                {
                    new SeparatedField("AttachmentBinary")
                    {
                        IsBinaryField = true,
                        FileName = "variant",
                        FileExtensionFieldName = "AttachmentExtension"
                    }
                }
            },
            SupportsVersioning = false,
            AllowRestore = false,
            // Variants do not touch parent, because none of the general features would use the resulting timestamp, and touching parent while editing variant could have side effects (such as copying missing binary data from FS to DB)
            AllowTouchParent = false,
            // Delete variants as dependency object using API to delete physical file from file system if configured in settings
            DeleteObjectWithAPI = true
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Input stream - for file upload.
        /// </summary>
        protected SystemIO.Stream mInputStream;


        /// <summary>
        /// True if the input stream has already been processed.
        /// </summary>
        protected bool mStreamProcessed;


        /// <summary>
        /// Attachment custom data.
        /// </summary>
        protected ContainerCustomData mAttachmentCustomData;


        /// <summary>
        /// Extracted content from attachment binary used for search indexing.
        /// </summary>
        protected XmlData mAttachmentSearchContent;


        /// <summary>
        /// Tree provider object to use for the database access.
        /// </summary>
        protected TreeProvider mTreeProvider;

        #endregion


        #region "Properties"

        /// <summary>
        /// Attachment ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentID
        {
            get
            {
                return GetIntegerValue("AttachmentID", 0);
            }
            set
            {
                SetValue("AttachmentID", value);
            }
        }


        /// <summary>
        /// Attachment variant parent ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentVariantParentID
        {
            get
            {
                return GetIntegerValue("AttachmentVariantParentID", 0);
            }
            set
            {
                SetValue("AttachmentVariantParentID", value, 0);
            }
        }


        /// <summary>
        /// Text identification of used variant definition.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentVariantDefinitionIdentifier
        {
            get
            {
                return GetStringValue("AttachmentVariantDefinitionIdentifier", string.Empty);
            }
            set
            {
                SetValue("AttachmentVariantDefinitionIdentifier", value, String.Empty);
            }
        }


        /// <summary>
        /// Attachment name.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentName
        {
            get
            {
                return GetStringValue("AttachmentName", string.Empty);
            }
            set
            {
                SetValue("AttachmentName", value);
            }
        }


        /// <summary>
        /// Attachment extension.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentExtension
        {
            get
            {
                return GetStringValue("AttachmentExtension", string.Empty);
            }
            set
            {
                SetValue("AttachmentExtension", value);
            }
        }


        /// <summary>
        /// Attachment size.
        /// </summary>
        [DatabaseField]
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
        /// Attachment mime type.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentMimeType
        {
            get
            {
                return GetStringValue("AttachmentMimeType", string.Empty);
            }
            set
            {
                SetValue("AttachmentMimeType", value);
            }
        }


        /// <summary>
        /// Attachment binary.
        /// </summary>
        [DatabaseField]
        public virtual byte[] AttachmentBinary
        {
            get
            {
                object val = GetValue("AttachmentBinary");
                if (val == null)
                {
                    // Ensure the data
                    byte[] data = LoadDataFromStream();
                    return data;
                }

                return (byte[])val;
            }
            set
            {
                SetValue("AttachmentBinary", value);
            }
        }


        /// <summary>
        /// Attachment image width.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentImageWidth
        {
            get
            {
                return GetIntegerValue("AttachmentImageWidth", 0);
            }
            set
            {
                SetValue("AttachmentImageWidth", value);
            }
        }


        /// <summary>
        /// Attachment image height.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentImageHeight
        {
            get
            {
                return GetIntegerValue("AttachmentImageHeight", 0);
            }
            set
            {
                SetValue("AttachmentImageHeight", value);
            }
        }


        /// <summary>
        /// Attachment document ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentDocumentID
        {
            get
            {
                return GetIntegerValue("AttachmentDocumentID", 0);
            }
            set
            {
                SetValue("AttachmentDocumentID", value, 0);
            }
        }


        /// <summary>
        /// Attachment GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid AttachmentGUID
        {
            get
            {
                return GetGuidValue("AttachmentGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Attachment site ID.
        /// </summary>
        [DatabaseField]
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
        /// Attachment last modified.
        /// </summary>
        [DatabaseField]
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
        /// Indicates if attachment is an unsorted attachment.
        /// </summary>
        [DatabaseField]
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
        [DatabaseField]
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
        [DatabaseField]
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
        /// Indicates that the attachment is temporary and it is bound to the specific instance of form with this GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid AttachmentFormGUID
        {
            get
            {
                return GetGuidValue("AttachmentFormGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttachmentFormGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// File input stream (for large files).
        /// </summary>
        public virtual SystemIO.Stream InputStream
        {
            get
            {
                return mInputStream;
            }
            set
            {
                mInputStream = value;

                mStreamProcessed = false;
            }
        }


        /// <summary>
        /// Attachment hash.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentHash
        {
            get
            {
                return GetStringValue("AttachmentHash", string.Empty);
            }
            set
            {
                SetValue("AttachmentHash", value);
            }
        }


        /// <summary>
        /// Attachment title.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentTitle
        {
            get
            {
                return GetStringValue("AttachmentTitle", string.Empty);
            }
            set
            {
                SetValue("AttachmentTitle", value);
            }
        }


        /// <summary>
        /// Attachment description.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentDescription
        {
            get
            {
                return GetStringValue("AttachmentDescription", string.Empty);
            }
            set
            {
                SetValue("AttachmentDescription", value);
            }
        }


        /// <summary>
        /// Extracted content from attachment binary used for search indexing.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual XmlData AttachmentSearchContent
        {
            get
            {
                if (mAttachmentSearchContent == null)
                {
                    // Load the settings data
                    mAttachmentSearchContent = new XmlData();
                    mAttachmentSearchContent.LoadData(GetStringValue("AttachmentSearchContent", ""));
                }
                return mAttachmentSearchContent;
            }
        }


        /// <summary>
        /// Attachment custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public ContainerCustomData AttachmentCustomData
        {
            get
            {
                return mAttachmentCustomData ?? (mAttachmentCustomData = new ContainerCustomData(this, "AttachmentCustomData"));
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

                if (AttachmentFormGUID == Guid.Empty)
                {
                    return TYPEINFO;
                }

                return TYPEINFO_TEMPORARY;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            // Delete the attachment through the standard provider
            AttachmentInfoProvider.DeleteAttachmentInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            // Update through standard provider
            AttachmentInfoProvider.SetAttachmentInfo(this);
        }


        /// <summary>
        /// Creates where condition for sibling objects
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            var where = new WhereCondition();

            if (AttachmentVariantParentID > 0)
            {
                // Attachment variants, not sorted, but technically have siblings
                where.WhereEquals("AttachmentVariantParentID", AttachmentVariantParentID);
            }
            else
            {
                where.WhereNull("AttachmentVariantParentID");

                if (AttachmentFormGUID != Guid.Empty)
                {
                    // Temporary attachments
                    where.WhereEquals("AttachmentFormGUID", AttachmentFormGUID);
                }
                else
                {
                    // Document attachments
                    where.WhereEquals("AttachmentDocumentID", AttachmentDocumentID);
                }

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


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            base.SetObjectOrderPostprocessing();

            // Drop the cache dependencies
            CacheHelper.TouchKeys(AttachmentInfoProvider.GetDependencyCacheKeys(AttachmentDocumentID));
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor of AttachmentInfo structure.
        /// </summary>
        public AttachmentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor of AttachmentInfo structure, creates a new AttachmentInfo object from the given DataRow data.
        /// </summary>
        public AttachmentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Creates a new AttachmentInfo object based on the file posted through the upload control.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        public AttachmentInfo(HttpPostedFile postedFile)
            : this(postedFile, 0, Guid.NewGuid())
        {
        }


        /// <summary>
        /// Creates a new AttachmentInfo object based on the file posted through the upload control.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        public AttachmentInfo(HttpPostedFile postedFile, int documentId, Guid attachmentGuid)
            : this()
        {
            int fileNameStartIndex = postedFile.FileName.LastIndexOfCSafe("\\") + 1;

            // Init the attachment data
            AttachmentName = URLHelper.GetSafeFileName(postedFile.FileName.Substring(fileNameStartIndex), null);
            AttachmentExtension = Path.GetExtension(postedFile.FileName);
            AttachmentSize = postedFile.ContentLength;
            AttachmentMimeType = MimeTypeHelper.GetMimetype(AttachmentExtension);

            // Try to load the data to the memory
            InputStream = postedFile.InputStream;

            // Set image properties
            if (ImageHelper.IsImage(AttachmentExtension))
            {
                ImageHelper ih = new ImageHelper(AttachmentBinary);
                AttachmentImageHeight = ih.ImageHeight;
                AttachmentImageWidth = ih.ImageWidth;
            }

            AttachmentDocumentID = documentId;
            AttachmentGUID = attachmentGuid;
        }


        /// <summary>
        /// Creates a new AttachmentInfo object based on the file specified by given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        public AttachmentInfo(string filePath)
            : this(filePath, 0, Guid.NewGuid())
        {
        }


        /// <summary>
        /// Creates a new AttachmentInfo object based on the file specified by given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="attachmentGuid">Attachment GUID</param>
        public AttachmentInfo(string filePath, int documentId, Guid attachmentGuid)
            : this()
        {
            // Map the virtual path to a physical file
            filePath = FileHelper.GetFullFilePhysicalPath(filePath, null);

            int fileNameStartIndex = filePath.LastIndexOfCSafe("\\") + 1;

            // Init the attachment data
            AttachmentName = URLHelper.GetSafeFileName(filePath.Substring(fileNameStartIndex), null);
            AttachmentExtension = Path.GetExtension(filePath);

            FileInfo fileInfo = FileInfo.New(filePath);
            FileStream file = fileInfo.OpenRead();
            AttachmentSize = Convert.ToInt32(fileInfo.Length);
            AttachmentMimeType = MimeTypeHelper.GetMimetype(AttachmentExtension);

            InputStream = file;

            // Set image properties
            if (ImageHelper.IsImage(AttachmentExtension))
            {
                ImageHelper ih = new ImageHelper(AttachmentBinary);
                AttachmentImageHeight = ih.ImageHeight;
                AttachmentImageWidth = ih.ImageWidth;
            }

            AttachmentDocumentID = documentId;
            AttachmentGUID = attachmentGuid;
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
        /// Returns binary data of the attachment in the BinaryData wrapper.
        /// </summary>
        protected override BinaryData GetBinaryData()
        {
            if (InputStream != null)
            {
                return new BinaryData(InputStream);
            }

            // Try to load from file system
            // Find storage place for attachment site
            var stream = GetFileStream();
            if (stream != null)
            {
                return new BinaryData(stream);
            }

            // Load the data from DB
            return new BinaryData(base.EnsureBinaryData(false));
        }


        /// <summary>
        /// Returns the search content of the attachment (including names, title, description and extracted content of the attachment binary data) which should be included to the content field of the SearchDocument.
        /// Caches the extracted value to the AttachmentSearchContent field if called for the first time (calls AttachmentInfoProvider.SetAttachmentInfo(this, false) in this case to save to value to the DB).
        /// </summary>
        /// <param name="context">Extration context passed to the text extractors</param>
        /// <param name="searchFields">Search fields collection that can be extended by extractors (DOES NOT modify the content field, this is dealt with separately - as a return value of this method)</param>
        public string EnsureSearchContent(ExtractionContext context, ISearchFields searchFields)
        {
            if (SearchHelper.IsSearchAllowedExtension(AttachmentExtension, AttachmentSiteID))
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(" ", AttachmentName);
                sb.Append(" ", AttachmentName.Replace(".", " "));

                // Add attachment title
                string title = AttachmentTitle;
                if (!string.IsNullOrEmpty(title))
                {
                    sb.Append(" ", title);
                }

                // Add attachment description
                string description = AttachmentDescription;
                if (!string.IsNullOrEmpty(description))
                {
                    sb.Append(" ", description);
                }

                // If the search content is not cached, we need to cache it after the extraction
                bool cachedValueUsed;

                // Get the extracted content
                var result = SearchHelper.GetBinaryDataSearchContent(this, context, out cachedValueUsed);
                if (result != null)
                {
                    // Set the  content field
                    var contentField = ValidationHelper.GetString(result.GetValue(SearchFieldsConstants.CONTENT), "");
                    if (!string.IsNullOrEmpty(contentField))
                    {
                        sb.Append(" ", contentField);
                    }

                    // Set all the other fields the extractor defined
                    foreach (var col in result.ColumnNames)
                    {
                        if (col != SearchFieldsConstants.CONTENT)
                        {
                            var colValue = ValidationHelper.GetString(result.GetValue(col), "");
                            searchFields.Add(SearchFieldFactory.Instance.Create(col, typeof(string), CreateSearchFieldOption.SearchableAndRetrievableWithTokenizer), () => colValue);
                        }
                    }

                    // Extraction has been processed, save the extracted value to cache
                    if (!cachedValueUsed)
                    {
                        AttachmentSearchContent.LoadData(result.GetData());
                        AttachmentInfoProvider.SetAttachmentInfo(this, false);
                    }
                }
                return sb.ToString();
            }

            return null;
        }


        /// <summary>
        /// Ensures the binary data - loads the binary data from file stream if present.
        /// </summary>
        /// <param name="forceLoadFromDB">If true, the data are loaded even from DB</param>
        protected override byte[] EnsureBinaryData(bool forceLoadFromDB)
        {
            // Load the data from stream - stream has higher priority
            byte[] data = LoadDataFromStream();
            if (data != null)
            {
                return data;
            }

            // Ensure binary data from database
            data = base.EnsureBinaryData(forceLoadFromDB);
            if (data != null)
            {
                return data;
            }

            // Data not found in DB and stream is not initialized
            if (InputStream == null)
            {
                // Try to load from file system
                // Find storage place for metafile site
                InputStream = GetFileStream();

                // Load the data from stream
                data = LoadDataFromStream();
                AttachmentBinary = data;
            }

            return data;
        }


        /// <summary>
        /// Returns the FileStream if file physically exists on disk.
        /// </summary>
        /// <returns></returns>
        private FileStream GetFileStream()
        {
            string siteName = SiteInfoProvider.GetSiteName(AttachmentSiteID);

            // Get file path for this file
            string filePath = AttachmentBinaryHelper.GetFilePhysicalPath(siteName, AttachmentGUID.ToString(), AttachmentExtension);
            if (File.Exists(filePath))
            {
                try
                {
                    FileInfo fileInfo = FileInfo.New(filePath);
                    FileStream file = fileInfo.OpenRead();
                    return file;
                }
                catch
                {
                }
            }
            return null;
        }


        /// <summary>
        /// Loads the attachment data from the input stream.
        /// </summary>
        private byte[] LoadDataFromStream()
        {
            // If no stream available or already processed no data
            if (mStreamProcessed || (InputStream == null))
            {
                return null;
            }

            // Check stream position
            if (InputStream.Position != 0)
            {
                throw new Exception("[AttachmentInfo.EnsureBinaryData]: Input stream is not at the beginning position.");
            }

            byte[] data = null;

            try
            {
                // Load from the stream
                long length = InputStream.Length;
                data = new byte[length];

                InputStream.Read(data, 0, (int)length);

                AttachmentBinary = data;
                AttachmentSize = (int)length;

                // Clear input stream
                InputStream.Close();
                InputStream.Dispose();
                InputStream = null;

                mStreamProcessed = true;
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("AttachmentInfo", "LoadDataFromStream", ex);
            }

            return data;
        }


        /// <summary>
        /// Returns clone object of current attachment info.
        /// </summary>
        /// <param name="clear">If true, the attachment is cleared to allow create new object</param>
        public override AttachmentInfo Clone(bool clear)
        {
            // Make sure binary data is loaded from stream for source attachment to clone binary data correctly
            LoadDataFromStream();

            AttachmentInfo clone = base.Clone(clear);

            if (clear)
            {
                clone.AttachmentHash = null;
            }

            return clone;
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
                var tree = new TreeProvider();
                var node = tree.SelectSingleDocument(AttachmentDocumentID);
                if (node != null)
                {
                    return DocumentSecurityHelper.IsAuthorizedPerDocument(node, GetPermissionToCheck(permission), userInfo, siteName);
                }
            }

            // For the attachment which is not assigned (newly created or empty) to a document and temporary attachments, 
            // there is not available document, which can be used. Base implementation uses only module permission but 
            // this can cause false negative result - e.g. permissions through module 'cms.content' have a 'deny modify' 
            // but PageType permissions for specific user have an 'allow modify'.
            return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
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

        #endregion     
    }
}