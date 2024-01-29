using System;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using DocumentFormat.OpenXml.Packaging;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from PPTX files.
    /// </summary>
    public class PptxSearchTextExtractor : ISearchTextExtractor
    {
        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from</param>
        /// <param name="context">Extraction context (ISeachDocument, Culture, etc.)</param>
        public XmlData ExtractContent(BinaryData data, ExtractionContext context)
        {
            StringBuilder result = new StringBuilder();

            var stream = data.Stream;
            if (stream != null)
            {
                using (var myPres = PresentationDocument.Open(stream, false))
                {
                    PresentationPart presPart = myPres.PresentationPart;
                    if (presPart != null)
                    {
                        SlidePart[] slidePartList = presPart.SlideParts.ToArray();
                        foreach (var slidePart in slidePartList)
                        {
                            if (slidePart.Slide != null)
                            {
                                // Iterate through all the paragraphs in the slide.
                                foreach (var paragraph in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                                {
                                    // Iterate through the lines of the paragraph.
                                    foreach (var text in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                                    {
                                        // Append each line to the previous lines.
                                        result.Append(" ", text.Text);
                                    }
                                }
                            }
                        }

                        // Append comments and notes
                        result.Append(" ", ExtractComments(presPart));
                        result.Append(" ", ExtractNotes(presPart));
                    }
                }
            }

            // Set the content field
            var content = new XmlData();
            content.SetValue(SearchFieldsConstants.CONTENT, result.ToString());

            return content;
        }


        /// <summary>
        /// Extracts comments from the slides.
        /// </summary>
        /// <param name="presentation">Presentation part of the PPTX file</param>
        private string ExtractNotes(PresentationPart presentation)
        {
            StringBuilder sb = new StringBuilder();

            if (presentation.SlideParts != null)
            {
                foreach (var slide in presentation.SlideParts)
                {
                    var notes = slide.NotesSlidePart;
                    if ((notes != null) && (notes.NotesSlide != null))
                    {
                        sb.Append(" ", HTMLHelper.HtmlToPlainText(notes.NotesSlide.InnerXml));
                    }
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Extracts author comments from the slides.
        /// </summary>
        /// <param name="presentation">Presentation part of the PPTX file</param>
        private string ExtractComments(PresentationPart presentation)
        {
            StringBuilder sb = new StringBuilder();
            if (presentation.SlideParts != null)
            {
                foreach (var slide in presentation.SlideParts)
                {
                    var comments = slide.SlideCommentsPart;
                    if ((comments != null) && (comments.CommentList != null) && (comments.CommentList.ChildElements != null))
                    {
                        foreach (var c in comments.CommentList.ChildElements)
                        {
                            sb.Append(" ", HTMLHelper.HtmlToPlainText(c.InnerXml));
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}