using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.ContinuousIntegration.Internal;

[assembly: RegisterObjectType(typeof(FileMetadataInfo), FileMetadataInfo.OBJECT_TYPE)]

namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Contains metadata for a file used in objects serialization.
    /// </summary>
    [Serializable]
    public class FileMetadataInfo : AbstractInfo<FileMetadataInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ci.filemetadata";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FileMetadataInfoProvider), OBJECT_TYPE, "CI.FileMetadata", "FileMetadataID", null, null, null, null, null, null, null, null)
        {
            ModuleName = ModuleName.CONTINUOUSINTEGRATION,
            TouchCacheDependencies = true,
            Feature = FeatureEnum.ContinuousIntegration,
            SupportsCloning = false,
            LogIntegration = false,
            LogEvents = false,
            UseUpsert = true,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// File metadata ID
        /// </summary>
        [DatabaseField]
        public virtual int FileMetadataID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileMetadataID"), 0);
            }
            set
            {
                SetValue("FileMetadataID", value);
            }
        }


        /// <summary>
        /// Location of the file.
        /// </summary>
        [DatabaseField]
        public virtual string FileLocation
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileLocation"), String.Empty);
            }
            set
            {
                SetValue("FileLocation", value);
            }
        }


        /// <summary>
        /// Hash of the file content.
        /// </summary>
        [DatabaseField]
        public virtual string FileHash
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileHash"), String.Empty);
            }
            set
            {
                SetValue("FileHash", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            FileMetadataInfoProvider.DeleteFileMetadataInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            FileMetadataInfoProvider.SetFileMetadataInfo(this);
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object. Existing object is primarily identified by its ID
        /// column value. If not set, the file location is used.
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            // Determine object's existence from location (which is unique), if ID is not available
            if (FileMetadataID == 0)
            {
                return new WhereCondition("FileLocation", QueryOperator.Equals, FileLocation);
            }

            return base.GetExistingWhereCondition();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public FileMetadataInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty FileMetadataInfo object.
        /// </summary>
        public FileMetadataInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new FileMetadataInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public FileMetadataInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            var bestLicense = LicenseKeyInfoProvider.GetBestLicense();

            return LicenseKeyInfoProvider.IsFeatureAvailable(bestLicense, TypeInfo.Feature);
        }

        #endregion
    }
}