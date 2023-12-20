using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.BannerManagement;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(BannerInfo), BannerInfo.OBJECT_TYPE)]

namespace CMS.BannerManagement
{
    /// <summary>
    /// BannerInfo data container class.
    /// </summary>
    public class BannerInfo : AbstractInfo<BannerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.banner";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BannerInfoProvider), OBJECT_TYPE, "CMS.Banner", "BannerID", "BannerLastModified", "BannerGuid", "BannerName", "BannerDisplayName", null, "BannerSiteID", null, null)
        {
            ModuleName = ModuleName.BANNERMANAGEMENT,

            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("BannerCategoryID", BannerCategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            },

            // When banner category is deleted, remove nested banners with API so they get e.g. to recycle bin.
            DeleteObjectWithAPI = true,

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING)
                },
                ExcludedStagingColumns = new List<string>
                {
                    "BannerHitsLeft",
                    "BannerClicksLeft"
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,

            DefaultOrderBy = "BannerName ASC",
            HasMetaFiles = true,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING)
                },
            },
            SupportsGlobalObjects = true,
            EnabledColumn = "BannerEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "BannerHitsLeft",
                    "BannerClicksLeft"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Code name of the banner.
        /// </summary>
        public virtual string BannerName
        {
            get
            {
                return GetStringValue("BannerName", "");
            }
            set
            {
                SetValue("BannerName", value);
            }
        }


        /// <summary>
        /// True if banner is enabled.
        /// </summary>
        public virtual bool BannerEnabled
        {
            get
            {
                return GetBooleanValue("BannerEnabled", false);
            }
            set
            {
                SetValue("BannerEnabled", value);
            }
        }


        /// <summary>
        /// Banner guid.
        /// </summary>
        public virtual Guid BannerGuid
        {
            get
            {
                return GetGuidValue("BannerGuid", Guid.Empty);
            }
            set
            {
                SetValue("BannerGuid", value);
            }
        }


        /// <summary>
        /// If true link will lead to new window (target _blank).
        /// </summary>
        public virtual bool BannerBlank
        {
            get
            {
                return GetBooleanValue("BannerBlank", false);
            }
            set
            {
                SetValue("BannerBlank", value);
            }
        }


        /// <summary>
        /// ID of the banner.
        /// </summary>
        public virtual int BannerID
        {
            get
            {
                return GetIntegerValue("BannerID", 0);
            }
            set
            {
                SetValue("BannerID", value);
            }
        }


        /// <summary>
        /// Banner will be displayed to this time.
        /// </summary>
        public virtual DateTime BannerTo
        {
            get
            {
                return GetDateTimeValue("BannerTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BannerTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Banner will be displayed from this time.
        /// </summary>
        public virtual DateTime BannerFrom
        {
            get
            {
                return GetDateTimeValue("BannerFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BannerFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Hits left.
        /// </summary>
        public virtual int? BannerHitsLeft
        {
            get
            {
                object value = GetValue("BannerHitsLeft");

                if (value != null)
                {
                    return ValidationHelper.GetInteger(value, 0);
                }

                return null;
            }
            set
            {
                SetValue("BannerHitsLeft", value);
            }
        }


        /// <summary>
        /// Clicks left.
        /// </summary>
        public virtual int? BannerClicksLeft
        {
            get
            {
                object value = GetValue("BannerClicksLeft");

                if (value != null)
                {
                    return ValidationHelper.GetInteger(value, 0);
                }

                return null;
            }
            set
            {
                SetValue("BannerClicksLeft", value);
            }
        }


        /// <summary>
        /// Weight of the banner. Banners with higher weight will be displayed more often.
        /// </summary>
        public virtual double BannerWeight
        {
            get
            {
                return GetDoubleValue("BannerWeight", 0.0);
            }
            set
            {
                SetValue("BannerWeight", value);
            }
        }


        /// <summary>
        /// Banner HTML text.
        /// </summary>
        public virtual string BannerContent
        {
            get
            {
                return GetStringValue("BannerContent", "");
            }
            set
            {
                SetValue("BannerContent", value);
            }
        }


        /// <summary>
        /// Category of the banner.
        /// </summary>
        public virtual int BannerCategoryID
        {
            get
            {
                return GetIntegerValue("BannerCategoryID", 0);
            }
            set
            {
                SetValue("BannerCategoryID", value);
            }
        }


        /// <summary>
        /// Display name of the banner.
        /// </summary>
        public virtual string BannerDisplayName
        {
            get
            {
                return GetStringValue("BannerDisplayName", "");
            }
            set
            {
                SetValue("BannerDisplayName", value);
            }
        }


        /// <summary>
        /// URL to be redirected to after clicking the banner.
        /// </summary>
        public virtual string BannerURL
        {
            get
            {
                return GetStringValue("BannerURL", "");
            }
            set
            {
                SetValue("BannerURL", value);
            }
        }


        /// <summary>
        /// Banner type.
        /// </summary>
        public virtual BannerTypeEnum BannerType
        {
            get
            {
                return (BannerTypeEnum)GetIntegerValue("BannerType", (int)BannerTypeEnum.Plain);
            }
            set
            {
                SetValue("BannerType", (int)value);
            }
        }


        /// <summary>
        /// Last modification of this banner.
        /// </summary>
        public virtual DateTime BannerLastModified
        {
            get
            {
                return GetDateTimeValue("BannerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BannerLastModified", value);
            }
        }


        /// <summary>
        /// Banner site ID.
        /// </summary>
        public virtual int BannerSiteID
        {
            get
            {
                return GetIntegerValue("BannerSiteID", 0);
            }
            set
            {
                SetValue("BannerSiteID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BannerInfoProvider.DeleteBannerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BannerInfoProvider.SetBannerInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BannerInfo object.
        /// </summary>
        public BannerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BannerInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public BannerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
