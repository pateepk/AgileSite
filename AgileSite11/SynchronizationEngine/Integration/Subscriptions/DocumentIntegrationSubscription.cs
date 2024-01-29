using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class representing subscription to documents
    /// </summary>
    public class DocumentIntegrationSubscription : BaseIntegrationSubscription
    {
        #region "Properties"

        /// <summary>
        /// Alias path of document to match
        /// </summary>
        public string DocumentNodeAliasPath
        {
            get;
            protected set;
        }


        /// <summary>
        /// Culture code of document to match
        /// </summary>
        public string DocumentCultureCode
        {
            get;
            protected set;
        }


        /// <summary>
        /// Class name of document to match
        /// </summary>
        public string DocumentClassName
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
        /// <param name="taskProcessType">Type of task processing (supported: AsyncSimple, AsyncSimpleSnapshot, SyncSnapshot)</param>
        /// <param name="taskType">Type of task which to subscribe to</param>
        /// <param name="siteName">Name of site which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        /// <param name="documentNodeAliasPath">Node alias path of document which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        /// <param name="documentCultureCode">Culture code of document which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        /// <param name="documentClassName">Class name of document which to subscribe to (accepts '%' as a wildcard representing 0-n characters)</param>
        public DocumentIntegrationSubscription(string connectorName, TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, string siteName, string documentNodeAliasPath, string documentCultureCode, string documentClassName)
            : base(connectorName, taskProcessType, taskType, siteName)
        {
            DocumentNodeAliasPath = documentNodeAliasPath;
            DocumentCultureCode = documentCultureCode;
            DocumentClassName = documentClassName;
            if (taskProcessType == TaskProcessTypeEnum.AsyncSnapshot)
            {
                TaskProcessType = TaskProcessTypeEnum.AsyncSimpleSnapshot;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Determines whether given node and task type match the subscription
        /// </summary>
        /// <param name="obj">TreeNode to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <param name="taskProcessType">Returns type of task processing</param>
        /// <returns>TRUE if the node and task correspond with subscription settings</returns>
        public override bool IsMatch(ICMSObject obj, TaskTypeEnum taskType, ref TaskProcessTypeEnum taskProcessType)
        {
            TreeNode node = obj as TreeNode;
            if (node != null)
            {
                bool result = base.IsMatch(node, taskType, ref taskProcessType);
                Regex nodeAliasPathRegex = GetRegex(DocumentNodeAliasPath);
                if (nodeAliasPathRegex != null)
                {
                    result &= nodeAliasPathRegex.IsMatch(node.NodeAliasPath);
                }
                Regex cultureCodeRegex = GetRegex(DocumentCultureCode);
                if (cultureCodeRegex != null)
                {
                    result &= cultureCodeRegex.IsMatch(node.DocumentCulture);
                }
                Regex classNameRegex = GetRegex(DocumentClassName);
                if (classNameRegex != null)
                {
                    result &= classNameRegex.IsMatch(node.NodeClassName);
                }
                return result;
            }
            return false;
        }

        #endregion
    }
}