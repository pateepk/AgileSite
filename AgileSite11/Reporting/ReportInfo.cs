using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportInfo), ReportInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// ReportInfo data container class.
    /// </summary>
    public class ReportInfo : AbstractInfo<ReportInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.REPORT;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ReportInfoProvider), OBJECT_TYPE, "Reporting.Report", "ReportID", "ReportLastModified", "ReportGUID", "ReportName", "ReportDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ReportCategoryID", ReportCategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            DeleteObjectWithAPI = true,
            ModuleName = "cms.reporting",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                LogExport = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            DefaultData = new DefaultDataSettings
            {
                ExcludedPrefixes = { "custom" }
            },
            FormDefinitionColumn = "ReportParameters",
            HasMetaFiles = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("ReportParameters")
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Report layout.
        /// </summary>
        public virtual string ReportLayout
        {
            get
            {
                return GetStringValue("ReportLayout", "");
            }
            set
            {
                SetValue("ReportLayout", value);
            }
        }


        /// <summary>
        /// Report ID.
        /// </summary>
        public virtual int ReportID
        {
            get
            {
                return GetIntegerValue("ReportID", 0);
            }
            set
            {
                SetValue("ReportID", value);
            }
        }


        /// <summary>
        /// If true, report's subscription is enabled.
        /// </summary>
        public virtual bool ReportEnableSubscription
        {
            get
            {
                return GetBooleanValue("ReportEnableSubscription", true);
            }
            set
            {
                SetValue("ReportEnableSubscription", value);
            }
        }


        /// <summary>
        /// Report parameters.
        /// </summary>
        public virtual string ReportParameters
        {
            get
            {
                return GetStringValue("ReportParameters", "");
            }
            set
            {
                SetValue("ReportParameters", value);
            }
        }


        /// <summary>
        /// Report name (code name).
        /// </summary>
        public virtual string ReportName
        {
            get
            {
                return GetStringValue("ReportName", "");
            }
            set
            {
                SetValue("ReportName", value);
            }
        }


        /// <summary>
        /// Report display name.
        /// </summary>
        public virtual string ReportDisplayName
        {
            get
            {
                return GetStringValue("ReportDisplayName", "");
            }
            set
            {
                SetValue("ReportDisplayName", value);
            }
        }


        /// <summary>
        /// Report category ID.
        /// </summary>
        public virtual int ReportCategoryID
        {
            get
            {
                return GetIntegerValue("ReportCategoryID", 0);
            }
            set
            {
                SetValue("ReportCategoryID", value);
            }
        }


        /// <summary>
        /// Access bit array.
        /// </summary>
        public virtual ReportAccessEnum ReportAccess
        {
            get
            {
                int intEnum = ValidationHelper.GetInteger(GetValue("ReportAccess"), 1);
                return (ReportAccessEnum)Enum.ToObject(typeof(ReportAccessEnum), intEnum);
            }
            set
            {
                SetValue("ReportAccess", (int)value);
            }
        }


        /// <summary>
        /// Connection string for report.
        /// </summary>
        public virtual string ReportConnectionString
        {
            get
            {
                return GetStringValue("ReportConnectionString", "");
            }
            set
            {
                SetValue("ReportConnectionString", value);
            }
        }


        /// <summary>
        /// Report GUID.
        /// </summary>
        public virtual Guid ReportGUID
        {
            get
            {
                return GetGuidValue("ReportGUID", Guid.Empty);
            }
            set
            {
                SetValue("ReportGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime ReportLastModified
        {
            get
            {
                return GetDateTimeValue("ReportLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ReportLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ReportInfoProvider.DeleteReportInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ReportInfoProvider.SetReportInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Set category
            bool subscriptions = false;
            bool savedreports = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                ReportCategoryID = ValidationHelper.GetInteger(p[PredefinedObjectType.REPORT + ".categoryid"], 0);

                savedreports = ValidationHelper.GetBoolean(p[PredefinedObjectType.REPORT + ".savedreports"], false);
                subscriptions = ValidationHelper.GetBoolean(p[PredefinedObjectType.REPORT + ".subscriptions"], false);
            }

            // Clone subscriptions if requested
            if (subscriptions)
            {
                if (settings.ExcludedChildTypes.Contains(ReportSubscriptionInfo.OBJECT_TYPE))
                {
                    settings.ExcludedChildTypes.Remove(ReportSubscriptionInfo.OBJECT_TYPE);
                }
            }
            else
            {
                settings.ExcludedChildTypes.Add(ReportSubscriptionInfo.OBJECT_TYPE);
            }

            // Clone saved reports if requested
            if (savedreports)
            {
                if (settings.ExcludedChildTypes.Contains(SavedReportInfo.OBJECT_TYPE))
                {
                    settings.ExcludedChildTypes.Remove(SavedReportInfo.OBJECT_TYPE);
                }
            }
            else
            {
                settings.ExcludedChildTypes.Add(SavedReportInfo.OBJECT_TYPE);
            }

            var originalReportName = ValidationHelper.GetString(GetOriginalValue("ReportName"), ReportName);
            ReportLayout = ReplaceReportNameInComponents(ReportLayout, originalReportName, ReportName);

            Insert();
        }


        private string ReplaceReportNameInComponents(string reportLayout, string originalReportName, string reportName)
        {
            var regEx = RegexHelper.GetRegex("(%%control:Report(?:Graph|HtmlGraph|Value|Table)\\?)" + Regex.Escape(originalReportName + "."), true);

            return regEx.Replace(reportLayout, "$1" + reportName + ".");
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportInfo object.
        /// </summary>
        public ReportInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportInfo object from the given DataRow.
        /// </summary>
        public ReportInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}