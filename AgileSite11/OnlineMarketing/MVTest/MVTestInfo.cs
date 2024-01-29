using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WebAnalytics;
using CMS.Base;
using CMS.Membership;
using CMS.OnlineMarketing;

[assembly: RegisterObjectType(typeof(MVTestInfo), MVTestInfo.OBJECT_TYPE)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MVTestInfo data container class.
    /// </summary>
    public class MVTestInfo : AbstractInfo<MVTestInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.mvtest";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MVTestInfoProvider), OBJECT_TYPE, "OM.MVTest", "MVTestID", "MVTestLastModified", "MVTestGUID", "MVTestName", "MVTestDisplayName", null, "MVTestSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING)
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ModuleName = ModuleName.ONLINEMARKETING,
            MaxCodeNameLength = 50,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING)
                }
            },
            EnabledColumn = "MVTestEnabled",
            Feature = FeatureEnum.MVTesting,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "MVTestConversions", "MVTestOpenFrom", "MVTestOpenTo", "MVTestEnabled" }
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Original culture of the multivariate test.
        /// </summary>
        protected string mOriginalCulture = string.Empty;

        #endregion


        #region "Properties"

        /// <summary>
        /// Multivariate test target conversion type.
        /// </summary>
        public virtual MVTTargetConversionTypeEnum MVTestTargetConversionType
        {
            get
            {
                return MVTestInfoProvider.GetMVTTargetConversionTypeEnum(ValidationHelper.GetString(GetValue("MVTestTargetConversionType"), "total"));
            }
            set
            {
                SetValue("MVTestTargetConversionType", MVTestInfoProvider.GetMVTTargetConversionTypeString(value));
            }
        }


        /// <summary>
        /// Multivariate test ID.
        /// </summary>
        public virtual int MVTestID
        {
            get
            {
                return GetIntegerValue("MVTestID", 0);
            }
            set
            {
                SetValue("MVTestID", value);
            }
        }


        /// <summary>
        /// Multivariate test conversions.
        /// </summary>
        public virtual int MVTestConversions
        {
            get
            {
                return GetIntegerValue("MVTestConversions", 0);
            }
            set
            {
                SetValue("MVTestConversions", value);
            }
        }


        /// <summary>
        /// Multivariate test culture.
        /// </summary>
        public virtual string MVTestCulture
        {
            get
            {
                return GetStringValue("MVTestCulture", "");
            }
            set
            {
                if (value == "-1")
                {
                    SetValue("MVTestCulture", null);
                }
                else
                {
                    SetValue("MVTestCulture", value);
                }
            }
        }


        /// <summary>
        /// Last modification of the multivariate test.
        /// </summary>
        public virtual DateTime MVTestLastModified
        {
            get
            {
                return GetDateTimeValue("MVTestLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("MVTestLastModified", value);
            }
        }


        /// <summary>
        /// Start date of the test.
        /// </summary>
        public virtual DateTime MVTestOpenFrom
        {
            get
            {
                return GetDateTimeValue("MVTestOpenFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value == DateTimeHelper.ZERO_TIME)
                {
                    SetValue("MVTestOpenFrom", null);
                }
                else
                {
                    SetValue("MVTestOpenFrom", value);
                }
            }
        }


        /// <summary>
        /// Unique multivariate test identifier.
        /// </summary>
        public virtual Guid MVTestGUID
        {
            get
            {
                return GetGuidValue("MVTestGUID", Guid.Empty);
            }
            set
            {
                SetValue("MVTestGUID", value);
            }
        }


        /// <summary>
        /// Multivariate test name.
        /// </summary>
        public virtual string MVTestName
        {
            get
            {
                return GetStringValue("MVTestName", "");
            }
            set
            {
                SetValue("MVTestName", value);
            }
        }


        /// <summary>
        /// Multivariate test display name.
        /// </summary>
        public virtual string MVTestDisplayName
        {
            get
            {
                return GetStringValue("MVTestDisplayName", "");
            }
            set
            {
                SetValue("MVTestDisplayName", value);
            }
        }


        /// <summary>
        /// Multivariate test maximum conversions.
        /// </summary>
        public virtual int MVTestMaxConversions
        {
            get
            {
                return GetIntegerValue("MVTestMaxConversions", 0);
            }
            set
            {
                SetValue("MVTestMaxConversions", value);
            }
        }


        /// <summary>
        /// Multivariate test description.
        /// </summary>
        public virtual string MVTestDescription
        {
            get
            {
                return GetStringValue("MVTestDescription", "");
            }
            set
            {
                SetValue("MVTestDescription", value);
            }
        }


        /// <summary>
        /// Multivariate test page.
        /// </summary>
        public virtual string MVTestPage
        {
            get
            {
                return GetStringValue("MVTestPage", "");
            }
            set
            {
                SetValue("MVTestPage", value);
            }
        }


        /// <summary>
        /// Multivariate test site ID.
        /// </summary>
        public virtual int MVTestSiteID
        {
            get
            {
                return GetIntegerValue("MVTestSiteID", 0);
            }
            set
            {
                SetValue("MVTestSiteID", value);
            }
        }


        /// <summary>
        /// End date of the test.
        /// </summary>
        public virtual DateTime MVTestOpenTo
        {
            get
            {
                return GetDateTimeValue("MVTestOpenTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value == DateTimeHelper.ZERO_TIME)
                {
                    SetValue("MVTestOpenTo", null);
                }
                else
                {
                    SetValue("MVTestOpenTo", value);
                }
            }
        }


        /// <summary>
        /// Indicates whether the multivariate test is enabled.
        /// </summary>
        public virtual bool MVTestEnabled
        {
            get
            {
                return GetBooleanValue("MVTestEnabled", false);
            }
            set
            {
                SetValue("MVTestEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the original culture of the test.
        /// </summary>
        public virtual string OriginalCulture
        {
            get
            {
                return mOriginalCulture;
            }
            set
            {
                mOriginalCulture = value;
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MVTestInfoProvider.DeleteMVTestInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MVTestInfoProvider.SetMVTestInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty MVTestInfo object.
        /// </summary>
        public MVTestInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MVTestInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MVTestInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();
            MVTestEnabled = false;
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the MVT Test object.
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
                    return UserInfoProvider.IsAuthorizedPerResource("CMS.MVTest", "Read", siteName, (UserInfo)userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource("CMS.MVTest", "Manage", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Removes dependencies from the specified MVTest.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            var statisticsInfos = StatisticsInfoProvider.GetStatistics()
#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                .WhereLike("StatisticsCode", string.Format("mvtconversion;{0};%", MVTestName))
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                .WhereEquals("StatisticsSiteID", MVTestSiteID)
                .Column("StatisticsID");

            var where = new WhereCondition().WhereIn("HitsStatisticsID", statisticsInfos);

            HitsYearInfoProvider.DeleteHitsYearInfo(where);
            HitsMonthInfoProvider.DeleteHitsMonthInfo(where);
            HitsWeekInfoProvider.DeleteHitsWeekInfo(where);
            HitsDayInfoProvider.DeleteHitsDayInfo(where);
            HitsHourInfoProvider.DeleteHitsHourInfo(where);
            StatisticsInfoProvider.DeleteStatisticsInfo(new WhereCondition().WhereIn("StatisticsID", statisticsInfos));

			base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion
    }
}
