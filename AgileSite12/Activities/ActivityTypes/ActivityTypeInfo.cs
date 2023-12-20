using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Activities;
using CMS.Core;
using CMS.Base;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ActivityTypeInfo), ActivityTypeInfo.OBJECT_TYPE)]

namespace CMS.Activities
{
    /// <summary>
    /// ActivityTypeInfo data container class.
    /// </summary>
    public class ActivityTypeInfo : AbstractInfo<ActivityTypeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.activitytype";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ActivityTypeInfoProvider), OBJECT_TYPE, "OM.ActivityType", "ActivityTypeID", null, null, "ActivityTypeName", "ActivityTypeDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, "##ONLINEMARKETING##")
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.ACTIVITIES,
            EnabledColumn = "ActivityTypeEnabled",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                IsExportable = true,
                WhereCondition = "ActivityTypeIsCustom = 1",
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, "##ONLINEMARKETING##")
                },
            },
            IsTriggerTarget = true,
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the display name of the activity type.
        /// </summary>
        [DatabaseField]
        public virtual string ActivityTypeDisplayName
        {
            get
            {
                return GetStringValue("ActivityTypeDisplayName", "");
            }
            set
            {
                SetValue("ActivityTypeDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the description for the activity.
        /// </summary>
        [DatabaseField]
        public virtual string ActivityTypeDescription
        {
            get
            {
                return GetStringValue("ActivityTypeDescription", "");
            }
            set
            {
                SetValue("ActivityTypeDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets whether the activity is custom.
        /// </summary>
        [DatabaseField]
        public virtual bool ActivityTypeIsCustom
        {
            get
            {
                return GetBooleanValue("ActivityTypeIsCustom", false);
            }
            set
            {
                SetValue("ActivityTypeIsCustom", value);
            }
        }


        /// <summary>
        /// Gets or sets the the code name of the activity type.
        /// </summary>
        [DatabaseField]
        public virtual string ActivityTypeName
        {
            get
            {
                return GetStringValue("ActivityTypeName", "");
            }
            set
            {
                SetValue("ActivityTypeName", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the activity type.
        /// </summary>
        [DatabaseField]
        public virtual int ActivityTypeID
        {
            get
            {
                return GetIntegerValue("ActivityTypeID", 0);
            }
            set
            {
                SetValue("ActivityTypeID", value);
            }
        }


        /// <summary>
        /// Gets or sets whether the activity is enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool ActivityTypeEnabled
        {
            get
            {
                return GetBooleanValue("ActivityTypeEnabled", false);
            }
            set
            {
                SetValue("ActivityTypeEnabled", value);
            }
        }


        /// <summary>
        /// Gets of sets whether activity should be hidden in context of content only sites/applications.
        /// </summary>
        [DatabaseField]
        public virtual bool ActivityTypeIsHiddenInContentOnly
        {
            get
            {
                return GetBooleanValue("ActivityTypeIsHiddenInContentOnly", false);
            }
            set
            {
                SetValue("ActivityTypeIsHiddenInContentOnly", value);
            }
        }


        /// <summary>
        /// Gets or sets whether the activity is enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool ActivityTypeManualCreationAllowed
        {
            get
            {
                return GetBooleanValue("ActivityTypeManualCreationAllowed", false);
            }
            set
            {
                SetValue("ActivityTypeManualCreationAllowed", value);
            }
        }


        /// <summary>
        /// Gets or sets name of form control (selector) for setting ItemID
        /// </summary>
        [DatabaseField]
        public virtual string ActivityTypeMainFormControl
        {
            get
            {
                return GetStringValue("ActivityTypeMainFormControl", null);
            }
            set
            {
                SetValue("ActivityTypeMainFormControl", value);
            }
        }


        /// <summary>
        /// Gets or sets name of form control (selector) for setting ItemDetailID
        /// </summary>
        [DatabaseField]
        public virtual string ActivityTypeDetailFormControl
        {
            get
            {
                return GetStringValue("ActivityTypeDetailFormControl", null);
            }
            set
            {
                SetValue("ActivityTypeDetailFormControl", value);
            }
        }


        /// <summary>
        /// Gets or sets color used for coloring of activity type name in listings.
        /// </summary>
        [DatabaseField]
        public virtual string ActivityTypeColor
        {
            get
            {
                return GetStringValue("ActivityTypeColor", null);
            }
            set
            {
                SetValue("ActivityTypeColor", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ActivityTypeInfoProvider.DeleteActivityTypeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ActivityTypeInfoProvider.SetActivityTypeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ActivityTypeInfo object.
        /// </summary>
        public ActivityTypeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ActivityTypeInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ActivityTypeInfo(DataRow dr)
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


        #region "Methods"

        /// <summary>
        /// Clones metafile and inserts it to DB as new object.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // System activity should clone as custom
            ActivityTypeIsCustom = true;

            Insert();
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            ActivityTypeIsHiddenInContentOnly = false;
        }

        #endregion
    }
}