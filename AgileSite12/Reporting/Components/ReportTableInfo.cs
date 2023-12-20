using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportTableInfo), ReportTableInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// ReportTableInfo data container class.
    /// </summary>
    public class ReportTableInfo : AbstractInfo<ReportTableInfo>
    {
        #region "Variables"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "reporting.reporttable";


        private ContainerCustomData mTableSettings;

        #endregion


        #region "Type information"

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ReportTableInfoProvider), OBJECT_TYPE, "Reporting.ReportTable", "TableID", "TableLastModified", "TableGUID", "TableName", "TableDisplayName", null, null, "TableReportID", ReportInfo.OBJECT_TYPE)
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
                    new StructuredField("TableSettings")
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Table ID.
        /// </summary>
        public virtual int TableID
        {
            get
            {
                return GetIntegerValue("TableID", 0);
            }
            set
            {
                SetValue("TableID", value);
            }
        }


        /// <summary>
        /// Table name (code name).
        /// </summary>
        public virtual string TableName
        {
            get
            {
                return GetStringValue("TableName", "");
            }
            set
            {
                SetValue("TableName", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        public virtual string TableDisplayName
        {
            get
            {
                return GetStringValue("TableDisplayName", "");
            }
            set
            {
                SetValue("TableDisplayName", value);
            }
        }


        /// <summary>
        /// Table query.
        /// </summary>
        public virtual string TableQuery
        {
            get
            {
                return GetStringValue("TableQuery", "");
            }
            set
            {
                SetValue("TableQuery", value);
            }
        }


        /// <summary>
        /// Connection string for report table.
        /// </summary>
        public virtual string TableConnectionString
        {
            get
            {
                return GetStringValue("TableConnectionString", "");
            }
            set
            {
                SetValue("TableConnectionString", value);
            }
        }


        /// <summary>
        /// Indicates whether query is stored procedure.
        /// </summary>
        public virtual bool TableQueryIsStoredProcedure
        {
            get
            {
                return GetBooleanValue("TableQueryIsStoredProcedure", false);
            }
            set
            {
                SetValue("TableQueryIsStoredProcedure", value);
            }
        }


        /// <summary>
        /// Table report ID.
        /// </summary>
        public virtual int TableReportID
        {
            get
            {
                return GetIntegerValue("TableReportID", 0);
            }
            set
            {
                SetValue("TableReportID", value);
            }
        }


        /// <summary>
        /// Table GUID.
        /// </summary>
        public virtual Guid TableGUID
        {
            get
            {
                return GetGuidValue("TableGUID", Guid.Empty);
            }
            set
            {
                SetValue("TableGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime TableLastModified
        {
            get
            {
                return GetDateTimeValue("TableLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("TableLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// CustomData used for load/store settings which can be used for customization
        /// </summary>
        public virtual ContainerCustomData TableSettings
        {
            get
            {
                return mTableSettings ?? (mTableSettings = new ContainerCustomData(this, "TableSettings"));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ReportTableInfoProvider.DeleteReportTableInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ReportTableInfoProvider.SetReportTableInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportTableInfo object.
        /// </summary>
        public ReportTableInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportTableInfo object from the given DataRow.
        /// </summary>
        public ReportTableInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}