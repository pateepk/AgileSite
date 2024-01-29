using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Membership;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(ConversionInfo), ConversionInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// ConversionInfo data container class.
    /// </summary>
    public class ConversionInfo : AbstractInfo<ConversionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.conversion";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ConversionInfoProvider), OBJECT_TYPE, "Analytics.Conversion", "ConversionID", "ConversionLastModified", "ConversionGUID", "ConversionName", "ConversionDisplayName", null, "ConversionSiteID", null, null)
        {
            ModuleName = ModuleName.WEBANALYTICS,
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            Feature = FeatureEnum.CampaignAndConversions
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Conversion object unique identifier.
        /// </summary>
        public virtual Guid ConversionGUID
        {
            get
            {
                return GetGuidValue("ConversionGUID", Guid.Empty);
            }
            set
            {
                SetValue("ConversionGUID", value);
            }
        }


        /// <summary>
        /// Conversion object description.
        /// </summary>
        public virtual string ConversionDescription
        {
            get
            {
                return GetStringValue("ConversionDescription", "");
            }
            set
            {
                SetValue("ConversionDescription", value);
            }
        }


        /// <summary>
        /// Conversion object ID.
        /// </summary>
        public virtual int ConversionID
        {
            get
            {
                return GetIntegerValue("ConversionID", 0);
            }
            set
            {
                SetValue("ConversionID", value);
            }
        }


        /// <summary>
        /// Conversion object site ID.
        /// </summary>
        public virtual int ConversionSiteID
        {
            get
            {
                return GetIntegerValue("ConversionSiteID", 0);
            }
            set
            {
                SetValue("ConversionSiteID", value);
            }
        }


        /// <summary>
        /// Date and time when the conversion object was last modified.
        /// </summary>
        public virtual DateTime ConversionLastModified
        {
            get
            {
                return GetDateTimeValue("ConversionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ConversionLastModified", value);
            }
        }


        /// <summary>
        /// Conversion object code name.
        /// </summary>
        public virtual string ConversionName
        {
            get
            {
                return GetStringValue("ConversionName", "");
            }
            set
            {
                SetValue("ConversionName", value);
            }
        }


        /// <summary>
        /// Conversion object display name.
        /// </summary>
        public virtual string ConversionDisplayName
        {
            get
            {
                return GetStringValue("ConversionDisplayName", "");
            }
            set
            {
                SetValue("ConversionDisplayName", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ConversionInfoProvider.DeleteConversionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ConversionInfoProvider.SetConversionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ConversionInfo object.
        /// </summary>
        public ConversionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ConversionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ConversionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Overrides permission name for managing the object info.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <returns>ManageConversions permission name for managing permission type, or base permission name otherwise</returns>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Destroy:
                    return "ManageConversions";

                default:
                    return base.GetPermissionName(permission);
            }
        }

        #endregion
    }
}