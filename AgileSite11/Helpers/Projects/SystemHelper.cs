using System;
using System.Diagnostics;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// System operations.
    /// </summary>
    public static class SystemHelper
    {
        #region "Constants"

        /// <summary>
        /// Maximum length of application name.
        /// </summary>
        public const int APPLICATION_NAME_MAX_LENGTH = 60;


        /// <summary>
        /// Key name in the web.config for application name
        /// </summary>
        public const string APP_NAME_KEY_NAME = "CMSApplicationName";


        /// <summary>
        /// Key name in the web.config for application GUID.
        /// </summary>
        public const string APP_GUID_KEY_NAME = "CMSApplicationGuid";

        #endregion


        #region "Variables"

        /// <summary>
        /// True if web project is compiled in debug mode.
        /// </summary>
        private static bool? mSanitizeXML;


        /// <summary>
        /// Application instance (current run) GUID.
        /// </summary>
        private static readonly Guid mApplicationInstanceGUID = Guid.NewGuid();


        /// <summary>
        /// Application name.
        /// </summary>
        private static string mApplicationName;

        /// <summary>
        /// Unvalidated application GUID.
        /// </summary>
        private static Guid? mApplicationGuidRaw;

        /// <summary>
        /// Application identifier.
        /// </summary>
        private static string mApplicationIdentifier;

        /// <summary>
        /// Indicates if application restart should occur after application initialization fails.
        /// </summary>
        private static bool? mRestartApplicationIfInitFails;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Application instance (current run) GUID.
        /// </summary>
        public static Guid ApplicationInstanceGUID
        {
            get
            {
                return mApplicationInstanceGUID;
            }
        }


        /// <summary>
        /// Gets application name from application configuration file.
        /// </summary>
        /// <remarks>
        /// Application configuration file must contain a value for key <see cref="APP_NAME_KEY_NAME"/>. If <see cref="APP_NAME_KEY_NAME"/> is missing, a fallback to <see cref="ApplicationGuid"/> is performed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the application configuration file does not specify value for any key of <see cref="APP_NAME_KEY_NAME"/> and <see cref="APP_GUID_KEY_NAME"/>.</exception>
        public static string ApplicationName
        {
            get
            {
                if (mApplicationName == null)
                {
                    var appName = CoreServices.AppSettings[APP_NAME_KEY_NAME];

                    if (String.IsNullOrEmpty(appName))
                    {
                        if (ApplicationGuidRaw == Guid.Empty)
                        {
                            throw new InvalidOperationException(String.Format("The application configuration file is missing both keys '{0}' and '{1}' or their value is not set. At least one of them has to be specified.",
                                APP_NAME_KEY_NAME, APP_GUID_KEY_NAME));
                        }

                        mApplicationName = FormatApplicationName(appName, ApplicationGuid);
                    }
                    else
                    {
                        mApplicationName = FormatApplicationName(appName, Guid.Empty);
                    }
                }

                return mApplicationName;
            }
        }


        /// <summary>
        /// Gets application GUID from application configuration file.
        /// </summary>
        /// <remarks>
        /// Application configuration file must contain a value for key <see cref="APP_GUID_KEY_NAME"/> which is different from <see cref="Guid.Empty"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the application configuration file does not specify a value for key <see cref="APP_GUID_KEY_NAME"/>.</exception>
        public static Guid ApplicationGuid
        {
            get
            {
                var rawGuid = ApplicationGuidRaw;

                // Check if instance path key exists and value is not empty
                if (rawGuid == Guid.Empty)
                {
                    throw new InvalidOperationException(String.Format("The application configuration file is missing key '{0}' or its value is not set. To fix this, set its value to non-empty GUID.", APP_GUID_KEY_NAME));
                }

                return rawGuid;
            }
        }


        /// <summary>
        /// Gets application GUID from application configuration file, or <see cref="Guid.Empty"/> if its value is either missing or is not a valid GUID.
        /// </summary>
        private static Guid ApplicationGuidRaw
        {
            get
            {
                if (mApplicationGuidRaw == null)
                {
                    mApplicationGuidRaw = CoreServices.AppSettings[APP_GUID_KEY_NAME].ToGuid(Guid.Empty);
                }

                return mApplicationGuidRaw.Value;
            }
        }


        /// <summary>
        /// Gets application identifier.
        /// </summary>
        /// <remarks>
        /// Application identifier is derived from <see cref="ApplicationGuid"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ApplicationGuid"/> is not specified.</exception>
        public static string ApplicationIdentifier
        {
            get
            {
                return mApplicationIdentifier ?? (mApplicationIdentifier = FormatApplicationIdentifier(ApplicationGuid));
            }
        }


        /// <summary>
        /// Indicates if usage of win services should be forced.
        /// </summary>
        public static bool WinServicesForceUsage
        {
            get
            {
                return CoreServices.AppSettings["CMSWinServicesForceUsage"].ToBoolean(false);
            }
        }


        /// <summary>
        /// Indicates if XML will be preprocessed by CMS.IO streams (removes illegal characters).
        /// </summary>
        public static bool SanitizeXML
        {
            get
            {
                if (mSanitizeXML == null)
                {
                    mSanitizeXML = CoreServices.AppSettings["CMSSanitizeXML"].ToBoolean(false);
                }
                return mSanitizeXML.Value;
            }
        }


        /// <summary>
        /// Indicates if application restart should occur after application initialization fails.
        /// </summary>
        public static bool RestartApplicationIfInitFails
        {
            get
            {
                if (mRestartApplicationIfInitFails == null)
                {
                    mRestartApplicationIfInitFails = SettingsHelper.AppSettings["CMSRestartApplicationIfInitFails"].ToBoolean(true);
                }
                return mRestartApplicationIfInitFails.Value;
            }
            set
            {
                mRestartApplicationIfInitFails = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the virtual memory size.
        /// </summary>
        public static long GetVirtualMemorySize()
        {
            Process proc = Process.GetCurrentProcess();
            return proc.VirtualMemorySize64;
        }


        /// <summary>
        /// Gets the working set size.
        /// </summary>
        public static long GetWorkingSetSize()
        {
            Process proc = Process.GetCurrentProcess();
            return proc.WorkingSet64;
        }


        /// <summary>
        /// Gets the peak working set size.
        /// </summary>
        public static long GetPeakWorkingSetSize()
        {
            Process proc = Process.GetCurrentProcess();
            return proc.PeakWorkingSet64;
        }


        /// <summary>
        /// Clears static properties (initializes fields of properties).
        /// </summary>
        public static void Clear()
        {
            mApplicationName = null;
            mApplicationGuidRaw = null;
            mApplicationIdentifier = null;
        }


        /// <summary>
        /// Sets the application name
        /// </summary>
        /// <param name="applicationName">Application name</param>
        public static void SetApplicationName(string applicationName)
        {
            mApplicationName = applicationName;
        }


        /// <summary>
        /// Sets the application GUID
        /// </summary>
        /// <param name="applicationGuid">Application GUID</param>
        public static void SetApplicationGuid(Guid applicationGuid)
        {
            mApplicationGuidRaw = applicationGuid;
        }


        /// <summary>
        /// Tries to restart application and returns if restart was successful.
        /// </summary>
        /// <param name="physicalApplicationPath">Physical application path</param>
        public static bool RestartApplication(string physicalApplicationPath)
        {
            // Restart azure instance
            if (SystemContext.IsRunningOnAzure)
            {
                AzureHelper.RestartAzureInstance();
            }
            // Restart classic application
            else
            {
                try
                {
                    // Try to restart application by unload app domain
                    HttpRuntime.UnloadAppDomain();
                }
                catch
                {
                    try
                    {
                        // Try to restart application by changing web.config file
                        File.SetLastWriteTimeUtc(physicalApplicationPath + "\\web.config", DateTime.UtcNow);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Formats the application identifier retrieved from the configuration file.
        /// </summary>
        /// <param name="value">The value to format.</param>
        public static string FormatApplicationIdentifier(Guid value)
        {
            return value.ToString("N");
        }

        
        /// <summary>
        /// Formats the application name retrieved from the configuration file.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="identifierValue">The application identifier to use when the name is empty.</param>
        public static string FormatApplicationName(string value, Guid identifierValue)
        {
            string result = value;
            if (String.IsNullOrEmpty(value))
            {
                result = identifierValue.ToString();
            }

            // Ensure maximal length of the application name
            if (result.Length > APPLICATION_NAME_MAX_LENGTH)
            {
                result = result.Substring(0, APPLICATION_NAME_MAX_LENGTH);
            }

            return result;
        }

        #endregion
    }
}