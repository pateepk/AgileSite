using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Base;
using CMS.Localization;
using CMS.Protection;

[assembly: RegisterObjectType(typeof(BadWordCultureInfo), BadWordCultureInfo.OBJECT_TYPE)]

namespace CMS.Protection
{
    /// <summary>
    /// BadWordCultureInfo data container class.
    /// </summary>
    public class BadWordCultureInfo : AbstractInfo<BadWordCultureInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "badwords.wordculture";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BadWordCultureInfoProvider), OBJECT_TYPE, "badwords.wordculture", null, null, null, null, null, null, null, "WordID", BadWordInfo.OBJECT_TYPE)
                                              {
                                                  TouchCacheDependencies = true,
                                                  DependsOn = new List<ObjectDependency>() { new ObjectDependency("CultureID", CultureInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
                                                  ModuleName = "cms.badwords",
                                                  DefaultData = new DefaultDataSettings(),
                                                  ContinuousIntegrationSettings =
                                                  {
                                                      Enabled = true
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Word ID.
        /// </summary>
        public virtual int WordID
        {
            get
            {
                return GetIntegerValue("WordID", 0);
            }
            set
            {
                SetValue("WordID", value);
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
            BadWordCultureInfoProvider.DeleteBadWordCultureInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BadWordCultureInfoProvider.SetBadWordCultureInfo(this);
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
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, "UseBadWords", siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BadWordCultureInfo object.
        /// </summary>
        public BadWordCultureInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BadWordCultureInfo object from the given DataRow.
        /// </summary>
        public BadWordCultureInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}