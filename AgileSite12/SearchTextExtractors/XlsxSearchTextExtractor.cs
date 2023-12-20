using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CMS.Search.TextExtractors
{
    /// <summary>
    /// Provides content extraction from XLSX files.
    /// </summary>
    public class XlsxSearchTextExtractor : ISearchTextExtractor
    {
        private static readonly char[] dateChars = { 'y', 'm', 'd', 'h', 's' };

        // Table of predefined formats, https://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.numberingformat(v=office.15).aspx
        private static readonly Dictionary<uint, string> predefinedFormats = new Dictionary<uint, string>
        {
            { 1, "0" },
            { 2, "0.00" },
            { 3, "#,##0" },
            { 4, "#,##0.00" },
            { 9, "0%" },
            { 10, "0.00%" },
            { 11, "0.00E+00" },
            { 12, "# ?/?" },
            { 13, "# ??/??" },
            { 37, "#,##0 ;(#,##0)" },
            { 38, "#,##0 ;[Red](#,##0)" },
            { 39, "#,##0.00;(#,##0.00)" },
            { 40, "#,##0.00;[Red](#,##0.00)" },
            { 48, "##0.0E+0" },
            { 49, "@" }
        };

        private static readonly Dictionary<uint, string> predefinedDateFormats = new Dictionary<uint, string>
        {
            { 14, "M/d/yyyy" },
            { 15, "d-MMM-yy" },
            { 16, "d-MMM" },
            { 17, "MMM-yy" },
            { 18, "h:mm tt" },
            { 19, "h:mm:ss tt" },
            { 20, "H:mm" },
            { 21, "H:mm:ss" },
            { 22, "M/d/yy H:mm" },
            { 45, "mm:ss" },
            { 46, "[h]:mm:ss" },
            { 47, "mmss.0" },
        };


        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        /// <param name="data">Data to extract text from.</param>
        /// <param name="context">Extraction context.</param>
        public XmlData ExtractContent(BinaryData data, ExtractionContext context)
        {
            var content = new XmlData();
            content.SetValue(SearchFieldsConstants.CONTENT, String.Join(" ", ExtractContent(data.Stream)));
            return content;
        }


        /// <summary>
        /// Extracts content from given data.
        /// </summary>
        internal IEnumerable<string> ExtractContent(Stream data)
        {
            List<string> extracts = new List<string>();
            if (data != null)
            {
                ExtractContentDataWithComments(data, extracts);
            }
            return extracts;
        }


        private static void ExtractContentDataWithComments(Stream data, IList<string> extracts)
        {
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(data, false))
            {
                ExtractSharedStrings(doc, extracts);

                var cellFormats = doc.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats;
                var numberingFormats = doc.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats;

                foreach (var sheetPart in doc.WorkbookPart.WorksheetParts)
                {
                    foreach (var sheet in sheetPart.Worksheet.Elements<SheetData>())
                    {
                        foreach (var row in sheet.Elements<Row>())
                        {
                            foreach (var cell in row.Elements<Cell>())
                            {
                                // Cell without specified datatype indicates not-string based value    
                                if (cell.DataType == null)
                                {
                                    try
                                    {
                                        ProcessNumberBasedValues(extracts, cellFormats, numberingFormats, cell);
                                    }
                                    catch
                                    {
                                        // Number based values can lead to format exception which should not stop processing of the document
                                    }
                                }
                                // Try get text value from table of shared strings
                                else if ((cell.DataType.Value != CellValues.SharedString) && !String.IsNullOrEmpty(cell.CellValue.Text))
                                {
                                    extracts.Add(cell.CellValue.Text);
                                }
                            }
                        }
                    }

                    ExtractComments(sheetPart, extracts);
                }
            }
        }


        private static void ProcessNumberBasedValues(IList<string> extracts, CellFormats cellFormats, NumberingFormats numberingFormats, Cell cell)
        {
            var cellFormat = (CellFormat)cellFormats.ElementAt((int)cell.StyleIndex.Value);
            if (cellFormat.NumberFormatId != null)
            {
                var value = cell.InnerText;
                var numberFormatId = cellFormat.NumberFormatId.Value;
                var numberingFormat = numberingFormats.Cast<NumberingFormat>()
                    .FirstOrDefault(f => f.NumberFormatId.Value == numberFormatId);

                string format;
                var isDateFormat = GetNumberFormatAndType(numberingFormat, numberFormatId, out format);

                if (!String.IsNullOrEmpty(format))
                {
                    double d;
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                    {
                        if (isDateFormat)
                        {
                            try
                            {
                                var date = DateTime.FromOADate(d);
                                value = date.ToString(format);
                            }
                            catch
                            {
                                value = d.ToString(format);
                            }
                        }
                        else
                        {
                            value = d.ToString(format);
                        }
                    }
                }

                if (!String.IsNullOrEmpty(value))
                {
                    extracts.Add(value);
                }
            }
        }


        private static void ExtractSharedStrings(SpreadsheetDocument doc, IList<string> extracts)
        {
            var stringTable = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            if (stringTable != null)
            {
                foreach (var sharedString in stringTable.SharedStringTable)
                {
                    extracts.Add(sharedString.InnerText);
                }
            }
        }


        private static bool GetNumberFormatAndType(NumberingFormat numberingFormat, uint numberFormatId, out string format)
        {
            if (numberingFormat != null)
            {
                format = numberingFormat.FormatCode.Value;
                return (format.IndexOfAny(dateChars) >= 0);
            }

            return !predefinedFormats.TryGetValue(numberFormatId, out format)
                && predefinedDateFormats.TryGetValue(numberFormatId, out format);
        }


        private static void ExtractComments(WorksheetPart workSheetPart, IList<string> extracts)
        {
            var commentsPart = workSheetPart.WorksheetCommentsPart;
            var comments = commentsPart?.Comments;
            if (comments?.ChildElements != null && (comments.ChildElements.Count > 1) && (comments.ChildElements[1].ChildElements != null))
            {
                foreach (var c in comments.ChildElements[1].ChildElements)
                {
                    extracts.Add(HTMLHelper.HtmlToPlainText(c.InnerXml));
                }
            }
        }
    }
}
