using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    internal class DocumentSearchHelper
    {
        /// <summary>
        /// Aggregates all allowed index settings into one instance of <see cref="SearchIndexSettingsInfo"/>.
        /// </summary>
        /// <param name="node">Tree node</param>
        /// <param name="settings">Search index settings</param>
        public static SearchIndexSettingsInfo GetIncludedSettings(TreeNode node, SearchIndexSettings settings)
        {
            var include = new SearchIndexSettingsInfo();

            // Combine all matched content
            foreach (var sisi in settings.Items.Values)
            {
                // Check class name and path
                if ((sisi.Type == SearchIndexSettingsInfo.TYPE_ALLOWED) && sisi.MatchClassNames(node.NodeClassName) && sisi.MatchPath(node.NodeAliasPath))
                {
                    // Get any enabled setting
                    include.IncludeBlogs |= sisi.IncludeBlogs;
                    include.IncludeForums |= sisi.IncludeForums;
                    include.IncludeMessageCommunication |= sisi.IncludeMessageCommunication;
                    include.IncludeCategories |= sisi.IncludeCategories;
                    include.IncludeAttachments |= sisi.IncludeAttachments;
                }
            }
            return include;
        }


        /// <summary>
        /// Joins collection of values to single string. Values are separated by a space.
        /// </summary>
        /// <param name="values">Values to join.</param>
        public static string GetSearchContent<T>(ISet<T> values)
        {
            return string.Join(" ", values).Trim();
        }
    }
}
