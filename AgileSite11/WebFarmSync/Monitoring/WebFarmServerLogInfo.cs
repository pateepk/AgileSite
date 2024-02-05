using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebFarmSync;

[assembly: RegisterObjectType(typeof(WebFarmServerLogInfo), WebFarmServerLogInfo.OBJECT_TYPE)]
    
namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmServerLogInfo data container class.
    /// </summary>
	[Serializable]
    public class WebFarmServerLogInfo : AbstractInfo<WebFarmServerLogInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmserverlog";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebFarmServerLogInfoProvider), OBJECT_TYPE, "CMS.WebFarmServerLog", "WebFarmServerLogID", "LogTime", null, null, null, null, null, null, null)
        {
            ModuleName = "CMS.WebFarm",
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ServerID", WebFarmServerInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            AllowTouchParent = false,
            TouchCacheDependencies = true,
            ImportExportSettings = { IsExportable = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Web farm server log ID
        /// </summary>
        [DatabaseField]
        public virtual int WebFarmServerLogID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("WebFarmServerLogID"), 0);
            }
            set
            {
                SetValue("WebFarmServerLogID", value);
            }
        }


        /// <summary>
        /// Server ID
        /// </summary>
        [DatabaseField]
        public virtual int ServerID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ServerID"), 0);
            }
            set
            {
                SetValue("ServerID", value);
            }
        }


        /// <summary>
        /// Log time
        /// </summary>
        [DatabaseField]
        public virtual DateTime LogTime
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("LogTime"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LogTime", value);
            }
        }


        /// <summary>
        /// Log code
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public virtual WebFarmServerStatusEnum LogCode
        {
            get
            {
                return GetStringValue("LogCode", String.Empty).ToEnum<WebFarmServerStatusEnum>();
            }
            set
            {
                SetValue("LogCode", value.ToStringRepresentation());
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebFarmServerLogInfoProvider.DeleteWebFarmServerLogInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public WebFarmServerLogInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty WebFarmServerLogInfo object.
        /// </summary>
        public WebFarmServerLogInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmServerLogInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public WebFarmServerLogInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}