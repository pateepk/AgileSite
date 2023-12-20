using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.CMSImportExport;

[assembly: RegisterObjectType(typeof(ExportHistoryInfo), ExportHistoryInfo.OBJECT_TYPE)]

namespace CMS.CMSImportExport
{
    /// <summary>
    /// ExportHistoryInfo data container class.
    /// </summary>
    public class ExportHistoryInfo : AbstractInfo<ExportHistoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "export.history";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ExportHistoryInfoProvider), OBJECT_TYPE, "Export.History", "ExportID", null, null, null, "ExportFileName", null, "ExportSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ExportUserID", PredefinedObjectType.USER) },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsCloning = false,
            ContainsMacros = false,
            SupportsGlobalObjects = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Date of the export history.
        /// </summary>
        public virtual DateTime ExportDateTime
        {
            get
            {
                return GetDateTimeValue("ExportDateTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ExportDateTime", value);
            }
        }


        /// <summary>
        /// File name of the export history.
        /// </summary>
        public virtual string ExportFileName
        {
            get
            {
                return GetStringValue("ExportFileName", "");
            }
            set
            {
                SetValue("ExportFileName", value);
            }
        }


        /// <summary>
        /// Site ID of the exported site.
        /// </summary>
        public virtual int ExportSiteID
        {
            get
            {
                return GetIntegerValue("ExportSiteID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("ExportSiteID", value);
                }
                else
                {
                    SetValue("ExportSiteID", null);
                }
            }
        }


        /// <summary>
        /// Export settings used during the export process.
        /// </summary>
        public virtual string ExportSettings
        {
            get
            {
                return GetStringValue("ExportSettings", "");
            }
            set
            {
                SetValue("ExportSettings", value);
            }
        }


        /// <summary>
        /// ID of the user who performed the export.
        /// </summary>
        public virtual int ExportUserID
        {
            get
            {
                return GetIntegerValue("ExportUserID", 0);
            }
            set
            {
                SetValue("ExportUserID", value);
            }
        }


        /// <summary>
        /// Export history ID.
        /// </summary>
        public virtual int ExportID
        {
            get
            {
                return GetIntegerValue("ExportID", 0);
            }
            set
            {
                SetValue("ExportID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ExportHistoryInfoProvider.DeleteExportHistoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ExportHistoryInfoProvider.SetExportHistoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ExportHistoryInfo object.
        /// </summary>
        public ExportHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ExportHistoryInfo object from the given DataRow.
        /// </summary>
        public ExportHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}