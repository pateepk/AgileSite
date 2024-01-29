using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Handles special actions during the board import process.
    /// </summary>
    internal static class BoardImport
    {
        #region "Methods"

        /// <summary>
        /// Initializes import handlers
        /// </summary>
        public static void Init()
        {
            ImportExportEvents.ImportObjectType.After += Import_After;
        }


        private static void Import_After(object sender, ImportDataEventArgs e)
        {
            var objectType = e.ObjectType;
            if (objectType == BoardInfo.OBJECT_TYPE)
            {
                var settings = e.Settings;
                var data = e.Data;

                // There is no data for this object type
                if (DataHelper.DataSourceIsEmpty(data) || DataHelper.DataSourceIsEmpty(data.Tables["Board_Board"]))
                {
                    return;
                }

                // Board messages
                DataTable table = data.Tables["Board_Board"];
                ImportBoardMessages(settings, table, e.TranslationHelper);
            }
        }


        /// <summary>
        /// Import board messages.
        /// </summary>
        /// <param name="settings">Import settings</param>
        /// <param name="table">Parent data</param>
        /// <param name="th">Translation helper</param>
        private static void ImportBoardMessages(SiteImportSettings settings, DataTable table, TranslationHelper th)
        {
            ProcessObjectEnum processType = settings.GetObjectsProcessType(BoardInfo.OBJECT_TYPE, true);
            if (processType == ProcessObjectEnum.None)
            {
                return;
            }

            // Check import settings
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BOARD_MESSAGES), true) || settings.ExistingSite)
            {
                return;
            }

            foreach (DataRow dr in table.Rows)
            {
                // Process canceled
                if (settings.ProcessCanceled)
                {
                    ImportProvider.ImportCanceled();
                }

                // Get board name
                string boardName = dr["BoardName"].ToString();
                string boardDisplayName = dr["BoardDisplayName"].ToString();
                string boardGuid = dr["BoardGUID"].ToString();

                try
                {
                    // Check if board selected
                    if ((processType != ProcessObjectEnum.All) && !settings.IsSelected(BoardInfo.OBJECT_TYPE, boardName, true))
                    {
                        continue;
                    }

                    // Log progress
                    ImportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ImportSite.ImportingBoardMessages", "Importing board '{0}' messages"), HTMLHelper.HTMLEncode(boardDisplayName)));

                    // Save the settings progress
                    settings.SavePersistentLog();

                    // Initialize data
                    string boardObjectType = ImportExportHelper.BOARDMESSAGE_PREFIX + boardGuid;

                    // Get data
                    DataSet ds = ImportProvider.LoadObjects(settings, boardObjectType, true);
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        // Import the objects
                        ImportProvider.ImportObjects(settings, ds, BoardMessageInfo.OBJECT_TYPE, false, th, true, ProcessObjectEnum.All, null);
                    }
                }
                catch (ProcessCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Log exception
                    ImportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ImportSite.ErrorImportingBoardMessages", "Error importing board '{0}' messages."), HTMLHelper.HTMLEncode(boardDisplayName)), ex);
                    throw;
                }
            }
        }

        #endregion
    }
}