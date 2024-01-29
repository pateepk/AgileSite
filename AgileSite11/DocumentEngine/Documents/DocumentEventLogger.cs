using System;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides logging to event log for document actions
    /// </summary>
    internal class DocumentEventLogger
    {
        private const string DEFAULT_EVENT_SOURCE = "Content";
        private readonly TreeNode mTreeNode;
        private readonly string mEventSource;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="treeNode">Document context for the action to log</param>
        /// <param name="eventSource">Event source</param>
        public DocumentEventLogger(TreeNode treeNode, string eventSource = DEFAULT_EVENT_SOURCE)
        {
            mTreeNode = treeNode;
            mEventSource = eventSource;
        }


        /// <summary>
        /// Logs document action to the event log.
        /// </summary>
        /// <param name="eventCode">Event code</param>
        /// <param name="eventDescription">Event description</param>
        /// <param name="logDocumentFieldChanges">Indicates if log should contain the changes to particular document document fields</param>
        /// <param name="eventType">Type of the event. Please use predefined constants from <see cref="EventType"/> class.</param>
        public void Log(string eventCode, string eventDescription, bool logDocumentFieldChanges = true, string eventType = EventType.INFORMATION)
        {
            if (!mTreeNode.LogEventsInternal)
            {
                return;
            }

            string name = mTreeNode.GetDocumentName();
            string text = string.Format(eventDescription, string.Format("{0} ({1})", name, mTreeNode.DocumentNamePath));

            // Add fields
            if (logDocumentFieldChanges && EventLogProvider.LogDocumentFieldChanges)
            {
                string fields = EventLogHelper.GetFields(mTreeNode);
                if (!string.IsNullOrEmpty(fields))
                {
                    text += "\r\n\r\n" + fields + "\r\n\r\n";
                }
            }

            var user = mTreeNode.TreeProvider.UserInfo;
            var nodeId = 0;
            switch (eventCode.ToLowerCSafe())
            {
                case "destroydoc":
                case "deletedoc":
                    break;

                default:
                    nodeId = mTreeNode.NodeID;
                    break;
            }

            LogContext.LogEventToCurrent(eventType, mEventSource, eventCode, text, RequestContext.RawURL, user.UserID, user.UserName, nodeId, name, RequestContext.UserHostAddress, mTreeNode.NodeSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
        }
    }
}