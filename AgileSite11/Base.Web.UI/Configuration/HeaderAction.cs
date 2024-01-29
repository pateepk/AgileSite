using System.Collections.Generic;

using CMS.Base;
using CMS.Helpers;
using CMS.Localization;

namespace CMS.Base.Web.UI.ActionsConfig
{
    /// <summary>
    /// Class for the header action.
    /// </summary>
    public class HeaderAction : NavigationItem
    {
        #region "Variables"

        private string mValidationGroup;
        private string mOnClientClick;
        private string mCommandArgument;
        private string mCommandName;
        private string mText;
        private string mCultureCode;
        private bool? mEnabled;
        private ButtonStyle mButtonStyle = ButtonStyle.Primary;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether separator class should be used before current item
        /// </summary>
        public bool GenerateSeparatorBeforeAction
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies the caption of the item.
        /// Sample value: "Caption string"
        /// (Has higher priority than the resource string.)
        /// </summary>
        public override string Text
        {
            get
            {
                if ((mText == null) && (BaseButton != null))
                {
                    return BaseButton.Text;
                }

                return mText;
            }
            set
            {
                mText = value;
            }
        }


        /// <summary>
        /// The JavaScript executed on client click event.
        /// </summary>
        public override string OnClientClick
        {
            get
            {
                if ((mOnClientClick == null) && (BaseButton != null))
                {
                    if (!string.IsNullOrEmpty(BaseButton.OnClientClick))
                    {
                        return BaseButton.OnClientClick;
                    }

                    return BaseButton.Attributes["onclick"];
                }

                return mOnClientClick;
            }
            set
            {
                mOnClientClick = value;
            }
        }


        /// <summary>
        /// Button to replace.
        /// </summary>
        public CMSButton BaseButton
        {
            get;
            set;
        }


        /// <summary>
        /// Index of the action.
        /// </summary>
        public int Index
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the action should be visible.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }


        /// <summary>
        /// Style of the header action button. Only applicable when action has no alternative actions.
        /// Multi-button (button displaying alternative actions) is always default.
        /// </summary>
        public ButtonStyle ButtonStyle
        {
            get
            {
                return mButtonStyle;
            }
            set
            {
                mButtonStyle = value;
            }
        }


        /// <summary>
        /// Indicates if the action is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                if ((mEnabled == null) && (BaseButton != null))
                {
                    return BaseButton.Enabled;
                }

                return !mEnabled.HasValue || mEnabled.Value;
            }
            set
            {
                mEnabled = value;
            }
        }


        /// <summary>
        /// Indicates if save shortcut script should be registered
        /// </summary>
        public bool RegisterShortcutScript
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the command representing the action.
        /// </summary>
        public string CommandName
        {
            get
            {
                if ((mCommandName == null) && (BaseButton != null))
                {
                    return BaseButton.CommandName;
                }

                return mCommandName;
            }
            set
            {
                mCommandName = value;
            }
        }


        /// <summary>
        /// Command argument of the action.
        /// </summary>
        public string CommandArgument
        {
            get
            {
                if ((mCommandArgument == null) && (BaseButton != null))
                {
                    return BaseButton.CommandArgument;
                }

                return mCommandArgument;
            }
            set
            {
                mCommandArgument = value;
            }
        }


        /// <summary>
        /// Target for hyperlink.
        /// </summary>
        public string Target
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the event.
        /// </summary>
        public string EventName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the action is inactive. Only applicable when action has alternative action.
        /// When inactive action can't be executed. Clicking on the action is used to toggle the visibility of alternative actions.
        /// </summary>
        public bool Inactive
        {
            get;
            set;
        }


        /// <summary>
        /// Validation group name.
        /// </summary>
        public string ValidationGroup
        {
            get
            {
                if ((mValidationGroup == null) && (BaseButton != null))
                {
                    return BaseButton.ValidationGroup;
                }

                return mValidationGroup;
            }
            set
            {
                mValidationGroup = value;
            }
        }


        /// <summary>
        /// Alternative actions to be displayed in context menu.
        /// </summary>
        public List<HeaderAction> AlternativeActions
        {
            get;
            set;
        }


        /// <summary>
        /// UI culture code.
        /// </summary>
        public string CultureCode
        {
            get
            {
                return mCultureCode ?? (mCultureCode = CMSActionContext.CurrentIsLiveSite ? LocalizationContext.PreferredCultureCode : CultureHelper.PreferredUICultureCode);
            }
            set
            {
                mCultureCode = value;
            }
        }


        /// <summary>
        /// Resource name.
        /// For Permission check to work, both Resource name and Permission must be filled.
        /// </summary>
        public string ResourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Permission name to check.
        /// For Permission check to work, both Resource name and Permission must be filled.
        /// </summary>
        public string Permission
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the action opens in dialog
        /// </summary>
        public bool OpenInDialog
        {
            get;
            set;
        }


        /// <summary>
        /// If action opens dialog, set dialog width
        /// </summary>
        public string DialogWidth
        {
            get;
            set;
        }


        /// <summary>
        /// If action opens dialog, set dialog height
        /// </summary>
        public string DialogHeight
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public HeaderAction()
        {
            Index = -1;
            Visible = true;
            AlternativeActions = new List<HeaderAction>();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Indicates if the action should be visible due to attributes values
        /// </summary>
        public bool IsVisible()
        {
            // Action is visible and has at least text or image set
            return Visible && !string.IsNullOrEmpty(Text);
        }

        #endregion
    }
}