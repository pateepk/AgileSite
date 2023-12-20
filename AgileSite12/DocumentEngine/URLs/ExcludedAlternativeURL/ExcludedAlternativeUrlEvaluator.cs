using System;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Class for evaluating excluded alternative URLs in settings.
    /// </summary>
    internal static class ExcludedAlternativeUrlEvaluator
    {
        private const string WILDCARD_CHARACTER = "*";

        /// <summary>
        /// Processes <paramref name="excludedUrls"/> from settings and returns excluded URL that matches given <paramref name="alternativeUrl"/>.
        /// </summary>
        /// <param name="alternativeUrl">Alternative URL for which matching excluded URL is found.</param>
        /// <param name="excludedUrls">Excluded URLs to which alternative URL is matched.</param>
        /// <returns>Matched excluded URL or <c>String.Empty</c> if no match is found.</returns>
        public static string GetMatchingExcludedUrl(string alternativeUrl, string excludedUrls)
        {
            if (alternativeUrl == null)
            {
                throw new ArgumentNullException(nameof(alternativeUrl));
            }

            if (String.IsNullOrEmpty(excludedUrls))
            {
                return String.Empty;
            }

            var excludedUrlsArray = excludedUrls.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var url in excludedUrlsArray)
            {
                if (IsWildcardMatch(alternativeUrl, url.Trim().TrimStart('/')))
                {
                    return url;
                }
            }

            return String.Empty;
        }


        private static bool IsWildcardMatch(string value, string pattern)
        {
            if (value == null || pattern == null)
            {
                return false;
            }

            // No pattern optimization
            if (pattern == String.Empty)
            {
                return String.IsNullOrEmpty(value);
            }

            // Wildcard only optimization
            if (pattern.Equals(WILDCARD_CHARACTER, StringComparison.Ordinal))
            {
                return true;
            }

            var leadingWildcard = pattern.StartsWith(WILDCARD_CHARACTER, StringComparison.Ordinal);
            var trailingWildcard = pattern.EndsWith(WILDCARD_CHARACTER, StringComparison.Ordinal);


            // Contains
            if (leadingWildcard && trailingWildcard)
            {
                return value.IndexOf(pattern.Substring(1, pattern.Length - 2), AlternativeUrlHelper.ALTERNATIVE_URL_COMPARER) >= 0;
            }

            // EndsWith
            if (leadingWildcard)
            {
                return value.EndsWith(pattern.Substring(1), AlternativeUrlHelper.ALTERNATIVE_URL_COMPARER);
            }

            // StartsWith
            if (trailingWildcard)
            {
                return value.StartsWith(pattern.Substring(0, pattern.Length - 1), AlternativeUrlHelper.ALTERNATIVE_URL_COMPARER);
            }


            // Exact match
            return value.Equals(pattern, AlternativeUrlHelper.ALTERNATIVE_URL_COMPARER);
        }
    }
}
