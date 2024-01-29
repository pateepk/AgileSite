using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportValueInfo), ReportValueInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// ReportValueInfo data container class.
    /// </summary>
    public class ReportValueInfo : AbstractInfo<ReportValueInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "reporting.reportvalue";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ReportValueInfoProvider), OBJECT_TYPE, "Reporting.ReportValue", "ValueID", "ValueLastModified", "ValueGUID", "ValueName", "ValueDisplayName", null, null, "ValueReportID", ReportInfo.OBJECT_TYPE)
        {
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            ModuleName = "cms.reporting",
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("ValueSettings")
                }
            }
        };

        #endregion


        #region "Variables"

        private ReportCustomData mValueSettings = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Value report ID.
        /// </summary>
        public virtual int ValueReportID
        {
            get
            {
                return GetIntegerValue("ValueReportID", 0);
            }
            set
            {
                SetValue("ValueReportID", value);
            }
        }


        /// <summary>
        /// Value ID.
        /// </summary>
        public virtual int ValueID
        {
            get
            {
                return GetIntegerValue("ValueID", 0);
            }
            set
            {
                SetValue("ValueID", value);
            }
        }


        /// <summary>
        /// Connection string for report value.
        /// </summary>
        public virtual string ValueConnectionString
        {
            get
            {
                return GetStringValue("ValueConnectionString", "");
            }
            set
            {
                SetValue("ValueConnectionString", value);
            }
        }


        /// <summary>
        /// Indicates whether query is stored procedure.
        /// </summary>
        public virtual bool ValueQueryIsStoredProcedure
        {
            get
            {
                return GetBooleanValue("ValueQueryIsStoredProcedure", false);
            }
            set
            {
                SetValue("ValueQueryIsStoredProcedure", value);
            }
        }


        /// <summary>
        /// Value query.
        /// </summary>
        public virtual string ValueQuery
        {
            get
            {
                return GetStringValue("ValueQuery", "");
            }
            set
            {
                SetValue("ValueQuery", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        public virtual string ValueDisplayName
        {
            get
            {
                return GetStringValue("ValueDisplayName", "");
            }
            set
            {
                SetValue("ValueDisplayName", value);
            }
        }


        /// <summary>
        /// CustomData used for load/store settings which can be used for customization
        /// </summary>
        public virtual ReportCustomData ValueSettings
        {
            get
            {
                if (mValueSettings == null)
                {
                    // Load the custom data
                    mValueSettings = new ReportCustomData(DataClass, "ValueSettings");
                    mValueSettings.LoadData(ValidationHelper.GetString(GetValue("ValueSettings"), ""));
                }
                return mValueSettings;
            }
        }


        /// <summary>
        /// Format string.
        /// </summary>
        public virtual string ValueFormatString
        {
            get
            {
                return GetStringValue("ValueFormatString", "");
            }
            set
            {
                SetValue("ValueFormatString", value);
            }
        }


        /// <summary>
        /// Value name (code name).
        /// </summary>
        public virtual string ValueName
        {
            get
            {
                return GetStringValue("ValueName", "");
            }
            set
            {
                SetValue("ValueName", value);
            }
        }


        /// <summary>
        /// Value GUID.
        /// </summary>
        public virtual Guid ValueGUID
        {
            get
            {
                return GetGuidValue("ValueGUID", Guid.Empty);
            }
            set
            {
                SetValue("ValueGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime ValueLastModified
        {
            get
            {
                return GetDateTimeValue("ValueLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ValueLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ReportValueInfoProvider.DeleteReportValueInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ReportValueInfoProvider.SetReportValueInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportValueInfo object.
        /// </summary>
        public ReportValueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportValueInfo object from the given DataRow.
        /// </summary>
        public ReportValueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}