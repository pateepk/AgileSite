using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Group of buttons with capability to highlight selected button
    /// </summary>
    [ToolboxItem(false)]
    public class CMSButtonGroup : CMSWebControl, IPostBackEventHandler, IPostBackDataHandler
    {
        #region "Variables"

        private List<CMSButtonGroupAction> mActions = new List<CMSButtonGroupAction>();
        private bool mAutomaticButtonSelection = true;
        private string mSelectedActionName;

        #endregion


        #region "Properties"


        /// <summary>
        /// Button actions
        /// </summary>
        public List<CMSButtonGroupAction> Actions
        {
            get
            {
                return mActions;
            }
            set
            {
                mActions = value;
            }
        }


        /// <summary>
        /// Name of the currently selected action (active button)
        /// </summary>
        public string SelectedActionName
        {
            get
            {
                return GetSelectedActionName();
            }
            set
            {
                // When view state is loaded, save value into viewstate and reset local property. 
                if (IsTrackingViewState)
                {
                    mSelectedActionName = null;
                    ViewState["CMSButtonGroupActionName"] = value;
                }
                else
                {
                    mSelectedActionName = value;
                }
            }
        }


        /// <summary>
        /// When true, clicked button is automaticly selected as active.
        /// Behaves similarly to radio buttons. Must be set until InitComplete.
        /// </summary>
        public bool AutomaticButtonSelection
        {
            get
            {
                return mAutomaticButtonSelection;
            }
            set
            {
                mAutomaticButtonSelection = value;
            }
        }

        #endregion


        #region "Events"

        /// <summary>
        /// Occures when button in button group is clicked
        /// </summary>
        public event EventHandler<CMSButtonActionClickedEventArgs> Click;

        #endregion


        #region "Methods"

        /// <summary>
        /// OnPreRender event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            // Ensure that selected action index is saved in viewstate.
            SelectedActionName = GetSelectedActionName();

            // Register control to handle LoadPostData
            if (AutomaticButtonSelection)
            {
                Page.RegisterRequiresPostBack(this);
            }

            base.OnPreRender(e);
        }


        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass + " btn-group");

            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            InitChildControls();
            foreach (Control control in Controls)
            {
                control.RenderControl(writer);
            }

            writer.RenderEndTag();
        }


        /// <summary>
        /// Creates and initalizes child controls
        /// </summary>
        protected void InitChildControls()
        {
            foreach (var action in Actions)
            {
                // Button click triggers postback of CMSButtonGroup control with button index as event argument
                var buttonPostBackScript = ControlsHelper.GetPostBackEventReference(this, action.Name) + "; return false;";
                
                // Create button
                CMSButton button;
                if (action.UseIconButton)
                {
                    button = new CMSAccessibleButton
                    {
                        // When text is null, CMSAccessibleButton will use tooltip as description
                        ScreenReaderDescription = action.Text
                    };
                }
                else
                {
                    button = new CMSButton
                    {
                        Text = action.Text
                    };

                    button.ButtonStyle = ButtonStyle.Default;
                }

                button.OnClientClick = action.OnClientClick + buttonPostBackScript;
                button.IconCssClass = action.IconCssClass;
                button.Enabled = action.Enabled;
                button.ToolTip = action.ToolTip;
                button.UseSubmitBehavior = false;

                // Set active button
                if ((action.Name == SelectedActionName))
                {
                    button.AddCssClass("active");
                }

                Controls.Add(button);
            }
        }


        /// <summary>
        /// Returns name of the currently selected action
        /// </summary>
        private string GetSelectedActionName()
        {
            // Get explicitly set selected action
            string actionName = mSelectedActionName;

            // When view state is loaded and local property doesn't contain other then default value, get value from viewstate.
            if (IsTrackingViewState && (mSelectedActionName == null))
            {
                actionName = ViewState["CMSButtonGroupActionName"] as string;
            }

            // Automaticly select first action when no action is selected
            if ((actionName == null) && AutomaticButtonSelection && Actions.Any())
            {
                actionName = Actions.First().Name;
            }

            return actionName;
        }

        #endregion


        #region "IPostBackEventHandler methods"

        /// <summary>
        /// Enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">Postback event argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            if (Click != null)
            {
                Click(this, new CMSButtonActionClickedEventArgs(eventArgument));
            }
        }

        #endregion


        #region "IPostBackDataHandler methods"

        /// <summary>
        /// Processes postback data
        /// </summary>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            // Set clicked button as selected when automatic selection is enabled 
            if (AutomaticButtonSelection && postCollection["__EVENTTARGET"] == postDataKey)
            {
                SelectedActionName = postCollection["__EVENTARGUMENT"];
            }

            return false;
        }


        /// <summary>
        /// Raises any change events for the server control that implements the IPostBackDataHandler interface.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            // Method will never be called because LoadPostData always returns false
        }

        #endregion
    }
}
