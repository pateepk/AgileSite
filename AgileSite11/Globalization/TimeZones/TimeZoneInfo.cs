using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

[assembly: RegisterObjectType(typeof(TimeZoneInfo), TimeZoneInfo.OBJECT_TYPE)]

namespace CMS.Globalization
{
    /// <summary>
    /// TimeZoneInfo data container class.
    /// </summary>
    public class TimeZoneInfo : AbstractInfo<TimeZoneInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.timezone";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TimeZoneInfoProvider), OBJECT_TYPE, "CMS.TimeZone", "TimeZoneID", "TimeZoneLastModified", "TimeZoneGUID", "TimeZoneName", "TimeZoneDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                    {
                        new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                    }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "TimeZoneRuleStartIn", "TimeZoneRuleEndIn" }
            },
            ContinuousIntegrationSettings =
            {
                 Enabled = true
            },
            DefaultData = new DefaultDataSettings
            {
                ExcludedPrefixes = { "custom" }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Time zone name.
        /// </summary>
        public virtual string TimeZoneName
        {
            get
            {
                return GetStringValue("TimeZoneName", "");
            }
            set
            {
                SetValue("TimeZoneName", value);
            }
        }


        /// <summary>
        /// Indicates if time zone uses daylight saving time (DST).
        /// </summary>
        public virtual bool TimeZoneDaylight
        {
            get
            {
                return GetBooleanValue("TimeZoneDaylight", false);
            }
            set
            {
                SetValue("TimeZoneDaylight", value);
            }
        }


        /// <summary>
        /// Display Name.
        /// </summary>
        public virtual string TimeZoneDisplayName
        {
            get
            {
                return GetStringValue("TimeZoneDisplayName", "");
            }
            set
            {
                SetValue("TimeZoneDisplayName", value);
            }
        }


        /// <summary>
        /// ID of time zone.
        /// </summary>
        public virtual int TimeZoneID
        {
            get
            {
                return GetIntegerValue("TimeZoneID", 0);
            }
            set
            {
                SetValue("TimeZoneID", value);
            }
        }


        /// <summary>
        /// Rule End In.
        /// </summary>
        public virtual DateTime TimeZoneRuleEndIn
        {
            get
            {
                return GetDateTimeValue("TimeZoneRuleEndIn", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TimeZoneRuleEndIn", value);
            }
        }


        /// <summary>
        /// Time zone DST Start Rule.
        /// </summary>
        public virtual string TimeZoneRuleStartRule
        {
            get
            {
                return GetStringValue("TimeZoneRuleStartRule", "");
            }
            set
            {
                SetValue("TimeZoneRuleStartRule", value);
            }
        }


        /// <summary>
        /// Time zone DST End Rule.
        /// </summary>
        public virtual string TimeZoneRuleEndRule
        {
            get
            {
                return GetStringValue("TimeZoneRuleEndRule", "");
            }
            set
            {
                SetValue("TimeZoneRuleEndRule", value);
            }
        }


        /// <summary>
        /// Rule Start In.
        /// </summary>
        public virtual DateTime TimeZoneRuleStartIn
        {
            get
            {
                return GetDateTimeValue("TimeZoneRuleStartIn", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TimeZoneRuleStartIn", value);
            }
        }


        /// <summary>
        /// Time zone UTC offset.
        /// </summary>
        public virtual double TimeZoneGMT
        {
            get
            {
                return GetDoubleValue("TimeZoneGMT", 0.0);
            }
            set
            {
                SetValue("TimeZoneGMT", value);
            }
        }


        /// <summary>
        /// Time zone GUID.
        /// </summary>
        public virtual Guid TimeZoneGUID
        {
            get
            {
                return GetGuidValue("TimeZoneGUID", Guid.Empty);
            }
            set
            {
                SetValue("TimeZoneGUID", value);
            }
        }


        /// <summary>
        /// Time zone modified.
        /// </summary>
        public virtual DateTime TimeZoneLastModified
        {
            get
            {
                return GetDateTimeValue("TimeZoneLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TimeZoneLastModified", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TimeZoneInfoProvider.DeleteTimeZoneInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TimeZoneInfoProvider.SetTimeZoneInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TimeZoneInfo object.
        /// </summary>
        public TimeZoneInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TimeZoneInfo object from the given DataRow.
        /// </summary>
        public TimeZoneInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}