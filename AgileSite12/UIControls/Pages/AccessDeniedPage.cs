using System;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page used for 'Access Denied' pages.
    /// </summary>
    public abstract class AccessDeniedPage : MessagePage
    {
        #region "Variables"

        private string mUIElementName;
        private ResourceInfo mResourceInfo;
        private string mResourceString;
        private string mResourceName;
        private string mMessage;
        private string mPermissionName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Resource string to display
        /// </summary>
        protected string ResourceString
        {
            get
            {
                if (mResourceString == null)
                {
                    mResourceString = QueryHelper.GetText("resstring", null);
                }

                return mResourceString;
            }
        }


        /// <summary>
        /// Resource which is denied.
        /// </summary>
        protected string ResourceName
        {
            get
            {
                if (mResourceName == null)
                {
                    mResourceName = QueryHelper.GetText("resource", null);
                }

                return mResourceName;
            }
        }


        /// <summary>
        /// Resource info object retrieved by ResourceName.
        /// </summary>
        protected ResourceInfo Resource
        {
            get
            {
                if (mResourceInfo == null)
                {
                    mResourceInfo = ResourceInfoProvider.GetResourceInfo(ResourceName);
                }

                return mResourceInfo;
            }
        }


        /// <summary>
        /// Custom message (resource string) defined in query string.
        /// </summary>
        protected string Message
        {
            get
            {
                if (mMessage == null)
                {
                    mMessage = QueryHelper.GetText("message", null);
                }

                return mMessage;
            }
        }


        /// <summary>
        /// Name of missing permission.
        /// </summary>
        protected string PermissionName
        {
            get
            {
                if (mPermissionName == null)
                {
                    mPermissionName = QueryHelper.GetText("permission", null);
                }

                return mPermissionName;
            }
        }


        /// <summary>
        /// Display name of missing UI element.
        /// </summary>
        protected string UIElementDisplayName
        {
            get
            {
                if (mUIElementName == null)
                {
                    string uiElementName = QueryHelper.GetText("uielement", string.Empty);
                    UIElementInfo uiElementInfo = UIElementInfoProvider.GetUIElementInfo(ResourceName, uiElementName);

                    if (uiElementInfo != null)
                    {
                        uiElementName = uiElementInfo.ElementDisplayName;
                    }

                    mUIElementName = ResHelper.LocalizeString(uiElementName);
                }

                return mUIElementName;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            // Try skip IIS HTTP errors
            Response.TrySkipIisCustomErrors = true;
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // Set forbidden state
            Response.StatusCode = 403;

            base.OnLoad(e);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Performs sign out of current user
        /// </summary>
        protected virtual void PerformSignOut()
        {
            if (!MembershipContext.SignOutPending)
            {
                AuthenticationHelper.SignOut();
            }
        }


        /// <summary>
        /// Gets the message of the access denied error
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="title">Page title</param>
        protected bool GetTexts(ref string message, ref string title)
        {
            bool hideLinks = false;

            // Validate the hash to ensure integrity of the "access denied" message
            var hashSettings = new HashSettings("") { Redirect = false };

            if (!QueryHelper.ValidateHash("hash", "requestguid;dialog", hashSettings))
            {
                message = GetString("general.accessdenied");
                title = GetString("general.accessdenied");
                return true;
            }

            if (!String.IsNullOrEmpty(ResourceName))
            {
                switch (ResourceName.ToLowerCSafe())
                {
                    // Not enabled admin interface
                    case "cms.adminui":
                        message = GetString("general.AdminUINotEnabled");
                        break;

                    default:
                        // Access denied to resource
                        if (Resource != null)
                        {
                            title = String.Format(GetString("general.accessdeniedonresource"), HTMLHelper.HTMLEncode(Resource.ResourceDisplayName));
                        }
                        break;
                }
            }
            else if (NodeID > 0)
            {
                // Access denied to document
                TreeNode node = Tree.SelectSingleNode(NodeID);
                if (node != null)
                {
                    title = String.Format(GetString("general.accessdeniedonnode"), HTMLHelper.HTMLEncode(node.GetDocumentName()));
                }
            }
            else if (!String.IsNullOrEmpty(ResourceString))
            {
                // Resource string
                message = ResHelper.GetString(ResourceString);
            }
            else if (!String.IsNullOrEmpty(Message))
            {
                // Custom message
                message = ResHelper.LocalizeString(Message);
                hideLinks = true;
            }

            // Add missing permission name message
            if (!String.IsNullOrEmpty(PermissionName))
            {
                string permMessage = PermissionName;
                if (PermissionName.Contains("|"))
                {
                    permMessage = String.Join(GetString("general.AccessDeniedOr"), PermissionName.Split('|'));
                }

                message = String.Format(GetString("general.accessdeniedonpermissionname"), permMessage);
                hideLinks = true;
            }
            // Add missing UI element name message
            else if (!String.IsNullOrEmpty(UIElementDisplayName))
            {
                string elemMessage = UIElementDisplayName;
                if (UIElementDisplayName.Contains("|"))
                {
                    elemMessage = String.Join(GetString("general.AccessDeniedOr"), UIElementDisplayName.Split('|'));
                }

                message = String.Format(GetString("general.AccessDeniedOnUIElementName"), HTMLHelper.HTMLEncode(elemMessage));
                hideLinks = true;
            }

            return hideLinks;
        }

        #endregion
    }
}
