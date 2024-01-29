using System;

using CMS.Core;
using CMS.Base;

namespace CMS.IO
{
    /// <summary>
    /// Helper class used for logging virtual path operations
    /// </summary>
    public class VirtualPathLog
    {
        #region "Log variables"

        // Flag whether the application start was already logged
        private static bool mStartLogged = false;
        // If true, virtual files are logged to the file (for debug purposes).
        private static bool? mLogVirtualFiles = null;
        /// Indentation level within the current virtual file log
        private static int mCurrentLogIndent = 0;
        // Last folder that was logged
        private static string mLastLoggedFolder = "";
        // Last logged file path
        private static string mLastLoggedPath = "";
        // True if the last log item was a load control method
        private static bool mLastLogLoadControl = false;
        // Folder start time for tracking the overall impact
        private static DateTime mFolderStartTime = DateTime.MinValue;
        // List of controls for which load was already called
        private static StringSafeDictionary<bool> mLoadedControls = new StringSafeDictionary<bool>();
        private static object locker = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether virtual files operation should be logged. Uses CMSLogVirtualFiles app setting key.
        /// </summary>
        public static bool LogVirtualFiles
        {
            get
            {
                if (mLogVirtualFiles == null)
                {
                    mLogVirtualFiles = CoreServices.Conversion.GetBoolean(SettingsHelper.AppSettings["CMSLogVirtualFiles"], DebugHelper.LogEverythingToFile);
                }

                return mLogVirtualFiles.Value;
            }
            set
            {
                mLogVirtualFiles = value;
            }
        }


        /// <summary>
        /// Logs file full path
        /// </summary>
        private static string LogFile
        {
            get
            {
                return GetLogFileName("logVirtualFiles.log");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the log file name
        /// </summary>
        /// <param name="fileName">File name</param>
        private static string GetLogFileName(string fileName)
        {
            string result = DebugHelper.LogFolder + "\\" + fileName;
            DebugHelper.EnsureDiskPath(result, SystemContext.WebApplicationPhysicalPath);

            return result;
        }


        /// <summary>
        /// Removes log file from system and reset class settings
        /// </summary>
        public static void RemoveLogFile()
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }

            mStartLogged = false;
            mLogVirtualFiles = null;
            mCurrentLogIndent = 0;
            mLastLoggedFolder = "";
            mLastLoggedPath = "";
            mLastLogLoadControl = false;
            mFolderStartTime = DateTime.MinValue;
        }


        /// <summary>
        /// Logs load control or load virtual file to the ~/App_Data/logVirtualFiles.log file
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="isPhysical">Indicates whether current file is physical</param>
        /// <param name="loadControl">Indicates whether log context is from load control method</param>
        public static void Log(string virtualPath, bool isPhysical = true, bool loadControl = true)
        {
            if (LogVirtualFiles)
            {
                if (loadControl)
                {
                    LogToFile(virtualPath, isPhysical);
                }
                else
                {
                    LogVirtualToFile(virtualPath, isPhysical);
                }
            }
        }


        /// <summary>
        /// Logs the virtual file request to the file.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="physicalFile">Indicates whether current path is physical</param>
        private static void LogToFile(string virtualPath, bool physicalFile)
        {
            DateTime now = DateTime.Now;

            // Handle the folder change
            HandleFolderLog(virtualPath, physicalFile, true);

            // Log load control with current indent only
            if (!mLoadedControls[virtualPath])
            {
                string controlLogText = String.Format("\r\n{0}[{1}] LoadControl(\"{2}\")", GetIndent(mCurrentLogIndent), now.ToString("M/d/yyyy HH:mm:ss.fff", SystemContext.EnglishCulture.Value.DateTimeFormat), virtualPath);

                AppendllTextThreadSafe(LogFile, controlLogText);

                mLoadedControls[virtualPath] = true;
                mLastLogLoadControl = true;
            }

        }


        /// <summary>
        /// Ensures thread-safe append all text method
        /// </summary>
        private static void AppendllTextThreadSafe(string file, string text)
        {
            lock (locker)
            {
                File.AppendAllText(file, text, false);
            }
        }


        /// <summary>
        /// Logs the virtual file request to the file.
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <param name="physicalFile">Indicates whether current path is physical</param>
        private static void LogVirtualToFile(string virtualPath, bool physicalFile)
        {

            DateTime now = DateTime.Now;

            // Handle the folder change
            mLastLoggedFolder = HandleFolderLog(virtualPath, physicalFile, false);

            // Prepare the log text
            string logText = String.Format("\r\n{0}[{1}] {2}", GetIndent(mCurrentLogIndent), now.ToString("M/d/yyyy HH:mm:ss.fff", SystemContext.EnglishCulture.Value.DateTimeFormat), virtualPath);

            AppendllTextThreadSafe(LogFile, logText);

            mLastLoggedPath = virtualPath;
            mLastLogLoadControl = false;

        }


        /// <summary>
        /// Handles the folder within the log
        /// </summary>
        /// <param name="virtualPath">Virtual path to log</param>
        /// <param name="physicalFile">Indicates whether current file is physical</param>
        /// <param name="isLoadControl">If true, current log item is a load control item</param>
        private static string HandleFolderLog(string virtualPath, bool physicalFile, bool isLoadControl)
        {
            if (!mStartLogged)
            {
                AppendllTextThreadSafe(LogFile, "[----- Application start -----]");
                mStartLogged = true;
            }

            // Get the folder path
            string folder = virtualPath;
            int indentSize = 1;

            DateTime now = DateTime.Now;

            int slashIndex = folder.LastIndexOf('/');
            if (slashIndex >= 0)
            {
                folder = folder.Substring(0, slashIndex);
            }

            if (folder != mLastLoggedFolder)
            {
                if (!isLoadControl || !mLastLogLoadControl)
                {
                    // Add folder time summary
                    DateTime startTime = mFolderStartTime;
                    if (startTime != DateTime.MinValue)
                    {
                        TimeSpan folderTime = now - startTime;

                        // Prepare the log text
                        string sumLogText = String.Format(" [{0:F3}]", folderTime.TotalSeconds);

                        AppendllTextThreadSafe(LogFile, sumLogText);
                    }

                    mFolderStartTime = now;

                    // Handle indent only for physical files
                    if (physicalFile)
                    {
                        if (mLastLoggedPath.EndsWith("x", StringComparison.InvariantCultureIgnoreCase) && virtualPath.EndsWith("x", StringComparison.InvariantCultureIgnoreCase))
                        {
                            mCurrentLogIndent += indentSize;
                        }
                        else if (mCurrentLogIndent >= indentSize)
                        {
                            mCurrentLogIndent -= indentSize;
                        }
                    }
                }
            }

            return folder;
        }


        /// <summary>
        /// Gets the indent for the given count
        /// </summary>
        /// <param name="indentCount">Indent count</param>
        private static string GetIndent(int indentCount)
        {
            // Prepare indentation string 
            string indent = null;
            if (indentCount > 0)
            {
                indent = String.Empty.PadRight(indentCount, '\t');
            }
            return indent;
        }

        #endregion
    }
}
