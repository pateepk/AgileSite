using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.Routing;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Handler for the MVC routes for URL rewriter.
    /// </summary>
    public static class CMSDocumentRouteHelper
    {
        /// <summary>
        /// Alias page info result type.
        /// </summary>
        public const string ALIAS_PAGE_INFO_RESULT = "AliasPageInfoResult";


        // Table of the registered routes [siteName] -> [List of RouteBase]
        private static readonly CMSStatic<SafeDictionary<string, List<RouteBase>>> mRoutes = new CMSStatic<SafeDictionary<string, List<RouteBase>>>(() => new SafeDictionary<string, List<RouteBase>>());
        private static SafeDictionary<string, List<RouteBase>> Routes => mRoutes.Value;


        // Table of the registers routes by the document [node key] -> [List of RouteBase]
        private static readonly CMSStatic<SafeDictionary<string, List<RouteBase>>> mRoutesByDocuments = new CMSStatic<SafeDictionary<string, List<RouteBase>>>(() => new SafeDictionary<string, List<RouteBase>>());
        private static SafeDictionary<string, List<RouteBase>> RoutesByDocuments => mRoutesByDocuments.Value;


        // Route lock.
        private static readonly object routeLock = new object();


        /// <summary>
        /// Drops all the registered routes.
        /// </summary>
        /// <param name="logWebFarmTask">Indicates whether a web farm synchronization task for routes drop should be created</param>
        public static void DropAllRoutes(bool logWebFarmTask = true)
        {
            // Do not remove system routes!
            // RouteTable.Routes.Clear();

            string[] siteNames;

            // Get route site names
            lock (routeLock)
            {
                siteNames = new string[Routes.Keys.Count];
                Routes.Keys.CopyTo(siteNames, 0);
            }

            // loop thru site  routes
            foreach (string siteName in siteNames)
            {
                DropRoutes(siteName);
            }

            Routes.Clear();

            if (logWebFarmTask)
            {
                WebFarmHelper.CreateTask(new DropAllRoutesWebFarmTask());
            }
        }


        /// <summary>
        /// Drops the routes for the given web site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static void DropRoutes(string siteName)
        {
            lock (routeLock)
            {
                string lowerSiteName = siteName.ToLowerInvariant();

                // Get the registered routes
                var routes = Routes[lowerSiteName];
                if (routes != null)
                {
                    // Ensure thread safe manipulation with the route collection
                    using (RouteTable.Routes.GetWriteLock())
                    {
                        // Remove all the routes
                        foreach (RouteBase route in routes)
                        {
                            RouteTable.Routes.Remove(route);
                        }
                    }

                    Routes[lowerSiteName] = null;
                }
            }
        }


        /// <summary>
        /// Ensures that the routes are registered for the given site.
        /// </summary>
        public static void EnsureRoutes(string siteName)
        {
            string lowerSiteName = siteName.ToLowerInvariant();

            if (Routes.Contains(lowerSiteName))
            {
                // Already registered
                return;
            }

            lock (routeLock)
            {
                if (Routes.Contains(lowerSiteName))
                {
                    // Already registered
                    return;
                }

                // Register the routes
                List<RouteBase> routes = RegisterRoutes(siteName);
                Routes[lowerSiteName] = routes;
            }
        }


        /// <summary>
        /// Registers the MVC routes for the given site.
        /// </summary>
        private static List<RouteBase> RegisterRoutes(string siteName)
        {
            var routes = new List<RouteBase>();

            int siteId = SiteInfoProvider.GetSiteID(siteName);
            if (siteId > 0)
            {
                // Register the document routes
                RegisterDocumentRoutes("NodeSiteID = " + siteId, siteName, routes);

                // Register the alias routes
                RegisterAliasRoutes("AliasSiteID = " + siteId, siteName, routes);
            }

            return routes;
        }


        /// <summary>
        /// Registers the routes of the documents matching the given where condition
        /// </summary>
        /// <param name="where">Where condition for the documents</param>
        /// <param name="siteName">Site name</param>
        /// <param name="routes">List of routes to collect the registered routes</param>
        private static void RegisterDocumentRoutes(string where, string siteName, List<RouteBase> routes)
        {
            // Get the MVC like URLs from documents
            var tree = new TreeProvider();
            var query = tree.SelectNodes()
                           .All()
                           .Where(where)
                           .Where(new WhereCondition().WhereStartsWith("DocumentURLPath", TreePathUtils.URL_PREFIX_ROUTE))
                           .Columns("NodeAliasPath", "DocumentURLPath", "NodeID", "DocumentCulture");

            var data = query.Result;
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // Register all items
            foreach (DataRow dr in data.Tables[0].Rows)
            {
                string aliasPath = ValidationHelper.GetString(dr["NodeAliasPath"], "");
                string urlPath = ValidationHelper.GetString(dr["DocumentURLPath"], "");

                int nodeId = ValidationHelper.GetInteger(dr["NodeID"], 0);
                string culture = ValidationHelper.GetString(dr["DocumentCulture"], "");

                // Register the route
                Route route = RegisterRoute(siteName, aliasPath, urlPath, nodeId, culture, false);

                if (route != null)
                {
                    routes?.Add(route);
                }
            }
        }


        /// <summary>
        /// Registers the routes of the aliases matching the given where condition
        /// </summary>
        /// <param name="where">Where condition for the aliases</param>
        /// <param name="siteName">Site name</param>
        /// <param name="routes">List of routes to collect the registered routes</param>
        private static void RegisterAliasRoutes(string where, string siteName, List<RouteBase> routes)
        {
            where = SqlHelper.AddWhereCondition(where, $"AliasURLPath LIKE '{TreePathUtils.URL_PREFIX_ROUTE}%'");

            const string REQUIRED_COLUMNS = "NodeAliasPath, AliasNodeID, AliasCulture, AliasURLPath, AliasSiteID, AliasActionMode";
            var data = DocumentAliasInfoProvider.GetDocumentAliasesWithNodesDataQuery()
                                                .Where(where)
                                                .Columns(REQUIRED_COLUMNS.Split(','));

            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            // Register all items
            foreach (DataRow dr in data.Tables[0].Rows)
            {
                string aliasPath = ValidationHelper.GetString(dr["NodeAliasPath"], string.Empty);
                string aliasUrlPath = DataHelper.GetStringValue(dr, "AliasURLPath");
                int nodeId = ValidationHelper.GetInteger(dr["AliasNodeID"], 0);
                string culture = DataHelper.GetStringValue(dr, "AliasCulture");
                string aliasAction = DataHelper.GetStringValue(dr, "AliasActionMode");
                AliasActionModeEnum aliasActionMode = aliasAction.ToEnum<AliasActionModeEnum>();

                Route route = RegisterRoute(siteName, aliasPath, aliasUrlPath, nodeId, culture, true);
                if (route == null)
                {
                    continue;
                }

                var pageResult = new PageInfoResult
                {
                    // Keep the document alias data
                    DocumentAliasActionMode = aliasActionMode,
                    DocumentAliasCulture = culture,
                    DocumentAliasUrlPath = aliasUrlPath
                };

                if (route.DataTokens == null)
                {
                    route.DataTokens = new RouteValueDictionary();
                }

                // Store the document alias data in the route
                route.DataTokens.Add(ALIAS_PAGE_INFO_RESULT, pageResult);

                routes?.Add(route);
            }
        }


        /// <summary>
        /// Registers the given route
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL path</param>
        /// <param name="nodeId">Node ID</param>
        /// <param name="culture">Document culture</param>
        /// <param name="alias">If true, the route comes from document alias</param>
        private static Route RegisterRoute(string siteName, string aliasPath, string urlPath, int nodeId, string culture, bool alias)
        {
            // Remove the first part
            var separatorIndex = urlPath?.IndexOf(':');
            if (separatorIndex == null || separatorIndex < 0)
            {
                return null;
            }

            // Parse the URL
            string prefix;
            Hashtable values = new Hashtable();

            TreePathUtils.ParseUrlPath(ref urlPath, out prefix, values);

            // Parse the pattern
            RouteValueDictionary defaults = new RouteValueDictionary();

            string pattern = urlPath.TrimStart('/');
            pattern = GetRoutingPattern(pattern, defaults);

            // Copy the defaults to the default values
            foreach (DictionaryEntry item in values)
            {
                defaults[(string)item.Key] = item.Value;
            }

            try
            {
                Route route;
                var routes = new List<RouteBase>();

                var pageInfoSource = (alias ? PageInfoSource.DocumentAlias : PageInfoSource.UrlPath);

                if (prefix.StartsWith(TreePathUtils.URL_PREFIX_ROUTE, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Standard route to document based on templates
                    var constraint = new CMSDocumentRouteConstraint(siteName, aliasPath, nodeId, culture, pageInfoSource);
                    var handler = new CMSDocumentRouteHandler();

                    //defaults.Add("DocumentConstraint", "");

                    // Register the route
                    route = new Route(pattern, defaults, handler);
                    route.Constraints = new RouteValueDictionary { { "DocumentConstraint", constraint } };

                    routes.Add(route);

                    // Ensure thread safe manipulation with the route collection
                    using (RouteTable.Routes.GetWriteLock())
                    {
                        RouteTable.Routes.Add(route);
                    }
                }
                else
                {
                    return null;
                }

                // Register by document
                string key = nodeId.ToString();
                if (alias)
                {
                    key += "|aliases";
                }
                else
                {
                    key += "|culture|" + culture?.ToLowerInvariant();
                }

                var documentRoutes = RoutesByDocuments[key];
                if (documentRoutes == null)
                {
                    documentRoutes = new List<RouteBase>();
                    RoutesByDocuments[key] = documentRoutes;
                }

                // Add the route to the list of routes by document
                documentRoutes.AddRange(routes);

                return route;
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Routing", "REGISTERROUTE", ex);
            }

            return null;
        }


        /// <summary>
        /// Drops the routes for the given document from the routing table
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="culture">Document culture</param>
        private static void DropRoutes(int nodeId, string culture)
        {
            lock (routeLock)
            {
                // Find out if the route is registered
                string key = nodeId + "|culture|" + culture?.ToLowerInvariant();

                var routes = RoutesByDocuments[key];
                if (routes != null)
                {
                    // Remove the routes
                    RoutesByDocuments[key] = null;

                    // Ensure thread safe manipulation with the route collection
                    using (RouteTable.Routes.GetWriteLock())
                    {
                        foreach (var route in routes)
                        {
                            if (RouteTable.Routes.Contains(route))
                            {
                                RouteTable.Routes.Remove(route);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Drops the routes for the given document from the routing table
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        private static void DropAliasRoutes(int nodeId)
        {
            lock (routeLock)
            {
                // Find out if the route is registered
                string key = nodeId + "|aliases";

                List<RouteBase> routes = RoutesByDocuments[key];
                if (routes != null)
                {
                    // Remove the routes
                    RoutesByDocuments[key] = null;

                    // Ensure thread safe manipulation with the route collection
                    using (RouteTable.Routes.GetWriteLock())
                    {
                        foreach (RouteBase route in routes)
                        {
                            if (RouteTable.Routes.Contains(route))
                            {
                                RouteTable.Routes.Remove(route);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Refreshes the routes for particular document
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="logWebFarmTask">Indicates whether a web farm synchronization task for page routes refresh should be created</param>
        public static void RefreshRoutes(TreeNode node, bool logWebFarmTask = true)
        {
            DropRoutes(node.NodeID, node.DocumentCulture);

            string siteName = node.NodeSiteName;

            // Register the main document route
            RegisterRoute(siteName, node.NodeAliasPath, node.DocumentUrlPath, node.NodeID, node.DocumentCulture, false);

            if (logWebFarmTask)
            {
                WebFarmHelper.CreateTask(new RefreshPageRoutesWebFarmTask { DocumentID = node.DocumentID });
            }
        }


        /// <summary>
        /// Refreshes the routes for particular document aliases
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="logWebFarmTask">Indicates whether a web farm synchronization task for alias routes refresh should be created</param>
        public static void RefreshAliasRoutes(int nodeId, string siteName, bool logWebFarmTask = true)
        {
            DropAliasRoutes(nodeId);

            // Register the aliases routes
            RegisterAliasRoutes("AliasNodeID = " + nodeId, siteName, null);

            if (logWebFarmTask)
            {
                WebFarmHelper.CreateTask(new RefreshAliasRoutesWebFarmTask { NodeID = nodeId, SiteName = siteName });
            }
        }


        /// <summary>
        /// Gets the URL pattern from wildcard URL path and places the default values into the defaults name-value collection
        /// </summary>
        /// <param name="urlPath">URL path to analyze</param>
        /// <param name="defaults">If set, returns the default values of the wildcards</param>
        private static string GetRoutingPattern(string urlPath, RouteValueDictionary defaults)
        {
            // Prepare the match
            MatchEvaluator evaluator =
                m => ParseWildcardElement(m, defaults);

            // Process all URLs
            urlPath = DocumentURLProvider.WildcardRegex.Replace(urlPath, evaluator);

            return urlPath;
        }


        /// <summary>
        /// Parses single wildcard element and extracts its value to the defaults collection. Returns the element without the default value
        ///
        /// Handles following scenarios:
        ///
        /// {name} - No changes, standard wildcard without default value
        /// {name;value} - Outputs the value as the hard part of the URL, value is also added to the defaults
        /// {?name;value} - Makes optional parameter wildcard with value added to defaults
        /// </summary>
        /// <param name="m">Element match</param>
        /// <param name="defaults">If set, returns the default values of the wildcards</param>
        private static string ParseWildcardElement(Match m, RouteValueDictionary defaults)
        {
            string name = m.Groups[1].ToString();

            // Check if the parameter is hidden
            bool hidden = name.StartsWith("*", StringComparison.Ordinal) && name.EndsWith("*", StringComparison.Ordinal);
            if (hidden)
            {
                name = name.Trim('*');
            }

            object value = null;

            // Look for the default value
            int delimiterIndex = name.IndexOf(DocumentURLProvider.WildcardDefaultValueDelimiter);
            if (delimiterIndex > 0)
            {
                value = name.Substring(delimiterIndex + 1);
                name = name.Substring(0, delimiterIndex).Trim();
            }

            // Split to name and value
            if ((defaults != null) && (value != null))
            {
                defaults.Add(name, value);
            }

            if (hidden)
            {
                // Hidden value
                return "";
            }

            // Return pattern
            return "{" + name + "}";
        }
    }
}