namespace WTE.Configuration
{
    /// <summary>
    /// Debug mode and logging
    /// </summary>
    public class DebugConfiguration : WTEConfiguration
    {
        #region logging

        /// <summary>
        /// The log folder
        /// </summary>
        public static string LogFolder
        {
            get
            {
                return GetAppSettingString("LogFolder", "log/");
            }
        }


        /// <summary>
        /// The log file name
        /// </summary>
        public static string LogFileName
        {
            get
            {
                return GetAppSettingString("LogFileName", "errorlog.log");
            }
        }

        /// <summary>
        /// Append Date to log file name
        /// </summary>
        public static bool AddDateToLogFile
        {
            get
            {
                return GetBoolAppSetting("AddDateToLogFile", false);
            }
        }

        /// <summary>
        /// Add time to log file
        /// </summary>
        public static bool AddTimeToLogFile
        {
            get
            {
                return GetBoolAppSetting("AddTimeToLogFile", false);
            }
        }

        /// <summary>
        /// Append Store Name to log file name
        /// </summary>
        public static bool AddStoreNameToLogFile
        {
            get
            {
                return GetBoolAppSetting("AddStoreNameToLogFile", false);
            }
        }

        /// <summary>
        /// Append machine name to log file name
        /// </summary>
        public static bool AddMachineNameToLogFile
        {
            get
            {
                return GetBoolAppSetting("AddMachineNameToLogFile", false);
            }
        }

        /// <summary>
        /// Maintain original log file without additional info in the file name
        /// </summary>
        public static bool MaintainMainLogFile
        {
            get
            {
                return GetBoolAppSetting("MaintainMainLogFile", true);
            }
        }

        #endregion logging
    }
}