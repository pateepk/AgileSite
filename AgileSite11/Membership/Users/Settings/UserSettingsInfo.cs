using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.MacroEngine;
using CMS.Membership;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

[assembly: RegisterObjectType(typeof(UserSettingsInfo), UserSettingsInfo.OBJECT_TYPE)]

namespace CMS.Membership
{
    /// <summary>
    /// UserSettingsInfo data container class.
    /// </summary>
    public class UserSettingsInfo : AbstractInfo<UserSettingsInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.USERSETTINGS;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UserSettingsInfoProvider), OBJECT_TYPE, "CMS.UserSettings", "UserSettingsID", null, "UserSettingsUserGUID", null, null, null, null, "UserSettingsUserID", UserInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            { 
                new ObjectDependency("UserActivatedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), 
                new ObjectDependency("UserTimeZoneID", TimeZoneInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), 
                new ObjectDependency("UserAvatarID", AvatarInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired), 
                new ObjectDependency("UserBadgeID", BadgeInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired)
            },
            ModuleName = "cms.users",
            LogIntegration = false,
            ImportExportSettings =
            {
                LogExport = false,
                IncludeToExportParentDataSet = IncludeToParentEnum.Incremental
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Incremental
            },
            SupportsVersioning = false,
            TouchCacheDependencies = true,
            LogEvents = false,
            DefaultData = new DefaultDataSettings
            {
                ExcludedColumns = new List<string> { "UserCustomData", "UserAuthenticationGUID", "UserBlogPosts", "UserURLReferrer", "UserPasswordRequestHash", "UserBadgeID", "UserPasswordLastChanged", "UserDateOfBirth", "UserDialogsConfiguration", "UserPicture", "UserUsedWebParts", "UserMessageBoardPosts", "UserInvalidLogOnAttemptsHash", "UserTimeZoneID", "UserWaitingForApproval", "UserPosition", "UserCampaign", "UserBlogComments", "UserAvatarType", "UserGender", "UserSkype", "UserDescription", "UserActivatedByUserID", "UserRegistrationInfo", "UserActivationDate", "UserPreferences", "WindowsLiveID", "UserInvalidLogOnAttempts", "UserIM", "UserPhone", "UserLinkedInID", "UserAccountLockReason", "UserShowIntroductionTile", "UserDashboardApplications", "UserActivityPoints", "UserForumPosts", "UserUsedWidgets", "UserDismissedSmartTips" }
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "UserForumPosts",
                    "UserMessageBoardPosts",
                    "UserBlogComments",
                    "UserBlogPosts",
                    "UserActivityPoints"
                },
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("UserDialogsConfiguration"),
                    new StructuredField("UserRegistrationInfo")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Maximum number of recently used web parts.
        /// </summary>
        private const int MAX_RECENTLY_USED_WEBPARTS = 15;


        /// <summary>
        /// Maximum number of recently used widgets.
        /// </summary>
        private const int MAX_RECENTLY_USED_WIDGETS = 15;


        /// <summary>
        /// User custom data.
        /// </summary>
        protected ContainerCustomData mUserCustomData = null;


        /// <summary>
        /// User custom data.
        /// </summary>
        protected ContainerCustomData mUserPreferences = null;


        /// <summary>
        /// Contains information of user registration.
        /// </summary>
        private UserDataInfo mUserRegistrationInfo;


        /// <summary>
        /// Contains information about user dialogs settings.
        /// </summary>
        private XmlData mUserDialogsConfiguration;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the XmlData object with information about user dialogs settings.
        /// </summary>
        [DatabaseField]
        public virtual XmlData UserDialogsConfiguration
        {
            get
            {
                if (mUserDialogsConfiguration == null)
                {
                    // Load the settings data
                    mUserDialogsConfiguration = new XmlData();
                    mUserDialogsConfiguration.LoadData(ValidationHelper.GetString(GetValue("UserDialogsConfiguration"), null));
                }
                return mUserDialogsConfiguration;
            }
        }


        /// <summary>
        /// User guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid UserSettingsUserGUID
        {
            get
            {
                return GetGuidValue("UserSettingsUserGUID", Guid.Empty);
            }
            set
            {
                SetValue("UserSettingsUserGUID", value);
            }
        }


        /// <summary>
        /// User settings ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserSettingsID
        {
            get
            {
                return GetIntegerValue("UserSettingsID", 0);
            }
            set
            {
                SetValue("UserSettingsID", value);
            }
        }


        /// <summary>
        /// User ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserSettingsUserID
        {
            get
            {
                return GetIntegerValue("UserSettingsUserID", 0);
            }
            set
            {
                SetValue("UserSettingsUserID", value);
            }
        }


        /// <summary>
        /// User campaign.
        /// </summary>
        [DatabaseField]
        public virtual string UserCampaign
        {
            get
            {
                return GetStringValue("UserCampaign", "");
            }
            set
            {
                SetValue("UserCampaign", value);
            }
        }


        /// <summary>
        /// Indicates whether user is waiting for approval.
        /// </summary>
        [DatabaseField]
        public virtual bool UserWaitingForApproval
        {
            get
            {
                return GetBooleanValue("UserWaitingForApproval", false);
            }
            set
            {
                SetValue("UserWaitingForApproval", value);
            }
        }


        /// <summary>
        /// Indicates whether logging activities is enabled for this user.
        /// </summary>
        [DatabaseField]
        public virtual bool UserLogActivities
        {
            get
            {
                return GetBooleanValue("UserLogActivities", false);
            }
            set
            {
                SetValue("UserLogActivities", value);
            }
        }


        /// <summary>
        /// Date of user's activation.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserActivationDate
        {
            get
            {
                return GetDateTimeValue("UserActivationDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value != DateTimeHelper.ZERO_TIME)
                {
                    SetValue("UserActivationDate", value);
                }
                else
                {
                    SetValue("UserActivationDate", null);
                }
            }
        }


        /// <summary>
        /// User signature.
        /// </summary>
        [DatabaseField]
        public virtual string UserSignature
        {
            get
            {
                return GetStringValue("UserSignature", "");
            }
            set
            {
                SetValue("UserSignature", value);
            }
        }


        /// <summary>
        /// User picture.
        /// </summary>
        [DatabaseField]
        public virtual string UserPicture
        {
            get
            {
                return GetStringValue("UserPicture", "");
            }
            set
            {
                SetValue("UserPicture", value);
            }
        }


        /// <summary>
        /// User avatar ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserAvatarID
        {
            get
            {
                return GetIntegerValue("UserAvatarID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("UserAvatarID", value);
                }
                else
                {
                    SetValue("UserAvatarID", null);
                }
            }
        }


        /// <summary>
        /// User avatar type.
        /// </summary>
        [DatabaseField]
        public virtual string UserAvatarType
        {
            get
            {
                return GetStringValue("UserAvatarType", AvatarInfoProvider.AVATAR);
            }
            set
            {
                SetValue("UserAvatarType", value);
            }
        }


        /// <summary>
        /// User nick name.
        /// </summary>
        [DatabaseField]
        public virtual string UserNickName
        {
            get
            {
                return GetStringValue("UserNickName", "");
            }
            set
            {
                SetValue("UserNickName", value);
            }
        }


        /// <summary>
        /// User Activated by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserActivatedByUserID
        {
            get
            {
                return GetIntegerValue("UserActivatedByUserID", 0);
            }
            set
            {
                SetValue("UserActivatedByUserID", value, 0);
            }
        }


        /// <summary>
        /// Messaging notification email address.
        /// </summary>
        [DatabaseField]
        public virtual string UserMessagingNotificationEmail
        {
            get
            {
                return GetStringValue("UserMessagingNotificationEmail", "");
            }
            set
            {
                SetValue("UserMessagingNotificationEmail", value);
            }
        }


        /// <summary>
        /// User custom data.
        /// </summary>
        [DatabaseField]
        public ContainerCustomData UserCustomData
        {
            get
            {
                return mUserCustomData ?? (mUserCustomData = new ContainerCustomData(this, "UserCustomData"));
            }
        }


        /// <summary>
        /// User registration info.
        /// </summary>
        [DatabaseField]
        public virtual UserDataInfo UserRegistrationInfo
        {
            get
            {
                if (mUserRegistrationInfo == null)
                {
                    // Load the xml data
                    mUserRegistrationInfo = new UserDataInfo();
                    mUserRegistrationInfo.LoadData(ValidationHelper.GetString(GetValue("UserRegistrationInfo"), ""));
                }
                return mUserRegistrationInfo;
            }
        }


        /// <summary>
        /// User preferences.
        /// </summary>
        [DatabaseField]
        public ContainerCustomData UserPreferences
        {
            get
            {
                return mUserPreferences ?? (mUserPreferences = new ContainerCustomData(this, "UserPreferences"));
            }
        }


        /// <summary>
        /// User time zone ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserTimeZoneID
        {
            get
            {
                return GetIntegerValue("UserTimeZoneID", 0);
            }
            set
            {
                SetValue("UserTimeZoneID", value, 0);
            }
        }


        /// <summary>
        /// URL Referrer of user.
        /// </summary>
        [DatabaseField]
        public virtual string UserURLReferrer
        {
            get
            {
                return GetStringValue("UserURLReferrer", "");
            }
            set
            {
                SetValue("UserURLReferrer", value);
            }
        }


        /// <summary>
        /// User badge ID.
        /// </summary>
        [DatabaseField]
        public virtual int UserBadgeID
        {
            get
            {
                return GetIntegerValue("UserBadgeID", 0);
            }
            set
            {
                SetValue("UserBadgeID", value, 0);
            }
        }


        /// <summary>
        /// Indicates whether the introduction tile should be displayed in dashboard.
        /// </summary>
        [DatabaseField]
        public bool UserShowIntroductionTile
        {
            get
            {
                return GetBooleanValue("UserShowIntroductionTile", true);
            }
            set
            {
                SetValue("UserShowIntroductionTile", value);
            }
        }
        

        /// <summary>
        /// User activity points.
        /// </summary>
        [DatabaseField]
        public virtual int UserActivityPoints
        {
            get
            {
                return GetIntegerValue("UserActivityPoints", 0);
            }
            set
            {
                SetValue("UserActivityPoints", value);
            }
        }


        /// <summary>
        /// User forum posts.
        /// </summary>
        [DatabaseField]
        public virtual int UserForumPosts
        {
            get
            {
                return GetIntegerValue("UserForumPosts", 0);
            }
            set
            {
                SetValue("UserForumPosts", value);
            }
        }


        /// <summary>
        /// User blog comments.
        /// </summary>
        [DatabaseField]
        public virtual int UserBlogComments
        {
            get
            {
                return GetIntegerValue("UserBlogComments", 0);
            }
            set
            {
                SetValue("UserBlogComments", value);
            }
        }


        /// <summary>
        /// User message board posts.
        /// </summary>
        [DatabaseField]
        public virtual int UserMessageBoardPosts
        {
            get
            {
                return GetIntegerValue("UserMessageBoardPosts", 0);
            }
            set
            {
                SetValue("UserMessageBoardPosts", value);
            }
        }


        /// <summary>
        /// User gender.
        /// </summary>
        [DatabaseField]
        public virtual int UserGender
        {
            get
            {
                return GetIntegerValue("UserGender", 0);
            }
            set
            {
                SetValue("UserGender", value);
            }
        }


        /// <summary>
        /// User date of birth.
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserDateOfBirth
        {
            get
            {
                return GetDateTimeValue("UserDateOfBirth", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("UserDateOfBirth", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Windows LiveID user identification token.
        /// </summary>
        [DatabaseField]
        public virtual string WindowsLiveID
        {
            get
            {
                return GetStringValue("WindowsLiveID", "");
            }
            set
            {
                SetValue("WindowsLiveID", value);
            }
        }


        /// <summary>
        /// Facebook user identification token.
        /// </summary>
        [DatabaseField]
        public virtual string UserFacebookID
        {
            get
            {
                return GetStringValue("UserFacebookID", "");
            }
            set
            {
                SetValue("UserFacebookID", value);
            }
        }


        /// <summary>
        /// LinkedIn user id.
        /// </summary>
        [DatabaseField]
        public virtual string UserLinkedInID
        {
            get
            {
                return GetStringValue("UserLinkedInID", "");
            }
            set
            {
                SetValue("UserLinkedInID", value);
            }
        }


        /// <summary>
        /// User position.
        /// </summary>
        [DatabaseField]
        public virtual string UserPosition
        {
            get
            {
                return GetStringValue("UserPosition", "");
            }
            set
            {
                SetValue("UserPosition", value);
            }
        }


        /// <summary>
        /// Skype account.
        /// </summary>
        [DatabaseField]
        public virtual string UserSkype
        {
            get
            {
                return GetStringValue("UserSkype", "");
            }
            set
            {
                SetValue("UserSkype", value);
            }
        }


        /// <summary>
        /// Phone number.
        /// </summary>
        [DatabaseField]
        public virtual string UserPhone
        {
            get
            {
                return GetStringValue("UserPhone", "");
            }
            set
            {
                SetValue("UserPhone", value);
            }
        }


        /// <summary>
        /// IM account.
        /// </summary>
        [DatabaseField]
        public virtual string UserIM
        {
            get
            {
                return GetStringValue("UserIM", "");
            }
            set
            {
                SetValue("UserIM", value);
            }
        }


        /// <summary>
        /// Total count of all user posts.
        /// </summary>
        [DatabaseField]
        public virtual int UserBlogPosts
        {
            get
            {
                return GetIntegerValue("UserBlogPosts", 0);
            }
            set
            {
                SetValue("UserBlogPosts", value);
            }
        }


        /// <summary>
        /// Description of user.
        /// </summary>
        [DatabaseField]
        public virtual string UserDescription
        {
            get
            {
                return GetStringValue("UserDescription", string.Empty);
            }
            set
            {
                SetValue("UserDescription", value);
            }
        }


        /// <summary>
        /// Sets or gets user used web parts.
        /// </summary>
        [DatabaseField]
        public virtual string UserUsedWebParts
        {
            get
            {
                return GetStringValue("UserUsedWebParts", string.Empty);
            }
            set
            {
                SetValue("UserUsedWebParts", value);
            }
        }


        /// <summary>
        /// Sets or gets user used widgets.
        /// </summary>
        [DatabaseField]
        public virtual string UserUsedWidgets
        {
            get
            {
                return GetStringValue("UserUsedWidgets", string.Empty);
            }
            set
            {
                SetValue("UserUsedWidgets", value);
            }
        }


        /// <summary>
        /// Gets or sets user password change request hash.
        /// </summary>
        [DatabaseField]
        public virtual string UserPasswordRequestHash
        {
            get
            {
                return GetStringValue("UserPasswordRequestHash", string.Empty);
            }
            set
            {
                SetValue("UserPasswordRequestHash", value);
            }
        }


        /// <summary>
        /// Gets or sets number of user invalid logon attempts
        /// </summary>
        [DatabaseField]
        public virtual int UserInvalidLogOnAttempts
        {
            get
            {
                return GetIntegerValue("UserInvalidLogOnAttempts", 0);
            }
            set
            {
                SetValue("UserInvalidLogOnAttempts", value);
            }
        }


        /// <summary>
        /// Gets or sets user invalid logon attempts hash.
        /// </summary>
        [DatabaseField]
        public virtual string UserInvalidLogOnAttemptsHash
        {
            get
            {
                return GetStringValue("UserInvalidLogOnAttemptsHash", string.Empty);
            }
            set
            {
                SetValue("UserInvalidLogOnAttemptsHash", value);
            }
        }


        /// <summary>
        /// Gets or sets time of last password change
        /// </summary>
        [DatabaseField]
        public virtual DateTime UserPasswordLastChanged
        {
            get
            {
                return GetDateTimeValue("UserPasswordLastChanged", DateTime.MinValue);
            }
            set
            {
                SetValue("UserPasswordLastChanged", value);
            }
        }


        /// <summary>
        /// Gets or sets number representing reason for user account lock
        /// </summary>
        [DatabaseField]
        public virtual int UserAccountLockReason
        {
            get
            {
                return GetIntegerValue("UserAccountLockReason", 0);
            }
            set
            {
                SetValue("UserAccountLockReason", value);
            }
        }


        /// <summary>
        /// Gets or sets list of serialized applications guids saved for the current user. 
        /// </summary>
        [DatabaseField]
        public virtual string UserDashboardApplications
        {
            get
            {
                return GetStringValue("UserDashboardApplications", string.Empty);
            }
            set
            {
                SetValue("UserDashboardApplications", value);
            }

        }


        /// <summary>
        /// Gets or sets list of serialized identifiers of smart tips that wont be shown to the user.
        /// </summary>
        [DatabaseField]
        public virtual string UserDismissedSmartTips
        {
            get
            {
                return GetStringValue("UserDismissedSmartTips", string.Empty);
            }
            set
            {
                SetValue("UserDismissedSmartTips", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UserSettingsInfoProvider.DeleteUserSettingsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UserSettingsInfoProvider.SetUserSettingsInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty UserSettingsInfo object.
        /// </summary>
        public UserSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new UserSettingsInfo object from the given DataRow.
        /// </summary>
        public UserSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the object default data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            UserActivationDate = DateTimeHelper.ZERO_TIME;
            UserActivityPoints = 0;
            UserAvatarType = AvatarInfoProvider.AVATAR;
            UserCampaign = "";
            UserDescription = "";
            UserFacebookID = "";
            UserGender = 0;
            UserIM = "";
            UserLinkedInID = "";
            UserLogActivities = true;
            UserMessagingNotificationEmail = "";
            UserNickName = "";
            UserPhone = "";
            UserPosition = "";
            UserShowIntroductionTile = true;
            UserSignature = "";
            UserSkype = "";
            UserURLReferrer = "";
            UserWaitingForApproval = false;
            WindowsLiveID = "";
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            if (UserSettingsInfoProvider.DeleteCustomAvatars)
            {
                // Delete user's avatar if is custom
                AvatarInfo ai = AvatarInfoProvider.GetAvatarInfo(UserAvatarID);
                if ((ai != null) && ai.AvatarIsCustom)
                {
                    AvatarInfoProvider.DeleteAvatarInfo(ai);
                }
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Updates information about which webparts user used recently.
        /// </summary>        
        /// <param name="webPartName">Web part name</param>
        public void UpdateRecentlyUsedWebPart(string webPartName)
        {
            string webParts = UpdateRecentlyUsedObject(webPartName, UserUsedWebParts, MAX_RECENTLY_USED_WEBPARTS);

            // Update database
            UserUsedWebParts = webParts;
            UserSettingsInfoProvider.SetUserSettingsInfo(this);
        }


        /// <summary>
        /// Updates information about which widgets user used recently.
        /// </summary>        
        /// <param name="widgetName">Widget codename</param>
        public void UpdateRecentlyUsedWidget(string widgetName)
        {
            string widgets = UpdateRecentlyUsedObject(widgetName, UserUsedWidgets, MAX_RECENTLY_USED_WIDGETS);

            // Update info
            UserUsedWidgets = widgets;
            UserSettingsInfoProvider.SetUserSettingsInfo(this);
        }


        /// <summary>
        ///  Returns updated version of last used object.
        /// </summary>
        /// <param name="codename">Codename of last used object</param>
        /// <param name="codenames">List of all used object</param>
        /// <param name="maxValue">Size of list with objects</param>        
        private string UpdateRecentlyUsedObject(string codename, string codenames, int maxValue)
        {
            codenames = ";" + codenames + ";";

            // Remove codename if is already in list
            codenames = codenames.Replace(";" + codename + ";", ";");

            // Add codename to first position
            codenames = codename + codenames;
            codenames = codenames.TrimEnd(';');

            // Check number of last recently used
            int numberOfObjects = codenames.Split(';').Length;
            if (numberOfObjects > maxValue)
            {
                codenames = codenames.Substring(0, codenames.LastIndexOfCSafe(';'));
            }

            return codenames;
        }

        #endregion
    }
}