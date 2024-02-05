using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebFarmSync;

[assembly: RegisterObjectType(typeof(WebFarmServerMonitoringInfo), WebFarmServerMonitoringInfo.OBJECT_TYPE)]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmServerMonitoringInfo data container class.
    /// </summary>
    [Serializable]
    internal class WebFarmServerMonitoringInfo : AbstractInfo<WebFarmServerMonitoringInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmservermonitoring";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebFarmServerMonitoringInfoProvider), OBJECT_TYPE, "CMS.WebFarmServerMonitoring", "WebFarmServerMonitoringID", null, null, null, null, null, null, null, null)
        {
            ModuleName = "CMS.WebFarm",
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ServerID", WebFarmServerInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            AllowTouchParent = false,
            TouchCacheDependencies = false,
            ImportExportSettings = { IsExportable = false }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Web farm server monitoring ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebFarmServerMonitoringID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("WebFarmServerMonitoringID"), 0);
            }
            set
            {
                SetValue("WebFarmServerMonitoringID", value);
            }
        }


        /// <summary>
        /// Server ID.
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
        /// Server ping. Time when the server reports as alive.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ServerPing
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ServerPing"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ServerPing", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebFarmServerMonitoringInfoProvider.DeleteWebFarmServerMonitoringInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebFarmServerMonitoringInfoProvider.SetWebFarmServerMonitoringInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        internal WebFarmServerMonitoringInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty WebFarmServerMonitoringInfo object.
        /// </summary>
        public WebFarmServerMonitoringInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmServerMonitoringInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        internal WebFarmServerMonitoringInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}