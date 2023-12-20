using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides static helper methods for converting and validating paths, aliases and names.
    /// </summary>
    public static class TreePathUtils
    {
        #region "Constants"

        /// <summary>
        /// Maximal length reserved for the unique suffix.
        /// </summary>
        private const int UNIQUE_SUFFIX_MAX_LENGTH = 6;

        /// <summary>
        /// Maximal index to be used for unique name and alias.
        /// </summary>
        private const int MAX_UNIQUE_INDEX = 1000;

        /// <summary>
        /// Regex matching the wildcard within alias path.
        /// </summary>
        private static readonly CMSRegex IsWildCardRegEx = new CMSRegex("(?<!\\[)[_%]|[_%](?!\\])");

        /// <summary>
        /// Regex for matching the escaped content within alias path.
        /// </summary>
        /// Groups:                                                       (esc.content)
        private static readonly CMSRegex RemoveWildCardRegEx = new CMSRegex("\\[(\\]|[^\\]]*)\\]");

        /// <summary>
        /// Maximal length of the node URLPath.
        /// </summary>
        public const int MAXURLPATHLENGTH = 450;

        /// <summary>
        /// Maximal length of the node document name.
        /// </summary>
        public const int MAX_NAME_LENGTH = 100;


        /// <summary>
        /// Maximal length of the node alias.
        /// </summary>
        internal const int MAX_ALIAS_LENGTH = 50;


        /// <summary>
        /// URL prefix for the Routing
        /// </summary>
        public const string URL_PREFIX_ROUTE = "ROUTE:";

        #endregion


        #region "Variables"

        private static int? mMaxAliasLength;
        private static int? mMaxNameLength;
        private static bool? mUseLimitReplacementsForUrlPath;
        private static Regex mWildcardCharsRegex;
        private static string mAllowedURLPathValues;
        private static bool? mRemoveDiacriticForSafeURLPath;
        private static int? mMaxURLPathLength;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns string containing allowed characters that cant be used in URL path.
        /// </summary>
        public static string AllowedURLPathValues
        {
            get
            {
                return mAllowedURLPathValues ?? (mAllowedURLPathValues = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAllowedURLPathValues"], "/"));
            }
            set
            {
                mAllowedURLPathValues = value;
            }
        }


        /// <summary>
        /// Indicates if diacritics should be removed from the URL path.
        /// </summary>
        public static bool RemoveDiacriticForSafeURLPath
        {
            get
            {
                if (mRemoveDiacriticForSafeURLPath == null)
                {
                    mRemoveDiacriticForSafeURLPath = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSRemoveDiacriticForSafeURLPath"], true);
                }

                return mRemoveDiacriticForSafeURLPath.Value;
            }
            set
            {
                mRemoveDiacriticForSafeURLPath = value;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether whether multiple, starting and trailing replacements should be limited.
        /// </summary>
        public static bool UseLimitURLReplacements
        {
            get
            {
                if (mUseLimitReplacementsForUrlPath == null)
                {
                    mUseLimitReplacementsForUrlPath = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSUseLimitReplacementsForUrlPath"], true);
                }

                return mUseLimitReplacementsForUrlPath.Value;
            }
            set
            {
                mUseLimitReplacementsForUrlPath = value;
            }
        }


        /// <summary>
        /// Maximal length of the node alias name.
        /// </summary>
        public static int MaxAliasLength
        {
            get
            {
                if (mMaxAliasLength == null)
                {
                    mMaxAliasLength = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSMaxNodeAliasLength"], MAX_ALIAS_LENGTH);
                }

                return mMaxAliasLength.Value;
            }
            set
            {
                mMaxAliasLength = value;
            }
        }


        /// <summary>
        /// Maximal length of the node document name.
        /// </summary>
        public static int MaxNameLength
        {
            get
            {
                if (mMaxNameLength == null)
                {
                    mMaxNameLength = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSMaxNodeNameLength"], MAX_NAME_LENGTH);
                }

                return mMaxNameLength.Value;
            }
            set
            {
                mMaxNameLength = value;
            }
        }


        /// <summary>
        /// Maximal length of the URL path.
        /// </summary>
        public static int MaxURLPathLength
        {
            get
            {
                if (mMaxURLPathLength == null)
                {
                    mMaxURLPathLength = MAXURLPATHLENGTH;
                }

                return mMaxURLPathLength.Value;
            }
            set
            {
                mMaxURLPathLength = value;
            }
        }


        /// <summary>
        /// Regex for wildcard characters not surrounded by escape brackets
        /// </summary>
        private static Regex WildcardCharsRegex
        {
            get
            {
                return mWildcardCharsRegex ?? (mWildcardCharsRegex = RegexHelper.GetRegex(@"(?<!\[[^[\]]*)(%|_)(?![^[\]]*\])"));
            }
        }

        #endregion


        #region "Path validation"

        /// <summary>
        /// Gets a where condition for a sub-tree starting with the document
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="includeCurrent">If true, current document is included</param>
        internal static WhereCondition GetSubTreeWhereCondition(int siteId, string aliasPath, bool includeCurrent)
        {
            var pathWhere = GetPathWhereCondition(aliasPath, includeCurrent ? PathTypeEnum.Section : PathTypeEnum.Children);

            var bulkWhere =
                new WhereCondition()
                    .WhereEquals("NodeSiteID", siteId)
                    .Where(pathWhere);

            return bulkWhere;
        }


        /// <summary>
        /// Filters the data to include only documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define selection scope</param>
        internal static WhereCondition GetPathWhereCondition(string path, PathTypeEnum type = PathTypeEnum.Explicit)
        {
            // When path not provided, use root
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be empty", nameof(path));
            }

            // Ensure correct path format
            switch (type)
            {
                case PathTypeEnum.Single:
                    return
                        new WhereCondition()
                            .WhereEquals("NodeAliasPath", path);

                case PathTypeEnum.Children:
                    return
                        new WhereCondition()
                            .WhereStartsWith("NodeAliasPath", path.TrimEnd('/') + '/');

                case PathTypeEnum.Section:
                    return
                        new WhereCondition()
                            .WhereEquals("NodeAliasPath", path)
                            .Or()
                            .WhereStartsWith("NodeAliasPath", path.TrimEnd('/') + '/');

                default:
                    return
                        new WhereCondition()
#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                            .WhereLike("NodeAliasPath", path);
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
            }
        }



        /// <summary>
        /// Ensures node alias path to be only for single document.
        /// </summary>
        /// <param name="path">Original node alias path</param>
        public static string EnsureSingleNodePath(string path)
        {
            return EnsureSinglePath(path);
        }


        /// <summary>
        /// Ensures path to be only for a single page.
        /// </summary>
        /// <param name="path">Original path</param>
        public static string EnsureSinglePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = path.TrimEnd('%');
            if (path != "/")
            {
                path = "/" + path.Trim('/');
            }

            return path;
        }


        /// <summary>
        /// Ensures path to be for all child pages.
        /// </summary>
        /// <param name="path">Original path</param>
        public static string EnsureChildPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.EndsWith("/%", StringComparison.Ordinal))
                {
                    path = path.TrimEnd('%', '/') + "/%";
                }
            }
            else
            {
                path = TreeProvider.ALL_DOCUMENTS;
            }

            return path;
        }


        /// <summary>
        /// Returns node alias without forbidden characters.
        /// </summary>
        /// <param name="nodeAlias">Node alias</param>
        /// <param name="siteName">Site name</param>
        public static string GetSafeNodeAlias(string nodeAlias, string siteName)
        {
            return URLHelper.GetSafeUrlPart(nodeAlias, siteName);
        }


        /// <summary>
        /// Returns node alias path without forbidden characters.
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="siteName">Site name</param>
        public static string GetSafeNodeAliasPath(string aliasPath, string siteName)
        {
            if (aliasPath == null)
            {
                return "/";
            }

            // Process path parts
            aliasPath = "/" + aliasPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(p => GetSafeNodeAlias(p, siteName))
                                       .Join("/");

            return aliasPath;
        }


        /// <summary>
        /// Returns the unique node alias for the given node, node alias and parent node.
        /// </summary>
        /// <param name="nodeAlias">Original node alias</param>
        /// <param name="siteName">Site name</param>
        /// <param name="parentNodeId">Parent node ID</param>
        /// <param name="currentNodeId">Current node ID</param>
        public static string GetUniqueNodeAlias(string nodeAlias, string siteName, int parentNodeId, int currentNodeId)
        {
            // If alias not specified, do not process
            if (string.IsNullOrEmpty(nodeAlias))
            {
                return string.Empty;
            }
            
            // Remove forbidden characters
            nodeAlias = GetSafeNodeAlias(nodeAlias, siteName);

            // Ensure maximal length
            nodeAlias = EnsureMaxNodeAliasLength(nodeAlias);

            // There is only one root node, no need to check uniqueness
            if (parentNodeId <= 0)
            {
                return nodeAlias;
            }

            // Prepare base alias
            var baseAlias = nodeAlias;

            // Remove unique suffix if present
            if (baseAlias.EndsWith(")", StringComparison.Ordinal))
            {
                var replacement = URLHelper.ForbiddenCharactersReplacement(siteName);
                baseAlias = Regex.Replace(baseAlias, "[ " + replacement + "](\\(\\d+\\))$", string.Empty);
                if (baseAlias == string.Empty)
                {
                    baseAlias = nodeAlias;
                }
            }

            // Get candidates
            var searchAliasPath = TextHelper.LimitLength(baseAlias, MaxAliasLength - UNIQUE_SUFFIX_MAX_LENGTH, string.Empty);
            var data = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                   .Distinct()
                                                   .Columns("NodeAlias")
                                                   .WhereEquals("NodeParentID", parentNodeId)
                                                   .WhereStartsWith("NodeAlias", searchAliasPath)
                                                   .WhereNotEquals("NodeID", currentNodeId)
                                                   .Result;

            // No candidates available, the node alias is unique
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return nodeAlias;
            }

            var candidates = data.Tables[0];
            var uniqueIndex = 1;

            do
            {
                // Get matching duplicate
                var match = candidates.Select($"NodeAlias = '{SqlHelper.EscapeQuotes(nodeAlias)}'");
                if (match.Length == 0)
                {
                    // If not match, consider as unique
                    return nodeAlias;
                }

                // Prepare next candidate
                var uniqueString = $" ({uniqueIndex})";

                // Get the available length for the URL path without the unique suffix
                int availableLength = MaxAliasLength - uniqueString.Length;

                // Trim the path if necessary
                nodeAlias = (baseAlias.Length > availableLength) ? baseAlias.Substring(0, availableLength) : baseAlias;

                // Ensure safe alias together with the unique suffix
                nodeAlias = GetSafeNodeAlias(nodeAlias + uniqueString, siteName);

                // Maximal number of attemps reached
                if (uniqueIndex >= MAX_UNIQUE_INDEX)
                {
                    throw new Exception("[TreePathUtils.GetUniqueNodeAlias]: The maximum number of tries to get a unique node alias was reached. Please validate URL settings to make sure the restrictions aren't too strict.");
                }

                uniqueIndex++;
            }
            while (true);
        }


        /// <summary>
        /// Returns the where condition for node alias path expression.
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="exclude">Indicates if the path should be excluded</param>
        /// <param name="combined">Indicates if return condition is combined with another</param>
        public static WhereCondition GetAliasPathCondition(string aliasPath, bool exclude = false, bool combined = false)
        {
            WhereCondition whereCondition = new WhereCondition();

            // Empty alias path
            if (String.IsNullOrEmpty(aliasPath))
            {
                return whereCondition;
            }

            // If alias path covers all documents and condition is not combined with another one and it is not an excluding condition, generate no condition
            if (((aliasPath == "%") || (aliasPath == "/%")) && !combined && !exclude)
            {
                return whereCondition;
            }

            // Get the condition (LIKE if contains wildcards)
            if (aliasPath.IndexOfAny(new[] { '%', '_', '[' }) >= 0)
            {
                // Check if there are real wildcards
                if (IsWildCardRegEx.IsMatch(aliasPath))
                {
                    var safeLikeQueryString = SqlHelper.GetSafeQueryString(aliasPath, false);

#pragma warning disable BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                    if (exclude)
                    {
                        return whereCondition.WhereNotLike("NodeAliasPath", safeLikeQueryString);
                    }
                    return whereCondition.WhereLike("NodeAliasPath", safeLikeQueryString);
#pragma warning restore BH2000 // Method 'WhereLike()' or 'WhereNotLike()' should not be used used.
                }

                // Remove the escapes if wildcards not found
                aliasPath = RemoveWildCardRegEx.Replace(aliasPath, "$1");
            }

            var safeEqualsQueryString = SqlHelper.GetSafeQueryString(aliasPath, false);

            if (exclude)
            {
                return whereCondition.WhereNotEquals("NodeAliasPath", safeEqualsQueryString);
            }
            return whereCondition.WhereEquals("NodeAliasPath", safeEqualsQueryString);
        }


        /// <summary>
        /// Gets maximal length of the node document name. Reflects the class node name source field size.
        /// </summary>
        /// <param name="className">Class name</param>
        public static int GetMaxNameLength(string className)
        {
            // Check source field
            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
            if (string.IsNullOrEmpty(classInfo?.ClassNodeNameSource))
            {
                return MaxNameLength;
            }

            // Get source field size
            var form = FormHelper.GetFormInfo(classInfo.ClassName, false);
            var field = form.GetFormField(classInfo.ClassNodeNameSource);

            // Field not found
            if (field == null)
            {
                return MaxNameLength;
            }

            // Use maximal length for long text
            return (field.DataType == FieldDataType.LongText) ? MaxNameLength : Math.Min(field.Size, MaxNameLength);
        }


        /// <summary>
        /// Ensures maximal length of the node document name. Reflects the class node name source field size.
        /// </summary>
        /// <param name="name">Name to trim</param>
        /// <param name="className">Class name</param>
        public static string EnsureMaxNodeNameLength(string name, string className)
        {
            return TextHelper.LimitLength(name, GetMaxNameLength(className), String.Empty);
        }


        /// <summary>
        /// Ensures maximal length of the file name. Reflects the class node name source field size.
        /// </summary>
        /// <param name="name">Name to trim</param>
        /// <param name="className">Class name</param>
        public static string EnsureMaxFileNameLength(string name, string className)
        {
            return TextHelper.LimitFileNameLength(name, GetMaxNameLength(className));
        }


        /// <summary>
        /// Ensures maximal allowed node alias length.
        /// </summary>
        /// <param name="nodeAlias">Original node alias</param>
        public static string EnsureMaxNodeAliasLength(string nodeAlias)
        {
            return TextHelper.LimitLength(nodeAlias, MaxAliasLength, String.Empty);
        }


        /// <summary>
        /// Returns the unique node name for the given node, node name and parent node.
        /// </summary>
        /// <param name="nodeName">Original node name</param>
        /// <param name="parentNodeId">Parent node ID</param>
        /// <param name="currentNodeId">Current node ID</param>
        /// <param name="className">Node class name</param>
        public static string GetUniqueNodeName(string nodeName, int parentNodeId, int currentNodeId, string className)
        {
            // If name not specified, do not process
            if (string.IsNullOrEmpty(nodeName))
            {
                return string.Empty;
            }

            // Ensure maximal length
            nodeName = nodeName.Trim();
            nodeName = EnsureMaxNodeNameLength(nodeName, className);

            // There is only one root node, no need to check uniqueness
            if (parentNodeId <= 0)
            {
                return nodeName;
            }

            // Prepare base name
            var baseName = nodeName;
            if (baseName.EndsWith(")", StringComparison.Ordinal))
            {
                baseName = Regex.Replace(baseName, "[ ](\\(\\d+\\))$", string.Empty);
                if (baseName == string.Empty)
                {
                    baseName = nodeName;
                }
            }

            // Get candidates
            var maxNameLength = GetMaxNameLength(className);
            var searchName = TextHelper.LimitLength(baseName, maxNameLength - UNIQUE_SUFFIX_MAX_LENGTH, string.Empty);
            var data = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                   .Distinct()
                                                   .Columns("NodeName")
                                                   .WhereEquals("NodeParentID", parentNodeId)
                                                   .WhereStartsWith("NodeName", searchName)
                                                   .WhereNotEquals("NodeID", currentNodeId)
                // Linked documents have same node name as their originals
                                                   .WhereNull("NodeLinkedNodeID")
                                                   .Result;

            // No candidates available, the node name is unique
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return nodeName;
            }

            var candidates = data.Tables[0];
            var uniqueIndex = 1;

            do
            {
                // Get matching duplicate
                var match = candidates.Select($"NodeName = '{SqlHelper.EscapeQuotes(nodeName)}'");
                if (match.Length == 0)
                {
                    // If not match, consider as unique
                    return nodeName;
                }

                // Prepare next candidate
                var uniqueString = $" ({uniqueIndex})";

                // Get the available length for the name without the unique suffix
                int availableLength = maxNameLength - uniqueString.Length;

                // Trim the name if necessary
                nodeName = (baseName.Length > availableLength) ? baseName.Substring(0, availableLength) : baseName;

                // Ensure unique suffix
                nodeName += uniqueString;

                // Maximal number of attemps reached
                if (uniqueIndex >= MAX_UNIQUE_INDEX)
                {
                    throw new Exception("[TreePathUtils.GetUniqueNodeName]: The maximum number of tries to get a unique node name was reached. Please validate URL settings to make sure the restrictions aren't too strict.");
                }

                uniqueIndex++;
            }
            while (true);
        }


        /// <summary>
        /// Gets the URL path display name
        /// </summary>
        /// <param name="urlPath">URL path</param>
        public static string GetUrlPathDisplayName(string urlPath)
        {
            // Parse the path
            string prefix;
            ParseUrlPath(ref urlPath, out prefix, null);

            // Add prefix from resource string
            if (prefix.StartsWith(URL_PREFIX_ROUTE, StringComparison.InvariantCultureIgnoreCase))
            {
                urlPath = ResHelper.GetString("URLPath.Route") + ": " + urlPath;
            }

            return urlPath;
        }


        /// <summary>
        /// Parses the prefix and default values out of the URL path
        /// </summary>
        /// <param name="urlPath">URL path to process</param>
        /// <param name="prefix">Returning the prefix</param>
        /// <param name="values">Collection of values</param>
        public static void ParseUrlPath(ref string urlPath, out string prefix, Hashtable values)
        {
            prefix = "";

            // If path starts with slash or empty, no prefix parsed
            if (String.IsNullOrEmpty(urlPath) || urlPath.StartsWith("/", StringComparison.Ordinal))
            {
                return;
            }

            // Remove the prefix
            int separatorIndex = values == null ? urlPath.LastIndexOf(":", StringComparison.Ordinal) : urlPath.IndexOf(':');
            if (separatorIndex > 0)
            {
                // Get the prefix
                prefix = urlPath.Substring(0, separatorIndex + 1);

                urlPath = urlPath.Substring(separatorIndex + 1);
            }

            // Values are not required
            if (values == null)
            {
                return;
            }

            separatorIndex = urlPath.IndexOf(':');

            while (separatorIndex > 0)
            {
                // Get the prefix
                string value = urlPath.Substring(0, separatorIndex);
                prefix += value;

                string[] couple = value.Trim('[', ']').Split('=');
                if (couple.Length == 2)
                {
                    values[couple[0].ToLowerInvariant()] = couple[1];
                }

                // Remove the prefix from URL path
                urlPath = urlPath.Substring(separatorIndex + 1);

                separatorIndex = urlPath.IndexOf(':');
            }
        }


        /// <summary>
        /// Returns the URL path without forbidden characters and trimmed to the maximal allowed length.
        /// </summary>
        /// <param name="urlPath">URL path</param>
        /// <param name="siteName">Site name</param>
        public static string GetSafeUrlPath(string urlPath, string siteName)
        {
            // If URLPath not specified, do not process
            if (string.IsNullOrEmpty(urlPath))
            {
                return string.Empty;
            }

            // Get special prefix if available
            string prefix;
            ParseUrlPath(ref urlPath, out prefix, null);

            // Validate the prefix length, keep one character for the path itself
            if (prefix.Length > MaxURLPathLength - 1)
            {
                throw new Exception("[TreePathUtils.GetSafeUrlPath]: The URL path prefix is longer then maximal length of the path.");
            }

            // Ensure correct URL path format
            urlPath = "/" + urlPath.TrimStart('/');

            // Remove the '.' char from the ends of the URL path segments to maintain valid URL
            urlPath = Regex.Replace(urlPath, "\\.\\.+", ".").TrimEnd('.');

            // Cut the URL path to the valid length
            var maxLength = MaxURLPathLength - prefix.Length;
            if (urlPath.Length > maxLength)
            {
                urlPath = urlPath.Substring(0, maxLength);
            }

            // Get additional allowed characters
            string allowedChars = AllowedURLPathValues;
            // Prefix is used especially for special format for MVC and route paths.
            if (!String.IsNullOrEmpty(prefix))
            {
                // Characters which can be used in the path
                allowedChars += ".*";
            }

            // Ensure safe parts of the URL path
            urlPath = "/" + urlPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => URLHelper.GetSafeUrlPart(p, siteName, RemoveDiacriticForSafeURLPath, UseLimitURLReplacements, allowedChars))
                .Join("/");

            // URL path together with prefix
            return prefix + urlPath;
        }


        /// <summary>
        /// Gets the safe unique URL path for the specified document. Safe URL path means that the forbidden characters are removed from the path parts, 
        /// the path is trimmed to the maximal allowed length and the parts correspond with level of nesting.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="cultureSuffix">Culture suffix to be used as a candidate for unique path suffix</param>
        public static string GetUniqueUrlPath(TreeNode node, string cultureSuffix = null)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node), "[TreePathUtils.GetUniqueUrlPath]: Missing node instance.");
            }

            var pathsUpdater = new DocumentPathsBuilder(node);
            var urlPath = pathsUpdater.GetUrlPath();

            return GetUniqueUrlPath(urlPath, node.DocumentID, node.NodeSiteName, cultureSuffix);
        }


        /// <summary>
        /// Replaces back slash in document name.
        /// </summary>
        /// <param name="documentName">Document name</param>
        /// <param name="siteName">Site name</param>
        public static string GetSafeDocumentName(string documentName, string siteName)
        {
            if (String.IsNullOrEmpty(documentName))
            {
                return documentName;
            }

            char replacement = URLHelper.ForbiddenCharactersReplacement(siteName);
            return documentName.Replace('/', replacement);
        }


        /// <summary>
        /// Returns true if path contains wildcard characters(% or _)
        /// </summary>
        /// <param name="path">Node alias path</param>
        public static bool PathContainsWildcardChars(string path)
        {
            return !String.IsNullOrEmpty(path) && WildcardCharsRegex.Match(path).Success;
        }

        #endregion


        #region "Path validation - Internal"

        /// <summary>
        /// Gets the safe unique URL path for the specified original URL path of a document on given site. Safe URL path means that the forbidden characters are removed from the path parts and 
        /// the path is trimmed to the maximal allowed length.
        /// </summary>
        /// <param name="urlPath">Original URL path</param>
        /// <param name="currentDocumentId">Document ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="cultureSuffix">Culture suffix to be used as a candidate for unique path suffix</param>
        internal static string GetUniqueUrlPath(string urlPath, int currentDocumentId, string siteName, string cultureSuffix = null)
        {
            // If URL path not specified, do not process
            if (string.IsNullOrEmpty(urlPath))
            {
                return string.Empty;
            }

            // Get safe URL path trimmend to the maximal length
            urlPath = GetSafeUrlPath(urlPath, siteName);

            // Validate site
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                throw new Exception("[TreePathUtils.GetUniqueUrlPath]: Site name '" + siteName + "' does not exist");
            }

            // Prepare base path
            var basePath = urlPath;
            var replacement = URLHelper.ForbiddenCharactersReplacement(siteName);
            basePath = Regex.Replace(basePath, "[_ " + replacement + "](\\d+)$", string.Empty);
            if (basePath == string.Empty)
            {
                basePath = urlPath;
            }

            // If culture suffix specified, start with this suffix first
            int uniqueIndex = string.IsNullOrEmpty(cultureSuffix) ? 1 : 0;

            // Check duplicity
            do
            {
                // Get the candidate
                var tree = new TreeProvider();
                var candidateId = tree.SelectNodes()
                               .All()
                               .TopN(1)
                               .Column("DocumentID")
                               .OnSite(site.SiteID)
                               .WhereEquals("DocumentUrlPath", urlPath)
                               .WhereNull("NodeLinkedNodeID")
                               .GetScalarResult(0);

                // The unique path is found
                if ((candidateId == 0) || (candidateId == currentDocumentId))
                {
                    return urlPath;
                }

                // Prepare next candidate
                var suffix = (uniqueIndex == 0) ? cultureSuffix : uniqueIndex.ToString();
                var uniqueString = $" {suffix}";

                // Get the available length for the URL path without the unique suffix
                int availableLength = MaxURLPathLength - uniqueString.Length;

                // Trim the path if necessary
                urlPath = (basePath.Length > availableLength) ? basePath.Substring(0, availableLength) : basePath;

                // Ensure safe path together with the unique suffix
                urlPath = GetSafeUrlPath(urlPath + uniqueString, siteName);

                // Maximal number of attemps reached
                if (uniqueIndex >= MAX_UNIQUE_INDEX)
                {
                    throw new Exception("[TreePathUtils.GetUniqueUrlPath]: The maximum number of tries to get a unique URL path was reached. Please validate URL settings to make sure the restrictions aren't too strict.");
                }

                uniqueIndex++;
            }
            while (true);
        }

        #endregion


        #region "Conversion functions"

        /// <summary>
        /// Returns the AliasPath equivalent for the given node ID.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public static string GetAliasPathByNodeId(int nodeId)
        {
            var node = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            return (node != null) ? node.NodeAliasPath : string.Empty;
        }


        /// <summary>
        /// Gets the base node record (CMS_Tree) by the alias path and site name.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="columns">List of columns to retrieve</param>
        public static DataSet GetNodeByAliasPath(string siteName, string aliasPath, string columns = null)
        {
            // Check site
            int siteId = SiteInfoProvider.GetSiteID(siteName);
            if (siteId <= 0)
            {
                throw new Exception("[TreePathUtils.GetNodeByAliasPath]: Site " + siteName + " does not exist!");
            }

            // Get the node AliasPath by selecting the base node record
            return DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                .Columns(SqlHelper.ParseColumnList(columns))
                                                .OnSite(siteId)
                                                .WhereEquals("NodeAliasPath", aliasPath)
                                                .Result;
        }


        /// <summary>
        /// Returns the node class name corresponding to the given document ID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static string GetClassNameByDocumentID(int documentId)
        {
            // Get node ID
            var nodeId = DocumentCultureDataInfoProvider.GetDocumentCultures()
                                                        .WithID(documentId)
                                                        .Columns("DocumentNodeID")
                                                        .TopN(1)
                                                        .GetScalarResult(0);
            if (nodeId <= 0)
            {
                return null;
            }

            // Get cached node data
            var nodeData = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);

            return nodeData?.NodeClassName;
        }


        /// <summary>
        /// Returns the node ID corresponding to the given aliasPath.
        /// </summary>
        /// <param name="siteName">Node site name</param>
        /// <param name="aliasPath">Alias path</param>
        public static int GetNodeIdByAliasPath(string siteName, string aliasPath)
        {
            // Get the data
            DataSet ds = GetNodeByAliasPath(siteName, aliasPath, "NodeID");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataRow nodeRow = ds.Tables[0].Rows[0];
                return (int)nodeRow["NodeID"];
            }

            return 0;
        }


        /// <summary>
        /// Returns the Node GUID equivalent for the given node ID.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public static Guid GetNodeGUIDByNodeId(int nodeId)
        {
            var node = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            return node?.NodeGUID ?? Guid.Empty;
        }


        /// <summary>
        /// Returns the Node ID equivalent for the given node GUID.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="siteName">Node site name</param>
        public static int GetNodeIdByNodeGUID(Guid nodeGuid, string siteName)
        {
            var siteId = SiteInfoProvider.GetSiteID(siteName);
            if (siteId <= 0)
            {
                throw new Exception("[TreePathUtils.GetNodeIDByNodeGUID]: Site name '" + siteName + "' not found.");
            }

            var data = DocumentNodeDataInfoProvider.GetDocumentNodes()
                                                   .TopN(1)
                                                   .Column("NodeID")
                                                   .WhereEquals("NodeGUID", nodeGuid)
                                                   .WhereEquals("NodeSiteID", siteId).Result;

            // If found, return the GUID
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                return ValidationHelper.GetInteger(data.Tables[0].Rows[0]["NodeID"], 0);
            }

            return 0;
        }


        /// <summary>
        /// Returns the Document ID equivalent for the given document GUID.
        /// </summary>
        /// <param name="documentGuid">Document GUID</param>
        /// <param name="siteName">Node site name</param>
        public static int GetDocumentIdByDocumentGUID(Guid documentGuid, string siteName)
        {
            // Get the site
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site == null)
            {
                throw new Exception("[TreePathUtils.GetDocumentIdByDocumentGUID]: Site name '" + siteName + "' not found.");
            }

            // Get the data
            var tree = new TreeProvider();
            var documentId = tree.SelectNodes()
                           .All()
                           .TopN(1)
                           .Column("DocumentID")
                           .WhereEquals("DocumentGUID", documentGuid)
                           .OnSite(siteName)
                           .GetScalarResult(0);

            return documentId;
        }


        /// <summary>
        /// Returns the site info of the specified original document ID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static SiteInfo GetDocumentSite(int documentId)
        {
            var tree = new TreeProvider();
            var siteId = tree.SelectNodes()
                             .All()
                             .TopN(1)
                             .Column("NodeSiteID")
                             .WhereEquals("DocumentID", documentId)
                             .WhereNull("NodeLinkedNodeID")
                             .GetScalarResult(0);

            return siteId > 0 ? SiteInfoProvider.GetSiteInfo(siteId) : null;
        }


        /// <summary>
        /// Returns the site info of the specified node.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public static SiteInfo GetNodeSite(int nodeId)
        {
            var node = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            if (node != null)
            {
                return SiteInfoProvider.GetSiteInfo(node.NodeSiteID);
            }

            return null;
        }


        /// <summary>
        /// Returns the parent node ID.
        /// </summary>
        /// <param name="nodeId">Node ID</param>
        public static int GetNodeParentId(int nodeId)
        {
            var node = DocumentNodeDataInfoProvider.GetDocumentNodeDataInfo(nodeId);
            return node?.NodeParentID ?? 0;
        }

        #endregion


        #region "Common methods"

        /// <summary>
        /// Gets number of slashes in path.
        /// </summary>
        /// <param name="path">Path to count the slashes.</param>
        internal static int GetPathSlashCount(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }

            return path.Split('/').Length - 1;
        }


        /// <summary>
        /// Returns URL extension used for friendly URLs for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetUrlExtension(string siteName = null)
        {
            if (siteName == null)
            {
                siteName = SiteContext.CurrentSiteName;
            }

            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSFriendlyURLExtension").Split(';')[0];
        }


        /// <summary>
        /// Returns files URL extension used for friendly URLs for specified site.
        /// </summary>
        /// <param name="siteName">Site name. If not specified current site name is used.</param>
        public static string GetFilesUrlExtension(string siteName = null)
        {
            if (siteName == null)
            {
                siteName = SiteContext.CurrentSiteName;
            }

            return SettingsKeyInfoProvider.GetFilesUrlExtension(siteName);
        }


        /// <summary>
        /// Returns true if the document alias should be automatically updated upon document name change.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool AutomaticallyUpdateDocumentAlias(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUpdateDocumentAlias");
        }


        /// <summary>
        /// Returns true if the documents should be accessible after change (if the alias should be automatically generated for them).
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool KeepChangedDocumentsAccessible(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSKeepChangedDocumentAccesible");
        }


        /// <summary>
        /// Returns true if the new documents should automatically use the name path for their URL path.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseNamePathForUrlPath(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSUseNamePathForUrlPath");
        }


        /// <summary>
        /// Returns true if document workflow cycle GUID shouldn't be changed when moving from publish/archive step to edit step.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool AllowPermanentPreviewLink(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSAllowPermanentPreviewLink");
        }


        /// <summary>
        /// Returns the alias from the given alias path.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        public static string GetAlias(string aliasPath)
        {
            if (aliasPath == null)
            {
                return null;
            }

            int lastSeparator = aliasPath.LastIndexOf("/", StringComparison.Ordinal);
            if (lastSeparator > 0)
            {
                return aliasPath.Substring(lastSeparator + 1);
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns the parent path for the specified path, with distance of parentLevel from the given path.
        /// </summary>
        /// <param name="path">Source path</param>
        /// <param name="parentLevel">Parent level (distance) from the given path</param>
        public static string GetParentPath(string path, int parentLevel)
        {
            while ((parentLevel > 0) && (path != "/"))
            {
                path = GetParentPath(path);
                parentLevel -= 1;
            }
            return path;
        }


        /// <summary>
        /// Returns the parent path for the specified path (any kind of path with "/" as a separator)
        /// </summary>
        /// <param name="path">Original path</param>
        public static string GetParentPath(string path)
        {
            return DataHelper.GetParentPath(path);
        }


        /// <summary>
        /// Returns the specified path level created from the given path.
        /// </summary>
        /// <param name="path">Source path</param>
        /// <param name="level">Level to return</param>
        public static string GetPathLevel(string path, int level)
        {
            if (string.IsNullOrEmpty(path) || (level <= 0))
            {
                return "/";
            }
            // Split to segments
            string[] segments = path.Trim('/').Split('/');
            string result = null;
            int currentLevel = 0;
            // Concatenate the segments
            foreach (string segment in segments)
            {
                result += "/" + segment;
                currentLevel += 1;
                if (currentLevel == level)
                {
                    break;
                }
            }

            return (string.IsNullOrEmpty(result)) ? "/" : result;
        }


        /// <summary>
        /// Returns true, if the class name is considered to be menu item type document.
        /// </summary>
        /// <param name="className">Class name to analyze</param>
        public static bool IsMenuItemType(string className)
        {
            if (String.IsNullOrEmpty(className))
            {
                return false;
            }

            DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(className);
            if (ci == null)
            {
                throw new DocumentTypeNotExistsException("Page type with '" + className + "' class name not found.");
            }

            return ci.ClassIsMenuItemType;
        }


        /// <summary>
        /// Returns the new document order settings for the given site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static DocumentOrderEnum NewDocumentOrder(string siteName)
        {
            if (DocumentActionContext.CurrentDocumentNewOrder != DocumentOrderEnum.Unknown)
            {
                return DocumentActionContext.CurrentDocumentNewOrder;
            }

            switch (SettingsKeyInfoProvider.GetValue(siteName + ".CMSNewDocumentOrder").ToLowerInvariant())
            {
                case "first":
                    return DocumentOrderEnum.First;

                case "last":
                    return DocumentOrderEnum.Last;

                case "alphabetical":
                    return DocumentOrderEnum.Alphabetical;

                default:
                    return DocumentOrderEnum.Unknown;
            }
        }

        #endregion


        #region "Inherited values methods"

        /// <summary>
        /// Gets the inherited value for a single document column.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="column">Column to retrieve value for</param>
        /// <param name="culture">Document culture</param>
        /// <param name="valueCondition">Where condition indicating if inherited value is present</param>
        internal static object GetNodeInheritedValueInternal(int siteId, string nodeAliasPath, string column, string culture = null, WhereCondition valueCondition = null)
        {
            // There is no upper tree for root document
            if (string.IsNullOrEmpty(nodeAliasPath) || (nodeAliasPath == "/"))
            {
                return null;
            }

            // Get upper tree query
            var data = GetUpperTreeQueryInternal(siteId, nodeAliasPath, culture)
                .Column(column)
                // Get the best match
                .TopN(1);

            // Ensure default condition for relevant parent value
            if (valueCondition == null)
            {
                valueCondition = new WhereCondition().WhereNotNull(column);
            }

            // Include parent value condition
            data.Where(valueCondition);

            // Get the value
            var result = data.Result;
            if (!DataHelper.DataSourceIsEmpty(result))
            {
                return data.Tables[0].Rows[0][0];
            }

            return null;
        }


        /// <summary>
        /// Gets data with candidates for inherited value of specified document columns.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="columns">Columns to retrieve value for</param>
        /// <param name="culture">Document culture</param>
        internal static DataSet GetNodeUpperTreeInternal(int siteId, string nodeAliasPath, string columns, string culture = null)
        {
            // There is no upper tree for root document
            if (string.IsNullOrEmpty(nodeAliasPath) || (nodeAliasPath == "/"))
            {
                return new DataSet();
            }

            // Get upper tree query
            var data = GetUpperTreeQueryInternal(siteId, nodeAliasPath, culture);

            if (columns != null)
            {
                // Ensure columns to process data hierarchy
                data.Columns(SqlHelper.MergeColumns(columns, "NodeLevel, DocumentCulture"));
            }

            return data.Result;
        }


        /// <summary>
        /// Returns inherited value.
        /// </summary>
        /// <param name="upperTree">Tree to inherit from</param>
        /// <param name="column">Name of column to get the value from</param>
        /// <param name="culture">Document culture code</param>
        /// <param name="valueChecker">Method to check the inherited value if relevant</param>
        /// <param name="dataEvaluator">Method to additionally evaluate the inherited value in context of other data if relevant</param>
        internal static object GetNodeInheritedValueInternal(DataSet upperTree, string column, string culture = null, Func<object, bool> valueChecker = null, Func<DataRow, bool> dataEvaluator = null)
        {
            if (DataHelper.DataSourceIsEmpty(upperTree))
            {
                return null;
            }

            // Ensure default value checker
            if (valueChecker == null)
            {
                valueChecker = value => value != DBNull.Value;
            }

            int bestMatchLevel = -1;
            object bestMatchValue = null;

            foreach (DataRow dr in upperTree.Tables[0].Rows)
            {
                // If the value was found in previous level, return the value from previous level
                int level = ValidationHelper.GetInteger(dr["NodeLevel"], 0);
                if ((bestMatchValue != null) && (bestMatchValue != DBNull.Value) && (level != bestMatchLevel))
                {
                    return bestMatchValue;
                }

                // Check value if relevant
                object value = DataHelper.GetDataRowValue(dr, column);
                if (valueChecker(value))
                {
                    // Evaluate value validity
                    if ((dataEvaluator == null) || dataEvaluator(dr))
                    {
                        // Check the culture, if matches the current document culture, return as best match
                        string cult = ValidationHelper.GetString(dr["DocumentCulture"], "");
                        if (cult.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return value;
                        }

                        bestMatchLevel = level;
                        bestMatchValue = value;
                    }
                }
            }

            return bestMatchValue;
        }


        /// <summary>
        /// Gets query to retrieve data for upper tree of a document
        /// </summary>
        /// <param name="siteId">Document site ID</param>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="culture">Document culture</param>
        internal static MultiDocumentQuery GetUpperTreeQueryInternal(int siteId, string nodeAliasPath, string culture)
        {
            var siteName = SiteInfoProvider.GetSiteName(siteId);

            var tree = new TreeProvider();
            var data = tree.SelectNodes()
                .CombineWithDefaultCulture(false)
                .OnSite(siteName)
                .Published(false)
                .Where(GetNodesOnPathWhereCondition(nodeAliasPath, true, false))
                .OrderByDescending("NodeLevel");

            // For all cultures get data from any culture version
            if (culture != TreeProvider.ALL_CULTURES)
            {
                // Include given culture
                var cults = new List<string> { culture };

                // Include the default culture to the results as well
                if (SiteInfoProvider.CombineWithDefaultCulture(siteName))
                {
                    cults.Add(CultureHelper.GetDefaultCultureCode(siteName));
                }

                data.Culture(cults.ToArray());
            }
            else
            {
                data.AllCultures();
            }

            return data;
        }
        

        /// <summary>
        /// Returns where condition for all nodes on the path to the given alias path.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>        
        /// <param name="includeRoot">Indicates whether root item should be included.</param>
        /// <param name="includeCurrent">Indicates whether item with the exact alias path should be included.</param>
        public static WhereCondition GetNodesOnPathWhereCondition(string aliasPath, bool includeRoot, bool includeCurrent)
        {
            var paths = GetNodeAliasPathsOnPath(aliasPath, includeRoot, includeCurrent);

            return new WhereCondition().WhereIn("NodeAliasPath", paths);
        }


        /// <summary>
        /// Gets list of node alias paths for all documents on given path.
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="includeRoot">Indicates if root path should be included</param>
        /// <param name="includeCurrent">Indicates if path of current document should be included</param>
        public static List<string> GetNodeAliasPathsOnPath(string aliasPath, bool includeRoot, bool includeCurrent)
        {
            var paths = new List<string>();

            if(string.IsNullOrEmpty(aliasPath))
            {
                return paths;
            }

            if (!includeCurrent)
            {
                aliasPath = GetParentPath(aliasPath);
            }

            do
            {
                // Add the path
                if ((aliasPath != "/") || includeRoot)
                {
                    paths.Add(aliasPath);
                }

                // If root, end building the list
                if (aliasPath == "/")
                {
                    break;
                }

                aliasPath = GetParentPath(aliasPath);
            } while (true);

            return paths;
        }

        #endregion


        #region "Navigation functions"

        /// <summary>
        /// Returns the menu caption.
        /// </summary>
        /// <param name="documentMenuCaption">Document menu caption</param>
        /// <param name="documentName">Document name</param>
        /// <returns>If regular menu caption not set, document name is returned instead</returns>
        public static string GetMenuCaption(string documentMenuCaption, string documentName)
        {
            return string.IsNullOrWhiteSpace(documentMenuCaption) ? documentName : documentMenuCaption;
        }


        /// <summary>
        /// Gets relative redirection URL based on given settings
        /// </summary>
        /// <param name="pi">Source page info</param>
        /// <param name="resolver">Macro resolver to use</param>
        public static string GetRedirectionUrl(PageInfo pi, MacroResolver resolver = null)
        {
            if (pi == null)
            {
                return string.Empty;
            }

            var siteName = pi.SiteName;
            var alias = pi.NodeAliasPath;
            var culture = pi.DocumentCulture;
            var url = pi.DocumentMenuRedirectUrl;
            var redirectToChild = pi.DocumentMenuRedirectToFirstChild;

            // Redirect to first child
            if (pi.DocumentMenuRedirectToFirstChild)
            {
                url = GetFirstChildUrl(siteName, alias, culture);
                redirectToChild = false;
            }

            return GetRedirectionUrl(siteName, alias, culture, url, redirectToChild, resolver);
        }


        /// <summary>
        /// Gets relative redirection URL based on given settings
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Document culture code</param>
        /// <param name="documentMenuRedirectUrl">Redirection URL</param>
        /// <param name="documentMenuRedirectToFirstChild">Indicates if document should redirect to first child document</param>
        /// <param name="resolver">Macro resolver to use</param>
        public static string GetRedirectionUrl(string siteName, string aliasPath, string cultureCode, string documentMenuRedirectUrl, bool documentMenuRedirectToFirstChild, MacroResolver resolver = null)
        {
            string url = documentMenuRedirectUrl;
            if (!string.IsNullOrEmpty(url))
            {
                // Ensure resolver
                if (resolver == null)
                {
                    resolver = MacroContext.CurrentResolver;
                }

                // Resolve macros
                if (resolver != null)
                {
                    url = resolver.ResolveMacros(url);
                }
            }
            else if (documentMenuRedirectToFirstChild)
            {
                url = GetFirstChildUrl(siteName, aliasPath, cultureCode);
            }

            return url;
        }


        /// <summary>
        /// Gets first child document URL
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Document culture code</param>
        public static string GetFirstChildUrl(string siteName, string aliasPath, string cultureCode)
        {
            // Get first child
            var tree = new TreeProvider();
            var child = tree.SelectNodes()
                            .OnSite(siteName)
                            .Path(aliasPath, PathTypeEnum.Children)
                            .Culture(cultureCode)
                            .OrderBy("NodeOrder", "DocumentName")
                            .TopN(1)
                            .Published()
                            .NestingLevel(1)
                            .FirstOrDefault();

            return (child != null) ? child.RelativeURL : string.Empty;
        }

        #endregion


        #region "URL methods"

        /// <summary>
        /// Returns document URL.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        [Obsolete("Use CMS.DocumentEngine.DocumentURLProvider.GetUrl(CMS.DocumentEngine.TreeNode) instead.")]
        public static string GetDocumentUrl(int documentId)
        {
            var tree = new TreeProvider();
            var node = DocumentHelper.GetDocument(documentId, tree);
            return (node != null) ? DocumentURLProvider.GetUrl(node) : string.Empty;
        }

        #endregion
    }
}