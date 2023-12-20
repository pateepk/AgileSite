using System;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from HTML files.
    /// </summary>
    public class HtmlSearchTextExtractor : ISearchTextExtractor
    {
        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from.</param>
        /// <param name="context">Extraction context.</param>
        public XmlData ExtractContent(BinaryData data, ExtractionContext context)
        {
            if (data.Stream != null)
            {
                var htmlCode = data.Stream.ReadToEnd(Encoding.UTF8);

                // Set the content field
                var content = new XmlData();
                content.SetValue(SearchFieldsConstants.CONTENT, HTMLHelper.HtmlToPlainText(htmlCode));

                return content;
            }
            return null;
        }
    }
}
