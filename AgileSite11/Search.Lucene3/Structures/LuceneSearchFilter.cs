using System;
using System.Collections;

using CMS.Base;

using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace CMS.Search.Lucene3
{
    /// <summary>
    /// Search alias path filter.
    /// </summary>
    [Serializable]
    internal class LuceneSearchFilter : Filter, ISearchFilter
    {
        #region "Properties"

        /// <summary>
        /// Field name.
        /// </summary>
        public string FieldName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Alias path.
        /// </summary>
        public string Match
        {
            get;
            protected set;
        }


        /// <summary>
        /// Filter condition
        /// </summary>
        public Func<string, string, bool> Condition
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Alias path filter constructor.
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="match">Match value</param>
        /// <param name="condition">Filter condition</param>
        public LuceneSearchFilter(string fieldName, string match, Func<string, string, bool> condition)
        {
            FieldName = fieldName;
            Match = match;

            if (condition == null)
            {
                condition = StringExtensions.EqualsCSafe;
            }

            Condition = condition;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns BitArray of filtered results.
        /// </summary>
        /// <param name="reader">Index reader</param>
        public override DocIdSet GetDocIdSet(IndexReader reader)
        {
            OpenBitSet bits = new OpenBitSet((reader.MaxDoc % 64 == 0 ? reader.MaxDoc / 64 : reader.MaxDoc / 64 + 1) * 64);

            // Use lower variant of the input values to make it case insensitive because terms are indexed in lower too
            var matchingTerm = new Term(FieldName.ToLowerCSafe(), Match.ToLowerCSafe());
            TermEnum enumerator = reader.Terms(matchingTerm);
            try
            {
                if (enumerator.Term == null)
                {
                    return bits;
                }

                TermDocs termDocs = reader.TermDocs();

                try
                {
                    do
                    {
                        // We have the correct term we were looking for
                        Term term = enumerator.Term;
                        // Validate term against the condition
                        if ((term != null) && Condition(term.Text, Match))
                        {
                            termDocs.Seek(term);
                            while (termDocs.Next())
                            {
                                bits.Set(termDocs.Doc);
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (enumerator.Next());
                }
                finally
                {
                    termDocs.Dispose();
                }
            }
            finally
            {
                enumerator.Dispose();
            }

            return bits;
        }

        #endregion
    }
}