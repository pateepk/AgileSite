using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(MembershipUserInfo), MembershipUserInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// MembershipUserInfo data container class.
    /// </summary>
    public class MembershipUserInfo : AbstractInfo<MembershipUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.membershipuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MembershipUserInfoProvider), OBJECT_TYPE, "CMS.MembershipUser", "MembershipUserID", null, null, null, null, null, null, "MembershipID", MembershipInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            ModuleName = "cms.membership",
            AllowRestore = false,
            IsBinding = true,
            ImportExportSettings =
            {
                LogExport = false
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            TouchCacheDependencies = true,
            LogEvents = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Membership user ID
        /// </summary>
        [DatabaseField]
        public virtual int MembershipUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MembershipUserID"), 0);
            }
            set
            {
                SetValue("MembershipUserID", value);
            }
        }


        /// <summary>
        /// ID of User object.
        /// </summary>
        [DatabaseField]
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
        /// ID of membership object.
        /// </summary>
        [DatabaseField]
        public virtual int MembershipID
        {
            get
            {
                return GetIntegerValue("MembershipID", 0);
            }
            set
            {
                SetValue("MembershipID", value);
            }
        }


        /// <summary>
        /// Date to membership is valid for given user.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ValidTo
        {
            get
            {
                return GetDateTimeValue("ValidTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ValidTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates if given user is to be notified in advance about expiration of his membership.
        /// </summary>
        [DatabaseField]
        public virtual bool SendNotification
        {
            get
            {
                return GetBooleanValue("SendNotification", false);
            }
            set
            {
                SetValue("SendNotification", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MembershipUserInfoProvider.DeleteMembershipUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MembershipUserInfoProvider.SetMembershipUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MembershipUserInfo object.
        /// </summary>
        public MembershipUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MembershipUserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MembershipUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}