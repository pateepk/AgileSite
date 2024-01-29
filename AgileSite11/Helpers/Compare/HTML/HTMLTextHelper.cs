using System;
using System.Collections.Generic;

namespace CMS.Helpers
{
    /// <summary>
    /// Internal static class used for inspecting Text for HTML code.
    /// </summary>
    internal static class HTMLTextHelper
    {
        /// <summary>
        /// Gets list of HTML tags.
        /// </summary>
        /// <param name="text">Source text to inspect</param>
        /// <returns>List with ranges of HTML tags</returns>
        public static List<int[]> GetHTMLTagsList(string text)
        {
            List<int> leadingChars = new List<int>();
            List<int[]> tags = new List<int[]>();
            char[] tagMarks = "<>".ToCharArray();
            int start = 0, match;

            // Searching for tags brackets
            while ((match = text.IndexOfAny(tagMarks, start)) != -1)
            {
                switch (text[match])
                {
                        // If open tag bracket found,push it to stack
                    case '<':
                        {
                            leadingChars.Add(match);
                        }
                        break;
                        // If close tag bracket appear, pop from stack or save range
                    case '>':
                        {
                            if (leadingChars.Count == 1)
                            {
                                tags.Add(new int[] { (int)leadingChars[0], match });
                                leadingChars.RemoveAt(0);
                            }
                            else if (leadingChars.Count > 1)
                            {
                                leadingChars.RemoveAt(leadingChars.Count - 1);
                            }
                        }
                        break;
                }
                start = match + 1;
            }

            return tags;
        }


        /// <summary>
        /// Method to check if following part of text is in HTML tag.
        /// </summary>
        /// <param name="index">Starting index of text part</param>
        /// <param name="tags">Array fit HTML tag ranges</param>
        /// <returns>True if text part is included in HTML tag,false otherwise</returns>
        public static bool isHTMLTag(int index, List<int[]> tags)
        {
            foreach (int[] tag in tags)
            {
                if (index >= tag[0] && index <= tag[1])
                {
                    return true;
                }
            }
            return false;
        }
    }
}