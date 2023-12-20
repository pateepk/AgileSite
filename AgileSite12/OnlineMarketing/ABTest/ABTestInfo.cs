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

[assembly: RegisterObjectType(typeof(ABTestInfo), ABTestInfo.OBJECT_TYPE)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// ABTestInfo data container class.
    /// </summary>
    public class ABTestInfo : AbstractInfo<ABTestInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ABTEST;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ABTestInfoProvider), OBJECT_TYPE, "OM.ABTest", "ABTestID", "ABTestLastModified", "ABTestGUID", "ABTestName", "ABTestDisplayName", null, "ABTestSiteID", null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
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
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                },
            },
            Feature = FeatureEnum.ABTesting,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "ABTestOpenFrom", "ABTestOpenTo" }
            }
        };

        #endregion


        #region "Private variables"

        private ABTestConversionConfiguration mConversionConfiguration;

        #endregion


        #region "Properties"

        /// <summary>
        /// Unique test identifier.
        /// </summary>
        [DatabaseField]
        public virtual Guid ABTestGUID
        {
            get
            {
                return GetGuidValue("ABTestGUID", Guid.Empty);
            }
            set
            {
                SetValue("ABTestGUID", value);
            }
        }


        /// <summary>
        /// Test name.
        /// </summary>
        [DatabaseField]
        public virtual string ABTestName
        {
            get
            {
                return GetStringValue("ABTestName", "");
            }
            set
            {
                SetValue("ABTestName", value, "");
            }
        }


        /// <summary>
        /// Test display name.
        /// </summary>
        [DatabaseField]
        public virtual string ABTestDisplayName
        {
            get
            {
                return GetStringValue("ABTestDisplayName", "");
            }
            set
            {
                SetValue("ABTestDisplayName", value, "");
            }
        }


        /// <summary>
        /// End date for test.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ABTestOpenTo
        {
            get
            {
                return GetDateTimeValue("ABTestOpenTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ABTestOpenTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Start date for test.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ABTestOpenFrom
        {
            get
            {
                return GetDateTimeValue("ABTestOpenFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ABTestOpenFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Test conversions. Only those conversions that are assigned to the test will be shown on overview tab.
        /// If none conversions are set, all conversions will be visible.
        /// </summary>
        /// <remarks>This property can be used only on portal engine sites. Use <see cref="ABTestConversionConfiguration"/> on content only sites.</remarks>
        [DatabaseField]
        public virtual IEnumerable<string> ABTestConversions
        {
            get
            {
                return GetStringValue("ABTestConversions", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                SetValue("ABTestConversions", value?.Join(";"), "");
            }
        }


        /// <summary>
        /// Returns object representation of A/B Test conversion configuration.
        /// </summary>
        /// <remarks>This property can be used only on content only sites. Use <see cref="ABTestConversions"/> on portal engine sites.</remarks>
        public virtual ABTestConversionConfiguration ABTestConversionConfiguration
        {
            get
            {
                if (mConversionConfiguration == null)
                {
                    mConversionConfiguration = new ABTestConversionConfiguration(this, GetStringValue("ABTestConversions", null));
                }

                return mConversionConfiguration;
            }
        }


        /// <summary>
        /// Test culture.
        /// </summary>
        [DatabaseField]
        public virtual string ABTestCulture
        {
            get
            {
                return GetStringValue("ABTestCulture", "");
            }
            set
            {
                SetValue("ABTestCulture", value, "-1");
            }
        }


        /// <summary>
        /// Test ID.
        /// </summary>
        [DatabaseField]
        public virtual int ABTestID
        {
            get
            {
                return GetIntegerValue("ABTestID", 0);
            }
            set
            {
                SetValue("ABTestID", value);
            }
        }


        /// <summary>
        /// Site ID of test.
        /// </summary>
        [DatabaseField]
        public virtual int ABTestSiteID
        {
            get
            {
                return GetIntegerValue("ABTestSiteID", 0);
            }
            set
            {
                SetValue("ABTestSiteID", value);
            }
        }


        /// <summary>
        /// Test winner GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ABTestWinnerGUID
        {
            get
            {
                return GetGuidValue("ABTestWinnerGUID", Guid.Empty);
            }
            set
            {
                SetValue("ABTestWinnerGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Original page for test.
        /// </summary>
        [DatabaseField]
        public virtual string ABTestOriginalPage
        {
            get
            {
                return GetStringValue("ABTestOriginalPage", "");
            }
            set
            {
                SetValue("ABTestOriginalPage", value);
            }
        }


        /// <summary>
        /// Last modification of test.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ABTestLastModified
        {
            get
            {
                return GetDateTimeValue("ABTestLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ABTestLastModified", value);
            }
        }


        /// <summary>
        /// Test description.
        /// </summary>
        [DatabaseField]
        public virtual string ABTestDescription
        {
            get
            {
                return GetStringValue("ABTestDescription", "");
            }
            set
            {
                SetValue("ABTestDescription", value);
            }
        }


        /// <summary>
        /// Indicates what percentage of visitors will be included in AB test.
        /// This condition is resolved after segmentation macro.
        /// </summary>
        [DatabaseField]
        public virtual int ABTestIncludedTraffic
        {
            get
            {
                return GetIntegerValue("ABTestIncludedTraffic", 100);
            }
            set
            {
                SetValue("ABTestIncludedTraffic", value);
            }
        }


        /// <summary>
        /// Segmentation macro which decides whether visitor will be included in ab test or not.
        /// This condition is resolved before traffic condition.
        /// Available objects are Contact, User and Visitor. 
        /// </summary>
        [DatabaseField]
        public virtual string ABTestVisitorTargeting
        {
            get
            {
                return GetStringValue("ABTestVisitorTargeting", "");
            }
            set
            {
                SetValue("ABTestVisitorTargeting", value, "");
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ABTestInfoProvider.DeleteABTestInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ABTestInfoProvider.SetABTestInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ABTestInfo object.
        /// </summary>
        public ABTestInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ABTestInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ABTestInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes dependencies from the specified AB test.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Name", ABTestName);
            parameters.Add("@SiteID", ABTestSiteID);

            ConnectionHelper.ExecuteQuery("OM.ABTest.removefromwebanalytics", parameters);

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the AB Test object.
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
                    return UserInfoProvider.IsAuthorizedPerResource("CMS.ABTest", "Read", siteName, (UserInfo)userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource("CMS.ABTest", "Manage", siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}