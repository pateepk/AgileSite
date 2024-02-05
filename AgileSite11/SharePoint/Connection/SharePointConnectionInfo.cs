using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SharePoint;

[assembly: RegisterObjectType(typeof(SharePointConnectionInfo), SharePointConnectionInfo.OBJECT_TYPE)]

namespace CMS.SharePoint
{
    /// <summary>
    /// Class defines connection to the SharePoint server including credentials.
    /// </summary>
    [Serializable]
    public class SharePointConnectionInfo : AbstractInfo<SharePointConnectionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "sharepoint.sharepointconnection";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SharePointConnectionInfoProvider), OBJECT_TYPE, "SharePoint.SharePointConnection", "SharePointConnectionID", "SharePointConnectionLastModified", "SharePointConnectionGUID", "SharePointConnectionName", "SharePointConnectionDisplayName", null, "SharePointConnectionSiteID", null, null)
        {
            ModuleName = ModuleName.SHAREPOINT,
            Feature = FeatureEnum.SharePoint,
            ImportExportSettings =
            {
                LogExport = true,
                AllowSingleExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION)
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            LogEvents = true,
            SensitiveColumns = new List<string> { "SharePointConnectionPassword" },
        };

        #endregion


        #region "Fields"

        /// <summary>
        /// Properties to be skipped when transforming to SharePointConnectionData
        /// </summary>
        private readonly HashSet<string> connectionDataPropertyBlacklist = new HashSet<string>()
        {
            "SharePointConnectionPassword"
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Name under which the object appears in the user interface
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionDisplayName"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionDisplayName", value);
            }
        }


        /// <summary>
        /// Internal name used by the system to identify the object
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionName"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionName", value);
            }
        }


        /// <summary>
        /// The URL of a SharePoint site.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionSiteUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionSiteUrl"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionSiteUrl", value);
            }
        }


        /// <summary>
        /// The version of a SharePoint server.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionSharePointVersion
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionSharePointVersion"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionSharePointVersion", value);
            }
        }


        /// <summary>
        /// Mode that is used for authentication.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionAuthMode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionAuthMode"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionAuthMode", value);
            }
        }


        /// <summary>
        /// Username that will be used for authentication.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionUserName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionUserName"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionUserName", value);
            }
        }


        /// <summary>
        /// Domain that will be used for authentication.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionDomain
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SharePointConnectionDomain"), String.Empty);
            }
            set
            {
                SetValue("SharePointConnectionDomain", value);
            }
        }


        /// <summary>
        /// Password that will be used for authentication.
        /// </summary>
        [DatabaseField]
        public virtual string SharePointConnectionPassword
        {
            get
            {
                return EncryptionHelper.DecryptData(ValidationHelper.GetString(GetValue("SharePointConnectionPassword"), String.Empty));
            }
            set
            {
                SetValue("SharePointConnectionPassword", EncryptionHelper.EncryptData(value));
            }
        }

        #endregion


        #region "System properties"

        /// <summary>
        /// Share point connection ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointConnectionID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointConnectionID"), 0);
            }
            set
            {
                SetValue("SharePointConnectionID", value);
            }
        }


        /// <summary>
        /// Share point connection site ID
        /// </summary>
        [DatabaseField]
        public virtual int SharePointConnectionSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SharePointConnectionSiteID"), 0);
            }
            set
            {
                SetValue("SharePointConnectionSiteID", value);
            }
        }


        /// <summary>
        /// Share point connection GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid SharePointConnectionGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("SharePointConnectionGUID"), Guid.Empty);
            }
            set
            {
                SetValue("SharePointConnectionGUID", value);
            }
        }


        /// <summary>
        /// Share point connection last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime SharePointConnectionLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("SharePointConnectionLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SharePointConnectionLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SharePointConnectionInfoProvider.DeleteSharePointConnectionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SharePointConnectionInfoProvider.SetSharePointConnectionInfo(this);
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets <see cref="SharePointConnectionData"/> representing this object.
        /// </summary>
        /// <returns>This object as SharePointConnectionData</returns>
        public SharePointConnectionData ToSharePointConnectionData()
        {   
            SharePointConnectionData sharePointConnectionData = new SharePointConnectionData();
            List<string> columnNames = GetColumnNames();

            foreach (var columnName in columnNames)
            {
                if (connectionDataPropertyBlacklist.Contains(columnName))
                {
                    continue;
                }

                sharePointConnectionData[columnName] = GetValue(columnName);
            }

            // Password has to be handled this way due to encryption
            sharePointConnectionData["SharePointConnectionPassword"] = SharePointConnectionPassword;

            return sharePointConnectionData;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public SharePointConnectionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty SharePointConnectionInfo object.
        /// </summary>
        public SharePointConnectionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SharePointConnectionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SharePointConnectionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}