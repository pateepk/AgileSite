using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebFarmSync;

[assembly: RegisterObjectType(typeof(WebFarmServerInfo), WebFarmServerInfo.OBJECT_TYPE)]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmServerInfo data container class.
    /// </summary>
    public class WebFarmServerInfo : AbstractInfo<WebFarmServerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmserver";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebFarmServerInfoProvider), OBJECT_TYPE, "CMS.WebFarmServer", "ServerID", "ServerLastModified", "ServerGUID", "ServerName", "ServerDisplayName", null, null, null, null)
        {
            LogEvents = true,
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            EnabledColumn = "ServerEnabled",
            Feature = FeatureEnum.Webfarm,
            AllowRestore = false,
            SupportsCloning = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Server ID.
        /// </summary>
        [DatabaseField]
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
        /// Server name.
        /// </summary>
        [DatabaseField]
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
        /// Server display name.
        /// </summary>
        [DatabaseField]
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
        /// Server GUID.
        /// </summary>
        [DatabaseField]
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
        [DatabaseField]
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


        /// <summary>
        /// Indicated whether server is enabled.
        /// </summary>
        [DatabaseField]
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
        /// Indicates that this web farm server is dedicated for external web application, such as MVC.
        /// </summary>
        [DatabaseField]
        public virtual bool IsExternalWebAppServer
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IsExternalWebAppServer"), false);
            }
            set
            {
                SetValue("IsExternalWebAppServer", value);
            }
        }


        /// <summary>
        /// Last known status of current server.
        /// </summary>
        public WebFarmServerStatusEnum Status
        {
            get
            {
                List<WebFarmServerLogInfo> serverLogs;
                if (WebFarmServerLogInfoProvider.Logs.TryGetValue(this, out serverLogs) && serverLogs.Any())
                {
                    return serverLogs.First().LogCode;
                }
                return ServerEnabled ? WebFarmServerStatusEnum.Transitioning : WebFarmServerStatusEnum.NotResponding;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebFarmServerInfoProvider.DeleteWebFarmServerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebFarmServerInfoProvider.SetWebFarmServerInfo(this);
        }


        /// <summary>
        /// Determines whether the specified object instances are considered equal.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        public override bool Equals(object obj)
        {
            if (obj is WebFarmServerInfo)
            {
                return Equals(obj as WebFarmServerInfo);
            }
            return false;
        }


        /// <summary>
        /// Determines whether the specified object instances are considered equal.
        /// </summary>
        /// <param name="other">Object to compare.</param>
        protected bool Equals(WebFarmServerInfo other)
        {
            return ServerName == other.ServerName;
        }


        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode()
        {
            return ServerName.GetHashCode();
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched.
        /// </summary>
        /// <param name="action">Object action.</param>
        /// <param name="domainName">Domain name, if not set, uses current domain.</param>
        /// <exception cref="LicenseException">Throws <see cref="LicenseException"/> if license check failed.</exception>
        protected sealed override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            if (action == ObjectActionEnum.Read)
            {
                return true;
            }

            WebFarmLicenseHelper.CheckLicense(domainName);
            
            return true;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebFarmServerInfo object.
        /// </summary>
        public WebFarmServerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmServerInfo object from the given DataRow.
        /// </summary>
        public WebFarmServerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}