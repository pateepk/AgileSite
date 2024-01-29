using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;

namespace CMS.Search.Internal
{
    /// <summary>
    /// Class encapsulates logic to log actions related to indexes and search tasks.
    /// </summary>
    public class IndexLogger
    {
        /// <summary>
        /// Logs task started message to the log context. 
        /// </summary>
        /// <param name="taskInfo"></param>
        public void LogTaskStart(SearchTaskInfo taskInfo)
        {
            GeneralizedInfo objectInfo = null;
            
            // Just to be sure that trying to obtain object can't kill task processing. 
            try
            {
                objectInfo = ProviderHelper.GetInfoById(taskInfo.SearchTaskRelatedObjectType, taskInfo.SearchTaskRelatedObjectID);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogEvent(EventType.ERROR, "Smart search", "PROCESSSEARCHTASK",
                    "Cannot obtain object with type " + taskInfo.SearchTaskRelatedObjectType + " and ID " + taskInfo.SearchTaskRelatedObjectID + ".\n\nException:\n" + EventLogProvider.GetExceptionLogMessage(ex));
            }

            string msg = LocalizationHelper.GetStringFormat(
                "smartsearch.taskstarted",
                taskInfo.SearchTaskType.ToStringRepresentation(),
                TypeHelper.GetNiceObjectTypeName(taskInfo.SearchTaskObjectType),
                objectInfo != null ? objectInfo.GetFullObjectName(false, true, false) : LocalizationHelper.GetString("general.objectnotfound"),
                taskInfo.SearchTaskField,
                taskInfo.SearchTaskValue
            );

            LogMessage(msg);
        }


        /// <summary>
        /// Logs the start of batch processing. 
        /// </summary>
        /// <param name="batchNumber">Number of batch</param>
        /// <param name="lastID">ID of last item that is not present in the batch</param>
        public void LogBatchStart(int batchNumber, int lastID)
        {
            LogMessage(LocalizationHelper.GetStringFormat("smartsearch.taskbatch.processingstart", batchNumber, lastID));
        }


        /// <summary>
        /// Logs the end of the batch processing.
        /// </summary>
        /// <param name="itemsCount"></param>
        public void LogBatchEnd(int itemsCount)
        {
            LogContext.Append(" " + LocalizationHelper.GetStringFormat("smartsearch.taskbatch.processingend", itemsCount), "Search");
        }


        /// <summary>
        /// Logs start of batch processing. 
        /// </summary>
        /// <param name="batchNumber">Number of batch.</param>
        /// <param name="lastID">ID of last item that is not present in the batch.</param>
        /// <param name="objectType">Object type of processed objects</param>
        public void LogBatchStart(int batchNumber, int lastID, string objectType)
        {
            if (batchNumber == 1)
            {
                LogMessage(LocalizationHelper.GetStringFormat("smartsearch.taskbatch.startobject", TypeHelper.GetNiceObjectTypeName(objectType)));
            }
            else
            {
                LogBatchStart(batchNumber, lastID);
            }
        }


        /// <summary>
        /// Logs start of batch processing. 
        /// </summary>
        /// <param name="batchNumber">Number of batch.</param>
        /// <param name="lastID">ID of last item that is not present in the batch.</param>
        /// <param name="tableName">Code name of the table of processed objects</param>
        public void LogCustomTableBatchStart(int batchNumber, int lastID, string tableName)
        {
            DataClassInfo customTable = DataClassInfoProvider.GetDataClassInfo(tableName);
            string customTableDisplayName = customTable != null ? ResHelper.LocalizeString(customTable.ClassDisplayName) : tableName;

            if (batchNumber == 1)
            {
                LogMessage(LocalizationHelper.GetStringFormat("smartsearch.taskbatch.startcustomtable", customTableDisplayName));
            }
            else
            {
                LogBatchStart(batchNumber, lastID);
            }
        }


        /// <summary>
        /// Appends line to the log context. 
        /// </summary>
        /// <param name="message"></param>
        public void LogMessage(string message)
        {
            LogContext.AppendLine("(" + DateTime.Now + ") " + message, "Search");
        }
    }
}
