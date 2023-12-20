using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;

namespace CMS.IO
{
    /// <summary>
    /// File debug methods
    /// </summary>
    public class FileDebug
    {
        #region "Variables"

        private static readonly CMSLazy<DebugSettings> mSettings = new CMSLazy<DebugSettings>(GetDebugSettings);

        #endregion


        #region "Properties"

        /// <summary>
        /// Debug settings
        /// </summary>
        public static DebugSettings Settings
        {
            get
            {
                return mSettings.Value;
            }
        }


        /// <summary>
        /// Current request log.
        /// </summary>
        public static RequestLog CurrentRequestLog
        {
            get
            {
                return DebugContext.CurrentRequestLogs.EnsureLog(Settings, NewLogTable);
            }
            set
            {
                DebugContext.CurrentRequestLogs[Settings] = value;
            }
        }


        /// <summary>
        /// Debug current request files access.
        /// </summary>
        public static bool DebugCurrentRequest
        {
            get
            {
                if (Settings.LogOperations)
                {
                    return DebugContext.CurrentRequestSettings[Settings];
                }
                else
                {
                    return false;
                }
            }
            set
            {
                DebugContext.CurrentRequestSettings[Settings] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the debug settings
        /// </summary>
        private static DebugSettings GetDebugSettings()
        {
            return new DebugSettings("Files")
            {
                SizeColumn = "FileSize",
                FinalizeData = FinalizeData,
                LogControl = "~/CMSAdminControls/Debug/FilesLog.ascx"
            };
        }


        /// <summary>
        /// Groups read/write data in the table.
        /// </summary>
        /// <param name="dt">DataTable to process</param>
        public static void FinalizeData(DataTable dt)
        {
            if (dt != null)
            {
                var cs = CoreServices.Conversion;
                var fileOperationMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lock (dt)
                    {
                        DataRow dr = dt.Rows[i];

                        string operation = cs.GetString(dr["FileOperation"], "").ToLowerInvariant();
                        string path = cs.GetString(dr["FilePath"], "");

                        // Index of last open operation of this file
                        int fileOperationRowIndex = GetFileOperationRecordIndex(path, operation, fileOperationMappings);
                        // Index of the record of the same operation that occurred previously
                        int openFileOperationRowIndex = GetFileOperationRecordIndex(path, "open", fileOperationMappings);

                        // Row with record of same operation that occurred previously
                        DataRow fileOperationRow = null;
                        // Row with record of last open operation
                        DataRow openFileOperationRow = openFileOperationRowIndex > -1 ? dt.Rows[openFileOperationRowIndex] : null;

                        // Ignore operations that happened before file was open.
                        if ((fileOperationRowIndex > openFileOperationRowIndex) && (fileOperationRowIndex >= 0))
                        {
                            fileOperationRow = dt.Rows[fileOperationRowIndex];
                        }
                
                        // Set read/write operation indicator
                        bool isRW = (operation.StartsWith("write", StringComparison.Ordinal) || operation.StartsWith("read", StringComparison.Ordinal) || operation.StartsWith("append", StringComparison.Ordinal));
                                       
                        // Check if file operation is already registered 
                        if (fileOperationRow != null)
                        {
                            // Group write and read operations together (sum up size and number of read/writes)
                            if (isRW)
                            {
                                fileOperationRow["FileAccesses"] = cs.GetInteger(fileOperationRow["FileAccesses"], 0) + 1;
                                fileOperationRow["FileSize"] = cs.GetInteger(dr["FileSize"], 0) + cs.GetInteger(dr["FileSize"], 0);

                                dt.Rows.RemoveAt(i);
                                i--;                       
                            }
                            // Remove duplicate close operations (can happen when you call close manually and then dispose is called, i.e. withing an using block).
                            else if (operation == "close")
                            {
                                dt.Rows.RemoveAt(i);
                                i--;
                            }
                        }
                        // Mark file as closed
                        else if ((operation == "close") && (openFileOperationRow != null))
                        {
                            openFileOperationRow["FileNotClosed"] = false;
                        }
                        // Operation occurred for the first time since file was lastly opened
                        // Register or override previous file operation
                        else if ((operation == "open") || isRW)
                        {
                            AddFileOperationRecordIndex(path, operation, i, fileOperationMappings);

                            if (isRW && cs.GetInteger(dr["FileAccesses"], 0) == 0)
                            {
                                // Init file accesses to 1
                                dr["FileAccesses"] = 1;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Logs the file operation. Logs the file operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="fileOperation">Operation with file (open, close, read, write)</param>
        /// <param name="providerName">Provider name</param>
        [HideFromDebugContext]
        public static DataRow LogFileOperation(string filePath, string fileOperation, string providerName)
        {
            return LogFileOperation(filePath, fileOperation, -1, String.Empty, String.Empty, providerName);
        }


        /// <summary>
        /// Logs the file operation. Logs the file operation to the file and to current request log for debugging.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="fileOperation">Operation with file (open, close, read, write)</param>
        /// <param name="size">Size of the read / write operation in bytes</param>
        /// <param name="text">Text written to the file</param>
        /// <param name="parameters">Parameters of the operation (i.e. access mode of open operation)</param>
        /// <param name="providerName">Provider name</param>
        [HideFromDebugContext]
        public static DataRow LogFileOperation(string filePath, string fileOperation, int size = -1, string text = null, string parameters = null, string providerName = null)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                if (DebugCurrentRequest)
                {
                    // Log to the file
                    LogToFile(filePath, fileOperation);

                    if (Settings.Enabled)
                    {
                        return CurrentRequestLog.LogNewItem(dr =>
                        {
                            // Prepare parameters string
                            dr["FilePath"] = Path.EnsureBackslashes(filePath);
                            dr["FileOperation"] = fileOperation;
                            dr["FileSize"] = size;
                            dr["FileParameters"] = parameters;
                            if (text != null)
                            {
                                dr["FileText"] = (text.Length > 120 ? text.Remove(120) + " ..." : text);
                            }
                            dr["FileNotClosed"] = true;

                            // Get provider name if not provided
                            if (String.IsNullOrEmpty(providerName) && !String.IsNullOrEmpty(filePath))
                            {
                                providerName = String.Join("|", filePath.Split('|').Select(GetProviderName));
                            }

                            dr["ProviderName"] = providerName;

                            // Context
                            if ((fileOperation != "write") && (fileOperation != "read"))
                            {
                                dr["Context"] = DebugHelper.GetContext(Settings.Stack);
                            }
                            dr["Counter"] = DebugHelper.GetDebugCounter(true);
                            dr["Time"] = DateTime.Now;

                            DebugEvents.IODebugItemLogged.StartEvent(dr);
                        });
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the provider name for the given path
        /// </summary>
        /// <param name="filePath">File path</param>
        private static string GetProviderName(string filePath)
        {
            string providerName = "";

            try
            {
                // Try to get corresponding provider
                var provider = StorageHelper.GetStorageProvider(filePath);
                if (provider != null)
                {
                    providerName = provider.Name;
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("FileDebug", "LOGOPERATION", ex);
            }

            return providerName;
        }


        /// <summary>
        /// Logs end of the read operation (logs the total size).
        /// </summary>
        /// <param name="size">Size to log</param>
        public static void LogReadEnd(int size)
        {
            if (Settings.Enabled)
            {
                RequestLog log = CurrentRequestLog;
                DataRow dr = log.CurrentLogItem;
                if ((dr != null) && dr["FileOperation"].ToString("").Equals("read", StringComparison.OrdinalIgnoreCase))
                {
                    dr["FileSize"] = size;
                }

                log.CurrentLogItem = null;
                log.CurrentContextItem = null;
            }
        }


        /// <summary>
        /// Logs the file operation to the log file.
        /// </summary>
        /// <param name="filePath">Path of the file the operation of which is logged</param>
        /// <param name="fileOperation">Operation with file (open, close, read, write)</param>
        public static void LogToFile(string filePath, string fileOperation)
        {
            if (Settings.LogToFile)
            {
                var log = Settings.RequestLog;

                log.Append("\r\n");
                log.Append(fileOperation.ToUpperInvariant());
                log.Append(" ");
                log.Append(filePath);
            }
        }


        /// <summary>
        /// Creates a new table for the cache log.
        /// </summary>
        private static DataTable NewLogTable()
        {
            // Create new log table
            DataTable result = new DataTable();
            result.TableName = "FilesLog";

            var cols = result.Columns;
            cols.Add(new DataColumn("FilePath", typeof(string)));
            cols.Add(new DataColumn("FileOperation", typeof(string)));
            cols.Add(new DataColumn("FileSize", typeof(int)));
            cols.Add(new DataColumn("FileAccesses", typeof(int)));
            cols.Add(new DataColumn("FileNotClosed", typeof(bool)));
            cols.Add(new DataColumn("FileParameters", typeof(string)));
            cols.Add(new DataColumn("FileText", typeof(string)));
            cols.Add(new DataColumn("ProviderName", typeof(string)));
            cols.Add(new DataColumn("Context", typeof(string)));
            cols.Add(new DataColumn("Counter", typeof(int)));
            cols.Add(new DataColumn("Time", typeof(DateTime)));

            return result;
        }


        /// <summary>
        /// Gets the index of file operation record from table based on file path and operation.
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="operation">Operation performed on the file</param>
        /// <param name="fileOperationsMapping">File operations mapping table</param>
        /// <returns>Index of file operation record or -1 if index was not found in mapping table</returns>
        private static int GetFileOperationRecordIndex(string path, string operation, Dictionary<string, int> fileOperationsMapping)
        {
            int fileOperationIndex;
            if (fileOperationsMapping.TryGetValue(path + "|" + operation, out fileOperationIndex))
            {
                return fileOperationIndex;
            }

            return -1;
        }

        /// <summary>
        /// Adds index of file operation into the mapping table. 
        /// When file operation index mapping is already present, it is overridden with new index value.
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="operation">Operation performed on the file</param>
        /// <param name="index">Index of operation</param>
        /// <param name="fileOperationsMapping">File operations mapping table</param>
        private static void AddFileOperationRecordIndex(string path, string operation, int index, Dictionary<string, int> fileOperationsMapping)
        {
            fileOperationsMapping[path + "|" + operation] = index;
        }

        #endregion
    }
}
