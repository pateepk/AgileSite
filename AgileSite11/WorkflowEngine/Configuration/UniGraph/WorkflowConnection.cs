using System;
using System.Runtime.Serialization;

using CMS.Helpers.UniGraphConfig;

namespace CMS.WorkflowEngine.GraphConfig
{
    /// <summary>
    /// Class creating connection definition from information given by workflow.
    /// </summary>
    [DataContract(Name="Connection", Namespace="CMS.Helpers.UniGraphConfig")]
    public class WorkflowConnection : Connection
    {
        #region "Constructors"

        /// <summary>
        /// Creates Connection configuration object by given WorkflowTransitionInfo object.
        /// </summary>
        /// <param name="transition">Transition to be rewritten</param>
        /// <returns>Connection configuration object</returns>
        public WorkflowConnection(WorkflowTransitionInfo transition)
        {
            if (transition == null)
            {
                throw new NullReferenceException("[WorkflowConnection] : Workflow transition is null.");
            }
            ID = transition.TransitionID.ToString();
            SourceNodeID = transition.TransitionStartStepID.ToString();
            TargetNodeID = transition.TransitionEndStepID.ToString();
            SourcePointID = transition.TransitionSourcePointGUID.ToString();
        }


        /// <summary>
        /// Method for support of basic workflows.
        /// </summary>
        /// <param name="fromID">ID of source node</param>
        /// <param name="toID">ID of target node</param>
        /// <returns>New connection</returns>
        public WorkflowConnection(string fromID, string toID)
        {
            ID = Guid.NewGuid().ToString();
            SourceNodeID = fromID;
            TargetNodeID = toID;
        }

        #endregion
    }
}
