using System;
using System.IO;

using CMS.Search;

using Lucene.Net.Analysis;

namespace CMS.CustomSearchAnalyzer
{
    /// <summary>
    /// Custom search analyzer.
    /// </summary>
    public class CustomSearchAnalyzer : Analyzer, ISearchAnalyzer
    {
        /// <summary>
        /// Token stream.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="reader">Text reader</param>
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            return new LowerCaseTokenizer(reader);
        }
    }
}