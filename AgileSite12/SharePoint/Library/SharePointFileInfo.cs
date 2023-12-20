using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SharePoint;

[assembly: RegisterObjectType(typeof(SharePointFileInfo), SharePointFileInfo.OBJECT_TYPE)]

namespace CMS.SharePoint
{
    /// <summary>
    /// SharePointFileInfo data container class.
    /// </summary>
    [Serializable]
    public class SharePointFileInfo : AbstractInfo<SharePointFileInfo>
    {
        #region "Type information"

        /// <summary>
        /// SharePointFile object type.
        /// </summary>
        public const string OBJECT_TYPE = "sharepoint.sharepointfile";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SharePointFileInfoProvider), OBJECT_TYPE, "SharePoint.SharePointFile", "SharePointFileID", null, "SharePointFileGUID", null, "SharePointFileName", "SharePointFileBinary", "SharePointFileSiteID", "SharePointFileSharePointLibraryID", SharePointLibraryInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.SHAREPOINT,
            Feature = FeatureEnum.SharePoint,
            TouchCacheDependencies = true,
            SynchronizationSettings = 
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            ImportExportSettings =
            {
                LogExport = false,
                AllowSingleExport = false,
                IsExportable = false,
                IncludeToExportParentDataSet = IncludeToParentEnum.None
            },

            LogEvents = false,
            SupportsCloning = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// SharePoint file ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointFileID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointFileID"), 0);
            }
            set
            {
                SetValue("SharePointFileID", value);
            }
        }


        /// <summary>
        /// SharePoint file GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid SharePointFileGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("SharePointFileGUID"), Guid.Empty);
            }
            set
            {
                SetValue("SharePointFileGUID", value);
            }
        }


        /// <summary>
        /// SharePoint file site ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointFileSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointFileSiteID"), 0);
            }
            set
            {
                SetValue("SharePointFileSiteID", value);
            }
        }


        /// <summary>
        /// SharePoint file name
        /// </summary>
        [DatabaseField]
        public virtual string SharePointFileName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointFileName"), String.Empty);
            }
            set
            {
                SetValue("SharePointFileName", value);
            }
        }


        /// <summary>
        /// SharePoint file extension
        /// </summary>
        [DatabaseField]
        public virtual string SharePointFileExtension
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointFileExtension"), String.Empty);
            }
            set
            {
                SetValue("SharePointFileExtension", value, String.Empty);
            }
        }


        /// <summary>
        /// SharePoint file mime type
        /// </summary>
        [DatabaseField]
        public virtual string SharePointFileMimeType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointFileMimeType"), String.Empty);
            }
            set
            {
                SetValue("SharePointFileMimeType", value, String.Empty);
            }
        }


        /// <summary>
        /// SharePoint file E-tag
        /// </summary>
        [DatabaseField]
        public virtual string SharePointFileETag
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointFileETag"), String.Empty);
            }
            set
            {
                SetValue("SharePointFileETag", value, String.Empty);
            }
        }


        /// <summary>
        /// SharePoint file size (bytes)
        /// </summary>
        [DatabaseField]
        public virtual long SharePointFileSize
        {
            get
            {
                return ValidationHelper.GetLong(GetValue("SharePointFileSize"), 0);
            }
            set
            {
                SetValue("SharePointFileSize", value);
            }
        }


        /// <summary>
        /// SharePoint file binary content
        /// </summary>
        [DatabaseField]
        public virtual byte[] SharePointFileBinary
        {
            get
            {
                return EnsureBinaryData();
            }
            set
            {
                SetValue("SharePointFileBinary", value);
            }
        }


        /// <summary>
        /// SharePoint file server last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime SharePointFileServerLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SharePointFileServerLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SharePointFileServerLastModified", value);
            }
        }


        /// <summary>
        /// SharePoint file server relative URL
        /// </summary>
        [DatabaseField]
        public virtual string SharePointFileServerRelativeURL
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointFileServerRelativeURL"), String.Empty);
            }
            set
            {
                SetValue("SharePointFileServerRelativeURL", value);
            }
        }


        /// <summary>
        /// SharePoint file SharePoint library ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointFileSharePointLibraryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointFileSharePointLibraryID"), 0);
            }
            set
            {
                SetValue("SharePointFileSharePointLibraryID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SharePointFileInfoProvider.DeleteSharePointFileInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SharePointFileInfoProvider.SetSharePointFileInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public SharePointFileInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty SharePointFileInfo object.
        /// </summary>
        public SharePointFileInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SharePointFileInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SharePointFileInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Advanced methods"

        /// <summary>
        /// Gets <see cref="SharePointFile"/> representing this object.
        /// </summary>
        /// <returns>This object as SharePointFile.</returns>
        internal SharePointFile ToSharePointFile()
        {
            return new SharePointFile(SharePointFileBinary)
            {
                ETag = SharePointFileETag,
                Name = SharePointFileName,
                Extension = SharePointFileExtension,
                MimeType = SharePointFileMimeType,
                ServerRelativeUrl = SharePointFileServerRelativeURL,
                TimeLastModified = SharePointFileServerLastModified
            };
        }

        #endregion
    }
}