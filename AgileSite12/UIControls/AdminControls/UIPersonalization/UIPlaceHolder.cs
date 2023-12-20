using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;

namespace CMS.UIControls
{
    /// <summary>
    /// Server control which represents container for UI element of the page.
    /// </summary>
    [ToolboxData("<{0}:UIPlaceHolder runat=server></{0}:UIPlaceHolder>")]
    public class UIPlaceHolder : PlaceHolder
    {
        #region "Delegates"

        /// <summary>
        /// OnIsHidden delegate
        /// </summary>
        public delegate bool OnIsHiddenDelegate();

        #endregion


        #region "Events"

        /// <summary>
        /// Fires to find out whether the place holder is hidden.
        /// </summary>
        public event OnIsHiddenDelegate OnIsHidden;


        /// <summary>
        /// Fires before the main visibility check.
        /// </summary>
        public event EventHandler OnBeforeCheck;

        #endregion


        #region "Variables"

        /// <summary>
        /// False if user is either global administrator or is authorized to see specified 
        /// UI element of the specified module, otherwise true.
        /// </summary>
        protected bool? mIsHidden = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns false if user is either global administrator or is authorized to see specified 
        /// UI element of the specified module, otherwise returns true.
        /// </summary>
        public bool IsHidden
        {
            get
            {
                if (AlwaysVisible)
                {
                    return false;
                }

                if (mIsHidden == null)
                {
                    mIsHidden = !EvaluateVisibility();
                }

                return mIsHidden.Value;
            }
        }


        /// <summary>
        /// Code name of the UIElement.
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// Code name of the Permission.
        /// </summary>
        public string PermissionName
        {
            get;
            set;
        }


        /// <summary>
        /// Code name of the module.
        /// </summary>
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if processing of the UI panel should be stopped.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the control doesn't perform the permission check and is always visible
        /// </summary>
        public bool AlwaysVisible
        {
            get;
            set;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Evaluates the visibility of the control
        /// </summary>
        private bool EvaluateVisibility()
        {
            bool result = false;

            // If custom OnIsHidden event is defined use it to get the IsHiddenProperty
            if (OnIsHidden != null)
            {
                result = OnIsHidden();
            }
            else
            {
                // Raise OnBeforeCheck event if defined
                if (OnBeforeCheck != null)
                {
                    OnBeforeCheck(this, EventArgs.Empty);
                }

                var ui = MembershipContext.AuthenticatedUser;

                // Check UI element
                if (!string.IsNullOrEmpty(ModuleName) && !string.IsNullOrEmpty(ElementName))
                {
                    // Check visible condition and feature availability
                    var uiElement = UIElementInfoProvider.GetUIElementInfo(ModuleName, ElementName);
                    if (uiElement != null)
                    {
                        result = !UIContextHelper.CheckElementAvailabilityInUI(uiElement);
                    }

                    result = result || ((ui == null) || !ui.IsAuthorizedPerUIElement(ModuleName, ElementName));
                }

                // Check permission
                if (!string.IsNullOrEmpty(PermissionName))
                {
                    result = result || ((ui == null) || !ui.IsAuthorizedPerResource(ModuleName, PermissionName));
                }
            }

            return !result;
        }


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                return;
            }

            if (!StopProcessing)
            {
                EnableViewState = !IsHidden;
                Visible = !IsHidden || SystemContext.DevelopmentMode;

                base.OnInit(e);
            }
        }


        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Check for the Design Mode in Visual Studio
            if (Context == null)
            {
                writer.Write("[UIPlaceHolder: " + ID + "]");
                return;
            }

            if (IsHidden)
            {
                if (SystemContext.DevelopmentMode)
                {
                    string text = null;

                    // UI element message
                    if (!String.IsNullOrEmpty(ModuleName) && !String.IsNullOrEmpty(ElementName))
                    {
                        text += String.Format(ResHelper.GetString("UIElement.HiddenElement"), ID, ElementName, ModuleName);
                    }
                    // Permission message
                    if (!String.IsNullOrEmpty(ModuleName) && !String.IsNullOrEmpty(PermissionName))
                    {
                        text += String.Format(ResHelper.GetString("UIElement.HiddenPermission"), ID, PermissionName, ModuleName);
                    }

                    // Render as informative placeholder in development mode
                    if (!String.IsNullOrEmpty(text))
                    {
                        writer.Write("<img src=\"" + UIHelper.GetImageUrl(Page, "Design/Controls/UI/hidden.png") + "\" class=\"HiddenUI\" title=\"" + text + "\" />");
                    }

                    return;
                }
                else
                {
                    return;
                }
            }

            base.Render(writer);
        }

        #endregion
    }
}