using System;
using System.Data;

using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(ActivityInfo), ActivityInfo.OBJECT_TYPE)]

namespace CMS.Activities
{
    /// <summary>
    /// ActivityInfo data container class.
    /// </summary>
    public class ActivityInfo : AbstractInfo<ActivityInfo>, IActivityInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ACTIVITY;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ActivityInfoProvider), OBJECT_TYPE, "OM.Activity", "ActivityID", null, null, null,
            "ActivityTitle", null, "ActivitySiteID", "ActivityContactID", PredefinedObjectType.CONTACT)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.ACTIVITIES,
            SupportsCloning = false,
            ContainsMacros = false,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            Feature = FeatureEnum.FullContactManagement
        };

        #endregion

        
        #region "Properties"

        /// <summary>
        /// Gets or sets the UTM source for this activity. <c> NULL </c> value is saved if source is empty.
        /// </summary>
        public virtual string ActivityUTMSource
        {
            get
            {
                return GetStringValue("ActivityUTMSource", String.Empty);
            }
            set
            {
                SetValue("ActivityUTMSource", TextHelper.LimitLength(value, 200, String.Empty), !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets the campaign UTM content for this activity.
        /// </summary>
        public virtual string ActivityUTMContent
        {
            get
            {
                return GetStringValue("ActivityUTMContent", String.Empty);
            }
            set
            {
                SetValue("ActivityUTMContent", TextHelper.LimitLength(value, 200, String.Empty), !string.IsNullOrEmpty(value));
            }
        }


        /// <summary>
        /// Gets or sets the campaign code name for this activity.
        /// </summary>
        public virtual string ActivityCampaign
        {
            get
            {
                return GetStringValue("ActivityCampaign", null);
            }
            set
            {
                SetValue("ActivityCampaign", TextHelper.LimitLength(value, 200, String.Empty));
            }
        }


        /// <summary>
        /// Gets or sets the ID of the item detail that relates to this activity.
        /// </summary>
        public virtual int ActivityItemDetailID
        {
            get
            {
                return GetIntegerValue("ActivityItemDetailID", 0);
            }
            set
            {
                SetValue("ActivityItemDetailID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public virtual int ActivityID
        {
            get
            {
                return GetIntegerValue("ActivityID", 0);
            }
            set
            {
                SetValue("ActivityID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the active contact for this activity.
        /// </summary>
        public virtual int ActivityContactID
        {
            get
            {
                return GetIntegerValue("ActivityContactID", 0);
            }
            set
            {
                SetValue("ActivityContactID", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the active contact for this activity. This fields does not have database representation.
        /// </summary>
        public virtual Guid ActivityContactGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the node ID that relates to this activity.
        /// </summary>
        public virtual int ActivityNodeID
        {
            get
            {
                return GetIntegerValue("ActivityNodeID", 0);
            }
            set
            {
                SetValue("ActivityNodeID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the activity title.
        /// </summary>
        public virtual string ActivityTitle
        {
            get
            {
                return GetStringValue("ActivityTitle", "");
            }
            set
            {
                SetValue("ActivityTitle", TextHelper.LimitLength(value, 250, String.Empty));
            }
        }


        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        public virtual string ActivityType
        {
            get
            {
                return GetStringValue("ActivityType", "");
            }
            set
            {
                SetValue("ActivityType", TextHelper.LimitLength(value, 250, String.Empty));
            }
        }


        /// <summary>
        /// Gets or sets the value of this activity.
        /// </summary>
        public virtual string ActivityValue
        {
            get
            {
                return GetStringValue("ActivityValue", "");
            }
            set
            {
                SetValue("ActivityValue", TextHelper.LimitLength(value, 250, String.Empty));
            }
        }


        /// <summary>
        /// Gets or sets the URL of this activity.
        /// </summary>
        public virtual string ActivityURL
        {
            get
            {
                return GetStringValue("ActivityURL", "");
            }
            set
            {
                SetValue("ActivityURL", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time the activity was created.
        /// </summary>
        public virtual DateTime ActivityCreated
        {
            get
            {
                return GetDateTimeValue("ActivityCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ActivityCreated", value);
            }
        }


        /// <summary>
        /// Gets or sets the site ID for this activity.
        /// </summary>
        public virtual int ActivitySiteID
        {
            get
            {
                return GetIntegerValue("ActivitySiteID", 0);
            }
            set
            {
                SetValue("ActivitySiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the AB test variant name.
        /// </summary>
        public virtual string ActivityABVariantName
        {
            get
            {
                return GetStringValue("ActivityABVariantName", String.Empty);
            }
            set
            {
                SetValue("ActivityABVariantName", value);
            }
        }


        /// <summary>
        /// Gets or sets the MVT Combination name.
        /// </summary>
        public virtual string ActivityMVTCombinationName
        {
            get
            {
                return GetStringValue("ActivityMVTCombinationName", String.Empty);
            }
            set
            {
                SetValue("ActivityMVTCombinationName", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the item that relates to this activity.
        /// </summary>
        public virtual int ActivityItemID
        {
            get
            {
                return GetIntegerValue("ActivityItemID", 0);
            }
            set
            {
                SetValue("ActivityItemID", value);
            }
        }


        /// <summary>
        /// Gets or sets URL referrer.
        /// </summary>
        public virtual string ActivityURLReferrer
        {
            get
            {
                return GetStringValue("ActivityURLReferrer", null);
            }
            set
            {
                SetValue("ActivityURLReferrer", value);
            }
        }


        /// <summary>
        /// Gets or set additional comment.
        /// </summary>
        public virtual string ActivityComment
        {
            get
            {
                return GetStringValue("ActivityComment", null);
            }
            set
            {
                SetValue("ActivityComment", value);
            }
        }


        /// <summary>
        /// Gets or sets the document culture where activity occurred.
        /// </summary>
        public virtual string ActivityCulture
        {
            get
            {
                return GetStringValue("ActivityCulture", "");
            }
            set
            {
                SetValue("ActivityCulture", TextHelper.LimitLength(value, 10, String.Empty));
            }
        }


        /// <summary>
        /// Gets or sets hash result of <see cref="IActivityInfo.ActivityURL"/>. The hash is needed for <see cref="PredefinedActivityType.PAGE_VISIT"/> activity type created on content only sites <see cref="SiteInfo.SiteIsContentOnly"/>.
        /// </summary>
        public virtual long ActivityURLHash
        {
            get
            {
                return ValidationHelper.GetLong(GetValue("ActivityURLHash"), 0);
            }
            set
            {
                SetValue("ActivityURLHash", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ActivityInfoProvider.DeleteActivityInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        /// <remarks>
        /// This method should not be used directly. For inserting new activity info use implementation of IActivityRepository.
        /// </remarks>
        protected override void SetObject()
        {
            ActivityInfoProvider.SetActivityInfo(this);
        }


        /// <summary>
        /// Loads the default object data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            ActivityURLHash = 0;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ActivityInfo object.
        /// </summary>
        public ActivityInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ActivityInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ActivityInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ACTIVITIES, "ReadActivities", siteName, exceptionOnFailure);
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ACTIVITIES, "ManageActivities", siteName, exceptionOnFailure);
                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region "License check methods"

        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, feature in the best license is check</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Read, string domainName = null)
        {
            return ObjectFactory<ILicenseService>.StaticSingleton().IsFeatureAvailable(TypeInfo.Feature);
        }

        #endregion


        /// <summary>
        /// Registers the properties of this object.
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("Poll", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.POLL))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.POLL);

            RegisterProperty("Form", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.BIZFORM))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.BIZFORM);

            RegisterProperty("Forum", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.FORUM))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.FORUM);

            RegisterProperty("Group", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.GROUP))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.GROUP);

            RegisterProperty("Board", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.BOARD))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.BOARD);

            RegisterProperty("SKU", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.SKU))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.SKU);

            RegisterProperty("Issue", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.NEWSLETTERISSUE))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.NEWSLETTERISSUE);

            RegisterProperty("Newsletter", x => ActivityObjectMapper.GetLinkedObject(x, PredefinedObjectType.NEWSLETTER))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.NEWSLETTER);

            RegisterProperty("Node", x => ProviderHelper.GetInfoById(PredefinedObjectType.NODE, x.ActivityNodeID))
                .EmptyObjectFactory = new InfoObjectFactory(PredefinedObjectType.NODE);
        }
    }
}