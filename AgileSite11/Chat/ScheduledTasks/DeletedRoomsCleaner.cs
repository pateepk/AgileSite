using System;
using System.Linq;
using System.Text;

using CMS.Scheduler;

namespace CMS.Chat
{
    /// <summary>
    /// Scheduled task which cleans rooms marked for deletion.
    /// </summary>
    public class DeletedRoomsCleaner : ITask
    {
        /// <summary>
        /// Executes task.
        /// </summary>
        /// <param name="task">TaskInfo</param>
        /// <returns>Number of rooms deleted</returns>
        public string Execute(TaskInfo task)
        {
            ChatRoomInfoProvider.DeleteScheduledRooms();

            return "";
        }
    }
}
