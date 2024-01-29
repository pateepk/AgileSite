using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using CMS.Helpers;
using CMS.UIControls.UniMenuConfig;
using CMS.WorkflowEngine.Factories;
using CMS.WorkflowEngine.GraphConfig;
using CMS.DataEngine;

namespace CMS.WorkflowEngine.Web.UI
{
    /// <summary>
    /// Items representing step types in UniMenu control.
    /// </summary>
    public class StepTypeUniMenuItems : StepTypeDependencyInjector<Item>
    {
        #region "Private variables"

        /// <summary>
        /// Scope of draggable setting in JS.
        /// </summary>
        private readonly string mDraggableScope = "";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor setting context to menu items.
        /// </summary>
        /// <param name="draggableScope">Scope for JS dragging mechanism.</param>
        public StepTypeUniMenuItems(string draggableScope)
        {
            mDraggableScope = draggableScope;
        }

        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Method covering shared bootstrap for menu item.
        /// </summary>
        /// <returns>New menu item</returns>
        private Item CreateMenuItem()
        {
            Item newItem = new Item();
            newItem.CssClass = "BigButton";
            newItem.ImageAlign = ImageAlign.Top;
            newItem.MinimalWidth = 40;
            newItem.DraggableScope = mDraggableScope;
            return newItem;
        }


        /// <summary>
        /// Returns HTML representation of default draggable handler.
        /// </summary>
        /// <param name="rel">WorkflowStepTypeEnum value</param>
        /// <param name="text">Text</param>
        /// <returns>HTML code</returns>
        private string GetDefaultDraggableHandler(WorkflowStepTypeEnum rel, string text)
        {
            WorkflowNode node = StepTypeWorkflowNode.GetNode(rel);
            return string.Format(
"<div class='Node' rel='{0}'>"+
"   <div class='header {1}'>"+
"       <div class='text'>{2}</div>"+
"       <div class='clear'></div>"+
"   </div>"+
"   <div class='content gray gradient'>"+
"       <div class='main'></div>"+
"   </div>"+
"</div>", rel, node.CssClass, text);
        }


        /// <summary>
        /// Get tooltip
        /// </summary>
        /// <param name="text">Tooltip text</param>
        private string GetTooltip(string text)
        {
            return text + " " + ResHelper.GetString("workflow.dragtooltip");
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates menu items for workflow actions
        /// </summary>
        /// <param name="actions">Info objects</param>
        /// <returns>UniMenu Items</returns>
        public List<Item> GetSettingsObjectBy(InfoDataSet<WorkflowActionInfo> actions)
        {
            List<Item> menuItems = new List<Item>();
            foreach (WorkflowActionInfo action in actions)
            {
                menuItems.Add(GetActionSettings(action));
            }
            return menuItems;
        }


        /// <summary>
        /// Creates menu item for workflow action.
        /// </summary>
        /// <param name="actionDefinition">Info object</param>
        /// <returns>UniMenu Item</returns>
        public Item GetActionSettings(WorkflowActionInfo actionDefinition)
        {
            WorkflowNode node = StepTypeWorkflowNode.GetNode(WorkflowStepTypeEnum.Action);
            node.LoadAction(actionDefinition);

            string name = HTMLHelper.HTMLEncode(ResHelper.LocalizeString(actionDefinition.ActionDisplayName));
            string description = ResHelper.LocalizeString(actionDefinition.ActionDescription);
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.ImagePath = node.IconImageUrl;
            newItem.IconClass = node.IconClass;
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = String.Format(
"<div class='Node' rel='11' rev='{0}'>"+
"   <div class='header {1}'>"+
"       <div class='text'>{2}</div>" +
"       <div class='clear'></div>" +
"   </div>"+
"   <div class='content gray gradient'>"+
"       {3}"+
"   </div>"+
"</div>", actionDefinition.ActionID, node.CssClass, newItem.Text, !string.IsNullOrEmpty(node.ThumbnailImageUrl) ? "<div class='icon' style='background-image: url(" + node.ThumbnailImageUrl + ");'></div>" : "<div class='cms-icon-container'><i class='cms-icon-150 " + node.ThumbnailClass + "' /></div>");
            return newItem;
        }

        #endregion


        #region "Injection methods"

        /// <summary>
        /// Returns new menu item representing condition step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetConditionSettings()
        {
            string description = ResHelper.GetString("workflow.conditionStepTooltip");
            string name = ResHelper.GetString("workflow.conditionStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-diamond";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Condition, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing archived document step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetDocumentArchivedSettings()
        {
            string description = ResHelper.GetString("workflow.documentArchivedStepTooltip");
            string name = ResHelper.GetString("workflow.documentArchivedStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-square";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.DocumentArchived, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing edited document step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetDocumentEditSettings()
        {
            string description = ResHelper.GetString("workflow.documentEditStepTooltip");
            string name = ResHelper.GetString("workflow.documentEditStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-square";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.DocumentEdit, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing published document step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetDocumentPublishedSettings()
        {
            string description = ResHelper.GetString("workflow.documentPublishedStepTooltip");
            string name = ResHelper.GetString("workflow.documentPublishedStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-square";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.DocumentPublished, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing multi choice step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetMultichoiceSettings()
        {
            string description = ResHelper.GetString("workflow.multichoiceStepTooltip");
            string name = ResHelper.GetString("workflow.multichoiceStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-choice-multi-scheme";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Multichoice, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing multi choice first win step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetMultichoiceFirstWinSettings()
        {
            string description = ResHelper.GetString("workflow.multichoiceFirstWinStepTooltip");
            string name = ResHelper.GetString("workflow.multichoiceFirstWinStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-choice-single-scheme";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.MultichoiceFirstWin, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing standard step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetStandardSettings()
        {
            string description = ResHelper.GetString("workflow.standardStepTooltip");
            string name = ResHelper.GetString("workflow.standardStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-square";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Standard, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing start step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetStartSettings()
        {
            string description = ResHelper.GetString("workflow.startStepTooltip");
            string name = ResHelper.GetString("workflow.startStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-triangle-right";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Start, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing finished step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetFinishedSettings()
        {
            string description = ResHelper.GetString("workflow.finishedStepTooltip");
            string name = ResHelper.GetString("workflow.finishedStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-square";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Finished, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing user choice step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetUserchoiceSettings()
        {
            string description = ResHelper.GetString("workflow.userchoiceStepTooltip");
            string name = ResHelper.GetString("workflow.userchoiceStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-choice-user-scheme";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Userchoice, name);
            return newItem;
        }


        /// <summary>
        /// Returns new menu item representing wait step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetWaitSettings()
        {
            string description = ResHelper.GetString("workflow.waitStepTooltip");
            string name = ResHelper.GetString("workflow.waitStep");
            Item newItem = CreateMenuItem();
            newItem.Text = name;
            newItem.Tooltip = GetTooltip(description);
            newItem.IconClass = "icon-clock";
            newItem.ImageAltText = name;
            newItem.DraggableTemplateHandler = GetDefaultDraggableHandler(WorkflowStepTypeEnum.Wait, name);
            return newItem;
        }


        /// <summary>
        /// Throws appropriate exception.
        /// </summary>
        public override Item GetUndefinedSettings()
        {
            throw new InvalidOperationException("[StepTypeUniMenuItems]: Step of undefined type can not be in advanced workflow graph editor toolbar.");
        }


        /// <summary>
        /// Throws appropriate exception.
        /// </summary>
        public override Item GetDefaultSettings()
        {
            throw new InvalidOperationException("[StepTypeUniMenuItems]: Step of default type can not be in advanced workflow graph editor toolbar.");
        }


        /// <summary>
        /// Returns new menu item representing action step.
        /// </summary>
        /// <returns>New menu item</returns>
        public override Item GetActionSettings()
        {
            throw new InvalidOperationException("[StepTypeUniMenuItems]: Custom actions does not have default menu item.");
        }

        #endregion
    }
}
