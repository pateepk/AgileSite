using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.WebFarmSync;

[assembly: RegisterObjectType(typeof(WebFarmServerTaskInfo), WebFarmServerTaskInfo.OBJECT_TYPE)]

namespace CMS.WebFarmSync
{
    /// <summary>
    /// WebFarmServerTaskInfo data container class.
    /// </summary>
    public class WebFarmServerTaskInfo : AbstractInfo<WebFarmServerTaskInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webfarmservertask";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebFarmServerTaskInfoProvider), OBJECT_TYPE, "CMS.WebFarmServerTask", null, null, null, null, null, null, null, "ServerID", WebFarmServerInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("TaskID", WebFarmTaskInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            RegisterAsBindingToObjectTypes = new List<string>(),
            RegisterAsOtherBindingToObjectTypes = new List<string>(),
            ImportExportSettings = { IsExportable = false },
            Feature = FeatureEnum.Webfarm,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

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
        /// Task ID.
        /// </summary>
        public virtual int TaskID
        {
            get
            {
                return GetIntegerValue("TaskID", 0);
            }
            set
            {
                SetValue("TaskID", value);
            }
        }


        /// <summary>
        /// Error message.
        /// </summary>
        public virtual string ErrorMessage
        {
            get
            {
                return GetStringValue("ErrorMessage", "");
            }
            set
            {
                SetValue("ErrorMessage", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebFarmServerTaskInfoProvider.DeleteWebFarmServerTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebFarmServerTaskInfoProvider.SetWebFarmServerTaskInfo(this);
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched.
        /// </summary>
        /// <param name="action">Object action.</param>
        /// <param name="domainName">Domain name, if not set, uses current domain.</param>
        /// <exception cref="LicenseException">Throws <see cref="LicenseException"/> if license check failed.</exception>
        protected sealed override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            WebFarmLicenseHelper.CheckLicense(domainName);
            
            return true;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WebFarmServerTaskInfo object.
        /// </summary>
        public WebFarmServerTaskInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WebFarmServerTaskInfo object from the given DataRow.
        /// </summary>
        public WebFarmServerTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}