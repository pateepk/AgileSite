using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(UserSiteInfo), UserSiteInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// UserSiteInfo data container class.
    /// </summary>
    public class UserSiteInfo : AbstractInfo<UserSiteInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.usersite";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UserSiteInfoProvider), OBJECT_TYPE, "CMS.UserSite", "UserSiteID", null, null, null, null, null, "SiteID", "UserID", UserInfo.OBJECT_TYPE)
        {
            TouchCacheDependencies = true,
            AllowRestore = false,
            IsBinding = true,
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
            ModuleName = "cms.users"
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Record's site ID.
        /// </summary>
        public int UserSiteID
        {
            get
            {
                return GetIntegerValue("UserSiteID", 0);
            }
            set
            {
                SetValue("UserSiteID", value);
            }
        }


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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UserSiteInfoProvider.DeleteUserSiteInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UserSiteInfoProvider.SetUserSiteInfo(this);
        }


        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        protected override BaseInfo GetExisting()
        {
            return UserSiteInfoProvider.GetUserSiteInfo(UserID, SiteID);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserSiteInfo object.
        /// </summary>
        public UserSiteInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserSiteInfo object from the given DataRow.
        /// </summary>
        public UserSiteInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}