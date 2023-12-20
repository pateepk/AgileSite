using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.DataEngine;

using SystemIO = System.IO;

[assembly: RegisterObjectType(typeof(MetaFileInfo), MetaFileInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// MetaFileInfo data container class.
    /// </summary>
    public class MetaFileInfo : AbstractInfo<MetaFileInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.metafile";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MetaFileInfoProvider), OBJECT_TYPE, "CMS.Metafile", "MetaFileID", "MetaFileLastModified", "MetaFileGUID", null, "MetaFileName", "MetaFileBinary", "MetaFileSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("MetafileObjectID", null, ObjectDependencyEnum.Required, "MetafileObjectType"),
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.TouchParent,
            },
            LogEvents = true,
            AllowRestore = false,
            MimeTypeColumn = "MetaFileMimeType",
            ExtensionColumn = "MetaFileExtension",
            SizeColumn = "MetaFileSize",
            TouchCacheDependencies = true,
            ContainsMacros = false,
            SupportsGlobalObjects = true,
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "MetaFileCustomData" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                DependencyColumns = { "MetafileObjectID", "MetafileObjectType" },
                IdentificationField = "MetaFileGUID",
                ObjectFileNameFields = { "MetaFileName" },
                SeparatedFields = new List<SeparatedField>(){
                    new SeparatedField("MetaFileBinary")
                    {
                        IsBinaryField = true,
                        FileExtensionFieldName = "MetaFileExtension",
                        FileName = "file"
                    }
                }
            },
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Input stream - for file upload.
        /// </summary>
        protected SystemIO.Stream mInputStream = null;


        /// <summary>
        /// Attachment custom data.
        /// </summary>
        protected ContainerCustomData mMetaFileCustomData = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// MetaFile image width.
        /// </summary>
        public virtual int MetaFileImageWidth
        {
            get
            {
                return GetIntegerValue("MetaFileImageWidth", 0);
            }
            set
            {
                SetValue("MetaFileImageWidth", value, (value > 0));
            }
        }


        /// <summary>
        /// MetaFile MIME type.
        /// </summary>
        public virtual string MetaFileMimeType
        {
            get
            {
                return GetStringValue("MetaFileMimeType", string.Empty);
            }
            set
            {
                SetValue("MetaFileMimeType", value);
            }
        }


        /// <summary>
        /// MetaFile extension.
        /// </summary>
        public virtual string MetaFileExtension
        {
            get
            {
                return GetStringValue("MetaFileExtension", string.Empty);
            }
            set
            {
                SetValue("MetaFileExtension", value);
            }
        }


        /// <summary>
        /// Gets or sets meta file binary data.
        /// </summary>
        /// <remarks>
        /// When meta file binary data is not set and <see cref="InputStream"/> is provided,
        /// the binary data is loaded from the stream.
        /// </remarks>
        public virtual byte[] MetaFileBinary
        {
            get
            {
                object val = GetValue("MetaFileBinary");
                if (val == null)
                {
                    // Ensure the data
                    byte[] data = LoadDataFromInputStream();
                    return data;
                }

                return (byte[])val;
            }
            set
            {
                SetValue("MetaFileBinary", value);
            }
        }


        /// <summary>
        /// MetaFile group name.
        /// </summary>
        public virtual string MetaFileGroupName
        {
            get
            {
                return GetStringValue("MetaFileGroupName", string.Empty);
            }
            set
            {
                SetValue("MetaFileGroupName", value);
            }
        }


        /// <summary>
        /// MetaFile image height.
        /// </summary>
        public virtual int MetaFileImageHeight
        {
            get
            {
                return GetIntegerValue("MetaFileImageHeight", 0);
            }
            set
            {
                SetValue("MetaFileImageHeight", value, (value > 0));
            }
        }


        /// <summary>
        /// MetaFile object ID.
        /// </summary>
        public virtual int MetaFileObjectID
        {
            get
            {
                return GetIntegerValue("MetaFileObjectID", 0);
            }
            set
            {
                SetValue("MetaFileObjectID", value);
            }
        }


        /// <summary>
        /// MetaFile name.
        /// </summary>
        public virtual string MetaFileName
        {
            get
            {
                return GetStringValue("MetaFileName", string.Empty);
            }
            set
            {
                SetValue("MetaFileName", value);
            }
        }


        /// <summary>
        /// MetaFile ID.
        /// </summary>
        public virtual int MetaFileID
        {
            get
            {
                return GetIntegerValue("MetaFileID", 0);
            }
            set
            {
                SetValue("MetaFileID", value);
            }
        }


        /// <summary>
        /// MetaFile GUID.
        /// </summary>
        public virtual Guid MetaFileGUID
        {
            get
            {
                return GetGuidValue("MetaFileGUID", Guid.Empty);
            }
            set
            {
                SetValue("MetaFileGUID", value);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime MetaFileLastModified
        {
            get
            {
                return GetDateTimeValue("MetaFileLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MetaFileLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// MetaFile object type.
        /// </summary>
        public virtual string MetaFileObjectType
        {
            get
            {
                return GetStringValue("MetaFileObjectType", string.Empty);
            }
            set
            {
                SetValue("MetaFileObjectType", value);
            }
        }


        /// <summary>
        /// MetaFile size.
        /// </summary>
        public virtual int MetaFileSize
        {
            get
            {
                return GetIntegerValue("MetaFileSize", 0);
            }
            set
            {
                SetValue("MetaFileSize", value);
            }
        }


        /// <summary>
        /// MetaFile site ID.
        /// </summary>
        public virtual int MetaFileSiteID
        {
            get
            {
                return GetIntegerValue("MetaFileSiteID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("MetaFileSiteID", null);
                }
                else
                {
                    SetValue("MetaFileSiteID", value);
                }
            }
        }


        /// <summary>
        /// Gets or sets an input stream for loading the binary data of meta file. The loading is performed upon either <see cref="MetaFileInfoProvider.SetMetaFileInfo"/>
        /// call or <see cref="get_MetaFileBinary"/> access.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Binary data can be provided either by setting an input stream, or by setting the <see cref="MetaFileBinary"/> property.
        /// The provided input stream is properly disposed after it is processed (and this property is reset to null).
        /// </para>
        /// <para>
        /// When meta files are configured to be stored in the file system, a stream-like processing is used (i.e. the binary data is not materialized
        /// in memory).
        /// </para>
        /// </remarks>
        public virtual SystemIO.Stream InputStream
        {
            get
            {
                return mInputStream;
            }
            set
            {
                mInputStream = value;
            }
        }


        /// <summary>
        /// Binary data checksum to compare if data changed.
        /// </summary>
        internal string BinaryDataChecksum
        {
            get
            {
                return new BinaryData(EnsureBinaryData()).Checksum;
            }
        }


        /// <summary>
        /// MetaFile title.
        /// </summary>
        public virtual string MetaFileTitle
        {
            get
            {
                return GetStringValue("MetaFileTitle", string.Empty);
            }
            set
            {
                SetValue("MetaFileTitle", value);
            }
        }


        /// <summary>
        /// MetaFile description.
        /// </summary>
        public virtual string MetaFileDescription
        {
            get
            {
                return GetStringValue("MetaFileDescription", string.Empty);
            }
            set
            {
                SetValue("MetaFileDescription", value);
            }
        }


        /// <summary>
        /// MetaFile custom data.
        /// </summary>
        public ContainerCustomData MetaFileCustomData
        {
            get
            {
                if (mMetaFileCustomData == null)
                {
                    mMetaFileCustomData = new ContainerCustomData(this, "MetaFileCustomData");
                }
                return mMetaFileCustomData;
            }
        }


        /// <summary>
        /// Object parent ID
        /// </summary>
        protected override int ObjectParentID
        {
            get
            {
                return MetaFileObjectID;
            }
            set
            {
                MetaFileObjectID = value;
            }
        }


        /// <summary>
        /// Object parent type
        /// </summary>
        protected override string ParentObjectType
        {
            get
            {
                return MetaFileObjectType;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MetaFileInfoProvider.DeleteMetaFileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MetaFileInfoProvider.SetMetaFileInfo(this);
        }


        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        protected override ICollection<string> GetCacheDependencies()
        {
            var dependencies = base.GetCacheDependencies();
            dependencies.Add("metafile|" + MetaFileGUID.ToString().ToLowerCSafe());

            return dependencies;
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            if (Parent != null)
            {
                return Parent.CheckPermissions(permission, siteName, userInfo, exceptionOnFailure);
            }
            else
            {
                return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MetaFileInfo object.
        /// </summary>
        public MetaFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MetaFileInfo object from the given DataRow.
        /// </summary>
        public MetaFileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Creates a new MetaFileInfo object based on the file specified by given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="objectType">Object type</param>
        /// <param name="category">Category</param>
        public MetaFileInfo(string filePath, int objectId, string objectType, string category)
            : base(TYPEINFO)
        {
            int fileNameStartIndex = filePath.LastIndexOfCSafe("\\") + 1;

            // Init the MetaFile data
            MetaFileName = URLHelper.GetSafeFileName(filePath.Substring(fileNameStartIndex), null);
            MetaFileExtension = Path.GetExtension(filePath);

            FileInfo fileInfo = FileInfo.New(filePath);
            FileStream file = fileInfo.OpenRead();
            MetaFileSize = Convert.ToInt32(fileInfo.Length);
            MetaFileMimeType = MimeTypeHelper.GetMimetype(MetaFileExtension);

            InputStream = file;

            // Set image properties
            if (ImageHelper.IsImage(MetaFileExtension))
            {
                ImageHelper ih = new ImageHelper(MetaFileBinary);
                MetaFileImageHeight = ih.ImageHeight;
                MetaFileImageWidth = ih.ImageWidth;
            }

            MetaFileObjectID = objectId;
            MetaFileObjectType = objectType;
            MetaFileGroupName = category;
        }


        /// <summary>
        /// Creates a new AttachmentInfo object based on the file posted through the upload control.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="objectType">Object type</param>
        /// <param name="category">Category</param>
        public MetaFileInfo(HttpPostedFile postedFile, int objectId, string objectType, string category)
            : base(TYPEINFO)
        {
            int fileNameStartIndex = postedFile.FileName.LastIndexOfCSafe("\\") + 1;

            // Set the metafile
            MetaFileName = URLHelper.GetSafeFileName(postedFile.FileName.Substring(fileNameStartIndex), null);
            MetaFileExtension = Path.GetExtension(postedFile.FileName);
            MetaFileSize = postedFile.ContentLength;
            MetaFileMimeType = MimeTypeHelper.GetSafeMimeType(postedFile.FileName, postedFile.ContentType);

            // Try to load the data to the memory
            InputStream = postedFile.InputStream;

            // Set the image properties
            if (ImageHelper.IsImage(MetaFileExtension))
            {
                ImageHelper ih = new ImageHelper(MetaFileBinary);
                MetaFileImageHeight = ih.ImageHeight;
                MetaFileImageWidth = ih.ImageWidth;
            }

            MetaFileObjectID = objectId;
            MetaFileObjectType = objectType;
            MetaFileGroupName = category;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the binary data - loads the binary data from file stream if present.
        /// </summary>
        /// <param name="forceLoadFromDB">If true, the data are loaded even from DB</param>
        protected internal override byte[] EnsureBinaryData(bool forceLoadFromDB)
        {
            // Try to load the data from stream - stream has higher priority
            byte[] data = LoadDataFromInputStream();
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
                string filePath = MetaFileInfoProvider.GetFilePhysicalPath(ObjectSiteName, MetaFileGUID.ToString(), MetaFileExtension);
                if (File.Exists(filePath))
                {
                    try
                    {
                        FileInfo fileInfo = FileInfo.New(filePath);
                        using (FileStream file = fileInfo.OpenRead())
                        {
                            // Load the data from stream
                            data = LoadDataFromStream(file);
                            MetaFileBinary = data;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return data;
        }


        /// <summary>
        /// Loads the attachment data from <see cref="InputStream"/>, disposes it and sets to null.
        /// </summary>
        private byte[] LoadDataFromInputStream()
        {
            var data = LoadDataFromStream(InputStream);
            InputStream = null;

            return data;
        }


        /// <summary>
        /// Loads the attachment data from <paramref name="stream"/> and disposes it.
        /// Does nothing when <paramref name="stream"/> is null.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="stream"/> is not positioned to 0.</exception>
        private byte[] LoadDataFromStream(SystemIO.Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            // Check stream position, if possible
            if (stream.CanSeek && (stream.Position != 0))
            {
                throw new InvalidOperationException("[MetaFileInfo.LoadDataFromStream]: Stream is not at the beginning position.");
            }

            byte[] data = null;
            try
            {
                try
                {
                    data = ReadStreamToEnd(stream);

                    MetaFileBinary = data;
                    MetaFileSize = data.Length;
                }
                catch (Exception ex)
                {
                    // Log the error
                    CoreServices.EventLog.LogException("MetaFileInfo", "LoadDataFromStream", ex);
                }
            }
            finally
            {
                stream.Dispose();
            }

            return data;
        }


        /// <summary>
        /// Reads stream to byte array. Does not close the stream.
        /// </summary>
        private static byte[] ReadStreamToEnd(SystemIO.Stream stream)
        {
            long initialBufferLength = stream.CanSeek ? stream.Length : 0;
            if (initialBufferLength > Int32.MaxValue)
            {
                // Storing anything larger than 2GB in an array (which MemoryStream uses internally) is impossible anyway. The Int32.MaxValue limit is too generous.
                throw new ArgumentException("The InputStream property identifies a stream which is too long. Max supported length is Int32.MaxValue.", "stream");
            }

            const int bufferSize = 16 * 1024;
            byte[] buffer = new byte[bufferSize];

            using (var ms = new SystemIO.MemoryStream((int)initialBufferLength))
            {
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                return ms.ToArray();
            }
        }


        /// <summary>
        /// Updates the parent object, enables to update the data that is overridden in the parent object before or after it is saved
        /// </summary>
        /// <param name="parent">Parent object that will be saved</param>
        protected override void SetParent(GeneralizedInfo parent)
        {
            // Automatically synchronize thumbnail and icon GUID to the parent object
            ObjectTypeInfo ti = parent.TypeInfo;

            if (IsThumbnail(ti))
            {
                if (ObjectStatus == ObjectStatusEnum.WasDeleted)
                {
                    // Clear the thumbnail GUID
                    parent.ObjectThumbnailGUID = Guid.Empty;
                }
                else
                {
                    // Update the thumbnail GUID
                    parent.ObjectThumbnailGUID = MetaFileGUID;
                }
            }

            if (IsIcon(ti))
            {
                if (ObjectStatus == ObjectStatusEnum.WasDeleted)
                {
                    // Clear the icon GUID
                    parent.ObjectIconGUID = Guid.Empty;
                }
                else
                {
                    // Update the icon GUID
                    parent.ObjectIconGUID = MetaFileGUID;
                }
            }

            if (parent.MetaFiles != null)
            {
                parent.MetaFiles.ClearCache();
            }

            base.SetParent(parent);
        }


        /// <summary>
        /// Returns true if the MetaFile is a thumbnail metafile
        /// </summary>
        /// <param name="parentTypeInfo">Parent type info</param>
        public bool IsIcon(ObjectTypeInfo parentTypeInfo)
        {
            return (parentTypeInfo.IconGUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && MetaFileGroupName.EqualsCSafe(parentTypeInfo.IconMetaFileGroup, true);
        }


        /// <summary>
        /// Returns true if the MetaFile is a thumbnail metafile
        /// </summary>
        /// <param name="parentTypeInfo">Parent type info</param>
        public bool IsThumbnail(ObjectTypeInfo parentTypeInfo)
        {
            return (parentTypeInfo.ThumbnailGUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && MetaFileGroupName.EqualsCSafe(parentTypeInfo.ThumbnailMetaFileGroup, true);
        }


        /// <summary>
        /// Clones metafile and inserts it to DB as new object.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected internal override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Keep the original file name (do not ad suffix)
            MetaFileName = originalObject.Generalized.ObjectDisplayName;
            MetaFileObjectID = settings.ParentID;
            Insert();

            // Add warning for metafiles, except for thumbnails and icons, references of those are updated
            if ((result != null) && (settings.CloneBase != null) && (settings.CloneBase.TypeInfo.ObjectType != TypeInfo.ObjectType))
            {
                if (!MetaFileGroupName.EqualsCSafe(ObjectAttachmentsCategories.THUMBNAIL, true) &&
                    !MetaFileGroupName.EqualsCSafe(ObjectAttachmentsCategories.ICON, true))
                {
                    result.Warnings.Add(string.Format("Please update references to cloned metafile (original GUID: {0}, new GUID: {1})", originalObject.GetStringValue("MetaFileGUID", ""), MetaFileGUID));
                }
            }
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentially modified are cleared</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete all file occurrences in file system
            MetaFileInfoProvider.DeleteFile(ObjectSiteName, MetaFileGUID.ToString(), false);

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Gets the where condition to filter out the default installation data
        /// </summary>
        /// <param name="recursive">Indicates whether where condition should contain further dependency conditions.</param>
        /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override string GetDefaultDataWhereCondition(bool recursive = true, bool globalOnly = true, IEnumerable<string> excludedNames = null)
        {
            string[] dependencies = { "cms.webpart", "cms.webtemplate", "cms.widget", "ma.automationaction", "cms.workflowaction", "cms.formusercontrol", "cms.deviceprofile", "cms.layout", "cms.class" };

            string where = null;
            foreach (string dependency in dependencies)
            {
                string dependencyWhere = AddDependencyDefaultDataWhereCondition(SqlHelper.GetWhereCondition("MetaFileObjectType", new[] { dependency }), ModuleManager.GetObject(dependency), "MetaFileObjectID", recursive, "AND", null, excludedNames);

                where = SqlHelper.AddWhereCondition(where, dependencyWhere, "OR");
            }

            return SqlHelper.AddWhereCondition(base.GetDefaultDataWhereCondition(recursive, globalOnly, excludedNames), where);
        }

        #endregion
    }
}