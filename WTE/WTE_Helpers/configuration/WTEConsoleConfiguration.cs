namespace WTE.Configuration
{
    /// <summary>
    /// Configuration specific to the Console
    /// </summary>
    public class WTEConsoleConfiguration : WTEConfiguration
    {
        #region settings

        /// <summary>
        /// Output to the console?
        /// </summary>
        public static bool OutputToConsole
        {
            get
            {
                return GetBoolAppSetting("OutputToConsole", false);
            }
        }

        /// <summary>
        /// output to file to logging directory
        /// </summary>
        public static bool OutputToFile
        {
            get
            {
                return GetBoolAppSetting("OutputToFile", false);
            }
        }

        /// <summary>
        /// out put file name
        /// </summary>
        public static string OutputFileName
        {
            get
            {
                return GetAppSettingString("OutputFileName", "console.log");
            }
        }

        /// <summary>
        /// Run the app without pausing
        /// </summary>
        public static bool RunWithoutPause
        {
            get
            {
                return GetBoolAppSetting("RunWithoutPause", true);
            }
        }

        /// <summary>
        /// Minute to sleep after completely run
        /// </summary>
        public static int SleepAfterRunMinute
        {
            get
            {
                return GetIntAppSetting("SleepAfterRunMinute", 1);
            }
        }

        /// <summary>
        /// Wait for a key press to exit the program.
        /// </summary>
        public bool TerminateWithKeyPress
        {
            get
            {
                return GetBoolAppSetting("TerminateWithKeyPress", false);
            }
        }

        #endregion settings
    }
}