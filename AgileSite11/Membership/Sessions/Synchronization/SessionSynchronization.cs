using System;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm synchronization for session
    /// </summary>
    internal class SessionSynchronization
    {
        #region "Methods"

        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SessionTaskType.AddUserToKicked,
                Execute = AddUserToKicked,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SessionTaskType.RemoveUserFromKicked,
                Execute = RemoveUserFromKicked,
                IsMemoryTask = true
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SessionTaskType.RemoveUser,
                Execute = RemoveUser,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SessionTaskType.RemoveAuthenticatedUser,
                Execute = RemoveAuthenticatedUser,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = SessionTaskType.UpdateDatabaseSession,
                Execute = UpdateDatabaseSession,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Adds user to the kicked users
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void AddUserToKicked(string target, string[] data, BinaryData binaryData)
        {
            AuthenticationHelper.AddUserToKicked(ValidationHelper.GetInteger(data.FirstOrDefault(), 0));
        }


        /// <summary>
        /// Removes users from the kicked users 
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemoveUserFromKicked(string target, string[] data, BinaryData binaryData)
        {
            AuthenticationHelper.RemoveUserFromKicked(ValidationHelper.GetInteger(data.FirstOrDefault(), 0));
        }


        /// <summary>
        /// Removes the user session
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemoveUser(string target, string[] data, BinaryData binaryData)
        {
            SessionManager.RemoveUser(ValidationHelper.GetInteger(data.FirstOrDefault(), 0), false);
        }


        /// <summary>
        /// Removes the authenticated user session
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RemoveAuthenticatedUser(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException(String.Format("[SessionSynchronization.RemoveAuthenticatedUser]: Expected 2 data arguments, but received {0}", data.Length));
            }

            SessionManager.RemoveAuthenticatedUser(data[0], ValidationHelper.GetInteger(data[1], 0), false);
        }


        /// <summary>
        /// Updates the database session
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateDatabaseSession(string target, string[] data, BinaryData binaryData)
        {
            if (!SchedulingHelper.RunSchedulerWithinRequest && SessionManager.OnlineUsersEnabled && SessionManager.StoreOnlineUsersInDatabase)
            {
                SessionManager.UpdateDatabaseSession();
            }
        }

        #endregion
    }
}
