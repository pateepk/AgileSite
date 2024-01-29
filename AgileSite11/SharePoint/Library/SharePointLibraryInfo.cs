using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SharePoint;
using CMS.Core;

[assembly: RegisterObjectType(typeof(SharePointLibraryInfo), SharePointLibraryInfo.OBJECT_TYPE)]

namespace CMS.SharePoint
{
    /// <summary>
    /// SharePointLibraryInfo data container class.
    /// </summary>
    [Serializable]
    public class SharePointLibraryInfo : AbstractInfo<SharePointLibraryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sharepoint.sharepointlibrary";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SharePointLibraryInfoProvider), OBJECT_TYPE, "SharePoint.SharePointLibrary", "SharePointLibraryID", "SharePointLibraryLastModified", "SharePointLibraryGuid", "SharePointLibraryName", "SharePointLibraryDisplayName", null, "SharePointLibrarySiteID", null, null)
        {
            ModuleName = ModuleName.SHAREPOINT,
            Feature = FeatureEnum.SharePoint,
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() 
            {
                new ObjectDependency("SharePointLibrarySharePointConnectionID", SharePointConnectionInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), 
            },
            ImportExportSettings =
            {
                LogExport = true,
                AllowSingleExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE,CONTENTMANAGEMENT)
                }
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONTENTMANAGEMENT)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            LogEvents = false,
            SupportsVersioning = false,
            SupportsCloning = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// SharePoint library ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointLibraryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointLibraryID"), 0);
            }
            set
            {
                SetValue("SharePointLibraryID", value);
            }
        }


        /// <summary>
        /// SharePoint library guid
        /// </summary>
        [DatabaseField]
        public virtual Guid SharePointLibraryGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("SharePointLibraryGUID"), Guid.Empty);
            }
            set
            {
                SetValue("SharePointLibraryGUID", value);
            }
        }


        /// <summary>
        /// SharePoint library site ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointLibrarySiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointLibrarySiteID"), 0);
            }
            set
            {
                SetValue("SharePointLibrarySiteID", value);
            }
        }


        /// <summary>
        /// SharePoint library last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime SharePointLibraryLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SharePointLibraryLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SharePointLibraryLastModified", value);
            }
        }


        /// <summary>
        /// The name of the SharePoint library used in the system's administration interface.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointLibraryDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointLibraryDisplayName"), String.Empty);
            }
            set
            {
                SetValue("SharePointLibraryDisplayName", value);
            }
        }


        /// <summary>
        /// The string identifier of the SharePoint connection object used by the system to identify the object.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointLibraryName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointLibraryName"), String.Empty);
            }
            set
            {
                SetValue("SharePointLibraryName", value);
            }
        }


        /// <summary>
        /// SharePoint library SharePoint connection ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointLibrarySharePointConnectionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointLibrarySharePointConnectionID"), 0);
            }
            set
            {
                SetValue("SharePointLibrarySharePointConnectionID", value);
            }
        }


        /// <summary>
        /// SharePoint library list type
        /// </summary>
        [DatabaseField]
        public virtual int SharePointLibraryListType
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointLibraryListType"), 0);
            }
            set
            {
                SetValue("SharePointLibraryListType", value);
            }
        }


        /// <summary>
        /// SharePoint library list title
        /// </summary>
        [DatabaseField]
        public virtual string SharePointLibraryListTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointLibraryListTitle"), String.Empty);
            }
            set
            {
                SetValue("SharePointLibraryListTitle", value);
            }
        }


        /// <summary>
        /// SharePoint library synchronization period in number of minutes
        /// </summary>
        [DatabaseField]
        public virtual int SharePointLibrarySynchronizationPeriod
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointLibrarySynchronizationPeriod"), 720);
            }
            set
            {
                SetValue("SharePointLibrarySynchronizationPeriod", value, 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SharePointLibraryInfoProvider.DeleteSharePointLibraryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SharePointLibraryInfoProvider.SetSharePointLibraryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public SharePointLibraryInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty SharePointLibraryInfo object.
        /// </summary>
        public SharePointLibraryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SharePointLibraryInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SharePointLibraryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}