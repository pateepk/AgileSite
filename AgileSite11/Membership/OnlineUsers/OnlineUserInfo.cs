using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(OnlineUserInfo), OnlineUserInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// On-line users info data container class.
    /// </summary>
    public class OnlineUserInfo : AbstractInfo<OnlineUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.onlineuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OnlineUserInfoProvider), OBJECT_TYPE, "CMS.OnlineUser", "SessionID", null, null, null, "SessionFullName", null, "SessionSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("SessionUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
            },
            RegisterAsBindingToObjectTypes = new List<string>(),
            RegisterAsOtherBindingToObjectTypes = new List<string>()
        };

        #endregion


        /// <summary>
        /// Contact ID corresponding to the session.
        /// Used internally in tests.
        /// </summary>
        [DatabaseField]
        internal virtual int SessionContactID
        {
            get
            {
                return GetIntegerValue("SessionContactID", 0);
            }
            set
            {
                SetValue("SessionContactID", value);
            }
        }


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OnlineUserInfo object.
        /// </summary>
        public OnlineUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OnlineUserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public OnlineUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
