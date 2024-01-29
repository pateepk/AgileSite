using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;
using CMS.URLRewritingEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Helper methods for document management UI
    /// </summary>
    public class DocumentUIHelper
    {
        #region "Variables"

        private const string CONTENT_CMSDESK_FOLDER = "~/CMSModules/Content/CMSDesk/";
        private static ISet<string> mContentOnlyHiddenUIElements;

        #endregion


        #region "Properties"

        /// <summary>
        /// Set of UI elements which should not be visible for content only class.
        /// </summary>
        private static ISet<string> ContentOnlyHiddenUIElements
        {
            get
            {
                return mContentOnlyHiddenUIElements ?? (mContentOnlyHiddenUIElements = GetHiddenContentOnlyElements());
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the set of UI elements which should not be visible for content only class.
        /// </summary>
        private static ISet<string> GetHiddenContentOnlyElements()
        {
            return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "onlinemarketing.abtests",
                "onlinemarketing.mvtests",
                "onlinemarketing.mvtvariants",
                "masterpage",
                "page",
                "design",
                "properties.template",
                "analytics"
            };
        }


        /// <summary>
        /// Returns display name of UI element. Handles display name exceptions for content only pages.
        /// </summary>
        /// <param name="element">UI element for which alternative display name should be found</param>
        /// <param name="node">Node displayed in UI</param>
        public static string GetUIElementDisplayName(UIElementInfo element, TreeNode node)
        {
            if (element == null)
            {
                return String.Empty;
            }

            string elementDisplayName = null;

            // Check if content only version of display name is required
            if ((node != null) && (node.NodeIsContentOnly || node.Site.SiteIsContentOnly))
            {
                // Replace the display name for the following UI elements
                switch (element.ElementName.ToLowerCSafe())
                {
                    case "properties.urls":
                        elementDisplayName = ResHelper.GetString("content.ui.propertiespagealias");
                        break;

                    case "properties.menu":
                        elementDisplayName = ResHelper.GetString("content.ui.propertiessearch");
                        break;

                    case "editform":
                        elementDisplayName = ResHelper.GetString("content.ui.content");
                        break;
                }
            }

            if (String.IsNullOrEmpty(elementDisplayName))
            {
                // Get the original element's display name
                // Display name can contain a localization macro in format {$xx.yy.zz$}
                elementDisplayName = ResHelper.LocalizeString(element.ElementDisplayName);
            }

            return elementDisplayName;
        }


        /// <summary>
        /// Gets link to edit document in Administration.
        /// </summary>
        /// <param name="nodeId">NodeID</param>
        /// <param name="cultureCode">Culture code</param>
        public static string GetDocumentEditUrl(int nodeId, string cultureCode)
        {
            string url = "~/Admin/cmsadministration.aspx";

            url = URLHelper.AddParameterToUrl(url, "action", "edit");
            url = URLHelper.AddParameterToUrl(url, "nodeid", nodeId.ToString());
            url = URLHelper.AddParameterToUrl(url, "culture", cultureCode);

            url += ApplicationUrlHelper.GetApplicationHash("cms.content", "content");

            return url;
        }


        /// <summary>
        /// Checks document permissions
        /// </summary>
        /// <param name="node">Document to check</param>
        /// <param name="permission">Permission to check</param>
        public static bool CheckDocumentPermissions(TreeNode node, PermissionsEnum permission)
        {
            if (node != null)
            {
                return node.CheckPermissions(permission, node.NodeSiteName, MembershipContext.AuthenticatedUser);
            }

            return false;
        }


        /// <summary>
        /// Ensures current document breadcrumbs (non-clickable)
        /// </summary>
        /// <param name="breadcrumbs">Breadcrumbs control</param>
        /// <param name="node">Current document. If not provided, action text is used instead.</param>
        /// <param name="action">Name of the action</param>
        /// <param name="rootUrl">Root URL</param>
        /// <param name="typeCodeName">Object type</param>
        public static void EnsureDocumentBreadcrumbs(Breadcrumbs breadcrumbs, TreeNode node, string action, string rootUrl, string typeCodeName = null)
        {
            // Ensure root URL
            if (String.IsNullOrEmpty(rootUrl))
            {
                rootUrl = CONTENT_CMSDESK_FOLDER + "default.aspx";
            }

            if (node == null)
            {
                breadcrumbs.AddBreadcrumb(new BreadcrumbItem
                {
                    Text = ResHelper.GetString("app.pages"),
                    Target = "_parent",
                    RedirectUrl = rootUrl
                });
                breadcrumbs.AddBreadcrumb(new BreadcrumbItem
                {
                    Text = action
                });
            }
            else
            {
                breadcrumbs.AddBreadcrumb(new BreadcrumbItem
                {
                    Text = node.Site.Generalized.ObjectDisplayName,
                    Target = "_parent",
                    RedirectUrl = rootUrl
                });

                breadcrumbs.AddBreadcrumb(new BreadcrumbItem
                {
                    Text = node.GetDocumentName()
                });
            }

            if (String.IsNullOrEmpty(typeCodeName))
            {
                typeCodeName = "content.ui.page";
            }

            string type = !String.IsNullOrEmpty(action) ? "" : ResHelper.GetString(typeCodeName, CultureHelper.PreferredUICultureCode);

            // Register breadcrumbsSuffix to client application for Breadcrumbs.js module
            UIHelper.SetBreadcrumbsSuffix(type);
        }


        /// <summary>
        /// Gets UI page page URL for document action.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        public static string GetDocumentActionPageUrl(UIPageURLSettings settings)
        {
            string result = null;

            string parameterPrefix = null;
            bool excludeIdentifiers = false;

            // Get appropriate URL for action
            switch (settings.Action.ToLowerCSafe())
            {
                case "new":
                    {
                        result = CONTENT_CMSDESK_FOLDER + "New/New.aspx";
                        parameterPrefix = "parent";
                    }
                    break;

                case "newvariant":
                    {
                        result = "~/CMSModules/OnlineMarketing/Pages/Content/ABTesting/ABVariant/NewPage.aspx";
                        parameterPrefix = "parent";
                    }
                    break;

                case "copy":
                case "move":
                case "linkdoc":
                    {
                        settings.ProtectUsingHash = true;
                        excludeIdentifiers = true;

                        DialogConfiguration config = GetDocumentDialogConfig(settings.Action, settings.Culture);

                        result = CMSDialogHelper.GetDialogUrl(config, false, false, null, false);
                        result = URLHelper.AddParameterToUrl(result, "sourcenodeids", settings.NodeID.ToString());
                    }
                    break;

                case "delete":
                    result = CONTENT_CMSDESK_FOLDER + "Delete.aspx";
                    break;

                case "drag":
                    result = CONTENT_CMSDESK_FOLDER + "DragOperation.aspx";
                    break;

                case "search":
                    result = CONTENT_CMSDESK_FOLDER + "Search/Default.aspx";
                    break;

                case "notallowed":
                    {
                        result = CONTENT_CMSDESK_FOLDER + "NotAllowed.aspx";
                        result = URLHelper.AddParameterToUrl(result, "action", QueryHelper.GetString("subaction", String.Empty).ToLowerCSafe());
                    }
                    break;

                default:
                    if (!String.IsNullOrEmpty(settings.Action))
                    {
                        result = UIContextHelper.GetElementUrl("CMS.Content", "Edit");

                        if (settings.Action.EqualsCSafe("edit"))
                        {
                            settings.QueryParameters["tabname"] = "EditForm";
                        }

                        settings.QueryParameters["mode"] = settings.Mode;
                        settings.QueryParameters["displaytitle"] = "false";
                    }
                    break;
            }

            if (!String.IsNullOrEmpty(result))
            {
                if (!excludeIdentifiers && (settings.NodeID > 0))
                {
                    settings.QueryParameters[parameterPrefix + "nodeid"] = settings.NodeID.ToString();
                    if (!String.IsNullOrEmpty(settings.Culture))
                    {
                        settings.QueryParameters[parameterPrefix + "culture"] = settings.Culture;
                    }
                }

                result = FinalizeQueryString(result, settings);
                result = UrlResolver.ResolveUrl(result);
            }

            return result;
        }


        /// <summary>
        /// Gets UI page page URL.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        public static string GetDocumentPageUrl(UIPageURLSettings settings)
        {
            return GetDocumentPageUrl(settings, null);
        }


        /// <summary>
        /// Gets UI page page URL.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        /// <param name="getActionUrl">Function to retrieve the action page URL</param>
        public static string GetDocumentPageUrl(UIPageURLSettings settings, Func<UIPageURLSettings, string> getActionUrl)
        {
            try
            {
                // Don't allow split mode if Languages UI element with Compare button is hidden
                bool isSplitMode = PortalUIHelper.DisplaySplitMode && MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement(ModuleName.CONTENT, "Properties.Languages") && (PortalContext.ViewMode != ViewModeEnum.EditLive);
                string siteName = SiteContext.CurrentSiteName;
                bool siteCombineWithDefaultCulture = SiteInfoProvider.CombineWithDefaultCulture(siteName);
                bool combine = !isSplitMode && siteCombineWithDefaultCulture && !settings.TransformToCompareUrl;

                settings.Mode = settings.Mode.ToLowerCSafe();
                var tree = new TreeProvider();

                if (settings.Node == null)
                {
                    // Get appropriate node
                    switch (settings.Mode)
                    {
                        case "livesite":
                        case "preview":
                            settings.Node = tree.SelectSingleNode(settings.NodeID, settings.Culture, combine);
                            break;

                        case "listing":
                            // Get node of any culture (listing can be displayed even for non-existent culture versions)
                            settings.Node = tree.SelectSingleNode(settings.NodeID, TreeProvider.ALL_CULTURES);
                            break;

                        default:
                            tree.CombineWithDefaultCulture &= !settings.TransformToCompareUrl;
                            if ((settings.NodeID) > 0 && !String.IsNullOrEmpty(settings.Culture))
                            {
                                settings.Node = tree.SelectSingleNode(settings.NodeID, settings.Culture, combine);
                            }
                            break;
                    }
                }

                if (settings.TransformToCompareUrl)
                {
                    PortalUIHelper.SplitModeCultureCode = settings.Culture;
                }
                else
                {
                    // Get action URL
                    if (getActionUrl == null)
                    {
                        getActionUrl = GetDocumentActionPageUrl;
                    }

                    string actionUrl = getActionUrl(settings);
                    if (actionUrl != null)
                    {
                        return actionUrl;
                    }
                }

                string url = null;
                bool splitViewSupported = false;

                // Get UI element name
                string elementName = null;
                String moduleName = ModuleName.CONTENT;
                switch (settings.Mode)
                {
                    case "edit":
                        elementName = "Page";
                        break;

                    case "design":
                        elementName = "Design";
                        moduleName = ModuleName.DESIGN;
                        break;
                }

                // Check UI elements for tabs only if mode is set (skip UI element check for actions, which don't have own UI element like NEW)
                if (!String.IsNullOrEmpty(elementName) && !MembershipContext.AuthenticatedUser.IsAuthorizedPerUIElement(moduleName, elementName))
                {
                    return UrlResolver.ResolveUrl(CMSPage.GetCMSDeskUIElementAccessDenied(ModuleName.CONTENT, elementName));
                }

                string documentName = null;

                if (settings.Node != null)
                {
                    documentName = settings.Node.DocumentName;

                    DataClassInfo ci = null;
                    if ((settings.Mode == "edit") || (settings.Mode == "editform") || (settings.Mode == "preview") || (settings.Mode == "listing"))
                    {
                        // Get data class only when needed
                        ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(settings.Node.GetIntegerValue("NodeClassID", 0));
                    }

                    bool showViewValidate;

                    switch (settings.Mode)
                    {
                        case "edit":
                            {
                                splitViewSupported = true;

                                // Get document type URL
                                if (ci != null)
                                {
                                    if (!String.IsNullOrEmpty(ci.ClassViewPageUrl))
                                    {
                                        EnsureDocumentIdentifiers(settings, true);
                                        url = ci.ClassViewPageUrl;
                                    }
                                    else if (String.IsNullOrEmpty(settings.PreferredURL))
                                    {
                                        EnsureLang(settings);
                                        url = URLRewriter.GetEditingUrl(settings.Node);
                                        url = URLHelper.AddParameterToUrl(url, "viewmode", ((int)ViewModeEnum.Edit).ToString());
                                    }

                                    // Show management panel
                                    url = URLHelper.AddParameterToUrl(url, "showpanel", "1");

                                    // Clear the editor widgets in the temporary interlayer for the first page load.
                                    url = URLHelper.AddParameterToUrl(url, "cmscontentchanged", "false");
                                }
                            }
                            break;

                        case "design":
                            {
                                EnsureLang(settings);

                                // Ensure correct URL
                                if (String.IsNullOrEmpty(settings.PreferredURL))
                                {
                                    url = URLRewriter.GetEditingUrl(settings.Node);
                                    url = URLHelper.AddParameterToUrl(url, "viewmode", ((int)ViewModeEnum.Design).ToString());

                                    // Show management panel
                                    url = URLHelper.AddParameterToUrl(url, "showpanel", "1");

                                    splitViewSupported = true;
                                }
                            }
                            break;

                        case "editform":
                            {
                                splitViewSupported = true;
                                bool ensureMode = false;

                                // Get document type URL
                                if (ci != null)
                                {
                                    // Check if view page URL is set
                                    if (!String.IsNullOrEmpty(ci.ClassEditingPageURL))
                                    {
                                        url = ci.ClassEditingPageURL;
                                        ensureMode = true;
                                    }
                                    else if (String.IsNullOrEmpty(settings.PreferredURL))
                                    {
                                        if (ci.ClassIsProduct && settings.Node.HasSKU && ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
                                        {
                                            url = "~/CMSModules/Ecommerce/Pages/Content/Product/Product_Edit_General.aspx";
                                        }
                                        else
                                        {
                                            url = CONTENT_CMSDESK_FOLDER + "Edit/edit.aspx";
                                        }
                                    }
                                }
                                EnsureDocumentIdentifiers(settings, ensureMode);
                            }
                            break;

                        case "preview":
                            {
                                // Get document type URL
                                if ((ci != null) && !String.IsNullOrEmpty(ci.ClassPreviewPageUrl))
                                {
                                    url = ci.ClassPreviewPageUrl;
                                }

                                if (String.IsNullOrEmpty(url))
                                {
                                    if (settings.Node.NodeIsContentOnly)
                                    {
                                        // Use preview link for content only pages
                                        url = settings.Node.GetPreviewLink(MembershipContext.AuthenticatedUser.UserName);
                                    }
                                    else
                                    {
                                        // Use permanent URL to get proper preview mode
                                        url = URLRewriter.GetEditingUrl(settings.Node);
                                        EnsureLang(settings);
                                        url = URLHelper.AddParameterToUrl(url, "viewmode", ((int)ViewModeEnum.Preview).ToString());

                                        // Split mode
                                        if (isSplitMode)
                                        {
                                            url = URLHelper.AddParameterToUrl(url, "cmssplitmode", "1");
                                        }
                                    }
                                }
                                else
                                {
                                    EnsureDocumentIdentifiers(settings, true);

                                    // Add query string parameters to custom preview page url
                                    url = FinalizeQueryString(url, settings);
                                }

                                // Show the Edit menu panel only when not located in the split view (Split view generates its own Edit menu panel)
                                bool showPanel = String.IsNullOrEmpty(settings.SplitViewSourceURL);

                                showViewValidate = CheckValidationVisibility(settings);

                                if (showPanel && settings.AllowViewValidate)
                                {
                                    if (!showViewValidate)
                                    {
                                        settings.QueryParameters["hidevalidate"] = "1";
                                    }

                                    EnsureDocumentIdentifiers(settings, false);
                                    url = GetViewValidateUrl(url, settings);


                                    // Do not show management panel in the sub-frame
                                    url = URLHelper.AddParameterToUrl(url, "showpanel", "0");
                                }
                                else
                                {
                                    if (!showViewValidate)
                                    {
                                        // Show breadcrumbs if Validate tab is not visible
                                        url = URLHelper.AddParameterToUrl(url, "showbreadcrumbs", "1");
                                    }

                                    splitViewSupported = true;
                                }
                            }
                            break;

                        case "livesite":
                            {
                                EnsureLang(settings);

                                PageInfo pi = null;

                                if (settings.Node.IsRoot())
                                {
                                    url = URLRewriter.GetLiveUrl(settings.Node);

                                    // Get information for page located at default alias path of a site
                                    pi = PageInfoProvider.GetPageInfoForUrl(URLHelper.GetAbsoluteUrl(url), settings.Culture, null, siteCombineWithDefaultCulture, true, siteName);

                                    if (pi != null)
                                    {
                                        // Get appropriate node for further checks (IsPublished etc.)
                                        settings.Node = tree.SelectSingleNode(pi.SiteName, pi.NodeAliasPath, pi.DocumentCulture);
                                        if (settings.Node == null)
                                        {
                                            return UrlResolver.ResolveUrl(GetPageNotAvailable(isSplitMode ? "splitviewmissingculture" : "missingculture", false, documentName));
                                        }
                                    }
                                }

                                if (URLRewriter.PageNotFoundForNonPublished(siteName))
                                {
                                    if (settings.Node.IsRoot())
                                    {
                                        // Check whether document specified by default alias path is published or not
                                        // Perform the check only for root document when displaying of page not found is desired
                                        try
                                        {
                                            if (pi != null)
                                            {
                                                URLRewritingContext.CurrentPageInfoSource = pi.PageResult.PageSource;
                                            }

                                            URLRewriter.CheckPublishedState(ref pi, new SiteNameOnDemand { Value = siteName }, new ViewModeOnDemand { Value = ViewModeEnum.LiveSite });
                                            if (pi == null)
                                            {
                                                return UrlResolver.ResolveUrl(GetPageNotAvailable("notpublished", documentName));
                                            }
                                        }
                                        catch
                                        {
                                            return UrlResolver.ResolveUrl(GetPageNotAvailable("notpublished", documentName));
                                        }
                                    }
                                    else
                                    {
                                        // If node is not published and page not found should be shown for non-published documents
                                        if (!settings.Node.IsPublished)
                                        {
                                            // Last chance to get the document URL - try to find published document in default culture
                                            if (siteCombineWithDefaultCulture)
                                            {
                                                string defaultCulture = CultureHelper.GetDefaultCultureCode(siteName);

                                                TreeNode defCultNode = tree.SelectSingleNode(settings.NodeID, defaultCulture, false);

                                                // If there is an published document in default culture
                                                if ((defCultNode != null) && defCultNode.IsPublished)
                                                {
                                                    settings.Node = defCultNode;

                                                    // Do not use document URL path - preferred culture could be changed
                                                    url = URLRewriter.GetLiveUrl(defCultNode);
                                                }
                                            }
                                            if (url == null)
                                            {
                                                // Document is not published
                                                return UrlResolver.ResolveUrl(GetPageNotAvailable("notpublished", documentName));
                                            }
                                        }
                                    }
                                }

                                if (url == null)
                                {
                                    // Do not use document URL path - preferred culture could be changed
                                    url = URLRewriter.GetLiveUrl(settings.Node);
                                }
                                url = URLHelper.AddParameterToUrl(url, "viewmode", ((int)ViewModeEnum.LiveSite).ToString());
                                url = URLHelper.AddParameterToUrl(url, DeviceProfileInfoProvider.DEVICENAME_QUERY_PARAM, settings.DeviceProfile);

                                // Split mode
                                if (isSplitMode)
                                {
                                    url = URLHelper.AddParameterToUrl(url, "cmssplitmode", "1");
                                }

                                // Show management panel
                                url = URLHelper.AddParameterToUrl(url, "showpanel", "1");

                                // Load device profile from cookies
                                if (QueryHelper.GetBoolean(DeviceProfileInfoProvider.DEVICES_QUERY_PARAM, false))
                                {
                                    url = URLHelper.AddParameterToUrl(url, DeviceProfileInfoProvider.DEVICES_QUERY_PARAM, "1");
                                }

                                showViewValidate = CheckValidationVisibility(settings);
                                bool deviceProfilesEnabled = DeviceProfileInfoProvider.IsDeviceProfilesEnabled(siteName);
                                if ((showViewValidate || deviceProfilesEnabled) && settings.AllowViewValidate)
                                {
                                    if (!showViewValidate)
                                    {
                                        settings.QueryParameters["hidevalidate"] = "1";
                                    }

                                    EnsureDocumentIdentifiers(settings, false);

                                    url = GetViewValidateUrl(url, settings);
                                }
                                else
                                {
                                    splitViewSupported = settings.Node.IsPublished;
                                }
                            }
                            break;

                        case "listing":
                            {
                                // Listing mode
                                url = CONTENT_CMSDESK_FOLDER + "View/listing.aspx";
                                bool ensureMode = false;
                                // Get document type URL
                                if ((ci != null) && !String.IsNullOrEmpty(ci.ClassListPageURL))
                                {
                                    url = ci.ClassListPageURL;
                                    ensureMode = true;
                                }

                                EnsureDocumentIdentifiers(settings, ensureMode);
                            }
                            break;

                        default:
                            {
                                splitViewSupported = (settings.Mode == "masterpage");

                                // Take original URL, and add document identifiers
                                url = settings.PreferredURL;
                                EnsureDocumentIdentifiers(settings, false);
                            }
                            break;
                    }

                    if (settings.TransformToCompareUrl && (url == null))
                    {
                        url = settings.SplitViewSourceURL;
                    }
                }
                else
                {
                    switch (settings.Mode)
                    {
                        case "preview":
                        case "livesite":
                            // Check (and ensure) the proper content culture
                            settings.CheckPreferredCulture = true;
                            return UrlResolver.ResolveUrl(GetPageNotAvailable(isSplitMode ? "splitviewmissingculture" : "missingculture", false, documentName));

                        default:
                            {
                                EnsureDocumentIdentifiers(settings, false);
                                url = UrlResolver.ResolveUrl(GetNewCultureVersionPageUrl(false));
                            }
                            break;
                    }
                }

                // Ensure view mode and query string parameters
                if (!String.IsNullOrEmpty(url))
                {
                    settings.AppendCurrentQuery = true;
                    url = FinalizeQueryString(url, settings);

                    if (settings.TransformToCompareUrl)
                    {
                        if (url == null)
                        {
                            url = settings.SplitViewSourceURL;
                        }
                        return TransformToCompareUrl(url);
                    }
                    else
                    {
                        if (isSplitMode)
                        {
                            // Current URL doesn't have to support splitview but one of its possible 
                            // child frames could. For that case, append node identifier.
                            if (settings.Node != null)
                            {
                                url = URLHelper.UpdateParameterInUrl(url, "nodeid", settings.NodeID.ToString());
                                url = URLHelper.UpdateParameterInUrl(url, "mode", settings.Mode);
                            }
                            // Split mode enabled
                            if (splitViewSupported)
                            {
                                url = GetSplitViewUrl(url); //URLHelper.AppendQuery(GetSplitViewUrl(url), URLHelper.Url.Query);
                            }
                        }
                    }

                    // And finally resolve the URL
                    url = UrlResolver.ResolveUrl(url);

                    return url;
                }

                return settings.PreferredURL;
            }
            finally
            {
                if (settings.CheckPreferredCulture && !CMSPage.CheckPreferredCulture())
                {
                    CMSPage.RefreshParentWindow();
                }
            }
        }


        /// <summary>
        /// Appends additional query parameters to URL.
        /// </summary>
        /// <param name="url">Original URL</param>
        /// <param name="settings">Settings object to take parameters from</param>
        /// <returns>URL with all requested parameters</returns>
        private static string FinalizeQueryString(string url, UIPageURLSettings settings)
        {
            // Get query part from URL
            string originalQuery = URLHelper.GetQuery(url);

            string resultQuery = originalQuery;

            // Merge original query string with current to ensure backward compatibility
            if (settings.AppendCurrentQuery)
            {
                /*** Following line can be uncommented when custom query parameters are being passed ***/
                //resultQuery = URLHelper.MergeQueryStrings(resultQuery, URLHelper.Url.Query, false); 
            }
            resultQuery = URLHelper.MergeQueryStrings(resultQuery, settings.AdditionalQuery, false);

            if (settings.QueryParameters != null)
            {
                if (!resultQuery.StartsWith("?", StringComparison.Ordinal))
                {
                    resultQuery = "?" + resultQuery;
                }
                // Ensure requested query parameters
                resultQuery = settings.QueryParameters.Keys.Cast<string>().Aggregate(resultQuery, (current, name) => URLHelper.UpdateParameterInUrl(current, name, settings.QueryParameters[name]));
            }

            // Remove empty query entries
            resultQuery = RemoveEmptyQueryParameters(resultQuery);

            // Remove original query and append result query to the URL
            url = URLHelper.RemoveQuery(url);
            url = URLHelper.AppendQuery(url, resultQuery);
            if (settings.ProtectUsingHash)
            {
                url = URLHelper.RemoveParameterFromUrl(url, "hash");
                // Append hash created upon URL without hash parameter
                url = URLHelper.UpdateParameterInUrl(url, "hash", QueryHelper.GetHash(url));
            }
            return url;
        }


        /// <summary>
        /// Removes empty query parameters. (eg. '?param=' -> '')
        /// </summary>
        /// <param name="query">Query string to clean from empty parameters</param>
        /// <returns>Query string with no empty parameters</returns>
        private static string RemoveEmptyQueryParameters(string query)
        {
            string result = null;
            NameValueCollection parameters = HttpUtility.ParseQueryString(query);
            foreach (string param in parameters)
            {
                if (!string.IsNullOrEmpty(parameters[param]))
                {
                    result = URLHelper.AddUrlParameter(result, param, HttpUtility.UrlEncode(parameters[param]));
                }
            }
            return result;
        }


        /// <summary>
        /// Creates DialogConfiguration object for specified document action.
        /// </summary>
        /// <param name="action">Action to create configuration for.</param>
        /// <param name="culture">Culture code</param>
        public static DialogConfiguration GetDocumentDialogConfig(string action, string culture)
        {
            return new DialogConfiguration
            {
                ContentSelectedSite = SiteContext.CurrentSiteName,
                OutputFormat = OutputFormatEnum.Custom,
                SelectableContent = SelectableContentEnum.AllContent,
                HideAttachments = false,
                CustomFormatCode = action.ToLowerCSafe(),
                Culture = culture
            };
        }


        /// <summary>
        /// Gets URL for new document language version page.
        /// </summary>
        /// <param name="appendCurrentQuery">If true, current query string is appended</param>
        public static string GetNewCultureVersionPageUrl(bool appendCurrentQuery = true)
        {
            string url = CONTENT_CMSDESK_FOLDER + "New/NewCultureVersion.aspx";
            if (appendCurrentQuery)
            {
                url += RequestContext.CurrentQueryString;
            }
            return url;
        }


        /// <summary>
        /// Gets URL for new document language version page.
        /// </summary>
        /// <param name="documentName">Document name</param>
        /// <param name="reason">Reason why the page is not available</param>
        public static string GetPageNotAvailable(string reason, string documentName)
        {
            return GetPageNotAvailable(reason, true, documentName);
        }


        /// <summary>
        /// Gets URL for new document language version page.
        /// </summary>
        /// <param name="showLink">Whether to show link for redirection to homepage</param>
        /// <param name="documentName">Document name</param>
        /// <param name="reason">Reason why the page is not available</param>
        public static string GetPageNotAvailable(string reason, bool showLink, string documentName)
        {
            string url = "~/CMSMessages/PageNotAvailable.aspx";

            url = URLHelper.AddParameterToUrl(url, "showlink", showLink.ToString().ToLowerCSafe());
            if (reason != null)
            {
                url = URLHelper.AddParameterToUrl(url, "reason", reason.ToLowerCSafe());
            }
            if (documentName != null)
            {
                url = URLHelper.AddParameterToUrl(url, "docname", documentName);
            }

            return url;
        }


        /// <summary>
        /// Ensures lang and langobjectlifetime parameters in given query collection.
        /// </summary>
        /// <param name="settings">Settings object to take parameters from</param>
        private static void EnsureLang(UIPageURLSettings settings)
        {
            settings.QueryParameters[URLHelper.LanguageParameterName] = settings.Node.DocumentCulture;
            settings.QueryParameters[URLHelper.LanguageParameterName + ObjectLifeTimeFunctions.OBJECT_LIFE_TIME_KEY] = "request";
        }


        /// <summary>
        /// Ensures node identifier and culture parameters in given query collection.
        /// </summary>
        /// <param name="settings">Settings object to take parameters from</param>
        /// <param name="addMode">Whether to add also mode parameter</param>
        private static void EnsureDocumentIdentifiers(UIPageURLSettings settings, bool addMode)
        {
            settings.QueryParameters["nodeid"] = settings.NodeID.ToString();
            settings.QueryParameters["culture"] = settings.Culture;

            if (addMode)
            {
                settings.QueryParameters["mode"] = settings.Mode;
            }
        }


        /// <summary>
        /// Check whether the page should be displayed directly or through ViewValidate page.
        /// </summary>
        /// <param name="settings">Settings object to take parameters from</param>
        private static bool CheckValidationVisibility(UIPageURLSettings settings)
        {
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            string siteName = SiteContext.CurrentSiteName;
            var validationExcludedClassNames = ";" + SystemDocumentTypes.File + ";";

            return
                // Check that node exists
                (settings.Node != null)
                // Class name is not excluded
                && !validationExcludedClassNames.Contains(";" + settings.Node.NodeClassName.ToLowerCSafe() + ";")
                // Node is not content only
                && !settings.Node.NodeIsContentOnly
                // Site is not content only
                && !settings.Node.Site.SiteIsContentOnly
                // User is authorized per UI element
                && user.IsAuthorizedPerUIElement("CMS.Content", "Validation", siteName)
                // Node is published in livesite mode
                && (settings.Node.IsPublished || (settings.Mode != "livesite"))
                // URL is not being transformed to splitview comparative culture
                && !settings.TransformToCompareUrl;
        }


        /// <summary>
        /// Transforms given URL to its compare variant using UIContext.SplitModeCultureCode.
        /// </summary>
        /// <param name="url">URL to transform</param>
        /// <returns>URL for comparison</returns>
        public static string TransformToCompareUrl(string url)
        {
            url = UrlResolver.ResolveUrl(URLHelper.UnResolveUrl(url, URLHelper.GetFullApplicationUrl()));
            NameValueCollection parameters = new NameValueCollection();
            // Ensure culture parameter
            parameters.Add("culture", PortalUIHelper.SplitModeCultureCode);
            // Remove culture-specific parameters
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.LanguageParameterName + ObjectLifeTimeFunctions.OBJECT_LIFE_TIME_KEY);
            url = URLHelper.RemoveParameterFromUrl(url, URLHelper.LanguageParameterName);
            url = URLHelper.RemoveParameterFromUrl(url, "culture");
            url = URLHelper.AddParameterToUrl(url, "compare", "1");
            url = VirtualContext.GetVirtualContextPath(url, parameters);
            return url;
        }


        /// <summary>
        /// Gets URL for the split view mode.
        /// </summary>
        /// <param name="originalUrl">Original URL</param>
        public static string GetSplitViewUrl(string originalUrl)
        {
            return AddUrlToQueryString("splitUrl", originalUrl, CONTENT_CMSDESK_FOLDER + "SplitView.aspx");
        }


        /// <summary>
        /// Gets URL for the view validate page.
        /// </summary>
        /// <param name="originalUrl">Original URL</param>
        /// <param name="settings">Settings object to take parameters from</param>
        public static string GetViewValidateUrl(string originalUrl, UIPageURLSettings settings)
        {
            settings.QueryParameters["mode"] = settings.Mode;
            settings.QueryParameters["displaytitle"] = "false";

            var url = UIContextHelper.GetElementUrl("CMS.Content", "Preview");

            return AddUrlToQueryString("viewpage", originalUrl, url);
        }


        /// <summary>
        /// Gets the view page URL
        /// </summary>
        public static string GetViewPageUrl()
        {
            var url = QueryHelper.GetString("viewpage", String.Empty);

            url = HttpUtility.UrlDecode(url);

            if (!String.IsNullOrEmpty(url))
            {
                url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, QueryHelper.GetString(URLHelper.LanguageParameterName, null));
                url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName + ObjectLifeTimeFunctions.OBJECT_LIFE_TIME_KEY, QueryHelper.GetString(URLHelper.LanguageParameterName + ObjectLifeTimeFunctions.OBJECT_LIFE_TIME_KEY, null));

                // Add device name if sent via query string
                String deviceName = QueryHelper.GetString(DeviceProfileInfoProvider.DEVICENAME_QUERY_PARAM, String.Empty);
                if (deviceName != String.Empty)
                {
                    url = URLHelper.AddParameterToUrl(url, DeviceProfileInfoProvider.DEVICENAME_QUERY_PARAM, deviceName);
                }

                // Split mode enabled
                if (PortalUIHelper.DisplaySplitMode && !PortalHelper.DeviceProfileActive)
                {
                    // Ensure identifiers for splitview toolbar
                    url = URLHelper.AddParameterToUrl(url, "nodeid", QueryHelper.GetString("nodeid", null));
                    url = URLHelper.AddParameterToUrl(url, "culture", QueryHelper.GetString("culture", null));
                    url = URLHelper.AddParameterToUrl(url, "mode", QueryHelper.GetString("mode", null));
                    // Do not show management panel in the sub-frame
                    url = URLHelper.AddParameterToUrl(url, "showpanel", "0");

                    url = GetSplitViewUrl(url);
                }
                else
                {
                    url = UrlResolver.ResolveUrl(url);
                }

                // Ensure device profile is loaded from cookies in CMSDesk
                url = URLHelper.UpdateParameterInUrl(url, DeviceProfileInfoProvider.DEVICES_QUERY_PARAM, "1");
            }
            else
            {
                // Show info page that preview is not available
                url = URLHelper.ResolveUrl(AdministrationUrlHelper.GetInformationUrl("document.nopreviewavailable"));
            }

            return url;
        }


        /// <summary>
        /// Appends one URL to another as a query parameter.
        /// </summary>
        /// <param name="queryParam">Name of query parameter under which the original URL will be stored </param>
        /// <param name="originalUrl">Original URL</param>
        /// <param name="newUrl">URL to which the original URL will be appended as a parameter</param>
        private static string AddUrlToQueryString(string queryParam, string originalUrl, string newUrl)
        {
            string queryString = URLHelper.GetQuery(originalUrl);

            string url = URLHelper.AppendQuery(UrlResolver.ResolveUrl(newUrl), queryString);

            url = URLHelper.AddParameterToUrl(url, queryParam, HttpUtility.UrlEncode(UrlResolver.ResolveUrl(originalUrl)));

            return url;
        }


        /// <summary>
        /// Checks currently edited document permissions and optionally redirects to access denied page
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="node">Document to check</param>
        public static void CheckDocumentPermissions(PermissionsEnum permission, TreeNode node)
        {
            if (node != null)
            {
                string message;

                switch (permission)
                {
                    case PermissionsEnum.Read:
                        message = "cmsdesk.notauthorizedtoreaddocument";
                        break;

                    case PermissionsEnum.Create:
                        message = "cmsdesk.notauthorizedtocreatedocument";
                        break;

                    default:
                        message = "cmsdesk.notauthorizedtoeditdocument";
                        break;
                }

                if (!CheckDocumentPermissions(node, permission))
                {
                    CMSPage.RedirectToAccessDenied(String.Format(AbstractCMSPage.GetString(message), node.NodeAliasPath));
                }
            }
        }


        /// <summary>
        /// Checks document permissions regarding the document manager mode and optionally redirects to access denied page
        /// </summary>
        public static void CheckDocumentPermissions(ICMSDocumentManager manager)
        {
            // Perform check only for normal page load
            if (RequestHelper.IsCallback())
            {
                return;
            }

            var node = manager.Node;
            if (node != null)
            {
                var permission = PermissionsEnum.Read;

                if (manager.Mode != FormModeEnum.Update)
                {
                    permission = PermissionsEnum.Create;
                }

                CheckDocumentPermissions(permission, node);
            }
        }


        /// <summary>
        /// Returns true if element should be hidden when editing specific tree node.
        /// </summary>
        /// <param name="element">Element to be checked if it is available for content only class.</param>
        /// <param name="node">Tree node to check visibility for.</param>
        public static bool IsElementHiddenForNode(UIElementInfo element, TreeNode node)
        {
            if ((element == null) || (node == null))
            {
                return false;
            }

            // Check blacklist in case of content only node/site
            if (node.NodeIsContentOnly || node.Site.SiteIsContentOnly)
            {
                var name = element.ElementName.ToLowerCSafe();

                return ContentOnlyHiddenUIElements.Contains(name);
            }

            return false;
        }

        #endregion


        #region "Document marks methods"

        /// <summary>
        /// Gets document mark image tag.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="mark">Document mark type</param>
        /// <param name="customTitle">Optional custom title for the icon</param>
        /// <param name="renderEnvelope">Indicates if surrounding span should be rendered</param>
        public static string GetDocumentMarkImage(Page page, DocumentMarkEnum mark, string customTitle = null, bool renderEnvelope = false)
        {
            string title;
            string className;

            switch (mark)
            {
                case DocumentMarkEnum.Archived:
                    title = CoreServices.Localization.GetString("Tree.ArchivedNodeTooltip");
                    className = "icon-circle tn color-gray-100";
                    break;

                case DocumentMarkEnum.Link:
                    title = CoreServices.Localization.GetString("Tree.LinkedNodeTooltip");
                    className = "icon-arrow-right-top-square tn";
                    break;

                case DocumentMarkEnum.NoTranslation:
                    title = CoreServices.Localization.GetString("Tree.NotTranslatedNodeTooltip");
                    className = "icon-rectangle-a-o tn color-gray-100";
                    break;

                case DocumentMarkEnum.Redirect:
                    title = CoreServices.Localization.GetString("Tree.RedirectedNodeTooltip");
                    className = "icon-arrow-right tn";
                    break;

                case DocumentMarkEnum.CheckedOut:
                    title = CoreServices.Localization.GetString("Tree.CheckedOutNodeTooltip");
                    className = "icon-lock tn";
                    break;

                case DocumentMarkEnum.DocumentWaitingForTranslation:
                    title = CoreServices.Localization.GetString("Tree.WaitingForTranslationTooltip");
                    className = "icon-rectangle-a tn";
                    break;

                case DocumentMarkEnum.Published:
                    title = CoreServices.Localization.GetString("Tree.PublishedNodeTooltip");
                    className = "icon-check-circle tn color-green-100";
                    break;

                case DocumentMarkEnum.Unpublished:
                    title = CoreServices.Localization.GetString("Tree.UnPublishedNodeTooltip");
                    className = "icon-times-circle tn color-red-70";
                    break;

                case DocumentMarkEnum.VersionNotPublished:
                    title = CoreServices.Localization.GetString("Tree.VersionNotPublishedNodeTooltip");
                    className = "icon-diamond tn color-orange-80";
                    break;

                case DocumentMarkEnum.ScheduledToBePublished:
                    title = CoreServices.Localization.GetString("Tree.ScheduledToBePublishedTooltip");
                    className = "icon-clock tn color-blue-100";
                    break;

                default:
                    return String.Empty;
            }

            // Use custom title
            if (!String.IsNullOrEmpty(customTitle))
            {
                title = customTitle;
            }

            // Get icon image tag
            var markup = UIHelper.GetAccessibleIconTag("NodeLink " + className, title);

            // Include envelope
            if (renderEnvelope)
            {
                markup = String.Format("<span class=\"tn-icon\">{0}</span>", markup);
            }

            return markup;
        }


        /// <summary>
        /// Gets document mark image tag.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="siteName">Site name</param>
        /// <param name="preferredCultureCode">User preferred culture code</param>
        /// <param name="stepType">Workflow step type</param>
        /// <param name="container">Data container</param>
        /// <param name="renderIconEnvelope">Indicates if icons should be surrounded by span envelope</param>
        public static string GetDocumentMarks(Page page, string siteName, string preferredCultureCode, WorkflowStepTypeEnum stepType, IDataContainer container, bool renderIconEnvelope = false)
        {
            if ((page != null) && (container != null))
            {
                string result = String.Empty;
                bool documentTranslated = false;

                if (!String.IsNullOrEmpty(preferredCultureCode))
                {
                    documentTranslated = (preferredCultureCode.ToLowerCSafe() == ValidationHelper.GetString(container.GetValue("DocumentCulture"), String.Empty).ToLowerCSafe());
                }

                if (documentTranslated)
                {
                    // 'Archived' icon
                    if (IsIconUsed(IconType.Archived, siteName) && ValidationHelper.GetBoolean(container.GetValue("DocumentIsArchived"), false))
                    {
                        result += GetDocumentMarkImage(page, DocumentMarkEnum.Archived, renderEnvelope: renderIconEnvelope);
                    }
                    else
                    {
                        bool displayPublishedIcon = IsIconUsed(IconType.Published, siteName);
                        bool displayNotPublishedIcon = IsIconUsed(IconType.NotPublished, siteName);

                        // Indicates if document uses workflow
                        bool usesWorkflow = (stepType != WorkflowStepTypeEnum.Undefined);
                        // Indicates if document uses workflow and is in 'Published' step
                        bool isPublishedStep = usesWorkflow && (stepType == WorkflowStepTypeEnum.DocumentPublished);
                        // Indicates if document uses workflow and isn't in 'Published' step
                        bool isNotPublishedStep = usesWorkflow && (stepType != WorkflowStepTypeEnum.DocumentPublished);

                        int checkedOutVersionHistoryId = ValidationHelper.GetInteger(container.GetValue("DocumentCheckedOutVersionHistoryID"), 0);
                        int publishedVersionHistoryId = ValidationHelper.GetInteger(container.GetValue("DocumentPublishedVersionHistoryID"), 0);
                        // Indicates if last version isn't published
                        bool lastVersionIsNotPublished = (checkedOutVersionHistoryId != publishedVersionHistoryId);

                        bool published = DocumentHelper.GetPublished(container);

                        if (published)
                        {
                            bool displayVersionNotPublishedIcon = IsIconUsed(IconType.VersionNotPublished, siteName);
                            bool displayVersionScheduledToBePublishedIcon = (displayPublishedIcon || displayVersionNotPublishedIcon);

                            // 'Published' icon
                            if (!lastVersionIsNotPublished && displayPublishedIcon && (!usesWorkflow || isPublishedStep))
                            {
                                result += GetDocumentMarkImage(page, DocumentMarkEnum.Published, renderEnvelope: renderIconEnvelope);
                            }
                            // 'Version not published' icon
                            else if (isNotPublishedStep && displayVersionNotPublishedIcon)
                            {
                                result += GetDocumentMarkImage(page, DocumentMarkEnum.VersionNotPublished, renderEnvelope: renderIconEnvelope);
                            }
                            // 'Version is scheduled to be published' icon
                            else if (isPublishedStep && lastVersionIsNotPublished && displayVersionScheduledToBePublishedIcon)
                            {
                                result += GetDocumentMarkImage(page, DocumentMarkEnum.Published, CoreServices.Localization.GetString("Tree.VersionPublishedVersionTooltip"), renderIconEnvelope);
                                result += GetDocumentMarkImage(page, DocumentMarkEnum.ScheduledToBePublished, renderEnvelope: renderIconEnvelope);
                            }
                        }
                        else
                        {
                            if (displayPublishedIcon || displayNotPublishedIcon)
                            {
                                DateTime publishFrom = ValidationHelper.GetDateTime(container.GetValue("DocumentPublishFrom"), DateTime.MinValue);
                                DateTime publishTo = ValidationHelper.GetDateTime(container.GetValue("DocumentPublishTo"), DateTime.MaxValue);

                                // 'Document is scheduled to be published' icon
                                if ((isPublishedStep && lastVersionIsNotPublished)
                                    || (!usesWorkflow && (((publishFrom > DateTime.Now) || (publishFrom == DateTimeHelper.ZERO_TIME))
                                                          && (((publishTo > publishFrom) && (publishTo > DateTime.Now))))))
                                {
                                    result += GetDocumentMarkImage(page, DocumentMarkEnum.ScheduledToBePublished, renderEnvelope: renderIconEnvelope);
                                }
                                // 'UnPublished' icon
                                else if (displayNotPublishedIcon)
                                {
                                    result += GetDocumentMarkImage(page, DocumentMarkEnum.Unpublished, renderEnvelope: renderIconEnvelope);
                                }
                            }
                        }
                    }

                    // 'Checked out' icon
                    if (IsIconUsed(IconType.CheckedOut, siteName) && (ValidationHelper.GetInteger(container.GetValue("DocumentCheckedOutByUserID"), 0) > 0))
                    {
                        result += GetDocumentMarkImage(page, DocumentMarkEnum.CheckedOut, renderEnvelope: renderIconEnvelope);
                    }

                }

                if (documentTranslated)
                {
                    // Waiting for translation icon
                    if (ValidationHelper.GetBoolean(container.GetValue("DocumentIsWaitingForTranslation"), false))
                    {
                        result += GetDocumentMarkImage(page, DocumentMarkEnum.DocumentWaitingForTranslation, renderEnvelope: renderIconEnvelope);
                    }
                }

                // 'Not translated' icon
                if (!documentTranslated && IsIconUsed(IconType.NotTranslated, siteName))
                {
                    result += GetDocumentMarkImage(page, DocumentMarkEnum.NoTranslation, renderEnvelope: renderIconEnvelope);
                }

                if (documentTranslated)
                {
                    // 'Redirected' icon
                    if (IsIconUsed(IconType.Redirected, siteName) && (!String.IsNullOrEmpty(ValidationHelper.GetString(container.GetValue("DocumentMenuRedirectURL"), null)) || ValidationHelper.GetBoolean(container.GetValue("DocumentMenuRedirectToFirstChild"), false)))
                    {
                        result += GetDocumentMarkImage(page, DocumentMarkEnum.Redirect, renderEnvelope: renderIconEnvelope);
                    }
                }

                // 'Linked' icon
                if (IsIconUsed(IconType.Linked, siteName) && (ValidationHelper.GetInteger(container.GetValue("NodeLinkedNodeID"), 0) > 0))
                {
                    result += GetDocumentMarkImage(page, DocumentMarkEnum.Link, renderEnvelope: renderIconEnvelope);
                }

                // Prepare arguments for event
                var e = new DocumentMarkEventArgs
                {
                    MarkContent = result,
                    Container = container,
                    PreferredCultureCode = preferredCultureCode,
                    SiteName = siteName,
                    StepType = stepType
                };

                // Raise event and return the content
                DocumentEvents.GetDocumentMark.StartEvent(e);

                return e.MarkContent;
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns true if icon type is enabled 
        /// </summary>
        /// <param name="type">Icon type</param>
        /// <param name="siteName">Site name</param>
        public static bool IsIconUsed(IconType type, string siteName)
        {
            string keyName = null;

            switch (type)
            {
                case IconType.Archived:
                    keyName = ".CMSDisplayArchivedIcon";
                    break;

                case IconType.CheckedOut:
                    keyName = ".CMSDisplayCheckedOutIcon";
                    break;

                case IconType.Linked:
                    keyName = ".CMSDisplayLinkedIcon";
                    break;

                case IconType.NotPublished:
                    keyName = ".CMSDisplayNotPublishedIcon";
                    break;

                case IconType.NotTranslated:
                    keyName = ".CMSDisplayNotTranslatedIcon";
                    break;

                case IconType.Published:
                    keyName = ".CMSDisplayPublishedIcon";
                    break;

                case IconType.Redirected:
                    keyName = ".CMSDisplayRedirectedIcon";
                    break;

                case IconType.VersionNotPublished:
                    keyName = ".CMSDisplayVersionNotPublishedIcon";
                    break;

                default:
                    EventLogProvider.LogWarning("DocumentHelper", "ISICONUSED", null, SiteContext.CurrentSiteID, "Setting key for IconType '" + type.ToStringRepresentation() + "' was not found.");
                    break;
            }

            if (!String.IsNullOrEmpty(keyName))
            {
                return SettingsKeyInfoProvider.GetBoolValue(siteName + keyName);
            }

            return false;
        }


        /// <summary>
        /// Returns true if any of the icon is used
        /// </summary>
        /// <param name="iconTypeFlags">Icon flags</param>
        public static bool IconsUsed(IconType iconTypeFlags)
        {
            string siteName = SiteContext.CurrentSiteName;

            // Loop thru available icon types
            foreach (IconType flag in Enum.GetValues(typeof(IconType)))
            {
                // Check whether enum value should be checked
                if (CheckEnum(flag, iconTypeFlags))
                {
                    if (IsIconUsed(flag, siteName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if type is used in current flags
        /// </summary>
        /// <param name="type">Icon type</param>
        /// <param name="flags">Enum type flags</param>
        private static bool CheckEnum(IconType type, IconType flags)
        {
            // Note: Do not use HasFlag method due to performance issue in .NET4 
            return ((type & flags) == type);
        }

        #endregion
    }
}