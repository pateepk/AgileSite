using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Scheduler;
using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Class providing AutomationStateInfo management.
    /// </summary>
    public class AutomationStateInfoProvider : AbstractInfoProvider<AutomationStateInfo, AutomationStateInfoProvider>
    {
        #region "Variables"

        private static readonly StringSafeDictionary<ProcessInstanceStatusEnum> statuses = new StringSafeDictionary<ProcessInstanceStatusEnum>();
        internal static IDateTimeNowService mDateTimeNowService = Service.Resolve<IDateTimeNowService>();

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all automation states records.
        /// </summary>
        public static ObjectQuery<AutomationStateInfo> GetAutomationStates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns state with specified ID.
        /// </summary>
        /// <param name="stateId">State ID.</param>        
        public static AutomationStateInfo GetAutomationStateInfo(int stateId)
        {
            return ProviderObject.GetInfoById(stateId);
        }


        /// <summary>
        /// Returns state with specified GUID.
        /// </summary>
        /// <param name="stateGuid">State GUID.</param>        
        public static AutomationStateInfo GetAutomationStateInfo(Guid stateGuid)
        {
            return ProviderObject.GetInfoByGuid(stateGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified state.
        /// </summary>
        /// <param name="stateObj">state to be set.</param>
        public static void SetAutomationStateInfo(AutomationStateInfo stateObj)
        {
            ProviderObject.SetInfo(stateObj);
        }


        /// <summary>
        /// Deletes specified state.
        /// </summary>
        /// <param name="stateObj">state to be deleted.</param>
        public static void DeleteAutomationStateInfo(AutomationStateInfo stateObj)
        {
            ProviderObject.DeleteInfo(stateObj);
        }


        /// <summary>
        /// Deletes state with specified ID.
        /// </summary>
        /// <param name="stateId">state ID.</param>
        public static void DeleteAutomationStateInfo(int stateId)
        {
            AutomationStateInfo stateObj = GetAutomationStateInfo(stateId);
            DeleteAutomationStateInfo(stateObj);
        }


        /// <summary>
        /// Deletes automation states associated with specified object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">List of IDs</param>
        public static void DeleteAutomationStates(string objectType, IList<int> ids)
        {
            ProviderObject.DeleteAutomationStatesInternal(objectType, ids);
        }


        /// <summary>
        /// Gets automation process instance status
        /// </summary>
        /// <param name="stateObj">State object</param>
        public static ProcessInstanceStatusEnum GetProcessInstanceStatus(AutomationStateInfo stateObj)
        {
            return ProviderObject.GetProcessInstanceStatusInternal(stateObj);
        }


        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            statuses.Clear();
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            switch (actionName)
            {
                case "ClearProcessInstanceStatus":
                    statuses.Clear();
                    break;

                case "RemoveProcessInstanceStatus":
                    if (statuses.ContainsKey(data))
                    {
                        statuses.Remove(data);
                    }
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(AutomationStateInfo info)
        {
            if (info.StateID == 0)
            {
                info.StateCreated = mDateTimeNowService.GetDateTimeNow();
            }

            base.SetInfo(info);

            // Remove from hashtable
            RemoveProcessInstanceStatusInternal(info);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(AutomationStateInfo info)
        {
            if (info != null)
            {
                TaskInfoProvider.DeleteObjectsTasks(AutomationStateInfo.OBJECT_TYPE, new List<int> { info.StateID });
            }

            base.DeleteInfo(info);

            // Remove from hashtable
            RemoveProcessInstanceStatusInternal(info);
        }


        /// <summary>
        /// Deletes automation states associated with specified object. Also removes scheduled tasks associated to these automation states.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="ids">List of IDs</param>
        protected virtual void DeleteAutomationStatesInternal(string objectType, IList<int> ids)
        {
            using (var cs = new CMSConnectionScope())
            {
                cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                var where = new WhereCondition()
                    .WhereEquals("StateObjectType", objectType)
                    .WhereIn("StateObjectID", ids);

                // Clear scheduled tasks associated to the automation states
                var taskWhere = new WhereCondition()
                    .WhereEquals("TaskObjectType", AutomationStateInfo.OBJECT_TYPE)
                    .WhereIn("TaskObjectID", GetAutomationStates().Column("StateID")
                    .Where(where));
                TaskInfoProvider.DeleteObjectsTasks(taskWhere);

                // Clear history of the automation states
                var historyWhere = new WhereCondition()
                    .WhereIn("HistoryStateID", GetAutomationStates()
                    .Column("StateID")
                    .Where(where));       
                AutomationHistoryInfoProvider.DeleteObjectsHistories(historyWhere);

                // Then delete automation states
                BulkDelete(where);
            }
        }


        /// <summary>
        /// Gets automation process instance status.
        /// </summary>
        /// <param name="state">State object</param>
        protected virtual ProcessInstanceStatusEnum GetProcessInstanceStatusInternal(AutomationStateInfo state)
        {
            string key = GetStatusKey(state.StateObjectType, state.StateObjectID, state.StateWorkflowID);
            ProcessInstanceStatusEnum status;
            if (statuses.TryGetValue(key, out status))
            {
                return status;
            }

            var states = GetAutomationStates()
                .WhereEquals("StateObjectType", state.StateObjectType)
                .WhereEquals("StateObjectID", state.StateObjectID)
                .WhereEquals("StateWorkflowID", state.StateWorkflowID)
                .Column("StateStatus")
                .ToList();

            if (states.Any())
            {
                bool running = states.Any(s => s.StateStatus != ProcessStatusEnum.Finished);
                status = running ? ProcessInstanceStatusEnum.Running : ProcessInstanceStatusEnum.Finished;
            }

            // Store status
            statuses[key] = status;

            return status;
        }


        /// <summary>
        /// Gets status key
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="processId">Process ID</param>
        private string GetStatusKey(string objectType, int objectId, int processId)
        {
            return string.Format("{0}_{1}_{2}", objectType, objectId, processId);
        }


        /// <summary>
        /// Removes status from hashtable
        /// </summary>
        /// <param name="stateObj">State object</param>
        private void RemoveProcessInstanceStatusInternal(AutomationStateInfo stateObj)
        {
            // Remove from hashtable
            string key = GetStatusKey(stateObj.StateObjectType, stateObj.StateObjectID, stateObj.StateWorkflowID);
            if (statuses.ContainsKey(key))
            {
                statuses.Remove(key);
            }
        }

        #endregion


        #region "Bulk methods"

        /// <summary>
        /// Moves all automation states assigned to contact identified by given <paramref name="sourceContactID"/> to the state assigned to contact identified by <paramref name="targetContactID"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used only in the merging process. Note that there is no consistency check on whether the states with given IDs exist or not. 
        /// Caller of this method should perform all the neccessary checks prior to the method invocation.
        /// </remarks>
        /// <param name="sourceContactID">Identifier of the contact the assigned state is moved from</param>
        /// <param name="targetContactID">Identifier of the contact the assigned state is moved to</param>
        public static void BulkMoveAutomationStateToTargetContact(int sourceContactID, int targetContactID)
        {
            var updateDictionary = new Dictionary<string, object>
            {
                {"StateObjectID", targetContactID}
            };

            var whereCondition = new WhereCondition()
                .WhereEquals("StateObjectID", sourceContactID)
                .WhereEquals("StateObjectType", PredefinedObjectType.CONTACT);

            ProviderObject.UpdateData(whereCondition, updateDictionary);
        }


        /// <summary>
        /// Inserts all automation states given by <paramref name="states" /> collection into database.
        /// <see cref="AutomationStateInfo.StateCreated"/> will be set to the insertion time.
        /// </summary>
        /// <param name="states">Collection of automation states to insert into database</param>
        public static void BulkInsertAutomationState(IEnumerable<AutomationStateInfo> states)
        {
            states = states.ToList();

            foreach (var state in states)
            {
                state.StateCreated = mDateTimeNowService.GetDateTimeNow();
            }

            ProviderObject.BulkInsertInfos(states);
        }

        #endregion
    }
}
