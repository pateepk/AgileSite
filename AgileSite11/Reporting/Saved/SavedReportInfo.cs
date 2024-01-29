using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(SavedReportInfo), SavedReportInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// SavedReportInfo data container class.
    /// </summary>
    public class SavedReportInfo : AbstractInfo<SavedReportInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "reporting.savedreport";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SavedReportInfoProvider), OBJECT_TYPE, "Reporting.SavedReport", "SavedReportID", "SavedReportLastModified", "SavedReportGUID", null, "SavedReportTitle", null, null, "SavedReportReportID", ReportInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency> { new ObjectDependency("SavedReportCreatedByUserID", UserInfo.OBJECT_TYPE) },
            ModuleName = "cms.reporting",
            SupportsCloning = false,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                AllowSingleExport = true,
                IsExportable = true,
                LogExport = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("SavedReportParameters")
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Saved report HTML.
        /// </summary>
        public virtual string SavedReportHTML
        {
            get
            {
                return GetStringValue("SavedReportHTML", "");
            }
            set
            {
                SetValue("SavedReportHTML", value);
            }
        }


        /// <summary>
        /// Saved report GUID.
        /// </summary>
        public virtual Guid SavedReportGUID
        {
            get
            {
                return GetGuidValue("SavedReportGUID", Guid.Empty);
            }
            set
            {
                SetValue("SavedReportGUID", value);
            }
        }


        /// <summary>
        /// Report ID.
        /// </summary>
        public virtual int SavedReportReportID
        {
            get
            {
                return GetIntegerValue("SavedReportReportID", 0);
            }
            set
            {
                SetValue("SavedReportReportID", value);
            }
        }


        /// <summary>
        /// Saved report date.
        /// </summary>
        public virtual DateTime SavedReportDate
        {
            get
            {
                return GetDateTimeValue("SavedReportDate", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SavedReportDate", value);
            }
        }


        /// <summary>
        /// Saved report title.
        /// </summary>
        public virtual string SavedReportTitle
        {
            get
            {
                return GetStringValue("SavedReportTitle", "");
            }
            set
            {
                SetValue("SavedReportTitle", value);
            }
        }


        /// <summary>
        /// Created by user ID.
        /// </summary>
        public virtual int SavedReportCreatedByUserID
        {
            get
            {
                return GetIntegerValue("SavedReportCreatedByUserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("SavedReportCreatedByUserID", null);
                }
                else
                {
                    SetValue("SavedReportCreatedByUserID", value);
                }
            }
        }


        /// <summary>
        /// Saved report ID.
        /// </summary>
        public virtual int SavedReportID
        {
            get
            {
                return GetIntegerValue("SavedReportID", 0);
            }
            set
            {
                SetValue("SavedReportID", value);
            }
        }


        /// <summary>
        /// Saved report parameters.
        /// </summary>
        public virtual string SavedReportParameters
        {
            get
            {
                return GetStringValue("SavedReportParameters", "");
            }
            set
            {
                SetValue("SavedReportParameters", value);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime SavedReportLastModified
        {
            get
            {
                return GetDateTimeValue("SavedReportLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SavedReportLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SavedReportInfoProvider.DeleteSavedReportInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SavedReportInfoProvider.SetSavedReportInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SavedReportInfo object.
        /// </summary>
        public SavedReportInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SavedReportInfo object from the given DataRow.
        /// </summary>
        public SavedReportInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}