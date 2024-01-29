using System;
using System.Linq;

using DocumentFormat.OpenXml.Spreadsheet;

namespace CMS.ImportExport
{
    /// <summary>
    /// Class that holds coordinates of a macro and macro itself.
    /// </summary>
    internal class ExportOptions
    {
        #region "Properties"

        /// <summary>
        /// What should be exported.
        /// </summary>
        public ExportContents Contents
        {
            get;
            private set;
        }


        /// <summary>
        /// Whether supplied sheet is a template.
        /// </summary>
        public bool TemplateMode
        {
            get;
            private set;
        }


        /// <summary>
        /// Reference to cell that contains the macro.
        /// </summary>
        public string CellReference
        {
            get;
            set;
        }


        /// <summary>
        /// Reference to a sheet containing macro.
        /// </summary>
        public SheetData Sheet
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of a table to export.
        /// </summary>
        public string TableName
        {
            get;
            private set;
        }


        /// <summary>
        /// Number of row that contains the cell with macro.
        /// </summary>
        public int RowNumber
        {
            get
            {
                return DataExportHelper.GetRowNumber(CellReference);
            }
        }


        /// <summary>
        /// Name of a column that contains the cell with macro.
        /// </summary>
        public string ColumnName
        {
            get
            {
                return DataExportHelper.GetColumnName(CellReference);
            }
        }


        /// <summary>
        /// Number of column that contains the cell with macro.
        /// </summary>
        public int ColumnNumber
        {
            get
            {
                return DataExportHelper.GetColumnNumber(ColumnName);
            }
        }


        /// <summary>
        /// Whether the header of the table should be generated.
        /// </summary>
        public bool GenerateHeader
        {
            get
            {
                return (Contents == ExportContents.Header) || (Contents == ExportContents.Table);
            }
        }


        /// <summary>
        /// Whether the contents of the table should be generated.
        /// </summary>
        public bool GenerateData
        {
            get
            {
                return (Contents == ExportContents.Data) || (Contents == ExportContents.Table);
            }
        }


        /// <summary>
        /// Gets or sets a flag indicating that this instance was processed (exported to sheet).
        /// </summary>
        public bool IsProcessed
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for macros.
        /// </summary>
        /// <param name="macro">Name of a macro</param>
        /// <param name="cellReference">Reference to a cell containing macro</param>
        /// <param name="sheet">Reference to a sheet containing macro</param>
        /// <param name="templateMode">Whether the supplied sheet is a template</param>
        public ExportOptions(string macro, string cellReference, SheetData sheet, bool templateMode)
            : this(default(ExportContents), cellReference, sheet, templateMode)
        {
            // Trim '#'
            macro = macro.TrimStart('#').TrimEnd('#');

            if (macro.Contains(':'))
            {
                string[] macroParts = macro.Split(':');
                if (macroParts.Length == 2)
                {
                    TableName = macroParts[0];
                    macro = macroParts[1];
                }
            }
            Contents = (ExportContents)Enum.Parse(typeof(ExportContents), macro, true);
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contents">What should be exported</param>
        /// <param name="cellReference">Position of generated data</param>
        /// <param name="sheet">Reference to a sheet to export into</param>
        /// <param name="templateMode">Whether the supplied sheet is a template</param>
        public ExportOptions(ExportContents contents, string cellReference, SheetData sheet, bool templateMode)
        {
            Contents = contents;
            CellReference = cellReference;
            Sheet = sheet;
            TemplateMode = templateMode;
        }

        #endregion
    }
}
