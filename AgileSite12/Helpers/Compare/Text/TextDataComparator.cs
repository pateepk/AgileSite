using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Divide TextData into pieces according to their resemblance.
    /// </summary>
    public class TextDataComparator
    {
        #region "Variables"

        // Previous version
        private string mSourceLine = null;

        // Newer version
        private string mDestLine = null;

        // List with found longest matches 
        private ArrayList mLongestMatches = null;

        //private ArrayList mTags = null;

        #endregion


        #region "Constants"

        // Constant determining string doesn't have match in paired string
        private const int NOT_FOUND = -1;

        #endregion


        #region "Properties"

        /// <summary>
        /// String containing text from previous version.
        /// </summary>
        private string SourceLine
        {
            get
            {
                return mSourceLine;
            }
            set
            {
                mSourceLine = value;
            }
        }


        /// <summary>
        /// String containing text from newer version.
        /// </summary>
        private string DestLine
        {
            get
            {
                return mDestLine;
            }
            set
            {
                mDestLine = value;
            }
        }


        /// <summary>
        /// ArrayList with found longest possible string matches.
        /// </summary>
        public ArrayList LongestMatches
        {
            get
            {
                if (mLongestMatches == null)
                {
                    mLongestMatches = new ArrayList();
                }
                return mLongestMatches;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor using 2 strings.
        /// </summary> 
        /// <param name="src">Source string</param>
        /// <param name="dst">Destination string</param>
        public TextDataComparator(string src, string dst)
        {
            mSourceLine = src;
            mDestLine = dst;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Analyze 2 strings and divide them into text parts.
        /// </summary>
        public void DivideIntoParts()
        {
            //this.mTags = HTMLTextHelper.GetHTMLTagsList(this.SourceLine);

            // Analyze matches and divide string into text parts 
            LongestMatches.Add(GetLongestMatches(0, SourceLine.Length, 0, DestLine.Length));

            // Sort textparts using index in destination string 
            ComparisonTextPart.SortBy = ComparisonTextPartSortBy.DestIndex;
            LongestMatches.Sort();

            int processedID = 0;
            int srcParts = LongestMatches.Count;

            // Process destination string into textparts
            for (int i = 0; i < srcParts; i++)
            {
                ComparisonTextPart part = (ComparisonTextPart)LongestMatches[i];

                // Check if textpart has match in destination string
                if (part.DestIndex != NOT_FOUND)
                {
                    // Any part of string before found match 
                    if (processedID < part.DestIndex)
                    {
                        LongestMatches.Add(new ComparisonTextPart(DestLine.Substring(processedID, part.DestIndex - processedID), NOT_FOUND, processedID, ComparisonStatus.NoMatch));
                    }
                    processedID = part.DestIndex + part.Length;
                }
            }

            // Process possible end of destination string for additional textpart
            if (processedID != DestLine.Length)
            {
                LongestMatches.Add(new ComparisonTextPart(DestLine.Substring(processedID, DestLine.Length - processedID), NOT_FOUND, processedID, ComparisonStatus.NoMatch));
            }
        }

        #endregion


        #region "Private helper methods"

        /// <summary>
        /// Find longest match in 2 strings, add it to LongestMatches arraylist and recursively call itself to text before found match and text after found match
        /// <param name="srcStart">Index to start of first substring</param>
        /// <param name="srcEnd">Index to end of first substring</param>
        /// <param name="destStart">Index to start of second substring</param>
        /// <param name="destEnd">Index to end of second substring</param>
        /// </summary>
        private TextPart GetLongestMatches(int srcStart, int srcEnd, int destStart, int destEnd)
        {
            ComparisonStatus status = ComparisonStatus.Unknown;

            int counter_best = 0;
            int srcID = NOT_FOUND;
            int destID = NOT_FOUND;

            if ((destEnd - destStart > 0) && (srcEnd - srcStart > 0))
            {
                CompareInfo ci = CultureHelper.EnglishCulture.CompareInfo;
                string sourceLine = SourceLine;
                string destLine = DestLine;
                int match;

                // Go through the string characters of first string
                for (int i = srcStart; i < srcEnd; i++)
                {
                    if (srcEnd - i >= counter_best + 1)
                    {
                        string best_match = sourceLine.Substring(i, counter_best + 1);

                        int charID = destStart;

                        // Find the best match
                        while ((match = ci.IndexOf(destLine, best_match, charID, destEnd - charID, CompareOptions.Ordinal)) >= 0)
                        {
                            int destActualID = match + best_match.Length;
                            int srcActualID = i + best_match.Length;

                            // Find the best match from current location
                            while (((srcActualID < srcEnd) && (destActualID < destEnd)) && (destLine[destActualID] == sourceLine[srcActualID]))
                            {
                                destActualID++;
                                srcActualID++;
                            }

                            // If longer match is found store its index
                            int counter = srcActualID - i;
                            if (counter > counter_best)
                            {
                                counter_best = counter;
                                srcID = i;
                                destID = match;
                                best_match = sourceLine.Substring(srcID, counter);
                            }

                            charID = Math.Min(destActualID, destEnd);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // If any match was found, add it to ArrayList as match
            if (counter_best > 0)
            {
                status = ComparisonStatus.Match;

                // Analyze rest of the string before found match recursively
                if (srcStart != srcID)
                {
                    LongestMatches.Add(GetLongestMatches(srcStart, srcID, destStart, destID));
                }

                // Analyze rest of the string after found match recursively
                if (srcID + counter_best != srcEnd)
                {
                    LongestMatches.Add(GetLongestMatches(srcID + counter_best, srcEnd, destID + counter_best, destEnd));
                }
            }
            else
            {
                // No match was found, create TextPart removed from source string  
                status = ComparisonStatus.NoMatch;
                counter_best = Math.Abs(srcEnd - srcStart);
                srcID = srcStart;
            }

            return new ComparisonTextPart(SourceLine.Substring(srcID, counter_best), srcID, destID, status);
        }


        /// <summary>
        /// Compare HTML and clear text version.
        /// </summary>
        public void CompareWithHTMLVersion()
        {
            int index, counter = 0;
            int[] partTag;
            string part;

            // If strings are equal, do not continue 
            if (!SourceLine.EqualsCSafe(DestLine, true))
            {
                // Get list of included tags
                var tags = GetHTMLTagsList(SourceLine);

                // Process start of string
                part = SourceLine.Substring(0, ((int[])tags[0])[0]);
                if (part != String.Empty)
                {
                    LongestMatches.Add(new ComparisonTextPart(part, 0, 0, ComparisonStatus.Match));
                    counter += part.Length;
                }

                // Process strings before tags
                for (int i = 0; i < tags.Count; i++)
                {
                    partTag = (int[])tags[i];
                    part = SourceLine.Substring(partTag[0], partTag[1] - partTag[0] + 1);
                    LongestMatches.Add(new ComparisonTextPart(part, partTag[0], NOT_FOUND, ComparisonStatus.NoMatch));
                    if (i + 1 < tags.Count)
                    {
                        part = SourceLine.Substring(partTag[1] + 1, ((int[])tags[i + 1])[0] - (partTag[1] + 1));
                        if (!string.IsNullOrEmpty(part))
                        {
                            LongestMatches.Add(new ComparisonTextPart(part, partTag[1] + 1, counter, ComparisonStatus.Match));
                            counter += part.Length;
                        }
                    }
                }
                // Process end of string
                index = ((int[])tags[tags.Count - 1])[1] + 1;
                part = SourceLine.Substring(index);
                if (part != String.Empty)
                {
                    LongestMatches.Add(new ComparisonTextPart(part, index, DestLine.IndexOfCSafe(part, counter), ComparisonStatus.Match));
                }
            }
            else
            {
                LongestMatches.Add(new ComparisonTextPart(SourceLine, 0, 0, ComparisonStatus.Match));
            }
        }


        /// <summary>
        /// Gets list of HTML tags.
        /// </summary>
        /// <param name="text">Source text to inspect</param>
        /// <returns>List with ranges of HTML tags</returns>
        private static List<int[]> GetHTMLTagsList(string text)
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

        #endregion
    }
}