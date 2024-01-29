using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form button control.
    /// </summary>
    [ToolboxData("<{0}:FormSubmitButton runat=server></{0}:FormSubmitButton>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormSubmitButton : LocalizedButton
    {
        #region "Variables"

        private bool? mRegisterHeaderAction = null;
        private HeaderActions mHeaderActions = null;
        private SaveAction mAction = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if header action should be registered. The submit button is hidden and Save header action is added.
        /// </summary>
        public bool RegisterHeaderAction
        {
            get
            {
                if (mRegisterHeaderAction == null)
                {
                    mRegisterHeaderAction = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUIRegisterHeaderAction"], true);
                }

                return mRegisterHeaderAction.Value;
            }
            set
            {
                mRegisterHeaderAction = value;
            }
        }


        /// <summary>
        /// Header action to be inserted to the header actions.
        /// </summary>
        public SaveAction Action
        {
            get
            {
                if (mAction == null)
                {
                    // Get default action
                    mAction = new SaveAction { Index = -2, BaseButton = this, CssClass = CssClass };
                }

                return mAction;
            }
        }


        /// <summary>
        /// Form
        /// </summary>
        public BasicForm Form
        {
            get;
            set;
        }


        /// <summary>
        /// Header actions control
        /// </summary>
        public HeaderActions HeaderActions
        {
            get
            {
                if (mHeaderActions == null)
                {
                    // Get page
                    ICMSPage page = Page as ICMSPage;

                    // Try to get header actions from parent controls
                    mHeaderActions = ControlsHelper.GetParentProperty<AbstractUserControl, HeaderActions>(this, s => s.HeaderActions, (page != null) ? page.HeaderActions : null);
                }

                return mHeaderActions;
            }
            set
            {
                mHeaderActions = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormSubmitButton()
        {
            EnableViewState = false;

            ButtonStyle = ButtonStyle.Primary;
            ResourceString = "general.ok";
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// OnLoad event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            if (!IsLiveSite && RegisterHeaderAction && !StopProcessing)
            {
                // Set save action
                AddHeaderAction();
            }

            base.OnLoad(e);
        }

        #endregion


        #region "Header actions methods"

        /// <summary>
        /// Adds action to the page header actions.
        /// </summary>
        private void AddHeaderAction()
        {
            HeaderActions actions = HeaderActions;
            if (actions != null)
            {
                actions.AddAction(Action);

                // Register save
                ComponentEvents.RequestEvents.RegisterForComponentEvent(ComponentName, ComponentEvents.SAVE, (s, args) =>
                                                                                                                {
                                                                                                                    if (Visible)
                                                                                                                    {
                                                                                                                        OnClick(args);
                                                                                                                    }
                                                                                                                });
            }
        }

        #endregion
    }
}