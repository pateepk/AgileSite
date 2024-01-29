using System;

using CMS.Core;
using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Provides base class for text extractors from binary files.
    /// </summary>
    public interface ISearchTextExtractor
    {
        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from</param>
        /// <param name="context">Extraction context (ISeachDocument, Culture, etc.)</param>
        XmlData ExtractContent(BinaryData data, ExtractionContext context);
    }
}
