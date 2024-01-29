using System;
using System.Data;

using CMS.CMSImportExport;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Handles special actions during the board export process.
    /// </summary>
    internal static class BoardExport
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.ExportObjects.After += Export_After;
        }


        private static void Export_After(object sender, ExportEventArgs e)
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
                ExportBoardMessages(settings, table);
            }
        }


        /// <summary>
        /// Export board messages.
        /// </summary>
        /// <param name="settings">Export settings</param>
        /// <param name="table">Parent data</param>
        private static void ExportBoardMessages(SiteExportSettings settings, DataTable table)
        {
            // Check export setting
            if (!ValidationHelper.GetBoolean(settings.GetSettings(ImportExportHelper.SETTINGS_BOARD_MESSAGES), true))
            {
                return;
            }

            // Get message object
            GeneralizedInfo messageObj = ModuleManager.GetReadOnlyObject(BoardMessageInfo.OBJECT_TYPE);
            if (messageObj != null)
            {
                foreach (DataRow dr in table.Rows)
                {
                    // Export process canceled
                    if (settings.ProcessCanceled)
                    {
                        ExportProvider.ExportCanceled();
                    }

                    // Get board name
                    string boardDisplayName = ValidationHelper.GetString(dr["BoardDisplayName"], "");
                    string boardGuid = ValidationHelper.GetString(dr["BoardGUID"], "");

                    // Log progress
                    ExportProvider.LogProgress(LogStatusEnum.Info, settings, string.Format(settings.GetAPIString("ExportSite.ExportingBoardMessages", "Exporting board '{0}' messages"), HTMLHelper.HTMLEncode(boardDisplayName)));

                    // Save the settings
                    settings.SavePersistentLog();

                    try
                    {
                        // Initialize data
                        int boardId = ValidationHelper.GetInteger(dr["BoardID"], 0);
                        string boardObjectType = ImportExportHelper.BOARDMESSAGE_PREFIX + boardGuid;

                        // Get board messages data
                        DataSet ds = messageObj.GetData(null, "MessageBoardID = " + boardId, null, -1);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            ds.Tables[0].TableName = "Board_Message";

                            // Save data
                            ExportProvider.SaveObjects(settings, ds, boardObjectType, true);
                        }
                    }
                    catch (ProcessCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                        ExportProvider.LogProgressError(settings, string.Format(settings.GetAPIString("ExportSite.ErrorExportingBoardMessages", "Error exporting board '{0}' messages."), HTMLHelper.HTMLEncode(boardDisplayName)), ex);
                        throw;
                    }
                }
            }
        }

        #endregion
    }
}