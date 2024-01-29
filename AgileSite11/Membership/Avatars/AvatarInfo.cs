using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(AvatarInfo), AvatarInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// AvatarInfo data container class.
    /// </summary>
    public class AvatarInfo : AbstractInfo<AvatarInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.avatar";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AvatarInfoProvider), OBJECT_TYPE, "CMS.Avatar", "AvatarID", "AvatarLastModified", "AvatarGUID", null, "AvatarName", "AvatarBinary", null, null, null)
        {
            ModuleName = "cms.membership",
            MimeTypeColumn = "AvatarMimeType",
            ExtensionColumn = "AvatarFileExtension",
            SizeColumn = "AvatarFileSize",
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY)
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, SOCIALANDCOMMUNITY)
                }
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true,
                ObjectFileNameFields = { "AvatarName" },
                SeparatedFields = new List<SeparatedField>(){
                    new SeparatedField("AvatarBinary")
                    {
                        IsBinaryField = true,
                        FileExtensionFieldName = "AvatarFileExtension",
                        FileName = "file"
                    }
                }
            },
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = true
        };

        #endregion


        #region "Private variables"

        /// <summary>
        /// Input stream avatar data.
        /// </summary>
        protected byte[] mInputStreamData = null;

        /// <summary>
        /// Indicates whether the data from input stream were processed.
        /// </summary>
        protected bool mStreamProcessed = false;

        #endregion


        #region "Properties"

        /// <summary>
        /// Avatar last modified.
        /// </summary>
        public virtual DateTime AvatarLastModified
        {
            get
            {
                return GetDateTimeValue("AvatarLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AvatarLastModified", value);
            }
        }


        /// <summary>
        /// Binary data checksum.
        /// </summary>
        internal string BinaryDataChecksum
        {
            get
            {
                return new BinaryData(EnsureBinaryData()).Checksum;
            }
        }


        /// <summary>
        /// Avatar binary data.
        /// </summary>
        public virtual byte[] AvatarBinary
        {
            get
            {
                return ValidationHelper.GetBinary(GetValue("AvatarBinary"), null);
            }
            set
            {
                SetValue("AvatarBinary", value);
            }
        }


        /// <summary>
        /// Avatar file name.
        /// </summary>
        public virtual string AvatarFileName
        {
            get
            {
                return GetStringValue("AvatarFileName", "");
            }
            set
            {
                SetValue("AvatarFileName", value);
            }
        }


        /// <summary>
        /// Avatar ID.
        /// </summary>
        public virtual int AvatarID
        {
            get
            {
                return GetIntegerValue("AvatarID", 0);
            }
            set
            {
                SetValue("AvatarID", value);
            }
        }


        /// <summary>
        /// Avatar name.
        /// </summary>
        public virtual string AvatarName
        {
            get
            {
                return GetStringValue("AvatarName", "");
            }
            set
            {
                SetValue("AvatarName", value);
            }
        }


        /// <summary>
        /// Whether avatar is custom.
        /// </summary>
        public virtual bool AvatarIsCustom
        {
            get
            {
                return GetBooleanValue("AvatarIsCustom", false);
            }
            set
            {
                SetValue("AvatarIsCustom", value);
            }
        }


        /// <summary>
        /// Avatar GUID.
        /// </summary>
        public virtual Guid AvatarGUID
        {
            get
            {
                return GetGuidValue("AvatarGUID", Guid.Empty);
            }
            set
            {
                SetValue("AvatarGUID", value);
            }
        }


        /// <summary>
        /// Avatar type - All,User or Group.
        /// </summary>
        public virtual string AvatarType
        {
            get
            {
                return GetStringValue("AvatarType", "");
            }
            set
            {
                SetValue("AvatarType", value);
            }
        }


        /// <summary>
        /// Avatar file extension.
        /// </summary>
        public virtual string AvatarFileExtension
        {
            get
            {
                return GetStringValue("AvatarFileExtension", "");
            }
            set
            {
                SetValue("AvatarFileExtension", value);
            }
        }


        /// <summary>
        /// Avatar MIME type specification.
        /// </summary>
        public virtual string AvatarMimeType
        {
            get
            {
                return GetStringValue("AvatarMimeType", "");
            }
            set
            {
                SetValue("AvatarMimeType", value);
            }
        }


        /// <summary>
        /// Avatar file size detail.
        /// </summary>
        public virtual int AvatarFileSize
        {
            get
            {
                return GetIntegerValue("AvatarFileSize", 0);
            }
            set
            {
                SetValue("AvatarFileSize", value);
            }
        }


        /// <summary>
        /// Avatar image height.
        /// </summary>
        public virtual int AvatarImageHeight
        {
            get
            {
                return GetIntegerValue("AvatarImageHeight", 0);
            }
            set
            {
                SetValue("AvatarImageHeight", value);
            }
        }


        /// <summary>
        /// Avatar image height.
        /// </summary>
        public virtual int AvatarImageWidth
        {
            get
            {
                return GetIntegerValue("AvatarImageWidth", 0);
            }
            set
            {
                SetValue("AvatarImageWidth", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the avatar is default user avatar.
        /// </summary>
        public virtual bool DefaultUserAvatar
        {
            get
            {
                return GetBooleanValue("DefaultUserAvatar", false);
            }
            set
            {
                SetValue("DefaultUserAvatar", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the avatar is default male avatar.
        /// </summary>
        public virtual bool DefaultMaleUserAvatar
        {
            get
            {
                return GetBooleanValue("DefaultMaleUserAvatar", false);
            }
            set
            {
                SetValue("DefaultMaleUserAvatar", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the avatar is default female avatar.
        /// </summary>
        public virtual bool DefaultFemaleUserAvatar
        {
            get
            {
                return GetBooleanValue("DefaultFemaleUserAvatar", false);
            }
            set
            {
                SetValue("DefaultFemaleUserAvatar", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the avatar is default group avatar.
        /// </summary>
        public virtual bool DefaultGroupAvatar
        {
            get
            {
                return GetBooleanValue("DefaultGroupAvatar", false);
            }
            set
            {
                SetValue("DefaultGroupAvatar", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AvatarInfoProvider.DeleteAvatarInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AvatarInfoProvider.SetAvatarInfo(this);
        }


        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        protected override ICollection<string> GetCacheDependencies()
        {
            var dependencies = base.GetCacheDependencies();

            // Merge general dependencies with custom dependecies 
            dependencies.Add("avatarfile|" + AvatarGUID.ToString().ToLowerCSafe());

            return dependencies;
        }


        /// <summary>
        /// Gets collection of dependency keys to be touched when modifying the current object.
        /// </summary>
        [Obsolete("Use Generalized.GetCacheDependencies() instead.")]
        public List<string> GetDependencyCacheKeys()
        {
            return base.GetDependencyCacheKeys(null, null);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Reset the default flags of avatars
            this.DefaultFemaleUserAvatar = false;
            this.DefaultMaleUserAvatar = false;
            this.DefaultGroupAvatar = false;
            this.DefaultUserAvatar = false;

            this.Insert();
        }


        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete all file occurrences in file system
            AvatarInfoProvider.DeleteAvatarFile(AvatarGUID.ToString(), AvatarFileExtension, false, false);

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AvatarInfo object.
        /// </summary>
        public AvatarInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AvatarInfo object from the given DataRow.
        /// </summary>
        public AvatarInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AvatarInfo object based on the file posted through the upload control.
        /// </summary>
        /// <param name="postedFile">Posted file</param>
        /// <param name="maxWidth">Maximal image width</param>
        /// <param name="maxHeight">Maximal image height</param>
        /// <param name="maxSideSize">Maximal side size</param>
        public AvatarInfo(HttpPostedFile postedFile, int maxWidth, int maxHeight, int maxSideSize)
            : base(TYPEINFO)
        {
            AvatarInfoProvider.UploadAvatar(this, postedFile, maxWidth, maxHeight, maxSideSize);
        }


        /// <summary>
        /// Creates a new AvatarInfo object based on the file specified by given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        public AvatarInfo(string filePath)
            : base(TYPEINFO)
        {
            int fileNameStartIndex = filePath.LastIndexOfCSafe("\\") + 1;

            // Init the MetaFile data
            AvatarFileName = URLHelper.GetSafeFileName(filePath.Substring(fileNameStartIndex), null);
            AvatarFileExtension = Path.GetExtension(filePath);

            FileInfo fileInfo = FileInfo.New(filePath);
            FileStream file = fileInfo.OpenRead();
            AvatarFileSize = Convert.ToInt32(fileInfo.Length);
            AvatarMimeType = MimeTypeHelper.GetMimetype(AvatarFileExtension);

            // Read the data
            byte[] buffer = new byte[AvatarFileSize];
            file.Read(buffer, 0, AvatarFileSize);
            AvatarBinary = buffer;
            file.Close();
            file.Dispose();

            // Set image properties
            if (ImageHelper.IsImage(AvatarFileExtension))
            {
                ImageHelper ih = new ImageHelper(AvatarBinary);
                AvatarImageHeight = ih.ImageHeight;
                AvatarImageWidth = ih.ImageWidth;
            }
        }

        #endregion


        #region "Support methods"

        /// <summary>
        /// Ensures the binary data - loads the binary data from file stream if present.
        /// </summary>
        public new byte[] GetBinaryData()
        {
            return AvatarBinary ?? AvatarInfoProvider.GetAvatarFile(AvatarGUID);
        }


        /// <summary>
        /// Ensures the binary data - loads the binary data from file stream if present.
        /// </summary>
        protected override byte[] EnsureBinaryData()
        {
            AvatarBinary = GetBinaryData();
            return AvatarBinary;
        }

        #endregion
    }
}