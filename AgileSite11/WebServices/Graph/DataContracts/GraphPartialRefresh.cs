using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.SiteProvider;
using CMS.WorkflowEngine;
using CMS.WorkflowEngine.GraphConfig;


namespace CMS.WebServices
{
    /// <summary>
    /// Service response containing part of the graph to be refreshed.
    /// </summary>
    [DataContract]
    public class GraphPartialRefresh : ServiceResponse
    {
        #region "Variables"

        private List<Node> mNodes = null;
        private List<Connection> mConnections = null;

        #endregion


        #region "Data member properties"

        /// <summary>
        /// Nodes to be refreshed.
        /// </summary>
        [DataMember]
        public List<Node> Nodes
        {
            get
            {
                if (mNodes == null)
                {
                    mNodes = new List<Node>();
                }
                return mNodes;
            }
            set
            {
                mNodes = value;
            }
        }

        /// <summary>
        /// Connections to be refreshed.
        /// </summary>
        [DataMember]
        public List<Connection> Connections
        {
            get
            {
                if (mConnections == null)
                {
                    mConnections = new List<Connection>();
                }
                return mConnections;
            }
            set
            {
                mConnections = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GraphPartialRefresh()
        {
            StatusCode = ResponseStatusEnum.None;
        }


        /// <summary>
        /// Simple parametric constructor.
        /// </summary>
        /// <param name="statusCode">Status code</param>
        public GraphPartialRefresh(ResponseStatusEnum statusCode)
        {
            StatusCode = statusCode;
            ScreenLockInterval = SecurityHelper.GetSecondsToShowScreenLockAction(SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Parametric constructor.
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="statusMessage">Status message</param>
        public GraphPartialRefresh(ResponseStatusEnum statusCode, string statusMessage)
        {
            StatusCode = statusCode;
            StatusMessage = statusMessage;
            ScreenLockInterval = SecurityHelper.GetSecondsToShowScreenLockAction(SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Constructor filling data from workflow format.
        /// </summary>
        /// <param name="steps">Steps to be refreshed</param>
        /// <param name="transitions">Transitions to be refreshed</param>
        public GraphPartialRefresh(IEnumerable<WorkflowStepInfo> steps, IEnumerable<WorkflowTransitionInfo> transitions)
        {
            foreach (var step in steps)
            {
                Nodes.Add(WorkflowNode.GetInstance(step));
            }
            foreach (var transition in transitions)
            {
                Connections.Add(new WorkflowConnection(transition));
            }
            StatusCode = ResponseStatusEnum.OK;
        }


        /// <summary>
        /// Constructor filling data from default format.
        /// </summary>
        /// <param name="nodes">Nodes to be refreshed</param>
        /// <param name="connections">Connections to be refreshed</param>
        public GraphPartialRefresh(IEnumerable<Node> nodes, IEnumerable<Connection> connections)
        {
            if (nodes != null)
            {
                Nodes = nodes.ToList();
            }
            if (connections != null)
            {
                Connections = connections.ToList();
            }
            StatusCode = ResponseStatusEnum.OK;
        }

        #endregion
    }
}