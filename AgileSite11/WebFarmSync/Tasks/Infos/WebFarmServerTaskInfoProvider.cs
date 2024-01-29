using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Class providing WebFarmServerTaskInfo management.
    /// </summary>
    public class WebFarmServerTaskInfoProvider : AbstractInfoProvider<WebFarmServerTaskInfo, WebFarmServerTaskInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the query for all relationships between web farm servers and web farm server tasks.
        /// </summary>   
        public static ObjectQuery<WebFarmServerTaskInfo> GetWebFarmServerTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the query for all relationships between web farm servers and web farm server tasks.
        /// </summary>   
        internal static ObjectQuery<WebFarmServerTaskInfo> GetWebFarmServerTasksInternal()
        {
            return ProviderObject.GetObjectQuery(false);
        }


        /// <summary>
        /// Returns the WebFarmServerTaskInfo structure for the specified Web Farm Server Task.
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="taskId">Task ID</param>
        public static WebFarmServerTaskInfo GetWebFarmServerTaskInfo(int serverId, int taskId)
        {
            return ProviderObject.GetWebFarmServerTaskInfoInternal(serverId, taskId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified Web Farm Server Task.
        /// </summary>
        /// <param name="infoObj">WebFarmServerTask to set</param>
        public static void SetWebFarmServerTaskInfo(WebFarmServerTaskInfo infoObj)
        {
            ProviderObject.SetWebFarmServerTaskInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified Web Farm Server Task.
        /// </summary>
        /// <param name="infoObj">WebFarmServerTask object</param>
        public static void DeleteWebFarmServerTaskInfo(WebFarmServerTaskInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Provides delete query.
        /// </summary>
        internal static ObjectQuery<WebFarmServerTaskInfo> DeleteQuery()
        {
            return ProviderObject.GetDeleteQuery();
        }


        /// <summary>
        /// Deletes specified Web Farm Server Task.
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="taskId">Task ID</param>
        public static void DeleteWebFarmServerTaskInfo(int serverId, int taskId)
        {
            WebFarmServerTaskInfo infoObj = GetWebFarmServerTaskInfo(serverId, taskId);
            DeleteWebFarmServerTaskInfo(infoObj);
        }


        /// <summary>
        /// Add specified Web Farm Server Task.
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="taskId">Task ID</param>
        public static void AddWebFarmServerTaskInfo(int serverId, int taskId)
        {
            WebFarmServerTaskInfo infoObj = new WebFarmServerTaskInfo();
            infoObj.ServerID = serverId;
            infoObj.TaskID = taskId;
            SetWebFarmServerTaskInfo(infoObj);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the WebFarmServerTaskInfo structure for the specified Web Farm Server Task.
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="taskId">Task ID</param>
        protected virtual WebFarmServerTaskInfo GetWebFarmServerTaskInfoInternal(int serverId, int taskId)
        {
            var where = new WhereCondition().WhereEquals("ServerID", serverId).WhereEquals("TaskID", taskId);

            return GetWebFarmServerTasks().Where(where).TopN(1).BinaryData(true).FirstObject;
        }


        /// <summary>
        /// Sets (updates or inserts) specified Web Farm Server Task.
        /// </summary>
        /// <param name="infoObj">WebFarmServerTask to set</param>
        protected virtual void SetWebFarmServerTaskInfoInternal(WebFarmServerTaskInfo infoObj)
        {
            if (infoObj != null)
            {
                // Check IDs
                if ((infoObj.ServerID <= 0) || (infoObj.TaskID <= 0))
                {
                    throw new Exception("[WebFarmServerTaskInfoProvider.SetWebFarmServerTaskInfo]: Object IDs not set.");
                }

                // Get existing
                WebFarmServerTaskInfo existing = GetWebFarmServerTaskInfoInternal(infoObj.ServerID, infoObj.TaskID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data                    
                }
                else
                {
                    infoObj.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[WebFarmServerTaskInfoProvider.SetWebFarmServerTaskInfo]: No WebFarmServerTaskInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(WebFarmServerTaskInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Sets given error message to task with given ID on server with given ID.
        /// </summary>
        /// <param name="taskId">Task Id to be set</param>
        /// <param name="serverId">Server Id to be set</param>
        /// <param name="errorMessage">Error message to be set</param>
        internal static void SetErrorInTask(int taskId, int serverId, string errorMessage)
        {
            var condition = new WhereCondition()
                .WhereEquals("TaskID", taskId)
                .And()
                .WhereEquals("ServerID", serverId);

            ProviderObject.UpdateData(condition, new Dictionary<string, object> { { "ErrorMessage", errorMessage  } });
        }

        #endregion
    }
}