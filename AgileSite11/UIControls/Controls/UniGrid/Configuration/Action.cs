﻿using System;
using System.Xml.Linq;

using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid action button.
    /// </summary>
    public class Action : AbstractAction
    {
        #region "Variables"

        private string mCaptionText;
        private string mCaption;
        private string mSafeName;
        private string mName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the caption text of the action. Caches the value.
        /// </summary>
        public string CaptionText
        {
            get
            {
                return mCaptionText ?? (mCaptionText = ResHelper.GetString(String.IsNullOrEmpty(Caption) ? "general.action" : LocalizationHelper.GetResourceName(Caption)));
            }
        }


        /// <summary>
        /// Safe name of the action which can be used as an identifier. Caches the value.
        /// </summary>
        public string SafeName
        {
            get
            {
                return mSafeName ?? (mSafeName = Name.Replace('#', '_').ToLowerInvariant());
            }
        }


        /// <summary>
        /// Specifies the resource string used as the tooltip of the image defined in the icon attribute. Must begin and end with the $ character. 
        /// Sample value: "$General.Delete$"
        /// </summary>
        public string Caption
        {
            get
            {
                return mCaption;
            }
            set
            {
                mCaption = value;
                mCaptionText = null;
            }
        }


        /// <summary>
        /// The name of the column whose value should be passed as the actionArgument parameter of the OnAction event handler. If not defined, the first column of the data source is used.
        /// </summary>
        public string CommandArgument
        {
            get;
            set;
        }


        /// <summary>
        /// The resource string used in a JavaScript confirmation. Most commonly used as a confirmation for delete type actions. Must begin and end with the $ character.
        /// Sample value: "$General.ConfirmDelete$"
        /// </summary>
        public string Confirmation
        {
            get;
            set;
        }


        /// <summary>
        /// The relative path to a control (.ascx file) that implements a context menu for the action. Controls created for this purpose must inherit from the CMS.Base.Web.UI.CMSContextMenuControl class.
        /// Sample value: "~/CMSAdminControls/UI/UniGrid/Controls/ObjectMenu.ascx"
        /// </summary>
        public string ContextMenu
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the action that is passed as the sourceName parameter of the OnExternalDataBound event handler. 
        /// Sample value: "deletefile"
        /// </summary>
        public string ExternalSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the image that should be used as the icon of the action. The image must be located in the folder defined by the ImageDirectoryPath property of the UniGrid.
        /// Sample value: "delete.png"
        /// </summary>
        public string Icon
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the CSS class that serves as icon for the button.
        /// </summary>
        public string FontIconClass
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the style of the font icon.
        /// </summary>
        public GridIconStyle FontIconStyle
        {
            get;
            set;
        }


        /// <summary>
        /// Contains an array of parameters passed to the control implementing the action's context menu (the path to this control must be specified in the contextmenu attribute). These parameters may be retrieved in the control's code using the GetContextMenuParameter JavaScript function.
        /// The columns defined in the parameters attribute of the &lt;actions&gt; element may be entered as parameters using the following expressions:
        /// {0} - first parameter
        /// {1} - second parameter
        /// and so forth.
        /// "new Array('cms.site', '{0}')"
        /// </summary>
        public string MenuParameter
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies which mouse button causes the action's context menu to appear (if a context menu is enabled via the contextmenu attribute). 
        /// If not defined, both mouse buttons open the context menu.
        /// Possible values: "left", "right"
        /// </summary>
        public string MouseButton
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the action. This is passed to the handler of the OnAction event as the actionName parameter.
        /// Sample value: "delete"
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                mSafeName = null;
            }
        }


        /// <summary>
        /// The JavaScript OnClick function for the given action. It may use the columns defined in the parameters attribute of the 'actions' element as parameters, which can be called by using the following expressions: 
        /// {0} - first parameter
        /// {1} - second parameter
        /// and so forth.
        /// "alert(‘{0}’);"
        /// </summary>
        public string OnClick
        {
            get;
            set;
        }


        /// <summary>
        /// Module code name.
        /// Sample value: "cms.ecommerce"
        /// </summary>
        public string ModuleName
        {
            get;
            set;
        }


        /// <summary>
        /// Names of the module permissions which should be checked before the action is handled. Use comma as separator for multiple permissions.
        /// Sample value: "modify;manage"
        /// </summary>
        public string Permissions
        {
            get;
            set;
        }


        /// <summary>
        /// Names of the module UI elements which should be checked before the action is handled. Use comma as separator for multiple UI elements.
        /// Sample value: "uielement1;uielement2"
        /// </summary>
        public string UIElements
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if action is hidden if user is not authorized for specified module permissions and module UI elements.
        /// </summary>
        public bool HideIfNotAuthorized
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public Action()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Action name</param>
        public Action(string name)
        {
            Name = name;

            // Built-in action for object menu
            switch (Name)
            {
                case "#objectmenu":
                    Caption = "$General.OtherActions$";
                    FontIconClass = "icon-ellipsis";
                    ContextMenu = "~/CMSAdminControls/UI/UniGrid/Controls/ObjectMenu.ascx";
                    MenuParameter = "new Array('{objecttype}', '{0}')";
                    break;

                case "#clone":
                    Caption = "$General.Clone$";
                    FontIconClass = "icon-doc-copy";
                    break;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">XML element with the action definition</param>
        public Action(XElement action)
            : this(action.GetAttributeStringValue("name"))
        {
            Caption = action.GetAttributeValue("caption", Caption);
            CommandArgument = action.GetAttributeValue("commandargument", CommandArgument);
            Confirmation = action.GetAttributeValue("confirmation", Confirmation);
            ContextMenu = action.GetAttributeValue("contextmenu", ContextMenu);
            ExternalSourceName = action.GetAttributeValue("externalsourcename", ExternalSourceName);
            Icon = action.GetAttributeValue("icon", Icon);
            FontIconClass = action.GetAttributeValue("fonticonclass", FontIconClass);
            FontIconStyle = action.GetAttributeValue("fonticonstyle", "default").ToEnum<GridIconStyle>();
            MenuParameter = action.GetAttributeValue("menuparameter", MenuParameter);
            MouseButton = action.GetAttributeValue("mousebutton", MouseButton);
            OnClick = action.GetAttributeValue("onclick", OnClick);
            ModuleName = action.GetAttributeValue("modulename", ModuleName);
            Permissions = action.GetAttributeValue("permissions", Permissions);
            UIElements = action.GetAttributeValue("uielements", UIElements);
            HideIfNotAuthorized = action.GetAttributeValue("hideifnotauthorized", HideIfNotAuthorized);
        }

        #endregion
        

        /// <summary>
        /// Check whether authenticated user has given permissions for given resource on current site to perform this action.
        /// </summary>
        internal bool CheckPermissionsAuthorization()
        {
            if (string.IsNullOrEmpty(Permissions) || string.IsNullOrEmpty(ModuleName))
            {
                return true;
            }

            var user = MembershipContext.AuthenticatedUser;
            var currentSite = SiteContext.CurrentSiteName;

            var permissions = Permissions.Split(';');
            foreach (var permission in permissions)
            {
                if (!user.IsAuthorizedPerResource(ModuleName, permission, currentSite))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Check whether authenticated user is authorized for given UI elements on current site to perform this action.
        /// </summary>
        internal bool CheckUIElementsAuthorization()
        {
            if (string.IsNullOrEmpty(UIElements) || string.IsNullOrEmpty(ModuleName))
            {
                return true;
            }

            return MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement(ModuleName, UIElements.Split(';'), SiteContext.CurrentSiteName);
        }
    }
}