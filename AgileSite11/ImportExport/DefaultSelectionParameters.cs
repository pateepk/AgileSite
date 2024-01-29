using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Parameters for loading default selection to import/export settings.
    /// </summary>
    public sealed class DefaultSelectionParameters
    {
        #region "Variables"

        private bool mSiteObjects = true;
        private ImportTypeEnum mImportType = ImportTypeEnum.None;
        private ExportTypeEnum mExportType = ExportTypeEnum.None;
        private bool mLoadObjects = true;
        private bool mLoadTasks = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Filter current where condition.
        /// </summary>
        public string FilterCurrentWhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Type of the object to load.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the object is site dependent. Default value is true.
        /// </summary>
        public bool SiteObjects
        {
            get
            {
                return mSiteObjects;
            }
            set
            {
                mSiteObjects = value;
            }
        }


        /// <summary>
        /// Type of the import. Default value is ImportTypeEnum.None.
        /// </summary>
        public ImportTypeEnum ImportType
        {
            get
            {
                return mImportType;
            }
            set
            {
                mImportType = value;
            }
        }


        /// <summary>
        /// Type of the export. Default value is ExportTypeEnum.None.
        /// </summary>
        public ExportTypeEnum ExportType
        {
            get
            {
                return mExportType;
            }
            set
            {
                mExportType = value;
            }
        }


        /// <summary>
        /// If true, the objects are loaded. Default value is true.
        /// </summary>
        public bool LoadObjects
        {
            get
            {
                return mLoadObjects;
            }
            set
            {
                mLoadObjects = value;
            }
        }


        /// <summary>
        /// If true, the tasks are loaded. Default value is true.
        /// </summary>
        public bool LoadTasks
        {
            get
            {
                return mLoadTasks;
            }
            set
            {
                mLoadTasks = value;
            }
        }


        /// <summary>
        /// If true, progress log is cleared before loading.  Default value is false.
        /// </summary>
        public bool ClearProgressLog
        {
            get;
            set;
        }

        #endregion
    }
}
