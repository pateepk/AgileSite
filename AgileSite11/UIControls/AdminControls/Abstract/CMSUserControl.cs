using System;
using System.Web;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for CMS administration interface control.
    /// </summary>
    public abstract class CMSUserControl : InlineUserControl
    {
        #region "Variables"

        // General access denied page
        private string mAccessDeniedPage = AdministrationUrlHelper.ACCESSDENIED_PAGE;

        // Administration access denied page
        private string mCMSDeskAccessDeniedPage = AdministrationUrlHelper.ADMIN_ACCESSDENIED_PAGE;

        // Relative path
        private string mRelativePath;

        // Indicates whether CMSDesk access denied page should be used
        private bool? mUseCMSDeskAccessDeniedPage;

        private MacroResolver mCurrentResolver;
        private UIContext mUIContext;

        #endregion


        #region "Properties"

        /// <summary>
        /// Control's UI Context
        /// </summary>
        public UIContext UIContext
        {
            get
            {
                return mUIContext ?? (mUIContext = UIContextHelper.GetUIContext(this));
            }
        }


        /// <summary>
        /// Control's edited object
        /// </summary>
        public object EditedObject
        {
            get
            {
                return UIContext.EditedObject;
            }
            set
            {
                UIContext.EditedObject = value;
            }
        }


        /// <summary>
        /// Returns the object type name.
        /// </summary>
        [Obsolete("Use GetType().Name instead.")]
        public string TypeName
        {
            get
            {
                return GetType().Name;
            }
        }


        /// <summary>
        /// Control's resolver
        /// </summary>
        public MacroResolver CurrentResolver
        {
            get
            {
                if (mCurrentResolver == null)
                {
                    mCurrentResolver = MacroResolver.GetInstance();
                }

                return mCurrentResolver;
            }
            set
            {
                mCurrentResolver = value;
            }
        }


        /// <summary>
        /// Gets or sets display mode of the control.
        /// </summary>
        public virtual ControlDisplayModeEnum DisplayMode
        {
            get;
            set;
        } = ControlDisplayModeEnum.Default;


        /// <summary>
        /// Page relative path.
        /// </summary>
        [Obsolete("Use System.Web.UI.TemplateControl.AppRelativeVirtualPath.TrimStart('~') instead.")]
        public string RelativePath
        {
            get
            {
                return mRelativePath ?? (mRelativePath = AppRelativeVirtualPath.TrimStart('~'));
            }
        }


        /// <summary>
        /// Control context.
        /// </summary>
        public virtual string ControlContext
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether CMSDesk access denied page should be used.
        /// </summary>
        public bool UseCMSDeskAccessDeniedPage
        {
            get
            {
                if (mUseCMSDeskAccessDeniedPage == null)
                {
                    mUseCMSDeskAccessDeniedPage = false;

                    var ctrl = Parent;
                    while (ctrl != null)
                    {
                        if (ctrl is CMSDeskPage)
                        {
                            mUseCMSDeskAccessDeniedPage = true;
                            break;
                        }

                        ctrl = ctrl.Parent;
                    }
                }

                return mUseCMSDeskAccessDeniedPage.Value;
            }
            set
            {
                mUseCMSDeskAccessDeniedPage = value;
            }
        }


        /// <summary>
        /// Determines whether the current control lies on the page that is currently located under the CMS Desk.
        /// </summary>
        public bool IsCMSDesk
        {
            get
            {
                CMSPage cmsPage = Page as CMSPage;
                if (cmsPage != null)
                {
                    return (cmsPage).IsCMSDesk;
                }
                return false;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns access denied page with dependence on current setting.
        /// </summary>
        protected string GetAccessDeniedPageUrl()
        {
            if (UseCMSDeskAccessDeniedPage)
            {
                return mCMSDeskAccessDeniedPage;
            }

            return mAccessDeniedPage;
        }


        /// <summary>
        /// Redirects the user to Access denied page.
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="elements">UI element names that failed</param>
        protected void RedirectToUIElementAccessDenied(string resourceName, string elements)
        {
            var url = AdministrationUrlHelper.GetAccessDeniedUrl(resourceName, null, elements, null, GetAccessDeniedPageUrl());
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Redirects the user to Access denied page.
        /// </summary>
        /// <param name="resourceName">Resource name that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        protected void RedirectToAccessDenied(string resourceName, string permissionName)
        {
            var url = AdministrationUrlHelper.GetAccessDeniedUrl(resourceName, permissionName, null, null, GetAccessDeniedPageUrl());
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Redirects the user to Access denied page.
        /// </summary>
        /// <param name="nodeId">Node ID that failed</param>
        /// <param name="permissionName">Permission name that failed</param>
        protected void RedirectToAccessDenied(int nodeId, string permissionName)
        {
            var url = String.Format("{0}?nodeid={1}&permission={2}", GetAccessDeniedPageUrl(), nodeId.ToString(), HttpUtility.UrlEncode(permissionName));
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url));

            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Redirects the user to Access denied page.
        /// </summary>
        /// <param name="message">Displayed access denied message</param>
        public void RedirectToAccessDenied(string message)
        {
            var url = AdministrationUrlHelper.GetAccessDeniedUrl(null, null, null, message, GetAccessDeniedPageUrl());
            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Redirects the user to the info page which says that the UI of the requested page is not available.
        /// </summary>        
        public static void RedirectToUINotAvailable()
        {
            RedirectToInformation("uiprofile.uinotavailable");
        }


        /// <summary>
        /// Redirects the user to the info page which displays specified message.
        /// </summary>     
        /// <param name="message">Message which should be displayed</param>
        public static void RedirectToInformation(string message)
        {
            URLHelper.Redirect(AdministrationUrlHelper.GetInformationUrl(message));
        }


        /// <summary>
        /// Sets the control context.
        /// </summary>
        public void SetContext()
        {
            if (ControlContext != null)
            {
                DebugHelper.SetContext(ControlContext);
            }
        }


        /// <summary>
        /// Releases the control context.
        /// </summary>
        public void ReleaseContext()
        {
            if (ControlContext != null)
            {
                DebugHelper.ReleaseContext();
            }
        }

        #endregion
    }
}