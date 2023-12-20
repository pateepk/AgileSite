using System;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Core;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(HitsDayInfo), HitsDayInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// HitsDayInfo data container class.
    /// </summary>
    public class HitsDayInfo : AbstractInfo<HitsDayInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.hitsday";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(HitsDayInfoProvider), OBJECT_TYPE, "Analytics.HitsDay", "HitsID", null, null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
                        {
                            new ObjectDependency("HitsStatisticsID", StatisticsInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
                        },
            ModuleName = ModuleName.WEBANALYTICS,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false, IsExportable = false },
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Hits ID.
        /// </summary>
        public virtual int HitsID
        {
            get
            {
                return GetIntegerValue("HitsID", 0);
            }
            set
            {
                SetValue("HitsID", value);
            }
        }


        /// <summary>
        /// Hits statistics ID.
        /// </summary>
        public virtual int HitsStatisticsID
        {
            get
            {
                return GetIntegerValue("HitsStatisticsID", 0);
            }
            set
            {
                SetValue("HitsStatisticsID", value, value > 0);
            }
        }


        /// <summary>
        /// Hits start time.
        /// </summary>
        public virtual DateTime HitsStartTime
        {
            get
            {
                return GetDateTimeValue("HitsStartTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("HitsStartTime", value);
            }
        }


        /// <summary>
        /// Hits end time.
        /// </summary>
        public virtual DateTime HitsEndTime
        {
            get
            {
                return GetDateTimeValue("HitsEndTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("HitsEndTime", value);
            }
        }


        /// <summary>
        /// Hits count.
        /// </summary>
        public virtual int HitsCount
        {
            get
            {
                return GetIntegerValue("HitsCount", 0);
            }
            set
            {
                SetValue("HitsCount", value);
            }
        }


        /// <summary>
        /// Hits value.
        /// </summary>
        public virtual double HitsValue
        {
            get
            {
                return GetDoubleValue("HitsValue", 0);
            }
            set
            {
                SetValue("HitsValue", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            HitsDayInfoProvider.DeleteHitsDayInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            HitsDayInfoProvider.SetHitsDayInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty HitsDayInfo object.
        /// </summary>
        public HitsDayInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new HitsDayInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public HitsDayInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}