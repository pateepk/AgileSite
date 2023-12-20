using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Defines sides to which dropdown items of the multi button can be aligned.
    /// </summary>
    public enum CMSMultiButtonDropDownItemsAlignment
    {
        /// <summary>
        /// LEFT edge of the dropdown items will be aligned with the LEFT side of the visible button, overflow will float RIGHT.
        /// </summary>
        Left,

        /// <summary>
        /// RIGHT edge of the dropdown items will be aligned with the RIGHT side of the visible button, overflow will float LEFT
        /// </summary>
        Right,
    }


    /// <summary>
    /// Base class for CMS Button with support for multiple actions.
    /// </summary>
    public abstract class CMSMultiButtonBase : CMSWebControl, IPostBackEventHandler
    {
        /// <summary>
        /// Occurs when button in button group is clicked
        /// </summary>
        public event EventHandler<CMSButtonActionClickedEventArgs> Click;


        /// <summary>
        /// Button actions. Has to be set before PreRender event.
        /// </summary>
        public List<CMSButtonAction> Actions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates to which direction button actions will be aligned. Default is LEFT.
        /// </summary>
        public CMSMultiButtonDropDownItemsAlignment DropDownItemsAlignment
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the first action should be rendered as part of the primary button.
        /// </summary>
        public bool RenderFirstActionSeparately { get; set; } = true;


        /// <summary>
        /// Constructor.
        /// </summary>
        protected CMSMultiButtonBase() : base(HtmlTextWriterTag.Div)
        {
            Actions = new List<CMSButtonAction>();
            DropDownItemsAlignment = CMSMultiButtonDropDownItemsAlignment.Left;
        }


        /// <summary>
        /// Creates controls which are rendered in the Render phase.
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Do nothing if no action is set
            if (!Actions.Any())
            {
                return;
            }

            // If only one action is added, render classic CMSButton.
            if (RenderFirstActionSeparately && Actions.Count == 1)
            {
                InitChildControlsForOneAction(Actions.Single());
            }
            else
            {
                InitChildControlsForMoreThanOneActions(Actions);
            }
        }


        /// <summary>
        /// Initializes the Controls collection when only one CMSButtonAction is present.
        /// </summary>
        /// <param name="onlyAction">The only action of the button</param>
        protected virtual void InitChildControlsForOneAction(CMSButtonAction onlyAction)
        {
            var button = CreateButtonForOneAction(onlyAction);

            Controls.Add(button);
        }


        /// <summary>
        /// Creates button which is rendered when there is only one CMSButtonAction added.
        /// </summary>
        /// <param name="onlyAction">The only action of the button</param>
        /// <returns>Button representing the onl action of the multi button</returns>
        protected virtual CMSButton CreateButtonForOneAction(CMSButtonAction onlyAction)
        {
            var button = new CMSButton
            {
                ButtonStyle = ButtonStyle.Default,
                Text = onlyAction.Text,
                Enabled = Enabled && onlyAction.Enabled,
                OnClientClick = GetActionOnClientClick(onlyAction),
                ToolTip = onlyAction.ToolTip,
                UseSubmitBehavior = false
            };
            return button;
        }


        /// <summary>
        /// Initializes Controls collection if there are more than one actions added.
        /// </summary>
        /// <param name="actions">Actions of this multi button</param>
        protected virtual void InitChildControlsForMoreThanOneActions(List<CMSButtonAction> actions)
        {
            // button after which the dropdown with other options is shown
            var toggleButton = InitOpeningButtonControls(actions.First());

            HtmlGenericControl actionslistControl = new HtmlGenericControl("ul");
            actionslistControl.Attributes.Add("role", "menu");
            actionslistControl.Attributes.Add("aria-labelledby", toggleButton.ClientID);
            actionslistControl.Attributes.Add("class", "dropdown-menu");
            if (DropDownItemsAlignment == CMSMultiButtonDropDownItemsAlignment.Right)
            {
                actionslistControl.Attributes["class"] += " dropdown-menu-right";
            }

            Controls.Add(actionslistControl);

            var filteredActions = RenderFirstActionSeparately ? actions.Skip(1) : actions;
            foreach (var action in filteredActions)
            {
                // item of the dropdown (li)
                var listItem = CreateListItem(action);
                actionslistControl.Controls.Add(listItem);
            }
        }


        /// <summary>
        /// Creates one dropdown list item when there are more than one actions added. List item contains hyperlink which triggers postback when clicked.
        /// </summary>
        /// <param name="action">List item representing this action will be created</param>
        /// <returns>List item representing one action</returns>
        protected virtual HtmlGenericControl CreateListItem(CMSButtonAction action)
        {
            var listItem = new HtmlGenericControl("li");
            if (!Enabled || !action.Enabled)
            {
                listItem.Attributes.Add("class", "disabled");
            }

            var hyperLink = CreateListItemHyperLink(action);

            listItem.Controls.Add(hyperLink);
            return listItem;
        }


        /// <summary>
        /// Creates hyperlink which is used as a content of the list item. This is the clickable part of the dropdown list. 
        /// </summary>
        /// <param name="action">Hyperlink will represent this action</param>
        /// <returns></returns>
        protected virtual HyperLink CreateListItemHyperLink(CMSButtonAction action)
        {
            var hyperLink = new HyperLink
            {
                Text = HTMLHelper.HTMLEncode(action.Text),
                NavigateUrl = "javascript:void(0)",
            };

            if (Enabled && action.Enabled)
            {
                hyperLink.Attributes.Add("OnClick", GetActionOnClientClick(action));
            }

            if (action.ToolTip != null)
            {
                hyperLink.Attributes.Add("title", action.ToolTip);
            }
            return hyperLink;
        }


        /// <summary>
        /// Creates javascript code which triggers postback to this control and optionally additional client code specified by the CMSButtonAction OnClientClick property.
        /// </summary>
        /// <param name="action">Script's action</param>
        /// <returns>Client script which should be triggered after clicking on the specified action</returns>
        protected virtual string GetActionOnClientClick(CMSButtonAction action)
        {
            return action.OnClientClick + ControlsHelper.GetPostBackEventReference(this, action.Name) + "; return false;";
        }


        /// <summary>
        /// Creates controls displayed at first when dropdown with other actions is hidden. After clicking on this control, dropdown with other actions will be automatically expanded.
        /// </summary>
        /// <param name="firstAction">Action which represents the opening control</param>
        /// <returns>Click on the returned control will expand dropdown list with other actions</returns>
        protected abstract WebControl InitOpeningButtonControls(CMSButtonAction firstAction);


        #region "IPostBackEventHandler methods"

        /// <summary>
        /// Enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">Postback event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            if (Click != null)
            {
                Click(this, new CMSButtonActionClickedEventArgs
                {
                    ActionName = eventArgument,
                });
            }
        }
    }

    #endregion
}