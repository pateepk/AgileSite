using System;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class representing subscription to documents or objects
    /// </summary>
    public class BaseIntegrationSubscription : AbstractIntegrationSubscription
    {
        #region "Properties"

        /// <summary>
        /// Type of task processing (sync/async etc.)
        /// </summary>
        public TaskProcessTypeEnum TaskProcessType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Task type of document or object to match.
        /// </summary>
        public TaskTypeEnum TaskType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Site name of document or object to match.
        /// Use AbstractIntegrationSubscription.GLOBAL_OBJECTS to match only global objects.
        /// </summary>
        public string SiteName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Name of connector the subscription is attached to.
        /// </summary>
        public override string ConnectorName
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="connectorName">Name of connector the subscription is attached to</param>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task which to subscribe to</param>
        /// <param name="siteName">Name of site which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        public BaseIntegrationSubscription(string connectorName, TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, string siteName)
        {
            ConnectorName = connectorName;
            TaskProcessType = taskProcessType;
            TaskType = taskType;
            SiteName = siteName;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Determines whether given CMS object and task type match the subscription.
        /// This method recognizes TreeNode and GeneralizedInfo as the CMS object.
        /// </summary>
        /// <param name="obj">CMS object to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if the CMS object and task correspond with subscription settings</returns>
        public override bool IsMatch(ICMSObject obj, TaskTypeEnum taskType, ref TaskProcessTypeEnum taskProcessType)
        {
            if (obj is TreeNode)
            {
                return IsMatch(obj as TreeNode, taskType, ref taskProcessType);
            }
            else if (obj is BaseInfo)
            {
                return IsMatch(obj as BaseInfo, taskType, ref taskProcessType);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Determines whether given info object and task type match the subscription
        /// </summary>
        /// <param name="infoObj">Info object to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if the info object and task correspond with subscription settings</returns>
        public bool IsMatch(GeneralizedInfo infoObj, TaskTypeEnum taskType, ref TaskProcessTypeEnum taskProcessType)
        {
            if (infoObj != null)
            {
                return IsMatch(taskType, out taskProcessType) && IsMatch(infoObj.ObjectSiteName);
            }
            return false;
        }


        /// <summary>
        /// Determines whether given node and task type match the subscription
        /// </summary>
        /// <param name="node">Node to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if the node and task correspond with subscription settings</returns>
        public bool IsMatch(TreeNode node, TaskTypeEnum taskType, ref TaskProcessTypeEnum taskProcessType)
        {
            if (node != null)
            {
                return IsMatch(taskType, out taskProcessType) && IsMatch(node.NodeSiteName);
            }
            return false;
        }


        /// <summary>
        /// Determines whether given task type matches the TaskType property
        /// </summary>
        /// <param name="taskType">Task type to evaluate</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if task type is equal to TaskType property</returns>
        protected bool IsMatch(TaskTypeEnum taskType, out TaskProcessTypeEnum taskProcessType)
        {
            taskProcessType = TaskProcessType;
            return ((TaskType == taskType) || (TaskType == TaskTypeEnum.All));
        }


        /// <summary>
        /// Determines whether given site name matches the SiteName property
        /// </summary>
        /// <param name="siteName">Name of site to evaluate</param>
        /// <returns>TRUE if site name matches SiteName property</returns>
        protected bool IsMatch(string siteName)
        {
            Regex siteRegex = GetRegex(SiteName);
            if (SiteName == GLOBAL_OBJECTS)
            {
                return string.IsNullOrEmpty(siteName);
            }
            else if (siteRegex != null)
            {
                return siteRegex.IsMatch(siteName);
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Returns regular expression with pattern used to match words like 'cms.%' or 'cms.pagetemplate%' where '%' represents with 0-n characters
        /// </summary>
        /// <param name="expression">User-friendly expression to transformate to regular expression</param>
        /// <returns>Regular expression object</returns>
        protected Regex GetRegex(string expression)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                return RegexHelper.GetRegex("^" + expression.Replace("%", ".*") + "$");
            }
            return null;
        }

        #endregion
    }
}