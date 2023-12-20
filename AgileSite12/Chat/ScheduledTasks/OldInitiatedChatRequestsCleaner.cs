using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Scheduler;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// Sheduled task which cleans old, unused chat records (rooms, messages, users). Setting how old data should be cleaned is in Settings.
    /// </summary>
    public class OldInitiatedChatRequestsCleaner : ITask
    {
        /// <summary>
        /// Cleans old chat records.
        /// </summary>
        /// <param name="task">Container with task information</param>
        /// <returns>Number of records cleaned</returns>
        public string Execute(TaskInfo task)
        {
            ChatInitiatedChatRequestInfoProvider.CleanOldRequests();

            return "";
        }
    }
}
