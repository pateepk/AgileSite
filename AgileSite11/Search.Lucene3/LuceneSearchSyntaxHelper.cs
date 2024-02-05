using System;
using System.Globalization;
using System.Linq;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Search;
using CMS.Search.Lucene3;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

using WorldNet.Net;

using Version = Lucene.Net.Util.Version;

[assembly: RegisterImplementation(typeof(ISearchSyntaxHelper), typeof(LuceneSearchSyntaxHelper), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Helper methods for search condition syntax
    /// </summary>
    public class LuceneSearchSyntaxHelper : AbstractSearchSyntaxHelper
    {
        #region "Methods"

        /// <summary>
        /// Escapes the key words to be searched
        /// </summary>
        /// <param name="keywords">Keywords</param>
        public override string EscapeKeyWords(string keywords)
        {
            return QueryParser.Escape(keywords);
        }


        /// <summary>
        /// Adds ~ signs to each term to force fuzzy search.
        /// </summary>
        /// <param name="searchExpression">Search expression to transform</param>
        public override string TransformToFuzzySearch(string searchExpression)
        {
            var a = new StandardAnalyzer(Version.LUCENE_21);
            var ts = a.TokenStream(SearchFieldsConstants.CONTENT, new IO.StringReader(searchExpression));
            var termAtt = ts.AddAttribute<ITermAttribute>();

            string result = "";

            while (ts.IncrementToken())
            {
                var word = termAtt.Term;
                if (!word.EndsWithCSafe("~"))
                {
                    word += "~";
                }

                result += word + " ";
            }

            return result.Trim();
        }


        /// <summary>
        /// Creates the index searcher for synonym search.
        /// </summary>
        /// <param name="culture">Culture of the synonyms index</param>
        private IndexSearcher CreateSynonymSearcher(string culture)
        {
            // Try to load current culture. If no or invalid culture is specified, the EN culture is used.
            CultureInfo cultureInfo;
            if (string.IsNullOrEmpty(culture))
            {
                cultureInfo = CultureHelper.EnglishCulture;
            }
            else
            {
                try
                {
                    cultureInfo = new CultureInfo(culture);
                }
                catch
                {
                    cultureInfo = CultureHelper.EnglishCulture;
                }
            }

            if (cultureInfo != null)
            {
                // Find the synonym index in the _Synonyms directory (regional specific "en-us" language has the priority)
                string path = SearchIndexInfo.IndexPathPrefix + "_Synonyms\\[" + cultureInfo.Name + ".zip]\\";
                if (!IO.DirectoryInfo.New(path).Exists)
                {
                    // Try to find neutral culture "en"
                    path = SearchIndexInfo.IndexPathPrefix + "_Synonyms\\[" + cultureInfo.TwoLetterISOLanguageName + ".zip]\\";
                }

                if (IO.DirectoryInfo.New(path).Exists)
                {
                    try
                    {
                        return new LuceneIndexSearcher(new SearchDirectory(path));
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Expands given search expression with synonyms. If the data base of synonyms for given language is not found, searchExpression is returned without any modifications.
        /// </summary>
        /// <param name="searchExpression">Search expression which should be expanded with synonyms</param>
        /// <param name="culture">Language code of the search expression (if null, en-us is used)</param>
        public override string ExpandWithSynonyms(string searchExpression, string culture)
        {
            // Get the searcher in the WordNet index
            var wordNetSearcher = CreateSynonymSearcher(culture);
            if (wordNetSearcher != null)
            {
                // Do the synonym expansion
                var q = SynExpand.Expand(searchExpression, wordNetSearcher, null, SearchFieldsConstants.CONTENT, (float)SearchHelper.SynonymsWeight.Value);

                return q.ToString();
            }

            return searchExpression;
        }

        #endregion
    }
}
