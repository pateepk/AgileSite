using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Contains <see cref="ILuceneSearchDocument"/> related helper methods.
    /// </summary>
    public class LuceneSearchDocumentHelper : AbstractHelper<LuceneSearchDocumentHelper>
    {
        /// <summary>
        /// Names of initialization parameters to omit when performing search document conversion.
        /// </summary>
        private static readonly HashSet<string> InitializationParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SearchFieldsConstants.TYPE,
                SearchFieldsConstants.ID,
                SearchFieldsConstants.SITE,
                SearchFieldsConstants.CREATED,
                SearchFieldsConstants.CULTURE,
                SearchFieldsConstants.INDEX
            };


        /// <summary>
        /// Converts object of general purpose type <see cref="SearchDocument"/> to Lucene search specific <see cref="ILuceneSearchDocument"/>.
        /// </summary>
        /// <param name="searchDocument">Search document to convert.</param>
        /// <returns>Lucene search document created from search document.</returns>
        /// <remarks>
        /// The default system implementation converts all search document field values to string and adds them to <see cref="ILuceneSearchDocument"/>
        /// by calling <see cref="ILuceneSearchDocument.AddGeneralField"/>. Initialization fields of search document are added via <see cref="ILuceneSearchDocument.Add"/> call.
        /// </remarks>
        /// <seealso cref="SearchDocumentExtensions.Initialize(SearchDocument,SearchDocumentParameters)"/>
        public static ILuceneSearchDocument ToLuceneSearchDocument(SearchDocument searchDocument)
        {
            return HelperObject.ToLuceneSearchDocumentInternal(searchDocument);
        }


        /// <summary>
        /// Converts object of general purpose type <see cref="SearchDocument"/> to Lucene search specific <see cref="ILuceneSearchDocument"/>.
        /// </summary>
        /// <param name="searchDocument">Search document to convert.</param>
        /// <returns>Lucene search document created from search document.</returns>
        protected virtual ILuceneSearchDocument ToLuceneSearchDocumentInternal(SearchDocument searchDocument)
        {
            ILuceneSearchDocument luceneDocument = Service.Resolve<ILuceneSearchDocument>();

            luceneDocument.Add(SearchFieldsConstants.TYPE, (string)searchDocument.GetValue(SearchFieldsConstants.TYPE));
            luceneDocument.Add(SearchFieldsConstants.ID, (string)searchDocument.GetValue(SearchFieldsConstants.ID));
            luceneDocument.Add(SearchFieldsConstants.SITE, DataHelper.GetNotEmpty((string)searchDocument.GetValue(SearchFieldsConstants.SITE), SearchHelper.INVARIANT_FIELD_VALUE).ToLowerInvariant());
            luceneDocument.Add(SearchFieldsConstants.CREATED, SearchValueConverter.DateToString((DateTime)searchDocument.GetValue(SearchFieldsConstants.CREATED)));
            luceneDocument.Add(SearchFieldsConstants.CULTURE, DataHelper.GetNotEmpty((string)searchDocument.GetValue(SearchFieldsConstants.CULTURE), SearchHelper.INVARIANT_FIELD_VALUE).ToLowerInvariant());
            luceneDocument.Add(SearchFieldsConstants.INDEX, (string)searchDocument.GetValue(SearchFieldsConstants.INDEX));

            foreach (var name in searchDocument.Names.Where(name => !InitializationParameterNames.Contains(name)))
            {
                var value = searchDocument.GetValue(name);
                if (name.Equals("DocumentCategoryIDs", StringComparison.OrdinalIgnoreCase))
                {
                    value = JoinValues((IEnumerable<int>)value);
                }
                else if (name.Equals("DocumentCategories", StringComparison.OrdinalIgnoreCase))
                {
                    value = JoinValues((IEnumerable<string>)value);
                }

                luceneDocument.AddGeneralField(name, value, searchDocument.GetStore(name), searchDocument.GetTokenize(name));
            }

            return luceneDocument;
        }


        private string JoinValues<T>(IEnumerable<T> values)
        {
            return String.Join(" ", values).Trim();
        }
    }
}
