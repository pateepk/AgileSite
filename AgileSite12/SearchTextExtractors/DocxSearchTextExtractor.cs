using System;
using System.Text;
using System.Xml;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using DocumentFormat.OpenXml.Packaging;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from DOCX files.
    /// </summary>
    public class DocxSearchTextExtractor : ISearchTextExtractor
    {
        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from.</param>
        /// <param name="context">Extraction context.</param>
        public XmlData ExtractContent(BinaryData data, ExtractionContext context)
        {
            StringBuilder result = new StringBuilder();

            var stream = data.Stream;
            if (stream != null)
            {
                using (var doc = WordprocessingDocument.Open(stream, false))
                {
                    // Manage namespaces to perform XPath queries.  
                    NameTable nt = new NameTable();
                    XmlNamespaceManager nsManager = new XmlNamespaceManager(nt);
                    nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

                    // Get the document part from the package.  
                    // Load the XML in the document part into an XmlDocument instance.  
                    XmlDocument xdoc = new XmlDocument(nt);
                    xdoc.Load(doc.MainDocumentPart.GetStream());

                    // Get the paragraph node
                    XmlNodeList paragraphNodes = xdoc.SelectNodes("//w:p", nsManager);
                    if (paragraphNodes != null)
                    {
                        foreach (XmlNode paragraphNode in paragraphNodes)
                        {
                            // Get the text nodes
                            XmlNodeList textNodes = paragraphNode.SelectNodes(".//w:t", nsManager);
                            if (textNodes != null)
                            {
                                foreach (XmlNode textNode in textNodes)
                                {
                                    result.Append(textNode.InnerText);
                                }
                                result.Append(Environment.NewLine);
                            }
                        }
                    }

                    result.Append(" ", ExtractComments(doc.MainDocumentPart));
                }
            }

            // Set the content field
            var content = new XmlData();
            content.SetValue(SearchFieldsConstants.CONTENT, result.ToString());

            return content;
        }


        /// <summary>
        /// Extracts author comments from the slides.
        /// </summary>
        /// <param name="mainPart">Main part of the DOCX file</param>
        private string ExtractComments(MainDocumentPart mainPart)
        {
            StringBuilder sb = new StringBuilder();

            var commentsPart = mainPart.WordprocessingCommentsPart;
            if (commentsPart != null)
            {
                var comments = commentsPart.Comments;
                if ((comments != null) && (comments.ChildElements != null))
                {
                    foreach (var c in comments.ChildElements)
                    {
                        sb.Append(" ", HTMLHelper.HtmlToPlainText(c.InnerXml));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
