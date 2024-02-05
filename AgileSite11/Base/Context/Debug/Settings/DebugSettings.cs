using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Container for the particular debug settings
    /// </summary>
    public class DebugSettings
    {
        #region "Variables"

        private bool mInitializing;
        private bool? mLogToFile;
        private bool? mEnabled;
        private bool? mLive;
        private bool? mDebugOnAllPages;
        private bool? mStack;
        private int? mLogLength;
        private List<RequestLog> mLastLogs;
        private readonly RequestStockValue<StringBuilder> mRequestLog;
        private readonly RequestStockValue<int> mCurrentIndent;
        
        // No key by default - Not registered
        private int mDebugKey = -1;

        // Lock object to avoid concurrent initialization. Lock object must remain static, otherwise several debugs could face a deadlock, mostly between SqlDebug and HandlersDebug which may need each other during initialization.
        private static readonly object lockObject = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Debug settings key to access the RequestDebugs flags
        /// </summary>
        public int DebugKey
        {
            get
            {
                return mDebugKey;
            }
            internal set
            {
                mDebugKey = value;
            }
        }


        /// <summary>
        /// Name of the debug
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns true, if the debug is allowed to log the operations, either to file or to debug tables
        /// </summary>
        public bool LogOperations
        {
            get
            {
                return Enabled || LogToFile;
            }
        }


        /// <summary>
        /// If true, the debug logs operations to physical file?
        /// </summary>
        public bool LogToFile
        {
            get
            {
                return EnsureValue(ref mLogToFile, () => DebugHelper.GetDebugBoolSetting("CMSLog" + Name), false);
            }
            set
            {
                mLogToFile = value;
            }
        }


        /// <summary>
        /// If true, the debug is enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return EnsureValue(ref mEnabled, () => DebugHelper.GetDebugBoolSetting("CMSDebug" + Name), false);
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// If true, the live pages are debugged
        /// </summary>
        public bool Live
        {
            get
            {
                return EnsureValue(ref mLive, () => DebugHelper.GetDebugBoolSetting("CMSDebug" + Name + "Live"), false);
            }
            set
            {
                mLive = value;
            }
        }


        /// <summary>
        /// Debug on all pages, including the UI and preview pages.
        /// </summary>
        public bool All
        {
            get
            {
                return EnsureValue(ref mDebugOnAllPages, () => DebugHelper.GetDebugBoolSetting("CMSDebugAll" + Name), false);
            }
            set
            {
                mDebugOnAllPages = value;
            }
        }


        /// <summary>
        /// Track stack when debugging.
        /// </summary>
        public bool Stack
        {
            get
            {
                return EnsureValue(ref mStack, () => DebugHelper.GetDebugBoolSetting("CMSDebug" + Name + "Stack"), false);
            }
            set
            {
                mStack = value;
            }
        }


        /// <summary>
        /// Maximum length of the log in number of requests.
        /// </summary>
        public int LogLength
        {
            get
            {
                return EnsureValue(ref mLogLength, () => DebugHelper.GetDebugLogLength("CMSDebug" + Name + "LogLength"), 10);
            }
            set
            {
                mLogLength = value;
            }
        }


        /// <summary>
        /// Current request query log.
        /// </summary>
        public StringBuilder RequestLog
        {
            get
            {
                return mRequestLog.Value;
            }
            set
            {
                mRequestLog.Value = value;
            }
        }


        /// <summary>
        /// Gets or sets current indentation of the debug
        /// </summary>
        public int CurrentIndent
        {
            get
            {
                return mCurrentIndent;
            }
            set
            {
                // Do not allow negative indent
                if (value < 0)
                {
                    value = 0;
                }

                mCurrentIndent.Value = value;
            }
        }


        /// <summary>
        /// Last requests query logs, contains the arrays of [URL, SecurityLog, Time].
        /// </summary>
        public List<RequestLog> LastLogs
        {
            get
            {
                return mLastLogs ?? (mLastLogs = new List<RequestLog>());
            }
        }


        /// <summary>
        /// Column in the log containing the data size in the operation
        /// </summary>
        public string SizeColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Column in the log containing the duration of the operation
        /// </summary>
        public string DurationColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Method that finalizes the data prior the reporting
        /// </summary>
        public Action<DataTable> FinalizeData
        {
            get;
            set;
        }


        /// <summary>
        /// Log control that displays the debug log
        /// </summary>
        public string LogControl
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Debug name</param>
        public DebugSettings(string name)
        {
            Name = name;

            mRequestLog = new RequestStockValue<StringBuilder>("Debug_" + Name + "_Log", () => new StringBuilder())
                {
                    AllowCloneForNewThread = false
                };

            mCurrentIndent = new RequestStockValue<int>("Debug_" + Name + "_Indent", 0)
                {
                    AllowCloneForNewThread = false
                };
        }


        /// <summary>
        /// Ensures boolean value or default value if initialization is detected
        /// </summary>
        /// <param name="value">Value to load</param>
        /// <param name="init">Initializer</param>
        /// <param name="defaultValue">Default value to use</param>
        private T EnsureValue<T>(ref T? value, Func<T> init, T defaultValue)
            where T : struct
        {
            if (value == null)
            {
                lock (lockObject)
                {
                    if (value == null)
                    {
                        if (mInitializing)
                        {
                            return defaultValue;
                        }

                        mInitializing = true;

                        value = init();

                        mInitializing = false;
                    }
                }
            }

            return value.Value;
        }


        /// <summary>
        /// Sets all the debug settings to null and causes them to be reloaded.
        /// </summary>
        public void ResetSettings()
        {
            mDebugOnAllPages = null;
            mEnabled = null;
            mLive = null;
            mStack = null;
            mLogToFile = null;
            mLogLength = null;
        }


        /// <summary>
        /// Writes the request log to the log file.
        /// </summary>
        /// <param name="folder">Folder to which the log is written</param>
        public void WriteRequestLog(string folder)
        {
            if (!LogToFile)
            {
                return;
            }

            if (mRequestLog.IsValueCreated)
            {
                WriteToLogFile(folder, RequestLog);
            }
        }


        /// <summary>
        /// Writes the given 
        /// </summary>
        /// <param name="folder">Log folder</param>
        /// <param name="logSb">Content to log</param>
        public void WriteToLogFile(string folder, StringBuilder logSb)
        {
            var log = logSb.ToString();

            if (!String.IsNullOrEmpty(log))
            {
                try
                {
                    // Write the URL
                    string url = DebugHelper.GetRequestUrl();

                    log = String.Concat("\r\n\r\n// ", url, " [", DateTime.Now.ToString(), "]\r\n", log);

                    // Write the log to the file
                    var folderPath = GetLogFolder(folder);
                    if (folderPath != null)
                    {
                        lock (lockObject)
                        {
#pragma warning disable BH1014 // Do not use System.IO
                            // Ensure the directory
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }

                            // Get the log file
                            var file = String.Concat(folderPath, "\\log", Name, ".log");

                            File.AppendAllText(file, log);
#pragma warning restore BH1014 // Do not use System.IO
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    CoreServices.EventLog.LogException("Debug", "Log_" + Name, ex, LoggingPolicy.ONLY_ONCE);
                }
            }
        }


        /// <summary>
        /// File path, to which the debug logs operations
        /// </summary>
        /// <param name="folder">Folder to which the log is written</param>
        public string GetLogFolder(string folder)
        {
            if (!String.IsNullOrEmpty(folder))
            {
                return String.Concat(DebugHelper.LogFolder, "\\", folder);
            }

            return DebugHelper.LogFolder;
        }

        #endregion
    }
}
