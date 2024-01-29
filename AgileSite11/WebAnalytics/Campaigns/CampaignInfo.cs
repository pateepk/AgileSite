using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(CampaignInfo), CampaignInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// CampaignInfo data container class.
    /// </summary>
    public class CampaignInfo : AbstractInfo<CampaignInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.campaign";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CampaignInfoProvider), OBJECT_TYPE, "Analytics.Campaign", "CampaignID", "CampaignLastModified", "CampaignGUID", "CampaignName", "CampaignDisplayName", null, "CampaignSiteID", null, null)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            DependsOn = new List<ObjectDependency>() {
                new ObjectDependency("CampaignScheduledTaskID", PredefinedObjectType.OBJECTSCHEDULEDTASK)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ExcludedStagingColumns = new List<string>
                {
                    "CampaignOpenFrom", "CampaignOpenTo", "CampaignCalculatedTo", "CampaignScheduledTaskID", "CampaignVisitors"
                },
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                }
            },

            ContinuousIntegrationSettings =
            {
                Enabled = true
            },

            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "CampaignOpenFrom", "CampaignOpenTo", "CampaignCalculatedTo", "CampaignScheduledTaskID", "CampaignVisitors"
                }
            },

            SupportsCloning = false,
            SupportsCloneToOtherSite = false,
            LogEvents = true,
            TouchCacheDependencies = true,
            Feature = FeatureEnum.CampaignAndConversions
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Campaign object display name.
        /// </summary>
        [DatabaseField]
        public virtual string CampaignDisplayName
        {
            get
            {
                return GetStringValue("CampaignDisplayName", "");
            }
            set
            {
                SetValue("CampaignDisplayName", value);
            }
        }


        /// <summary>
        /// Campaign object ID.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignID
        {
            get
            {
                return GetIntegerValue("CampaignID", 0);
            }
            set
            {
                SetValue("CampaignID", value);
            }
        }


        /// <summary>
        /// Gets or sets the campaign scheduled task ID.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignScheduledTaskID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignScheduledTaskID"), 0);
            }
            set
            {
                SetValue("CampaignScheduledTaskID", value, value > 0);
            }
        }


        /// <summary>
        /// Campaign object site ID.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignSiteID
        {
            get
            {
                return GetIntegerValue("CampaignSiteID", 0);
            }
            set
            {
                SetValue("CampaignSiteID", value);
            }
        }


        /// <summary>
        /// Campaign object description.
        /// </summary>
        [DatabaseField]
        public virtual string CampaignDescription
        {
            get
            {
                return GetStringValue("CampaignDescription", "");
            }
            set
            {
                SetValue("CampaignDescription", value);
            }
        }


        /// <summary>
        /// Campaign object name.
        /// </summary>
        [DatabaseField]
        public virtual string CampaignName
        {
            get
            {
                return GetStringValue("CampaignName", "");
            }
            set
            {
                SetValue("CampaignName", value);
            }
        }


        /// <summary>
        /// Campaign UTM code.
        /// </summary>
        [DatabaseField]
        public virtual string CampaignUTMCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CampaignUTMCode"), String.Empty);
            }
            set
            {
                SetValue("CampaignUTMCode", value, String.Empty);
            }
        }


        /// <summary>
        /// Campaign open to.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignOpenTo
        {
            get
            {
                return GetDateTimeValue("CampaignOpenTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignOpenTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Campaign open from.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignOpenFrom
        {
            get
            {
                return GetDateTimeValue("CampaignOpenFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignOpenFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Unique campaign visitors count.
        /// </summary>
        [DatabaseField]
        public virtual int CampaignVisitors
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CampaignVisitors"), 0);
            }
            set
            {
                SetValue("CampaignVisitors", value, 0);
            }
        }


        /// <summary>
        /// Campaign object unique identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid CampaignGUID
        {
            get
            {
                return GetGuidValue("CampaignGUID", Guid.Empty);
            }
            set
            {
                SetValue("CampaignGUID", value);
            }
        }


        /// <summary>
        /// Campaign last modification date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignLastModified
        {
            get
            {
                return GetDateTimeValue("CampaignLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignLastModified", value);
            }
        }


        /// <summary>
        /// Stores the date (and time) of the last campaign report processing.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CampaignCalculatedTo
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("CampaignCalculatedTo"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CampaignCalculatedTo", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CampaignInfoProvider.DeleteCampaignInfo(this);
        }


        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete scheduled task            
            var task = TaskInfoProvider.GetTaskInfo(CampaignScheduledTaskID);
            if (task != null)
            {
                TaskInfoProvider.DeleteTaskInfo(task);
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CampaignInfoProvider.SetCampaignInfo(this);
        }


        /// <summary>
        /// Computes the current campaign status for the given <paramref name="dateTime"/>.
        /// </summary>
        /// <param name="dateTime">Date time for which the status is computed.</param>
        /// <returns>Status of the campaign.</returns>
        public CampaignStatusEnum GetCampaignStatus(DateTime dateTime)
        {
            if (CampaignOpenFrom == DateTimeHelper.ZERO_TIME)
            {
                return CampaignStatusEnum.Draft;
            }

            if ((CampaignOpenTo != DateTimeHelper.ZERO_TIME) && (dateTime >= CampaignOpenTo))
            {
                return CampaignStatusEnum.Finished;
            }

            if ((dateTime < CampaignOpenFrom) || (CampaignScheduledTaskID != 0))
            {
                return CampaignStatusEnum.Scheduled;
            }

            return CampaignStatusEnum.Running;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CampaignInfo object.
        /// </summary>
        public CampaignInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CampaignInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CampaignInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>ManageCampaigns permission name for managing permission type, or base permission name otherwise</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return "ManageCampaigns";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}