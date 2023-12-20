using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Taxonomy
{
    /// <summary>
    /// Tag helper class.
    /// </summary>
    public static class TagHelper
    {
        #region "Variables"

        /// <summary>
        /// Regex for detection tags separated with comma or space from string.
        /// </summary>
        private static readonly CMSRegex mTagsRegex = new CMSRegex(@"^(?:[ ,]*(""[^""]*""|[^ ,]+))*[ ,]*$");

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns hash table of tags from tags string.
        /// </summary>
        ///<param name="tags">Tags string separated with comma or space</param>
        public static ISet<string> GetTags(string tags)
        {
            Match m = mTagsRegex.Match(tags);

            // Use hashset to avoid multiple tags
            ISet<string> tagSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var capture in m.Groups[1].Captures)
            {
                var tag = capture.ToString().Trim('"').Trim();
                tagSet.Add(tag);
            }

            return tagSet;
        }


        /// <summary>
        /// Returns tags prepared for save for document
        /// (alphabetical sorted and with maximal length 250)
        /// </summary>
        /// <param name="tags">Unsorted tags string</param>
        public static string GetTagsForSave(string tags)
        {
            Match m = mTagsRegex.Match(tags);

            // Get tags array
            if (m.Success)
            {
                // Fill hash table with tags
                Hashtable tagTable = new Hashtable();
                for (int i = 0; i < m.Groups[1].Captures.Count; i++)
                {
                    string tagValue = m.Groups[1].Captures[i].ToString();
                    if (tagValue.Length > 250)
                    {
                        tagValue = tagValue.Substring(0, 250);
                    }
                    tagValue = tagValue.Trim('"').Trim();
                    if (!tagTable.Contains(tagValue.ToLowerCSafe()))
                    {
                        tagTable[tagValue.ToLowerCSafe()] = tagValue;
                    }
                }

                // Sort final tags
                ArrayList tagSorted = new ArrayList();
                tagSorted.AddRange(tagTable.Values);
                tagSorted.Sort();

                // Create tags string
                StringBuilder builder = new StringBuilder();
                foreach (string tag in tagSorted)
                {
                    if (tag.Contains(" "))
                    {
                        builder.Append("\"" + tag + "\", ");
                    }
                    else
                    {
                        builder.Append(tag + ", ");
                    }
                }

                return builder.ToString().TrimEnd().TrimEnd(',').Replace("|", "-").Replace("%", "-");
            }

            return string.Empty;
        }

        #endregion
    }
}