using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.PortalEngine;
using CMS.Mvc;

using RequestContext = System.Web.Routing.RequestContext;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Helper for the MVC operations
    /// </summary>
    public static class CMSMvcHelper
    {
        #region "Public methods"

        /// <summary>
        /// Gets the MVC page handler for the given page info.
        /// </summary>
        /// <param name="pageInfo">The page info.</param>
        /// <param name="requestContext">The request context.</param>
        /// <param name="status">The status.</param>
        public static IHttpHandler GetMVCPageHandler(PageInfo pageInfo, RequestContext requestContext, ref RequestStatusEnum status)
        {
            IHttpHandler handler = null;

            // Check the template
            PageTemplateInfo pti = pageInfo.UsedPageTemplateInfo;
            string urlPath = pageInfo.NodeAliasPath.Trim('/');

            if (IsMVCPageInfo(pageInfo))
            {
                RouteData data;
                string controllerName = string.Empty;
                string controllerAction = string.Empty;

                // MVC document
                if (IsMVCDocument(pageInfo))
                {
                    string prefix;
                    Hashtable registeredPathValues = new Hashtable();
                    urlPath = pageInfo.DocumentUrlPath;

                    // Parse the URL
                    TreePathUtils.ParseUrlPath(ref urlPath, out prefix, registeredPathValues);

                    controllerName = (string)registeredPathValues["controller"];
                    controllerAction = (string)registeredPathValues["action"];
                }
                // MVC template
                else if (IsMVCPageTemplate(pageInfo))
                {
                    controllerName = pti.PageTemplateDefaultController;
                    controllerAction = pti.PageTemplateDefaultAction;
                }

                if (requestContext == null)
                {
                    // Ensure the request context for routing
                    HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
                    IRouteHandler routeHandler = MvcAdapter.GetMvcRouteHandler();

                    urlPath = urlPath.TrimStart('/');

                    // Ensure wildcard with hidden default value, e.g. {*name;value*}
                    int wildCardIndex = urlPath.IndexOf("{*", StringComparison.Ordinal);
                    if (wildCardIndex >= 1)
                    {
                        urlPath = urlPath.Substring(0, wildCardIndex);
                    }

                    Route route = new Route(urlPath, routeHandler);
                    data = new RouteData(route, route.RouteHandler);

                    // Update the request context with the actual route data
                    requestContext = new RequestContext(httpContext, data);
                }
                else
                {
                    // Use data from the given context
                    data = requestContext.RouteData;
                }

                RouteValueDictionary values = data.Values;

                // Ensure controller and action in values
                if (!values.ContainsKey("controller"))
                {
                    values.Add("controller", controllerName);
                }
                if (!values.ContainsKey("action"))
                {
                    values.Add("action", controllerAction);
                }

                // Process the values to match proper controller and action
                if (ProcessValues(requestContext, (Route)data.Route, values, pageInfo.SiteName, pageInfo.NodeAliasPath, urlPath))
                {
                    handler = MvcAdapter.GetMvcHandler(requestContext);
                }

                status = RequestStatusEnum.MVCPage;
            }

            return handler;
        }


        /// <summary>
        /// Processes the request values, controller and view
        /// </summary>
        /// <param name="context">Request context</param>
        /// <param name="route">Route</param>
        /// <param name="values">Route values</param>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL path</param>
        public static bool ProcessValues(RequestContext context, Route route, RouteValueDictionary values, string siteName, string aliasPath, string urlPath)
        {
            // Ensure the default values
            EnsureDefaultValues(route, values);

            string controllerName = ValidationHelper.GetString(values["controller"], "");

            // Check the controller availability
            string missingValue = null;
            if (String.IsNullOrEmpty(controllerName))
            {
                missingValue = "controller";
            }
            else
            {
                // Check the action availability
                string actionName = ValidationHelper.GetString(values["action"], "");
                if (String.IsNullOrEmpty(actionName))
                {
                    missingValue = "action";
                }
            }

            if (missingValue != null)
            {
                // Log the routing error
                string error = String.Format("[CMSMvcDocumentConstraint.Match]: The {0} for route '{1}' is not initialized. You must specify the {0} in the URL path. Current URL path for page '{2}' is '{3}'.", missingValue, route.Url, aliasPath, urlPath);

                LogMVCRoutingError(error);

                return false;
            }

            // Try the controller by specific namespace (if fully qualified)
            Type controllerType = null;
            string[] ns = null;

            // Ensure the route data tokens
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            RouteData data = context.RouteData;

            int dotIndex = controllerName.LastIndexOfCSafe('.');
            if (dotIndex > 0)
            {
                string nsPart = controllerName.Substring(0, dotIndex);
                string namePart = controllerName.Substring(dotIndex + 1);

                ns = new[] { nsPart };
                data.DataTokens["Namespaces"] = ns;

                if ((controllerType = CMSControllerFactory.GetSingleControllerType(context, namePart)) != null)
                {
                    values["ControllerFullName"] = controllerName;

                    controllerName = namePart;

                    ns = new[] { controllerType.Namespace };

                    values["Controller"] = controllerName;
                }
                else
                {
                    // No specific controller found, do not match
                    throw new Exception("[CMSMvcDocumentConstraint.Match]: The fully qualified controller '" + controllerName + "' was not found.");
                }
            }

            // Try site controller
            if (controllerType == null)
            {
                ns = new[] { "CMS.Controllers." + siteName, siteName };
                data.DataTokens["Namespaces"] = ns;

                if ((controllerType = CMSControllerFactory.GetSingleControllerType(context, controllerName)) != null)
                {
                    ns = new[] { controllerType.Namespace };
                }
            }

            // Try global controller
            if (controllerType == null)
            {
                ns = new[] { "CMS.Controllers.Global", "Global" };
                data.DataTokens["Namespaces"] = ns;

                if ((controllerType = CMSControllerFactory.GetSingleControllerType(context, controllerName)) != null)
                {
                    ns = new[] { controllerType.Namespace };
                }
            }

            // Try any controller
            if (controllerType == null)
            {
                ns = null;
                data.DataTokens["Namespaces"] = null;

                try
                {
                    if (CMSControllerFactory.GetAnyControllerType(context, controllerName) == null)
                    {
                        // No single controller found, do not match
                        throw new Exception("[CMSMvcDocumentConstraint.Match]: The controller with name '" + controllerName + "' was not found.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    EventLogProvider.LogException("Routing", "FINDCONTROLLER", ex);

                    return false;
                }
            }

            if (ns != null)
            {
                route.DataTokens["Namespaces"] = ns;
            }

            return true;
        }


        /// <summary>
        /// Determines whether the given page info carries either MVC document or MVC template.
        /// </summary>
        /// <param name="pageInfo">The page info.</param>
        public static bool IsMVCPageInfo(PageInfo pageInfo)
        {
            return IsMVCDocument(pageInfo) || IsMVCPageTemplate(pageInfo);
        }


        /// <summary>
        /// Determines whether the given page info carries a MVC template.
        /// </summary>
        /// <param name="pageInfo">The page info.</param>
        public static bool IsMVCPageTemplate(PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                throw new Exception("[URLRewriter.IsMVCPageTemplate]: Missing page info");
            }

            // Check the template
            PageTemplateInfo pti = pageInfo.UsedPageTemplateInfo;
            return (pti != null) && (pti.PageTemplateType == PageTemplateTypeEnum.MVC);
        }


        /// <summary>
        /// Determines whether the given page info carries a MVC document.
        /// </summary>
        /// <param name="pageInfo">The page info.</param>
        public static bool IsMVCDocument(PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                throw new Exception("[URLRewriter.IsMVCDocument]: Missing page info");
            }

            string documentUrlPath = pageInfo.DocumentUrlPath;
            if (!string.IsNullOrEmpty(documentUrlPath))
            {
                return documentUrlPath.StartsWith(TreePathUtils.URL_PREFIX_MVC, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Logs an MVC routing error to the event log
        /// </summary>
        /// <param name="error">Error message to log</param>
        private static void LogMVCRoutingError(string error)
        {
            EventLogProvider.LogEvent(EventType.ERROR, "MVCRouting", "ROUTE", error);
        }


        /// <summary>
        /// Ensures the default values from the given route in the value dictionary
        /// </summary>
        /// <param name="route">Route</param>
        /// <param name="values">Values</param>
        private static void EnsureDefaultValues(Route route, RouteValueDictionary values)
        {
            // Get route defaults
            RouteValueDictionary routeDefaults = route.Defaults;
            if (routeDefaults == null)
            {
                return;
            }

            // Ensure the defaults
            RouteValueDictionary defaults = (RouteValueDictionary)routeDefaults["DefaultValues"];
            if (defaults == null)
            {
                return;
            }

            // Load default values
            foreach (KeyValuePair<string, object> item in defaults)
            {
                if (!values.ContainsKey(item.Key))
                {
                    object value = item.Value;
                    if (value != MvcAdapter.OptionalUrlParameter)
                    {
                        values.Add(item.Key, value);
                    }
                }
            }
        }

        #endregion
    }
}
