using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Synchronization;
using CMS.MediaLibrary;

using SystemIO = System.IO;

[assembly: RegisterObjectType(typeof(MediaFileInfo), MediaFileInfo.OBJECT_TYPE)]

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Media file info data container class.
    /// </summary>
    public class MediaFileInfo : AbstractInfo<MediaFileInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.MEDIAFILE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MediaFileInfoProvider), OBJECT_TYPE, "Media.File", "FileID", "FileModifiedWhen", "FileGUID", null, "FileName", null, "FileSiteID", "FileLibraryID", MediaLibraryInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("FileCreatedByUserID", UserInfo.OBJECT_TYPE),
                new ObjectDependency("FileModifiedByUserID", UserInfo.OBJECT_TYPE)
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "FilePath" }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ModuleName = "cms.medialibrary",
            RegisterAsChildToObjectTypes = new List<string> { MediaLibraryInfo.OBJECT_TYPE, MediaLibraryInfo.OBJECT_TYPE_GROUP },
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false, LogProgress = false }
        };

        #endregion


        #region "Variables"

        private ContainerCustomData mFileCustomData;

        #endregion


        #region "Properties"

        /// <summary>
        /// File custom data.
        /// </summary>
        public ContainerCustomData FileCustomData
        {
            get
            {
                return mFileCustomData ?? (mFileCustomData = new ContainerCustomData(this, "FileCustomData"));
            }
        }


        /// <summary>
        /// MediaFile binary - for file information transfer purposes, not saved to the database.
        /// </summary>
        public virtual byte[] FileBinary
        {
            get;
            set;
        }


        /// <summary>
        /// MediaFile binary stream - for file information transfer purposes, not saved to the database.
        /// </summary>
        public virtual SystemIO.Stream FileBinaryStream
        {
            get;
            set;
        }


        /// <summary>
        /// File extension.
        /// </summary>
        public virtual string FileExtension
        {
            get
            {
                return GetStringValue("FileExtension", "");
            }
            set
            {
                SetValue("FileExtension", value);
            }
        }


        /// <summary>
        /// File library ID.
        /// </summary>
        public virtual int FileLibraryID
        {
            get
            {
                return GetIntegerValue("FileLibraryID", 0);
            }
            set
            {
                SetValue("FileLibraryID", value);
            }
        }


        /// <summary>
        /// File GUID.
        /// </summary>
        public virtual Guid FileGUID
        {
            get
            {
                return GetGuidValue("FileGUID", Guid.Empty);
            }
            set
            {
                SetValue("FileGUID", value);
            }
        }


        /// <summary>
        /// Date and time when the file was last modified.
        /// </summary>
        public virtual DateTime FileModifiedWhen
        {
            get
            {
                return GetDateTimeValue("FileModifiedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileModifiedWhen", value);
            }
        }


        /// <summary>
        /// ID of user who last modified current file.
        /// </summary>
        public virtual int FileModifiedByUserID
        {
            get
            {
                return GetIntegerValue("FileModifiedByUserID", 0);
            }
            set
            {
                SetValue("FileModifiedByUserID", value);
            }
        }


        /// <summary>
        /// Date ant time when the file was created.
        /// </summary>
        public virtual DateTime FileCreatedWhen
        {
            get
            {
                return GetDateTimeValue("FileCreatedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileCreatedWhen", value);
            }
        }


        /// <summary>
        /// ID of user who created current file.
        /// </summary>
        public virtual int FileCreatedByUserID
        {
            get
            {
                return GetIntegerValue("FileCreatedByUserID", 0);
            }
            set
            {
                SetValue("FileCreatedByUserID", value);
            }
        }


        /// <summary>
        /// File ID.
        /// </summary>
        public virtual int FileID
        {
            get
            {
                return GetIntegerValue("FileID", 0);
            }
            set
            {
                SetValue("FileID", value);
            }
        }


        /// <summary>
        /// File size.
        /// </summary>
        public virtual long FileSize
        {
            get
            {
                return GetIntegerValue("FileSize", 0);
            }
            set
            {
                SetValue("FileSize", value);
            }
        }


        /// <summary>
        /// File site ID.
        /// </summary>
        public virtual int FileSiteID
        {
            get
            {
                return GetIntegerValue("FileSiteID", 0);
            }
            set
            {
                SetValue("FileSiteID", value);
            }
        }


        /// <summary>
        /// File image width.
        /// </summary>
        public virtual int FileImageWidth
        {
            get
            {
                return GetIntegerValue("FileImageWidth", 0);
            }
            set
            {
                SetValue("FileImageWidth", value, value > 0);
            }
        }


        /// <summary>
        /// File path.
        /// </summary>
        public virtual string FilePath
        {
            get
            {
                return GetStringValue("FilePath", "");
            }
            set
            {
                SetValue("FilePath", value);
            }
        }


        /// <summary>
        /// File name.
        /// </summary>
        public virtual string FileName
        {
            get
            {
                return GetStringValue("FileName", "");
            }
            set
            {
                SetValue("FileName", value);
            }
        }


        /// <summary>
        /// File title.
        /// </summary>
        public virtual string FileTitle
        {
            get
            {
                return GetStringValue("FileTitle", "");
            }
            set
            {
                SetValue("FileTitle", value);
            }
        }


        /// <summary>
        /// File description.
        /// </summary>
        public virtual string FileDescription
        {
            get
            {
                return GetStringValue("FileDescription", "");
            }
            set
            {
                SetValue("FileDescription", value);
            }
        }


        /// <summary>
        /// File mime type.
        /// </summary>
        public virtual string FileMimeType
        {
            get
            {
                return GetStringValue("FileMimeType", "");
            }
            set
            {
                SetValue("FileMimeType", value);
            }
        }


        /// <summary>
        /// File image height.
        /// </summary>
        public virtual int FileImageHeight
        {
            get
            {
                return GetIntegerValue("FileImageHeight", 0);
            }
            set
            {
                SetValue("FileImageHeight", value, value > 0);
            }
        }


        /// <summary>
        /// Indicates if the object versioning is supported.
        /// </summary>
        protected override bool SupportsVersioning
        {
            get
            {
                return base.SupportsVersioning && ObjectVersionManager.IsObjectExtensionVersioned(TypeInfo.ObjectType, FileExtension, ObjectSiteName);
            }
            set
            {
                base.SupportsVersioning = value;
            }
        }



        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MediaFileInfoProvider.DeleteMediaFileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MediaFileInfoProvider.SetMediaFileInfo(this);
        }


        /// <summary>
        /// Gets media files physical files.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="binaryData">If true, gets the binary data to the DataSet</param>
        protected override DataSet GetPhysicalFiles(OperationTypeEnum operationType, bool binaryData)
        {
            var manager = new FileBinaryDataSyncManager(this);
            return manager.GetPhysicalFiles(operationType, binaryData);
        }


        /// <summary>
        /// Saves media file physical files.
        /// </summary>
        /// <param name="filesData">DataSet with physical files data</param>
        protected override void UpdatePhysicalFiles(DataSet filesData)
        {
            var manager = new FileBinaryDataSyncManager(this);
            manager.UpdatePhysicalFiles(filesData);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MediaFileInfo object.
        /// </summary>
        public MediaFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new MediaFileInfo object from the existing MediaFileInfo object.
        /// Copy constructor.
        /// </summary>
        /// <param name="info">Original object to copy</param>
        /// <param name="keepSourceData">If true, the copy is shallow, otherwise a deep copy is created (all object's data is cloned)</param>
        public MediaFileInfo(MediaFileInfo info, bool keepSourceData)
            : base(TYPEINFO, info.DataClass, keepSourceData)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MediaFileInfo object from the given DataRow.
        /// </summary>
        public MediaFileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object from posted file.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        public MediaFileInfo(HttpPostedFile postedFile, int libraryId)
            : this(postedFile, libraryId, "")
        {
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object from posted file.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        /// <param name="librarySubfolder">Library subfolder path where file should be saved</param>
        public MediaFileInfo(HttpPostedFile postedFile, int libraryId, string librarySubfolder)
            : this(postedFile, libraryId, librarySubfolder, 0, 0, 0)
        {
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object from posted file.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        /// <param name="librarySubfolder">Library subfolder path where file should be saved</param>
        /// <param name="height">Height of the image</param>
        /// <param name="maxSideSize">Max side size of the image</param>
        /// <param name="width">Width of the image</param>
        public MediaFileInfo(HttpPostedFile postedFile, int libraryId, string librarySubfolder, int width, int height, int maxSideSize)
            : this(postedFile, libraryId, librarySubfolder, width, height, maxSideSize, 0)
        {
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object from posted file.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        /// <param name="librarySubfolder">Library subfolder path where file should be saved</param>
        /// <param name="height">Height of the image</param>
        /// <param name="maxSideSize">Max side size of the image</param>
        /// <param name="width">Width of the image</param>
        /// <param name="siteId">ID of the site new media file is related to</param>
        public MediaFileInfo(HttpPostedFile postedFile, int libraryId, string librarySubfolder, int width, int height, int maxSideSize, int siteId)
            : base(TYPEINFO)
        {
            // Get safe file name
            string fileName = Path.GetFileNameWithoutExtension(postedFile.FileName);
            fileName = URLHelper.GetSafeFileName(fileName, SiteContext.CurrentSiteName, false);

            // Init the file data
            FileName = fileName;
            FileExtension = URLHelper.GetSafeFileName(Path.GetExtension(postedFile.FileName), SiteContext.CurrentSiteName);
            FileTitle = "";
            FileDescription = "";
            FileSize = postedFile.ContentLength;
            FileMimeType = MimeTypeHelper.GetMimetype(FileExtension);

            string filePath = ((!string.IsNullOrEmpty(librarySubfolder)) ? librarySubfolder.Trim('/') + "/" : "") + FileName + FileExtension;
            FilePath = filePath;

            FileLibraryID = libraryId;
            FileSiteID = ((siteId > 0) ? siteId : SiteContext.CurrentSiteID);

            FileGUID = new Guid();

            // Read the data to memory only if it's image (to get it's properties)
            if (ImageHelper.IsImage(FileExtension))
            {
                // If image should be resized
                if ((width > 0) || (height > 0) || (maxSideSize > 0))
                {
                    FileBinary = ImageHelper.GetResizedImageData(postedFile, width, height, maxSideSize);
                }
                else
                {
                    FileBinary = new byte[postedFile.ContentLength];
                    postedFile.InputStream.Read(FileBinary, 0, postedFile.ContentLength);
                    postedFile.InputStream.Close();
                    postedFile.InputStream.Dispose();
                }

                // Get the dimensions
                ImageHelper ih = new ImageHelper(FileBinary);
                FileImageHeight = ih.ImageHeight;
                FileImageWidth = ih.ImageWidth;
                FileSize = ih.SourceData.Length;
            }
            else
            {
                // Use the stream for better performance of big files
                FileBinaryStream = postedFile.InputStream;
            }
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object specified by its file physical path.
        /// </summary>
        /// <param name="path">File physical path</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        public MediaFileInfo(string path, int libraryId)
            : this(path, libraryId, "")
        {
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object specified by its file physical path.
        /// </summary>
        /// <param name="path">File physical path</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        /// <param name="librarySubfolder">Library subfolder path where file should be saved</param>
        public MediaFileInfo(string path, int libraryId, string librarySubfolder)
            : this(path, libraryId, librarySubfolder, 0, 0, 0)
        {
        }


        /// <summary>
        /// Constructor - creates a new MediaFileInfo object specified by its file physical path.
        /// </summary>
        /// <param name="path">File physical path</param>
        /// <param name="libraryId">ID of the library the file will be assigned to</param>
        /// <param name="librarySubfolder">Library subfolder path where file should be saved</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="maxSideSize">Max side size of the image</param>
        public MediaFileInfo(string path, int libraryId, string librarySubfolder, int width, int height, int maxSideSize)
            : base(TYPEINFO)
        {
            MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
            if (mli != null)
            {
                // Init the attachment data
                FileName = Path.GetFileNameWithoutExtension(path);
                FileName = URLHelper.GetSafeFileName(FileName, SiteContext.CurrentSiteName, false);
                FileExtension = Path.GetExtension(path);

                string filePath = String.Format("{0}/{1}{2}", librarySubfolder.Trim('/'), FileName, FileExtension);

                // Prepare new file info
                FileInfo fileInfo = FileInfo.New(path);
                FileStream file = fileInfo.OpenRead();

                // Initialize new media file info
                FileSize = ValidationHelper.GetLong(fileInfo.Length, 0);
                FileMimeType = MimeTypeHelper.GetMimetype(FileExtension);
                FileTitle = "";
                FileDescription = "";
                FilePath = filePath.Trim('/');
                FileLibraryID = mli.LibraryID;
                FileSiteID = mli.LibrarySiteID;

                FileGUID = Guid.NewGuid();

                // Read the data to memory only if it's image (to get it's properties)
                if (ImageHelper.IsImage(FileExtension))
                {
                    // Read the image
                    FileBinary = new byte[FileSize];
                    file.Read(FileBinary, 0, (int)FileSize);
                    file.Close();
                    file.Dispose();

                    // If image should be resized
                    if ((width > 0) || (height > 0) || (maxSideSize > 0))
                    {
                        FileBinary = ImageHelper.GetResizedImageData(FileBinary, FileExtension, width, height, maxSideSize);
                    }

                    // Get the dimensions
                    ImageHelper ih = new ImageHelper(FileBinary);
                    FileImageHeight = ih.ImageHeight;
                    FileImageWidth = ih.ImageWidth;
                    FileSize = ih.SourceData.Length;
                }
                else
                {
                    // Read directly from memory for better performance of large files loading
                    FileBinaryStream = file;
                }
            }
            else
            {
                new Exception(string.Format("[MediaFileInfo.Constructor]: Given library width id '{0}' not found.", libraryId));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public override MediaFileInfo Clone(bool clear)
        {
            MediaFileInfo clone = base.Clone(clear);

            if (clear)
            {
                if (!MembershipContext.AuthenticatedUser.IsPublic())
                {
                    int userId = MembershipContext.AuthenticatedUser.UserID;
                    clone.FileModifiedByUserID = userId;
                    clone.FileCreatedByUserID = userId;
                }
            }

            return clone;
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
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return userInfo.IsAuthorizedPerResource("CMS.MediaLibrary", "Manage", siteName, exceptionOnFailure);

                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource("CMS.MediaLibrary", "Destroy", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Inserts the file to the database using standard MediaFileInfoProvider.SetMediaFileInfo function.
        /// </summary>
        /// <param name="settings">Clone settings</param>
        /// <param name="result">Result</param>
        /// <param name="originalObject">Original object</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Set the GUID/CodeName manually, because InsertAsClone sets it to null
            FileGUID = Guid.NewGuid();

            MediaFileInfoProvider.SetMediaFileInfo(this, true, FileCreatedByUserID);
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            FileCreatedWhen = DateTime.Now;
        }


        /// <summary>
        /// Checks if the media file path is unique within its media library. Returns true if the file path is unique.
        /// </summary>
        internal bool IsFilePathUnique()
        {
            return CheckUniqueValues(new[] { TypeInfo.ParentIDColumn, "FilePath" });
        }


        /// <summary>
        /// Sets default values for all possible custom fields.
        /// </summary>
        internal void SetDefaultDataFromFormDefinition()
        {
            // Just for the new object.
            if (FileID != 0)
            {
                return;
            }

            // Custom field are non systemic.
            var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(OBJECT_TYPE);
            var definition = new DataDefinition(dataClassInfo.ClassFormDefinition);
            var customFields = definition.GetFields<FieldInfo>().Where(i => !i.System);

            foreach (var customField in customFields)
            {
                var currentValue = GetValue(customField.Name);
                if (currentValue == null)
                {
                    var convertedValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, customField.DataType, customField.DefaultValue);
                    SetValue(customField.Name, convertedValue);
                }
            }
        }

        #endregion
    }
}