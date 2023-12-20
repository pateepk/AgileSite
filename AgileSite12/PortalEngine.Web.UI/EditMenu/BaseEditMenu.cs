using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Base.Web.UI.ActionsConfig;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Base class for edit menu control.
    /// </summary>
    public abstract class BaseEditMenu : InlineUserControl
    {
        #region "Constants"

        /// <summary>
        /// Callback ID
        /// </summary>
        protected const string CALLBACK_ID = "##CALL##";

        /// <summary>
        /// Callback data separator
        /// </summary>
        protected const string CALLBACK_SEP = "##SEP##";

        #endregion


        #region "Variables"

        /// <summary>
        /// List of the extra actions added to the menu
        /// </summary>
        protected List<HeaderAction> mExtraActions;

        private bool mEnabled = true;
        private string mResourceCulture;

        #endregion


        #region "Button display options"

        /// <summary>
        /// If true, the menu shows the standard editing buttons
        /// </summary>
        public bool HideStandardButtons 
        { 
            get; 
            set; 
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Resource strings culture
        /// </summary>
        protected string ResourceCulture
        {
            get
            {
                if(string.IsNullOrEmpty(mResourceCulture))
                {
                    mResourceCulture = HTMLHelper.HTMLEncode(IsLiveSite ? LocalizationContext.PreferredCultureCode : MembershipContext.AuthenticatedUser.PreferredUICultureCode);
                }

                return mResourceCulture;
            }
            set
            {
                mResourceCulture = value;
            }
        }


        /// <summary>
        /// Link CSS class
        /// </summary>
        public virtual string LinkCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Refresh interval
        /// </summary>
        public int RefreshInterval
        {
            get;
            set;
        }


        /// <summary>
        /// Refresh script to be called when step ID of document changes
        /// </summary>
        public virtual string OnClientStepChanged
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if small icons should be used for actions
        /// </summary>
        public virtual bool UseSmallIcons
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if Save action is allowed.
        /// </summary>
        public virtual bool AllowSave
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the menu is enabled
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
        /// If true, the access permissions to the items are checked.
        /// </summary>
        public virtual bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// If set, the given text is displayed as information text
        /// </summary>
        public string InformationText
        { 
            get; 
            set; 
        }

        #endregion


        #region "Events"

        /// <summary>
        /// On get client validation script handler.
        /// </summary>
        public event EventHandler<EditMenuEventArgs> OnGetClientValidationScript;


        /// <summary>
        /// On get client action script handler.
        /// </summary>
        public event EventHandler<EditMenuEventArgs> OnGetClientActionScript;


        /// <summary>
        /// Raises event to get client validation script.
        /// </summary>
        /// <param name="actionName">Action</param>
        /// <param name="actionScript">Action script</param>
        protected string RaiseGetClientValidationScript(string actionName, string actionScript)
        {
            string valScript = null;
            if (OnGetClientValidationScript != null)
            {
                // Get script
                EditMenuEventArgs args = new EditMenuEventArgs(actionName);
                OnGetClientValidationScript(this, args);
                valScript = args.ValidationScript;
            }

            if (!string.IsNullOrEmpty(valScript))
            {
                valScript = "if(!" + valScript + ") { return false; }";
            }
            return valScript + actionScript;
        }


        /// <summary>
        /// Raises event to get client action script.
        /// </summary>
        /// <param name="actionName">Action name</param>
        protected string RaiseGetClientActionScript(string actionName)
        {
            string actionScript = String.Empty;

            if (OnGetClientActionScript != null)
            {
                EditMenuEventArgs args = new EditMenuEventArgs(actionName);
                OnGetClientActionScript(this, args);
                actionScript = args.ClientActionScript;
            }

            return actionScript;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds the alert message to the output request window.
        /// </summary>
        /// <param name="message">Message to display</param>
        protected void AddAlert(string message)
        {
            ScriptHelper.RegisterStartupScript(this, typeof(string), message.GetHashCode().ToString(), ScriptHelper.GetAlertScript(message));
        }


        /// <summary>
        /// Adds the script to the output request window.
        /// </summary>
        /// <param name="script">Script to add</param>
        protected void AddScript(string script)
        {
            if (script != null)
            {
                ScriptHelper.RegisterStartupScript(this, typeof(string), script.GetHashCode().ToString(), ScriptHelper.GetScript(script));
            }
        }


        /// <summary>
        /// Ensures java-script for split mode.
        /// </summary>
        protected void EnsureSplitModeScript()
        {
            if (RequestHelper.IsPostBack() && PortalUIHelper.DisplaySplitMode)
            {
                // Indicates if is post-back and original and split mode cultures are same
                bool refresh = (CMSString.Compare(CultureHelper.GetOriginalPreferredCulture(), PortalUIHelper.SplitModeCultureCode, true) == 0);

                // Register java-script
                ScriptHelper.RegisterSplitModeSync(Page, true, false, refresh, true);
            }
        }


        /// <summary>
        /// Adds the extra action to the menu
        /// </summary>
        /// <param name="action">Action to add</param>
        public void AddExtraAction(HeaderAction action)
        {
            if (mExtraActions == null)
            {
                mExtraActions = new List<HeaderAction>();
            }

            mExtraActions.Add(action);
        }

        #endregion
    }
}
