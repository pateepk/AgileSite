using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using CMS.IO;
using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Contains various methods for text formatting and transformation.
    /// </summary>
    public class TextHelper
    {
        #region "Variables"

        private static Regex mRegExLineEnd;
        private static Regex mRegExLineEndWithSpaces;
        private static Regex mRegExWhiteSpaces;
        private static Regex mRegExLineBreak;

        private static Regex mRegExSplitPagesTags;
        private static Regex mRegExSplitPagesSentences;
        private static Regex mRegExToTitleCaseWords;

        private static Regex mRegExRemoveMultipleCommas;
        private static Regex mRegExRemoveMultipleCommasStartEnd;

        /// <summary>
        /// Default ellipsis for the limit length methods.
        /// </summary>
        public const string DEFAULT_ELLIPSIS = "...";

        /// <summary>
        /// Newline symbol.
        /// </summary>
        public static string NewLine = "\r\n";


        private static readonly CMSRegex RegexNumberSuffix = new CMSRegex("[0-9]+$");

        #endregion


        #region "Events"

        /// <summary>
        /// Represents the method that will handle an event that should be called before remove diacritics.
        /// </summary>
        /// <param name="text">Input text with diacritics</param>
        /// <param name="e">EventArgs</param>
        /// <returns>Returns false if original remove method should not be used.</returns>
        public delegate bool OnBeforeRemoveDiacriticsEventHandler(ref string text, EventArgs e);

        /// <summary>
        /// Occurs when the RemoveDiacritics method is called, returns false if original remove method should not be used.
        /// </summary>
        public static event OnBeforeRemoveDiacriticsEventHandler OnBeforeRemoveDiacritics;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether to encode merged text.
        /// </summary>
        public bool EncodeValues
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the regular expression for line break method.
        /// </summary>
        public static Regex RegExLineBreak
        {
            get
            {
                return mRegExLineBreak ?? (mRegExLineBreak = RegexHelper.GetRegex("(.*?)(<.*?>)|.*"));
            }
            set
            {
                mRegExLineBreak = value;
            }
        }


        /// <summary>
        /// Regular expression for the line ending.
        /// </summary>
        public static Regex RegExLineEnd
        {
            get
            {
                // Expression groups: none
                return mRegExLineEnd ?? (mRegExLineEnd = RegexHelper.GetRegex("\\r?\\n"));
            }
            set
            {
                mRegExLineEnd = value;
            }
        }


        /// <summary>
        /// Regular expression for the line ending with the spaces before and after.
        /// </summary>
        public static Regex RegExLineEndWithSpaces
        {
            get
            {
                // Expression groups: none
                return mRegExLineEndWithSpaces ?? (mRegExLineEndWithSpaces = RegexHelper.GetRegex("\\s*\\r?\\n\\s*", RegexHelper.DefaultOptions));
            }
            set
            {
                mRegExLineEndWithSpaces = value;
            }
        }


        /// <summary>
        /// Regular expression for white spaces.
        /// </summary>
        public static Regex RegeExWhiteSpaces
        {
            get
            {
                return mRegExWhiteSpaces ?? (mRegExWhiteSpaces = RegexHelper.GetRegex("\\s+"));
            }
            set
            {
                mRegExWhiteSpaces = value;
            }
        }


        /// <summary>
        /// Regular expression for the tag match for SplitPages method.
        /// </summary>
        public static Regex RegExSplitPagesTags
        {
            get
            {
                // Expression groups: none
                return mRegExSplitPagesTags ?? (mRegExSplitPagesTags = RegexHelper.GetRegex("<(?:/?)(?:\\w+)[^>]*>"));
            }
            set
            {
                mRegExSplitPagesTags = value;
            }
        }


        /// <summary>
        /// Regular expression for the tag match for SplitPages method.
        /// </summary>
        public static Regex RegExSplitPagesSentences
        {
            get
            {
                // Expression groups: none
                return mRegExSplitPagesSentences ?? (mRegExSplitPagesSentences = RegexHelper.GetRegex("[^\\.]*\\.*\\s*"));
            }
            set
            {
                mRegExSplitPagesSentences = value;
            }
        }


        /// <summary>
        /// Regular expression for the word match for ToTitleCase method.
        /// </summary>
        public static Regex RegExToTitleCaseWords
        {
            get
            {
                // Expression groups: none
                return mRegExToTitleCaseWords ?? (mRegExToTitleCaseWords = RegexHelper.GetRegex(@"\w+"));
            }
            set
            {
                mRegExToTitleCaseWords = value;
            }
        }


        /// <summary>
        /// Regular expression to identify multiple occurrences of comma (surrounded by white space).
        /// </summary>
        public static Regex RegExRemoveMultipleCommas
        {
            get
            {
                // Expression groups: none
                return mRegExRemoveMultipleCommas ?? (mRegExRemoveMultipleCommas = RegexHelper.GetRegex("(:?,\\s*)+"));
            }
            set
            {
                mRegExRemoveMultipleCommas = value;
            }
        }


        /// <summary>
        ///  Regular expression to identify comma at the beginning and at the end of a text.
        /// </summary>
        public static Regex RegExRemoveMultipleCommasStartEnd
        {
            get
            {
                // Expression groups: none
                return mRegExRemoveMultipleCommasStartEnd ?? (mRegExRemoveMultipleCommasStartEnd = RegexHelper.GetRegex("^(:?\\s*,\\s*)|(:?\\s*,\\s*)$"));
            }
            set
            {
                mRegExRemoveMultipleCommasStartEnd = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Splits the given string by a defined string separator
        /// </summary>
        /// <param name="text">Text to split</param>
        /// <param name="separator">Separator</param>
        /// <param name="minItems">If defined, the output array is filled with empty (null) items up to the given count</param>
        public static string[] SplitByString(string text, string separator, int minItems = 0)
        {
            var items = new List<string>();

            // If input is null, return an empty array
            if (text != null)
            {
                // If separator is not defined, return one item with the full text
                if (String.IsNullOrEmpty(separator))
                {
                    items.Add(text);
                }
                else
                {
                    int startIndex = 0;
                    int next;

                    // Add items until next separator found
                    while ((next = text.IndexOfCSafe(separator, startIndex)) >= 0)
                    {
                        items.Add(text.Substring(startIndex, next - startIndex));
                        startIndex = next + separator.Length;
                    }

                    // Add the last item
                    items.Add(text.Substring(startIndex, text.Length - startIndex));
                }
            }

            // Ensure required number of items
            while (items.Count < minItems)
            {
                items.Add(null);
            }

            return items.ToArray();
        }
        

        /// <summary>
        /// Checks if two text contents are equal. Ignores extra white spaces, and is case insensitive.
        /// </summary>
        /// <param name="text1">First text</param>
        /// <param name="text2">Second text</param>
        /// <param name="returnDifference">If true, the difference (remainders that don't match) are returned through original values</param>
        /// <param name="settings">If true, the input texts are normalized to avoid mismatch on whitespaces</param>
        /// <exception cref="ArgumentNullException">When text paramater is null</exception>
        public static bool ContentEquals(ref string text1, ref string text2, bool returnDifference = false, TextNormalizationSettings settings = null)
        {
            if (text1 == null)
            {
                throw new ArgumentNullException("text1");
            }

            if (text2 == null)
            {
                throw new ArgumentNullException("text2");
            }

            settings = settings ?? new TextNormalizationSettings();

            string wText1;
            string wText2;

            // Normalize if necessary
            if (settings.NormalizeWhiteSpaces)
            {
                // Normalize all white spaces
                wText1 = ReduceWhiteSpaces(text1, " ").Trim();
                wText2 = ReduceWhiteSpaces(text2, " ").Trim();
            }
            else if (settings.NormalizeLineEndings)
            {
                // Normalize only line endings
                wText1 = EnsureLineEndings(text1, Environment.NewLine).Trim();
                wText2 = EnsureLineEndings(text2, Environment.NewLine).Trim();
            }
            else
            {
                // Do not normalize
                wText1 = text1;
                wText2 = text2;
            }

            // Compare
            var result = wText1.EqualsCSafe(wText2, true);

            if (returnDifference)
            {
                int max = Math.Min(wText1.Length, wText2.Length);
                int i = 0;

                // Find first non-matching char
                while (i < max)
                {
                    if (wText1[i] != wText2[i])
                    {
                        break;
                    }

                    i++;
                }

                int matchingStartChars = i;

                // Find last non-matching char
                i = 1;

                if (matchingStartChars < max)
                {
                    while (i <= max)
                    {
                        if (wText1[wText1.Length - i] != wText2[wText2.Length - i])
                        {
                            break;
                        }

                        i++;
                    }
                }

                int matchingEndChars = i - 1;

                // Produce output
                var length1 = wText1.Length - matchingStartChars - matchingEndChars;
                if (length1 < 0)
                {
                    length1 = 0;
                }
                text1 = (matchingStartChars >= wText1.Length) ? "" : wText1.Substring(matchingStartChars, length1);
                
                var length2 = wText2.Length - matchingStartChars - matchingEndChars;
                if (length2 < 0)
                {
                    length2 = 0;
                }
                text2 = (matchingStartChars >= wText2.Length) ? "" : wText2.Substring(matchingStartChars, length2);
            }

            return result;
        }


        /// <summary>
        /// Removes diacritics from Latin characters, non-Latin characters are not changed.
        /// </summary>
        /// <param name="s">Input string</param>
        public static string RemoveDiacritics(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return s;
            }

            // Check whether remove diacritics should be pre-processed by custom code
            if (OnBeforeRemoveDiacritics != null)
            {
                if (!OnBeforeRemoveDiacritics(ref s, null))
                {
                    return s;
                }
            }

            // Remove invalid characters that prevent normalization
            s = RemoveCharacters(s, UnicodeCategory.OtherNotAssigned);

            s = s.Normalize(NormalizationForm.FormD);

            // Remove diacritics from normalized string
            s = RemoveCharacters(s, UnicodeCategory.NonSpacingMark);

            s = s.Normalize(NormalizationForm.FormC);

            s = RemoveCyrillicDiacritics(s);

            return s;
        }


        private static string RemoveCyrillicDiacritics(string s)
        {
            // The builder is allocated only when necessary
            StringBuilder result = null;
            
            for (int i = 0; i < s.Length; ++i)
            {
                if (IsLatinExtendedA(s[i]))
                {
                    var convertedCharacter = Encoding.UTF8.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(s[i].ToString()));
                    if (!convertedCharacter.Equals("?", StringComparison.Ordinal))
                    {
                        if (result == null)
                        {
                            result = new StringBuilder(s);
                        }
                        result.Remove(i, 1);
                        result.Insert(i, convertedCharacter);
                    }
                }
            }

            return result?.ToString() ?? s;
        }


        /// <summary>
        /// Indicates whether <paramref name="character"/> belongs to Unicode block Latin Extended-A.
        /// </summary>
        private static bool IsLatinExtendedA(char character)
        {
            // Latin Extended-A ranges from U+0100 to U+017F
            return (character >= 0x0100) && (character <= 0x017F);
        }
        

        /// <summary>
        /// Removes the invalid characters from the given string
        /// </summary>
        /// <param name="s">String to process</param>
        /// <param name="category">Unicode category to remove</param>
        private static string RemoveCharacters(string s, UnicodeCategory category)
        {
            var sb = new StringBuilder();

            foreach (char c in s)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != category)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Sets the specific line of the text to a new value
        /// </summary>
        /// <param name="text">Complete text</param>
        /// <param name="newValue">New line value</param>
        /// <param name="line">Line number to set (indexed from 1)</param>
        public static string SetLine(string text, string newValue, int line)
        {
            if (line <= 0)
            {
                return text;
            }

            text = EnsureLineEndings(text, "\n");

            List<string> lines = new List<string>(text.Split('\n'));

            // Ensure the number of lines
            while (lines.Count < line)
            {
                lines.Add("");
            }

            lines[line - 1] = newValue;

            return String.Join("\r\n", lines.ToArray());
        }


        /// <summary>
        /// Add specified break tag into the input text in specified index.
        /// </summary>
        /// <param name="inputText">Input text</param>
        /// <param name="size">Index, where text should be broken</param>
        /// <param name="breakTag">Break tag</param>
        public static string BreakLine(string inputText, int size, string breakTag)
        {
            // If size of current text is smaller than  required size return original text
            if (String.IsNullOrEmpty(inputText) || (inputText.Length < size) || (HTMLHelper.StripTags(inputText).Length < size))
            {
                return inputText;
            }

            // Get line break matches
            MatchCollection mc = RegExLineBreak.Matches(inputText);

            string outputText = String.Empty;
            int currentSize = size;
            int tagLength = 0;

            // Loop thru all matches
            foreach (Match m in mc)
            {
                // If current match is empty, break this loop
                if (String.IsNullOrEmpty(m.Groups[0].Value))
                {
                    break;
                }

                // Set tag value
                string currentTag = m.Groups[2].Value;

                // Set current text without html tags
                string currentText = m.Groups[!String.IsNullOrEmpty(currentTag) ? 1 : 0].Value;

                // Combine processed text with current text
                string tempText = outputText + currentText;

                // If combined text size is bigger than max size, break the line  
                if ((tempText.Length - tagLength) > currentSize)
                {
                    // Get break index
                    int breakIn = (currentSize - (outputText.Length - tagLength));
                    // Set new size
                    currentSize = (currentSize + size + (currentText.Length - breakIn));
                    // Break the line
                    tempText = outputText + currentText.Substring(0, breakIn) + breakTag + currentText.Substring(breakIn);
                }

                // Add original html tag if is defined
                outputText = tempText + currentTag;

                // Set length of the tags
                tagLength = tagLength + currentTag.Length;
            }

            return outputText;
        }


        /// <summary>
        /// Returns an array of text pages created from the input string according to the page size.
        /// </summary>
        /// <param name="text">Input string to create pages from</param>
        /// <param name="pageSize">Page size</param>
        public static string[] SplitPages(string text, int pageSize)
        {
            return SplitPages(text, pageSize, false);
        }


        /// <summary>
        /// Returns an array of text pages created from the input string according to the page size.
        /// </summary>
        /// <param name="text">Input string to create pages from</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="processTextDecoded">If true, the plain text within the given text is processed decoded (with entities resolved)</param>
        public static string[] SplitPages(string text, int pageSize, bool processTextDecoded)
        {
            ArrayList resultPages = new ArrayList();

            if (!string.IsNullOrEmpty(text))
            {
                if (pageSize > 0)
                {
                    ArrayList elements = new ArrayList();
                    ArrayList elementSizes = new ArrayList();

                    // Get the tags to separate logical blocks of text
                    MatchCollection matches = RegExSplitPagesTags.Matches(text);

                    string currentElement = "";
                    int currentElementSize = 0;
                    int lastIndex = 0;
                    Stack<string> tags = new Stack<string>();
                    string plainText;
                    int plainTextLength;

                    // Process found elements according to the page size
                    foreach (Match match in matches)
                    {
                        // Add text to the current element
                        plainTextLength = match.Index - lastIndex;
                        if (plainTextLength > 0)
                        {
                            plainText = text.Substring(lastIndex, plainTextLength);
                            if ((tags.Count > 0))
                            {
                                // Inside block
                                currentElement += plainText;
                                if (processTextDecoded)
                                {
                                    // Add decoded length
                                    currentElementSize += HttpUtility.HtmlDecode(plainText).Length;
                                }
                                else
                                {
                                    // Add normal length
                                    currentElementSize += plainText.Length;
                                }
                            }
                            else
                            {
                                if (currentElement != "")
                                {
                                    elements.Add(currentElement);
                                    elementSizes.Add(currentElementSize);
                                }

                                // Get the sentences
                                MatchCollection sentenceMatches = RegExSplitPagesSentences.Matches(plainText);
                                foreach (Match m in sentenceMatches)
                                {
                                    string sentence = m.ToString();
                                    elements.Add(sentence);

                                    int sentenceSize = processTextDecoded ? HttpUtility.HtmlDecode(sentence).Length : sentence.Length;

                                    elementSizes.Add(sentenceSize);
                                }
                                currentElement = "";
                                currentElementSize = 0;
                            }
                        }

                        // Process the tag
                        string matchString = match.ToString();

                        // If closed tag, add
                        if (matchString.EndsWithCSafe("/>"))
                        {
                            // If not inside block, add both blocks
                            if (tags.Count == 0)
                            {
                                if (currentElement != "")
                                {
                                    elements.Add(currentElement);
                                    elementSizes.Add(currentElementSize);
                                }
                                elements.Add(matchString);
                                elementSizes.Add(0);
                                currentElement = "";
                                currentElementSize = 0;
                            }
                            else
                            {
                                // Inside block, add the match to the current element
                                currentElement += matchString;
                            }
                        }
                        else
                        {
                            string tagName = match.Groups[2].ToString().ToLowerCSafe();
                            if (matchString.StartsWithCSafe("</")) // End tag, check the stack
                            {
                                currentElement += matchString;
                                // Remove all inner
                                if (tags.Contains(tagName))
                                {
                                    while ((tags.Peek() != tagName) && (tags.Count > 0))
                                    {
                                        tags.Pop();
                                    }
                                }

                                // If peek matches, remove
                                if (tags.Peek() == tagName)
                                {
                                    tags.Pop();
                                }

                                // Finish the block if closed
                                if (tags.Count == 0)
                                {
                                    elements.Add(currentElement);
                                    elementSizes.Add(currentElementSize);
                                    currentElement = "";
                                    currentElementSize = 0;
                                }
                            }
                            else // Start tag, add to the stack
                            {
                                if (tags.Count == 0)
                                {
                                    if (currentElement != "")
                                    {
                                        elements.Add(currentElement);
                                        elementSizes.Add(currentElementSize);
                                        currentElement = "";
                                        currentElementSize = 0;
                                    }
                                }
                                currentElement += matchString;
                                tags.Push(tagName);
                            }
                        }

                        lastIndex = match.Index + match.Length;
                    }

                    // Process remaining plain text
                    plainTextLength = text.Length - lastIndex;
                    if (plainTextLength > 0)
                    {
                        plainText = text.Substring(lastIndex, plainTextLength);
                        if ((tags.Count > 0))
                        {
                            // Inside block
                            currentElement += plainText;
                            if (processTextDecoded)
                            {
                                // Add decoded length
                                currentElementSize += HttpUtility.HtmlDecode(plainText).Length;
                            }
                            else
                            {
                                // Add normal length
                                currentElementSize += plainText.Length;
                            }

                            elements.Add(currentElement);
                            elementSizes.Add(currentElementSize);
                        }
                        else
                        {
                            if (currentElement != "")
                            {
                                elements.Add(currentElement);
                                elementSizes.Add(currentElementSize);
                            }

                            // Get the sentences
                            MatchCollection sentenceMatches = RegExSplitPagesSentences.Matches(plainText);
                            foreach (Match m in sentenceMatches)
                            {
                                string sentence = m.ToString();
                                elements.Add(sentence);

                                int sentenceSize = processTextDecoded ? HttpUtility.HtmlDecode(sentence).Length : sentence.Length;

                                elementSizes.Add(sentenceSize);
                            }
                        }
                    }

                    // Build back the pages
                    int halfPageSize = pageSize / 2;
                    int currentPageSize = 0;
                    StringBuilder currentPage = new StringBuilder(pageSize);
                    int index = 0;

                    foreach (string element in elements)
                    {
                        int elementSize = (int)elementSizes[index];

                        // Add element text to the current page
                        if ((currentPageSize + elementSize <= pageSize) || (currentPageSize <= halfPageSize))
                        {
                            currentPage.Append(element);
                            currentPageSize += elementSize;
                        }
                        // Create new page and add element text to it
                        else
                        {
                            // Add to the result
                            resultPages.Add(currentPage.ToString());

                            currentPageSize = elementSize;
                            currentPage.Length = 0;
                            currentPage.Append(element);
                        }

                        index++;
                    }

                    // Add last page to the result
                    if (currentPage.Length > 0)
                    {
                        resultPages.Add(currentPage.ToString());
                    }
                }
                else
                {
                    // Return input text as single page
                    resultPages.Add(text);
                }
            }

            return (String[])resultPages.ToArray(typeof(string));
        }


        /// <summary>
        /// Splits the camel cased input text into separate words.
        /// </summary>
        /// <param name="text">Camel cased text</param>
        public static IEnumerable<string> SplitCamelCase(string text)
        {
            var matches = Regex.Matches(text, "[A-Z]+(?=[A-Z][a-z]|[0-9]|$)|[A-Z][a-z]+|[0-9]+");
            return matches.Cast<Match>().Select(m => m.Value);
        }


        /// <summary>
        /// Limits the line length of given plain text.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="maxLength">Max line length</param>
        public static string EnsureMaximumLineLength(string text, int maxLength)
        {
            return EnsureMaximumLineLength(text, maxLength, "<span></span>", false);
        }


        /// <summary>
        /// Limits the line length of given plain text.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="maxLength">Max line length</param>
        /// <param name="separation">String which will be used as line separator</param>
        /// <param name="encode">If true, original string will be encoded (all the separators remain un-encoded)</param>
        public static string EnsureMaximumLineLength(string text, int maxLength, string separation, bool encode)
        {
            if (text == null)
            {
                return null;
            }

            if ((maxLength <= 0) || (maxLength >= text.Length))
            {
                return text;
            }

            // Separate the text
            if (!encode)
            {
                int index = maxLength;
                int textLength = text.Length;
                while (index < textLength)
                {
                    text = text.Insert(index, separation);
                    index += maxLength + separation.Length;
                    textLength += separation.Length;
                }
            }
            else
            {
                // Ensure line breaks for long texts - encode the original text, but do not encode the separator
                StringBuilder sb = new StringBuilder();
                int count = text.Length / maxLength;
                int i = 0;
                for (; i < count - 1; i++)
                {
                    sb.Append(HTMLHelper.HTMLEncode(text.Substring(i * maxLength, maxLength)), separation);
                }

                // Append the last chunk
                sb.Append(HTMLHelper.HTMLEncode(text.Substring(i * maxLength, text.Length - i * maxLength)));
                text = sb.ToString();
            }

            return text;
        }


        /// <summary>
        /// Limits the filename length (leaves extension).
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="maxLength">Max length</param>
        public static string LimitFileNameLength(string fileName, int maxLength)
        {
            if (!string.IsNullOrEmpty(fileName) && (fileName.Length > maxLength))
            {
                // Get extension
                string extension = Path.GetExtension(fileName);
                // Get name
                string name = Path.GetFileNameWithoutExtension(fileName);
                // Extension length
                int extLength = extension.Length;

                if (extLength > maxLength)
                {
                    throw new Exception(string.Format("[TextHelper.LimitFileNameLength] : File '{0}' extension is too long.", fileName));
                }

                fileName = LimitLength(name, maxLength - extension.Length, "");
                fileName += extension;
            }

            return fileName;
        }


        /// <summary>
        /// Limits the string length.
        /// </summary>
        /// <param name="text">Text to trim</param>
        /// <param name="maxLength">Max length</param>
        /// <param name="cutLocation">Location in the text where the cut of extra text occurs</param>
        public static string LimitLength(string text, int maxLength, CutTextEnum cutLocation)
        {
            return LimitLength(text, maxLength, null, false, cutLocation);
        }


        /// <summary>
        /// Limits the string length.
        /// </summary>
        /// <param name="text">Text to trim</param>
        /// <param name="maxLength">Max length</param>
        /// <param name="padString">Trim character</param>
        /// <param name="wholeWords">If true, the text won't be cut in the middle of the word</param>
        /// <param name="cutLocation">Location in the text where the cut of extra text occurs</param>
        public static string LimitLength(string text, int maxLength, string padString = DEFAULT_ELLIPSIS, bool wholeWords = false, CutTextEnum cutLocation = CutTextEnum.End)
        {
            if (text == null)
            {
                return null;
            }

            if (maxLength <= 0)
            {
                return text;
            }

            if (padString == null)
            {
                padString = DEFAULT_ELLIPSIS;
            }

            // Trim the text
            int trimmedLength = maxLength - padString.Length;
            int limitTrimmedLength = trimmedLength;
            if (trimmedLength <= 0)
            {
                return padString;
            }

            if (text.Length > maxLength)
            {
                if (cutLocation == CutTextEnum.Middle)
                {
                    // Cut in the middle
                    var halfLength = trimmedLength / 2;

                    return LimitLength(text, halfLength, "", wholeWords) + padString + LimitLength(text, halfLength, "", wholeWords, CutTextEnum.Start);
                }

                bool reversed = (cutLocation == CutTextEnum.Start);

                // Ensure the cut to the whole words
                if (wholeWords && Char.IsLetterOrDigit(GetCharAt(text, trimmedLength, reversed)))
                {
                    // Go to the beginning of the word
                    while ((trimmedLength > 0) && Char.IsLetterOrDigit(GetCharAt(text, trimmedLength - 1, reversed)))
                    {
                        trimmedLength--;
                    }

                    // Go to the end of the next word before (skip the white spaces and special chars)
                    while ((trimmedLength > 0) && !Char.IsLetterOrDigit(GetCharAt(text, trimmedLength - 1, reversed)))
                    {
                        trimmedLength--;
                    }

                    // We don't want to remove whole text if it is one long word
                    if (trimmedLength == 0)
                    {
                        trimmedLength = limitTrimmedLength;
                    }
                }

                if (reversed)
                {
                    text = text.Substring(text.Length - trimmedLength);
                    text = padString + text;
                }
                else
                {
                    text = text.Substring(0, trimmedLength);
                    text = text + padString;
                }
            }

            return text;
        }


        /// <summary>
        /// Gets the character at the given position of the text
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="index">Index of the character</param>
        /// <param name="reverse">If true, the character is retrieved in the reversed order (from the end)</param>
        private static char GetCharAt(string text, int index, bool reverse)
        {
            return reverse ? text[text.Length - index - 1] : text[index];
        }


        /// <summary>
        /// Ensures the specified line endings in the given text.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="lineEnd">New line ending</param>
        public static string EnsureLineEndings(string text, string lineEnd)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            return RegExLineEnd.Replace(text, lineEnd);
        }


        /// <summary>
        /// Reformats the C# or JS code so it is properly indented.
        /// </summary>
        /// <param name="text">Text to indent</param>
        public static string ReformatCode(string text)
        {
            return ReformatCode(text, 0, "  ");
        }


        /// <summary>
        /// Reformats the C# or JS code so it is properly indented.
        /// </summary>
        /// <param name="text">Text to indent</param>
        /// <param name="indent">Initial indent level</param>
        public static string ReformatCode(string text, int indent)
        {
            return ReformatCode(text, indent, "  ");
        }


        /// <summary>
        /// Reformats the C# or JS code so it is properly indented.
        /// </summary>
        /// <param name="text">Text to indent</param>
        /// <param name="indent">Initial indent level</param>
        /// <param name="indentString">Indentation string</param>
        public static string ReformatCode(string text, int indent, string indentString)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            StringBuilder sb = new StringBuilder();

            // Split the text into the lines
            string[] lines = text.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string l in lines)
            {
                string line = l.Trim();
                if (!String.IsNullOrEmpty(line))
                {
                    if (line.StartsWithCSafe("}"))
                    {
                        indent--;
                    }

                    // Add the indented line
                    if (sb.Length > 0)
                    {
                        sb.Append("\r\n");
                    }
                    sb.AppendIndent(indent, indentString);
                    sb.Append(line);

                    if (line.EndsWithCSafe("{"))
                    {
                        indent++;
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Ensures the specified line endings in the given text.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="level">Level of the indentation</param>
        public static string EnsureIndentation(string text, int level)
        {
            return EnsureIndentation(text, level, "  ");
        }


        /// <summary>
        /// Ensures the specified line endings in the given text.
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="level">Level of the indentation</param>
        /// <param name="indentString">New line ending</param>
        public static string EnsureIndentation(string text, int level, string indentString)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            // Prepare the indent
            StringBuilder indent = new StringBuilder(indentString.Length * level);
            for (int i = 0; i < level; i++)
            {
                indent.Append(indentString);
            }

            return indentString + RegExLineEndWithSpaces.Replace(text, "\r\n" + indent);
        }


        /// <summary>
        /// Encodes regex substitutions like $_, $+, ... so they don't apply as substitutes in the replacement string.
        /// </summary>
        /// <param name="text">Text to encode</param>        
        public static string EncodeRegexSubstitutes(string text)
        {
            if (text != null)
            {
                text = Regex.Replace(text, "\\$(?:[0-9{&`'+_])", "$$$+");
                text = Regex.Replace(text, "\\$$", "$$");
            }

            return text;
        }


        /// <summary>
        /// Replace whitespaces in input text to specified replacement.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="replacement">Replacement</param>
        public static string ReduceWhiteSpaces(string text, string replacement)
        {
            // Check whether input text is defined
            if (text != null)
            {
                // Replace whitespaces
                text = RegeExWhiteSpaces.Replace(text, replacement);
            }

            return text;
        }


        /// <summary>
        /// Removes specified word from the end of the text (if the text ends with the word).
        /// Note: Ignore case enabled.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="word">Text to be removed</param>        
        public static string TrimEndingWord(string text, string word)
        {
            // Removed whote spaces from the end
            text = text.TrimEnd();
            if (word != null)
            {
                if (text.EndsWithCSafe(word, true))
                {
                    text = text.Substring(0, text.Length - word.Length);
                }
            }

            return text;
        }


        /// <summary>
        /// Trims the number suffix from an identifier
        /// </summary>
        /// <param name="id">Base web part id (required)</param>
        public static string TrimNumberSuffix(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return id;
            }

            // Optimization to skip RegEx for id without number suffix
            if (!Char.IsDigit(id[id.Length - 1]))
            {
                return id;
            }

            return RegexNumberSuffix.Replace(id, "");
        }


        /// <summary>
        /// Converts the specified string to title case.
        /// </summary>
        /// <param name="text">Input text</param>
        public static string ToTitleCase(string text)
        {
            return RegExToTitleCaseWords.Replace(text, match =>
            {
                string word = match.Value;
                return char.ToUpper(word[0]) + word.Substring(1, word.Length - 1).ToLowerCSafe();
            });
        }


        /// <summary>
        /// Converts the first character to upper case.
        /// </summary>
        /// <param name="text">Text to process</param>
        public static string FirstLetterToUpper(string text)
        {
            if (!string.IsNullOrEmpty(text) && (text.Length > 0))
            {
                return Char.ToUpper(text[0]) + text.Substring(1);
            }

            return text;
        }


        /// <summary>
        /// Joins the given list of values with a given separator.
        /// </summary>
        /// <param name="separator">Separator</param>
        /// <param name="items">Items to join</param>
        public static string Join(string separator, IEnumerable items)
        {
            if (items == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // Join the items
            foreach (object item in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(item);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Joins the given list of values with a given separator.
        /// </summary>
        /// <param name="separator">Separator</param>
        /// <param name="values">Values</param>
        public static string Merge(string separator, params object[] values)
        {
            StringBuilder sb = new StringBuilder();

            // Join all values
            foreach (object value in values)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(value);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Joins the given list of values with a given separator. Each value is inserted into the final string only if not empty.
        /// </summary>
        /// <param name="separator">Separator</param>
        /// <param name="values">Values</param>
        public static string MergeIfNotEmpty(string separator, params object[] values)
        {
            // Join all values
            var nonEmptyValues = values
                .Select(value => ValidationHelper.GetString(value, null))
                .Where(value => !String.IsNullOrEmpty(value));
            
            return Join(separator, nonEmptyValues);
        }


        /// <summary>
        /// Removes multiple commas (i.e. transforms text "a,,b,," to "a, b").
        /// </summary>
        public static string RemoveMultipleCommas(string text)
        {
            if (text == null)
            {
                return null;
            }
            return RegExRemoveMultipleCommasStartEnd.Replace(RegExRemoveMultipleCommas.Replace(text, ", "), "");
        }


        /// <summary>
        /// If amount is equal to 1, expression using singular formatting string is returned, otherwise expression using plural formatting string is returned.
        /// </summary>
        /// <param name="amount">Amount to be used</param>
        /// <param name="singular">Singular formatting string. If formatting item {0} is not included, it is formatted as '[amount] [singular]', e.g.: 1 unit</param>
        /// /// <param name="plural">Plural formatting string. If formatting item {0} is not included, it is formatted as '[amount] [plural]', e.g.: 3 units</param>        
        public static string GetAmountText(int amount, string singular, string plural)
        {
            string result;

            if (amount == 1)
            {
                // Use singular
                if (singular.Contains("{0}"))
                {
                    result = singular;
                }
                else
                {
                    result = "{0} " + singular;
                }
            }
            else
            {
                // Use plural
                if (singular.Contains("{0}"))
                {
                    result = plural;
                }
                else
                {
                    result = "{0} " + plural;
                }
            }

            return string.Format(result, amount);
        }


        /// <summary>
        /// Performs bulk replace of given replacements
        /// </summary>
        /// <param name="text">Text to process</param>
        /// <param name="replacements">Key-Value pairs of replacements</param>
        /// <param name="encode">If true, replacements are encoded</param>
        /// <param name="useRegEx">If true, regular expressions are used to perform the replace</param>
        /// <param name="escapeRegExSubstitutions">If true, regex special characters are escaped so the replacement value is the value of the result, not the regular expression of replace.</param>
        public static string BulkReplace(string text, string[,] replacements, bool encode = false, bool useRegEx = true, bool escapeRegExSubstitutions = true)
        {
            string result = text;
            if (replacements != null)
            {
                for (int i = 0; i <= replacements.GetUpperBound(0); i++)
                {
                    string key = replacements[i, 0];
                    if (!string.IsNullOrEmpty(key))
                    {
                        string replacement = replacements[i, 1];

                        if (replacement == null)
                        {
                            continue;
                        }

                        // Encode the value
                        if (encode)
                        {
                            replacement = HTMLHelper.HTMLEncode(replacement);
                        }

                        if (useRegEx)
                        {
                            // Escape the substitutions
                            replacement = EncodeRegexSubstitutes(replacement);

                            // Resolve macro as the regular expression
                            result = Regex.Replace(result, key, replacement, CMSRegex.IgnoreCase);
                        }
                        else
                        {
                            // Resolve macro as the replacement (case-sensitive)
                            result = result.Replace(key, replacement);
                        }
                    }
                }
            }
            return result;
        }

        #endregion
    }
}