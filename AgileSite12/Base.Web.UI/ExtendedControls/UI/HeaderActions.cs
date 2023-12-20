using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI.ActionsConfig;
using CMS.Helpers;
using CMS.Modules;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// HeaderActions base class.
    /// </summary>
    [ToolboxItem(false)]
    public abstract class HeaderActions : AbstractUserControl
    {
        #region "Constants"

        /// <summary>
        /// Link button type
        /// </summary>
        public const string TYPE_LINKBUTTON = "linkbutton";

        /// <summary>
        /// Hyper link button type
        /// </summary>
        public const string TYPE_HYPERLINK = "hyperlink";

        /// <summary>
        /// Save button type
        /// </summary>
        public const string TYPE_SAVEBUTTON = "savebutton";

        #endregion


        #region "Variables"

        private List<HeaderAction> mActionsList = new List<HeaderAction>();
        private List<AbstractUserControl> mAdditionalControls;
        private List<CMSButton> mProcessedBaseButtons;
        private bool mEnabled = true;
        private bool mPerformFullPostBack = true;
        private bool? mUseSmallIcons;

        #endregion


        #region "Events"

        /// <summary>
        /// Get action script delegate.
        /// </summary>
        public delegate string GetActionScriptEventHandler(object sender, GetActionScriptEventArgs e);


        /// <summary>
        /// Event to retrieve custom action script.
        /// </summary>
        public event GetActionScriptEventHandler OnGetActionScript;


        /// <summary>
        /// Event which raises when LinkButton is used for action and its Command event raises.
        /// </summary>
        public event CommandEventHandler ActionPerformed;


        /// <summary>
        /// Event fired before and after every action control construction.
        /// </summary>
        public HeaderActionControlCreatedHandler ActionControlCreated = new HeaderActionControlCreatedHandler();


        /// <summary>
        /// Raises the action performed event.
        /// </summary>
        protected void RaiseActionPerformed(object sender, CommandEventArgs e)
        {
            if (ActionPerformed != null)
            {
                ActionPerformed(sender, e);
            }
            else if (!string.IsNullOrEmpty(e.CommandName))
            {
                // Special treatment for save action
                if (e.CommandName == ComponentEvents.SAVE)
                {
                    // Fire the validation event
                    SimpleManagerEventArgs args = new SimpleManagerEventArgs(e.CommandName);
                    ComponentEvents.RequestEvents.RaiseComponentEvent(this, args, ComponentName, ComponentEvents.VALIDATE_DATA);
                    
                    // Validation passed successfully
                    if (args.IsValid)
                    {
                        // Fire save data event
                        ComponentEvents.RequestEvents.RaiseComponentEvent(this, e, ComponentName, ComponentEvents.SAVE_DATA);

                        // Fire save event
                        ComponentEvents.RequestEvents.RaiseComponentEvent(this, e, ComponentName, e.CommandName);
                    }
                    else
                    {
                        // Show error message
                        AddError(args.ErrorMessage);
                    }
                }
                else
                {
                    // Fire the event
                    ComponentEvents.RequestEvents.RaiseComponentEvent(this, e, ComponentName, e.CommandName);
                }
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Update panel
        /// </summary>
        public abstract CMSUpdatePanel UpdatePanel
        {
            get;
        }


        /// <summary>
        /// Panel for actions.
        /// </summary>
        protected abstract Panel ActionsPanel
        {
            get;
        }


        /// <summary>
        /// Panel for additional controls.
        /// </summary>
        protected abstract Panel AdditionalControlsPanel
        {
            get;
        }


        /// <summary>
        /// Indicates if basic styles should be used
        /// </summary>
        public virtual bool UseBasicStyles
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if container should be rendered
        /// </summary>
        public virtual bool RenderContainer
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the actions should perform full post-back
        /// </summary>
        public virtual bool PerformFullPostBack
        {
            get
            {
                return mPerformFullPostBack;
            }
            set
            {
                mPerformFullPostBack = value;
            }
        }


        /// <summary>
        /// Indicates if control is enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Indicates if small icons should be used for actions
        /// </summary>
        public virtual bool UseSmallIcons
        {
            get
            {
                if (mUseSmallIcons == null)
                {
                    mUseSmallIcons = !UseBasicStyles && !IsLiveSite;
                }

                return mUseSmallIcons.Value;
            }
            set
            {
                mUseSmallIcons = value;
            }
        }


        /// <summary>
        /// Gets or sets the list of actions
        /// </summary>
        public virtual List<HeaderAction> ActionsList
        {
            get
            {
                return mActionsList;
            }
            set
            {
                mActionsList = value;
            }
        }


        /// <summary>
        /// Gets or sets CssClass of the panel where all the actions are placed.
        /// </summary>
        public virtual string PanelCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Additional controls CSS class
        /// </summary>
        public virtual string AdditionalControlsCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Additional controls list
        /// </summary>
        public virtual List<AbstractUserControl> AdditionalControls
        {
            get
            {
                return mAdditionalControls ?? (mAdditionalControls = new List<AbstractUserControl>());
            }
        }


        /// <summary>
        /// List of processed base buttons
        /// </summary>
        protected List<CMSButton> ProcessedBaseButtons
        {
            get
            {
                return mProcessedBaseButtons ?? (mProcessedBaseButtons = new List<CMSButton>());
            }
        }


        /// <summary>
        /// Action that is connected with a keyboard shortcut.
        /// </summary>
        protected HeaderAction ShortcutAction
        {
            get;
            private set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reloads the action panel - creates actions from the <see cref="ActionsList"/>.
        /// </summary>
        public virtual void ReloadData()
        {
            int actionsCount = ActionsList.Count;
            if (actionsCount > 0)
            {
                ActionsPanel.Controls.Clear();

                if (!String.IsNullOrEmpty(PanelCssClass))
                {
                    ActionsPanel.CssClass = PanelCssClass;
                }

                CreateActions(ActionsList, ActionsPanel);
            }

            AdditionalControlsPanel.CssClass += " " + AdditionalControlsCssClass;
        }


        /// <summary>
        /// Adds given action to the <see cref="ActionsList"/>.
        /// </summary>
        /// <param name="action">Action</param>
        public virtual void AddAction(HeaderAction action)
        {
            if (action == null)
            {
                return;
            }

            // Make sure the Save action is set only once
            string key = string.Format("HeaderActionsSaveSet_{0}_{1}", action.CommandArgument, ClientID);
            bool saveSet = ValidationHelper.GetBoolean(RequestStockHelper.GetItem(key), false);
            if (!(action is SaveAction) || !saveSet)
            {
                bool added = false;

                // Ensure correct index
                if (action.Index == -1)
                {
                    action.Index = ActionsList.Count;
                }
                else
                {
                    // Post processing of action attribute
                    for (int i = 0; i < ActionsList.Count; i++)
                    {
                        if (ActionsList[i].Index == action.Index)
                        {
                            // Replace action with the same index
                            ActionsList[i] = action;

                            // Button added
                            added = true;
                            break;
                        }
                    }
                }

                // If action with the same index was not found, add it to the list
                if (!added)
                {
                    ActionsList.Add(action);
                }

                // Keep flag
                if (action is SaveAction)
                {
                    RequestStockHelper.Add(key, (action.BaseButton == null) || action.BaseButton.Visible);
                }
            }

            // Store base buttons
            if ((action.BaseButton != null) && !ProcessedBaseButtons.Contains(action.BaseButton))
            {
                ProcessedBaseButtons.Add(action.BaseButton);
            }
        }


        /// <summary>
        /// Adds the list of header actions to the <see cref="ActionsList"/>.
        /// </summary>
        /// <param name="actions">Header actions</param>
        public void AddActions(params HeaderAction[] actions)
        {
            foreach (var action in actions)
            {
                AddAction(action);
            }
        }


        /// <summary>
        /// Inserts action to the <see cref="ActionsList"/> at specified index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="action">Action</param>
        public virtual void InsertAction(int index, HeaderAction action)
        {
            if (action == null)
            {
                return;
            }

            action.Index = index;
            AddAction(action);
        }


        /// <summary>
        /// Reloads additional controls panel from the list <see cref="AdditionalControls"/>.
        /// </summary>
        public virtual void ReloadAdditionalControls()
        {
            AdditionalControlsPanel.Controls.Clear();
            foreach (var ctrl in AdditionalControls)
            {
                AdditionalControlsPanel.Controls.Add(ctrl);
            }
        }


        /// <summary>
        /// Indicates if the menu has any action to display.
        /// </summary>
        public bool HasAnyVisibleAction()
        {
            return (ActionsList.Any(action => action.IsVisible()));
        }


        /// <summary>
        /// Indicates if the menu has content to display.
        /// </summary>
        public bool IsVisible()
        {
            return HasAnyVisibleAction() && Visible;
        }


        /// <summary>
        /// Clears content rendered by header actions control.
        /// </summary>
        public void Clear()
        {
            ActionsPanel.Controls.Clear();
            AdditionalControlsPanel.Controls.Clear();
        }


        /// <summary>
        /// Returns true if user has permission for given resource.
        /// Returns true for empty resource name or permission.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="permission">Permission name to check</param>
        protected bool CheckPermissions(string resourceName, string permission)
        {
            // Check only if both filled
            if (!String.IsNullOrEmpty(resourceName) && !String.IsNullOrEmpty(permission))
            {
                return CurrentUser.IsAuthorizedPerResource(resourceName, permission, SiteContext.CurrentSiteName);
            }

            return true;
        }


        /// <summary>
        /// Indicates if given action can be added to the header.
        /// </summary>
        /// <param name="action">Action to be checked</param>
        protected virtual bool IsActionVisible(HeaderAction action)
        {
            return action.IsVisible() && ((action.BaseButton == null) || (action.BaseButton.Visible));
        }

        #endregion


        #region "Private methods"

        private void CreateActions(List<HeaderAction> actions, Control parent)
        {
            int actionsCount = actions.Count;
            // Sort actions by index to be sure the order is ensured for multiple actions
            if (actionsCount > 1)
            {
                // At least one action has index
                if (actions.Exists(a => (a.Index != -1)))
                {
                    // Sort the actions
                    actions.Sort((a1, a2) => a1.Index.CompareTo(a2.Index));
                }
            }

            // Generate the actions
            for (int i = 0; i < actionsCount; ++i)
            {
                var action = actions[i];

                // If the text is not specified or visibility is false, skip the action
                if (!IsActionVisible(action))
                {
                    // Skip empty action
                    action.Visible = false;
                    continue;
                }

                // Check permission if action is enabled
                if (action.Enabled)
                {
                    action.Enabled = CheckPermissions(action.ResourceName, action.Permission);
                }

                // Get the action parameters
                string ctrlId = String.Concat(ID, "_HA_", i);
                Control actionControl;

                HeaderActionControlCreatedEventArgs args = new HeaderActionControlCreatedEventArgs
                {
                    Action = action,
                };

                // Start the ActionControlCreated event
                using (var actionCreated = ActionControlCreated.StartEvent(args))
                {
                    if (actionCreated.CanContinue())
                    {
                        // Ensure correct HeaderAction instance is used
                        action = args.Action;

                        // Use multi button when action contains alternative actions
                        if ((action.AlternativeActions != null) && action.AlternativeActions.Any())
                        {
                            // Get main action
                            var controlActions = new List<CMSButtonAction>
                            {
                                GetControlAction(action)
                            };

                            if (action.RegisterShortcutScript)
                            {
                                RegisterSaveShortcutScript(action, ctrlId);
                            }

                            // Get other actions
                            for (int j = 0; j < action.AlternativeActions.Count; j++)
                            {
                                var alternativeAction = action.AlternativeActions[j];

                                controlActions.Add(GetControlAction(alternativeAction));
                                if (action.RegisterShortcutScript)
                                {
                                    RegisterSaveShortcutScript(action, ctrlId + "_" + j);
                                }
                            }

                            var button = action.Inactive ? (CMSMultiButtonBase)new CMSToggleButton() : new CMSMoreOptionsButton();

                            button.Enabled = Enabled;
                            button.ID = ctrlId;
                            button.Actions = controlActions;

                            actionControl = button;
                        }
                        // Use classic button
                        else
                        {
                            var controlAction = GetControlAction(action);
                            var button = new CMSButton
                            {
                                ButtonStyle = action.ButtonStyle,
                                ID = ctrlId,
                                Enabled = controlAction.Enabled,
                                Text = controlAction.Text,
                                OnClientClick = controlAction.OnClientClick,
                                ToolTip = action.Tooltip,
                                UseSubmitBehavior = false
                            };

                            if (action.RegisterShortcutScript)
                            {
                                RegisterSaveShortcutScript(action, ctrlId);
                            }

                            actionControl = button;
                        }

                        if ((action.CssClass != null) && (actionControl is WebControl))
                        {
                            ((WebControl)actionControl).AddCssClass(action.CssClass);
                        }

                        args.ActionControl = actionControl;
                    }

                    // Finish the AcrtionControlCreated event
                    actionCreated.FinishEvent();
                }

                actionControl = args.ActionControl;

                // Add control to the panel
                if (actionControl != null)
                {
                    parent.Controls.Add(actionControl);
                }
            }
        }


        private CMSButtonAction GetControlAction(HeaderAction headerAction)
        {
            var controlAction = new CMSButtonAction();

            controlAction.Text = headerAction.Text;
            controlAction.Enabled = headerAction.Enabled && Enabled;
            controlAction.ToolTip = headerAction.Tooltip;

            // Register script only when action is active
            if (Enabled && headerAction.Enabled && !headerAction.Inactive)
            {
                    // Wrap script from OnClick property into anonymous function so it won't cancel the following script in case this property script returns true. 
                    // The execution of following script is canceled only when anonymous function returns false.
                    if (!String.IsNullOrEmpty(headerAction.OnClientClick))
                {
                    string onClickScript = "var onClickWrapper = function(sender) { " + headerAction.OnClientClick + "}; if (onClickWrapper(this) === false) { return false; }";
                    controlAction.OnClientClick = onClickScript;
                }

                string commandName = !string.IsNullOrEmpty(headerAction.CommandName) ? headerAction.CommandName : headerAction.EventName;

                // Perform post-back
                if (!String.IsNullOrEmpty(commandName) || !String.IsNullOrEmpty(headerAction.CommandArgument))
                {
                    string postbackScript = GetPostBackEventReference(commandName, headerAction.CommandArgument, headerAction.ValidationGroup);
                    controlAction.OnClientClick += postbackScript + ";";
                }
                else
                {
                    // Use URL only for standard link
                    if (!String.IsNullOrEmpty(headerAction.RedirectUrl))
                    {
                        var target = headerAction.Target ?? "_self";

                        var url = ScriptHelper.ResolveUrl(headerAction.RedirectUrl);

                        if (headerAction.OpenInDialog)
                        {
                            url = URLHelper.AddParameterToUrl(url, "dialog", "1");
                            url = ApplicationUrlHelper.AppendDialogHash(url);

                            ScriptHelper.RegisterDialogScript(Page);

                            controlAction.OnClientClick = ScriptHelper.GetModalDialogScript(url, "action" + headerAction.Index, headerAction.DialogWidth, headerAction.DialogHeight, false);
                        }
                        else
                        {
                            controlAction.OnClientClick += "window.open('" + url + "','" + target + "');";
                        }
                    }
                }

                // Stop automatic postback rendered by asp button 
                controlAction.OnClientClick += " return false;";
            }

            return controlAction;
        }


        /// <summary>
        /// Register the CRTL+S shortcut.
        /// </summary>
        /// <param name="action">Save header action</param>
        /// <param name="scriptID">Id of the save control</param>
        private void RegisterSaveShortcutScript(HeaderAction action, string scriptID)
        {
            // Register script only when action is active
            if (Enabled && action.Enabled && !action.Inactive)
            {
                string commandName = !string.IsNullOrEmpty(action.CommandName) ? action.CommandName : action.EventName;

                string script = null;

                // Perform post-back
                if (!String.IsNullOrEmpty(commandName) || !String.IsNullOrEmpty(action.CommandArgument))
                {
                    // Register encapsulation function for OnClientClick in shortcut. 
                    if (!string.IsNullOrEmpty(action.OnClientClick))
                    {
                        script = "if (PerfAction_" + scriptID + "() === false) { return false; }";

                        string scriptFunction = "function PerfAction_" + scriptID + "() { " + action.OnClientClick + "}";

                        ScriptHelper.RegisterStartupScript(Page, typeof(string), "PerfAction_" + scriptID, scriptFunction, true);
                    }

                    // Store action information for shortcut event validation registration
                    ShortcutAction = new HeaderAction
                    {
                        CommandArgument = action.CommandArgument,
                        ValidationGroup = action.ValidationGroup
                    };

                    string postbackScript = GetPostBackEventReference(commandName, action.CommandArgument, action.ValidationGroup);

                    // Prepare action script
                    script = String.Concat(script, " ", postbackScript, ";");
                }
                else
                {
                    script = action.OnClientClick;
                }

                ScriptHelper.RegisterSaveShortcut(Page, script);
            }
        }


        private string GetPostBackEventReference(string commandName, string commandArgument, string validationGroup)
        {
            string argument = string.Join(";", new[]
            {
                commandName,
                commandArgument
            });

            var opt = new PostBackOptions(this, argument)
            {
                PerformValidation = true,
                ValidationGroup = validationGroup
            };

            var original = ControlsHelper.GetPostBackEventReference(this, opt, false, !PerformFullPostBack, ActionsPanel);
            if (OnGetActionScript != null)
            {
                return OnGetActionScript(this, new GetActionScriptEventArgs
                {
                    ActionName = commandName,
                    OriginalScript = original
                });
            }

            return original;
        }

        #endregion
    }
}
