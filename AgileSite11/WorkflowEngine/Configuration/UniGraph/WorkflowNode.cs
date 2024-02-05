using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.WorkflowEngine.Factories;

using WorkflowSourcePointDefinition = CMS.WorkflowEngine.Definitions.SourcePoint;

namespace CMS.WorkflowEngine.GraphConfig
{
    /// <summary>
    /// Workflow node configuration
    /// </summary>
    [DataContract(Name = "Node", Namespace = "CMS.Helpers.UniGraphConfig")]
    public class WorkflowNode : Node
    {
        #region "Variables"

        /// <summary>
        /// Reference to workflow step on which is based this node.
        /// </summary>
        private WorkflowStepInfo mWorkflowStep = null;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for default node container.
        /// </summary>
        public WorkflowNode()
        {
        }

        #endregion


        #region "Static methods"

        /// <summary>
        /// Method creating node based on given workflow step.
        /// </summary>
        /// <param name="step">Type of workflow step</param>
        /// <returns>New workflow node</returns>
        public static WorkflowNode GetInstance(WorkflowStepInfo step)
        {
            if (step == null)
            {
                throw new NullReferenceException("[WorkflowConnection] : Workflow step is null.");
            }

            WorkflowNode node = StepTypeWorkflowNode.GetNode(step.StepType);

            node.mWorkflowStep = step;
            node.ID = step.StepID.ToString();
            node.Name = ResHelper.LocalizeString(step.StepDisplayName);
            node.IsNameLocalized = node.Name != step.StepDisplayName;
            node.Position = step.StepDefinition.Position;
            node.IsDeletable = step.StepIsDeletable;
            node.HasTimeout = step.StepHasTimeout;

            if (step.StepActionID > 0)
            {
                WorkflowActionInfo action = WorkflowActionInfoProvider.GetWorkflowActionInfo(step.StepActionID);
                node.LoadAction(action);
            }

            return node;
        }


        /// <summary>
        /// Method creating empty node just with predefined type-specific properties.
        /// </summary>
        /// <param name="type">Type of workflow step</param>
        /// <returns>New workflow node</returns>
        public static WorkflowNode GetInstance(WorkflowStepTypeEnum type)
        {
            return StepTypeWorkflowNode.GetNode(type);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Fills information about action of step.
        /// </summary>
        /// <param name="action">Workflow action</param>
        public void LoadAction(WorkflowActionInfo action)
        {
            // Get icon
            string iconUrl = action.Generalized.GetIconUrl(0, 0, 24);
            if (!string.IsNullOrEmpty(iconUrl))
            {
                IconImageUrl = URLHelper.ResolveUrl(iconUrl);
            }
            else
            {
                IconClass = !string.IsNullOrEmpty(action.ActionIconClass) ? action.ActionIconClass : "icon-cogwheels";
            }

            // Get thumbnail
            string thumbnailUrl = action.Generalized.GetThumbnailUrl(0, 0, 80);
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                ThumbnailImageUrl = URLHelper.ResolveUrl(thumbnailUrl);
            }
            else
            {
                ThumbnailClass = !string.IsNullOrEmpty(action.ActionThumbnailClass) ? action.ActionThumbnailClass : "icon-cogwheels";
            }
        }


        /// <summary>
        /// Method returning default list of source points based on given information.
        /// </summary>
        /// <returns>List of source points</returns>
        protected override List<SourcePoint> GetDefaultSourcePoints()
        {
            List<SourcePoint> result = new List<SourcePoint>();
            if ((mWorkflowStep != null) && (mWorkflowStep.StepDefinition != null))
            {
                List<WorkflowSourcePointDefinition> sourcePoints = mWorkflowStep.StepDefinition.SourcePoints;

                foreach (WorkflowSourcePointDefinition sourcePoint in sourcePoints)
                {
                    result.Add(new WorkflowSourcePoint(sourcePoint));
                }
            }
            return result;
        }

        #endregion
    }
}
