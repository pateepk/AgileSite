using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Helper methods for alternative URLs.
    /// </summary>
    public class AlternativeUrlHelper : AbstractHelper<AlternativeUrlHelper>
    {
        internal const string ALTERNATIVE_URLS_CONSTRAINT_SETTINGS_KEY = "CMSAlternativeURLsConstraint";
        internal const string ALTERNATIVE_URLS_BLACKLIST_SETTINGS_KEY = "CMSAlternativeURLsExcludedURLs";
        internal const StringComparison ALTERNATIVE_URL_COMPARER = StringComparison.InvariantCultureIgnoreCase;

        private static readonly Lazy<IEnumerable<string>> mSystemUrls = new Lazy<IEnumerable<string>>(() => new List<string>
        {
            "kentico.",
            "kentico/",
            "KenticoCookiePolicyCheck",
            "cms/getfile/",
            "cms/getattachment/",
            "cmspages/",
            "cmsmodules/",
            "getmedia/",
            "getmetafile/",
            "getfile/",
            "getimage/",
            "getattachment/",
            $"{VirtualContext.VirtualContextPrefix.Trim('/')}/"
        });


        /// <summary>
        /// Returns main document of given <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <param name="alternativeUrl"></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="alternativeUrl"/> is <c>null</c>.</exception>
        public static TreeNode GetMainDocument(AlternativeUrlInfo alternativeUrl)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            return HelperObject.GetMainDocumentInternal(alternativeUrl);
        }


        /// <summary>
        /// Returns main document of given <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <param name="alternativeUrl"></param>
        protected virtual TreeNode GetMainDocumentInternal(AlternativeUrlInfo alternativeUrl)
        {
            return DocumentHelper.GetDocument(alternativeUrl.AlternativeUrlDocumentID, new TreeProvider());
        }


        /// <summary>
        /// Checks whether the given site contains any of the alternative URLs defined for the given page or its child pages in all cultures.
        /// </summary>
        /// <param name="node">Page for which alternative URLs will be checked.</param>
        /// <param name="siteId">ID of the site to check for alternative URLs.</param>
        /// <returns>True if the site has at least one identical alternative URL.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="siteId"/> is invalid.</exception>
        public static bool IsAlternativeUrlsSiteConflict(TreeNode node, int siteId)
        {
            return HelperObject.IsAlternativeUrlsSiteConflictInternal(node, siteId);
        }


        /// <summary>
        /// Checks whether the given site contains any of the alternative URLs defined for the given page or its child pages in all cultures.
        /// </summary>
        /// <param name="node">Page for which alternative URLs will be checked.</param>
        /// <param name="siteId">ID of the site to check for alternative URLs.</param>
        /// <returns>True if the site has at least one identical alternative URL.</returns>
        protected virtual bool IsAlternativeUrlsSiteConflictInternal(TreeNode node, int siteId)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var site = SiteInfoProvider.GetSiteInfo(siteId);
            if (site == null)
            {
                throw new ArgumentException("Site ID is invalid", nameof(siteId));
            }

            if (!node.Site.SiteIsContentOnly || !site.SiteIsContentOnly)
            {
                return false;
            }

            var allDocumentIds = new DocumentQuery()
                .Path(node.NodeAliasPath, PathTypeEnum.Section)
                .WhereEquals("NodeIsContentOnly", true)
                .OnSite(node.NodeSiteID)
                .Column("DocumentID");

            return AlternativeUrlInfoProvider.GetAlternativeUrls()
                    .From(new QuerySourceTable("CMS_AlternativeUrl", "Source"))
                    .Source(source => source.InnerJoin(new QuerySourceTable("CMS_AlternativeUrl", "Target"), "Source.AlternativeUrlUrl", "Target.AlternativeUrlUrl"))
                    .WhereIn("Source.AlternativeUrlDocumentID", allDocumentIds)
                    .WhereEquals("Target.AlternativeUrlSiteID", siteId)
                    .GetCount() > 0;
        }


        /// <summary>
        /// Returns <see cref="AlternativeUrlInfo"/> that has the same URL on the same site as given <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <param name="alternativeUrl">Alternative URL to find conflict with.</param>
        public static AlternativeUrlInfo GetConflictingAlternativeUrl(AlternativeUrlInfo alternativeUrl)
        {
            return HelperObject.GetConflictingAlternativeUrlInternal(alternativeUrl);
        }


        /// <summary>
        /// Returns <see cref="AlternativeUrlInfo"/> that has the same URL on the same site as given <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <param name="alternativeUrl">Alternative URL to find conflict with.</param>
        protected virtual AlternativeUrlInfo GetConflictingAlternativeUrlInternal(AlternativeUrlInfo alternativeUrl)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            return AlternativeUrlInfoProvider.GetAlternativeUrls()
                   .OnSite(alternativeUrl.AlternativeUrlSiteID)
                   .WhereEquals("AlternativeUrlUrl", alternativeUrl.AlternativeUrlUrl.NormalizedUrl)
                   .WhereNotEquals("AlternativeUrlID", alternativeUrl.AlternativeUrlID).FirstOrDefault();
        }


        /// <summary>
        /// Returns alternative URL that is in conflict with given <paramref name="node"/>.
        /// Alternative URL is in conflict with the <paramref name="node"/> when their URLs match.
        /// </summary>
        /// <param name="node">Page for which to find an alternative URL it might be in conflict with.</param>
        public static AlternativeUrlInfo GetConflictingAlternativeUrl(TreeNode node)
        {
            return HelperObject.GetConflictingAlternativeUrlInternal(node);
        }


        /// <summary>
        /// Returns alternative URL that is in conflict with given <paramref name="node"/>.
        /// Alternative URL is in conflict with the <paramref name="node"/> when their URLs match.
        /// </summary>
        /// <param name="node">Page for which to find an alternative URL it might be in conflict with.</param>
        protected virtual AlternativeUrlInfo GetConflictingAlternativeUrlInternal(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var normalizedUrl = NormalizeAlternativeUrl(DocumentURLProvider.GetUrl(node));

            return AlternativeUrlInfoProvider.GetAlternativeUrl(normalizedUrl, node.NodeSiteID);
        }


        /// <summary>
        /// Returns a page that is in conflict with given <paramref name="alternativeUrlInfo"/>.
        /// Page is in conflict with the <paramref name="alternativeUrlInfo"/> when their URLs match.
        /// </summary>
        /// <param name="alternativeUrlInfo">Alternative URL for which to find a page it might be in conflict with.</param>
        /// <returns>Page that is in conflict with <paramref name="alternativeUrlInfo"/>.</returns>
        public static TreeNode GetConflictingPage(AlternativeUrlInfo alternativeUrlInfo)
        {
            return HelperObject.GetConflictingPageInternal(alternativeUrlInfo);
        }


        /// <summary>
        /// Returns a page that is in conflict with given <paramref name="alternativeUrl"/>.
        /// Page is in conflict with the <paramref name="alternativeUrl"/> when their URLs match.
        /// </summary>
        /// <param name="alternativeUrl">Alternative URL for which to find a page it might be in conflict with.</param>
        /// <returns>Page that is in conflict with <paramref name="alternativeUrl"/>.</returns>
        /// <seealso cref="GetConflictingPage(AlternativeUrlInfo)"/>
        protected virtual TreeNode GetConflictingPageInternal(AlternativeUrlInfo alternativeUrl)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }
            InfoDataSet<DataClassInfo> pageTypesWithUrlPatterns = GetPageTypes();

            if (!pageTypesWithUrlPatterns.Any())
            {
                return null;
            }

            IEnumerable<ColumnDefinition> columnDefinitions = GetPageDefaultColumnDefinitions();
            var alternativeUrlSegments = alternativeUrl.AlternativeUrlUrl.NormalizedUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pageType in pageTypesWithUrlPatterns)
            {
                var whereCondition = IsUrlPatternSuspect(pageType, alternativeUrlSegments, columnDefinitions);
                if (whereCondition != null)
                {
                    var query = new DocumentQuery(pageType.ClassName).OnSite(alternativeUrl.AlternativeUrlSiteID).Where(whereCondition);
                    var conflictingPage = FindConflictingPage(query, alternativeUrlSegments);
                    if (conflictingPage != null)
                    {
                        return conflictingPage;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns a page that is in conflict with <see cref="AlternativeUrlInfo"/> represented by <paramref name="alternativeUrlSegments"/>.
        /// </summary>
        /// <param name="documentQuery">Document query used for retrieving pages that might be in conflict with <see cref="AlternativeUrlInfo"/> represented by <paramref name="alternativeUrlSegments"/>.</param>
        /// <param name="alternativeUrlSegments">
        /// Represents alternative URL to be checked against the pages.
        /// <see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> is split into segments e.g. alternative URL 'short/example/url' is split into ["short","example","url"]. 
        /// </param>
        /// <returns> Returns a page that s in conflict with alternative URL, otherwise returns null.</returns>
        /// <seealso cref="IsInCollision(TreeNode, string[])"/>
        protected virtual TreeNode FindConflictingPage(DocumentQuery documentQuery, string[] alternativeUrlSegments)
        {
            TreeNode conflictingPage = null;
            documentQuery.ForEachPage(q =>
            {
                // Find collision for published documents
                var documentsDataSet = q.TypedResult;
                conflictingPage = documentsDataSet.FirstOrDefault(d => IsInCollision(d, alternativeUrlSegments));
                if (conflictingPage != null)
                {
                    // Conflicting page is found, next page does not need to be fetched from database
                    throw new ActionCancelledException();
                }

                // Apply latest version data to published documents and find collision for latest version
                q.Properties.ApplyVersionData(documentsDataSet);
                conflictingPage = documentsDataSet.FirstOrDefault(d => IsInCollision(d, alternativeUrlSegments));
                if (conflictingPage != null)
                {
                    // Conflicting page is found, next page does not need to be fetched from database
                    throw new ActionCancelledException();
                }
            }, TreeProvider.PROCESSING_BATCH);

            return conflictingPage;
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="alternativeUrl"/> matches allowed alternative URL constraint defined in settings application.
        /// </summary>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo"/> to be match against the pattern constraint.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="alternativeUrl"/> is <c>null</c>.</exception>
        /// <exception cref="RegexMatchTimeoutException">Thrown when matching pattern constraint against <see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> takes too long.</exception>
        public static bool UrlMatchesConstraint(AlternativeUrlInfo alternativeUrl)
        {
            return HelperObject.UrlMatchesConstraintInternal(alternativeUrl);
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="alternativeUrl"/> matches allowed alternative URL constraint defined in settings application.
        /// </summary>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo"/> to be match against the pattern constraint.</param>
        /// <exception cref="RegexMatchTimeoutException">Thrown when matching pattern constraint against <see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> takes too long.</exception>
        protected virtual bool UrlMatchesConstraintInternal(AlternativeUrlInfo alternativeUrl)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            var regex = SettingsKeyInfoProvider.GetValue(ALTERNATIVE_URLS_CONSTRAINT_SETTINGS_KEY, alternativeUrl.AlternativeUrlSiteID);
            if (String.IsNullOrEmpty(regex))
            {
                return true;
            }

            try
            {
                return Regex.Match(alternativeUrl.AlternativeUrlUrl.NormalizedUrl, regex, RegexOptions.None, TimeSpan.FromSeconds(5d)).Success;
            }
            catch (RegexMatchTimeoutException ex)
            {
                EventLogProvider.LogException("Alternative URLs", "REGEXMATCH", ex, alternativeUrl.AlternativeUrlSiteID,
                    $"Execution timeout occurred. Alternative URL '{alternativeUrl.AlternativeUrlUrl}' took too long to evaluate against '{ex.Pattern}'. This may be a sign of an inefficient pattern.");

                throw;
            }
        }


        /// <summary>
        /// Returns conflicting URL when given <paramref name="alternativeUrl"/> matches any excluded URL defined in settings application or any system URL.
        /// </summary>
        /// <remarks>The <paramref name="alternativeUrl"/> is matched against global settings and site settings identified by <paramref name="siteId"/>.</remarks>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> to be matched against alternative URL constraints.</param>
        /// <param name="siteId">Identifier of the site to which the URL belongs.</param>
        public static string GetConflictingExcludedUrl(NormalizedAlternativeUrl alternativeUrl, int siteId)
        {
            return HelperObject.GetConflictingExcludedUrlInternal(alternativeUrl, siteId);
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="alternativeUrl"/> matches any excluded URL defined in settings application.
        /// </summary>
        /// <remarks>The <paramref name="alternativeUrl"/> is matched against global settings and site settings identified by <paramref name="siteId"/>.</remarks>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> to be matched against alternative URL constraints.</param>
        /// <param name="siteId">Identifier of the site to which the URL belongs.</param>
        public static bool MatchesAnyExcludedUrl(NormalizedAlternativeUrl alternativeUrl, int siteId)
        {
            return HelperObject.MatchesAnyExcludedUrlInternal(alternativeUrl, siteId);
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="alternativeUrl"/> matches any excluded URL defined in settings application.
        /// </summary>
        /// <remarks>The <paramref name="alternativeUrl"/> is matched against global settings and site settings identified by <paramref name="siteId"/>.</remarks>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> to be matched against alternative URL constraints.</param>
        /// <param name="siteId">Identifier of the site to which the URL belongs.</param>
        protected virtual bool MatchesAnyExcludedUrlInternal(NormalizedAlternativeUrl alternativeUrl, int siteId)
        {
            return !String.IsNullOrEmpty(GetConflictingExcludedUrl(alternativeUrl, siteId));
        }


        /// <summary>
        /// Returns conflicting URL when given <paramref name="alternativeUrl"/> matches any excluded URL defined in settings application or any system URL.
        /// </summary>
        /// <remarks>The <paramref name="alternativeUrl"/> is matched against global settings and site settings identified by <paramref name="siteId"/>.</remarks>
        /// <param name="alternativeUrl"><see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> to be matched against alternative URL constraints.</param>
        /// <param name="siteId">Identifier of the site to which the URL belongs.</param>
        protected virtual string GetConflictingExcludedUrlInternal(NormalizedAlternativeUrl alternativeUrl, int siteId)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            if (siteId <= 0)
            {
                throw new ArgumentException("Site ID is invalid", nameof(siteId));
            }

            var url = alternativeUrl.NormalizedUrl;

            var systemConflict = mSystemUrls.Value.FirstOrDefault(i => url.StartsWith(i, StringComparison.InvariantCultureIgnoreCase));
            if (!String.IsNullOrEmpty(systemConflict))
            {
                return systemConflict;
            }

            var excludedUrls = SettingsKeyInfoProvider.GetValue(ALTERNATIVE_URLS_BLACKLIST_SETTINGS_KEY, siteId);

            return ExcludedAlternativeUrlEvaluator.GetMatchingExcludedUrl(url, excludedUrls);
        }


        /// <summary>
        /// Ensures correct format of the <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Following rules are applied
        /// <para>
        /// Leading/trailing whitespace characters are trimmed.
        /// </para>
        /// <para>
        /// Leading tilde with slash or leading slash is removed.
        /// </para>
        /// <para>
        /// Multiple slashes are replaced by single slash.
        /// </para>
        /// <para>
        /// Trailing slash is removed.
        /// </para>
        /// <para>
        /// Query string and anchor is removed.
        /// </para>
        /// <para>
        /// Value is decoded.
        /// </para>
        /// </para>
        /// </remarks>
        /// <param name="alternativeUrl">Url in string to be normalized.</param>
        public static NormalizedAlternativeUrl NormalizeAlternativeUrl(string alternativeUrl)
        {
            return HelperObject.NormalizeAlternativeUrlInternal(alternativeUrl);
        }


        /// <summary>
        /// Ensures correct format of the <paramref name="alternativeUrl"/>.
        /// </summary>
        protected virtual NormalizedAlternativeUrl NormalizeAlternativeUrlInternal(string alternativeUrl)
        {
            if (String.IsNullOrEmpty(alternativeUrl))
            {
                return new NormalizedAlternativeUrl(alternativeUrl);
            }

            // Remove leading trailing whitespaces
            alternativeUrl = alternativeUrl.Trim();
            // Replace double slashes
            alternativeUrl = Regex.Replace(alternativeUrl, "/+", "/", RegexOptions.None);
            // Remove leading tilde
            alternativeUrl = alternativeUrl.TrimStart('~');
            // Remove leading slash
            alternativeUrl = alternativeUrl.TrimStart('/');
            // Remove trailing slash
            alternativeUrl = alternativeUrl.TrimEnd('/');

            // Remove query parameters
            var querystringIndex = alternativeUrl.IndexOf('?');
            if (querystringIndex >= 0)
            {
                alternativeUrl = alternativeUrl.Remove(querystringIndex);
            }

            // Remove anchor
            var anchorIndex = alternativeUrl.IndexOf('#');
            if (anchorIndex >= 0)
            {
                alternativeUrl = alternativeUrl.Remove(anchorIndex);
            }

            // Decode url segments
            alternativeUrl = HttpUtility.UrlDecode(alternativeUrl);

            return new NormalizedAlternativeUrl(alternativeUrl);
        }


        /// <summary>
        /// Returns human-readable document identification for given <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <returns>
        /// String value in form 'document name path' (document culture).
        /// </returns>
        internal static string GetDocumentIdentification(AlternativeUrlInfo alternativeUrl)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            var dataSet = DocumentCultureDataInfoProvider.GetDocumentCultures().Columns("DocumentNamePath", "DocumentCulture")
                                             .WhereEquals("DocumentID", alternativeUrl.AlternativeUrlDocumentID)
                                             .TopN(1)
                                             .Result;

            if (DataHelper.DataSourceIsEmpty(dataSet))
            {
                return null;
            }

            var documentNamePath = DataHelper.GetStringValue(dataSet.Tables[0].Rows[0], "DocumentNamePath");
            var documentCulture = DataHelper.GetStringValue(dataSet.Tables[0].Rows[0], "DocumentCulture");

            return GetDocumentIdentification(documentNamePath, documentCulture);
        }


        /// <summary>
        /// Returns human-readable document identification for given <paramref name="documentNamePath"/> and <paramref name="documentCulture"/>.
        /// </summary>
        /// <returns>
        /// String value in form 'document name path' (document culture).
        /// </returns>
        public static string GetDocumentIdentification(string documentNamePath, string documentCulture)
        {
            return HelperObject.GetDocumentIdentificationInternal(documentNamePath, documentCulture);
        }


        /// <summary>
        /// Returns human-readable document identification for given <paramref name="documentNamePath"/> and <paramref name="documentCulture"/>.
        /// </summary>
        /// <returns>
        /// String value in form 'document name path' (document culture).
        /// </returns>
        protected virtual string GetDocumentIdentificationInternal(string documentNamePath, string documentCulture)
        {
            return $"'{documentNamePath}' ({documentCulture})";
        }


        /// <summary>
        /// Checks whether <paramref name="dataClassInfo"/>'s URL pattern might be resolved to an alternative URL composed of <paramref name="alternativeUrlSegments"/>.
        /// Returns a <see cref="WhereCondition"/> to be used for a <see cref="DocumentQuery"/> to locate documents that might be in conflict with alternative URL.
        /// Returns null, if the <paramref name="dataClassInfo"/>'s URL pattern will not be in conflict with alternative URL.  
        /// </summary>
        /// <example>
        /// <para>
        /// URL pattern contains a constant part e.g. /Articles/{%NodeGuid%} and alternative URL is like /News/Example
        /// then we do not need to check pages of that page type.
        /// </para>
        /// <para>
        /// {%NodeName%}/{%DocumentName%}/News might be a suspect when alternative URL is /Awesome/Fresh/News.
        /// </para>
        /// <para>
        /// {%NodeId%}/{%NodeGuid%}/News cannot be a suspect when alternative URL is /Awesome/Fresh/News, because 'Awesome'
        /// cannot be converted to int supposed that {%NodeId%} macro is backed by integer type. {%NodeGuid%} is also incompatible with 'Fresh'.
        /// </para>
        /// </example>
        private WhereCondition IsUrlPatternSuspect(DataClassInfo dataClassInfo, string[] alternativeUrlSegments, IEnumerable<ColumnDefinition> pageColumnDefinitions)
        {
            // Create segments out of the URL
            var urlPatternSegments = dataClassInfo.ClassURLPattern.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // If pattern has different number of segments than alternative URL there is no point in checking as the URLs are different
            // Considering pattern '/Articles/{%Macro%}' vs alternative URL 'News/First/' to be a suspect just by the number of segments
            // The same goes for pattern '/Articles/{%Macro%}/' vs alternative URL 'News/First'
            if (urlPatternSegments.Length != alternativeUrlSegments.Length)
            {
                return null;
            }

            var pageTypeColumnDefinitions = ClassStructureInfo.GetClassInfo(dataClassInfo.ClassName).ColumnDefinitions;
            var classColumnDefinitionMap = pageColumnDefinitions.Union(pageTypeColumnDefinitions).ToDictionary(x => x.ColumnName, StringComparer.OrdinalIgnoreCase);

            var whereCondition = new WhereCondition();
            for (int i = 0; i < urlPatternSegments.Length; i++)
            {
                string segment = urlPatternSegments[i];

                // If segment is a macro then it is possible that the evaluation of the macro
                // might equal to the alternative URL's segment on the same index therefore next segment should be checked
                if (MacroProcessor.ContainsMacro(segment))
                {
                    var columnName = MacroProcessor.RemoveMacroBrackets(segment, out _);

                    // If column name is found within Page columns check if the alternative URL segment is compatible by type to Page's column type
                    // If not then it might be custom/complicated macro that needs to be resolved
                    if (classColumnDefinitionMap.TryGetValue(columnName, out ColumnDefinition columnDefinition))
                    {
                        // Converts value from alternative URL's segment to a column's type that it is backing the URL pattern's macro
                        if (TryConvertToDatabaseValue(alternativeUrlSegments[i], columnDefinition, out object convertedValue))
                        {
                            whereCondition.WhereEquals(columnName, convertedValue);
                        }
                        else
                        {
                            // If it cannot be converted, e.g. alternative URL segment contains string 'testString'
                            // and macro is {%NodeId%} that has int column definition then class is not a suspect anymore
                            return null;
                        }
                    }
                }
                else
                {
                    // Compare constant segment of the pattern with segment in alternative URL on the same index
                    if (!segment.Equals(alternativeUrlSegments[i], ALTERNATIVE_URL_COMPARER))
                    {
                        return null;
                    }
                }
            }

            return whereCondition;
        }


        /// <summary>
        /// Tries to convert the value from the alternative URL's segment to a type defined by <paramref name="columnDefinition"/>.
        /// </summary>
        private bool TryConvertToDatabaseValue(string value, ColumnDefinition columnDefinition, out object convertedValue)
        {
            convertedValue = null;
            bool parsed = false;
            switch (columnDefinition.ColumnType)
            {
                case Type stringType when stringType == typeof(string):
                    convertedValue = value;
                    return true;

                case Type guidType when guidType == typeof(Guid):
                    parsed = Guid.TryParse(value, out Guid guidValue);
                    convertedValue = guidValue;
                    return parsed;

                case Type intType when intType == typeof(int):
                    parsed = int.TryParse(value, out int intValue);
                    convertedValue = intValue;
                    return parsed;

                case Type floatType when floatType == typeof(float):
                    parsed = float.TryParse(value, out float floatValue);
                    convertedValue = floatValue;
                    return parsed;

                case Type longType when longType == typeof(long):
                    parsed = long.TryParse(value, out long longValue);
                    convertedValue = longValue;
                    return parsed;

                case Type decimalType when decimalType == typeof(decimal):
                    parsed = decimal.TryParse(value, out decimal decimalValue);
                    convertedValue = decimalValue;
                    return parsed;

                case Type dateTimeType when dateTimeType == typeof(DateTime):
                    parsed = DateTime.TryParse(value, out DateTime dateTimeValue);
                    convertedValue = dateTimeValue;
                    return parsed;

                case Type boolType when boolType == typeof(bool):
                    parsed = bool.TryParse(value, out bool boolValue);
                    convertedValue = boolValue;
                    return parsed;
            }

            return false;
        }


        /// <summary>
        /// Returns columns definitions for a page.
        /// </summary>
        private IEnumerable<ColumnDefinition> GetPageDefaultColumnDefinitions()
        {
            var node = ClassStructureInfo.GetClassInfo("cms.tree");
            var document = ClassStructureInfo.GetClassInfo(PredefinedObjectType.DOCUMENT);
            var sku = ClassStructureInfo.GetClassInfo(PredefinedObjectType.SKU);

            var columnDefinitions = node.ColumnDefinitions.Union(document.ColumnDefinitions).Union(sku.ColumnDefinitions);
            return columnDefinitions;
        }


        /// <summary>
        /// Returns true, if <paramref name="page"/> is in conflict with alternative URL represented by <paramref name="alternativeUrlSegments"/>.
        /// </summary>
        /// <param name="page">Page to be checked if it is in conflict with given alternative URL.</param>
        /// <param name="alternativeUrlSegments">
        /// Represents alternative URL to be checked against the <paramref name="page"/>.
        /// <see cref="AlternativeUrlInfo.AlternativeUrlUrl"/> is split into segments e.g. alternative URL 'short/example/url' is split into ["short","example","url"]. 
        /// </param>
        /// <seealso cref="FindConflictingPage(DocumentQuery, string[])"/>
        protected bool IsInCollision(TreeNode page, string[] alternativeUrlSegments)
        {
            string relativeUrl = DocumentURLProvider.GetUrl(page);

            // Remove '~/' from the relative URL so it can be compared to alternative URL
            var urlSegmentsToCheck = relativeUrl.TrimStart('~').TrimStart('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (urlSegmentsToCheck.Length == alternativeUrlSegments.Length)
            {
                for (int i = 0; i < urlSegmentsToCheck.Length; i++)
                {
                    if (!urlSegmentsToCheck[i].Equals(alternativeUrlSegments[i], ALTERNATIVE_URL_COMPARER))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns collection of page types with URL pattern 
        /// </summary>
        private InfoDataSet<DataClassInfo> GetPageTypes()
        {
            return CacheHelper.Cache(cs =>
            {
                var pageTypes = DataClassInfoProvider.GetClasses()
                   .WhereTrue("ClassIsContentOnly")
                   .WhereNotEmpty("ClassURLPattern")
                   .Columns("ClassName", "ClassURLPattern")
                   .TypedResult;

                cs.CacheDependency = CacheHelper.GetCacheDependency($"{DataClassInfo.OBJECT_TYPE}|all");

                return pageTypes;
            }, new CacheSettings(60, true, "AlternativeUrlHelper", "GetPageTypes"));
        }
    }
}
