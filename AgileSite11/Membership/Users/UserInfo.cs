using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;

using CMS;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.Modules;
using CMS.Protection;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Synchronization;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

[assembly: RegisterObjectType(typeof(UserInfo), UserInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// UserInfo data container class.
    /// </summary>
    [Serializable]
    public class UserInfo : AbstractInfo<UserInfo>, IUserInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.USER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UserInfoProvider), OBJECT_TYPE, "CMS.User", "UserID", "UserLastModified", "UserGUID", "UserName", "FullName", null, null, null, null)
        {
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(ObjectVersionHistoryInfo.OBJECT_TYPE, "VersionModifiedByUserID"),
                new ExtraColumn(ObjectVersionHistoryInfo.OBJECT_TYPE, "VersionDeletedByUserID"),
                new ExtraColumn(EmailUserInfo.OBJECT_TYPE, "UserID", ObjectDependencyEnum.Binding),
                new ExtraColumn(EventLogInfo.OBJECT_TYPE, "UserID"),
                new ExtraColumn(AbuseReportInfo.OBJECT_TYPE, "ReportUserID"),
                new ExtraColumn(ObjectSettingsInfo.OBJECT_TYPE, "ObjectCheckedOutByUserID"),
            },
            ModuleName = "cms.users",
            Feature = FeatureEnum.SiteMembers,
            SupportsInvalidation = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                },
                ExcludedStagingColumns = new List<string>
                {
                    "UserVisibility"
                }
            },
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = true,
            EnabledColumn = "UserEnabled",
            SensitiveColumns = new List<string> { "UserPassword", "UserPasswordRequestHash", "UserPasswordLastChanged", "UserPasswordFormat", "UserAuthenticationGUID", "UserMFSecret", "UserMFTimestep", "UserSecurityStamp" },
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "UserLastLogonInfo", "UserMFSecret", "UserMFTimestep", "UserSecurityStamp" }
            },
            NestedInfoTypes = new List<string> { UserSettingsInfo.OBJECT_TYPE },
            SerializationSettings =
            {
                ExcludedFieldNames = { "UserCreated", "LastLogon", "UserLastLogonInfo", "UserMFSecret", "UserMFTimestep" },
                AdditionalFieldFilter = IsExcludedField,
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("UserVisibility")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            }
        };

        #endregion


        #region "Variables"

        private UserSettingsInfo mUserSettings;
        private SafeDictionary<string, SafeDictionary<string, int?>> mSiteCultures;
        private UserDataInfo mUserLastLogonInfo;
        private TimeZoneInfo mTimeZoneInfo;
        private IInfoObjectCollection mOrders;
        private IInfoObjectCollection mPurchasedProducts;
        private IInfoObjectCollection mWishlist;
        private SafeDictionary<int, FriendshipStatusEnum> mFriends;
        private SafeDictionary<int, int> mGroups;
        private readonly StringSafeDictionary<bool> mGroupsMember = new StringSafeDictionary<bool>();
        private static string mUserSaltColumn;

        private UserSecurityCollections userSecurityCollections;
        private readonly object securityCollectionsLoadLock = new object();

        // Collection with all UI elements within each module across all sites the user can see.
        private SafeDictionary<string, SafeDictionary<string, DateTime?>> mResourceUIElements;

        // Represents the group admin status of the user
        private const int GROUP_ADMIN = 3;

        /// <summary>
        /// Constant for global roles in siteroles collection.
        /// </summary>
        public const string GLOBAL_ROLES_KEY = "##GLOBALROLESKEY##";

        #endregion


        #region "IUserInfo properties"


        #region "UserInfo properties"

        /// <summary>
        /// A unique value used for tracking changes to the user profile. 
        /// Used for security purposes.
        /// </summary>
        /// <remarks>
        /// Currently this field is used only in our MVC UserStore implementation.
        /// Changing this field's value will invalidate any tokens generated within the Identity/OWIN implementation, e.g.:
        /// email confirmation tokens, authentication cookies, etc.
        /// </remarks>
        [DatabaseField]
        public virtual string UserSecurityStamp
        {
            get
            {
                return GetStringValue("UserSecurityStamp", string.Empty);
            }
            set
            {
                SetValue("UserSecurityStamp", value);
            }
        }


        /// <summary>
        /// Secret used in MFA.
        /// </summary>
        [DatabaseField]
        public virtual byte[] UserMFSecret
        {
            get
            {
                object encryptedSecret = GetValue("UserMFSecret");
                if (encryptedSecret != null)
                {
                    return MachineKey.Unprotect((byte[])encryptedSecret, "TOTP MF Secret");                  
                }

                return null;
            }     
            set
            {
                if (value != null)
                {
                    SetValue("UserMFSecret", MachineKey.Protect(value, "TOTP MF Secret"));
                }
                else
                {
                    SetValue("UserMFSecret", value);
                }               
            }
        }


        /// <summary>
        /// Timestep of the last valid passcode used by the user to authenticate.
        /// </summary>
        [DatabaseField]
        public virtual long UserMFTimestep
        {
            get
            {
                return ValidationHelper.GetLong(GetValue("UserMFTimestep"), 0);
            }
            set
            {
                SetValue("UserMFTimestep", value);
            }
        }


        /// <summary>
        /// Returns collection of the user Sites and roles.
        /// </summary>
        public virtual SafeDictionary<string, SafeDictionary<string, int?>> SitesRoles
        {
            get
            {
                return EnsureSecurityCollections().SitesRoles;
            }
        }


        /// <summary>
        /// Returns collection of user's membership
        /// </summary>
        internal SafeDictionary<string, SafeDictionary<string, int?>> MembershipsInternal
        {
            get
            {
                return EnsureSecurityCollections().Memberships;
            }
        }


        /// <summary>
        /// Returns the user's allowed cultures.
        /// </summary>
        internal SafeDictionary<string, SafeDictionary<string, int?>> SiteCulturesInternal
        {
            get
            {
                if (mSiteCultures == null)
                {
                    int userId = UserID;
                    if (userId > 0 && UserHasAllowedCultures)
                    {
                        // Load the Sites & cultures table
                        var userSites = UserInfoProvider.GetUserSites(userId).Columns("SiteName, SiteID");
                        DataTable userCultures = UserCultureInfoProvider.GetUserCultures(userId, 0);
                        mSiteCultures = CreateSiteCulturesTable(userSites, userCultures);
                    }
                    else
                    {
                        mSiteCultures = new SafeDictionary<string, SafeDictionary<string, int?>>();
                    }
                }
                return mSiteCultures;
            }
        }


        /// <summary>
        /// Returns UI elements within each module across all sites the user can see.
        /// </summary>
        internal SafeDictionary<string, SafeDictionary<string, DateTime?>> ResourceUIElementsInternal
        {
            get
            {
                return mResourceUIElements ?? (mResourceUIElements = CreateSiteUIElementsTable());
            }
        }


        /// <summary>
        /// Indicates whether user is hidden.
        /// </summary>
        [DatabaseField]
        public virtual bool UserIsHidden
        {
            get
            {
                return GetBooleanValue("UserIsHidden", false);
            }
            set
            {
                SetValue("UserIsHidden", value);
            }
        }


        /// <summary>
        /// Last name.
        /// </summary>
        [DatabaseField]
        public virtual string LastName
        {
            get
            {
                return GetStringValue("LastName", string.Empty);
            }
            set
            {
                SetValue("LastName", value);
            }
        }


        /// <summary>
        /// Full name of user.
        /// </summary>
        [DatabaseField]
        public virtual string FullName
        {
            get
            {
                return GetStringValue("FullName", string.Empty);
            }
            set
            {
                SetValue("FullName", value);
            }
        }


        /// <summary>
        /// Last logon of user.
        /// </summary>
        [DatabaseField]
        public virtual DateTime LastLogon
        {
            get
            {
                return GetDateTimeValue("LastLogon", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("LastLogon", value);
            }
        }


        /// <summary>
        /// Preferred culture code.
        /// </summary>
        [DatabaseField]
        public virtual string PreferredCultureCode
        {
            get
            {
                return GetStringValue("PreferredCultureCode", string.Empty);
            }
            set
            {
                SetValue("PreferredCultureCode", value);
            }
        }


        /// <summary>
        /// Middle name of user.
        /// </summary>
        [DatabaseField]
        public virtual string MiddleName
        {
            get
            {
                return GetStringValue("MiddleName", string.Empty);
            }
            set
            {
                SetValue("MiddleName", value);
            }
        }     


        /// <summary>
        /// Preferred UI culture code.
        /// </summary>
        [DatabaseField]
        public virtual string PreferredUICultureCode
        {
            get
            {
                return GetStringValue("PreferredUICultureCode", string.Empty);
            }
            set
            {
                SetValue("PreferredUICultureCode", value);
            }
        }


        /// <summary>
        /// Indicated whether user is external.
        /// </summary>
        [DatabaseField]
        public virtual bool UserIsExternal
        {
            get
            {
                return GetBooleanValue("UserIsExternal", false);
            }
            set
            {
                SetValue("UserIsExternal", value);
            }
        }


        /// <summary>
        /// Indicates whether user is enabled.
        /// </summary>
        /// <remarks>
        /// Property is identical to <see cref="UserInfo.Enabled"/> property, which should preferably be used.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [RegisterProperty(Hidden = true)]
        public virtual bool UserEnabled
        {
            get
            {
                return GetBooleanValue("UserEnabled", false);
            }
            set
            {
                SetValue("UserEnabled", value);
            }
        }


        /// <summary>
        /// Multi-factor authentication is required for user.
        /// </summary>
        [DatabaseField]
        public virtual bool UserMFRequired
        {
            get
            {
                return GetBooleanValue("UserMFRequired", false);
            }
            set
            {
                SetValue("UserMFRequired", value);
            }
        }


        /// <summary>
        /// First name of user.
        /// </summary>
        [DatabaseField]
        public virtual string FirstName
        {
            get
            {
                return GetStringValue("FirstName", string.Empty);
            }
            set
            {
                SetValue("FirstName", value);
            }
        }


        /// <summary>
        /// When was user created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserCreated
        {
            get
            {
                return GetDateTimeValue("UserCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UserCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// User ID.
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

                // Invalidate collections
                mOrders = null;
                mPurchasedProducts = null;
                mWishlist = null;
            }
        }


        /// <summary>
        /// Format of user's password.
        /// </summary>
        [DatabaseField]
        public virtual string UserPasswordFormat
        {
            get
            {
                return GetStringValue("UserPasswordFormat", string.Empty);
            }
            set
            {
                SetValue("UserPasswordFormat", value);
            }
        }


        /// <summary>
        /// User name.
        /// </summary>
        [DatabaseField]
        public virtual string UserName
        {
            get
            {
                return GetStringValue("UserName", string.Empty);
            }
            set
            {
                SetValue("UserName", value);
            }
        }


        /// <summary>
        /// E-mail address.
        /// </summary>
        [DatabaseField]
        public virtual string Email
        {
            get
            {
                return GetStringValue("Email", string.Empty);
            }
            set
            {
                SetValue("Email", value);
            }
        }


        /// <summary>
        /// Indicates whether the user is enabled.
        /// </summary>
        [DatabaseField("UserEnabled")]
        public virtual bool Enabled
        {
            get
            {
                return GetBooleanValue("UserEnabled", false);
            }
            set
            {
                SetValue("UserEnabled", value);
            }
        }


        /// <summary>
        /// Format of the stored user password.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For generating password use <see cref="UserInfoProvider.SetPassword(string, string)"/>.
        /// </para>
        /// <para>
        /// For password verification use <see cref="UserInfoProvider.IsUserPasswordDifferent(UserInfo, string)"/>.
        /// </para>
        /// </remarks>
        [DatabaseField]
        public string PasswordFormat
        {
            get
            {
                return GetStringValue("UserPasswordFormat", string.Empty);
            }
            set
            {
                SetValue("UserPasswordFormat", value);
            }
        }


        /// <summary>
        ///  Starting alias path of user.
        /// </summary>
        [DatabaseField]
        public virtual string UserStartingAliasPath
        {
            get
            {
                return GetStringValue("UserStartingAliasPath", string.Empty);
            }
            set
            {
                SetValue("UserStartingAliasPath", value);
            }
        }


        /// <summary>
        /// If user has allowed more than one culture.
        /// </summary>
        [DatabaseField]
        public virtual bool UserHasAllowedCultures
        {
            get
            {
                return GetBooleanValue("UserHasAllowedCultures", false);
            }
            set
            {
                SetValue("UserHasAllowedCultures", value);
            }
        }


        /// <summary>
        /// User GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid UserGUID
        {
            get
            {
                return GetGuidValue("UserGUID", Guid.Empty);
            }
            set
            {
                SetValue("UserGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserLastModified
        {
            get
            {
                return GetDateTimeValue("UserLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UserLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Contains XML with user's custom form field visibility settings.
        /// </summary>
        [DatabaseField]
        public virtual string UserVisibility
        {
            get
            {
                return GetStringValue("UserVisibility", string.Empty);
            }
            set
            {
                SetValue("UserVisibility", value);
            }
        }


        /// <summary>
        /// Determines whether user is domain user.
        /// </summary>
        [DatabaseField]
        public virtual bool UserIsDomain
        {
            get
            {
                return GetBooleanValue("UserIsDomain", false);
            }
            set
            {
                SetValue("UserIsDomain", value);
            }
        }


        /// <summary>
        /// Temporary GUID for user identification for automatic sign-in in the CMS Desk.
        /// </summary>
        [DatabaseField]
        public virtual Guid UserAuthenticationGUID
        {
            get
            {
                return GetGuidValue("UserAuthenticationGUID", Guid.Empty);
            }
            set
            {
                SetValue("UserAuthenticationGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Indicates privilege level of user.
        /// </summary>
        /// <remarks>Use <see cref="CheckPrivilegeLevel"/> for security check since this property is not suitable.</remarks>
        [DatabaseField("UserPrivilegeLevel", ValueType = typeof(int))]
        public UserPrivilegeLevelEnum SiteIndependentPrivilegeLevel
        {
            get
            {
                if (CMSActionContext.CurrentUseGlobalAdminContext)
                {
                    return UserPrivilegeLevelEnum.GlobalAdmin;
                }

                return (UserPrivilegeLevelEnum)GetValue("UserPrivilegeLevel", (int)UserPrivilegeLevelEnum.None);
            }
            set
            {
                SetValue("UserPrivilegeLevel", (int)value);
            }
        }

        #endregion


        #region "UserInfoSettings properties"

        /// <summary>
        /// User picture.
        /// </summary>
        public virtual string UserPicture
        {
            get
            {
                return UserSettings.UserPicture;
            }
            set
            {
                UserSettings.UserPicture = value;
            }
        }


        /// <summary>
        /// User avatar ID.
        /// </summary>
        public virtual int UserAvatarID
        {
            get
            {
                return UserSettings.UserAvatarID;
            }
            set
            {
                UserSettings.UserAvatarID = value;
            }
        }


        /// <summary>
        /// Messaging notification email.
        /// </summary>
        public virtual string UserMessagingNotificationEmail
        {
            get
            {
                return UserSettings.UserMessagingNotificationEmail;
            }
            set
            {
                UserSettings.UserMessagingNotificationEmail = value;
            }
        }


        /// <summary>
        /// User signature.
        /// </summary>
        public virtual string UserSignature
        {
            get
            {
                return UserSettings.UserSignature;
            }
            set
            {
                UserSettings.UserSignature = value;
            }
        }


        /// <summary>
        /// User description.
        /// </summary>
        public virtual string UserDescription
        {
            get
            {
                return UserSettings.UserDescription;
            }
            set
            {
                UserSettings.UserDescription = value;
            }
        }


        /// <summary>
        /// User nick name.
        /// </summary>
        public virtual string UserNickName
        {
            get
            {
                return UserSettings.UserNickName;
            }
            set
            {
                UserSettings.UserNickName = value;
            }
        }


        /// <summary>
        /// URL Referrer of user.
        /// </summary>
        public virtual string UserURLReferrer
        {
            get
            {
                return UserSettings.UserURLReferrer;
            }
            set
            {
                UserSettings.UserURLReferrer = value;
            }
        }


        /// <summary>
        /// User campaign.
        /// </summary>
        public virtual string UserCampaign
        {
            get
            {
                return UserSettings.UserCampaign;
            }
            set
            {
                UserSettings.UserCampaign = value;
            }
        }


        /// <summary>
        /// User time zone ID.
        /// </summary>
        public virtual int UserTimeZoneID
        {
            get
            {
                return UserSettings.UserTimeZoneID;
            }
            set
            {
                UserSettings.UserTimeZoneID = value;
            }
        }


        /// <summary>
        /// User password change request hash.
        /// </summary>
        public virtual string UserPasswordRequestHash
        {
            get
            {
                return UserSettings.UserPasswordRequestHash;
            }
            set
            {
                UserSettings.UserPasswordRequestHash = value;
            }
        }


        /// <summary>
        /// User custom data.
        /// </summary>
        public ContainerCustomData UserCustomData
        {
            get
            {
                return UserSettings.UserCustomData;
            }
        }


        /// <summary>
        /// Gets or sets number of user invalid logon attempts.
        /// </summary>
        public virtual int UserInvalidLogOnAttempts
        {
            get
            {
                return UserSettings.UserInvalidLogOnAttempts;
            }
            set
            {
                UserSettings.UserInvalidLogOnAttempts = value;
            }
        }


        /// <summary>
        /// User password change request hash.
        /// </summary>
        public virtual string UserInvalidLogOnAttemptsHash
        {
            get
            {
                return UserSettings.UserInvalidLogOnAttemptsHash;
            }
            set
            {
                UserSettings.UserInvalidLogOnAttemptsHash = value;
            }
        }


        /// <summary>
        /// Gets or sets time of last password change
        /// </summary>
        public virtual DateTime UserPasswordLastChanged
        {
            get
            {
                return UserSettings.UserPasswordLastChanged;
            }
            set
            {
                UserSettings.UserPasswordLastChanged = value;
            }
        }

        /// <summary>
        /// Gets or sets number representing reason for user account lock
        /// </summary>
        public virtual int UserAccountLockReason
        {
            get
            {
                return UserSettings.UserAccountLockReason;
            }
            set
            {
                UserSettings.UserAccountLockReason = value;
            }
        }

        #endregion


        #endregion


        #region "Properties"

        /// <summary>
        /// User salt custom column.
        /// </summary>
        private static string UserSaltColumn
        {
            get
            {
                if (mUserSaltColumn == null)
                {
                    // Set empty string if App settings key is missing.
                    mUserSaltColumn = SettingsHelper.AppSettings["CMSUserSaltColumn"] ?? String.Empty;
                }

                return mUserSaltColumn;
            }
        }


        /// <summary>
        /// User password salt.
        /// </summary>
        internal string UserSalt
        {
            get
            {
                if (string.IsNullOrEmpty(UserSaltColumn))
                {
                    return UserGUID.ToString();
                }
                return GetStringValue(UserSaltColumn, string.Empty);
            }
        }


        /// <summary>
        ///  User last logon info.
        /// </summary>
        public virtual UserDataInfo UserLastLogonInfo
        {
            get
            {
                if (mUserLastLogonInfo == null)
                {
                    // Load the xml data
                    mUserLastLogonInfo = new UserDataInfo();
                    mUserLastLogonInfo.LoadData(ValidationHelper.GetString(GetValue("UserLastLogonInfo"), ""));
                }
                return mUserLastLogonInfo;
            }
        }


        /// <summary>
        /// Gets the UserSettings info object for the user.
        /// </summary>
        public UserSettingsInfo UserSettings
        {
            get
            {
                return UserSettingsInternal;
            }
            set
            {
                UserSettingsInternal = value;
            }
        }


        /// <summary>
        /// Indicates whether user settings are loaded within current instance.
        /// </summary>
        /// <remarks>
        /// This property can be used only in <see cref="CurrentUserInfo"/> class.
        /// </remarks>
        internal bool UserSettingsLoaded
        {
            get
            {
                return (mUserSettings != null);
            }

        }


        /// <summary>
        /// Gets the UserSettings info object for the user.
        /// </summary>
        /// <remarks>
        /// This property can be used and overridden only in <see cref="CurrentUserInfo"/> class.
        /// </remarks>
        internal virtual UserSettingsInfo UserSettingsInternal
        {
            get
            {
                // Load the object if not loaded
                return mUserSettings ?? (mUserSettings = UserSettingsInfoProvider.GetUserSettingsInfoByUser(UserID) ?? new UserSettingsInfo());
            }
            set
            {
                mUserSettings = value;
            }
        }


        /// <summary>
        /// Gets the user's time zone information.
        /// </summary>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return mTimeZoneInfo ?? (mTimeZoneInfo = TimeZoneInfoProvider.GetTimeZoneInfo(UserSettings.UserTimeZoneID));
            }
        }


        /// <summary>
        /// Indicates if user was disabled manually by admin.
        /// </summary>
        public bool UserIsDisabledManually
        {
            get
            {
                return !Enabled && (UserAccountLockReason == UserAccountLockCode.FromEnum(UserAccountLockEnum.DisabledManually));
            }
        }


        /// <summary>
        /// Return number of days before user password expiration, null value means that password will never expire,
        /// positive values indicates number of days the password is expired and negative value indicates number of days before expiration
        /// </summary>
        public int? UserPasswordExpiration
        {
            get
            {
                int expDays;

                if (AuthenticationHelper.IsPasswordExpirationEnabled(SiteContext.CurrentSiteName, out expDays))
                {
                    int days = expDays - (UserInfoProvider.DateTimeNow - UserPasswordLastChanged).Days;
                    return ((days > 0) ? days : 0);
                }

                return expDays;
            }
        }


        /// <summary>
        /// Gets or sets the display name of the user. The display name is read from and written to the <see cref="FullName"/> property.
        /// Gets the formatted user name inferred from <see cref="UserName"/>, if user has no full name specified.
        /// </summary>
        /// <seealso cref="UserInfoProvider.GetFormattedUserName"/>
        protected override string ObjectDisplayName
        {
            get
            {
                if (!String.IsNullOrEmpty(FullName))
                {
                    return FullName;
                }

                return UserInfoProvider.GetFormattedUserName(UserName, null);
            }
            set
            {
                FullName = value;
            }
        }

        #endregion
        

        #region "Duplicate properties (for backward compatibility)"

        /// <summary>
        /// True if the user is external.
        /// </summary>
        /// <remarks>This property has the same meaning as UserIsExternal property.</remarks>
        public bool IsExternal
        {
            get
            {
                return GetBooleanValue("UserIsExternal", false);
            }
            set
            {
                SetValue("UserIsExternal", value);
            }
        }

        #endregion


        #region "Collections"

        /// <summary>
        /// Collection or users orders made on current site.
        /// </summary>
        public virtual IInfoObjectCollection Orders
        {
            get
            {
                if (mOrders == null)
                {
                    int siteId = SiteContext.CurrentSiteID;

                    // Create collection of customer orders from current site
                    var col = InfoObjectCollection.New(PredefinedObjectType.ORDER);

                    col.Where = new WhereCondition(string.Format("OrderCustomerID IN (SELECT CustomerID FROM COM_Customer WHERE CustomerUserID = {0}) AND OrderSiteID = {1}", UserID, siteId));
                    col.OrderByColumns = "OrderDate DESC";

                    mOrders = col;
                }

                return mOrders;
            }
        }


        /// <summary>
        /// Collection of all products purchased by user at all sites.
        /// </summary>
        public virtual IInfoObjectCollection PurchasedProducts
        {
            get
            {
                if (mPurchasedProducts == null)
                {
                    // Create collection of purchased products from all sites.
                    var col = InfoObjectCollection.New(PredefinedObjectType.SKU);

                    col.Where = new WhereCondition(string.Format("SKUID IN (SELECT OrderItemSKUID FROM COM_OrderItem WHERE OrderItemOrderID IN (SELECT OrderID FROM COM_Order WHERE OrderCustomerID IN (SELECT CustomerID FROM COM_Customer WHERE CustomerUserID = {0})))", UserID));
                    col.OrderByColumns = "SKUName";

                    mPurchasedProducts = col;
                }

                return mPurchasedProducts;
            }
        }


        /// <summary>
        /// Collection of all users wishlist items from all sites.
        /// </summary>
        public virtual IInfoObjectCollection Wishlist
        {
            get
            {
                if (mWishlist == null)
                {
                    // Create collection of wishlist items from all sites
                    var col = InfoObjectCollection.New(PredefinedObjectType.SKU);

                    col.Where = new WhereCondition(string.Format("SKUID IN (SELECT COM_Wishlist.SKUID FROM COM_Wishlist WHERE COM_Wishlist.UserID = {0})", UserID));
                    col.OrderByColumns = "SKUName";

                    mWishlist = col;
                }

                return mWishlist;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UserInfoProvider.DeleteUser(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UserInfoProvider.SetUserInfo(this);
        }


        /// <summary>
        /// Stores local settings for object instance.
        /// </summary>
        protected override void StoreSettings()
        {
            base.StoreSettings();
            UserSettings.Generalized.StoreSettings();
        }


        /// <summary>
        /// Restores local settings for object instance.
        /// </summary>
        protected override void RestoreSettings()
        {
            base.RestoreSettings();

            UserSettings.Generalized.RestoreSettings();
        }


        /// <summary>
        /// Invalidates the object in the object table.
        /// </summary>
        /// <param name="keepThisInstanceValid">If true, this object is marked as updated to behave as valid</param>
        protected override void Invalidate(bool keepThisInstanceValid)
        {
            base.Invalidate(keepThisInstanceValid);

            // Force reloading cached data (i.a. site and role collections) on next access
            ClearCachedData();
        }


        /// <summary>
        /// Gets the child object where condition.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="objectType">Object type of the child object</param>
        protected override WhereCondition GetChildWhereCondition(WhereCondition where, string objectType)
        {
            // Ensure base where condition
            where = where ?? new WhereCondition();

            // ## Special case - get only ad-hoc tasks
            switch (objectType)
            {
                case PredefinedObjectType.FRIEND:
                    // Include friends for both sides
                    return where.Or().WhereEquals("FriendUserID", UserID);
            }

            return base.GetChildWhereCondition(where, objectType);
        }


        /// <summary>
        /// Registers properties of the object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("IsMale", u => u.UserSettings.UserGender == 1);
            RegisterProperty("IsFemale", u => u.UserSettings.UserGender == 2);
            RegisterProperty("UserTime", u => TimeZoneHelper.GetUserDateTime(u));
            RegisterProperty("UserAge", u => Math.Round((UserInfoProvider.DateTimeNow - u.UserSettings.UserDateOfBirth).TotalDays / 365.25, 1));
            RegisterProperty("UserPasswordExpiration", u => u.UserPasswordExpiration);

            RegisterProperty("Orders", m => m.Orders);
            RegisterProperty("PurchasedProducts", m => m.PurchasedProducts);
            RegisterProperty("Wishlist", m => m.Wishlist);

            RegisterProperty("Enabled", u => u.Enabled);
        }


        /// <summary>
        /// Returns default user set in the settings or Global Administrator if setting is not defined or user not found.
        /// </summary>
        protected override BaseInfo GetDefaultObject()
        {
            return UserInfoProvider.AdministratorUser;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates a new UserInfo object from the given DataClass.
        /// </summary>
        /// <param name="ui">Source user info</param>
        /// <param name="keepSourceData">If true, source data are kept</param>
        /// <param name="useCachedData">Indicates if cached data like user memberships, roles, cultures, etc. should be used from source user object</param>
        public UserInfo(UserInfo ui, bool keepSourceData, bool useCachedData = true)
            : base(TYPEINFO, ui.DataClass, keepSourceData)
        {
            if (useCachedData)
            {
                ui.CopyCachedDataTo(this);
            }

            UserSettings = ui.mUserSettings;
        }


        /// <summary>
        /// Constructor - Creates an empty UserInfo object.
        /// </summary>
        public UserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">Data row with the user data</param>
        public UserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Sets the object default values
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            FullName = string.Empty;
            UserName = string.Empty;
            SetValue("UserPassword", string.Empty);
            Enabled = false;
            UserGUID = Guid.NewGuid();

            SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None;
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected UserInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
            userSecurityCollections = (UserSecurityCollections)info.GetValue("UserSecurityCollections", typeof(UserSecurityCollections));

            mSiteCultures = (SafeDictionary<string, SafeDictionary<string, int?>>)info.GetValue("SiteCultures", typeof(SafeDictionary<string, SafeDictionary<string, int?>>));
        }


        /// <summary>
        /// Object serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Context</param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("UserSecurityCollections", userSecurityCollections);

            info.AddValue("SiteCultures", mSiteCultures);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(base.GetColumnNames(), UserSettings.ColumnNames);
        }


        private static bool IsExcludedField(string fieldName)
        {
            return !ClassStructureInfo.GetClassInfo("CMS.User").ContainsColumn(fieldName);
        }


        /// <summary>
        /// Returns <c>true</c> if user fulfills the required privilege level (the higher level contains all children: GlobalAdmin -> Admin -> Editor -> None)
        /// </summary>
        /// <param name="privilegeLevel">Required privilege level</param>
        /// <param name="siteName">Site name for editor assignment. If not set current site name is used</param>
        public bool CheckPrivilegeLevel(UserPrivilegeLevelEnum privilegeLevel, string siteName = null)
        {
            UserPrivilegeLevelEnum userLevel = GetPrivilegeLevelForSite(siteName);
            bool result = IsSufficientPrivilegeLevel(privilegeLevel, userLevel);

            SecurityDebug.LogSecurityOperation(UserName, "CheckPrivilegeLevel", null, privilegeLevel.ToStringRepresentation(), result, siteName);

            return result;
        }


        /// <summary>
        /// Returns highest privilege level of user.
        /// </summary>
        /// <param name="siteName">Site name for editor assignment. If not set current site name is used</param>
        /// <remarks>Method is internal only for test purposes.</remarks>
        private UserPrivilegeLevelEnum GetPrivilegeLevelForSite(string siteName = null)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                siteName = SiteContext.CurrentSiteName;
            }

            var privilegeLevel = SiteIndependentPrivilegeLevel;
            if ((privilegeLevel == UserPrivilegeLevelEnum.Editor) && !IsInSite(siteName))
            {
                // User has no privileges for the site
                return UserPrivilegeLevelEnum.None;
            }
            return privilegeLevel;
        }


        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete UserSettings
            UserSettingsInfo usi = UserSettingsInfoProvider.GetUserSettingsInfoByUser(UserID);
            UserSettingsInfoProvider.DeleteUserSettingsInfo(usi);

            // Remove relations in main database
            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Returns true if the object contains given column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool ContainsColumn(string columnName)
        {
            if (!base.ContainsColumn(columnName))
            {
                return UserSettings.ContainsColumn(columnName);
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetValue(string columnName, out object value)
        {
            if (base.TryGetValue(columnName, out value))
            {
                return true;
            }

            return UserSettings.TryGetValue(columnName, out value);
        }


        /// <summary>
        /// Gets the type of the given property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected override Type GetPropertyType(string propertyName)
        {
            Type result = base.GetPropertyType(propertyName);

            if (result == null)
            {
                // Try to search in user settings
                result = UserSettings.TypeInfo.ClassStructureInfo.GetColumnType(propertyName);
            }

            return result;
        }


        /// <summary>
        /// Gets the object type for the given column or null if the object type is not found or unknown.
        /// </summary>
        /// <param name="columnName">Column name to check</param>
        protected override string GetObjectTypeForColumn(string columnName)
        {
            string result = base.GetObjectTypeForColumn(columnName);

            if (string.IsNullOrEmpty(result))
            {
                // Try user settings info if not found
                result = UserSettingsInfo.TYPEINFO.GetObjectTypeForColumn(columnName);
            }

            return result;
        }


        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetProperty(string columnName, out object value)
        {
            // Try to get the regular field value
            bool result = base.TryGetProperty(columnName, out value);

            // Automatically evaluated bindings
            if (!result)
            {
                // Search by property name + ID (such as SiteDefaultStylesheet[ID])
                columnName = columnName + "ID";
                if (ContainsColumn(columnName))
                {
                    string bindingType = UserSettings.TypeInfo.GetObjectTypeForColumn(columnName);
                    if (bindingType != null)
                    {
                        int bindingId = ValidationHelper.GetInteger(GetValue(columnName), 0);
                        if (bindingId > 0)
                        {
                            // Get the object
                            value = ProviderHelper.GetInfoById(bindingType, bindingId);
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Sets the field value (first look into CMS_User if the field is not found, set into CMS_UserSettings).
        /// </summary>
        /// <param name="columnName">Filed name</param>
        /// <param name="value">Value to be saved</param>
        public override bool SetValue(string columnName, object value)
        {
            bool result;

            // Try to set the value in this object
            if (!base.SetValue(columnName, value))
            {
                // Try to set the value in settings
                result = UserSettings.SetValue(columnName, value);
            }
            else
            {
                return true;
            }

            if (result)
            {
                // Change status to changed document in case the document wasn't deleted
                if (ObjectStatus != ObjectStatusEnum.WasDeleted)
                {
                    ObjectStatus = ObjectStatusEnum.Changed;
                }
            }

            return result;
        }


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        public override void MakeComplete(bool loadFromDb)
        {
            base.MakeComplete(loadFromDb);
            UserSettings.MakeComplete(loadFromDb);
        }


        /// <summary>
        /// Filters the user search results
        /// </summary>
        /// <param name="inRoles">List of roles in which the user should be</param>
        /// <param name="notInRoles">List of roles in which the user should not be</param>
        /// <param name="addToIndex">Flag if the results should be added to the index</param>
        public void FilterSearchResults(List<string> inRoles, List<string> notInRoles, ref bool addToIndex)
        {
            // Get user roles for current site
            DataTable dt = UserInfoProvider.GetUserRoles(this, "(SiteID IS NULL OR SiteID = " + SiteContext.CurrentSiteID + ")", null, 0, "RoleName, SiteID");

            // Check whether exists at least one role
            if (!DataHelper.DataSourceIsEmpty(dt))
            {
                // Loop thru all roles
                foreach (DataRow dr in dt.Rows)
                {
                    // Get role name
                    string role = ValidationHelper.GetString(dr["RoleName"], String.Empty).ToLowerInvariant();
                    // Ensure global role
                    if (ValidationHelper.GetInteger(dr["SiteID"], 0) == 0)
                    {
                        role = "." + role;
                    }

                    // Skip generic roles
                    if ((role == RoleName.AUTHENTICATED) || (role == RoleName.EVERYONE))
                    {
                        continue;
                    }

                    // If is in role => Add to index
                    if (inRoles.Contains(role))
                    {
                        addToIndex = true;
                    }

                    // If is in role which is unwanted => Do not add to the index
                    if (notInRoles.Contains(role))
                    {
                        addToIndex = false;
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if the user is public user record.
        /// </summary>
        public virtual bool IsPublic()
        {
            return String.Equals(UserName, "public", StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Returns current user date time in dependence on user time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime ConvertUserDateTime(DateTime dateTime)
        {
            return TimeZoneHelper.ConvertToUserDateTime(dateTime, this);
        }


        /// <summary>
        /// Returns server date time in dependence on server time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime ConvertServerDateTime(DateTime dateTime)
        {
            return TimeZoneHelper.ConvertToServerDateTime(dateTime, this);
        }


        /// <summary>
        /// Returns formatted username in format: full name (nickname) if nickname specified otherwise full name (username).
        /// Allows you to customize how the usernames will look like throughout the admin UI. 
        /// </summary>
        /// <param name="isLiveSite">Indicates if returned username should be displayed on live site</param>
        public string GetFormattedUserName(bool isLiveSite)
        {
            return UserInfoProvider.GetFormattedUserName(UserName, FullName, UserNickName, isLiveSite);
        }


        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        public override UserInfo Clone(bool clear)
        {
            UserInfo clone = base.Clone(clear);
            clone.UserSettings = UserSettings.Clone(clear);
            if (clear)
            {
                clone.UserSettings.UserSettingsID = 0;
            }
            return clone;
        }


        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        public override UserInfo Clone()
        {
            UserInfo clone = base.Clone();
            clone.UserSettings = UserSettings.Clone();
            return clone;
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Add CMS_UserSettings as excluded child (this will be cloned always).
            settings.ExcludedChildTypes.Add(UserSettingsInfo.OBJECT_TYPE);

            // Clear privilege level for admin users
            var privilege = GetPrivilegeLevelForSite();
            if ((privilege == UserPrivilegeLevelEnum.GlobalAdmin) || (privilege == UserPrivilegeLevelEnum.Admin))
            {
                SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None;
            }

            // Erase special IDs
            var us = UserSettings;

            us.UserLinkedInID = null;
            us.UserFacebookID = null;
            us.WindowsLiveID = null;

            bool generatePassword = true;
            bool permissions = true;
            bool personalization = true;
            string pass = null;

            var p = settings.CustomParameters;
            if (p != null)
            {
                string newEmail = ValidationHelper.GetString(p[PredefinedObjectType.USER + ".email"], "");
                Email = newEmail;

                generatePassword = ValidationHelper.GetBoolean(p[PredefinedObjectType.USER + ".generatepassword"], true);
                personalization = ValidationHelper.GetBoolean(p[PredefinedObjectType.USER + ".personalization"], true);
                permissions = ValidationHelper.GetBoolean(p[PredefinedObjectType.USER + ".permissions"], true);
                pass = ValidationHelper.GetString(p[PredefinedObjectType.USER + ".password"], "");
            }

            // Clone Avatar
            if (us.UserAvatarID > 0)
            {
                AvatarInfo avatar = AvatarInfoProvider.GetAvatarInfo(us.UserAvatarID);
                if (avatar != null)
                {
                    BaseInfo avatarClone = avatar.Generalized.InsertAsClone(settings, result);
                    us.UserAvatarID = avatarClone.Generalized.ObjectID;
                }
            }

            Insert();

            if (generatePassword)
            {
                var password = UserInfoProvider.GenerateNewPassword(null);
                UserInfoProvider.SetPassword(this, password, true);
                // User can gain access by reseting his password
                AuthenticationHelper.SendPasswordRequest(this, null, "CLONEUSER", SettingsKeyInfoProvider.GetValue("CMSSendPasswordEmailsFrom"), "Membership.ChangePasswordRequest", null, AuthenticationHelper.GetResetPasswordUrl(SiteContext.CurrentSiteName));
            }
            else
            {
                // Delete password if new password was not requested
                UserInfoProvider.SetPassword(this, pass, true);
            }

            if (personalization)
            {
                if (settings.ExcludedChildTypes.Contains(PredefinedObjectType.PERSONALIZATION))
                {
                    settings.ExcludedChildTypes.Remove(PredefinedObjectType.PERSONALIZATION);
                }
                if (settings.ExcludedChildTypes.Contains(PredefinedObjectType.DASHBOARD))
                {
                    settings.ExcludedChildTypes.Remove(PredefinedObjectType.DASHBOARD);
                }
            }
            else
            {
                settings.ExcludedChildTypes.Add(PredefinedObjectType.PERSONALIZATION);
                settings.ExcludedChildTypes.Add(PredefinedObjectType.DASHBOARD);
            }

            var permissionsList = new[] { UserRoleInfo.OBJECT_TYPE, PredefinedObjectType.BOARDMODERATOR, PredefinedObjectType.FORUMMODERATOR, MembershipUserInfo.OBJECT_TYPE, PredefinedObjectType.WORKFLOWSTEPUSER, PredefinedObjectType.WORKFLOWUSER };

            if (permissions)
            {
                foreach (var item in permissionsList)
                {
                    if (settings.ExcludedOtherBindingTypes.Contains(item))
                    {
                        settings.ExcludedOtherBindingTypes.Remove(item);
                    }
                }
            }
            else
            {
                settings.ExcludedOtherBindingTypes.AddRange(permissionsList);
            }

            // Reset OpenID
            string openId = OpenIDUserInfoProvider.GetOpenIDByUserID(originalObject.Generalized.ObjectID);
            if (!string.IsNullOrEmpty(openId))
            {
                OpenIDUserInfoProvider.RemoveOpenIDFromUser(openId, originalObject.Generalized.ObjectID);
            }
        }


        /// <summary>
        /// Updates user settings.
        /// </summary>
        /// <param name="settings">User settings</param>
        internal void UpdateUserSettings(UserSettingsInfo settings)
        {
            UserSettings = settings;
            UserInfoProvider.UpdateUserInHashtablesInternal(this);
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return UserInfoProvider.CheckLicense(GetPrivilegeLevelForSite(domainName), action, domainName);
        }


        /// <summary>
        /// Gets the where condition to filter out the default installation data
        /// </summary>
        /// <param name="recursive">Indicates whether where condition should contain further dependency conditions.</param>
        /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override string GetDefaultDataWhereCondition(bool recursive = true, bool globalOnly = true, IEnumerable<string> excludedNames = null)
        {
            // Global default data are only these 2 users
            string where = "UserName = 'administrator' OR UserName = 'public'";

            if (!globalOnly)
            {
                where = base.GetDefaultDataWhereCondition(recursive, false, excludedNames);
            }

            return where;
        }


        /// <summary>
        /// Returns list of column names which values were changed.
        /// </summary>
        /// <returns>List of column names</returns>
        public override List<string> ChangedColumns()
        {
            var changedColumns = new List<string>(base.ChangedColumns());
            changedColumns.AddRange(UserSettings.ChangedColumns());

            return changedColumns;
        }

        #endregion


        #region "Cached data methods"

        /// <summary>
        /// Copies the cached data to another UserInfo object
        /// </summary>
        /// <param name="userInfo">Target user info</param>
        protected void CopyCachedDataTo(UserInfo userInfo)
        {
            userInfo.userSecurityCollections = userSecurityCollections;

            userInfo.mSiteCultures = mSiteCultures;
            userInfo.mResourceUIElements = mResourceUIElements;

            userInfo.mTimeZoneInfo = mTimeZoneInfo;
            userInfo.mUserLastLogonInfo = mUserLastLogonInfo;

            userInfo.mWishlist = mWishlist;
            userInfo.mPurchasedProducts = mPurchasedProducts;
            userInfo.mOrders = mOrders;
        }


        /// <summary>
        /// Clears the cached data of this object
        /// </summary>
        protected void ClearCachedData()
        {
            userSecurityCollections = null;

            mSiteCultures = null;
            mResourceUIElements = null;
            mTimeZoneInfo = null;
            mUserLastLogonInfo = null;

            mWishlist = null;
            mPurchasedProducts = null;
            mOrders = null;
        }


        /// <summary>
        /// Creates a new empty dictionary for storing roles
        /// </summary>
        public SafeDictionary<string, int?> CreateNewRolesDictionary()
        {
            return new SafeDictionary<string, int?>();
        }


        /// <remarks>This method can be used in <see cref="CurrentUserInfo"/> only.</remarks>
        internal virtual bool SecurityCollectionsLoadedByInheritedClass(out UserSecurityCollections securityCollections)
        {
            securityCollections = null;
            return false;
        }


        /// <remarks>This method can be used in <see cref="CurrentUserInfo"/> only.</remarks>
        internal void CopySecurityCollections(UserInfo userInfo)
        {
            userInfo.userSecurityCollections = userSecurityCollections;
        }


        internal UserSecurityCollections EnsureSecurityCollections()
        {
            var securityCollections = userSecurityCollections;
            if (securityCollections == null)
            {
                lock (securityCollectionsLoadLock)
                {
                    securityCollections = userSecurityCollections;
                    if (securityCollections == null)
                    {
                        if (SecurityCollectionsLoadedByInheritedClass(out securityCollections))
                        {
                            return securityCollections;
                        }

                        securityCollections = UserSecurityCollections.GetSecurityCollectionsForUser(this);
                    }

                    userSecurityCollections = securityCollections;
                }
            }
            return securityCollections;
        }


        /// <summary>
        /// Returns true if the user is member of the specified role on the specified site (or on global role if check global is true).
        /// </summary>
        /// <param name="roleName">Role name to check</param>
        /// <param name="siteName">Site name to check</param>
        /// <param name="checkGlobal">If true, global roles are checked</param>
        /// <param name="checkMembership">If true, membership roles are checked</param>
        public virtual bool IsInRole(string roleName, string siteName, bool checkGlobal, bool checkMembership)
        {
            bool membershipRoles = false;
            var securityTables = EnsureSecurityCollections();

            bool roles = CheckRole(roleName, securityTables.SitesRoles, securityTables.RolesValidity, siteName, checkGlobal);
            if (checkMembership)
            {
                membershipRoles = CheckRole(roleName, securityTables.MembershipRoles, securityTables.MembershipRoleValidity, siteName, checkGlobal);
            }

            var result = (roles || membershipRoles);

            SecurityDebug.LogSecurityOperation(UserName, "IsInRole", roleName, null, result, siteName);

            return result;
        }


        /// <summary>
        /// Returns true if role name is found in collection.
        /// </summary>
        /// <param name="roleName">Name of wanted role</param>
        /// <param name="roleTable">Collection with roles to search in</param>
        /// <param name="roleValidityTable">Collection with roles validity</param>
        /// <param name="siteName">Site name</param>
        /// <param name="checkGlobal">If true, global roles are checked</param>
        private bool CheckRole(string roleName, SafeDictionary<string, SafeDictionary<string, int?>> roleTable, SafeDictionary<int, DateTime?> roleValidityTable, string siteName, bool checkGlobal)
        {
            if (String.IsNullOrEmpty(roleName))
            {
                return false;
            }

            roleName = roleName.ToLowerInvariant();
            switch (roleName)
            {
                case RoleName.EVERYONE:
                    // If role to check is EVERYONE, return true
                    return true;

                case RoleName.AUTHENTICATED:
                    return !IsPublic() && RequestContext.IsUserAuthenticated;

                case RoleName.NOTAUTHENTICATED:
                    return IsPublic() || !RequestContext.IsUserAuthenticated;

                default:
                    return IsRoleInCollection(roleName, roleTable, roleValidityTable, siteName, checkGlobal);
            }
        }


        private static bool IsRoleInCollection(string roleName, SafeDictionary<string, SafeDictionary<string, int?>> table, SafeDictionary<int, DateTime?> roleValidityTable, string siteName, bool checkGlobal)
        {
            bool siteRole = false;
            bool globalRole = false;
            int roleID = 0;

            // Get site roles
            SafeDictionary<string, int?> roleTable = null;
            if (!String.IsNullOrEmpty(siteName))
            {
                roleTable = table[siteName.ToLowerInvariant()];
            }

            // Get global roles
            var globalRoleTable = table[GLOBAL_ROLES_KEY];

            // Check site roles
            if ((roleTable != null) && !roleName.StartsWith(".", StringComparison.Ordinal))
            {
                if (roleTable.ContainsKey(roleName))
                {
                    siteRole = true;
                    roleID = ValidationHelper.GetInteger(roleTable[roleName], 0);
                }
            }

            // Check global role
            if (checkGlobal)
            {
                if (globalRoleTable != null)
                {
                    string globalRoleName = roleName.TrimStart('.');
                    if (globalRoleTable.ContainsKey(globalRoleName))
                    {
                        globalRole = true;
                        roleID = ValidationHelper.GetInteger(globalRoleTable[globalRoleName], 0);
                    }
                }
            }

            DateTime dt = ValidationHelper.GetDateTime(roleValidityTable[roleID], DateTimeHelper.ZERO_TIME);
            if ((dt != DateTimeHelper.ZERO_TIME) && (dt < UserInfoProvider.DateTimeNow))
            {
                // Check validity
                if (siteRole)
                {
                    siteRole = false;
                    roleTable.Remove(roleName);
                }

                if (globalRole)
                {
                    globalRole = false;
                    globalRoleTable.Remove(roleName);
                }
            }

            return (globalRole || siteRole);
        }


        /// <summary>
        /// Returns true if the user is a member of the role in context of the specified site.
        /// </summary>
        /// <remarks>The check also accounts for membership roles and global roles.</remarks>
        /// <param name="roleName">Role name to check</param>
        /// <param name="siteName">Site name to check</param>
        public virtual bool IsInRole(string roleName, string siteName)
        {
            return IsInRole(roleName, siteName, true, true);
        }


        /// <summary>
        /// Check whether current user is in membership
        /// </summary>
        /// <param name="membership">Membership name</param>
        /// <param name="siteName">Site name</param>
        public virtual bool IsInMembership(string membership, string siteName)
        {
            return IsInMembership(membership, siteName, true);
        }


        /// <summary>
        /// Check whether current user is in membership
        /// </summary>
        /// <param name="membershipName">Membership name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="checkGlobal">If true global memberships are checked</param>
        public virtual bool IsInMembership(string membershipName, string siteName, bool checkGlobal)
        {
            int membershipID = 0;
            bool siteMembership = false;
            bool globalMembership = false;
            membershipName = membershipName.ToLowerInvariant();

            var securityTables = EnsureSecurityCollections();

            // Get site memberships
            SafeDictionary<string, int?> membershipTable = null;
            if (!String.IsNullOrEmpty(siteName))
            {
                membershipTable = securityTables.Memberships[siteName.ToLowerInvariant()];
            }

            // Get global memberships
            var globalMembershipTable = securityTables.Memberships[GLOBAL_ROLES_KEY];

            // Check site memberships
            if (membershipTable != null)
            {
                if (membershipTable.ContainsKey(membershipName))
                {
                    siteMembership = true;
                    membershipID = ValidationHelper.GetInteger(membershipTable[membershipName], 0);
                }
            }

            // Check global membership
            if (checkGlobal)
            {
                if (globalMembershipTable != null)
                {
                    if (globalMembershipTable.ContainsKey(membershipName))
                    {
                        globalMembership = true;
                        membershipID = ValidationHelper.GetInteger(globalMembershipTable[membershipName], 0);
                    }
                }
            }

            DateTime dt = ValidationHelper.GetDateTime(securityTables.MembershipsValidity[membershipID], DateTimeHelper.ZERO_TIME);
            if ((dt != DateTimeHelper.ZERO_TIME) && (dt < UserInfoProvider.DateTimeNow))
            {
                // Check validity
                if (siteMembership)
                {
                    siteMembership = false;
                    membershipTable.Remove(membershipName);
                }

                if (globalMembership)
                {
                    globalMembership = false;
                    globalMembershipTable.Remove(membershipName);
                }
            }

            return (globalMembership || siteMembership);
        }


        /// <summary>
        /// Determines whether culture is allowed for given user on given site.
        /// </summary>
        /// <param name="cultureCode">Code of culture</param>
        /// <param name="siteName">Name of site</param>
        /// <returns>True if user can edit culture on site.</returns>
        public virtual bool IsCultureAllowed(string cultureCode, string siteName)
        {
            bool result = false;

            // User is global administrator or allowed cultures are not set
            if (CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || !UserHasAllowedCultures)
            {
                result = true;
            }
            else
            {
                var cultures = SiteCulturesInternal[siteName.ToLowerInvariant()];
                if (cultures != null)
                {
                    result = cultures.Contains(cultureCode.ToLowerInvariant());
                }
            }

            SecurityDebug.LogSecurityOperation(UserName, "IsCultureAllowed", cultureCode, null, result, siteName);

            return result;
        }


        /// <summary>
        /// Returns true, if the user is member of the specified site (registered on it).
        /// </summary>
        /// <param name="siteName">Sitename to check</param>
        public virtual bool IsInSite(string siteName)
        {
            var result = SitesRoles.ContainsKey(siteName.ToLowerInvariant());

            SecurityDebug.LogSecurityOperation(UserName, "IsInSite", null, null, result, siteName);

            return result;
        }


        /// <summary>
        /// Gets user generic roles.
        /// </summary>
        public virtual List<string> GetGenericRoles()
        {
            List<string> roles = new List<string>();

            // All users are in Everyone role
            roles.Add(RoleName.EVERYONE);

            // Public user is not authenticated
            roles.Add(IsPublic() ? RoleName.NOTAUTHENTICATED : RoleName.AUTHENTICATED);

            return roles;
        }


        /// <summary>
        /// Initializes the user site cultures collection by given data.
        /// </summary>
        /// <param name="userSites">Site data</param>
        /// <param name="userCultures">Culture data</param>
        /// <returns>Initialized collection</returns>
        private static SafeDictionary<string, SafeDictionary<string, int?>> CreateSiteCulturesTable(IEnumerable<SiteInfo> userSites, DataTable userCultures)
        {
            var siteCultures = new SafeDictionary<string, SafeDictionary<string, int?>>();

            // Store sites and cultures into the HashTable
            if ((userSites != null) && (userCultures != null))
            {
                foreach (var site in userSites)
                {
                    // Create the cultures table
                    var cultureTable = new SafeDictionary<string, int?>();
                    siteCultures[site.SiteName.ToLowerInvariant()] = cultureTable;

                    // Get the user cultures for the specified site
                    DataRow[] rows = userCultures.Select("SiteID = " + site.SiteID);

                    // Fill cultures table
                    foreach (DataRow culture in rows)
                    {
                        int cultureId = DataHelper.GetIntValue(culture, "CultureID");
                        string cultureCode = CultureInfoProvider.GetCultureInfo(cultureId).CultureCode;
                        cultureTable[cultureCode.ToLowerInvariant()] = cultureId;
                    }
                }
            }

            return siteCultures;
        }


        /// <summary>
        /// Gets all roles for current user.
        /// </summary>
        /// <param name="includeGlobal">If true global roles are included</param>
        /// <param name="includeMembership">If true membership roles are included</param>
        /// <param name="siteName">Current site name</param>
        public string GetRoleIdList(bool includeGlobal, bool includeMembership, string siteName)
        {
            // If no site set return empty string
            if (String.IsNullOrEmpty(siteName))
            {
                return String.Empty;
            }

            var securityCollections = EnsureSecurityCollections();
            string roles = GetRoleIdListInternal(securityCollections, siteName, includeGlobal);

            if (includeMembership)
            {
                roles += GetRoleIdListInternal(securityCollections, siteName, includeGlobal, true);
            }

            roles = roles.TrimEnd(',');
            return roles;
        }


        /// <summary>
        /// Returns roles for user included in given collection.
        /// </summary>
        /// <param name="securityCollections">Collection with user security collections </param>
        /// <param name="siteName">Site name</param>
        /// <param name="includeGlobal">If true global roles are included</param>
        /// <param name="membershipRoles">Indicates whether membership roles should be checked instead of site roles</param>
        private string GetRoleIdListInternal(UserSecurityCollections securityCollections, string siteName, bool includeGlobal, bool membershipRoles = false)
        {
            StringBuilder roles = new StringBuilder();

            var table = membershipRoles 
                ? securityCollections.MembershipRoles 
                : securityCollections.SitesRoles;

            var siteRoles = table[siteName.ToLowerInvariant()];
            if (siteRoles != null)
            {
                foreach (int roleId in siteRoles.Values)
                {
                    DateTime dt = ValidationHelper.GetDateTime(securityCollections.RolesValidity[roleId], DateTimeHelper.ZERO_TIME);
                    if ((dt == DateTimeHelper.ZERO_TIME) || (dt > UserInfoProvider.DateTimeNow))
                    {
                        roles.Append(roleId + ",");
                    }
                }
            }

            if (includeGlobal)
            {
                var globalRoles = table[GLOBAL_ROLES_KEY];
                if (globalRoles != null)
                {
                    foreach (int roleId in globalRoles.Values)
                    {
                        DateTime dt = ValidationHelper.GetDateTime(securityCollections.RolesValidity[roleId], DateTimeHelper.ZERO_TIME);
                        if ((dt == DateTimeHelper.ZERO_TIME) || (dt > UserInfoProvider.DateTimeNow))
                        {
                            roles.Append(roleId + ",");
                        }
                    }
                }
            }

            return roles.ToString();
        }


        /// <summary>
        /// Creates collection for UIElements.
        /// </summary>
        private SafeDictionary<string, SafeDictionary<string, DateTime?>> CreateSiteUIElementsTable()
        {
            var result = new SafeDictionary<string, SafeDictionary<string, DateTime?>>();

            int userId = UserID;
            if (userId > 0)
            {
                HashSet<int> roleIds = new HashSet<int>();
                var securityCollections = EnsureSecurityCollections();

                foreach (string siteName in securityCollections.SitesRoles.Keys)
                {
                    var siteRoles = securityCollections.SitesRoles[siteName];
                    if (siteRoles != null)
                    {
                        roleIds.AddRange(siteRoles.Values.OfType<int>());
                    }
                }

                IEnumerable<int> membershiRoleIds = securityCollections.MembershipRoles.TypedValues
                                                                   .Where(roles => roles != null)
                                                                   .SelectMany(roles => roles.Values.OfType<int>());

                // Get UI elements for specific roles
                DataSet userElements = UIElementInfoProvider.GetRolesUIElements(roleIds.Union(membershiRoleIds), "RoleID, ResourceName, ElementName, SiteName");

                if (!DataHelper.DataSourceIsEmpty(userElements))
                {
                    foreach (DataRow dr in userElements.Tables[0].Rows)
                    {
                        string resourceName = ValidationHelper.GetString(dr["ResourceName"], "");
                        string elemName = ValidationHelper.GetString(dr["ElementName"], "");
                        string siteName = ValidationHelper.GetString(dr["SiteName"], "").ToLowerInvariant();
                        int roleId = ValidationHelper.GetInteger(dr["RoleID"], 0);
                        DateTime validTo = ValidationHelper.GetDateTime(securityCollections.RolesValidity[roleId], DateTimeHelper.ZERO_TIME);

                        // Ensure role validity thru membership if available
                        if (securityCollections.MembershipRoleValidity[roleId] != null)
                        {
                            DateTime membershipRoleValidTo = ValidationHelper.GetDateTime(securityCollections.MembershipRoleValidity[roleId], DateTimeHelper.ZERO_TIME);
                            if (!roleIds.Contains(roleId))
                            {
                                validTo = membershipRoleValidTo;
                            }
                        }

                        // If role site ID IS NULL -> global role
                        if (String.IsNullOrEmpty(siteName))
                        {
                            siteName = GLOBAL_ROLES_KEY;
                        }

                        // Ensure resource hashtable
                        var resElemHash = result[siteName] ?? new SafeDictionary<string, DateTime?>();

                        string key = resourceName.ToLowerInvariant() + "." + elemName.ToLowerInvariant();
                        if (!resElemHash.Contains(key))
                        {
                            resElemHash.Add(key, validTo);
                        }
                        else
                        {
                            // If both validTo not NULL - compare and store the greater
                            DateTime dt = ValidationHelper.GetDateTime(resElemHash[key], DateTimeHelper.ZERO_TIME);
                            if ((dt != DateTimeHelper.ZERO_TIME) && (validTo != DateTimeHelper.ZERO_TIME) && (validTo > dt))
                            {
                                resElemHash[key] = validTo;
                            }

                            // If item has validTo set and current is NULL  - remove old item
                            if ((dt != DateTimeHelper.ZERO_TIME) && (validTo == DateTimeHelper.ZERO_TIME))
                            {
                                resElemHash[key] = DateTimeHelper.ZERO_TIME;
                            }
                        }
                        result[siteName] = resElemHash;
                    }
                }
            }

            return result;
        }

        #endregion


        #region "ISearchable Members"

        /// <summary>
        /// Gets the type of current object.
        /// </summary>
        public override string SearchType
        {
            get
            {
                return PredefinedObjectType.USER;
            }
        }


        /// <summary>
        /// Returns the collection with user's friends.
        /// </summary>
        public SafeDictionary<int, FriendshipStatusEnum> Friends
        {
            get
            {
                if (mFriends == null)
                {
                    var tempFriends = new SafeDictionary<int, FriendshipStatusEnum>();

                    DataSet friends = ModuleCommands.CommunityGetUserFriendshipRelations(UserID, null, 0, "FriendUserID, FriendRequestedUserID, FriendStatus");
                    if (!DataHelper.DataSourceIsEmpty(friends))
                    {
                        foreach (DataRow dr in friends.Tables[0].Rows)
                        {
                            // Load the friend
                            int requestedUserId = (int)dr["FriendRequestedUserID"];
                            int userId = (int)dr["FriendUserID"];

                            var status = (FriendshipStatusEnum)dr["FriendStatus"];

                            if (requestedUserId == UserID)
                            {
                                tempFriends[userId] = status;
                            }
                            else
                            {
                                tempFriends[requestedUserId] = status;
                            }
                        }
                    }

                    mFriends = tempFriends;
                }

                return mFriends;
            }
        }


        /// <summary>
        /// Returns the collection with user's groups.
        /// </summary>
        public virtual SafeDictionary<int, int> Groups
        {
            get
            {
                if (mGroups == null)
                {
                    var tempGroups = new SafeDictionary<int, int>();

                    DataSet groups = ModuleCommands.CommunityGetUserGroups(UserID, "MemberGroupID, MemberStatus");
                    if (!DataHelper.DataSourceIsEmpty(groups))
                    {
                        foreach (DataRow dr in groups.Tables[0].Rows)
                        {
                            int groupId = (int)dr["MemberGroupID"];
                            int status = ValidationHelper.GetInteger(dr["MemberStatus"], 0);
                            tempGroups[groupId] = status;

                            if (status == 0)
                            {
                                // Get group admin roles
                                var roles = RoleInfoProvider.GetRoles().Where("(RoleIsGroupAdministrator = 1) AND (RoleGroupID = " + groupId + ")").Columns("RoleName");

                                // For each role check if user is in
                                if (roles.Any(role => IsInRole(role.RoleName, SiteContext.CurrentSiteName)))
                                {
                                    tempGroups[groupId] = GROUP_ADMIN;
                                }
                            }
                        }
                    }

                    mGroups = tempGroups;
                }

                return mGroups;
            }
        }

        #endregion


        #region "Security methods"

        /// <summary>
        /// Returns true if given user is granted with specified permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteName">Site name</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName, bool exceptionOnFailure)
        {
            return UserSecurityHelper.IsAuthorizedPerResource(resourceName, permissionName, siteName, this, exceptionOnFailure);
        }


        /// <summary>
        /// Checks whether the user is authorized for given class name and permission, returns true if so.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public bool IsAuthorizedPerClassName(string className, string permissionName, string siteName, bool exceptionOnFailure = false)
        {
            return UserSecurityHelper.IsAuthorizedPerClassName(className, permissionName, siteName, this, exceptionOnFailure);
        }


        /// <summary>
        /// Checks whether the user is authorized for given resource name and permission, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name to check</param>
        public virtual bool IsAuthorizedPerResource(string resourceName, string permissionName)
        {
            return IsAuthorizedPerResource(resourceName, permissionName, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Checks whether the user is authorized for given resource name and permission, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permissionName">Permission name to check</param>
        /// <param name="siteName">Site name</param>
        public virtual bool IsAuthorizedPerResource(string resourceName, string permissionName, string siteName)
        {
            return IsAuthorizedPerResource(resourceName, permissionName, siteName, false);
        }


        /// <summary>
        /// Checks whether the user is authorized for given UI element of the specified resource, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource code name</param>
        /// <param name="elementName">UI element code name</param>
        /// <param name="siteAvailabilityRequired">Indicates if site availability of the corresponding resource (resource with name in format "cms.[ElementName]") is required. Takes effect only when corresponding resource exists</param>        
        public virtual bool IsAuthorizedPerUIElement(string resourceName, string elementName, bool siteAvailabilityRequired)
        {
            return IsAuthorizedPerUIElement(resourceName, elementName, siteAvailabilityRequired, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Checks whether the user is authorized for given UI element of the specified resource, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource code name</param>
        /// <param name="elementName">UI element code name</param>
        public virtual bool IsAuthorizedPerUIElement(string resourceName, string elementName)
        {
            return IsAuthorizedPerUIElement(resourceName, elementName, false, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Checks whether the user is authorized for given UI element of the specified resource, returns true if so.
        /// </summary>
        /// <param name="resourceID">Resource ID</param>
        /// <param name="elementName">UI element code name</param>
        /// <param name="siteAvailabilityRequired">Indicates if site availability of the corresponding resource (resource with name in format "cms.[ElementName]") is required. Takes effect only when corresponding resource exists</param>
        public virtual bool IsAuthorizedPerUIElement(int resourceID, string elementName, bool siteAvailabilityRequired = false)
        {
            String resourceName = "cms";

            // Get proper module name from element
            ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(resourceID);
            if (ri != null)
            {
                resourceName = ri.ResourceName;
            }

            return IsAuthorizedPerUIElement(resourceName, elementName, siteAvailabilityRequired, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Checks whether the user is authorized for given UI element of the specified resource, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource code name</param>
        /// <param name="elementName">UI element code name</param>
        /// <param name="siteName">Site code name</param>
        public virtual bool IsAuthorizedPerUIElement(string resourceName, string elementName, string siteName)
        {
            return IsAuthorizedPerUIElement(resourceName, elementName, false, siteName);
        }


        /// <summary>
        /// Checks whether the user is authorized for given UI element of the specified resource, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource code name</param>
        /// <param name="elementName">UI element code name</param>
        /// <param name="siteAvailabilityRequired">Indicates if site availability of the corresponding resource (resource with name in format "cms.[ElementName]") is required. Takes effect only when corresponding resource exists</param>        
        /// <param name="siteName">Site code name</param>
        public virtual bool IsAuthorizedPerUIElement(string resourceName, string elementName, bool siteAvailabilityRequired, string siteName)
        {
            return UserSecurityHelper.IsAuthorizedPerUIElement(resourceName, elementName, siteAvailabilityRequired, siteName, this);
        }


        /// <summary>
        /// Checks whether the user is authorized for given resource name and UIElements, returns true if so.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="elementNames">UIElement names to check</param>
        /// <param name="siteName">Site name</param>
        public virtual bool IsAuthorizedPerUIElement(string resourceName, string[] elementNames, string siteName)
        {
            return UserSecurityHelper.IsAuthorizedPerUIElement(resourceName, elementNames, siteName, this);
        }


        /// <summary>
        /// Checks whether the user is authorized for given class name and permission, returns true if so.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="permissionName">Permission name to check</param>
        public virtual bool IsAuthorizedPerClassName(string className, string permissionName)
        {
            return IsAuthorizedPerClassName(className, permissionName, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Checks whether the user is authorized per object with given object type and permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteName">Site name</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public virtual bool IsAuthorizedPerObject(PermissionsEnum permission, string objectType, string siteName, bool exceptionOnFailure = false)
        {
            return UserSecurityHelper.IsAuthorizedPerObject(permission, objectType, siteName, this, exceptionOnFailure);
        }


        /// <summary>
        /// Checks whether the user is authorized per object with given object type and permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="obj">Object to check</param>
        /// <param name="siteName">Site name</param>
        public virtual bool IsAuthorizedPerObject(PermissionsEnum permission, BaseInfo obj, string siteName)
        {
            return IsAuthorizedPerObject(permission, obj, siteName, false);
        }


        /// <summary>
        /// Checks whether the user is authorized per object with given object type and permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="obj">Object to check</param>
        /// <param name="siteName">Site name</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        public virtual bool IsAuthorizedPerObject(PermissionsEnum permission, BaseInfo obj, string siteName, bool exceptionOnFailure)
        {
            return UserSecurityHelper.IsAuthorizedPerObject(permission, obj, siteName, this, exceptionOnFailure);
        }


        /// <summary>
        /// Checks whether the user is authorized per meta file for given object type and permission.
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="objectType">Object type</param>
        /// <param name="siteName">Site name</param>
        public virtual bool IsAuthorizedPerMetaFile(PermissionsEnum permission, string objectType, string siteName)
        {
            return UserSecurityHelper.IsAuthorizedPerMetaFile(permission, objectType, siteName, this);
        }


        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        /// <exception cref="PermissionCheckException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                    return AuthorizeSetUserInfo(siteName, userInfo, true, exceptionOnFailure);

                case PermissionsEnum.Modify:
                    return AuthorizeSetUserInfo(siteName, userInfo, false, exceptionOnFailure);

                case PermissionsEnum.Delete:
                    return AuthorizeDeleteUserInfo(siteName, userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Performs authorization check for set info operation.
        /// </summary>
        /// <param name="currentSiteName">Current site name for permissions evaluation.</param>
        /// <param name="userInfo">Authorization of this user will be evaluated.</param>
        /// <param name="isCreate">Whether create or modify operation should be evaluated.</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> or <see cref="SecurityException"/> is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform set info operation on the this object; otherwise false</returns>
        /// <exception cref="SecurityException">Thrown when permission check fail and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        /// <exception cref="PermissionCheckException">Thrown when user is about to delete themselves and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        private bool AuthorizeSetUserInfo(string currentSiteName, IUserInfo userInfo, bool isCreate, bool exceptionOnFailure)
        {
            // Global administrator privilege level - can do anything
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return true;
            }

            var editedUserIsOrWasAdmin = CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || CheckOriginalPrivilegeLevel(UserPrivilegeLevelEnum.Admin);

            var selfEditing = ((ValidationHelper.GetInteger(GetOriginalValue("UserID"), 0) == userInfo.UserID) && (UserID == userInfo.UserID));

            // Administrator privilege level
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                // Trying to modify user with administrator or global administrator privilege
                if (editedUserIsOrWasAdmin)
                {
                    // Administrator can not edit other administrators nor global administrators - check against current and original UserID (to prevent violating permissions by changing ID)
                    if (!selfEditing)
                    {
                        return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) with administrator privilege level can not modify user '{2}' (ID {3}). Global administrator privilege level is required.",
                            userInfo.UserName, userInfo.UserID, UserName, UserID), exceptionOnFailure);
                    }

                    // Administrator can not grant themselves privilege level global administrator
                    if (CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
                    {
                        return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) with administrator privilege level can not set themselves global administrator privilege level.", userInfo.UserName, userInfo.UserID), exceptionOnFailure);
                    }
                }

                return true;
            }

            // Editor or none privilege level

            // Can not edit administrator or global administrator
            if (editedUserIsOrWasAdmin)
            {
                return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) can not modify user '{2}' (ID {3}). Global administrator privilege level is required.",
                    userInfo.UserName, userInfo.UserID, UserName, UserID), exceptionOnFailure);
            }

            // Editor or none can edit their own properties and no permission based check is necessary unless changing the privilege level - check against current and original UserID (to prevent violating permissions by changing ID)
            if (selfEditing && !ItemChanged("UserPrivilegeLevel"))
            {
                return true;
            }

            var userInfoObject = (UserInfo)userInfo;
            // Editor with Users.Modify permission can edit editor and none and can change privilege level between editor and none. None with Users.Modify can edit only none.
            // If the user is not an editor and changes an editor or creates a new editor, fail the authorization. Otherwise continue with permission based check.
            if (userInfoObject.SiteIndependentPrivilegeLevel != UserPrivilegeLevelEnum.Editor && (ItemChanged("UserPrivilegeLevel") || (SiteIndependentPrivilegeLevel == UserPrivilegeLevelEnum.Editor)))
            {
                return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) with none privilege level can not modify privilege level of user '{2}' (ID {3}). Higher privilege level is required.",
                    userInfo.UserName, userInfo.UserID, UserName, UserID), exceptionOnFailure);
            }

            // Usual permission check. Privilege escalation has been prevented and if the editor or none has Users.Modify, they can modify this user.
            var requiredPermission = base.GetPermissionToCheck(isCreate ? PermissionsEnum.Create : PermissionsEnum.Modify);

            return base.CheckPermissionsInternal(requiredPermission, currentSiteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Performs authorization check for delete info operation.
        /// </summary>
        /// <param name="currentSiteName">Current site name for permissions evaluation.</param>
        /// <param name="userInfo">Authorization of this user will be evaluated.</param>
        /// <param name="exceptionOnFailure">If true, <see cref="PermissionCheckException"/> or <see cref="SecurityException"/> is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform delete info operation on the this object; otherwise false</returns>
        /// <exception cref="SecurityException">Thrown when user does not have permissions for deleting and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        /// <exception cref="PermissionCheckException">Thrown when user is about to delete themselves and exception is allowed by <paramref name="exceptionOnFailure"/> parameter.</exception>
        private bool AuthorizeDeleteUserInfo(string currentSiteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            // User can delete themselves only if they are None
            var userInfoObject = (UserInfo)userInfo;
            if (userInfo.UserID == UserID && userInfoObject.SiteIndependentPrivilegeLevel >= UserPrivilegeLevelEnum.Editor)
            {
                return false;
            }

            // Global administrator privilege level - can do anything
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.GlobalAdmin))
            {
                return true;
            }

            var editedUserIsOrWasAdmin = CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || CheckOriginalPrivilegeLevel(UserPrivilegeLevelEnum.Admin);

            // Administrator privilege level
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                // Trying to delete user with administrator or global administrator privilege
                if (editedUserIsOrWasAdmin)
                {
                    // Administrator can not delete other administrators nor global administrators - check against current UserID, not original (admin can take own user info, change ID and delete)
                    return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) with administrator privilege level can not delete user '{2}' (ID {3}). Global administrator privilege level is required.",
                        userInfo.UserName, userInfo.UserID, UserName, UserID), exceptionOnFailure);
                }

                return true;
            }

            // Editor or none privilege level

            // Can not delete administrator or global administrator
            if (editedUserIsOrWasAdmin)
            {
                return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) can not delete user '{2}' (ID {3}). Global administrator privilege level is required.",
                    userInfo.UserName, userInfo.UserID, UserName, UserID), exceptionOnFailure);
            }

            // Editor with Users.Modify permission can delete editor and none. None with Users.Modify can delete only none.
            // If the user is not an editor and tries to delete an editor, fail the authorization. Otherwise continue with permission based check.         
            if (userInfoObject.SiteIndependentPrivilegeLevel != UserPrivilegeLevelEnum.Editor
                && (ItemChanged("UserPrivilegeLevel") || SiteIndependentPrivilegeLevel == UserPrivilegeLevelEnum.Editor))
            {
                return SecurityCheckFailure(String.Format("User '{0}' (ID {1}) with none privilege level can not delete of user '{2}' (ID {3}). Higher privilege level is required.",
                    userInfo.UserName, userInfo.UserID, UserName, UserID), exceptionOnFailure);
            }

            // Usual permission check.
            var requiredPermission = base.GetPermissionToCheck(PermissionsEnum.Delete);

            return base.CheckPermissionsInternal(requiredPermission, currentSiteName, userInfo, exceptionOnFailure);
        }


        /// <summary>
        /// Handles security check failure - either throws an exception, or returns false.
        /// </summary>
        /// <param name="failureMessage">Reason for the failure.</param>
        /// <param name="exceptionOnFailure">Whether to throw exception, or just return false.</param>
        /// <returns>If this method returns, it returns false.</returns>
        /// <exception cref="SecurityException">Thrown when <paramref name="exceptionOnFailure"/> is true.</exception>
        /// <remarks>
        /// A <see cref="SecurityException"/> is thrown since the failure is not strictly permission related
        /// (e.g. insufficient privilege level).
        /// </remarks>
        private bool SecurityCheckFailure(string failureMessage, bool exceptionOnFailure)
        {
            if (exceptionOnFailure)
            {
                throw new SecurityException(failureMessage);
            }

            return false;
        }


        /// <summary>
        /// Validates whether <paramref name="actual"/> has at least <see paramref="required"/> privilege level.
        /// (Order of privilege levels from the highest is: GlobalAdmin -> Admin -> Editor -> None)
        /// </summary>
        /// <param name="required">Required privilege level</param>
        /// <param name="actual">Privilege level to validate</param>
        private static bool IsSufficientPrivilegeLevel(UserPrivilegeLevelEnum required, UserPrivilegeLevelEnum actual)
        {
            return actual >= required;
        }


        /// <summary>
        /// Returns <c>true</c> if user originally fulfilled the required privilege level.
        /// (Order of privilege levels from the highest is: GlobalAdmin -> Admin -> Editor -> None) 
        /// </summary>
        /// <param name="privilegeLevel">Required privilege level</param>
        private bool CheckOriginalPrivilegeLevel(UserPrivilegeLevelEnum privilegeLevel)
        {
            var originalPrivilegeLevel = (UserPrivilegeLevelEnum)ValidationHelper.GetInteger(GetOriginalValue("UserPrivilegeLevel"), 0);
            return IsSufficientPrivilegeLevel(privilegeLevel, originalPrivilegeLevel);
        }

        #endregion


        #region "Friend methods"

        /// <summary>
        /// Update user friends collection.
        /// </summary>
        /// <param name="userId">User id of the friend</param>
        /// <param name="status">Friendship status</param>
        public void UpdateFriendStatus(int userId, FriendshipStatusEnum status)
        {
            switch (status)
            {
                case FriendshipStatusEnum.Waiting:
                case FriendshipStatusEnum.Rejected:
                case FriendshipStatusEnum.Approved:
                    // Remove key if exists
                    if (Friends.ContainsKey(userId))
                    {
                        Friends.Remove(userId);
                    }
                    Friends.Add(userId, status);
                    break;

                case FriendshipStatusEnum.None:
                    Friends.Remove(userId);
                    break;
            }
        }


        /// <summary>
        /// Returns friendship status for given user.
        /// </summary>
        /// <param name="userId">ID of user to check</param>
        public virtual FriendshipStatusEnum HasFriend(int userId)
        {
            if (Friends != null)
            {
                if (Friends.ContainsKey(userId))
                {
                    return Friends[userId];
                }
            }

            return FriendshipStatusEnum.None;
        }

        #endregion


        #region "Group methods"

        /// <summary>
        /// Returns true if the user is member of the specified group.
        /// </summary>
        /// <param name="groupId">ID of the group check</param>
        public virtual bool IsGroupMember(int groupId)
        {
            bool result = false;

            // Do not check if no group is specified
            if (groupId > 0)
            {
                // Global administrator and community admin are group members by default
                result = CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || IsAuthorizedPerResource("CMS.Groups", "Manage");

                // Is group member? 
                if (!result && (Groups != null))
                {
                    result = Groups.ContainsKey(groupId);
                }
            }

            SecurityDebug.LogSecurityOperation(UserName, "IsGroupMember", groupId.ToString(), null, result, null);

            return result;
        }


        /// <summary>
        /// Returns true if the user is member of the specified group.
        /// </summary>
        /// <param name="groupName">Group name to test</param>
        /// <param name="siteName">Site name</param>
        public virtual bool IsGroupMember(string groupName, string siteName)
        {
            // Test if this group name was already asked for this user
            if (mGroupsMember.ContainsKey(groupName))
            {
                return ValidationHelper.GetBoolean(mGroupsMember[groupName], false);
            }

            bool result = false;

            // If not get group ID from DB
            BaseInfo info = ModuleCommands.CommunityGetGroupInfoByName(groupName, siteName);
            if (info != null)
            {
                // If group was found
                int groupID = ValidationHelper.GetInteger(info.GetValue("GroupID"), 0);

                // Get the result from hash table
                result = IsGroupMember(groupID);
            }

            // Add result to hashtable with results to prevent repetitive DB asks
            mGroupsMember[groupName] = result;

            return result;
        }


        /// <summary>
        /// Returns true if the user is administrator of the specified group.
        /// </summary>
        /// <param name="groupId">ID of the group check</param>
        public virtual bool IsGroupAdministrator(int groupId)
        {
            bool result = false;

            // Do not check if no group is specified
            if (groupId > 0)
            {
                // Global administrator and community admin are group administrators by default
                if (CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || IsAuthorizedPerResource("CMS.Groups", "Manage"))
                {
                    result = true;
                }
                else if (Groups != null)
                {
                    // Check whether member for current group has group admin status
                    result = (ValidationHelper.GetInteger(Groups[groupId], 0) == GROUP_ADMIN);
                }
            }

            SecurityDebug.LogSecurityOperation(UserName, "IsGroupAdministrator", groupId.ToString(), null, result, null);

            return result;
        }

        #endregion
    }
}