using System;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Core;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(HitsYearInfo), HitsYearInfo.OBJECT_TYPE)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// HitsYearInfo data container class.
    /// </summary>
    public class HitsYearInfo : AbstractInfo<HitsYearInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "analytics.hitsyear";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(HitsYearInfoProvider), OBJECT_TYPE, "Analytics.HitsYear", "HitsID", null, null, null, null, null, null, null, null)
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
            HitsYearInfoProvider.DeleteHitsYearInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            HitsYearInfoProvider.SetHitsYearInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty HitsYearInfo object.
        /// </summary>
        public HitsYearInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new HitsYearInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public HitsYearInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}