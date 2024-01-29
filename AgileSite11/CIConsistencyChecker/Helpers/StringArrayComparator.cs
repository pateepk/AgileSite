using System;
using System.Linq;

namespace CIConsistencyChecker
{
    /// <summary>
    /// Represents comparator of two strings arrays.
    /// </summary>
    internal static class StringArrayComparator
    {
        /// <summary>
        /// Compares two string arrays and returns first of their differences.
        /// </summary>
        public static string Compare(string[] originalFileContent, string[] newFileContent)
        {
            int shorterArrayLength = GetLengthOfShorterArray(originalFileContent, newFileContent);

            for (int i = 0; i < shorterArrayLength; i++)
            {
                var indexOfFirstDifference = GetIndexOfFirstDifference(originalFileContent[i], newFileContent[i]);

                if (indexOfFirstDifference >= 0)
                {
                    return String.Format(
                        "- Difference at ({0}, {1}):\n" +
                        "- Line from Original file   : {2}\n" +
                        "- Line from Serialized file : {3}\n" +
                        "- Difference                : {4}^",
                        i + 1, indexOfFirstDifference + 1,
                        originalFileContent[i],
                        newFileContent[i],
                        new String('-', indexOfFirstDifference));
                }
            }
            return "- Text content is same, but files have probably different encoding.";
        }


        /// <summary>
        /// Returns index of first difference in given strings.
        /// If strings are equal, returns -1.
        /// </summary>
        /// <param name="s1">First string.</param>
        /// <param name="s2">Second string.</param>
        private static int GetIndexOfFirstDifference(string s1, string s2)
        {
            if (s1 == s2)
            {
                return -1;
            }

            // Enumerates through strings s1 and s2 and comparers char by char (c1, c2)
            // Counts while characters are equal
            return s1.Zip(s2, (c1, c2) => c1 == c2).TakeWhile(b => b).Count();
        }


        /// <summary>
        /// Returns shorter length from given arrays.
        /// </summary>
        private static int GetLengthOfShorterArray(string[] originalFileContent, string[] newFileContent)
        {
            return Math.Min(originalFileContent.Length, newFileContent.Length);
        }
    }
}
