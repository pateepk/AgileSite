using System;

using CMS.EventLog;
using CMS.LicenseProvider;

using ProcessStatus = CMS.Base.ProcessStatus;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export manager class.
    /// </summary>
    public class ExportManager
    {
        #region "Variables"

        private ProcessStatus mExportStatus = ProcessStatus.Restarted;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Export settings.
        /// </summary>
        public SiteExportSettings Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Export status.
        /// </summary>
        public ProcessStatus ExportStatus
        {
            get
            {
                return mExportStatus;
            }
            set
            {
                mExportStatus = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates export manager.
        /// </summary>
        /// <param name="settings">Export settings</param>
        public ExportManager(SiteExportSettings settings)
        {
            Settings = settings;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs export process.
        /// </summary>
        /// <param name="parameter">Export parameter</param>
        public void Export(object parameter)
        {
            LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => ExportInternal());
        }


        [CanDisableLicenseCheck("FOk3FHac0x+npjuFotTAS2KWL7IPwWrQfk0An7JhLCE1QHYO0JhbO1ipnSEqtXgRDycAoevL04lZDL6FUKDH2A==")]
        private void ExportInternal()
        {
            try
            {
                mExportStatus = ProcessStatus.Running;

                // Export the data
                ExportProvider.ExportObjectsData(Settings);

                mExportStatus = ProcessStatus.Finished;
            }
            catch (Exception ex)
            {
                mExportStatus = ProcessStatus.Error;

                EventLogProvider.LogException("Export", "ExportObjects", ex);
            }
        }

        #endregion
    }
}