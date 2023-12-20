using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportGraphInfo), ReportGraphInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// ReportGraphInfo data container class.
    /// </summary>
    [Serializable]
    public class ReportGraphInfo : AbstractInfo<ReportGraphInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "reporting.reportgraph";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ReportGraphInfoProvider), OBJECT_TYPE, "Reporting.ReportGraph", "GraphID", "GraphLastModified", "GraphGUID", "GraphName", "GraphDisplayName", null, null, "GraphReportID", ReportInfo.OBJECT_TYPE)
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
                    new StructuredField("GraphSettings")
                }
            }
        };

        #endregion


        #region "Variables"

        private ReportCustomData mGraphSettings;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether graph is generated as HTML.
        /// </summary>
        public virtual bool GraphIsHtml
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("GraphIsHtml"), false);
            }
            set
            {
                SetValue("GraphIsHtml", value);
            }
        }


        /// <summary>
        /// Graph ID.
        /// </summary>
        public virtual int GraphID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("GraphID"), 0);
            }
            set
            {
                SetValue("GraphID", value);
            }
        }


        /// <summary>
        /// Graph query.
        /// </summary>
        public virtual string GraphQuery
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphQuery"), "");
            }
            set
            {
                SetValue("GraphQuery", value);
            }
        }


        /// <summary>
        /// Connection string for report graph.
        /// </summary>
        public virtual string GraphConnectionString
        {
            get
            {
                return GetStringValue("GraphConnectionString", "");
            }
            set
            {
                SetValue("GraphConnectionString", value);
            }
        }


        /// <summary>
        /// Graph report ID.
        /// </summary>
        public virtual int GraphReportID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("GraphReportID"), 0);
            }
            set
            {
                SetValue("GraphReportID", value);
            }
        }


        /// <summary>
        /// Graph name (code name).
        /// </summary>
        public virtual string GraphName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphName"), "");
            }
            set
            {
                SetValue("GraphName", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        public virtual string GraphDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphDisplayName"), "");
            }
            set
            {
                SetValue("GraphDisplayName", value);
            }
        }


        /// <summary>
        /// Whether query is stored procedure.
        /// </summary>
        public virtual bool GraphQueryIsStoredProcedure
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("GraphQueryIsStoredProcedure"), false);
            }
            set
            {
                SetValue("GraphQueryIsStoredProcedure", value);
            }
        }


        /// <summary>
        /// Graph type.
        /// </summary>
        public virtual string GraphType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphType"), "");
            }
            set
            {
                SetValue("GraphType", value);
            }
        }


        /// <summary>
        /// Graph title.
        /// </summary>
        public virtual string GraphTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphTitle"), "");
            }
            set
            {
                SetValue("GraphTitle", value);
            }
        }


        /// <summary>
        /// Axis x title.
        /// </summary>
        public virtual string GraphXAxisTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphXAxisTitle"), "");
            }
            set
            {
                SetValue("GraphXAxisTitle", value);
            }
        }


        /// <summary>
        /// Axis y title.
        /// </summary>
        public virtual string GraphYAxisTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("GraphYAxisTitle"), "");
            }
            set
            {
                SetValue("GraphYAxisTitle", value);
            }
        }


        /// <summary>
        /// Graph width.
        /// </summary>
        public virtual int GraphWidth
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("GraphWidth"), 0);
            }
            set
            {
                SetValue("GraphWidth", value);
            }
        }


        /// <summary>
        /// Graph height.
        /// </summary>
        public virtual int GraphHeight
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("GraphHeight"), 0);
            }
            set
            {
                SetValue("GraphHeight", value);
            }
        }


        /// <summary>
        /// Legend position.
        /// </summary>
        public virtual int GraphLegendPosition
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("GraphLegendPosition"), 0);
            }
            set
            {
                SetValue("GraphLegendPosition", value);
            }
        }


        /// <summary>
        /// Graph GUID.
        /// </summary>
        public virtual Guid GraphGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("GraphGUID"), Guid.Empty);
            }
            set
            {
                if (value == Guid.Empty)
                {
                    SetValue("GraphGUID", null);
                }
                else
                {
                    SetValue("GraphGUID", value);
                }
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime GraphLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("GraphLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                if (value == DateTimeHelper.ZERO_TIME)
                {
                    SetValue("GraphLastModified", null);
                }
                else
                {
                    SetValue("GraphLastModified", value);
                }
            }
        }


        /// <summary>
        /// CustomData used for load/store settings which can be used for customization
        /// </summary>
        public virtual ReportCustomData GraphSettings
        {
            get
            {
                return mGraphSettings ?? (mGraphSettings = new ReportCustomData(this, "GraphSettings"));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ReportGraphInfoProvider.DeleteReportGraphInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ReportGraphInfoProvider.SetReportGraphInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportGraphInfo object.
        /// </summary>
        public ReportGraphInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportGraphInfo object from the given DataRow.
        /// </summary>
        public ReportGraphInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}