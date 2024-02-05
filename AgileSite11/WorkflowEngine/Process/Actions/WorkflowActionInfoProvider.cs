using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.WorkflowEngine
{
    using TypedDataSet = InfoDataSet<WorkflowActionInfo>;

    /// <summary>
    /// Class providing WorkflowActionInfo management.
    /// </summary>
    public class WorkflowActionInfoProvider : AbstractInfoProvider<WorkflowActionInfo, WorkflowActionInfoProvider>, IFullNameInfoProvider
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowActionInfoProvider()
            : base(WorkflowActionInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true, 
                    FullName = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns all workflow actions.
        /// </summary>
        public static ObjectQuery<WorkflowActionInfo> GetWorkflowActions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns workflow action with specified ID.
        /// </summary>
        /// <param name="actionId">Workflow action ID</param>        
        public static WorkflowActionInfo GetWorkflowActionInfo(int actionId)
        {
            return ProviderObject.GetInfoById(actionId);
        }


        /// <summary>
        /// Returns workflow action with specified name.
        /// </summary>
        /// <param name="actionName">Workflow action name</param>
        /// <param name="type">Workflow type</param>
        public static WorkflowActionInfo GetWorkflowActionInfo(string actionName, WorkflowTypeEnum type)
        {
            return ProviderObject.GetWorkflowActionInfoInternal(actionName, type);
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static WorkflowActionInfo GetWorkflowActionInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified workflow action.
        /// </summary>
        /// <param name="actionObj">Workflow action to be set</param>
        public static void SetWorkflowActionInfo(WorkflowActionInfo actionObj)
        {
            ProviderObject.SetInfo(actionObj);
        }


        /// <summary>
        /// Deletes specified workflow action.
        /// </summary>
        /// <param name="actionObj">Workflow action to be deleted</param>
        public static void DeleteWorkflowActionInfo(WorkflowActionInfo actionObj)
        {
            ProviderObject.DeleteInfo(actionObj);
        }


        /// <summary>
        /// Deletes workflow action with specified ID.
        /// </summary>
        /// <param name="actionId">Workflow action ID</param>
        public static void DeleteWorkflowActionInfo(int actionId)
        {
            WorkflowActionInfo actionObj = GetWorkflowActionInfo(actionId);
            DeleteWorkflowActionInfo(actionObj);
        }


        /// <summary>
        /// Checks the object dependencies. Returns true if there are depending objects.
        /// </summary>
        /// <param name="workflowActionId">Workflow action ID</param>
        /// <param name="stepNames">List of step names which use given workflow step</param>
        public static bool CheckDependencies(int workflowActionId, ref List<string> stepNames)
        {
            var infoObj = ProviderObject.GetInfoById(workflowActionId);
            if (infoObj != null)
            {
                stepNames = infoObj.Generalized.GetDependenciesNames();
                return (stepNames != null) && (stepNames.Count > 0);
            }
            return false;
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all workflow actions matching the specified parameters. 
        /// </summary>
        /// <param name="where">Where condition</param>  
        /// <param name="orderBy">Order by expression</param>
        /// <param name="columns">Columns to be selected</param>
        /// <returns>Data set with actions</returns>
        [Obsolete("Use method GetWorkflowActions() instead.")]
        public static TypedDataSet GetWorkflowActions(string where, string orderBy, string columns)
        {
            return ProviderObject.GetWorkflowActionsInternal(where, orderBy, -1, columns);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns dataset of all workflow actions matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        [Obsolete("Use method GetWorkflowActions() instead.")]
        protected virtual TypedDataSet GetWorkflowActionsInternal(string where, string orderBy, int topN, string columns)
        {
            return GetWorkflowActions().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns workflow action with specified name.
        /// </summary>
        /// <param name="actionName">Workflow action name</param>
        /// <param name="type">Workflow type</param>
        protected virtual WorkflowActionInfo GetWorkflowActionInfoInternal(string actionName, WorkflowTypeEnum type)
        {
            string fullName = string.Format("{0}.{1}", actionName, (int)type);
            return GetInfoByFullName(fullName, true);
        }

        #endregion


        #region "Full name methods"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(TypeInfo.ObjectType, "ActionName;ActionWorkflowType");
        }


        /// <summary>
        /// Gets the where condition that searches the object based on the given full name
        /// </summary>
        /// <param name="fullName">Object full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string actionName;
            string typeString;
            int workflowType;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out actionName, out typeString) && Int32.TryParse(typeString, out workflowType))
            {
                // Build the where condition
                string where = string.Format("ActionName = N'{0}'", SqlHelper.GetSafeQueryString(actionName, false));
                string typeWhere = "ActionWorkflowType = " + workflowType;
                
                WorkflowTypeEnum type = (WorkflowTypeEnum)workflowType;
                if (type == WorkflowTypeEnum.Basic)
                {
                    typeWhere = SqlHelper.AddWhereCondition(typeWhere, "ActionWorkflowType IS NULL", "OR");
                }

                return SqlHelper.AddWhereCondition(where, typeWhere);
            }

            return null;
        }

        #endregion
    }
}
