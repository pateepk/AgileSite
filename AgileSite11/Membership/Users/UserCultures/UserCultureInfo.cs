using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Localization;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(UserCultureInfo), UserCultureInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// UserCultureInfo data container class.
    /// </summary>
    public class UserCultureInfo : AbstractInfo<UserCultureInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.USERCULTURE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UserCultureInfoProvider), OBJECT_TYPE, "CMS.UserCulture", null, null, null, null, null, null, "SiteID", "UserID", UserInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("CultureID", CultureInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            ModuleName = "cms.membership",
            IsSiteBinding = false,
            ImportExportSettings = { LogExport = false },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
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
            SupportsVersioning = false,
            TouchCacheDependencies = false,
            LogEvents = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// User ID.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// Site ID.
        /// </summary>
        public virtual int SiteID
        {
            get
            {
                return GetIntegerValue("SiteID", 0);
            }
            set
            {
                SetValue("SiteID", value);
            }
        }


        /// <summary>
        /// Culture ID.
        /// </summary>
        public virtual int CultureID
        {
            get
            {
                return GetIntegerValue("CultureID", 0);
            }
            set
            {
                SetValue("CultureID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UserCultureInfoProvider.DeleteUserCultureInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UserCultureInfoProvider.SetUserCultureInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserCultureInfo object.
        /// </summary>
        public UserCultureInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserCultureInfo object from the given DataRow.
        /// </summary>
        public UserCultureInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}