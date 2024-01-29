using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(OpenIDUserInfo), OpenIDUserInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// OpenIDUser data container class.
    /// </summary>
    public class OpenIDUserInfo : AbstractInfo<OpenIDUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.openiduser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OpenIDUserInfoProvider), OBJECT_TYPE, "CMS.OpenIDUser", "OpenIDUserID", null, null, null, null, null, null, "UserID", UserInfo.OBJECT_TYPE)
                                              {
                                                  ModuleName = "cms.users",
                                                  AllowRestore = false,
                                                  ImportExportSettings = { LogExport = false },
                                                  SupportsVersioning = false,
                                                  TouchCacheDependencies = true,
                                                  LogEvents = false
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Object ID.
        /// </summary>
        public virtual int OpenIDUserID
        {
            get
            {
                return GetIntegerValue("OpenIDUserID", 0);
            }
            set
            {
                SetValue("OpenIDUserID", value);
            }
        }


        /// <summary>
        /// OpenID Claimed Identifier.
        /// </summary>
        public virtual string OpenID
        {
            get
            {
                return GetStringValue("OpenID", "");
            }
            set
            {
                SetValue("OpenID", value);
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
        /// URL of OpenID provider.
        /// </summary>
        public virtual string OpenIDProviderUrl
        {
            get
            {
                return GetStringValue("OpenIDProviderUrl", "");
            }
            set
            {
                SetValue("OpenIDProviderUrl", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OpenIDUserInfoProvider.DeleteOpenIDUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OpenIDUserInfoProvider.SetOpenIDUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OpenIDUser object.
        /// </summary>
        public OpenIDUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OpenIDUser object from the given DataRow.
        /// </summary>
        public OpenIDUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}