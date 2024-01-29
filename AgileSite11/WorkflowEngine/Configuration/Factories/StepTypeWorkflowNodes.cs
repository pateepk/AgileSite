using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.UniGraphConfig;
using CMS.WorkflowEngine.GraphConfig;

namespace CMS.WorkflowEngine.Factories
{
    /// <summary>
    /// Factory class for getting display name based on step type.
    /// </summary>
    public class StepTypeWorkflowNode : StepTypeDependencyInjector<WorkflowNode>
    {
        #region "Variables"

        /// <summary>
        /// Instance for singleton pattern usage.
        /// </summary>
        private static StepTypeWorkflowNode mInstance = null;

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates new workflow node instance based on given workflow step type.
        /// </summary>
        /// <param name="type">Type of workflow step</param>
        /// <returns>New workflow node</returns>
        public static WorkflowNode GetNode(WorkflowStepTypeEnum type)
        {
            if (mInstance == null)
            {
                mInstance = new StepTypeWorkflowNode();
            }
            return mInstance.GetSettingsObject(type);
        }

        #endregion


        #region "Injection methods"

        /// <summary>
        /// Creates new workflow node for step type action. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetActionSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " orange";
            node.Type = NodeTypeEnum.Action;
            node.TypeName = "Action";
            node.TypeResourceStringPrefix = "workflow.actionStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type condition. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetConditionSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " brown";
            node.Type = NodeTypeEnum.Condition;
            node.TypeName = "Condition";
            node.TypeResourceStringPrefix = "workflow.conditionStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type of archived document. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetDocumentArchivedSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " gray";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "DocArchived";
            node.TypeResourceStringPrefix = "workflow.documentArchivedStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type of edited document. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetDocumentEditSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " lightblue";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "DocEdit";
            node.TypeResourceStringPrefix = "workflow.documentEditStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type of published document. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetDocumentPublishedSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " green";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "DocPublished";
            node.TypeResourceStringPrefix = "workflow.documentPublishedStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type multi choice. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetMultichoiceSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " brown";
            node.Type = NodeTypeEnum.Multichoice;
            node.TypeName = "Multichoice";
            node.TypeResourceStringPrefix = "workflow.multichoiceStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type multi choice first win. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetMultichoiceFirstWinSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " brown";
            node.Type = NodeTypeEnum.Multichoice;
            node.TypeName = "MultichoiceFirstWin";
            node.TypeResourceStringPrefix = "workflow.multichoiceFirstWinStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type standard. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetStandardSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " darkblue";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "Standard";
            node.TypeResourceStringPrefix = "workflow.standardStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type start. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetStartSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " lightblue";
            node.Type = NodeTypeEnum.Action;
            node.TypeName = "Start";
            node.TypeResourceStringPrefix = "workflow.startStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            node.HasTargetPoint = false;
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type start. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetFinishedSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " green";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "Finished";
            node.TypeResourceStringPrefix = "workflow.finishedStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type undefined. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetUndefinedSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " gray";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "Undefined";
            node.TypeResourceStringPrefix = string.Empty;
            node.Name = string.Empty;
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type user choice. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetUserchoiceSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " brown";
            node.Type = NodeTypeEnum.Userchoice;
            node.TypeName = "Userchoice";
            node.TypeResourceStringPrefix = "workflow.userchoiceStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type wait. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetWaitSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " orange";
            node.Type = NodeTypeEnum.Action;
            node.TypeName = "Wait";
            node.TypeResourceStringPrefix = "workflow.waitStep";
            node.Name = ResHelper.GetString(node.TypeResourceStringPrefix);
            node.ThumbnailClass = "icon-clock";
            return node;
        }


        /// <summary>
        /// Creates new workflow node for step type default. 
        /// </summary>
        /// <returns>New workflow node</returns>
        public override WorkflowNode GetDefaultSettings()
        {
            WorkflowNode node = new WorkflowNode();
            node.CssClass += " darkblue";
            node.Type = NodeTypeEnum.Standard;
            node.TypeName = "Default";
            node.TypeResourceStringPrefix = string.Empty;
            node.Name = string.Empty;
            return node;
        }
        
        #endregion
    }
}
