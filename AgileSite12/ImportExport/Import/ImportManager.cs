using System;
using System.Threading;

using CMS.Base;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.IO;
using CMS.LicenseProvider;

using ProcessStatus = CMS.Base.ProcessStatus;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class representing import manager.
    /// </summary>
    public class ImportManager
    {
        #region "Variables"

        private ProcessStatus mImportStatus = ProcessStatus.Restarted;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if exception should be thrown on error.
        /// </summary>
        public bool ThrowExceptionOnError
        {
            get;
            set;
        }


        /// <summary>
        /// Import settings.
        /// </summary>
        public SiteImportSettings Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Import status.
        /// </summary>
        public ProcessStatus ImportStatus
        {
            get
            {
                return mImportStatus;
            }
            set
            {
                mImportStatus = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates import manager.
        /// </summary>
        /// <param name="settings">Import settings</param>
        public ImportManager(SiteImportSettings settings)
        {
            Settings = settings;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs import process.
        /// </summary>
        /// <param name="parameter">Import parameter</param>
        public void Import(object parameter)
        {
            LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => ImportInternal());
        }


        [CanDisableLicenseCheck("CB0M9krYwaQCnopbpWW80YIYub/7X+9rIChWwRgSvcOe+JkPPqYD34tW2M7tNBkTUj5HV2NEX6wgOO9exkuDmw==")]
        private void ImportInternal()
        {
            try
            {
                mImportStatus = ProcessStatus.Running;

                // Execute import
                ImportProvider.ImportObjectsData(Settings);

                // Delete temporary files only if there are no warnings
                if (!Settings.IsWarning() && ValidationHelper.GetBoolean(Settings.GetSettings(ImportExportHelper.SETTINGS_DELETE_TEMPORARY_FILES), true))
                {
                    ImportProvider.DeleteTemporaryFiles(Settings, true);
                }

                mImportStatus = ProcessStatus.Finished;

                if (ThrowExceptionOnError && Settings.IsWarning())
                {
                    Thread.CurrentThread.Abort(CMSThread.ABORT_REASON_STOP);
                }
            }
            catch (RunningSiteException)
            {
                mImportStatus = ProcessStatus.Error;

                // Delete temporary files only if there are no warnings
                if (!Settings.IsWarning() && ValidationHelper.GetBoolean(Settings.GetSettings(ImportExportHelper.SETTINGS_DELETE_TEMPORARY_FILES), true))
                {
                    ImportProvider.DeleteTemporaryFiles(Settings, true);
                }

                if (ThrowExceptionOnError)
                {
                    Thread.CurrentThread.Abort(CMSThread.ABORT_REASON_STOP);
                }
            }
            catch (Exception)
            {
                mImportStatus = ProcessStatus.Error;

                if (ThrowExceptionOnError)
                {
                    Thread.CurrentThread.Abort(CMSThread.ABORT_REASON_STOP);
                }
            }
            finally
            {
                // Dispose the zip storage provider to release memory and the file
                string path = Settings.TemporaryFilesPath;
                if (ZipStorageProvider.IsZipFolderPath(path))
                {
                    ZipStorageProvider.Dispose(path);
                }
            }
        }
        
        #endregion
    }
}