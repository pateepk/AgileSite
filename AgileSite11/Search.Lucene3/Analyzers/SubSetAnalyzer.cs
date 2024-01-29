using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Lucene.Net.Analysis;

using StringReader = CMS.IO.StringReader;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// SubSet smart search analyzer.
    /// </summary>
    internal class SubSetAnalyzer : Analyzer
    {
        #region "Variables"

        private readonly bool mIsSearch;
        private readonly bool mStartsWith = true;
        private int mMinimalLength = 1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the minimal length of set which should be indexed.
        /// </summary>
        public int MinimalLength
        {
            get
            {
                return mMinimalLength;
            }
            set
            {
                mMinimalLength = value;
            }
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// SubSetAnalyzer constructor.
        /// </summary>
        /// <param name="isSearch">Indicates whether analyzer is used for indexing or searching</param>
        /// <param name="startsWith">If is true the words are indexed similar to word*, if is false the words are indexed similar to *word*</param>
        /// <param name="minimalLength">Sets the minimal length of set which should be indexed</param>
        public SubSetAnalyzer(bool isSearch, bool startsWith, int minimalLength)
        {
            mIsSearch = isSearch;
            mStartsWith = startsWith;
            MinimalLength = minimalLength;
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Token stream method.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="reader">Text reader</param>
        public override TokenStream TokenStream(String fieldName, TextReader reader)
        {
            // Do not split word in search mode
            if (!mIsSearch)
            {
                // Get original text
                string text = reader.ReadToEnd();
                // Add sub sets to the original text
                text += GetSubsets(text);
                // Create new TextReader instance
                StringReader sr = new StringReader(text);
                reader = sr;
            }

            // Use white space tokenizer
            TokenStream result = new WhitespaceTokenizer(reader);
            // Use lowercase filter and return token stream
            return new LowerCaseFilter(result);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns the subsets for words in specified text.
        /// </summary>
        /// <param name="text">Text</param>
        private string GetSubsets(string text)
        {
            // Check whether text is defined
            if (!String.IsNullOrEmpty(text))
            {
                // Create string builder object
                StringBuilder sb = new StringBuilder(text.Length);

                // Get words
                MatchCollection col = SearchHelper.SubsetAnalyzerWordRegex.Matches(text);
                // Loop thru all words
                foreach (Match m in col)
                {
                    // Process each word
                    sb.Append(ProcessWord(m.Value, mStartsWith));
                    sb.Append(" " + m.Value + " ");
                }
                // Return final text
                return sb.ToString();
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns subsets for specified word.
        /// </summary>
        /// <param name="word">Word</param>
        /// <param name="startsWith">Indicates whether word should be processed as 'subset' or startsWith'</param>
        private string ProcessWord(string word, bool startsWith)
        {
            // Check whether text is defined
            if (!String.IsNullOrEmpty(word))
            {
                // Create string builder object
                StringBuilder sb = new StringBuilder(word.Length);
                // Get word length
                int length = word.Length;

                // Starts with mode
                if (startsWith)
                {
                    // Loop thru all starting subsets
                    for (int i = MinimalLength - 1; i < length - 1; i++)
                    {
                        sb.Append(" " + word.Substring(0, i + 1) + " ");
                    }
                }
                // Subset mode
                else
                {
                    // Use recursion to get set of sub sets
                    sb.Append(SubSets(word, 2, word.Length - 1));
                }

                // return all subsets for current word with dependence on selected mode
                return sb.ToString();
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns the subset for specified word, with dependence on recursive round.
        /// </summary>
        /// <param name="word">Word</param>
        /// <param name="round">Recursive round</param>
        /// <param name="length">Required subset length</param>
        private string SubSets(string word, int round, int length)
        {
            // Check whether text is defined
            if (!String.IsNullOrEmpty(word))
            {
                // Create string builder object
                StringBuilder sb = new StringBuilder(length * round);

                // Loop thru all current subsets
                for (int i = 0; i < round; i++)
                {
                    sb.Append(" " + word.Substring(i, length) + " ");
                }

                // Check whether in the word are available subsets and if so call next round
                if (length > MinimalLength)
                {
                    sb.Append(SubSets(word, round + 1, length - 1));
                }

                // Return subsets for current round
                return sb.ToString();
            }

            return String.Empty;
        }

        #endregion
    }
}