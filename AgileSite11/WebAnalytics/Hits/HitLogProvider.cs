using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.Base;
using CMS.MacroEngine;
using CMS.DataEngine;
using CMS.Search;

using SystemIO = System.IO;

namespace CMS.WebAnalytics
{
    using DomainTable = SafeDictionary<string, bool?>;

    /// <summary>
    /// HitLogProvider class.
    /// </summary>
    public class HitLogProvider
    {
        #region "Statistics constants"

        /// <summary>
        /// Page views code name.
        /// </summary>
        public const string PAGE_VIEWS = "pageviews";

        /// <summary>
        /// Aggregated page views code name.
        /// </summary>
        public const string AGGREGATED_VIEWS = "aggviews";

        /// <summary>
        /// First visitors code name.
        /// </summary>
        public const string VISITORS_FIRST = "visitfirst";

        /// <summary>
        /// Returning visitors code name.
        /// </summary>
        public const string VISITORS_RETURNING = "visitreturn";

        /// <summary>
        /// File downloads code name.
        /// </summary>
        public const string FILE_DOWNLOADS = "filedownloads";

        /// <summary>
        /// Page not found code name.
        /// </summary>
        public const string PAGE_NOT_FOUND = "pagenotfound";

        /// <summary>
        /// URL referrals code name.
        /// </summary>
        public const string URL_REFERRALS = "urlreferrals";

        /// <summary>
        /// Conversions code name.
        /// </summary>
        public const string CONVERSIONS = "conversion";

        /// <summary>
        /// Campaigns code name.
        /// </summary>
        public const string CAMPAIGNS = "campaign";

        /// <summary>
        /// Browser type code name.
        /// </summary>
        public const string BROWSER_TYPE = "browsertype";

        /// <summary>
        /// IP to country conversion code name.
        /// </summary>
        public const string COUNTRIES = "countries";

        /// <summary>
        /// Registered user code name.
        /// </summary>
        public const string REGISTEREDUSER = "registereduser";

        /// <summary>
        /// Landing page code name.
        /// </summary>
        public const string LANDINGPAGE = "landingpage";

        /// <summary>
        /// Exit page candidate code name.
        /// </summary>
        public const string EXITPAGECANDIDATE = "exitpagecandidate";

        /// <summary>
        /// Exit page code name.
        /// </summary>
        public const string EXITPAGE = "exitpage";

        /// <summary>
        /// Avg. time on page code name.
        /// </summary>
        public const string AVGTIMEONPAGE = "avgtimeonpage";

        /// <summary>
        /// Referring sites code name.
        /// </summary>
        public const string REFERRINGSITE = "referringsite";

        /// <summary>
        /// Search keywords code name.
        /// </summary>
        public const string SEARCHKEYWORD = "searchkeyword";

        /// <summary>
        /// On-site search keywords code name.
        /// </summary>
        public const string ONSITESEARCHKEYWORD = "onsitesearchkeyword";

        /// <summary>
        /// Operating system code name.
        /// </summary>
        public const string OPERATINGSYSTEM = "operatingsystem";

        /// <summary>
        /// Screen color code name.
        /// </summary>
        public const string SCREENCOLOR = "screencolor";

        /// <summary>
        /// Screen resolution code name.
        /// </summary>
        public const string SCREENRESOLUTION = "screenresolution";

        /// <summary>
        /// Flash code name.
        /// </summary>
        public const string FLASH = "flash";

        /// <summary>
        /// Silverlight code name.
        /// </summary>
        public const string SILVERLIGHT = "silverlight";

        /// <summary>
        /// Java code name.
        /// </summary>
        public const string JAVA = "java";

        /// <summary>
        /// Constant used for loging semicolons 
        /// </summary>
        public const string SEMICOLONDELIMITER = "##SEMICOLONDELIMITER##";

        /// <summary>
        /// Crawler code name
        /// </summary>
        public const string CRAWLER = "crawler";

        /// <summary>
        /// Mobile device code name
        /// </summary>
        public const string MOBILEDEVICE = "mobiledevice";

        #endregion


        #region "Variables"

        /// <summary>
        /// Static memory storage for web analytics
        /// </summary>
        private static MemoryStorage mMemStorage;


        /// <summary>
        /// Memory storage locker
        /// </summary>
        private static readonly object msLocker = new object();


        /// <summary>
        /// Table with domains and true/false values which indicate whether web analytics is allowed for a domain.
        /// </summary>
        private static readonly CMSStatic<DomainTable> mDomains = new CMSStatic<DomainTable>(() => new DomainTable());


        /// <summary>
        /// Logs directory.
        /// </summary>
        private static string mLogDirectory;


        /// <summary>
        /// Private locking object used when new entry is appended into the log file.
        /// </summary>
        private static readonly object mLockingTarget = new object();


        /// <summary>
        /// Indicates whether memory storage should be used instead of direct file write
        /// </summary>
        private static bool? mUseMemoryStorage;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the value that indicates whether memory storage should be used instead of direct file write
        /// </summary>
        private static bool UseMemoryStorage
        {
            get
            {
                if (mUseMemoryStorage == null)
                {
                    mUseMemoryStorage = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSWebAnalyticsUseMemoryStorage"], true);
                }

                return mUseMemoryStorage.Value;
            }
        }


        /// <summary>
        /// Gets the memory storage
        /// </summary>
        private static MemoryStorage MemStorage
        {
            get
            {
                if (mMemStorage == null)
                {
                    lock (msLocker)
                    {
                        if (mMemStorage == null)
                        {
                            mMemStorage = new MemoryStorage("WA", LogDirectory);
                        }
                    }
                }

                return mMemStorage;
            }
        }


        /// <summary>
        /// Table with domains and true/false values which indicate whether web analytics is allowed for a domain.
        /// </summary>
        private static DomainTable Domains => mDomains;


        /// <summary>
        /// Logs directory.
        /// </summary>
        public static string LogDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(mLogDirectory))
                {
                    mLogDirectory = Path.Combine(SystemContext.WebApplicationPhysicalPath, @"App_Data\CMSModules\WebAnalytics");
                    if (!Directory.Exists(mLogDirectory))
                    {
                        Directory.CreateDirectory(mLogDirectory);
                    }
                }
                return mLogDirectory;
            }
            set
            {
                if (mLogDirectory != value)
                {
                    mLogDirectory = value;
                    if (!string.IsNullOrEmpty(mLogDirectory) && !Directory.Exists(mLogDirectory))
                    {
                        Directory.CreateDirectory(mLogDirectory);
                    }
                }
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Writes the hit log to the file.
        /// </summary>
        /// <param name="codeName">Statistics codename</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        [HideFromDebugContext]
        public static void LogHit(string codeName, string siteName, string culture, string objectName, int objectId)
        {
            LogHit(codeName, siteName, culture, objectName, objectId, 0.0);
        }


        /// <summary>
        /// Writes the hit log to the file.
        /// </summary>
        /// <param name="codeName">Statistics codename</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="value">Hit value</param>
        [HideFromDebugContext]
        public static void LogHit(string codeName, string siteName, string culture, string objectName, int objectId, double value)
        {
            LogHit(codeName, siteName, culture, objectName, objectId, 1, value);
        }


        /// <summary>
        /// Writes the hit log to the file.
        /// </summary>
        /// <param name="codeName">Statistics codename</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Hit count</param>
        [HideFromDebugContext]
        public static void LogHit(string codeName, string siteName, string culture, string objectName, int objectId, int count)
        {
            LogHit(codeName, siteName, culture, objectName, objectId, count, 0);
        }


        /// <summary>
        /// Writes the hit log to the file.
        /// </summary>
        /// <param name="codeName">Statistics codename</param>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Hit count</param>
        /// <param name="value">Hit value</param>
        [HideFromDebugContext]
        public static void LogHit(string codeName, string siteName, string culture, string objectName, int objectId, int count, double value)
        {
            // Check license & crawler request
            string domain = RequestContext.CurrentDomain;
            if (!IsDomainAllowed(domain) || SearchCrawler.IsCrawlerRequest())
            {
                return;
            }

            try
            {
                if (LogDirectory == null)
                {
                    return;
                }

                // Log the debug information
                AnalyticsDebug.LogAnalyticsOperation(codeName, siteName, culture, objectName, objectId, count, value);

                // Log the record to the file
                codeName = codeName?.ToLowerInvariant();
                var now = DateTime.Now;

                // "Encode" macro
                objectName = MacroProcessor.EncodeMacro(objectName);

                // Fix semicolons in object name if is defined
                if (!string.IsNullOrEmpty(objectName))
                {
                    objectName = objectName.Replace(";", SEMICOLONDELIMITER);
                }

                var fileName = AnalyticsFileHelper.GetFileName(codeName, now);

                var logStr = siteName + ";" + culture + ";" + objectName + ";" + objectId + ";" + count + ";" + value.ToString(CultureHelper.EnglishCulture) + "\r\n";

                if (UseMemoryStorage)
                {
                    MemStorage.AppendAllText(logStr, fileName, now);
                }
                else
                {
                    var filePath = DirectoryHelper.CombinePath(LogDirectory, fileName);
                    lock (mLockingTarget)
                    {
                        File.AppendAllText(filePath, logStr, Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("WebAnalytics", "LogHit", ex);
            }
        }


        /// <summary>
        /// Writes the hit log to the file for page view.
        /// Logs page view for the given page with the given culture.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Document culture code</param>
        /// <param name="objectName">Node alias path</param>
        /// <param name="objectId">Node ID</param>
        public static void LogPageView(string siteName, string culture, string objectName, int objectId)
        {
            // Log page view
            if (AnalyticsHelper.TrackPageViewsEnabled(siteName))
            {
                LogHit(PAGE_VIEWS, siteName, culture, objectName, objectId);
            }
        }


        /// <summary>
        /// Writes the hit log to the file for all available conversions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="value">Hit value</param>
        public static void LogConversions(string siteName, string culture, string objectName, int objectId, double value)
        {
            LogConversions(siteName, culture, objectName, objectId, 1, value);
        }


        /// <summary>
        /// Writes the hit log to the file for all available conversions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Site culture code</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Hit count</param>
        /// <param name="value">Hit value</param>
        public static void LogConversions(string siteName, string culture, string objectName, int objectId, int count, double value)
        {
            // Do not log crawler request
            if (SearchCrawler.IsCrawlerRequest())
            {
                return;
            }

            // Prepare class for event
            LogRecord conversion = new LogRecord()
            {
                CodeName = CONVERSIONS,
                Culture = culture,
                SiteName = siteName,
                Value = value,
                Hits = count,
                ObjectName = objectName,
                ObjectId = objectId,
            };

            // Start event
            using (var h = WebAnalyticsEvents.LogConversion.StartEvent(conversion))
            {
                if (h.CanContinue())
                {
                    LogHit(CONVERSIONS, siteName, culture, objectName, objectId, count, value);
                }
                h.FinishEvent();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns true if web analytics is allowed for specified domain.
        /// </summary>
        /// <param name="domain">Domain name</param>
        private static bool IsDomainAllowed(string domain)
        {
            // Get result from hashtable
            bool? result = Domains[domain];
            if (result == null)
            {
                // Check the specified domain and store the result in hashtable
                result = LicenseHelper.CheckFeature(domain, FeatureEnum.WebAnalytics);
                Domains[domain] = result;
            }

            return result.Value;
        }

        #endregion
    }


    /// <summary>
    /// Memory storage class provides a functionality to keep analytics records in memory during log phase
    /// instead of immediate disk write. 
    /// </summary>
    internal class MemoryStorage
    {
        #region "Variables"

        // Directory create thread locker
        private readonly object locker = new object();

        // Collection of virtual directories
        private static readonly Dictionary<string, VirtualDirectory> dirs = new Dictionary<string, VirtualDirectory>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the storage name
        /// </summary>
        protected string StorageName
        {
            get;
            set;
        }


        /// <summary>
        /// Physical directory path
        /// </summary>
        protected string DirectoryPath
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Appends the string value to the specific virtual file and directory
        /// </summary>
        /// <param name="value">Value to store</param>
        /// <param name="fileName">File name</param>
        /// <param name="dirDate">Directory date</param>
        public void AppendAllText(string value, string fileName, DateTime dirDate)
        {
            VirtualDirectory md = GetDirectory(dirDate);
            md.AppendAllText(fileName, value);
        }


        /// <summary>
        /// Returns virtual directory for specified name
        /// </summary>
        /// <param name="dirDate">Directory date</param>
        private VirtualDirectory GetDirectory(DateTime dirDate)
        {
            // Create unique directory name for current storage
            string dirName = dirDate.ToString("yyMMddHHmm") + StorageName;
            // Check whether directory exist
            if (!dirs.ContainsKey(dirName))
            {
                lock (locker)
                {
                    // Create new directory if not exist
                    if (!dirs.ContainsKey(dirName))
                    {
                        // Add directory to the collection
                        dirs.Add(dirName, new VirtualDirectory(DirectoryPath));
                        int aprox = 60 - dirDate.Second + 1;

                        var key = "MemStorage|" + dirName;
                        
                        // Create fake cache record to get callback method after x second to the next full minute + 20 seconds
                        CacheHelper.RegisterAutomaticCallback(key, dirDate.AddSeconds(aprox + 20), () =>
                        {
                            CMSThread.AllowEmptyContext();
                            SaveVirtualFiles(dirName);
                        }, false);
                    }
                }
            }

            // Return directory
            return dirs[dirName];
        }


        /// <summary>
        /// Callback function from cache re
        /// </summary>
        /// <param name="dirName">Directory name</param>
        private static void SaveVirtualFiles(string dirName)
        {
            // Get directory from collection
            var vd = dirs[dirName];
            if (vd != null)
            {
                // Save all virtual files in directory
                vd.SaveAllFiles();

                // Remove virtual directory from collection
                dirs.Remove(dirName);
            }
        }


        /// <summary>
        /// Constructor to initialize storage
        /// </summary>
        /// <param name="storageName">Sets the storage name</param>
        /// <param name="dirPath">Path to the physical directory</param>
        public MemoryStorage(string storageName, string dirPath)
        {
            StorageName = storageName;
            DirectoryPath = dirPath;
        }

        #endregion
    }


    /// <summary>
    /// Virtual directory class stores directories and their virtual files
    /// </summary>
    internal class VirtualDirectory
    {
        #region "Variables"

        // Collection of virtual files
        private readonly Dictionary<string, StreamWriter> files = new Dictionary<string, StreamWriter>();

        // File create thread locker
        private readonly object locker = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Physical directory path
        /// </summary>
        protected string DirectoryPath
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Saves all files in directory to the file system
        /// </summary>
        public void SaveAllFiles()
        {
            // Loop thru all files
            foreach (string key in files.Keys)
            {
                // Get file string builder
                StreamWriter sb = files[key];

                // Go to the beginning of the stream
                sb.BaseStream.Position = 0;

                // Create full path
                string file = DirectoryHelper.CombinePath(DirectoryPath, key);

                // Create new file
                using (FileStream fs = FileStream.New(file, FileMode.Append))
                {
                    // File stream writer
                    using (StreamWriter sw = StreamWriter.New(fs, Encoding.UTF8))
                    {
                        // Log stream reader
                        using (StreamReader sr = StreamReader.New(sb.BaseStream, Encoding.UTF8))
                        {
                            sw.Write(sr.ReadToEnd());
                        }
                    }
                }

                sb.BaseStream.Dispose();
                sb.Dispose();
            }
        }


        /// <summary>
        /// Appends  specified content to the file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <param name="content">String content</param>
        public void AppendAllText(string filename, string content)
        {
            StreamWriter sb = GetFile(filename);
            lock (sb)
            {
                sb.Write(content);
                sb.Flush();
            }
        }


        /// <summary>
        /// Returns stream writer representing specific file 
        /// </summary>
        /// <param name="filename">File name</param>
        private StreamWriter GetFile(string filename)
        {
            // Case insensitive file name
            filename = filename?.ToLowerInvariant();
            // Check whether file exist
            if (!files.ContainsKey(filename))
            {
                lock (locker)
                {
                    // If file not exist create new
                    if (!files.ContainsKey(filename))
                    {
                        var ms = new SystemIO.MemoryStream();
                        StreamWriter sw = StreamWriter.New(ms, Encoding.UTF8);
                        files.Add(filename, sw);
                    }
                }
            }

            // Return StreamWriter for specified filename
            return files[filename];
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dirPath">Physical directory path</param>
        public VirtualDirectory(string dirPath)
        {
            DirectoryPath = dirPath;
        }

        #endregion
    }
}