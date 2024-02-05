using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.files;
using org.pdfclown.tools;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from PDF files.
    /// </summary>
    public class PdfSearchTextExtractor : ISearchTextExtractor
    {
        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from</param>
        /// <param name="context">Extraction context (ISeachDocument, Culture, etc.)</param>
        public XmlData ExtractContent(BinaryData data, ExtractionContext context)
        {
            var content = SafelyExtractContent(data);

            // Set the content field
            var xmlDataContent = new XmlData();
            xmlDataContent.SetValue(SearchFieldsConstants.CONTENT, content);

            return xmlDataContent;
        }


        /// <summary>
        /// Extracts PDF content and returns it. When an error occurs, empty string is returned and corresponding exception is logged.
        /// </summary>
        private string SafelyExtractContent(BinaryData data)
        {
            try
            {
                using (var stream = new Stream(data.Stream))
                {
                    File file = null;
                    try
                    {
                        file = new File(stream);
                        var document = file.Document;

                        return ExtractDocumentContent(document);
                    }
                    catch (NotImplementedException)
                    {
                        // Ignore not implemented exception which is thrown when an encrypted document is loaded
                        return String.Empty;
                    }
                    finally
                    {
                        if (file != null)
                        {
                            file.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException("PdfSearchTextExtractor", "EXTRACTTEXT", ex);

                return String.Empty;
            }
        }


        /// <summary>
        /// Extracts content from <paramref name="document"/> and returns it as a string.
        /// </summary>
        /// <returns>Extracted document content, or an empty string when <paramref name="document"/> is null.</returns>
        private string ExtractDocumentContent(Document document)
        {
            if (document == null)
            {
                return String.Empty;
            }

            StringBuilder result = new StringBuilder();

            // Append text from metadata of the document
            var info = document.Information;
            if (info != null)
            {
                result.Append(" ", info.Author);
                result.Append(" ", info.Creator);
                result.Append(" ", info.Keywords);
                result.Append(" ", info.Producer);
                result.Append(" ", info.Subject);
                result.Append(" ", info.Title);
            }

            // Loop through all the pages
            TextExtractor extractor = new TextExtractor();
            foreach (Page page in document.Pages)
            {
                // Extract the page content
                IDictionary<RectangleF?, IList<ITextString>> textStrings = null;
                try
                {
                    textStrings = extractor.Extract(page.Contents);
                }
                catch
                {
                }
                if (textStrings != null)
                {
                    foreach (IList<ITextString> textString in textStrings.Values)
                    {
                        // Loop through all the text items and add them into the result
                        foreach (var s in textString)
                        {
                            result.Append(" ", s.Text);
                        }
                    }
                }
            }

            return result.ToString();
        }
    }
}
