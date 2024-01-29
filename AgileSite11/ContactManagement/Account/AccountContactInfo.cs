using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(AccountContactInfo), AccountContactInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// AccountContactInfo data container class.
    /// </summary>
    public class AccountContactInfo : AbstractInfo<AccountContactInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.accountcontact";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AccountContactInfoProvider), OBJECT_TYPE, "OM.AccountContact", "AccountContactID", null, null, null, null, null, null, "ContactID", ContactInfo.OBJECT_TYPE)
                                              {
                                                  SynchronizationSettings =
                                                  {
                                                      IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                      LogSynchronization = SynchronizationTypeEnum.None
                                                  },
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  DependsOn = new List<ObjectDependency>
                                                      {
                                                          new ObjectDependency("AccountID", AccountInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding), 
                                                          new ObjectDependency("ContactRoleID", ContactRoleInfo.OBJECT_TYPE)
                                                      },
                                                  AllowRestore = false,
                                                  SupportsCloning = false,
                                                  IsBinding = true,
                                                  ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
                                                  Feature = FeatureEnum.FullContactManagement,
                                                  ContainsMacros = false,
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// Primary key - ID of relation.
        /// </summary>
        public virtual int AccountContactID
        {
            get
            {
                return GetIntegerValue("AccountContactID", 0);
            }
            set
            {
                SetValue("AccountContactID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the account.
        /// </summary>
        public virtual int AccountID
        {
            get
            {
                return GetIntegerValue("AccountID", 0);
            }
            set
            {
                SetValue("AccountID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the contact.
        /// </summary>
        public virtual int ContactID
        {
            get
            {
                return GetIntegerValue("ContactID", 0);
            }
            set
            {
                SetValue("ContactID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the contact's role.
        /// </summary>
        public virtual int ContactRoleID
        {
            get
            {
                return GetIntegerValue("ContactRoleID", 0);
            }
            set
            {
                SetValue("ContactRoleID", value, (value > 0));
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AccountContactInfoProvider.DeleteAccountContactInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AccountContactInfoProvider.SetAccountContactInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AccountContactInfo object.
        /// </summary>
        public AccountContactInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AccountContactInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public AccountContactInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}