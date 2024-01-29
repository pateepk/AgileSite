using System;
using System.Data;

using CMS.Base;
using CMS.IO;
using CMS.Helpers;
using CMS.DataEngine;

using IOExceptions = System.IO;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Manual translations provider.
    /// </summary>
    public class ManualTranslationService : AbstractHumanTranslationService
    {
        #region "Properties"

        /// <summary>
        /// Folder path where translation submission are exported.
        /// </summary>
        public string ExportFolder
        {
            get
            {
                string folder = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSManualTranslationExportFolder");
                if (string.IsNullOrEmpty(folder))
                {
                    folder = "~/App_Data/Translations/Export/";
                }
                else if (Path.IsPathRooted(folder))
                {
                    throw new NotSupportedException("[ManualTranslationService.ExportFolder]: '" + folder + "' is a physical path, but a virtual path was expected.");
                }
                return URLHelper.GetPhysicalPath(folder);
            }
        }


        /// <summary>
        /// Folder path where the translated submission are retrieved from.
        /// </summary>
        public string ImportFolder
        {
            get
            {
                string folder = SettingsKeyInfoProvider.GetValue(SiteName + ".CMSManualTranslationImportFolder");
                if (string.IsNullOrEmpty(folder))
                {
                    folder = "~/App_Data/Translations/Import/";
                }
                else if (Path.IsPathRooted(folder))
                {
                    throw new NotSupportedException("[ManualTranslationService.ImportFolder]: '" + folder + "' is a physical path, but a virtual path was expected.");
                }
                return URLHelper.GetPhysicalPath(folder);
            }
        }


        /// <summary>
        /// If true, ZIP file with translated submission is deleted once the XLIFF files from within the ZIP file are downloaded into the submission item queue.
        /// </summary>
        public bool DeleteAfterSuccessfulDownload
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue(SiteName + ".CMSManualTranslationDeleteSuccessfulSubmissions");
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Checks if everything required to run the service is in the settings of the service.
        /// </summary>
        public override bool IsAvailable()
        {
            // This service needs only paths, but if nothing is in settings, default values are used
            return true;
        }


        /// <summary>
        /// Checks if target language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public override bool IsTargetLanguageSupported(string langCode)
        {
            // All languages are supported
            return true;
        }


        /// <summary>
        /// Checks if source language is supported within the service
        /// </summary>
        /// <param name="langCode">Code of the culture</param>
        public override bool IsSourceLanguageSupported(string langCode)
        {
            // All languages are supported
            return true;
        }


        /// <summary>
        /// Creates new submission (or resubmits existing if submission ticket is present).
        /// </summary>
        /// <param name="submission">Submission object</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="submission"/> is <c>null</c></exception>
        public override string CreateSubmission(TranslationSubmissionInfo submission)
        {
            if (submission == null)
            {
                throw new ArgumentNullException("submission");
            }

            try
            {
                // Get the unique path to the file
                string path;
                if (string.IsNullOrEmpty(submission.SubmissionTicket))
                {
                    path = Path.Combine(ExportFolder, TranslationServiceHelper.GetSubmissionFileName(submission));
                    path = FileHelper.GetUniqueFileName(path);

                    // Submission ticket is zip file name
                    submission.SubmissionTicket = Path.GetFileName(path);
                }
                else
                {
                    // Resubmit action - uses the same path, overwrites the file if exists
                    path = Path.Combine(ExportFolder, submission.SubmissionTicket);
                }

                // Write the file
                DirectoryHelper.EnsureDiskPath(path, SystemContext.WebApplicationPhysicalPath);
                using (var stream = File.Create(path))
                {
                    TranslationServiceHelper.WriteSubmissionInZIP(submission, stream);
                }
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return ex.Message;
            }

            return null;
        }


        /// <summary>
        /// Cancels given submission.
        /// </summary>
        /// <param name="submission">Submission to cancel</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="submission"/> is <c>null</c></exception>
        public override string CancelSubmission(TranslationSubmissionInfo submission)
        {
            if (submission == null)
            {
                throw new ArgumentNullException("submission");
            }

            try
            {
                // Try to delete the zip file (path is saved as the submission ticket)
                string path = Path.Combine(ExportFolder, submission.SubmissionTicket);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return ex.Message;
            }

            return null;
        }


        /// <summary>
        /// Retrieves completed XLIFF files from the service and processes them (imports them into the system). Returns empty string if everything went well.
        /// </summary>
        /// <param name="siteName">Name of site for which this method downloads completed XLIFF files.</param>
        public override string DownloadCompletedTranslations(string siteName)
        {
            const string DOWNLOAD_COMPLETED_CODE = "DOWNLOADCOMPLETED";

            try
            {
                if (!Directory.Exists(ImportFolder))
                {
                    return null;
                }

                string[] files = Directory.GetFiles(ImportFolder, "*.zip");
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);

                    // Get the submissions which belongs to this zip file (the path is stored as a SubmissionTicket).
                    DataSet ds = TranslationSubmissionInfoProvider.GetTranslationSubmissions().WhereEquals("SubmissionTicket", fileName);
                    if (DataHelper.DataSourceIsEmpty(ds))
                    {
                        TranslationServiceHelper.LogWarning(DOWNLOAD_COMPLETED_CODE, 
                            String.Format("File '{0}' does not belong to any translation submission.", fileName));

                        continue;
                    }

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Translate only Waiting for translation
                        var submission = new TranslationSubmissionInfo(dr);
                        if (submission.SubmissionStatus != TranslationStatusEnum.WaitingForTranslation)
                        {
                            continue;
                        }

                        // Get the zip name
                        string zipFileName = submission.SubmissionTicket;
                        string zipPackagePath = Path.Combine(ImportFolder, zipFileName);
                        if (!File.Exists(zipPackagePath))
                        {
                            TranslationServiceHelper.LogWarning(DOWNLOAD_COMPLETED_CODE, 
                                String.Format("File '{0}' does not exists.", zipPackagePath));

                            continue;
                        }

                        string err;
                        using (var stream = FileStream.New(zipPackagePath, FileMode.Open, FileAccess.Read))
                        {
                            // Import XLIFF files from zip
                            err = TranslationServiceHelper.ImportXLIFFfromZIP(submission, stream);
                        }

                        if (!string.IsNullOrEmpty(err))
                        {
                            return err;
                        }

                        // Change the status to Translation ready and save the submission
                        submission.SubmissionStatus = TranslationStatusEnum.TranslationReady;
                        TranslationSubmissionInfoProvider.SetTranslationSubmissionInfo(submission);

                        // Auto import if enabled
                        err = TranslationServiceHelper.AutoImportSubmission(submission);
                        if (!string.IsNullOrEmpty(err))
                        {
                            return err;
                        }

                        if (DeleteAfterSuccessfulDownload)
                        {
                            File.Delete(zipPackagePath);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                TranslationServiceHelper.LogEvent(ex);
                return ex.Message;
            }
        }

        #endregion
    }
}