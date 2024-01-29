using System;
using System.Web;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(StatisticsInfo), StatisticsInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// StatisticsInfo data container class.
    /// </summary>
    public class StatisticsInfo : AbstractInfo<StatisticsInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.statistics";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(StatisticsInfoProvider), OBJECT_TYPE, "Analytics.Statistics", "StatisticsID", null, null, "StatisticsCode", null, null, "StatisticsSiteID", null, null)
            {
                LogEvents = false,
                TouchCacheDependencies = true,
                SupportsVersioning = false,
                DeleteObjectWithAPI = true,
                ImportExportSettings = { LogExport = false },
                ContainsMacros = false
            };

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether the codename should be validated upon saving.
        /// It is set to false to circumvent semicolon checking in sample statistics generator.
        /// </summary>
        protected override bool ValidateCodeName
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Statistics ID.
        /// </summary>
        public virtual int StatisticsID
        {
            get
            {
                return GetIntegerValue("StatisticsID", 0);
            }
            set
            {
                SetValue("StatisticsID", value);
            }
        }


        /// <summary>
        /// Statistics code.
        /// </summary>
        public virtual string StatisticsCode
        {
            get
            {
                return GetStringValue("StatisticsCode", "");
            }
            set
            {
                SetValue("StatisticsCode", value);
            }
        }


        /// <summary>
        /// Statistics site ID.
        /// </summary>
        public virtual int StatisticsSiteID
        {
            get
            {
                return GetIntegerValue("StatisticsSiteID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("StatisticsSiteID", null);
                }
                else
                {
                    SetValue("StatisticsSiteID", value);
                }
            }
        }


        /// <summary>
        /// Statistics object name.
        /// </summary>
        public virtual string StatisticsObjectName
        {
            get
            {
                return GetStringValue("StatisticsObjectName", "");
            }
            set
            {
                SetValue("StatisticsObjectName", value);
            }
        }


        /// <summary>
        /// Statistics object ID.
        /// </summary>
        public virtual int StatisticsObjectID
        {
            get
            {
                return GetIntegerValue("StatisticsObjectID", 0);
            }
            set
            {
                SetValue("StatisticsObjectID", value);
            }
        }


        /// <summary>
        /// Statistics object culture.
        /// </summary>
        public virtual string StatisticsObjectCulture
        {
            get
            {
                return GetStringValue("StatisticsObjectCulture", "");
            }
            set
            {
                SetValue("StatisticsObjectCulture", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            StatisticsInfoProvider.DeleteStatisticsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            StatisticsInfoProvider.SetStatisticsInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty StatisticsInfo object.
        /// </summary>
        public StatisticsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new StatisticsInfo object from the given DataRow.
        /// </summary>
        public StatisticsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}