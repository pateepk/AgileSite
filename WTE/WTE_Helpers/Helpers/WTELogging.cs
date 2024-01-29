using System;
using System.Text;
using System.Web;
using WTE.Configuration;

namespace WTE.Helpers
{
    /// <summary>
    /// PSE Error logging Manager
    /// </summary>
    [Serializable()]
    public class WTELogging
    {
        #region members

        private bool _isTimerStarted = false;
        private DateTime _startTime;
        private DateTime _endTime;
        private HttpContext _httpContext = null;

        #endregion members

        #region Properties

        /// <summary>
        /// Start timer
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
            }
        }

        /// <summary>
        /// End timer
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
            set
            {
                _endTime = value;
            }
        }

        /// <summary>
        /// Flag indicating that the activity time is running
        /// </summary>
        public bool IsTimerStarted
        {
            get
            {
                return _isTimerStarted;
            }
            set
            {
                _isTimerStarted = value;
            }
        }

        /// <summary>
        /// The current context
        /// </summary>
        public HttpContext Context
        {
            get
            {
                return _httpContext;
            }
            set
            {
                _httpContext = value;
            }
        }

        #endregion Properties

        #region constructor

        /// <summary>
        /// Constructor - set context
        /// </summary>
        public WTELogging()
        {
            if (HttpContext.Current != null)
            {
                _httpContext = HttpContext.Current;
            }
        }

        /// <summary>
        /// Constructor set context
        /// </summary>
        /// <param name="p_context"></param>
        public WTELogging(HttpContext p_context)
        {
            _httpContext = p_context;
        }

        #endregion constructor

        #region log message builder

        #region text helper

        private const string EndOfDataTag = "[[_WTE_END_OF_DATA_]]";
        private const string NewLineTag = "[[_WTE_NEWLINE_]]";
        private const string SpaceTag = "[[_WTE_SPACE_]]";
        private const string CRTag = "[[_WTE_CR_]]";
        private const string LFTag = "[[_WTE_LF_]]";

        /// <summary>
        /// Get clean text
        /// </summary>
        /// <param name="p_text"></param>
        /// <param name="p_ishtml"></param>
        /// <returns></returns>
        private static string GetCleanText(string p_text, bool p_ishtml)
        {
            string ret = p_text;
            bool ishtml = false;
            string linebreak = WTETextHelper.GetNewLine(ishtml);
            string space = WTETextHelper.GetSpace(ishtml);

            ret += EndOfDataTag;

            // clean "\r\n" and "\n" and "\r"
            ret = ret.Replace("\r", CRTag).Replace("\n", LFTag);
            ret = ret.Replace(CRTag + LFTag, NewLineTag);
            ret = ret.Replace(LFTag + CRTag, NewLineTag);
            ret = ret.Replace(CRTag, NewLineTag);
            ret = ret.Replace(LFTag, NewLineTag);

            // clean "<br>", <br />, "<br/>"
            ret = ret.Replace("<br>", NewLineTag);
            ret = ret.Replace("<br/>", NewLineTag);
            ret = ret.Replace("<br >", NewLineTag);
            ret = ret.Replace("<br />", NewLineTag);

            // clean up space
            ret = ret.Replace(" ", SpaceTag).Replace("&nbsp;", SpaceTag);

            ret = ret.Replace(NewLineTag + EndOfDataTag, NewLineTag); // clean up the last line and add an extra EOL.
            ret = ret.Replace(EndOfDataTag, NewLineTag); // clean up the last line and add an extra EOL.

            ret = ret.Replace(NewLineTag, linebreak).Replace(SpaceTag, space);

            return ret;
        }

        /// <summary>
        /// Get section break
        /// </summary>
        /// <param name="p_num"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        private static string GetSectionBreak(int p_num)
        {
            string ret = String.Empty;
            string chars = "*";
            string newlinechar = NewLineTag;
            ret = String.Format("{0}{1}{2}", newlinechar, chars, newlinechar);
            return ret;
        }

        /// <summary>
        /// Get a log section (separated caption and info)
        /// </summary>
        /// <param name="p_caption"></param>
        /// <param name="p_info"></param>
        /// <param name="p_ignoreBlank"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        private static string GetLogSection(string p_caption, string p_info, bool p_ignoreBlank, bool p_isHtml)
        {
            return GetFormattedLine(p_caption, p_info, 0, p_ignoreBlank, true, true, true, p_isHtml);
        }

        /// <summary>
        /// Get a log line (caption on the same line as info)
        /// </summary>
        /// <param name="p_caption"></param>
        /// <param name="p_info"></param>
        /// <param name="p_ignoreBlank"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        private static string GetLogLine(string p_caption, string p_info, bool p_ignoreBlank, bool p_isHtml)
        {
            return GetFormattedLine(p_caption, p_info, 0, p_ignoreBlank, false, true, false, p_isHtml);
        }

        /// <summary>
        /// Make an information line
        /// </summary>
        /// <param name="p_caption"></param>
        /// <param name="p_info"></param>
        /// <param name="p_numPadChars"></param>
        /// <param name="p_ignoreBlank"></param>
        /// <param name="p_infoOnNextLine"></param>
        /// <param name="p_hasLineBreak"></param>
        /// <param name="p_hasLeadingLineBreak"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        private static string GetFormattedLine(string p_caption, string p_info, int p_numPadChars, bool p_ignoreBlank, bool p_infoOnNextLine, bool p_hasLineBreak, bool p_hasLeadingLineBreak, bool p_isHtml)
        {
            string logLine = String.Empty;

            if (!p_ignoreBlank || !String.IsNullOrWhiteSpace(p_info))
            {
                #region add caption

                if (p_hasLeadingLineBreak)
                {
                    logLine += WTETextHelper.GetNewLine(p_isHtml);
                }

                if (p_isHtml)
                {
                    logLine += "<b>" + p_caption.Replace(" ", WTETextHelper.GetSpace(p_isHtml)) + WTETextHelper.GetSpaces(p_numPadChars + 1, p_isHtml) + ":</b>";
                }
                else
                {
                    logLine += p_caption + WTETextHelper.GetSpaces(p_numPadChars + 1, p_isHtml) + ":";
                }

                #endregion add caption

                #region add a new line or padding after the caption

                if (p_infoOnNextLine)
                {
                    // add a line break
                    logLine += WTETextHelper.GetNewLine(p_isHtml);
                }
                else
                {
                    // add a space
                    logLine += WTETextHelper.GetSpace(p_isHtml);
                }

                #endregion add a new line or padding after the caption

                #region add the info

                // make sure the new line is correct
                logLine += p_info.Replace(WTETextHelper.GetNewLine(!p_isHtml), WTETextHelper.GetNewLine(p_isHtml));

                if (p_hasLineBreak)
                {
                    // add a linebreak at the end
                    logLine += WTETextHelper.GetNewLine(p_isHtml);
                }

                #endregion add the info
            }

            return logLine;
        }

        #endregion text helper

        #region parts factory

        /// <summary>
        /// Get Exception data as ActivityLog long data
        /// </summary>
        /// <param name="p_exception"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetActivityLogMessage(Exception p_exception, bool p_isHtml)
        {
            string message = String.Empty;
            if (p_exception != null)
            {
                if (p_exception is Exception)
                {
                    Exception pseEx = p_exception as Exception;
                    message += pseEx.Message;
                }
                else
                {
                    message += p_exception.Message;
                }
                message += GetErrorString(p_exception, p_isHtml);
            }
            return message;
        }

        /// <summary>
        /// Get Exception details
        /// </summary>
        /// <param name="p_exception"></param>
        /// <param name="p_isHtml"></param>
        /// <returns></returns>
        public static string GetErrorString(Exception p_exception, bool p_isHtml)
        {
            StringBuilder sb = new StringBuilder();
            string message = String.Empty;

            if (p_exception != null)
            {
                string errorMessage = p_exception.Message;
                string stackTrace = p_exception.StackTrace;
                string additionalInfo = String.Empty;
                string extraMessage = String.Empty;
                string innerException = GetErrorString(p_exception.InnerException, p_isHtml);

                sb.Append(WTETextHelper.GetNewLine(p_isHtml));
                sb.Append(GetLogLine("Error Message", errorMessage, true, p_isHtml));
                sb.Append(GetLogLine("Extra Message", extraMessage, true, p_isHtml));
                sb.Append(GetLogSection("Stack Trace", stackTrace, true, p_isHtml));
                sb.Append(GetLogSection("Additional Info", additionalInfo, true, p_isHtml));
                sb.Append(GetLogSection("Inner Exception", innerException, true, p_isHtml));
            }

            return sb.ToString();
        }

        #endregion parts factory

        #endregion log message builder

        #region log exceptions

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="p_exception"></param>
        public static void LogException(Exception p_exception)
        {
            string message = GetErrorString(p_exception, false);
            LogMessage(message);
        }

        #endregion log exceptions

        #region database logging

        public static void LogError(Exception exception)
        {
        }

        #endregion database logging

        #region text file logging

        /// <summary>
        /// lock object
        /// </summary>
        private static object _wteLogLock = new object();

        /// <summary>
        /// Log message
        /// </summary>
        /// <param name="p_message"></param>
        public static void LogMessage(string p_message)
        {
            LogMessage(p_message, String.Empty);
        }

        /// <summary>
        /// Logg message
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_addTimeStamp"></param>
        public static void LogMessage(string p_message, bool p_addTimeStamp)
        {
            LogMessage(p_message, String.Empty, false, false, false, p_addTimeStamp);
        }

        /// <summary>
        /// Write to a text log with a file name
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        public static void LogMessage(string p_message, string p_fileName)
        {
            LogMessage(p_message, p_fileName, false);
        }

        /// <summary>
        /// Log a message with check to add request info.
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addRequestInfo"></param>
        public static void LogMessage(string p_message, string p_fileName, bool p_addRequestInfo)
        {
            LogMessage(p_message, p_fileName, p_addRequestInfo, true);
        }

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addRequestInfo"></param>
        /// <param name="p_addSection"></param>
        public static void LogMessage(string p_message, string p_fileName, bool p_addRequestInfo, bool p_addSection)
        {
            LogMessage(p_message, p_fileName, p_addRequestInfo, p_addSection, null);
        }

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addRequestInfo"></param>
        /// <param name="p_addSection"></param>
        /// <param name="p_maintainFullLogFile"></param>
        public static void LogMessage(string p_message, string p_fileName, bool p_addRequestInfo, bool p_addSection, bool? p_maintainFullLogFile)
        {
            LogMessage(p_message, p_fileName, p_addRequestInfo, p_addSection, p_maintainFullLogFile, true);
        }

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addRequestInfo"></param>
        /// <param name="p_addSection"></param>
        /// <param name="p_maintainFullLogFile"></param>
        /// <param name="p_addTimeStamp"></param>
        public static void LogMessage(string p_message, string p_fileName, bool p_addRequestInfo, bool p_addSection, bool? p_maintainFullLogFile, bool? p_addTimeStamp)
        {
            LogMessage(p_message, p_fileName, p_addRequestInfo, p_addSection, p_maintainFullLogFile, p_addTimeStamp, HttpContext.Current);
        }

        /// <summary>
        /// Write a text log
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addRequestInfo"></param>
        /// <param name="p_addSection"></param>
        /// <param name="p_maintainFullLogFile"></param>
        /// <param name="p_context"></param>
        public static void LogMessage(string p_message, string p_fileName, bool p_addRequestInfo, bool p_addSection, bool? p_maintainFullLogFile, HttpContext p_context)
        {
            LogMessage(p_message, p_fileName, p_addRequestInfo, p_addSection, p_maintainFullLogFile, true, p_context);
        }

        /// <summary>
        /// Write a text log
        /// </summary>
        /// <param name="p_message"></param>
        /// <param name="p_fileName"></param>
        /// <param name="p_addRequestInfo"></param>
        /// <param name="p_addSection"></param>
        /// <param name="p_maintainFullLogFile"></param>
        /// <param name="p_addTimeStamp"></param>
        /// <param name="p_context"></param>
        public static void LogMessage(string p_message, string p_fileName, bool p_addRequestInfo, bool p_addSection, bool? p_maintainFullLogFile, bool? p_addTimeStamp, HttpContext p_context)
        {
            lock (_wteLogLock)
            {
                bool ishtml = false;
                string data = String.Empty;
                string filename = p_fileName;
                string originalFileName = String.Empty;
                string directory = String.Empty;
                string sectionBreak = GetSectionBreak(80);

                if (String.IsNullOrWhiteSpace(filename))
                {
                    filename = DebugConfiguration.LogFileName;
                }

                filename = GetLogFilePath(filename
                    , DebugConfiguration.AddStoreNameToLogFile
                    , DebugConfiguration.AddMachineNameToLogFile
                    , DebugConfiguration.AddDateToLogFile
                    , DebugConfiguration.AddTimeToLogFile
                    , p_maintainFullLogFile.GetValueOrDefault(DebugConfiguration.MaintainMainLogFile)
                    , out originalFileName
                    , out directory);

                if (!String.IsNullOrWhiteSpace(p_message))
                {
                    if (p_addSection)
                    {
                        data += sectionBreak;
                        if (p_addTimeStamp.GetValueOrDefault(true))
                        {
                            data += String.Format("TimeStamp:{0}{1}", DateTime.Now, NewLineTag);
                        }
                    }
                    else
                    {
                        if (p_addTimeStamp.GetValueOrDefault(true))
                        {
                            data += "TimeStamp: " + DateTime.Now.ToString() + " - ";
                        }
                    }

                    data += p_message;

                    if (p_addSection)
                    {
                        data += sectionBreak;
                    }
                }

                if (!String.IsNullOrWhiteSpace(data))
                {
                    data = GetCleanText(data, ishtml);
                    WTEStorageManager.AppendTextFile(data, filename, p_context);

                    if (!String.IsNullOrWhiteSpace(originalFileName)
                        && filename != originalFileName)
                    {
                        WTEStorageManager.AppendTextFile(data, originalFileName, p_context);
                    }
                }
            }
        }

        /// <summary>
        /// Log a message to the ZNode.Log File
        /// </summary>
        /// <param name="p_message"></param>
        private static void LogToZNodeLogFile(string p_message)
        {
            try
            {
                WTELogging.LogMessage(p_message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get log file path
        /// </summary>
        /// <param name="p_fileName"></param>
        /// <param name="p_addSiteName"></param>
        /// <param name="p_addMachineName"></param>
        /// <param name="p_addDate"></param>
        /// <param name="p_addTime"></param>
        /// <param name="p_getCleanFilePath"></param>
        /// <param name="p_orginalFilePath"></param>
        /// <returns></returns>
        private static string GetLogFilePath(string p_fileName, bool p_addSiteName, bool p_addMachineName, bool p_addDate, bool p_addTime, bool p_getCleanFilePath, out string p_orginalFilePath, out string p_directory)
        {
            string filePath = String.Empty;

            p_orginalFilePath = String.Empty;
            p_directory = String.Empty;

            filePath = WTEStorageManager.GetFileName(WTEFileType.LOG, p_fileName, p_addSiteName, p_addMachineName, p_addDate, p_addTime, p_getCleanFilePath, out p_orginalFilePath, out p_directory);

            if (!String.IsNullOrWhiteSpace(filePath))
            {
                if (!String.IsNullOrWhiteSpace(p_directory))
                {
                    filePath = System.IO.Path.Combine(p_directory, filePath);
                }
                else
                {
                    filePath = System.IO.Path.Combine(DebugConfiguration.LogFolder, filePath);
                }
            }

            if (!String.IsNullOrWhiteSpace(p_orginalFilePath))
            {
                p_orginalFilePath = System.IO.Path.Combine(DebugConfiguration.LogFolder, p_orginalFilePath);
            }

            return filePath;
        }

        #endregion text file logging
    }
}