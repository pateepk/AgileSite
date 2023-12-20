using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

using CMS;
using CMS.Activities;
using CMS.Automation;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using CMS.Core.Internal;

[assembly: RegisterObjectType(typeof(ContactInfo), ContactInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactInfo data container class.
    /// </summary>
    [Serializable]
    public class ContactInfo : AbstractInfo<ContactInfo>
    {
        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CONTACT;


        private IInfoObjectCollection<AccountInfo> mAccounts;
        private IInfoObjectCollection mOrders;
        private IInfoObjectCollection mPurchasedProducts;
        private IInfoObjectCollection mWishlist;
        private IInfoObjectCollection<UserInfo> mUsers;
        private IInfoObjectCollection<RoleInfo> mRoles;
        private IInfoObjectCollection<ContactGroupInfo> mContactGroups;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactInfoProvider), OBJECT_TYPE, "OM.Contact", "ContactID", "ContactLastModified", "ContactGUID", 
            null, "ContactLastName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ContactStateID", StateInfo.OBJECT_TYPE),
                new ObjectDependency("ContactCountryID", CountryInfo.OBJECT_TYPE),
                new ObjectDependency("ContactStatusID", ContactStatusInfo.OBJECT_TYPE),
                new ObjectDependency("ContactOwnerUserID", UserInfo.OBJECT_TYPE),
                new ObjectDependency("ContactPersonaID", PredefinedObjectType.PERSONA)
            },
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(PredefinedObjectType.EMAILQUEUEITEM, "EmailContactID"),
                new ExtraColumn(OnlineUserInfo.OBJECT_TYPE, "SessionContactID"),
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
            SupportsCloning = false,
            SupportsInvalidation = true,
            HasProcesses = true,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            Feature = FeatureEnum.SimpleContactManagement,
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the contact's e-mail address.
        /// </summary>
        [DatabaseField]
        public virtual string ContactEmail
        {
            get
            {
                return GetStringValue("ContactEmail", "");
            }
            set
            {
                SetValue("ContactEmail", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the contact was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ContactLastModified
        {
            get
            {
                return GetDateTimeValue("ContactLastModified", DateTime.MinValue);
            }
            set
            {
                SetValue("ContactLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets whether this contact is monitored.
        /// </summary>
        [DatabaseField]
        public virtual bool ContactMonitored
        {
            get
            {
                return GetBooleanValue("ContactMonitored", false);
            }
            set
            {
                SetValue("ContactMonitored", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's birth date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ContactBirthday
        {
            get
            {
                return GetDateTimeValue("ContactBirthday", DateTime.MinValue);
            }
            set
            {
                SetValue("ContactBirthday", value, (value > DateTime.MinValue));
            }
        }


        /// <summary>
        /// Gets or sets the contact's gender.
        /// </summary>
        [DatabaseField]
        public virtual int ContactGender
        {
            get
            {
                return GetIntegerValue("ContactGender", 0);
            }
            set
            {
                SetValue("ContactGender", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the contact's first name.
        /// </summary>
        [DatabaseField]
        public virtual string ContactFirstName
        {
            get
            {
                return GetStringValue("ContactFirstName", "");
            }
            set
            {
                SetValue("ContactFirstName", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's country ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContactCountryID
        {
            get
            {
                return GetIntegerValue("ContactCountryID", 0);
            }
            set
            {
                SetValue("ContactCountryID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the contact's ID.
        /// </summary>
        [DatabaseField]
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
        /// Gets or sets the contact's mobile phone number.
        /// </summary>
        [DatabaseField]
        public virtual string ContactMobilePhone
        {
            get
            {
                return GetStringValue("ContactMobilePhone", "");
            }
            set
            {
                SetValue("ContactMobilePhone", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's last name.
        /// </summary>
        [DatabaseField]
        public virtual string ContactLastName
        {
            get
            {
                return GetStringValue("ContactLastName", "");
            }
            set
            {
                SetValue("ContactLastName", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's city.
        /// </summary>
        [DatabaseField]
        public virtual string ContactCity
        {
            get
            {
                return GetStringValue("ContactCity", "");
            }
            set
            {
                SetValue("ContactCity", value);
            }
        }


		/// <summary>
		/// Gets or sets whether the contact is anonymous.
		/// </summary>
		public virtual bool ContactIsAnonymous
        {
            get
            {
				return !ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.CmsUser).Any();
			}
        }


        /// <summary>
        /// Gets or sets the ID of the user that owns this contact.
        /// </summary>
        [DatabaseField]
        public virtual int ContactOwnerUserID
        {
            get
            {
                return GetIntegerValue("ContactOwnerUserID", 0);
            }
            set
            {
                SetValue("ContactOwnerUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the the date and time when the contact was created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ContactCreated
        {
            get
            {
                return GetDateTimeValue("ContactCreated", DateTime.MinValue);
            }
            set
            {
                SetValue("ContactCreated", value, (value > DateTime.MinValue));
            }
        }


        /// <summary>
        /// Gets or sets the contact's unique identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid ContactGUID
        {
            get
            {
                return GetGuidValue("ContactGUID", Guid.Empty);
            }
            set
            {
                SetValue("ContactGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's job title.
        /// </summary>
        [DatabaseField]
        public virtual string ContactJobTitle
        {
            get
            {
                return GetStringValue("ContactJobTitle", "");
            }
            set
            {
                SetValue("ContactJobTitle", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's state ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContactStateID
        {
            get
            {
                return GetIntegerValue("ContactStateID", 0);
            }
            set
            {
                SetValue("ContactStateID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the first line of the contact's address.
        /// </summary>
        [DatabaseField]
        public virtual string ContactAddress1
        {
            get
            {
                return GetStringValue("ContactAddress1", "");
            }
            set
            {
                SetValue("ContactAddress1", value);
            }
        }


        /// <summary>
        /// Gets or sets the notes for this contact.
        /// </summary>
        [DatabaseField]
        public virtual string ContactNotes
        {
            get
            {
                return GetStringValue("ContactNotes", "");
            }
            set
            {
                SetValue("ContactNotes", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's postal code.
        /// </summary>
        [DatabaseField]
        public virtual string ContactZIP
        {
            get
            {
                return GetStringValue("ContactZIP", "");
            }
            set
            {
                SetValue("ContactZIP", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's middle name.
        /// </summary>
        [DatabaseField]
        public virtual string ContactMiddleName
        {
            get
            {
                return GetStringValue("ContactMiddleName", "");
            }
            set
            {
                SetValue("ContactMiddleName", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's business phone number.
        /// </summary>
        [DatabaseField]
        public virtual string ContactBusinessPhone
        {
            get
            {
                return GetStringValue("ContactBusinessPhone", "");
            }
            set
            {
                SetValue("ContactBusinessPhone", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact's status ID.
        /// </summary>
        [DatabaseField]
        public virtual int ContactStatusID
        {
            get
            {
                return GetIntegerValue("ContactStatusID", 0);
            }
            set
            {
                SetValue("ContactStatusID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the contact's campaign
        /// </summary>
        [DatabaseField]
        public virtual string ContactCampaign
        {
            get
            {
                return GetStringValue("ContactCampaign", String.Empty);
            }
            set
            {
                SetValue("ContactCampaign", value);
            }
        }


        /// <summary>
        /// Gets or sets the number of bounced e-mails for this contact.
        /// </summary>
        [DatabaseField]
        public virtual int ContactBounces
        {
            get
            {
                return GetIntegerValue("ContactBounces", 0);
            }
            set
            {
                SetValue("ContactBounces", value, (value > 0));
            }
        }

        
        /// <summary>
        /// Gets or sets the contact's e-company name.
        /// </summary>
        [DatabaseField]
        public virtual string ContactCompanyName
        {
            get
            {
                return GetStringValue("ContactCompanyName", "");
            }
            set
            {
                SetValue("ContactCompanyName", value);
            }
        }


        /// <summary>
        /// Gets or sets persona ID the contact belongs to or null, if no such persona exists.
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public virtual int? ContactPersonaID
        {
            get
            {
                var value = GetIntegerValue("ContactPersonaID", 0);
                return value == 0 ? null : (int?)value;
            }
            set
            {
                SetValue("ContactPersonaID", value);
            }
        }


        /// <summary>
        /// Gets the contact's age (returns 0 if contact's birthday is not set).
        /// </summary>
        public virtual int ContactAge
        {
            get
            {
                if (ContactBirthday == DateTime.MinValue)
                {
                    return 0;
                }

                var now = Service.Resolve<IDateTimeNowService>().GetDateTimeNow();
                int age = now.Year - ContactBirthday.Year;
                if (ContactBirthday > now.AddYears(-age))
                {
                    age--;
                }
                return age;
            }
        }


        /// <summary>
        /// Gets the contact full name with 'merged'/'global' flags.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public virtual string ContactDescriptiveName
        {
            get
            {                 
                return ContactInfoProvider.GetContactFullName(this);
            }
        }


        /// <summary>
        /// Collection of the accounts belonging to the object.
        /// </summary>
        public virtual IInfoObjectCollection<AccountInfo> Accounts
        {
            get
            {
                if (mAccounts != null)
                {
                    return mAccounts;
                }

                // Create the collection for accounts
                var accounts = new InfoObjectCollection<AccountInfo>();

                accounts.AddCacheDependencies(String.Format("om.contact|byid|{0}|children|om.accountcontact", ContactID));
                accounts.Where = new WhereCondition("AccountID IN (SELECT OM_AccountContact.AccountID FROM OM_AccountContact WHERE OM_AccountContact.ContactID = " + ObjectID + ")");
                accounts.OrderByColumns = "AccountName";
                accounts.ChangeParent(null, Generalized);

                mAccounts = accounts;

                return mAccounts;
            }
        }


        /// <summary>
        /// Collection of all contact orders.
        /// Contact does not have any orders even thou its merged contact have them.
        /// </summary>
        public virtual IInfoObjectCollection Orders
        {
            get
            {
                if (mOrders != null)
                {
                    return mOrders;
                }

                // Get users for contact
                List<int> membershipUserIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.CmsUser);

                // Get customers for contact
                List<int> membershipCustomersIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.EcommerceCustomer);

                WhereCondition ordersWhereCondition;
                if (membershipUserIDs.Count > 0 || membershipCustomersIDs.Count > 0)
                {
                    ordersWhereCondition = new WhereCondition().WhereIn("OrderCustomerID", new DataQuery().Column("CustomerID")
                                                                                                          .From("COM_Customer")
                                                                                                          .WhereIn("CustomerUserID", membershipUserIDs)
                                                                                                          .Or()
                                                                                                          .WhereIn("CustomerID", membershipCustomersIDs));
                }
                else
                {
                    ordersWhereCondition = new WhereCondition().NoResults();
                }
                
                // Create collection of customer orders
                var col = new InfoObjectCollection(PredefinedObjectType.ORDER);

                col.AddCacheDependencies("ecommerce.order|all");
                col.Where = ordersWhereCondition;
                col.OrderByColumns = "OrderDate DESC";
                col.ChangeParent(null, Generalized);

                mOrders = col;

                return mOrders;
            }
        }


        /// <summary>
        /// Collection of all contacts purchased products from all sites.
        /// Global contact does not have any products even thou its merged site contact have them.
        /// </summary>
        public virtual IInfoObjectCollection PurchasedProducts
        {
            get
            {
                if (mPurchasedProducts != null)
                {
                    return mPurchasedProducts;
                }

                // Get users for contact
                List<int> membershipUserIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.CmsUser);

                // Get customers for contact
                List<int> membershipCustomersIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.EcommerceCustomer);

                WhereCondition whereCondition;
                if (membershipUserIDs.Count > 0 || membershipCustomersIDs.Count > 0)
                {
                    whereCondition = new WhereCondition().WhereIn("SKUID", new DataQuery().Column("OrderItemSKUID")
                                                                                          .From("COM_OrderItem")
                                                                                          .WhereIn("OrderItemOrderID",
                                                                                              new DataQuery().Column("OrderID")
                                                                                                             .From("COM_Order")
                                                                                                             .WhereIn("OrderCustomerID",
                                                                                                                 new DataQuery().Column("CustomerID")
                                                                                                                                .From("COM_Customer")
                                                                                                                                .WhereIn("CustomerUserID", membershipUserIDs)
                                                                                                                                .Or()
                                                                                                                                .WhereIn("CustomerID", membershipCustomersIDs))));
                }
                else
                {
                    whereCondition = new WhereCondition().NoResults();
                }

                // Create collection of purchased products from all sites.
                var col = new InfoObjectCollection(PredefinedObjectType.SKU);

                col.AddCacheDependencies("ecommerce.orderitem|all");
                col.Where = whereCondition;
                col.OrderByColumns = "SKUName";
                col.ChangeParent(null, Generalized);

                mPurchasedProducts = col;

                return mPurchasedProducts;
            }
        }


        /// <summary>
        /// Collection of all contacts wishlist items from all sites.
        /// Global contact does not have any wishlist items even thou its merged site contact have them.
        /// </summary>
        public virtual IInfoObjectCollection Wishlist
        {
            get
            {
                if (mWishlist != null)
                {
                    return mWishlist;
                }

                // Get users for contact
                List<int> membershipIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.CmsUser);

                WhereCondition wishListWhereCondition;
                if (membershipIDs.Count > 0)
                {
                    // Get items in wishlist for all users assigned to this contact
                    var wishListQuery = new DataQuery().Column("COM_Wishlist.SKUID").From("COM_Wishlist").WhereIn("COM_Wishlist.UserID", membershipIDs);
                    // Where condition to find all SKUs for items in wishlist
                    wishListWhereCondition = new WhereCondition().WhereIn("SKUID", wishListQuery);
                }
                else
                {
                    // Contact has no users assigned which means he can't have items in wishlist
                    wishListWhereCondition = new WhereCondition().NoResults();
                }

                // Create collection of wishlist items from all sites
                var col = new InfoObjectCollection(PredefinedObjectType.SKU);
                col.AddCacheDependencies("ecommerce.wishlist|all");
                col.Where = wishListWhereCondition;
                col.OrderByColumns = "SKUName";
                col.ChangeParent(null, Generalized);

                mWishlist = col;

                return mWishlist;
            }
        }


        /// <summary>
        /// Collection of all contacts users. Global contact does not have any users even thou its merged site contact have them.
        /// </summary>
        public virtual IInfoObjectCollection<UserInfo> Users
        {
            get
            {
                if (mUsers != null)
                {
                    return mUsers;
                }

                // Get users for contact
                List<int> membershipIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.CmsUser);

                var usersWhereCondition = new WhereCondition().WhereIn("UserID", membershipIDs);

                // Create collection of users
                var col = new InfoObjectCollection<UserInfo>();

                col.AddCacheDependencies("cms.user|all", "cms.userrole|all");
                col.Where = usersWhereCondition;
                col.OrderByColumns = "UserName";
                col.ChangeParent(null, Generalized);

                mUsers = col;

                return mUsers;
            }
        }


        /// <summary>
        /// Collection of all contacts roles.
        /// Global contact does not have any roles even thou its merged site contact have them.
        /// </summary>
        public virtual IInfoObjectCollection<RoleInfo> Roles
        {
            get
            {
                if (mRoles != null)
                {
                    return mRoles;
                }

                // Get users for contact
                List<int> membershipUserIDs = ContactMembershipInfoProvider.GetRelationships(ContactID, MemberTypeEnum.CmsUser);

                // Create collection of roles
                var col = new InfoObjectCollection<RoleInfo>();
                col.AddCacheDependencies("cms.role|all");
                col.Where = new WhereCondition("RoleID IN (SELECT RoleID FROM CMS_UserRole WHERE (" + SqlHelper.GetWhereCondition("UserID", membershipUserIDs.AsEnumerable()) + "))");
                col.OrderByColumns = "RoleName";
                col.ChangeParent(null, Generalized);

                mRoles = col;

                return mRoles;
            }
        }


        /// <summary>
        /// Collection of all contacts groups. Value is cached for 1 minute.
        /// </summary>
        public virtual IInfoObjectCollection<ContactGroupInfo> ContactGroups
        {
            get
            {
                // Create collection of contact groups
                return CacheHelper.Cache(() =>
                {
                    var contactRelationships = ContactGroupMemberInfoProvider.GetRelationships()
                                                    .Column("ContactGroupMemberContactGroupID") 
                                                    .WhereEquals("ContactGroupMemberRelatedID", ObjectID)
                                                    .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact);
                   
                    var col = new InfoObjectCollection<ContactGroupInfo>();
                    col.Where = new WhereCondition().WhereIn("ContactGroupID", contactRelationships);
                    col.OrderByColumns = "ContactGroupName";
                    col.ChangeParent(null, Generalized);
                    mContactGroups = col;
                    
                    return mContactGroups;

                }, new CacheSettings(1, "ContactInfo.ContactGroups", ContactID));
            }
        }


        /// <summary>
        /// Last activity of the contact.
        /// </summary>
        public virtual ActivityInfo LastActivity
        {
            get
            {
                return ActivityInfoProvider.GetContactsLastActivity(ObjectID, null);
            }
        }


        /// <summary>
        /// Indicates if contact already visited site and is IP and User-Agent information are stored in session.
        /// </summary>
        public virtual bool CurrentIPStored
        {
            get
            {
                return ValidationHelper.GetBoolean(ContextHelper.GetItem("CurrentIPStored|" + ContactID, false, true, false), false);
            }
            set
            {
                // Set validity to 23 hours (when the user comes at the same time next day, gets next visit)
                ContextHelper.Add("CurrentIPStored|" + ContactID, value, false, true, false, Service.Resolve<IDateTimeNowService>().GetDateTimeNow().AddHours(23));
            }
        }


        /// <summary>
        /// Determines whether the contact was created manually from the administration or automatically during the request.
        /// </summary>
        public virtual bool ContactCreatedInAdministration
        {
            get;
            set;
        }


        /// <summary>
        /// Register the custom properties
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Accounts", c => c.Accounts);
            RegisterProperty("LastActivity", c => c.LastActivity);
            RegisterProperty("Orders", m => m.Orders);
            RegisterProperty("PurchasedProducts", m => m.PurchasedProducts);
            RegisterProperty("Wishlist", m => m.Wishlist);
            RegisterProperty("Roles", m => m.Roles);
            RegisterProperty("Users", m => m.Users);
            RegisterProperty("ContactGroups", m => m.ContactGroups);
            RegisterProperty("ContactAge", m => m.ContactAge);
            RegisterProperty("ContactIsAnonymous", c => c.ContactIsAnonymous);
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactInfoProvider.DeleteContactInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactInfoProvider.SetContactInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactInfo object.
        /// </summary>
        public ContactInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ContactInfo(SerializationInfo info, StreamingContext context)
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
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Remove marketing automation dependencies
            IList<int> ids = new List<int> { ContactID };
            AutomationStateInfoProvider.DeleteAutomationStates(PredefinedObjectType.CONTACT, ids);

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion
    }
}