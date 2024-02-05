using System;
using System.IO;
using System.Text;

using Lucene.Net.Analysis;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Stemming smart search analyzer. Converts words into their root form.
    /// </summary>
    internal class StemmingAnalyzer : Analyzer
    {
        #region "Variables"

        private readonly Analyzer mBaseAnalyzer = null;

        #endregion


        #region "Constructors"

        public StemmingAnalyzer(Analyzer baseAnalyzer)
        {
            mBaseAnalyzer = baseAnalyzer;
        }

        #endregion

        /// <summary>
        /// Token stream method.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <param name="reader">Text reader</param>
        public override TokenStream TokenStream(String fieldName, TextReader reader)
        {
            // It is required by the stem filter to use lowercase tokenizer for it to work properly.
            return new PorterStemFilter(new LowerCaseFilter(mBaseAnalyzer.TokenStream(fieldName, reader)));
        }
    }
}