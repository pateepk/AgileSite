using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CMS.Core;
using CMS.Helpers;

namespace CMS.Base
{
    /// <summary>
    /// Debug helping methods.
    /// </summary>
    public static class DebugHelper
    {
        #region "Variables"

        /// <summary>
        /// If true all debugs are disabled. The debug is disabled by default to prevent unwanted debugs during application PreInit phase. Debug gets initialized in the Init phase through SetApplicationStartDebug.
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugDisabled = new CMSStatic<bool?>(() => true);


        /// <summary>
        /// If true, at least one debug has the logging to file enabled.
        /// </summary>
        private static readonly CMSStatic<bool?> mAnyDebugLogToFileEnabled = new CMSStatic<bool?>();


        /// <summary>
        /// If true, operations for everything are logged to the file (for debug purposes).
        /// </summary>
        private static readonly CMSStatic<bool?> mLogEverythingToFile = new CMSStatic<bool?>();


        /// <summary>
        /// Debug everything everywhere?
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugEverythingEverywhere = new CMSStatic<bool?>();


        /// <summary>
        /// Debug everything?
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugEverything = new CMSStatic<bool?>();


        /// <summary>
        /// Debug everything live?
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugEverythingLive = new CMSStatic<bool?>();


        /// <summary>
        /// Debug everything on all pages?
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugAllForEverything = new CMSStatic<bool?>();


        /// <summary>
        /// Debug everything stack?
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugStackForEverything = new CMSStatic<bool?>();


        /// <summary>
        /// Everything log length (number of requests to log).
        /// </summary>
        private static readonly CMSStatic<int?> mEverythingLogLength = new CMSStatic<int?>();


        /// <summary>
        /// If false, all the resources (GetResource a GetCSS) requests are ignored in debugging.
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugResources = new CMSStatic<bool?>();


        /// <summary>
        /// If false, scheduler tasks are ignored in debugs.
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugScheduler = new CMSStatic<bool?>();


        /// <summary>
        /// If false, pages with Import/Export are excluded from debugs.
        /// </summary>
        private static readonly CMSStatic<bool?> mDebugImportExport = new CMSStatic<bool?>();


        /// <summary>
        /// If true all debugs are disabled.
        /// </summary>
        private static readonly CMSStatic<bool?> mShowDebugOnLiveSite = new CMSStatic<bool?>();


        /// <summary>
        /// The table of the method excluded from stack trace.
        /// </summary>
        private static HashSet<string> mExcludedMethods;


        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static readonly object tableLock = new object();


        /// <summary>
        /// Counter used to merge different debugs in time order.
        /// </summary>
        private static int mDebugCounter;


        /// <summary>
        /// If true, at least one debug is enabled.
        /// </summary>
        private static readonly CMSStatic<bool?> mAnyDebugEnabled = new CMSStatic<bool?>();


        /// <summary>
        /// If true, some debug is enabled on live site.
        /// </summary>
        private static readonly CMSStatic<bool?> mAnyLiveDebugEnabled = new CMSStatic<bool?>();


        /// <summary>
        /// Dictionary of the registered debugs
        /// </summary>
        private static readonly StringSafeDictionary<DebugSettings> mRegisteredDebugs = new StringSafeDictionary<DebugSettings>();


        /// <summary>
        /// List of the registered debugs
        /// </summary>
        private static readonly List<DebugSettings> mRegisteredDebugsList = new List<DebugSettings>();


        /// <summary>
        /// Folder for the debug logs
        /// </summary>
        private static string mLogFolder;


        private static int mMaxLogSize = 1000;


        private const int DEFAULT_LOG_LENGTH = 10;

        #endregion


        #region "Properties"

        /// <summary>
        /// Maximum debug log size
        /// </summary>
        public static int MaxLogSize
        {
            get
            {
                return mMaxLogSize;
            }
            set
            {
                mMaxLogSize = value;
            }
        }


        /// <summary>
        /// Returns the list of registered debugs
        /// </summary>
        public static List<DebugSettings> RegisteredDebugs
        {
            get
            {
                return mRegisteredDebugsList;
            }
        }


        /// <summary>
        /// File path, to which the debug logs operations
        /// </summary>
        public static string LogFolder
        {
            get
            {
                if ((mLogFolder == null))
                {
                    mLogFolder = SystemContext.WebApplicationPhysicalPath + "\\App_Data\\CMSTemp\\Debug";
                }

                return mLogFolder;
            }
            set
            {
                mLogFolder = value;
            }
        }


        /// <summary>
        /// Number of registered debugs
        /// </summary>
        public static int RegisteredDebugsCount
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns true if any debug is enabled.
        /// </summary>
        public static bool AnyDebugEnabled
        {
            get
            {
                if (mAnyDebugEnabled.Value == null)
                {
                    mAnyDebugEnabled.Value = !DebugDisabled && mRegisteredDebugsList.Any(s => s.LogOperations);
                }

                return mAnyDebugEnabled.Value.Value;
            }
            set
            {
                mAnyDebugEnabled.Value = value;
            }
        }


        /// <summary>
        /// Returns true if any debug has the logging to file enabled.
        /// </summary>
        public static bool AnyDebugLogToFileEnabled
        {
            get
            {
                if (mAnyDebugLogToFileEnabled.Value == null)
                {
                    mAnyDebugLogToFileEnabled.Value = !DebugDisabled && mRegisteredDebugsList.Any(s => s.LogToFile);
                }

                return mAnyDebugLogToFileEnabled.Value.Value;
            }
            set
            {
                mAnyDebugLogToFileEnabled.Value = value;
            }
        }


        /// <summary>
        /// Returns true if any LiveDebug is enabled.
        /// </summary>
        public static bool AnyLiveDebugEnabled
        {
            get
            {
                if (mAnyLiveDebugEnabled.Value == null)
                {
                    mAnyLiveDebugEnabled.Value = !DebugDisabled && (mRegisteredDebugsList.Any(s => s.Live) || GetDebugBoolSetting("CMSDebugViewStateLive"));
                }

                return mAnyLiveDebugEnabled.Value.Value;
            }
            set
            {
                mAnyLiveDebugEnabled.Value = value;
            }
        }


        /// <summary>
        /// If true, the debug control are allowed in the live site view
        /// </summary>
        public static bool ShowDebugOnLiveSite
        {
            get
            {
                // This property has AppSetting in web.config only
                if (mShowDebugOnLiveSite.Value == null)
                {
                    mShowDebugOnLiveSite.Value = CoreServices.Conversion.GetBoolean(CoreServices.AppSettings["CMSShowDebugOnLiveSite"], true);
                }

                return mShowDebugOnLiveSite.Value.Value;
            }
            set
            {
                mShowDebugOnLiveSite.Value = value;
            }
        }



        /// <summary>
        /// Indicates whether all debugs are disabled.
        /// </summary>
        public static bool DebugDisabled
        {
            get
            {
                return GetSetting(mDebugDisabled, "CMSDisableDebug", false, false, true);
            }
            set
            {
                mDebugDisabled.Value = value;
            }
        }


        /// <summary>
        /// If false, all the resources (GetResource a GetCSS) requests are ignored in debugging.
        /// </summary>
        public static bool DebugResources
        {
            get
            {
                return GetSetting(mDebugResources, "CMSDebugResources", false);
            }
            set
            {
                mDebugResources.Value = value;
            }
        }


        /// <summary>
        /// If false, scheduler tasks are ignored in debugs.
        /// </summary>
        public static bool DebugScheduler
        {
            get
            {
                return GetSetting(mDebugScheduler, "CMSDebugScheduler", false);
            }
            set
            {
                mDebugScheduler.Value = value;
            }
        }


        /// <summary>
        /// If false, pages with Import/Export are excluded from debugs.
        /// </summary>
        public static bool DebugImportExport
        {
            get
            {
                return GetSetting(mDebugImportExport, "CMSDebugImportExport", false);
            }
            set
            {
                mDebugImportExport.Value = value;
            }
        }


        /// <summary>
        /// Logs operations for everything?
        /// </summary>
        public static bool LogEverythingToFile
        {
            get
            {
                return GetSetting(mLogEverythingToFile, "CMSLogEverythingToFile", false);
            }
            set
            {
                mLogEverythingToFile.Value = value;
            }
        }


        /// <summary>
        /// Debug Everything everywhere?
        /// </summary>
        public static bool DebugEverythingEverywhere
        {
            get
            {
                return GetSetting(mDebugEverythingEverywhere, "CMSDebugEverythingEverywhere", false);
            }
            set
            {
                mDebugEverythingEverywhere.Value = value;
            }
        }


        /// <summary>
        /// Debug Everything?
        /// </summary>
        public static bool DebugEverything
        {
            get
            {
                return GetSetting(mDebugEverything, "CMSDebugEverything", DebugEverythingEverywhere);
            }
            set
            {
                mDebugEverything.Value = value;
            }
        }


        /// <summary>
        /// Debug Everything live?
        /// </summary>
        public static bool DebugEverythingLive
        {
            get
            {
                return GetSetting(mDebugEverythingLive, "CMSDebugEverythingLive", DebugEverythingEverywhere);
            }
            set
            {
                mDebugEverythingLive.Value = value;
            }
        }


        /// <summary>
        /// Debug all pages for everything.
        /// </summary>
        public static bool DebugAllForEverything
        {
            get
            {
                return GetSetting(mDebugAllForEverything, "CMSDebugAllForEverything", DebugEverythingEverywhere);
            }
            set
            {
                mDebugAllForEverything.Value = value;
            }
        }


        /// <summary>
        /// Track stack when debugging everything.
        /// </summary>
        public static bool DebugStackForEverything
        {
            get
            {
                return GetSetting(mDebugStackForEverything, "CMSDebugStackForEverything", false, true, false);
            }
            set
            {
                mDebugStackForEverything.Value = value;
            }
        }


        /// <summary>
        /// Maximum length of the Everything log in number of requests.
        /// </summary>
        public static int EverythingLogLength
        {
            get
            {
                if (mEverythingLogLength.Value == null)
                {
                    if (CoreServices.Settings.IsAvailable)
                    {
                        mEverythingLogLength.Value = GetDebugLogLength("CMSDebugEverythingLogLength", DEFAULT_LOG_LENGTH);
                    }
                    else
                    {
                        return DEFAULT_LOG_LENGTH;
                    }
                }

                return mEverythingLogLength.Value.Value;
            }
            set
            {
                mEverythingLogLength.Value = value;
            }
        }


        /// <summary>
        /// The table of the method excluded from stack trace.
        /// </summary>
        private static HashSet<string> ExcludedMethods
        {
            get
            {
                if (mExcludedMethods == null)
                {
                    lock (tableLock)
                    {
                        if (mExcludedMethods == null)
                        {
                            mExcludedMethods = GetExcludedMethods();
                        }
                    }
                }

                return mExcludedMethods;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the given method can be included in debug
        /// </summary>
        /// <param name="method">Method to check</param>
        public static bool CanDebug(MethodInfo method)
        {
            if (method != null)
            {
                var attrs = method.GetCustomAttributes(typeof(ExcludeFromDebug), true);

                return attrs.Length == 0;
            }

            return false;
        }


        /// <summary>
        /// Gets the value of given setting, initializes the variable based on key name and default value if not available
        /// </summary>
        /// <param name="variable">Setting variable</param>
        /// <param name="key">Key name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="checkDisabled">If true, the disabled setting of the debug is checked</param>
        /// <param name="initValue">Initialization value</param>
        private static bool GetSetting(CMSStatic<bool?> variable, string key, bool defaultValue, bool checkDisabled = true, bool? initValue = null)
        {
            if (variable.Value == null)
            {
                // Temporarily set initialization value to prevent infinite loop if nested call needs the same setting
                variable.Value = initValue;

                // Initialize the value
                variable.Value = GetDebugBoolSetting(key, defaultValue, checkDisabled);
            }

            return variable.Value.Value;
        }


        /// <summary>
        /// Registers the debug to the debug helper
        /// </summary>
        /// <param name="settings">Debug settings</param>
        public static void RegisterDebug(DebugSettings settings)
        {
            // Set up the debug key
            settings.DebugKey = RegisteredDebugsCount++;

            mRegisteredDebugs[settings.Name] = settings;
            mRegisteredDebugsList.Add(settings);
        }


        /// <summary>
        /// Disables all debugs if DebugHelper.DebugScheduler is false, returns original settings which can be restored afterwards.
        /// </summary>
        public static RequestSettings DisableSchedulerDebug()
        {
            RequestSettings current = DebugContext.CurrentRequestSettings;

            if (!DebugScheduler)
            {
                DebugContext.CurrentRequestSettings = new RequestSettings();
            }

            return current;
        }


        /// <summary>
        /// Restores the debug settings from array of values.
        /// </summary>
        /// <param name="settings">Settings to restore</param>
        public static void RestoreDebugSettings(RequestSettings settings)
        {
            DebugContext.CurrentRequestSettings = settings;
        }


        /// <summary>
        /// Sets all the debug settings to null and causes them to be reloaded. This method is synchronized through all web farm servers.
        /// </summary>
        /// <param name="createWebFarmTask">If true, reset is synchronized to all web farm servers</param>
        public static void ResetDebugSettings(bool createWebFarmTask = true)
        {
            mDebugDisabled.Value = null;
            // Due to caching of debug settings it is necessary to load DebugDisabled value before SqlDebug setting is cleared,
            // loading DebugDisabled will try to load SqlDebug (select from DB) and it will be cached with wrong value
            // (due to temporary assigning DebugDisabled to true to prevent infinite loop).
            var dummy = DebugDisabled;

            mDebugAllForEverything.Value = null;
            mDebugEverything.Value = null;
            mDebugEverythingEverywhere.Value = null;
            mDebugEverythingLive.Value = null;
            mDebugResources.Value = null;
            mDebugImportExport.Value = null;
            mDebugScheduler.Value = null;
            mDebugStackForEverything.Value = null;
            mEverythingLogLength.Value = null;
            mLogEverythingToFile.Value = null;

            mAnyDebugEnabled.Value = null;
            mAnyLiveDebugEnabled.Value = null;
            mAnyDebugLogToFileEnabled.Value = null;

            // Reset registered debugs
            mRegisteredDebugsList.ForEach(s => s.ResetSettings());

            // Fire reset event
            DebugEvents.SettingsReset.StartEvent(null);

            if (createWebFarmTask)
            {
                CoreServices.WebFarm.CreateTask(new ResetDebugSettingsWebFarmTask());
            }
        }


        /// <summary>
        /// Returns logical or of setting in web.config and setting.
        /// </summary>
        /// <param name="keyName">Name of the setting</param>
        /// <param name="defaultValue">Default value of the web.config setting</param>
        /// <param name="checkDisabled">If true, the disabled setting is checked for this setting</param>
        public static bool GetDebugBoolSetting(string keyName, bool defaultValue, bool checkDisabled = true)
        {
            if (checkDisabled && DebugDisabled)
            {
                return false;
            }

            return CoreServices.AppSettings[keyName].ToBoolean(defaultValue) || (CoreServices.Settings.IsAvailable && CoreServices.Settings[keyName].ToBoolean(false));
        }


        /// <summary>
        /// Returns logical or of setting in web.config and setting.
        /// </summary>
        /// <param name="keyName">Name of the setting</param>
        public static bool GetDebugBoolSetting(string keyName)
        {
            bool defaultValue;

            // Get the global default value according to settings name
            if (keyName.EndsWith("live", StringComparison.OrdinalIgnoreCase))
            {
                defaultValue = DebugEverythingLive;
            }
            else if (keyName.EndsWith("stack", StringComparison.OrdinalIgnoreCase))
            {
                defaultValue = DebugStackForEverything;
            }
            else if (keyName.StartsWith("cmsdebugall", StringComparison.OrdinalIgnoreCase))
            {
                defaultValue = DebugAllForEverything;
            }
            else if (keyName.StartsWith("cmslog", StringComparison.OrdinalIgnoreCase))
            {
                defaultValue = LogEverythingToFile;
            }
            else
            {
                defaultValue = DebugEverything;
            }

            return GetDebugBoolSetting(keyName, defaultValue);
        }


        /// <summary>
        /// Returns log length of given debug (looks first to the settings, if the value is not defined, takes value from web.config, if not defined even in web.config, returns 10 as default value).
        /// </summary>
        /// <param name="keyName">Name of the setting</param>
        public static int GetDebugLogLength(string keyName)
        {
            return GetDebugLogLength(keyName, EverythingLogLength);
        }


        /// <summary>
        /// Returns log length of given debug (looks first to the settings, if the value is not defined, takes value from web.config, if not defined even in web.config, returns 10 as default value).
        /// </summary>
        /// <param name="keyName">Name of the setting</param>
        /// <param name="defaultValue">Web.config default value</param>
        public static int GetDebugLogLength(string keyName, int defaultValue)
        {
            if (DebugDisabled || !CoreServices.Settings.IsAvailable)
            {
                return DEFAULT_LOG_LENGTH;
            }

            int result = CoreServices.Settings[keyName].ToInteger(0);
            if (result == 0)
            {
                result = CoreServices.Conversion.GetInteger(CoreServices.AppSettings[keyName], defaultValue);
            }
            if (result == 0)
            {
                result = DEFAULT_LOG_LENGTH;
            }

            return result;
        }


        /// <summary>
        /// Sets the debugging context.
        /// </summary>
        /// <param name="newContext">New context</param>
        public static void SetContext(string newContext)
        {
            if (AnyDebugEnabled)
            {
                // Get top context
                Stack<string> context = DebugContext.RequestContext;
                if (context != null)
                {
                    context.Push(newContext);
                }
            }
        }


        /// <summary>
        /// Releases the last context.
        /// </summary>
        public static void ReleaseContext()
        {
            if (AnyDebugEnabled)
            {
                // Get top context
                Stack<string> context = DebugContext.RequestContext;
                if ((context != null) && (context.Count > 0))
                {
                    context.Pop();
                }
            }
        }


        /// <summary>
        /// Gets the request URL for the log purposes.
        /// </summary>
        public static string GetRequestUrl()
        {
            string url;
            var httpContext = CMSHttpContext.Current;
            if (httpContext != null)
            {
                url = httpContext.Request.RawUrl;

                if (httpContext.Request.Headers["x-microsoftajax"] != null)
                {
                    // AJAX request
                    url = "[AJAX] " + url;
                }
            }
            else
            {
                // Thread
                url = (string)RequestItems.GetItem("ContextRawUrl", true);

                var threadId = CMSThread.GetCurrentThreadId();

                if (!String.IsNullOrEmpty(url))
                {
                    url = "[Thread ID " + threadId + "] " + url;
                }
                else
                {
                    url += "Thread ID " + threadId;
                }
            }

            return url;
        }


        /// <summary>
        /// Gets the current stack.
        /// </summary>
        public static string GetThreadStack()
        {
            var result = new StringBuilder();

            // Add thread stack if available
            var threads = DebugContext.CurrentThreadStack;
            if (threads != null)
            {
                foreach (var thread in threads.Reverse())
                {
                    if (result.Length > 0)
                    {
                        result.AppendLine();
                    }

                    result.Append(thread);
                }
            }

            return result.ToString();
        }


        /// <summary>
        /// Gets the current stack.
        /// </summary>
        /// <param name="depth">Stack depth</param>
        [HideFromDebugContext]
        public static string GetStack(int depth = 0)
        {
            if (depth <= 0)
            {
                depth = Int32.MaxValue;
            }

            var result = new StringBuilder();

            string lastMethod = null;
            int count = 0;
            int total = 0;

            // Add thread stack if available
            var threadStack = GetThreadStack();
            if (!String.IsNullOrEmpty(threadStack))
            {
                result.Append(threadStack);

                result.AppendLine();
                result.Append("---");
            }

            // Build the stack from stack trace
            var st = new StackTrace();

            for (int i = 0; i < st.FrameCount; i++)
            {
                var sf = st.GetFrame(i);
                if (sf != null)
                {
                    var m = sf.GetMethod();
                    if (m != null)
                    {
                        if (StopOnMethod(m))
                        {
                            break;
                        }

                        var methodName = GetMethodName(m);

                        // Process the method if not excluded
                        if (!ExcludeMethod(m, methodName))
                        {
                            if (methodName != lastMethod)
                            {
                                // Add count
                                if (result.Length > 0)
                                {
                                    // Add aggregated count for the previous method before listing a new one
                                    if (count > 1)
                                    {
                                        result.Append(" (");
                                        result.Append(count);
                                        result.Append(")");
                                    }

                                    result.AppendLine();
                                }

                                result.Append(methodName);

                                if (++total >= depth)
                                {
                                    break;
                                }

                                count = 1;
                            }
                            else
                            {
                                count++;
                            }

                            lastMethod = methodName;
                        }
                    }
                }
            }

            return result.ToString();
        }


        /// <summary>
        /// Returns true if the given method is excluded from debug (should not be listed in debug)
        /// </summary>
        /// <param name="method">Method to check</param>
        /// <param name="methodName">Method name</param>
        private static bool ExcludeMethod(MethodBase method, string methodName)
        {
            // Skip methods marked with [ExcludeFromDebug]
            if (method.GetCustomAttributes(typeof(HideFromDebugContextAttribute), true).Length > 0)
            {
                return true;
            }

            return ExcludedMethods.Contains(methodName);
        }


        /// <summary>
        /// Returns true if the given method should be skipped from the stack trace
        /// </summary>
        /// <param name="m">Method to check</param>
        private static bool StopOnMethod(MethodBase m)
        {
            // Don't skip methods without declaring type
            var type = m.DeclaringType;
            if (type == null)
            {
                return false;
            }

            // Skip methods from system namespaces except for RegEx methods
            var ns = type.Namespace;
            var cls = type.Name;

            if ((ns != null) &&
                ns.StartsWithCSafe("System") &&  // Skip System methods
                !ns.StartsWithCSafe("System.Text.RegularExpressions") &&  // Don't skip regular expressions
                !ns.StartsWithCSafe("System.Linq") &&  // Don't skip LINQ expressions
                !cls.EqualsCSafe("Enumerable")) // Skip enumerable methods (Any, etc.)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets the method name
        /// </summary>
        /// <param name="m">Method</param>
        private static string GetMethodName(MethodBase m)
        {
            // Prepare method name
            string method = null;
            if (m.DeclaringType != null)
            {
                method += m.DeclaringType.Name + ".";
            }
            method += m.Name;

            return method;
        }


        /// <summary>
        /// Returns state of the debug counter.
        /// </summary>
        /// <param name="increment">If true, counter is incremented</param>
        public static int GetDebugCounter(bool increment)
        {
            if (increment)
            {
                return mDebugCounter++;
            }
            else
            {
                return mDebugCounter;
            }
        }


        /// <summary>
        /// Checks if all folders of the given path exist and if not, it creates them.
        /// </summary>
        /// <param name="path">Full disk path of the file including file name</param>
        /// <param name="startingPath">Starting path where folders can be checked and created</param>
        public static void EnsureDiskPath(string path, string startingPath)
        {
            int folderIndex;
            int startingIndex = 0;

            // Prepare the starting path
            if (startingPath == null)
            {
                startingPath = "";
            }
            if (startingPath.EndsWithCSafe("\\"))
            {
                startingPath = startingPath.Substring(0, startingPath.Length - 1);
            }
            // If path outside of the application folder, ignore the starting path
            if (!path.StartsWithCSafe(startingPath, true))
            {
                startingPath = "";
            }

            bool networkDirectory = path.StartsWithCSafe("\\\\");

            // Remove file name from the path
            string folderPath = path.Substring(0, path.LastIndexOfCSafe(@"\"));
            string[] pathArray = folderPath.Split('\\');

            string currentPath = pathArray[0];

            // If starting path available, get starting index
            if ((startingPath != "") && folderPath.ToLowerCSafe().Trim().StartsWithCSafe(startingPath.ToLowerCSafe().Trim()))
            {
                startingIndex = startingPath.Split('\\').GetUpperBound(0);
            }

            for (folderIndex = 1; folderIndex <= pathArray.GetUpperBound(0); folderIndex++)
            {
                currentPath += @"\" + pathArray[folderIndex];
                if ((startingIndex < folderIndex) && (!networkDirectory || (folderIndex > 2)))
                {
#pragma warning disable BH1014 // Do not use System.IO
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
#pragma warning restore BH1014 // Do not use System.IO
                }
            }
        }


        /// <summary>
        /// Creates new debug log table
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="columns">Columns</param>
        public static DataTable CreateLogTable(string name, params DataColumn[] columns)
        {
            DataTable result = new DataTable(name);
            result.Columns.AddRange(columns);

            return result;
        }


        /// <summary>
        /// Finds the collection of request logs by the given request GUID
        /// </summary>
        /// <param name="requestGuid">Request GUID</param>
        public static RequestLogs FindRequestLogs(Guid requestGuid)
        {
            RequestLogs logs = null;
            if (requestGuid != Guid.Empty)
            {
                var log = FindRequestLog(requestGuid);
                if (log != null)
                {
                    logs = log.ParentLogs;
                }
            }
            return logs;
        }


        /// <summary>
        /// Finds the first log found by the given request GUID
        /// </summary>
        /// <param name="requestGuid">Request GUID</param>
        private static RequestLog FindRequestLog(Guid requestGuid)
        {
            // Search all debugs
            foreach (var debug in mRegisteredDebugsList)
            {
                var log = RequestLog.FindByGUID(debug.LastLogs, requestGuid);
                if (log != null)
                {
                    return log;
                }
            }

            return null;
        }


        /// <summary>
        /// Writes all request logs to the files
        /// </summary>
        public static void WriteRequestLogs()
        {
            // Write all request logs
            mRegisteredDebugsList.ForEach(d => d.WriteRequestLog(DebugContext.CurrentLogFolder));
        }


        /// <summary>
        /// Returns true if the given debug is enabled
        /// </summary>
        /// <param name="name">Debug name</param>
        public static bool IsDebugEnabled(string name)
        {
            var debug = mRegisteredDebugs[name];
            if (debug != null)
            {
                return debug.Enabled;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the given debug is enabled
        /// </summary>
        public static void SetApplicationStartDebug()
        {
            // Reset the debug settings to allow proper loading of them
            ResetDebugSettings(false);

            if (AnyDebugEnabled)
            {
                var liveSite = CMSActionContext.CurrentIsLiveSite;
                var settings = DebugContext.CurrentRequestSettings;

                // Setup all debug flags
                foreach (var debug in mRegisteredDebugsList)
                {
                    settings[debug.DebugKey] = debug.LogOperations && liveSite;
                }
            }
        }


        /// <summary>
        /// Sets the initial debugging settings.
        /// </summary>
        public static void SetInitialDebug()
        {
            if (AnyDebugEnabled)
            {
                var settings = DebugContext.CurrentRequestSettings;

                // Setup all debug flags
                foreach (var debug in mRegisteredDebugsList)
                {
                    if (debug.LogOperations)
                    {
                        settings[debug.DebugKey] = true;
                    }
                }
            }
        }


        /// <summary>
        /// Sets the debug flags for system page
        /// </summary>
        public static void SetSystemPageDebug()
        {
            if (AnyDebugEnabled)
            {
                var settings = DebugContext.CurrentRequestSettings;

                // Setup all debug flags
                foreach (var debug in mRegisteredDebugsList)
                {
                    if (debug.LogOperations)
                    {
                        settings[debug.DebugKey] = debug.All;
                    }
                }
            }
        }


        /// <summary>
        /// Sets the debug flags for content page
        /// </summary>
        public static void SetContentPageDebug()
        {
            if (AnyDebugEnabled)
            {
                var liveSite = CMSActionContext.CurrentIsLiveSite;
                var settings = DebugContext.CurrentRequestSettings;

                // Setup all debug flags
                foreach (var debug in mRegisteredDebugsList)
                {
                    if (debug.LogOperations)
                    {
                        settings[debug.DebugKey] = (liveSite || debug.All);
                    }
                }
            }
        }


        /// <summary>
        /// Disables all debugs in the context of current request / thread
        /// </summary>
        public static void DisableDebug()
        {
            if (AnyDebugEnabled)
            {
                var settings = DebugContext.CurrentRequestSettings;

                // Setup all debug flags
                foreach (var debug in mRegisteredDebugsList)
                {
                    if (debug.LogOperations)
                    {
                        settings[debug.DebugKey] = false;
                    }
                }
            }
        }


        /// <summary>
        /// Clears all logs within all debugs
        /// </summary>
        public static void ClearLogs()
        {
            // Setup all debug flags
            foreach (var debug in mRegisteredDebugsList)
            {
                debug.LastLogs.Clear();
            }
        }


        /// <summary>
        /// Registers all request logs to the appropriate log lists.
        /// </summary>
        public static void RegisterLogs()
        {
            var logs = DebugContext.CurrentRequestLogs;
            var settings = DebugContext.CurrentRequestSettings;

            // Setup all debug flags
            foreach (var debug in mRegisteredDebugsList)
            {
                var log = logs[debug];
                if ((log != null) && settings[debug])
                {
                    log.Register();
                }
            }

            logs.RegisterNewLogs = true;
        }


        /// <summary>
        /// Gets the settings for a given debug
        /// </summary>
        /// <param name="name">Debug name</param>
        public static DebugSettings GetSettings(string name)
        {
            return mRegisteredDebugs[name];
        }


        /// <summary>
        /// Gets the context for debug operation
        /// </summary>
        /// <param name="stack">If true, the stack information is retrieved</param>
        [HideFromDebugContext]
        public static string GetContext(bool stack)
        {
            return !stack ? DebugContext.CurrentExecutionContext : GetStack();
        }


        /// <summary>
        /// Gets the table of the method excluded from stack trace.
        /// </summary>
        private static HashSet<string> GetExcludedMethods()
        {
            // Fill in the methods table
            var methods = new HashSet<string>(new[]
                {
                    "DebugHelper.get_CurrentContext",
                    "DataConnection.get_NativeDBConnection",
                    "AbstractDataConnection.get_NativeConnection",
                    "RegexReplacement.Replace",
                    "Regex.Replace",
                });

            return methods;
        }


        /// <summary>
        /// Ensures the Duration column in the datateble of Request log.
        /// </summary>
        /// <param name="dt">DataTable of the request log</param>
        /// <param name="columnName">Column name</param>
        public static void EnsureDurationColumn(DataTable dt, string columnName = "Duration")
        {
            if (dt == null)
            {
                return;
            }

            // Ensure the duration column
            if (!dt.Columns.Contains(columnName))
            {
                dt.Columns.Add(columnName, typeof(double));

                DataRow lastRow = null;
                DateTime last = DateTime.MinValue;

                var conversionService = CoreServices.Conversion;

                foreach (DataRow dr in dt.Rows)
                {
                    // Calculate the duration
                    var time = conversionService.GetDateTime(dr["Time"], last, null);
                    if (lastRow != null)
                    {
                        double duration = (time - last).TotalSeconds;
                        lastRow[columnName] = duration;
                    }

                    last = time;
                    lastRow = dr;
                }
            }
        }

        #endregion
    }
}