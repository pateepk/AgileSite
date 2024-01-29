using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;

[assembly: RegisterObjectType(typeof(AccountInfo), AccountInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// AccountInfo data container class.
    /// </summary>
    [Serializable]
    public class AccountInfo : AbstractInfo<AccountInfo>
    {
        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.account";


        private IInfoObjectCollection mSubsidiaries;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AccountInfoProvider), OBJECT_TYPE, "OM.Account", "AccountID", "AccountLastModified", "AccountGUID", null, 
            "AccountName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("AccountCountryID", CountryInfo.OBJECT_TYPE),
                new ObjectDependency("AccountStateID", StateInfo.OBJECT_TYPE),
                new ObjectDependency("AccountOwnerUserID", UserInfo.OBJECT_TYPE),
                new ObjectDependency("AccountSubsidiaryOfID", OBJECT_TYPE),
                new ObjectDependency("AccountStatusID", AccountStatusInfo.OBJECT_TYPE),
                new ObjectDependency("AccountPrimaryContactID", ContactInfo.OBJECT_TYPE),
                new ObjectDependency("AccountSecondaryContactID", ContactInfo.OBJECT_TYPE)
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.CONTACTMANAGEMENT,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            Feature = FeatureEnum.FullContactManagement
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the account's state ID.
        /// </summary>
        public virtual int AccountStateID
        {
            get
            {
                return GetIntegerValue("AccountStateID", 0);
            }
            set
            {
                SetValue("AccountStateID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the account's email address.
        /// </summary>
        public virtual string AccountEmail
        {
            get
            {
                return GetStringValue("AccountEmail", "");
            }
            set
            {
                SetValue("AccountEmail", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the account was created.
        /// </summary>
        public virtual DateTime AccountCreated
        {
            get
            {
                return GetDateTimeValue("AccountCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AccountCreated", value);
            }
        }


        /// <summary>
        /// Gets or sets the notes for this account.
        /// </summary>
        public virtual string AccountNotes
        {
            get
            {
                return GetStringValue("AccountNotes", "");
            }
            set
            {
                SetValue("AccountNotes", value);
            }
        }


        /// <summary>
        /// Gets or sets the user that owns this account.
        /// </summary>
        public virtual int AccountOwnerUserID
        {
            get
            {
                return GetIntegerValue("AccountOwnerUserID", 0);
            }
            set
            {
                SetValue("AccountOwnerUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the second line of the account's address.
        /// </summary>
        public virtual string AccountAddress2
        {
            get
            {
                return GetStringValue("AccountAddress2", "");
            }
            set
            {
                SetValue("AccountAddress2", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's city.
        /// </summary>
        public virtual string AccountCity
        {
            get
            {
                return GetStringValue("AccountCity", "");
            }
            set
            {
                SetValue("AccountCity", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's fax number.
        /// </summary>
        public virtual string AccountFax
        {
            get
            {
                return GetStringValue("AccountFax", "");
            }
            set
            {
                SetValue("AccountFax", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's secondary contact ID.
        /// </summary>
        public virtual int AccountSecondaryContactID
        {
            get
            {
                return GetIntegerValue("AccountSecondaryContactID", 0);
            }
            set
            {
                SetValue("AccountSecondaryContactID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the account's ID.
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
        /// Gets or sets the account's phone number.
        /// </summary>
        public virtual string AccountPhone
        {
            get
            {
                return GetStringValue("AccountPhone", "");
            }
            set
            {
                SetValue("AccountPhone", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's primary contact ID.
        /// </summary>
        public virtual int AccountPrimaryContactID
        {
            get
            {
                return GetIntegerValue("AccountPrimaryContactID", 0);
            }
            set
            {
                SetValue("AccountPrimaryContactID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the account's name.
        /// </summary>
        public virtual string AccountName
        {
            get
            {
                return GetStringValue("AccountName", "");
            }
            set
            {
                SetValue("AccountName", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the account was last modified.
        /// </summary>
        public virtual DateTime AccountLastModified
        {
            get
            {
                return GetDateTimeValue("AccountLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AccountLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's status ID.
        /// </summary>
        public virtual int AccountStatusID
        {
            get
            {
                return GetIntegerValue("AccountStatusID", 0);
            }
            set
            {
                SetValue("AccountStatusID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the first line of the account's address.
        /// </summary>
        public virtual string AccountAddress1
        {
            get
            {
                return GetStringValue("AccountAddress1", "");
            }
            set
            {
                SetValue("AccountAddress1", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's web site URL.
        /// </summary>
        public virtual string AccountWebSite
        {
            get
            {
                return GetStringValue("AccountWebSite", "");
            }
            set
            {
                SetValue("AccountWebSite", value);
            }
        }


        /// <summary>
        /// Gets or sets the account that is a parent account to this one.
        /// </summary>
        public virtual int AccountSubsidiaryOfID
        {
            get
            {
                return GetIntegerValue("AccountSubsidiaryOfID", 0);
            }
            set
            {
                SetValue("AccountSubsidiaryOfID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the account's GUID.
        /// </summary>
        public virtual Guid AccountGUID
        {
            get
            {
                return GetGuidValue("AccountGUID", Guid.Empty);
            }
            set
            {
                SetValue("AccountGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the account's country ID.
        /// </summary>
        public virtual int AccountCountryID
        {
            get
            {
                return GetIntegerValue("AccountCountryID", 0);
            }
            set
            {
                SetValue("AccountCountryID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the account's postal code.
        /// </summary>
        public virtual string AccountZIP
        {
            get
            {
                return GetStringValue("AccountZIP", "");
            }
            set
            {
                SetValue("AccountZIP", value);
            }
        }


        /// <summary>
        /// Collection of subsidiaries belonging to the account.
        /// </summary>
        public virtual IInfoObjectCollection Subsidiaries
        {
            get
            {
                if (mSubsidiaries == null)
                {
                    var accounts = new InfoObjectCollection("om.account");
                    accounts.Where = new WhereCondition().WhereEquals("AccountSubsidiaryOfID", ObjectID);
                    accounts.OrderByColumns = "AccountName";
                    accounts.ChangeParent(null, Generalized);
                    mSubsidiaries = accounts;
                }

                return mSubsidiaries;
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AccountInfoProvider.DeleteAccountInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AccountInfoProvider.SetAccountInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AccountInfo object.
        /// </summary>
        public AccountInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AccountInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public AccountInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public AccountInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Returns name of the permission used for checking global permissions.
        /// </summary>
        /// <param name="permissionName">Name of the original permission</param>
        protected override string GetGlobalPermissionName(string permissionName)
        {
            // There is no special global permission in contact management
            return permissionName;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            AccountCreated = DateTime.Now;

            Insert();

            bool contactGroup = false;
            bool copySubsidiaries = true;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                contactGroup = ValidationHelper.GetBoolean(p["om.account" + ".contactgroup"], false);
                copySubsidiaries = ValidationHelper.GetBoolean(p["om.account" + ".subsidiaries"], true);
            }

            string originalName = settings.CodeName;
            string originalDisplayName = settings.DisplayName;
            settings.DisplayName = null;
            settings.CodeName = null;

            // Clone contact groups bindings
            if (contactGroup)
            {
                if (settings.ExcludedBindingTypes.Contains(ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT))
                {
                    settings.ExcludedBindingTypes.Remove(ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT);
                }
            }
            else
            {
                settings.ExcludedBindingTypes.Add(ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT);
            }

            if (copySubsidiaries)
            {
                AccountInfo originalAccount = ((AccountInfo)originalObject);

                // Clone merged accounts
                foreach (AccountInfo account in originalAccount.Subsidiaries)
                {
                    AccountInfo accountClone = account.Clone();
                    accountClone.AccountSubsidiaryOfID = AccountID;
                    accountClone.InsertAsClone(settings, result);
                }
            }

            settings.DisplayName = originalName;
            settings.CodeName = originalDisplayName;
        }

        #endregion
    }
}