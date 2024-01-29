using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DataEngine;
using CMS.Messaging;

[assembly: RegisterObjectType(typeof(ContactListInfo), ContactListInfo.OBJECT_TYPE)]

namespace CMS.Messaging
{
    /// <summary>
    /// ContactListInfo data container class.
    /// </summary>
    public class ContactListInfo : AbstractInfo<ContactListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CONTACTLIST;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactListInfoProvider), OBJECT_TYPE, "Messaging.ContactList", null, null, null, null, null, null, null, "ContactListUserID", UserInfo.OBJECT_TYPE)
                                              {
                                                  TouchCacheDependencies = true,
                                                  DependsOn = new List<ObjectDependency>()
                                                  {
                                                      new ObjectDependency("ContactListContactUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                                                  },
                                                  ModuleName = "cms.messaging",
                                                  SynchronizationSettings =
                                                    {
                                                        IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                        LogSynchronization = SynchronizationTypeEnum.None,
                                                    },
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of user in contact list.
        /// </summary>
        public virtual int ContactListContactUserID
        {
            get
            {
                return GetIntegerValue("ContactListContactUserID", 0);
            }
            set
            {
                SetValue("ContactListContactUserID", value);
            }
        }


        /// <summary>
        /// Contact List User ID.
        /// </summary>
        public virtual int ContactListUserID
        {
            get
            {
                return GetIntegerValue("ContactListUserID", 0);
            }
            set
            {
                SetValue("ContactListUserID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactListInfoProvider.DeleteContactListInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactListInfoProvider.SetContactListInfo(this);
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
                case PermissionsEnum.Create:
                case PermissionsEnum.Read:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = (ContactListUserID == userInfo.UserID);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactListInfo object.
        /// </summary>
        public ContactListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactListInfo object from the given DataRow.
        /// </summary>
        public ContactListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}