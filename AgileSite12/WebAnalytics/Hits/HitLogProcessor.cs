using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Scheduler;
using CMS.SiteProvider;

using CultureInfo = System.Globalization.CultureInfo;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// HitLogProcessor class.
    /// </summary>
    public class HitLogProcessor : ITask
    {
        // Number of minutes of inactivity after which the user is considered exited
        private const int USER_INACTIVITY_THRESHOLD = 20;
       

        /// <summary>
        /// Executes the logprocessor action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                ProcessFinishedLogs();
                ProcessExitPages();
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("WebAnalytics", "HitLogProcessor", e);
                return e.Message;
            }

            return null;
        }
        

        /// <summary>
        /// Creates log files for expired exit page candidates and removes exit pages records.
        /// </summary>
        public void ProcessExitPages()
        {
            var expireTime = DateTime.Now.AddMinutes(-USER_INACTIVITY_THRESHOLD);
            var dataLoaded = true;

            while (dataLoaded)
            {
                // Get expired exit pages
                using (var tr = new CMSTransactionScope())
                {
                    var exitPages = ExitPageInfoProvider.GetExitPages().TopN(50).Where("ExitPageLastModified", QueryOperator.LessThan, expireTime).ToList();

                    if (exitPages.Count > 0)
                    {
                        foreach (var exitPage in exitPages)
                        {
                            var siteName = SiteInfoProvider.GetSiteName(exitPage.ExitPageSiteID);
                            if (!String.IsNullOrEmpty(siteName))
                            {
                                // Log exit page
                                AnalyticsHelper.LogExitPage(siteName, exitPage.ExitPageCulture, null, exitPage.ExitPageNodeID, 1);
                            }
                            exitPage.Delete();
                        }
                    }
                    else
                    {
                        dataLoaded = false;
                    }
                    tr.Commit();
                }
            }
        }


        /// <summary>
        /// Gets the log files that are finished (are no longer valid).
        /// </summary>
        public void ProcessFinishedLogs()
        {
            // Check the directory
            if ((HitLogProvider.LogDirectory != null) && (Directory.Exists(HitLogProvider.LogDirectory)))
            {
                // Get all logs from '~/App_Data/CMSModules/WebAnalytics/'
                DirectoryInfo dir = DirectoryInfo.New(HitLogProvider.LogDirectory);
                foreach (FileInfo f in dir.GetFiles("*.log"))
                {
                    DateTime creationTime;
                    try
                    {
                        creationTime = GetCreationTimeFromFileName(f.Name);
                    }
                    catch (Exception ex)
                    {
                        LogExceptionOnlyOnceADay(ex, "GetCreationTimeFromFileName");
                        continue;
                    }
                    // Check that files creation time is different (greater) than actual time in minutes                     
                    if (creationTime.ToString("yyMMddHHmm") != DateTime.Now.ToString("yyMMddHHmm"))
                    {
                        // Save the log content to DB
                        ProcessLog(f.Name);
                    }
                }
            }
        }


        /// <summary>
        /// Processes the whole log file in memory and writes the results to the database.
        /// </summary>
        /// <param name="fileName">Log file name</param>
        /// <param name="codeName">Statistics code name</param>
        public void ProcessLog(string fileName, string codeName = null)
        {
            // Check the file existence and its minimal length (with extension - '.log')
            string filePath = DirectoryHelper.CombinePath(HitLogProvider.LogDirectory, fileName);
            if (File.Exists(filePath) && (fileName.Length > 16))
            {
                fileName = AnalyticsFileHelper.RemoveMachineName(fileName);

                // File name has format <codename>_<YY><MM><DD>_<HH><mm>
                int underscoreIndexTime = fileName.LastIndexOf("_", StringComparison.Ordinal);

                // We are searching for last underscore before found one. 
                // LastIndexOf moves backwards, we have to begin on previous search-1 (we do not want find same underscore again)
                // and then we have to move backwards same number of iterations to go to beginning of string.
                int underscoreIndexCodeName = fileName.LastIndexOf('_', underscoreIndexTime - 1, underscoreIndexTime - 1);

                if (string.IsNullOrEmpty(codeName))
                {
                    // Get codename from file name
                    codeName = fileName.Substring(0, underscoreIndexCodeName);
                }

                // Check both indexes are greater than 0             
                if ((underscoreIndexTime > 0) && (underscoreIndexCodeName > 0))
                {
                    // Create log creation time
                    DateTime creationTime = GetCreationTimeFromFileName(fileName);

                    // Hashtable key = 'siteName;culture;objectName;objectID'
                    // Hashtable value = object[2] {'hits', 'values'}
                    Hashtable records = new Hashtable();

                    // Read the data
                    StreamReader sr = File.OpenText(filePath);

                    string record;
                    List<double> valuesSet = new List<double>();

                    // Loop thru all records
                    while ((record = sr.ReadLine()) != null)
                    {
                        // Split items by semicolon
                        var items = record.Split(';');

                        if (items.Length > 4)
                        {
                            if (codeName.Equals(HitLogProvider.EXITPAGECANDIDATE, StringComparison.OrdinalIgnoreCase))
                            {
                                string[] exitValue = items[2].Split('#');
                                if (exitValue.Length > 1)
                                {
                                    long ticks = ValidationHelper.GetLong(exitValue[0], 0);

                                    // key = session ID, value = [DateTime, NodeID, SiteID, Culture]
                                    records[exitValue[1]] = new object[] { new DateTime(ticks), items[3], SiteInfoProvider.GetSiteID(items[0]), items[1] };
                                }
                            }
                            else
                            {
                                // Create item key (siteName;culture;objectName;objectID)
                                string key = items[0] + ";" + items[1] + ";" + items[2] + ";" + items[3];

                                // Get hits and values
                                object[] valuecount = records[key] as object[];
                                if (valuecount == null)
                                {
                                    // Default valuecount - convert value to double with default culture
                                    valuecount = new object[] { 0, Convert.ToDouble(0, CultureHelper.EnglishCulture) };
                                }

                                // Increase hits
                                valuecount[0] = ValidationHelper.GetInteger(valuecount[0], 0) + ValidationHelper.GetInteger(items[4], 0);

                                // Increase values
                                if (items.Length > 5)
                                {
                                    double value = ValidationHelper.GetDouble(items[5], 0, "en-us");
                                    valuecount[1] = ValidationHelper.GetDouble(valuecount[1], 0) + value;
                                    valuesSet.Add(value);
                                }

                                // Update hashtable value
                                records[key] = valuecount;
                            }
                        }
                    }

                    sr.Close();
                    sr.Dispose();

                    if (codeName.Equals(HitLogProvider.EXITPAGECANDIDATE, StringComparison.OrdinalIgnoreCase))
                    {
                        LogExitPageCandidates(records);

                        // Delete the file
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                    }
                    else
                    {
                        int i = 0;

                        // Process within transaction
                        using (var tr = new CMSTransactionScope())
                        {
                            // Process each line from hashtable to DB
                            foreach (DictionaryEntry item in records)
                            {
                                string key = ValidationHelper.GetString(item.Key, String.Empty);

                                if (key != "")
                                {
                                    // Split the key
                                    string[] keys = key.Split(';');

                                    object[] valueCount = (object[]) item.Value;

                                    LogRecord logRecord = new LogRecord()
                                    {
                                        LogTime = creationTime,
                                        CodeName = codeName,
                                        Culture = ValidationHelper.GetString(keys[1], String.Empty),
                                        SiteName = ValidationHelper.GetString(keys[0], String.Empty),
                                        Value = ValidationHelper.GetDouble(valueCount[1], 0),
                                        Hits = ValidationHelper.GetInteger(valueCount[0], 0),
                                        ObjectName = ValidationHelper.GetString(keys[2], String.Empty).Replace(HitLogProvider.SEMICOLONDELIMITER, ";"),
                                        ObjectId = ValidationHelper.GetInteger(keys[3], 0),
                                        ValuesSet = valuesSet,
                                    };

                                    // Try to process a log record - exception might occur for example when statistics were saved on license higher than the current
                                    try
                                    {
                                        ProcessLogRecord(logRecord);
                                    }
                                    catch (Exception e)
                                    {
                                        EventLogProvider.LogException("WebAnalytics", "HitLogProcessor", e);
                                    }
                                }
                                i++;

                                // Commit transaction after every 10 records
                                if (i >= 10)
                                {
                                    tr.CommitAndBeginNew();
                                    i = 0;
                                }
                            }

                            // Delete the file
                            File.SetAttributes(filePath, FileAttributes.Normal);
                            File.Delete(filePath);

                            // Commit transaction if necessary
                            tr.Commit();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Splits log into time intervals (days, hours, etc.) and saves it to the database.
        /// </summary>
        /// <param name="logRecord">Log record data</param>
        public static void SaveLogToDatabase(LogRecord logRecord)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();

            // Do not log if site not exists
            SiteInfo si = SiteInfoProvider.GetSiteInfo(logRecord.SiteName);
            if (si == null)
            {
                return;
            }
            parameters.Add("@SiteID", si.SiteID);

            // Culture code
            parameters.Add("@Culture", logRecord.Culture, !String.IsNullOrEmpty(logRecord.Culture));

            // Object name
            parameters.Add("@ObjectName", logRecord.ObjectName, !String.IsNullOrEmpty(logRecord.ObjectName));

            // Object ID
            parameters.AddId("@ObjectID", logRecord.ObjectId);

            parameters.Add("@Hits", logRecord.Hits);

            DateTime logTime = logRecord.LogTime;

            DateTime hourStart = DateTimeHelper.GetHourStart(logTime);
            parameters.Add("@HourStart", hourStart);

            DateTime dayStart = DateTimeHelper.GetDayStart(logTime);
            parameters.Add("@DayStart", dayStart);

            DateTime weekStart = DateTimeHelper.GetWeekStart(logTime);
            parameters.Add("@WeekStart", weekStart);

            DateTime monthStart = DateTimeHelper.GetMonthStart(logTime);
            parameters.Add("@MonthStart", monthStart);

            DateTime yearStart = DateTimeHelper.GetYearStart(logTime);
            parameters.Add("@YearStart", yearStart);

            parameters.Add("@HourEnd", hourStart.AddHours(1));
            parameters.Add("@DayEnd", dayStart.AddDays(1));
            parameters.Add("@WeekEnd", weekStart.AddDays(7));
            parameters.Add("@MonthEnd", monthStart.AddMonths(1));
            parameters.Add("@YearEnd", yearStart.AddYears(1));

            parameters.Add("@Codename", logRecord.CodeName);
            parameters.Add("@Value", logRecord.Value);

            // Write the data to the database
            ConnectionHelper.ExecuteQuery("analytics.statistics.processlog", parameters);
        }


        /// <summary>
        /// Loops thru all exit page candidates and logs them into DB (only when there are no candidates for given session or the candidate is newer).
        /// </summary>
        internal void LogExitPageCandidates(Hashtable records)
        {
            foreach (string sessionID in records.Keys)
            {
                if (!string.IsNullOrEmpty(sessionID))
                {
                    object[] values = records[sessionID] as object[];

                    var parameters = new QueryDataParameters
                    {
                        { "SessionID", sessionID },
                        { "NodeID", values[1] },
                        { "Date", values[0] },
                        { "SiteID", values[2] },
                        { "Culture", values[3] }
                    };

                    ConnectionHelper.ExecuteQuery("analytics.statistics.logexitpage", parameters);
                }
            }
        }


        /// <summary>
        /// Processes the log record to DB.
        /// </summary>
        /// <param name="logRecord">Log record data</param>
        private void ProcessLogRecord(LogRecord logRecord)
        {
            if (logRecord.Hits <= 0)
            {
                return;
            }

            // Do not log if object name or ID or site name not specified
            if ((String.IsNullOrEmpty(logRecord.SiteName)) || ((String.IsNullOrEmpty(logRecord.ObjectName)) && (logRecord.ObjectId <= 0)))
            {
                return;
            }

            // Start event
            using (var h = WebAnalyticsEvents.ProcessLogRecord.StartEvent(logRecord))
            {
                if (h.CanContinue())
                {
                    if (logRecord.CodeName.Equals("conversion", StringComparison.OrdinalIgnoreCase))
                    {
                        // Don't log, if conversion does not exist (event WebAnalyticsEvents.ProcessLogRecord.Failed will be called in that case)
                        if (!ConversionExists(logRecord.ObjectName, logRecord.SiteName))
                        {
                            return;
                        }
                    }

                    SaveLogToDatabase(logRecord);

                }
                h.FinishEvent();
            }
        }


        /// <summary>
        /// Returns false if conversion does not exists
        /// </summary>
        /// <param name="conversionName">Conversion name</param>
        /// <param name="siteName">Site name</param>
        private bool ConversionExists(string conversionName, string siteName)
        {
            // Find conversion by conversion code name 
            ConversionInfo ci = ConversionInfoProvider.GetConversionInfo(conversionName, siteName);
            return (ci != null);
        }


        /// <summary>
        ///  Returns creation time from file name.
        /// </summary>
        /// <param name="fileName">File name</param>
        private DateTime GetCreationTimeFromFileName(string fileName)
        {
            fileName = AnalyticsFileHelper.RemoveMachineName(fileName);

            // File name has format <codename>_<YY><MM><DD>_<HH><mm>
            int underscoreIndexTime = fileName.LastIndexOf("_", StringComparison.Ordinal);

            // We are searching for last underscore before found one. 
            // LastIndexOf moves backwards, we have to begin on previous search-1 (we do not want find same underscore again)
            // and then we have to move backwards same number of iterations to go to beginning of string.
            int underscoreIndexCodeName = fileName.LastIndexOf('_', underscoreIndexTime - 1, underscoreIndexTime - 1);

            // Check both indexes are greater than 0             
            if ((underscoreIndexTime > 0) && (underscoreIndexCodeName > 0))
            {
                // Get timestamp <YY><MM><DD>_<HH><mm> from file name
                string time = fileName.Substring(underscoreIndexCodeName + 1, 11);

                // Create log creation time
                DateTime creationTime = new DateTime(CultureInfo.InvariantCulture.Calendar.ToFourDigitYear(ValidationHelper.GetInteger(time.Substring(0, 2), 0)),
                                                     ValidationHelper.GetInteger(time.Substring(2, 2), 0),
                                                     ValidationHelper.GetInteger(time.Substring(4, 2), 0),
                                                     ValidationHelper.GetInteger(time.Substring(7, 2), 0),
                                                     ValidationHelper.GetInteger(time.Substring(9, 2), 0), 0);
                return creationTime;
            }
            return DateTimeHelper.ZERO_TIME;
        }


        private static void LogExceptionOnlyOnceADay(Exception ex, string methodName)
        {
            CacheHelper.Cache(() =>
            {
                EventLogProvider.LogException("WebAnalytics", methodName, ex);
                return true;
            }, new CacheSettings(TimeSpan.FromDays(1).TotalMinutes, "CMS", methodName));
        }
    }
}