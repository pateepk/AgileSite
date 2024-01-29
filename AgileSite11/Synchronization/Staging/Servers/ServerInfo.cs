using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.LicenseProvider;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(ServerInfo), ServerInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// ServerInfo data container class.
    /// </summary>
    public class ServerInfo : AbstractInfo<ServerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "staging.server";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ServerInfoProvider), OBJECT_TYPE, "Staging.Server", "ServerID", "ServerLastModified", "ServerGUID", "ServerName", "ServerDisplayName", null, "ServerSiteID", null, null)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = "cms.staging",
            Feature = FeatureEnum.Staging,
            EnabledColumn = "ServerEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                },
            },
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Server site ID.
        /// </summary>
        public virtual int ServerSiteID
        {
            get
            {
                return GetIntegerValue("ServerSiteID", 0);
            }
            set
            {
                SetValue("ServerSiteID", value);
            }
        }


        /// <summary>
        /// Server X509 server key ID.
        /// </summary>
        public virtual string ServerX509ServerKeyID
        {
            get
            {
                return GetStringValue("ServerX509ServerKeyID", "");
            }
            set
            {
                SetValue("ServerX509ServerKeyID", value);
            }
        }


        /// <summary>
        /// Server password.
        /// </summary>
        public virtual string ServerPassword
        {
            get
            {
                return GetStringValue("ServerPassword", "");
            }
            set
            {
                SetValue("ServerPassword", value);
            }
        }


        /// <summary>
        /// Server authentication.
        /// </summary>
        public virtual ServerAuthenticationEnum ServerAuthentication
        {
            get
            {
                if (ValidationHelper.GetString(GetValue("ServerAuthentication"), "").ToLowerCSafe() == "x509")
                {
                    return ServerAuthenticationEnum.X509;
                }
                else
                {
                    return ServerAuthenticationEnum.UserName;
                }
            }
            set
            {
                switch (value)
                {
                    case ServerAuthenticationEnum.UserName:
                        SetValue("ServerAuthentication", "USERNAME");
                        break;

                    case ServerAuthenticationEnum.X509:
                        SetValue("ServerAuthentication", "X509");
                        break;
                }
            }
        }


        /// <summary>
        /// Server ID.
        /// </summary>
        public virtual int ServerID
        {
            get
            {
                return GetIntegerValue("ServerID", 0);
            }
            set
            {
                SetValue("ServerID", value);
            }
        }


        /// <summary>
        /// Server display name.
        /// </summary>
        public virtual string ServerDisplayName
        {
            get
            {
                return GetStringValue("ServerDisplayName", "");
            }
            set
            {
                SetValue("ServerDisplayName", value);
            }
        }


        /// <summary>
        /// Server url.
        /// </summary>
        public virtual string ServerURL
        {
            get
            {
                return GetStringValue("ServerURL", "");
            }
            set
            {
                SetValue("ServerURL", value);
            }
        }


        /// <summary>
        /// Server X509 client key ID.
        /// </summary>
        public virtual string ServerX509ClientKeyID
        {
            get
            {
                return GetStringValue("ServerX509ClientKeyID", "");
            }
            set
            {
                SetValue("ServerX509ClientKeyID", value);
            }
        }


        /// <summary>
        /// Server name.
        /// </summary>
        public virtual string ServerName
        {
            get
            {
                return GetStringValue("ServerName", "");
            }
            set
            {
                SetValue("ServerName", value);
            }
        }


        /// <summary>
        /// Server user name.
        /// </summary>
        public virtual string ServerUsername
        {
            get
            {
                return GetStringValue("ServerUsername", "");
            }
            set
            {
                SetValue("ServerUsername", value);
            }
        }


        /// <summary>
        /// Server enabled.
        /// </summary>
        public virtual bool ServerEnabled
        {
            get
            {
                return GetBooleanValue("ServerEnabled", false);
            }
            set
            {
                SetValue("ServerEnabled", value);
            }
        }


        /// <summary>
        /// Server GUID.
        /// </summary>
        public virtual Guid ServerGUID
        {
            get
            {
                return GetGuidValue("ServerGUID", Guid.Empty);
            }
            set
            {
                SetValue("ServerGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime ServerLastModified
        {
            get
            {
                return GetDateTimeValue("ServerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ServerLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ServerInfoProvider.DeleteServerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ServerInfoProvider.SetServerInfo(this);
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
            bool allowed = false;
            switch (permission)
            {
                case PermissionsEnum.Read:
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, "ManageServers", siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentionally modified are cleared</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            base.RemoveObjectDependencies(deleteAll, clearHashtables);

            // Delete tasks which has no more servers to be run at when we delete a server
            StagingTaskInfoProvider.DeleteRedundantTasks();
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            if (action == ObjectActionEnum.Read)
            {
                // Staging of global objects should not depend on the current context domain
                var bestLicense = LicenseKeyInfoProvider.GetBestLicense();
                
                if (bestLicense != null)
                {
                    domainName = bestLicense.Domain;
                }
            }

            return base.CheckLicense(action, domainName);
        }

        #endregion


        #region "Contructors"

        /// <summary>
        /// Constructor - Creates an empty ServerInfo object.
        /// </summary>
        public ServerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ServerInfo object from the given DataRow.
        /// </summary>
        public ServerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}