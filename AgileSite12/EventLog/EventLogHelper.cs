using System;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.EventLog
{
    ///<summary>
    /// Class for event helper methods
    ///</summary>
    public class EventLogHelper : AbstractHelper<EventLogHelper>
    {
        #region "Variables"

        /// <summary>
        /// Indicates if events are logged to the file.
        /// </summary>
        private bool? mLogEventsToFile = null;

        /// <summary>
        /// Logs file path.
        /// </summary>
        private string mLogFile = null;

        /// <summary>
        /// Folder where log file will be saved.
        /// </summary>
        private string mLogFolder = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Physical path to folder where log file is saved.
        /// </summary>
        public static string LogFolder
        {
            get
            {
                return HelperObject.LogFolderInternal;
            }
            set
            {
                HelperObject.LogFolderInternal = value;
            }
        }


        /// <summary>
        /// Physical path to the log file.
        /// </summary>
        public static string LogFile
        {
            get
            {
                return HelperObject.LogFileInternal;
            }
        }


        /// <summary>
        /// Indicates if events are logged to the file.
        /// </summary>
        public static bool LogEventsToFile
        {
            get
            {
                return HelperObject.LogEventsToFileInternal;
            }
            set
            {
                HelperObject.LogEventsToFileInternal = value;
            }
        }

        #endregion


        #region "Internal Properties"

        /// <summary>
        /// Physical path to folder where log file is saved.
        /// </summary>
        protected virtual string LogFolderInternal
        {
            get
            {
                if (mLogFolder == null)
                {
                    mLogFolder = SystemContext.WebApplicationPhysicalPath + "\\App_Data";
                }

                return mLogFolder;
            }
            set
            {
                mLogFolder = value;
            }
        }


        /// <summary>
        /// Physical path to the log file.
        /// </summary>
        protected virtual string LogFileInternal
        {
            get
            {
                if (mLogFile == null)
                {
                    mLogFile = Path.Combine(LogFolder, "logEvents.log");

                    // Ensure the directory
                    DirectoryHelper.EnsureDiskPath(mLogFile, SystemContext.WebApplicationPhysicalPath);
                }

                return mLogFile;
            }
        }


        /// <summary>
        /// Indicates if events are logged to the file.
        /// </summary>
        protected virtual bool LogEventsToFileInternal
        {
            get
            {
                if (mLogEventsToFile == null)
                {
                    mLogEventsToFile = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSLogEventsToFile"], false);
                }

                return mLogEventsToFile.Value || SettingsKeyInfoProvider.GetBoolValue("CMSLogToFileSystem");
            }
            set
            {
                mLogEventsToFile = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Logs the query to file.
        /// </summary>
        /// <param name="eventType">Type of the event. I = information, E = error, W = warning</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (Security, Update, Delete, etc.)</param>
        /// <param name="eventDescription">Detailed description of the event</param>
        /// <param name="eventUrl">Event URL address</param>
        /// <param name="userId">ID of the user, who caused logged event</param>
        /// <param name="userName">Name of the user, who caused logged event</param>
        /// <param name="nodeId">ID value of the document</param>
        /// <param name="documentName">NamePath value of the document</param>
        /// <param name="ipAddress">IP Address of the user, who caused logged event</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="machineName">Name of machine</param>
        /// <param name="eventUrlReferrer">URL referrer</param>
        /// <param name="eventUserAgent">User agent</param>
        /// <param name="eventTime">Date and time when the event occurs</param>
        public static void LogEventToFile(string eventType, string source, string eventCode, string eventDescription, string eventUrl, int userId, string userName, int nodeId, string documentName, string ipAddress, int siteId, string machineName, string eventUrlReferrer, string eventUserAgent, DateTime eventTime)
        {
            HelperObject.LogEventToFileInternal(eventType, source, eventCode, eventDescription, eventUrl, userId, userName, nodeId, documentName, ipAddress, siteId, machineName, eventUrlReferrer, eventUserAgent, eventTime);
        }


        /// <summary>
        /// Logs the log object to file.
        /// </summary>
        /// <param name="eventLogInfo">Event info object</param>
        public static void LogEventToFile(EventLogInfo eventLogInfo)
        {
            HelperObject.LogEventToFileInternal(eventLogInfo);
        }


        /// <summary>
        /// Logs the query to file.
        /// </summary>
        /// <param name="eventObj">Event info object</param>
        /// <param name="siteName">Site name</param>
        public static void SendEmailNotification(EventLogInfo eventObj, string siteName)
        {
            HelperObject.SendEmailNotificationInternal(eventObj, siteName);
        }


        ///<summary>
        /// Gets log event text from DataRow
        ///</summary>
        ///<param name="eventObj">Object containing log event data</param>
        ///<returns>Text of log event</returns>
        public static string GetEventText(EventLogInfo eventObj)
        {
            return HelperObject.GetEventTextInternal(eventObj);
        }


        /// <summary>
        /// Gets the list of changed fields in the given object.
        /// </summary>
        /// <param name="bi">BaseInfo object</param>
        public static string GetChangedFields(BaseInfo bi)
        {
            return HelperObject.GetChangedFieldsInternal(bi);
        }


        /// <summary>
        /// Gets the list of fields in the given object.
        /// </summary>
        /// <param name="bi">BaseInfo object</param>
        public static string GetFields(BaseInfo bi)
        {
            return HelperObject.GetFieldsInternal(bi);
        }


        /// <summary>
        /// Returns user-friendly string for the given event type.
        /// </summary>
        /// <param name="eventType">Event type string</param>
        public static string GetEventTypeText(string eventType)
        {
            return HelperObject.GetEventTypeTextInternal(eventType);
        }


        /// <summary>
        /// Logs insertion of an object.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        public static void LogInsert(GeneralizedInfo infoObj)
        {
            HelperObject.LogInsertInternal(infoObj);
        }


        /// <summary>
        /// Logs update of an object.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        public static void LogUpdate(GeneralizedInfo infoObj)
        {
            HelperObject.LogUpdateInternal(infoObj);
        }


        /// <summary>
        /// Logs deletion of an object.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        public static void LogDelete(GeneralizedInfo infoObj)
        {
            HelperObject.LogDeleteInternal(infoObj);

        }

        #endregion


        #region "Internal Methods"

        /// <summary>
        /// Logs the query to file.
        /// </summary>
        /// <param name="eventType">Type of the event. I = information, E = error, W = warning</param>
        /// <param name="source">Source of the event (Content, Administration, etc.)</param>
        /// <param name="eventCode">Event code (Security, Update, Delete, etc.)</param>
        /// <param name="eventDescription">Detailed description of the event</param>
        /// <param name="eventUrl">Event URL address</param>
        /// <param name="userId">ID of the user, who caused logged event</param>
        /// <param name="userName">Name of the user, who caused logged event</param>
        /// <param name="nodeId">ID value of the document</param>
        /// <param name="documentName">NamePath value of the document</param>
        /// <param name="ipAddress">IP Address of the user, who caused logged event</param>
        /// <param name="siteId">ID of the site</param>
        /// <param name="machineName">Name of machine</param>
        /// <param name="eventUrlReferrer">URL referrer</param>
        /// <param name="eventUserAgent">User agent</param>
        /// <param name="eventTime">Date and time when the event occurs</param>
        protected virtual void LogEventToFileInternal(string eventType, string source, string eventCode, string eventDescription, string eventUrl, int userId, string userName, int nodeId, string documentName, string ipAddress, int siteId, string machineName, string eventUrlReferrer, string eventUserAgent, DateTime eventTime)
        {
            EventLogInfo logInfo = new EventLogInfo();
            logInfo.EventType = TextHelper.LimitLength(eventType, 5);
            logInfo.EventDescription = eventDescription;
            logInfo.EventTime = eventTime;
            logInfo.Source = TextHelper.LimitLength(source, 100);
            logInfo.EventCode = TextHelper.LimitLength(eventCode.ToUpperCSafe(), 100);
            logInfo.UserName = TextHelper.LimitLength(userName, 250, String.Empty);
            logInfo.DocumentName = TextHelper.LimitLength(documentName, 100, String.Empty);
            logInfo.IPAddress = TextHelper.LimitLength(ipAddress, 100);
            logInfo.EventUrl = TextHelper.LimitLength(eventUrl, 2000);
            logInfo.EventMachineName = TextHelper.LimitLength(machineName, 100);
            logInfo.EventUserAgent = eventUserAgent;
            logInfo.EventUrlReferrer = TextHelper.LimitLength(eventUrlReferrer, 2000);
            logInfo.UserID = userId;
            logInfo.NodeID = nodeId;
            logInfo.SiteID = siteId;

            LogEventToFile(logInfo);
        }


        /// <summary>
        /// Logs the log object to file.
        /// </summary>
        /// <param name="eventLogInfo">Event info object</param>
        protected virtual void LogEventToFileInternal(EventLogInfo eventLogInfo)
        {
            // Test if logging to file is enabled
            if (!LogEventsToFile)
            {
                return;
            }

            // Log the event
            try
            {
                if (LogFile != null)
                {
                    string message = EventLogProvider.GetEventLogText(eventLogInfo);

                    // Log directly to the file
                    if (message.Length > 0)
                    {
                        File.AppendAllText(LogFile, message);
                    }
                }
            }
            catch
            {
            }
        }


        /// <summary>
        /// Logs the query to file.
        /// </summary>
        /// <param name="eventObj">Event info object</param>
        /// <param name="siteName">Site name</param>
        protected virtual void SendEmailNotificationInternal(EventLogInfo eventObj, string siteName)
        {
            // Get settings
            string emailAddresses = SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendErrorNotificationTo");

            // Send email if email address is set 
            if (ValidationHelper.AreEmails(emailAddresses))
            {
                // Notification sender address
                string fromAddress = SettingsKeyInfoProvider.GetValue(siteName + ".CMSSendEmailNotificationsFrom");

                SendEmailNotificationInternal(emailAddresses, fromAddress, eventObj, siteName);
            }

            // Send e-mail if web.config settings are set
            try
            {
                string toEmail = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSSendErrorsToEmail"], "");
                string fromEmail = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSSendErrorsFromEmail"], "");
                if (ValidationHelper.IsEmail(toEmail) && ValidationHelper.IsEmail(fromEmail))
                {
                    SendEmailNotificationInternal(toEmail, fromEmail, eventObj, siteName);
                }
            }
            catch
            {
                // Can't send e-mail, do not process any code
            }
        }

        ///<summary>
        /// Gets log event text from DataRow
        ///</summary>
        ///<param name="eventObj">Object containing log event data</param>
        ///<returns>Text of log event</returns>
        protected virtual string GetEventTextInternal(EventLogInfo eventObj)
        {
            StringBuilder resultText = new StringBuilder();

            // Add all others (except site)
            foreach (string column in eventObj.ColumnNames)
            {
                string value = ValidationHelper.GetString(eventObj.GetValue(column), "");
                if (!String.IsNullOrEmpty(value))
                {
                    if (column == "SiteID")
                    {
                        // Get site name
                        string siteDisplayName = String.Empty;

                        // Prepare the parameters
                        QueryDataParameters parameters = new QueryDataParameters();
                        parameters.Add("@ID", ValidationHelper.GetInteger(value, 0));

                        DataSet ds = ConnectionHelper.ExecuteQuery("cms.site.select", parameters);
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            siteDisplayName = ValidationHelper.GetString(ds.Tables[0].Rows[0]["SiteDisplayName"], String.Empty);
                        }

                        if (!String.IsNullOrEmpty(siteDisplayName))
                        {
                            resultText.Append("Site: " + HTMLHelper.HTMLEncode(siteDisplayName) + Environment.NewLine + Environment.NewLine);
                        }
                        else
                        {
                            resultText.Append("Site: GLOBAL" + Environment.NewLine + Environment.NewLine);
                        }
                    }
                    else
                    {
                        resultText.Append(column + ": " + HTMLHelper.HTMLEncode(value) + Environment.NewLine + Environment.NewLine);
                    }
                }
            }

            return resultText.ToString();
        }


        /// <summary>
        /// Gets the list of changed fields in the given object.
        /// </summary>
        /// <param name="bi">BaseInfo object</param>
        protected virtual string GetChangedFieldsInternal(BaseInfo bi)
        {
            StringBuilder sb = new StringBuilder();

            // Write the detailed columns
            bool first = true;
            var sensitiveColumns = bi.TypeInfo.SensitiveColumns;
            foreach (string column in bi.ColumnNames)
            {
                if (bi.ItemChanged(column))
                {
                    string oldValue = null;
                    string newValue = null;

                    if ((sensitiveColumns != null) && sensitiveColumns.Contains(column, StringComparer.InvariantCultureIgnoreCase))
                    {
                        // Hidden column
                        oldValue = "********";
                        newValue = "********";
                    }
                    else
                    {
                        // Normal column
                        oldValue = ValidationHelper.GetString(bi.GetOriginalValue(column), "NULL");
                        newValue = ValidationHelper.GetString(bi.GetValue(column), "NULL");
                    }

                    // Do not add new line to first field
                    if (!first)
                    {
                        sb.Append("\r\n");
                    }
                    first = false;

                    sb.Append(String.Format(CoreServices.Localization.GetAPIString("TaskTitle.FieldChange", null, "{0}: '{1}' changed to '{2}'"), column, oldValue, newValue));
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gets the list of fields in the given object.
        /// </summary>
        /// <param name="bi">BaseInfo object</param>
        protected virtual string GetFieldsInternal(BaseInfo bi)
        {
            StringBuilder sb = new StringBuilder();

            // Write the detailed columns
            bool first = true;
            var sensitiveColumns = bi.TypeInfo.SensitiveColumns;
            foreach (string column in bi.ColumnNames)
            {
                // Output only when the value is not null
                object value = bi.GetValue(column);
                if ((value != DBNull.Value) && (value != null))
                {
                    // Do not add new line to first field
                    if (!first)
                    {
                        sb.Append("\r\n");
                    }
                    first = false;

                    string stringValue = null;
                    if ((sensitiveColumns != null) && sensitiveColumns.Contains(column, StringComparer.InvariantCultureIgnoreCase))
                    {
                        // Hidden column
                        stringValue = "********";
                    }
                    else
                    {
                        stringValue = ValidationHelper.GetString(value, "NULL");
                    }

                    // Add field and value
                    sb.Append(column);
                    sb.Append(": ");
                    sb.Append(stringValue);
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Returns user-friendly string for the given event type.
        /// </summary>
        /// <param name="eventType">Event type string</param>
        protected virtual string GetEventTypeTextInternal(string eventType)
        {
            if (String.IsNullOrEmpty(eventType))
            {
                return String.Empty;
            }

            switch (eventType.ToUpperCSafe())
            {
                case EventType.INFORMATION:
                    return GetString("EventLogList.EventTypeInformation");

                case EventType.WARNING:
                    return GetString("EventLogList.EventTypeWarning");

                case EventType.ERROR:
                    return GetString("EventLogList.EventTypeError");
            }

            return String.Empty;
        }


        /// <summary>
        /// Logs insertion of an object.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        protected virtual void LogInsertInternal(GeneralizedInfo infoObj)
        {
            // Log the object insert
            LogAction(infoObj, "CREATEOBJ", CoreServices.Localization.GetAPIString("TaskTitle.CreateObject", null, "Create {0}"));
        }


        /// <summary>
        /// Logs update of an object.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        protected virtual void LogUpdateInternal(GeneralizedInfo infoObj)
        {
            // Log the object update
            LogAction(infoObj, "UPDATEOBJ", CoreServices.Localization.GetAPIString("TaskTitle.UpdateObject", null, "Update {0}"), true);
        }


        /// <summary>
        /// Logs deletion of an object.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        protected virtual void LogDeleteInternal(GeneralizedInfo infoObj)
        {
            // Log the object deletion
            LogAction(infoObj, "DELETEOBJ", CoreServices.Localization.GetAPIString("TaskTitle.DeleteObject", null, "Delete {0}"));
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Logs the query to file.
        /// </summary>
        /// <param name="emailTo">E-mail address(es) to which the email should be sent to</param>
        /// <param name="emailFrom">E-mail address from which the email is sent</param>
        /// <param name="eventObj">Event log info object</param>
        /// <param name="siteName">Site name</param>
        private void SendEmailNotificationInternal(string emailTo, string emailFrom, EventLogInfo eventObj, string siteName)
        {
            if (eventObj != null)
            {
                // Get message body
                string emailSubject = "Error notification " + HTMLHelper.HTMLEncode(siteName);
                if (!String.IsNullOrEmpty(eventObj.EventMachineName))
                {
                    emailSubject += " (" + HTMLHelper.HTMLEncode(eventObj.EventMachineName) + ")";
                }
                string emailBody = "<html><body>" + HTMLHelper.EnsureHtmlLineEndings(GetEventTextInternal(eventObj)) + "</body></html>";

                ModuleCommands.SendEmail(emailTo, emailFrom, emailSubject, emailBody, GetEventTextInternal(eventObj), siteName);
            }
        }


        /// <summary>
        /// Logs object action to the event log.
        /// </summary>
        /// <param name="infoObj">Main info object</param>
        /// <param name="eventCode">Event code</param>
        /// <param name="eventDescription">Event description</param>
        /// <param name="changedFieldsOnly">If true, only changed fields are logged</param>
        private void LogAction(GeneralizedInfo infoObj, string eventCode, string eventDescription, bool changedFieldsOnly = false)
        {
            // Log the object action
            if (infoObj.LogEvents && EventLogProvider.LogMetadata(infoObj.ObjectSiteName))
            {
                string objectName = infoObj.TypeInfo.GetNiceObjectTypeName();
                string name = objectName + " '" + CoreServices.Localization.LocalizeString(infoObj.ObjectDisplayName) + "'";

                string text = String.Format(eventDescription, name);

                // Add fields
                BaseInfo bi = infoObj.MainObject;
                if ((bi != null) && EventLogProvider.LogFieldChanges)
                {
                    string fields = changedFieldsOnly ? GetChangedFields(bi) : GetFields(bi);

                    if (!String.IsNullOrEmpty(fields))
                    {
                        text += "\r\n\r\n" + fields;
                    }
                }

                EventLogProvider.LogEvent(EventType.INFORMATION, objectName, eventCode, text, null, 0, null, 0, null, null, infoObj.ObjectSiteID);
            }
        }

        #endregion
    }
}