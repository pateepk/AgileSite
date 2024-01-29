using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class which provides operations regarding email builder zone placeholders.
    /// </summary>
    public static class WidgetZonePlaceholderHelper
    {
        /// <summary>
        /// Regular expression that enables to locate widget zone placeholder in a template code.
        /// </summary>
        private static readonly Regex placeholderRegEx = RegexHelper.GetRegex(@"\${2}(?<zoneid>[^$]+)\${2}");


        /// <summary>
        /// Replaces widget zone placeholders in format <c>$$zoneIdentifier$$</c> with values provided by <paramref name="getZoneContent"/> delegate 
        /// which accepts zone identifier and returns appropriate replacement string.
        /// </summary>
        /// <param name="templateCode">Email template code</param>
        /// <param name="getZoneContent">Delegate which returns the replacement text for the given zone identifier</param>
        /// <returns>Email template code with replaced widget zone placeholders.</returns>
        public static string ReplacePlaceholders(string templateCode, Func<string, string> getZoneContent)
        {
            return placeholderRegEx.Replace(templateCode, m =>
            {
                string zoneId = m.Groups["zoneid"].Value;
                return getZoneContent(zoneId);
            });
        }


        /// <summary>
        /// Returns a collection of duplicated zone placeholders in the given template code.
        /// </summary>
        /// <param name="templateCode">Email template code</param>
        public static IEnumerable<string> GetDuplicatedPlaceholders(string templateCode)
        {
            if (String.IsNullOrEmpty(templateCode))
            {
                return Enumerable.Empty<string>();
            }

            var matches = placeholderRegEx.Matches(templateCode);
            var duplicatedPlaceholders = matches.Cast<Match>()
                                           .GroupBy(m => m.Value)
                                           .Where(group => group.Count() > 1)
                                           .Select(group => group.Key);

            return duplicatedPlaceholders;
        }
    }
}
