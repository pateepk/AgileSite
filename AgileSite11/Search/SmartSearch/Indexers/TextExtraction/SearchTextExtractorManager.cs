using System;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.Search
{
    using TextExtractorDictionary = StringSafeDictionary<ISearchTextExtractor>;

    /// <summary>
    /// Provides management over extraction of text from binary files using extension - extractor mapping.
    /// </summary>
    public class SearchTextExtractorManager
    {
        /// <summary>
        /// Dictionary of registered extractors [Extension -> SearchTextExtractor]
        /// </summary>
        private static readonly TextExtractorDictionary mExtractors = new TextExtractorDictionary();


        /// <summary>
        /// Registers the given extractor.
        /// </summary>
        /// <param name="extension">Extension for which this extractor should be used (you can use either ".ext" or "ext" format)</param>
        /// <param name="extractor">Extractor to be used</param>
        public static void RegisterExtractor(string extension, ISearchTextExtractor extractor)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return;
            }

            mExtractors[extension.Trim('.')] = extractor;
        }


        /// <summary>
        /// Returns an extractor to use for specified extension.
        /// </summary>
        /// <param name="extension">Extension of the file to the extractor (you can use either ".ext" or "ext" format)</param>
        public static ISearchTextExtractor GetExtractor(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            return mExtractors[extension.Trim('.')];
        }


        /// <summary>
        /// Extracts text content from the given data using extractor registered for given extension.
        /// Returns null if no extractor is found.
        /// </summary>
        /// <param name="extension">Extension type of the data (you can use either ".ext" or "ext" format)</param>
        /// <param name="data">Data to extract</param>
        /// <param name="context">Extraction context</param>
        public static XmlData ExtractData(string extension, BinaryData data, ExtractionContext context)
        {
            var extractor = GetExtractor(extension);
            if (extractor != null)
            {
                return extractor.ExtractContent(data, context);
            }
            return null;
        }


        /// <summary>
        /// Returns list of the extensions (separated by comma and space) for which there is a content extractor registered in the system.
        /// </summary>
        public static string GetSupportedContentExtractors()
        {
            return string.Join(", ", mExtractors.TypedKeys.Distinct());
        }
    }
}
