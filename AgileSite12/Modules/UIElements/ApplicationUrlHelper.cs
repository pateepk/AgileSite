using System;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Modules
{
    /// <summary>
    /// Provides functionality for working with application URL.
    /// </summary>
    public class ApplicationUrlHelper
    {

        /// <summary>
        /// Basic UI Element Page path
        /// </summary>
        public const String DEFAULT_UI_PAGE_PATH = "~/CMSModules/AdminControls/Pages/UIPage.aspx";


        /// <summary>
        /// Blank page path for controls
        /// </summary>
        public const String DEFAULT_UI_CONTROL_PAGE_PATH = "~/CMSModules/AdminControls/Pages/UIElementControlPage.aspx";


        /// <summary>
        /// Get the direct link for page identified by given node ID.
        /// </summary>
        /// <param name="nodeId">Node ID.</param>
        public static string GetPageEditLink(int nodeId)
        {
            UIElementInfo detailElement = UIElementInfoProvider.GetUIElementInfo("CMS.Content", "Edit");

            if (detailElement == null)
            {
                throw new ArgumentException("[UIContextHelper.GetPageEditLink]: Element with name 'Edit' does not exist under 'CMS.Content' module.");
            }

            return GetApplicationUrl(detailElement.Application, "nodeid=" + nodeId);
        }


        /// <summary>
        /// Get the direct link for page identified by given node ID.
        /// </summary>
        /// <param name="nodeId">Node ID.</param>
        /// <param name="culture">Culture name.</param>
        public static string GetPageEditLink(int nodeId, string culture)
        {
            UIElementInfo detailElement = UIElementInfoProvider.GetUIElementInfo("CMS.Content", "Edit");

            if (detailElement == null)
            {
                throw new ArgumentException("[UIContextHelper.GetPageEditLink]: Element with name 'Edit' does not exist under 'CMS.Content' module.");
            }

            return GetApplicationUrl(detailElement.Application, $"nodeid={nodeId}&culture={culture}");
        }



        /// <summary>
        /// Returns application URL for application specified by module name and element name
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        /// <param name="customQueryString">Custom querystring value</param>
        public static string GetApplicationUrl(String moduleName, String elementName, string customQueryString = null)
        {
            UIElementInfo ui = UIElementInfoProvider.GetUIElementInfo(moduleName, elementName);
            return GetApplicationUrl(ui, customQueryString);
        }


        /// <summary>
        /// Returns application URL for application specified by UI element object
        /// </summary>
        /// <param name="ui">UI element info</param>
        /// <param name="customQueryString">Custom querystring value</param>
        public static string GetApplicationUrl(UIElementInfo ui, string customQueryString = null)
        {
            string applicationId = GetApplicationHash(ui);

            // Ensures custom querystring
            if (!String.IsNullOrEmpty(customQueryString) && !customQueryString.StartsWithCSafe("?"))
            {
                customQueryString = "?" + customQueryString;
            }

            // Combine final path
            return "~/Admin/cmsadministration.aspx" + customQueryString + applicationId;
        }


        /// <summary>
        /// Gets the application hash (starting with #)
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        public static string GetApplicationHash(String moduleName, String elementName)
        {
            UIElementInfo ui = UIElementInfoProvider.GetUIElementInfo(moduleName, elementName);
            return GetApplicationHash(ui);
        }


        /// <summary>
        /// Gets the application hash (starting with #)
        /// </summary>
        /// <param name="ui">UI element info</param>
        public static string GetApplicationHash(UIElementInfo ui)
        {
            if (ui != null)
            {
                return "#" + ui.ElementGUID;
            }
            return String.Empty;
        }


        /// <summary>
        /// Returns dialog UI element URL, appends automatically Dialog and Hash parameters.
        /// </summary>
        /// <param name="ui">Target element</param>
        /// <param name="objectId">Object ID to append to query</param>
        /// <param name="additionalQuery">Additional query for URL</param>
        public static string GetElementDialogUrl(UIElementInfo ui, int objectId = 0, String additionalQuery = null)
        {
            if (ui != null)
            {
                String url = ui.ElementTargetURL;
                switch (ui.ElementType)
                {
                    case UIElementTypeEnum.UserControl:
                        url = String.Format("{0}?elementguid={1}", DEFAULT_UI_CONTROL_PAGE_PATH, ui.ElementGUID);
                        break;

                    case UIElementTypeEnum.PageTemplate:
                        url = String.Format("{0}?elementguid={1}", DEFAULT_UI_PAGE_PATH, ui.ElementGUID);
                        break;
                }

                // Append additional query
                url = URLHelper.AppendQuery(url, additionalQuery);

                // Append IsDialog
                url = URLHelper.AddParameterToUrl(url, "dialog", "true");

                // Append hash dialog
                url = AppendDialogHash(url);

                // Append object ID, object ID is not included in hash
                if (objectId > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "objectid", objectId.ToString());
                }

                return ResolveDialogUrl(url);
            }

            return AdministrationUrlHelper.GetInformationUrl(ResHelper.GetString("uielement.elementobjectnotfound"));
        }


        /// <summary>
        /// Appends hash to URL. Used in selectors (no need for dialog mode).
        /// </summary>
        /// <param name="url">URL to hash is added</param>
        public static string AppendDialogHash(String url)
        {
            string trimmed = URLHelper.GetQuery(URLHelper.UrlEncodeQueryString(url));
            trimmed = URLHelper.RemoveUrlParameters(trimmed, "objectid", "hash");
            return URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(trimmed));
        }


        /// <summary>
        /// Returns basic UIPage with no parameters. Use only in special cases, in most cases use GetElementUrl with parameters.
        /// </summary>
        public static string GetElementUrl()
        {
            return URLHelper.ResolveUrl(DEFAULT_UI_PAGE_PATH);
        }


        /// <summary>
        /// Resolves the UI URL. Keeps the URL in case the user is authenticated, uses the secured /cms/ prefix to ensure the authentication for already authenticated user.
        /// </summary>
        /// <param name="url">URL to resolve, it must start with ~/</param>
        /// <param name="prefix">URL prefix</param>
        private static string ResolveUIUrl(string url, string prefix)
        {
            var authenticatedUser = Service.Resolve<IAuthenticationService>().CurrentUser;
            if (authenticatedUser.IsPublic())
            {
                return URLHelper.ResolveUrl(url);
            }

            // Add the subpath to the URL
            if (url.StartsWithCSafe("~/"))
            {
                if (!String.IsNullOrEmpty(prefix))
                {
                    prefix = "/" + prefix;
                }
                url = "~/cms" + prefix + url.Substring(1);
            }
            else
            {
                throw new Exception("The URL must be relative to the application and start with \"~/\".");
            }

            return URLHelper.ResolveUrl(url);
        }


        /// <summary>
        /// Resolves the UI URL. Keeps the URL in case the user is authenticated, uses the secured /cms/ prefix to ensure the authentication for already authenticated user.
        /// </summary>
        /// <param name="url">URL to resolve, it must start with ~/</param>
        public static string ResolveUIUrl(string url)
        {
            return ResolveUIUrl(url, null);
        }


        /// <summary>
        /// Resolves the dialog URL. Keeps the URL in case the user is authenticated, uses the secured /cms/dialogs/ prefix to ensure the authentication for already authenticated user.
        /// </summary>
        /// <param name="url">URL to resolve, it must start with ~/</param>
        public static string ResolveDialogUrl(string url)
        {
            return ResolveUIUrl(url, "dialogs");
        }


        /// <summary>
        /// Returns dialog UI element URL, appends automatically Dialog and Hash parameters.
        /// </summary>
        /// <param name="moduleName">Element's module name</param>
        /// <param name="elementName">Element's name</param>
        /// <param name="objectId">Object ID to append to query</param>
        /// <param name="additionalQuery">Additional query for URL</param>
        public static string GetElementDialogUrl(String moduleName, String elementName, int objectId = 0, String additionalQuery = null)
        {
            UIElementInfo ui = UIElementInfoProvider.GetUIElementInfo(moduleName, elementName);
            return GetElementDialogUrl(ui, objectId, additionalQuery);
        }


        /// <summary>
        /// Returns resource name for given resource ID.
        /// </summary>
        /// <param name="resourceID">Resource ID</param>
        public static String GetResourceName(int resourceID)
        {
            ResourceInfo ri = ResourceInfoProvider.GetResourceInfo(resourceID);
            if (ri != null)
            {
                return ri.ResourceName;
            }

            return String.Empty;
        }

        #region "Element URL methods"

        /// <summary>
        /// Returns element's URL based on elements type
        /// </summary>
        /// <param name="ui">UI element info object</param>
        public static string GetElementUrl(UIElementInfo ui)
        {
            if (ui == null)
            {
                return URLHelper.ResolveUrl(AdministrationUrlHelper.GetInformationUrl(ResHelper.GetString("uielement.elementobjectnotfound")));
            }

            var url = ui.ElementTargetURL;

            var ri = ResourceInfoProvider.GetResourceInfo(ui.ElementResourceID);
            if (ri == null)
            {
                return url;
            }

            switch (ui.ElementType)
            {
                case UIElementTypeEnum.UserControl:
                    return URLHelper.ResolveUrl(String.Format("{0}?elementguid={1}", DEFAULT_UI_CONTROL_PAGE_PATH, ui.ElementGUID));

                case UIElementTypeEnum.PageTemplate:
                    return URLHelper.ResolveUrl(String.Format("{0}?elementguid={1}", DEFAULT_UI_PAGE_PATH, ui.ElementGUID));

                case UIElementTypeEnum.Javascript:
                    return url;

                case UIElementTypeEnum.Url:
                {
                    // If element is without content and only groups other UIElements in menu or tree
                    if (url == COLLAPSIBLE_EMPTY_PARENT_ELEMENT_URL)
                    {
                        return url;
                    }

                    // Resolve macros
                    if (MacroProcessor.ContainsMacro(url))
                    {
                        url = MacroResolver.Resolve(url);
                    }

                    return URLHelper.ResolveUrl(url);
                }
            }

            return URLHelper.ResolveUrl(AdministrationUrlHelper.GetInformationUrl(ResHelper.GetString("uielement.elementobjectnotfound")));
        }


        /// <summary>
        /// Returns element's URL based on element's type
        /// </summary>
        /// <param name="moduleName">Element's module</param>
        /// <param name="elementName">Element's name</param>
        public static string GetElementUrl(String moduleName, String elementName)
        {
            var ui = UIElementInfoProvider.GetUIElementInfo(moduleName, elementName);

            return GetElementUrl(ui);
        }


        /// <summary>
        /// Returns element's URL
        /// </summary>
        /// <param name="ui">UI element info.</param>
        /// <param name="displayTitle">Indicates whether append display title to result URL.</param>
        /// <param name="objectId">Indicates whether append object ID to URL.</param>
        /// <param name="additionalQuery">Additional query to append to URL</param>
        public static string GetElementUrl(UIElementInfo ui, bool displayTitle, int objectId = 0, string additionalQuery = null)
        {
            String url = GetElementUrl(ui);

            url = URLHelper.UpdateParameterInUrl(url, "displaytitle", displayTitle.ToString());

            if (objectId != 0)
            {
                url = URLHelper.UpdateParameterInUrl(url, "objectId", objectId.ToString());
            }

            if (!String.IsNullOrEmpty(additionalQuery))
            {
                url = URLHelper.AppendQuery(url, additionalQuery);
            }

            return url;
        }


        /// <summary>
        /// Returns element's URL
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="elementName">Element name</param>
        /// <param name="displayTitle">Indicates whether append display title to result URL.</param>
        /// <param name="objectId">Indicates whether append object ID to URL.</param>
        /// <param name="additionalQuery">Additional query to append to URL</param>
        public static string GetElementUrl(String moduleName, String elementName, bool displayTitle, int objectId = 0, string additionalQuery = null)
        {
            return GetElementUrl(UIElementInfoProvider.GetUIElementInfo(moduleName, elementName), displayTitle, objectId, additionalQuery);
        }

        #endregion


        /// <summary>
        /// Represents <see cref="UIElementInfo.ElementTargetURL"/> of a dummy <see cref="UIElementInfo"/> of type <see cref="UIElementTypeEnum.Url"/>.
        /// This kind of element is used as parent node in collapsible tree without any content.
        /// It's used for grouping <see cref="UIElementInfo"/>s in a tree.
        /// </summary>
        public const string COLLAPSIBLE_EMPTY_PARENT_ELEMENT_URL = "@";
    }
}
