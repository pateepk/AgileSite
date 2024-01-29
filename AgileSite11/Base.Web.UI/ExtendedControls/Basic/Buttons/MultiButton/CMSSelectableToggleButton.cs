using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Toggle multi button which acts as dropdown in the sense that one action can be selected and this actions is on top from now on.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSSelectableToggleButton : CMSToggleButton, IPostBackDataHandler
    {
        // Stores value explicitly set when it wasn't possible to set to ViewState directly
        private string mSelectedActionName;


        /// <summary>
        /// Gets or sets the name of the selected action from/to view state.
        /// </summary>
        private string SelectedActionNameViewState
        {
            get
            {
                return ViewState["SelectedActionName"] as string;
            }
            set
            {
                ViewState["SelectedActionName"] = value;
            }
        }


        /// <summary>
        /// Name of the currently selected action (active button). Becomes available at the Page's PreLoad phase.
        /// </summary>
        public virtual string SelectedActionName
        {
            get
            {
                // Get explicitly set selected action
                if (mSelectedActionName != null)
                {
                    return mSelectedActionName;
                }

                if (SelectedActionNameViewState != null)
                {
                    return SelectedActionNameViewState;
                }

                // Automatically select first action when no action is selected
                if (Actions.Any())
                {
                    return Actions.First().Name;
                }

                return null;
            }
            set
            {
                // When view state is loaded, save value into viewstate and reset local property. 
                if (IsTrackingViewState)
                {
                    mSelectedActionName = null;
                    SelectedActionNameViewState = value;
                }
                else
                {
                    mSelectedActionName = value;
                }
            }
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data. </param>
        protected override void OnPreRender(EventArgs e)
        {
            // Save selected action to the viewstate, so it is persisted across postbacks. 
            // This is needed when user has set value on the Init event (so it wasn't saved to the ViewState automatically) or when no action was selected.
            SelectedActionNameViewState = SelectedActionName;

            Page.RegisterRequiresPostBack(this);

            base.OnPreRender(e);
        }


        /// <summary>
        /// Moves the selected action to the first place before creating child controls.
        /// </summary>
        /// <param name="actions">Button actions</param>
        protected override void InitChildControlsForMoreThanOneActions(List<CMSButtonAction> actions)
        {
            var orderedActions = actions.OrderByDescending(a => a.Name == SelectedActionName).ToList();

            base.InitChildControlsForMoreThanOneActions(orderedActions);
        }


        #region "IPostBackDataHandler members"

        /// <summary>
        /// Processes postback data for an ASP.NET server control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control. </param>
        /// <param name="postCollection">The collection of all incoming name values. </param>
        /// <returns>Always false</returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            // Store selected item if postback is made to this button
            if (postCollection["__EVENTTARGET"] == postDataKey)
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