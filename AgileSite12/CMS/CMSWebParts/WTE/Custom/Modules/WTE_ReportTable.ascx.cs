using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.EmailEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.ImportExport;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Reporting;
using CMS.Reporting.Web.UI;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

using SystemIO = System.IO;

using X14 = DocumentFormat.OpenXml.Office2010.Excel;

namespace CMSApp.CMSWebParts.WTE.Custom.Modules
{
    #region "Customized data export helper"

    /// <summary>
    /// Which part of data table should be exported.
    /// </summary>
    public enum ExportContents_WTE
    {
        /// <summary>
        /// Header row.
        /// </summary>
        Header,

        /// <summary>
        /// Data of a table.
        /// </summary>
        Data,

        /// <summary>
        /// Whole table (header and data).
        /// </summary>
        Table
    }

    /// <summary>
    /// Class that holds coordinates of a macro and macro itself.
    /// </summary>
    public class ExportOptions_WTE
    {
        #region "Properties"

        /// <summary>
        /// What should be exported.
        /// </summary>
        public ExportContents_WTE Contents
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
                return (Contents == ExportContents_WTE.Header) || (Contents == ExportContents_WTE.Table);
            }
        }

        /// <summary>
        /// Whether the contents of the table should be generated.
        /// </summary>
        public bool GenerateData
        {
            get
            {
                return (Contents == ExportContents_WTE.Data) || (Contents == ExportContents_WTE.Table);
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

        #endregion "Properties"

        #region "Constructors"

        /// <summary>
        /// Constructor for macros.
        /// </summary>
        /// <param name="macro">Name of a macro</param>
        /// <param name="cellReference">Reference to a cell containing macro</param>
        /// <param name="sheet">Reference to a sheet containing macro</param>
        /// <param name="templateMode">Whether the supplied sheet is a template</param>
        public ExportOptions_WTE(string macro, string cellReference, SheetData sheet, bool templateMode)
            : this(default(ExportContents_WTE), cellReference, sheet, templateMode)
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
            Contents = (ExportContents_WTE)Enum.Parse(typeof(ExportContents_WTE), macro, true);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="contents">What should be exported</param>
        /// <param name="cellReference">Position of generated data</param>
        /// <param name="sheet">Reference to a sheet to export into</param>
        /// <param name="templateMode">Whether the supplied sheet is a template</param>
        public ExportOptions_WTE(ExportContents_WTE contents, string cellReference, SheetData sheet, bool templateMode)
        {
            Contents = contents;
            CellReference = cellReference;
            Sheet = sheet;
            TemplateMode = templateMode;
        }

        #endregion "Constructors"
    }

    /// <summary>
    /// Provides DataSet export functionality to formats defined by DataExportFormatEnum.
    /// </summary>
    public class DataExportHelper_WTE : CoreMethods
    {
        #region "Constants"

        /// <summary>
        /// Count of ASCII uppercase letters.
        /// </summary>
        private const int LETTER_COUNT = 26;

        /// <summary>
        /// Offset determining starting index of uppercase letters in ASCII table.
        /// </summary>
        private const int ASCII_BEGINNING = 65;

        /// <summary>
        /// Maximum size of Excel cell.
        /// </summary>
        protected static int MAX_EXCEL_CELL_SIZE = 32767;

        /// <summary>
        /// Maximum length of sheet name.
        /// </summary>
        private const short MAX_SHEETNAME_LENGTH = 31;

        /// <summary>
        /// Default name of Excel export template.
        /// </summary>
        protected static string DEFAULT_TEMPLATE_NAME = "Template.xlsx";

        /// <summary>
        /// Holds macro resolver.
        /// </summary>
        protected MacroResolver mMacroResolver = null;

        /// <summary>
        /// Indicates whether to use template during Excel export.
        /// </summary>
        private bool mUseTemplate = true;

        /// <summary>
        /// Indicates whether to use shared string table during Excel export.
        /// </summary>
        protected bool? mUseSharedStringStorage = null;

        /// <summary>
        /// File of name to export with extension.
        /// </summary>
        protected string mFileNameWithExtension = null;

        /// <summary>
        /// File of name to export (without extension).
        /// </summary>
        protected string mFileName = null;

        /// <summary>
        /// Source of data.
        /// </summary>
        protected DataSet mDataSource = null;

        /// <summary>
        /// Name of element representing column used when there is no other name available.
        /// </summary>
        protected const string DEFAULT_COLUMN_NAME = "Column";

        #endregion "Constants"

        #region "Variables"

        /// <summary>
        /// Delimiter for CSV format.
        /// </summary>
        private string mCSVDelimiter = CultureHelper.PreferredUICultureInfo.TextInfo.ListSeparator;

        /// <summary>
        /// Whether to generate header for exported tables.
        /// </summary>
        private bool mGenerateHeader = true;

        /// <summary>
        /// Holds path leading to Excel export template.
        /// </summary>
        protected string mDataExportTemplatePath = null;

        /// <summary>
        /// Holds startup path for Excel template lookup.
        /// </summary>
        private string mDataExportTemplateFolder;

        /// <summary>
        /// Holds actually exported columns.
        /// </summary>
        protected Hashtable mExportedColumns = null;

        /// <summary>
        /// Holds list of export options.
        /// </summary>
        private List<ExportOptions_WTE> optionList;

        /// <summary>
        /// Shared strings cache.
        /// </summary>
        private Dictionary<string, int> sharedStringCache = new Dictionary<string, int>();

        #endregion "Variables"

        #region "Delegates and events"

        /// <summary>
        /// Performes external data bound on column of datarow.
        /// </summary>
        /// <param name="dataRow">Row with data</param>
        /// <param name="columnIndex">Index of a column</param>
        public delegate object OnExternalDataBound(DataRowView dataRow, int columnIndex);

        /// <summary>
        /// External data bound handler.
        /// </summary>
        protected event OnExternalDataBound ExternalDataBound = null;

        /// <summary>
        /// Raised when error occurs.
        /// </summary>
        /// <param name="customMessage">Message set when error occurs</param>
        /// <param name="exception">Original exception</param>
        public delegate void OnError(string customMessage, Exception exception);

        /// <summary>
        /// Error handler.
        /// </summary>
        public event OnError Error = null;

        #endregion "Delegates and events"

        #region "Properties"

        /// <summary>
        /// Determines whether to allow export empty data source or raise an error
        /// </summary>
        public bool AllowExportEmptyDataSource
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a startup path for template lookup.
        /// </summary>
        public virtual string DataExportTemplateFolder
        {
            get
            {
                return mDataExportTemplateFolder ?? (mDataExportTemplateFolder = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSDataExportTemplateFolder"], "~/App_Data/CMSModules/DataExport/"));
            }
        }

        /// <summary>
        /// Gets a relative path leading to template.
        /// </summary>
        public virtual string DataExportTemplatePath
        {
            get
            {
                if (mDataExportTemplatePath == null)
                {
                    string path = DirectoryHelper.CombinePath(FileHelper.GetFullFolderPhysicalPath(DataExportTemplateFolder), DEFAULT_TEMPLATE_NAME);
                    if (CMS.IO.File.Exists(path))
                    {
                        mDataExportTemplatePath = path;
                    }
                }
                return mDataExportTemplatePath;
            }
            set
            {
                mDataExportTemplatePath = value;
            }
        }

        /// <summary>
        /// Gets or sets macro resolver.
        /// </summary>
        public virtual MacroResolver MacroResolver
        {
            get
            {
                return mMacroResolver ?? (mMacroResolver = MacroResolver.GetInstance());
            }
            set
            {
                mMacroResolver = value;
            }
        }

        /// <summary>
        /// Indicates whether to use XLSX template.
        /// </summary>
        public virtual bool UseTemplate
        {
            get
            {
                return mUseTemplate;
            }
            set
            {
                mUseTemplate = value;
            }
        }

        /// <summary>
        /// Defines delimiter for CSV format.
        /// </summary>
        public string CSVDelimiter
        {
            get
            {
                return mCSVDelimiter;
            }
            set
            {
                mCSVDelimiter = value;
            }
        }

        /// <summary>
        /// Indicates whether to generate also a header row.
        /// </summary>
        public bool GenerateHeader
        {
            get
            {
                return mGenerateHeader;
            }
            set
            {
                mGenerateHeader = value;
            }
        }

        /// <summary>
        /// Top N rows to export.
        /// </summary>
        public int TopN
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source of data to export.
        /// </summary>
        public virtual DataSet DataSource
        {
            get
            {
                //MacroResolver.SetDynamicParameter("TotalRecords", DataHelper.GetItemsCount(mDataSource));
                return mDataSource;
            }
            private set
            {
                mDataSource = value;
            }
        }

        /// <summary>
        /// List of macros available for current datasource.
        /// </summary>
        private List<string> AvailableMacros
        {
            get
            {
                List<string> availableMacros = new List<string>();
                // Add general macros
                foreach (string macro in Enum.GetNames(typeof(ExportContents_WTE)))
                {
                    availableMacros.Add("##" + macro.ToUpperCSafe() + "##");
                }
                // Add table macros
                if ((DataSource != null) && (DataSource.Tables.Count > 0))
                {
                    foreach (DataTable table in DataSource.Tables)
                    {
                        foreach (string macro in Enum.GetNames(typeof(ExportContents_WTE)))
                        {
                            availableMacros.Add("##" + table.TableName.ToUpperCSafe() + ":" + macro.ToUpperCSafe() + "##");
                        }
                    }
                }
                return availableMacros;
            }
        }

        /// <summary>
        /// Defines whether to use shared string table for storing text values.
        /// When value is false text is being stored as inline string.
        /// Applies only to Excel export.
        /// </summary>
        public virtual bool UseSharedStringStorage
        {
            get
            {
                if (mUseSharedStringStorage == null)
                {
                    mUseSharedStringStorage = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDataExportUseSharedStrings"], true);
                }
                return mUseSharedStringStorage.Value;
            }
            set
            {
                mUseSharedStringStorage = value;
            }
        }

        /// <summary>
        /// Name of exported file (without extension).
        /// </summary>
        public virtual string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(mFileName))
                {
                    mFileName = "Export";
                }
                return mFileName;
            }
            set
            {
                mFileName = value;
            }
        }

        /// <summary>
        /// Name of exported file with extension.
        /// </summary>
        public virtual string FileNameWithExtension
        {
            get
            {
                return mFileNameWithExtension;
            }
            set
            {
                mFileNameWithExtension = value;
            }
        }

        #endregion "Properties"

        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DataExportHelper_WTE()
        {
        }

        /// <summary>
        /// Constructor initializing data source.
        /// </summary>
        public DataExportHelper_WTE(DataSet dataSource)
        {
            DataSource = dataSource;
        }

        #endregion "Constructors"

        #region "Event methods"

        /// <summary>
        /// Raises error event.
        /// </summary>
        /// <param name="customMessage">Message set when error occurs</param>
        /// <param name="exception">Original exception</param>
        protected void RaiseError(string customMessage, Exception exception)
        {
            if (Error != null)
            {
                Error(customMessage, exception);
            }
        }

        #endregion "Event methods"

        #region "Public methods"

        /// <summary>
        /// Exports data stored in DataSource property, writes it to given response.
        /// </summary>
        /// <param name="format">Export format</param>
        /// <param name="response">HTTP response</param>
        public void ExportData(DataExportFormatEnum format, HttpResponse response)
        {
            SystemIO.Stream stream = null;
            bool errorOccurred = false;
            try
            {
                // Set file name
                if (string.IsNullOrEmpty(FileName))
                {
                    FileName = "Export." + format.ToString().ToLowerCSafe();
                }

                // Clear response
                response.Clear();
                response.ClearHeaders();

                stream = response.OutputStream;

                // Write data to stream
                ExportData(format, stream);

                // Set content type and headers
                response.ContentType = GetDataExportFormatContentType(format);
                response.AddHeader("Content-Disposition", string.Format("attachment;filename=\"{0}\"", HTTPHelper.GetDispositionFilename(GetFileName(format))));
            }
            catch (Exception ex)
            {
                errorOccurred = true;
                response.Clear();

                if (!(ex is NoDataException))
                {
                    // Log all but NoDataException
                    EventLogProvider.LogException("Data Export", "ExportData", ex);
                }

                RaiseError(CoreServices.Localization.GetString("export.errorexport"), ex);
            }
            finally
            {
                // Close stream
                if (stream != null)
                {
                    stream.Flush();
                    stream.Close();
                }
                if (!errorOccurred)
                {
                    // End response
                    if (response != null)
                    {
                        // Send all currently buffered output to the client, stops execution of the page
                        response.End();
                    }
                }
            }
        }

        /// <summary>
        /// Exports data stored in DataSource property, writes it to given stream.
        /// </summary>
        /// <param name="format">Format to export data in</param>
        /// <param name="stream">Stream to export data to</param>
        public void ExportData(DataExportFormatEnum format, SystemIO.Stream stream)
        {
            // Raise error if data source is empty (and if desired)
            if (!AllowExportEmptyDataSource && DataHelper.DataSourceIsEmpty(DataSource))
            {
                string errorMessage = CoreServices.Localization.GetString("export.emptydatasource");
                RaiseError(errorMessage, null);
                throw new NoDataException(errorMessage);
            }

            switch (format)
            {
                case DataExportFormatEnum.XLSX:
                    ExportToExcel(DataSource, stream);
                    break;

                case DataExportFormatEnum.CSV:
                    ExportToCSV(DataSource, stream);
                    break;

                case DataExportFormatEnum.XML:
                    ExportToXML(DataSource, stream);
                    break;
            }
        }

        #endregion "Public methods"

        #region "Data source related methods"

        /// <summary>
        /// Gets columns that should be exported.
        /// </summary>
        /// <param name="tableName">Name of a table</param>
        protected virtual List<string> GetExportedColumns(string tableName)
        {
            if (mExportedColumns == null)
            {
                mExportedColumns = new Hashtable();
            }

            if (!mExportedColumns.Contains(tableName))
            {
                if ((DataSource != null) && (DataSource.Tables.Count > 0))
                {
                    List<string> columns = new List<string>();
                    if (DataSource.Tables.Contains(tableName))
                    {
                        foreach (DataColumn column in DataSource.Tables[tableName].Columns)
                        {
                            if (!string.IsNullOrEmpty(column.ColumnName))
                            {
                                columns.Add(column.ColumnName);
                            }
                        }
                        mExportedColumns[tableName] = columns;
                    }
                }
            }
            return (List<string>)mExportedColumns[tableName];
        }

        /// <summary>
        /// Gets index of a column corresponding to columns actually selected to export.
        /// </summary>
        /// <param name="tableName">Name of a table</param>
        /// <param name="columnName">Name of a column</param>
        /// <returns>Index of a column</returns>
        protected virtual int GetColumnIndex(string tableName, string columnName)
        {
            return GetExportedColumns(tableName).IndexOf(columnName);
        }

        /// <summary>
        /// Gets a caption for given column name.
        /// </summary>
        /// <param name="columnIndex">Index of column</param>
        /// <param name="tableName">Name of a table</param>
        /// <returns>Caption for a column</returns>
        protected virtual string GetColumnCaptionText(int columnIndex, string tableName)
        {
            if ((DataSource != null) && (DataSource.Tables.Count > 0))
            {
                DataTable table = DataSource.Tables[tableName];
                if (table != null)
                {
                    List<string> columnNames = GetExportedColumns(tableName);
                    if ((columnNames != null) && (columnNames.Count > 0))
                    {
                        string columnName = columnNames[columnIndex];
                        if ((columnName != null) && table.Columns.Contains(columnName))
                        {
                            DataColumn column = table.Columns[columnName];
                            if (column != null)
                            {
                                return column.Caption;
                            }
                        }
                    }
                }
            }

            throw new Exception("[DataExportHelper.GetColumnCaptionText]: Column was not found.");
        }

        #endregion "Data source related methods"

        #region "Internal export methods"

        #region "XLSX Export"

        #region "Excel generation entry point"

        /// <summary>
        /// Exports given DataSet to XLSX format.
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="stream">Stream to write to</param>
        private void ExportToExcel(DataSet dataSet, SystemIO.Stream stream)
        {
            dataSet = GetExportedData(dataSet, false);
            ClearSharedStringsCache();

            using (var sheetStream = new SystemIO.MemoryStream())
            {
                if (!TryCreateExcelFromTemplate(dataSet, sheetStream))
                {
                    CreateNewExcel(dataSet, sheetStream);
                }
                sheetStream.WriteTo(stream);
            }
        }

        private bool TryCreateExcelFromTemplate(DataSet dataSet, SystemIO.MemoryStream sheetStream)
        {
            SpreadsheetDocument spreadSheet;
            bool templateLoaded = false;
            if (UseTemplate && (DataExportTemplatePath != null))
            {
                try
                {
                    using (var templateStream = CMS.IO.FileStream.New(DataExportTemplatePath, CMS.IO.FileMode.Open, CMS.IO.FileAccess.Read))
                    {
                        templateStream.CopyTo(sheetStream);
                    }
                    templateLoaded = true;
                }
                catch
                {
                    CoreServices.EventLog.LogEvent("W", "Data Export", "ExportToExcel", $"Failed to load template from '{DataExportTemplatePath}'. Make sure the file is not being used by another process.");
                }
            }

            templateLoaded &= (sheetStream.Length != 0);

            if (UseTemplate && templateLoaded)
            {
                // Open document from memory
                spreadSheet = SpreadsheetDocument.Open(sheetStream, true);
                if (dataSet != null)
                {
                    using (spreadSheet)
                    {
                        WorkbookPart workbookPart = spreadSheet.WorkbookPart;
                        // Cache existing string table
                        CacheExistingSharedStrings(workbookPart);

                        foreach (WorksheetPart worksheetPart in workbookPart.WorksheetParts)
                        {
                            // Resolve CMS macros and get custom macros
                            optionList = HandleMacros(worksheetPart.Worksheet.ChildElements.OfType<SheetData>());
                            foreach (ExportOptions_WTE options in optionList)
                            {
                                DataTable table = null;
                                if (options.TableName == null)
                                {
                                    if (dataSet.Tables.Count > 0)
                                    {
                                        table = dataSet.Tables[0];
                                    }
                                }
                                else
                                {
                                    if (dataSet.Tables.Contains(options.TableName))
                                    {
                                        table = dataSet.Tables[options.TableName];
                                    }
                                }

                                // Fill sheets according to custom macros
                                FillSheetWithData(table, options);
                            }
                        }
                        // Save shared strings cache
                        FillStringTable(workbookPart);
                    }
                }

                return true;
            }

            return false;
        }

        public static Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
            new DocumentFormat.OpenXml.Spreadsheet.Fonts(
                new DocumentFormat.OpenXml.Spreadsheet.Font(
                    new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 11 }, new Color() { Rgb = new HexBinaryValue() { Value = "000000" } }, new FontName() { Val = "Calibri" }),// Index 0 - The default font.
                    new Font(new Bold(), new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 11 }, new Color() { Rgb = new HexBinaryValue() { Value = "000000" } }, new FontName() { Val = "Calibri" }),  // Index 1 - The bold font.
                    new Font(new Italic(), new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 11 }, new Color() { Rgb = new HexBinaryValue() { Value = "000000" } }, new FontName() { Val = "Calibri" }),  // Index 2 - The Italic font.
                    new Font(new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 18 }, new Color() { Rgb = new HexBinaryValue() { Value = "000000" } }, new FontName() { Val = "Calibri" }),  // Index 3 - The Times Roman font. with 16 size
                    new Font(new Bold(), new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 18 }, new Color() { Rgb = new HexBinaryValue() { Value = "000000" } }, new FontName() { Val = "Calibri" }),  // Index 4 - The Times Roman font. with 16 size
                    new Font(new Bold(), new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 11 }, new Color() { Rgb = new HexBinaryValue() { Value = "FFFFFF" } }, new FontName() { Val = "Calibri" }),  // Index 5 - The bold font.
                    new Font(new Bold(), new DocumentFormat.OpenXml.Spreadsheet.FontSize() { Val = 11 }, new Color() { Rgb = new HexBinaryValue() { Value = "C0C0C0" } }, new FontName() { Val = "Calibri" })  // Index 6 - The bold font/gray background
                ),
                new Fills(
                    // Index 0 - The default fill.
                    new DocumentFormat.OpenXml.Spreadsheet.Fill(new DocumentFormat.OpenXml.Spreadsheet.PatternFill() { PatternType = PatternValues.None }),
                    // Index 1 - The default fill of gray 125 (required)
                    new DocumentFormat.OpenXml.Spreadsheet.Fill(new DocumentFormat.OpenXml.Spreadsheet.PatternFill() { PatternType = PatternValues.Gray125 }),
                    // Index 2 - The yellow fill.
                    new DocumentFormat.OpenXml.Spreadsheet.Fill(new DocumentFormat.OpenXml.Spreadsheet.PatternFill(new DocumentFormat.OpenXml.Spreadsheet.ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }) { PatternType = PatternValues.Solid }),
                    // Index 3 - The Blue fill.
                    new DocumentFormat.OpenXml.Spreadsheet.Fill(new DocumentFormat.OpenXml.Spreadsheet.PatternFill(new DocumentFormat.OpenXml.Spreadsheet.ForegroundColor() { Rgb = new HexBinaryValue() { Value = "8EA9DB" } }) { PatternType = PatternValues.Solid }),
                    // Index 4 - Gray blackground.
                    new DocumentFormat.OpenXml.Spreadsheet.Fill(new DocumentFormat.OpenXml.Spreadsheet.PatternFill(new DocumentFormat.OpenXml.Spreadsheet.ForegroundColor() { Rgb = new HexBinaryValue() { Value = "808080" } }) { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    // Index 0 - The default border.
                    new Border(
                        new DocumentFormat.OpenXml.Spreadsheet.LeftBorder(),
                        new DocumentFormat.OpenXml.Spreadsheet.RightBorder(),
                        new DocumentFormat.OpenXml.Spreadsheet.TopBorder(),
                        new DocumentFormat.OpenXml.Spreadsheet.BottomBorder(),
                        new DiagonalBorder()
                        ),
                    // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                    new Border(
                        new DocumentFormat.OpenXml.Spreadsheet.LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DocumentFormat.OpenXml.Spreadsheet.RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DocumentFormat.OpenXml.Spreadsheet.TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DocumentFormat.OpenXml.Spreadsheet.BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()
                        ),
                   // Index 2 - Applies bottom border to the cell
                   new Border(
                        new DocumentFormat.OpenXml.Spreadsheet.LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.None },
                        new DocumentFormat.OpenXml.Spreadsheet.RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.None },
                        new DocumentFormat.OpenXml.Spreadsheet.TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.None },
                        new DocumentFormat.OpenXml.Spreadsheet.BottomBorder(new Color() { Rgb = new HexBinaryValue() { Value = "FFA500" } }) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()
                        )
                 ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }, // Index 0 - The default cell style. If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 1 - Bold
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 2 - Italic
                    new CellFormat() { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 3 - Times Roman
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true }, // Index 4 - Yellow Fill
                    new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 0, ApplyAlignment = true },  // Index 5 - Alignment
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }, // Index 6 - Border
                    new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 1, FillId = 0, BorderId = 0, ApplyAlignment = true }, // Index 7 - Alignment
                    new CellFormat() { FontId = 4, FillId = 0, BorderId = 0, ApplyFont = true }, // Index 8 - Times Roman
                    new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 0, FillId = 0, BorderId = 2, ApplyFont = true }, // Index 9 - Bottom Border with Color 70AD47
                    new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 5, FillId = 3, BorderId = 0, ApplyAlignment = true }, // Index 10 - Alignment
                    new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }) { FontId = 6, FillId = 4, BorderId = 1, ApplyAlignment = true, ApplyFill = true }, // Index 11 - TN excel header
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyFill = true } // Index 12 - TN excel body
                )
            ); // return
        }

        private void CreateNewExcel(DataSet dataSet, SystemIO.MemoryStream sheetStream)
        {
            var spreadSheet = SpreadsheetDocument.Create(sheetStream, SpreadsheetDocumentType.Workbook);

            using (spreadSheet)
            {
                // Get existing workbook part or create a new one
                WorkbookPart workbookPart = spreadSheet.WorkbookPart ?? spreadSheet.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart wsp = workbookPart.AddNewPart<WorksheetPart>();

                WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();
                // add styles to sheet
                wbsp.Stylesheet = GenerateStyleSheet();
                wbsp.Stylesheet.Save();

                // Append sheets element
                spreadSheet.WorkbookPart.Workbook.AppendChild(new Sheets());

                if ((dataSet == null) || (dataSet.Tables.Count == 0) || (DataHelper.DataSourceIsEmpty(dataSet) && !GenerateHeader))
                {
                    // Insert a new worksheet
                    InsertWorksheet(workbookPart, "Sheet");
                }
                else
                {
                    // Insert new worksheet for each data table
                    foreach (DataTable dataTable in dataSet.Tables)
                    {
                        // Insert a new worksheet (and trim name to 31 chars)
                        WorksheetPart worksheetPart = InsertWorksheet(workbookPart, TextHelper.LimitLength(dataTable.TableName, MAX_SHEETNAME_LENGTH, string.Empty));

                        // Get data sheet
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                        // Setup new export options and export data
                        ExportOptions_WTE options = new ExportOptions_WTE(GenerateHeader ? ExportContents_WTE.Table : ExportContents_WTE.Data, "A1", sheetData, false);
                        FillSheetWithData(dataTable, options);
                    }

                    // Save shared strings cache
                    FillStringTable(workbookPart);
                }
            }
        }

        #endregion "Excel generation entry point"

        #region "Methods for filling sheets with data"

        /// <summary>
        /// Fills sheet with data from given data table.
        /// </summary>
        /// <param name="dataTable">Table with data</param>
        /// <param name="options">Export settings</param>
        private void FillSheetWithData(DataTable dataTable, ExportOptions_WTE options)
        {
            if (dataTable != null)
            {
                List<string> exportedColumns = GetExportedColumns(dataTable.TableName);
                Worksheet worksheet = options.Sheet.Parent as Worksheet;
                if (worksheet != null)
                {
                    if (worksheet.SheetDimension == null)
                    {
                        worksheet.SheetDimension = new SheetDimension();
                    }

                    // Create header
                    Row previous = CreateExcelHeader(dataTable, options);

                    if (options.GenerateData && !DataHelper.DataSourceIsEmpty(dataTable))
                    {
                        // Calculate row offset
                        int rowNumberOffset = Convert.ToInt32(options.GenerateHeader) + options.RowNumber;
                        int newRows = 0;

                        // Append content rows
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            int rowNumber = rowNumberOffset + i;

                            Row row;
                            if (options.TemplateMode)
                            {
                                // Insert new row if needed (do not overwrite rows)
                                if (previous == null)
                                {
                                    row = GetExistingRow(rowNumberOffset, options.Sheet);
                                }
                                else
                                {
                                    row = CloneRow(previous, options.Sheet);
                                    newRows++;
                                }
                            }
                            else
                            {
                                row = new Row();
                            }

                            // Set row index
                            row.RowIndex = (UInt32)rowNumber;

                            // Fill row with data (DataRow)
                            row = FillContentRow(row, dataTable.Rows[i], options.ColumnNumber, exportedColumns);

                            if (options.TemplateMode)
                            {
                                previous = row;
                            }
                            else
                            {
                                options.Sheet.AppendChild(row);
                            }
                        }

                        // Calculate filled sheet dimension
                        string startCell = "A1";
                        string endCell = startCell;

                        string dims = worksheet.SheetDimension.Reference;
                        if (!String.IsNullOrEmpty(dims))
                        {
                            string[] dim = dims.Split(':');
                            startCell = dim[0];
                            endCell = (dim.Length == 2) ? dim[1] : dim[0];
                        }

                        // Don't count the column with ##TABLE## twice.
                        int columns = options.ColumnNumber + exportedColumns.Count - 1;

                        // Template can be wider than exported data.
                        columns = Math.Max(columns, GetColumnNumber(GetColumnName(endCell)));

                        int rows = GetRowNumber(endCell);

                        // New rows are counted only when using template.
                        rows += options.TemplateMode ? newRows : dataTable.Rows.Count;

                        worksheet.SheetDimension.Reference = String.Format("{0}:{1}", startCell, CreateColumnReference(columns, rows));

                        UpdateExportOptions_WTE(options.RowNumber, newRows);
                    }

                    options.IsProcessed = true;
                }
            }
        }

        /// <summary>
        /// Creates header in the excel file
        /// </summary>
        /// <param name="dataTable">Data table with the data to be exported</param>
        /// <param name="options">Export options</param>
        private Row CreateExcelHeader(DataTable dataTable, ExportOptions_WTE options)
        {
            // Temporary storage for already processed rows
            Row previous = null;

            // Create first row with header (column names)
            if (options.GenerateHeader)
            {
                Row headerRow = options.TemplateMode ? GetExistingRow(options.RowNumber, options.Sheet) : new Row();

                // Set row number
                headerRow.RowIndex = (UInt32)options.RowNumber;

                int i = 0;

                foreach (DataColumn dc in dataTable.Columns)
                {
                    // Get cell reference
                    string cellReference = CreateColumnReference(options.ColumnNumber + i, options.RowNumber);

                    // Delete cell with corresponding reference if exists
                    DeleteExistingCell(headerRow, cellReference);

                    // Create cell
                    Cell headerCell = CreateCell(cellReference, dc.Caption, 11);

                    // Append cell to first row
                    headerRow.AppendChild(headerCell);

                    i++;
                }

                // Sort cells to ensure row's validity
                SortCellsInRow(headerRow);

                // Append header to sheet
                if (options.TemplateMode)
                {
                    previous = headerRow;
                }
                else
                {
                    options.Sheet.AppendChild(headerRow);
                }
            }

            return previous;
        }

        /// <summary>
        /// Fills given row with content (overwrites columns).
        /// </summary>
        /// <param name="row">Row to fill</param>
        /// <param name="dataRow">Row containing data</param>
        /// <param name="columnIndexOffset">Offset of column</param>
        /// <param name="exportedColumns">Columns to export</param>
        /// <returns>Created row</returns>
        private Row FillContentRow(Row row, DataRow dataRow, int columnIndexOffset, List<string> exportedColumns)
        {
            // Calculate column offset
            int columnNumber = columnIndexOffset;

            // Append cells (columns) to the row
            for (int i = 0; i < exportedColumns.Count; i++)
            {
                // Get column name
                string columnName = exportedColumns[i];

                // Apply external data bound if set
                var value = dataRow[columnName];

                // Get cell reference
                string cellReference = CreateColumnReference(i + columnNumber, Convert.ToInt32(row.RowIndex.Value));

                // Delete cell if exists (column values are being overwrited)
                DeleteExistingCell(row, cellReference);

                // Create new cell
                Cell dataCell = CreateCell(cellReference, value, 12);

                // Append created cell to the row
                row.AppendChild(dataCell);
            }

            // Sort cells to ensure row's validity
            SortCellsInRow(row);
            return row;
        }

        #endregion "Methods for filling sheets with data"

        #region "Macro & export options handling"

        /// <summary>
        /// Resolves macros using macro resolver and gets custom macros (##HEADER##, ##DATA##...).
        /// </summary>
        /// <param name="sheets">Excel sheets</param>
        /// <returns>List of custom macros</returns>
        private List<ExportOptions_WTE> HandleMacros(IEnumerable<SheetData> sheets)
        {
            List<ExportOptions_WTE> offsets = new List<ExportOptions_WTE>();
            foreach (SheetData sheetData in sheets)
            {
                // Get workbook part
                Worksheet worksheet = sheetData.Parent as Worksheet;
                if (worksheet != null)
                {
                    WorksheetPart worksheetPart = worksheet.WorksheetPart;
                    WorkbookPart workbookPart = worksheetPart.GetParentParts().OfType<WorkbookPart>().First();
                    SharedStringTablePart sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                    List<Cell> cellsToRemove = new List<Cell>();

                    foreach (Cell cell in sheetData.Descendants<Cell>())
                    {
                        string cellValue = null;
                        int sharedStringId = -1;
                        SharedStringItem sharedString = null;
                        if (cell.DataType != null)
                        {
                            switch (cell.DataType.Value)
                            {
                                case CellValues.SharedString:

                                    if (sharedStringTablePart != null)
                                    {
                                        IEnumerable<SharedStringItem> items = sharedStringTablePart.SharedStringTable.Elements<SharedStringItem>();

                                        // Get shared string by identifier
                                        if (cell.CellValue != null)
                                        {
                                            sharedStringId = int.Parse(cell.CellValue.Text);
                                            sharedString = items.ElementAt(sharedStringId);
                                            cellValue = sharedString.InnerText;
                                        }
                                    }
                                    break;

                                case CellValues.InlineString:
                                    if (cell.CellValue != null)
                                    {
                                        cellValue = cell.CellValue.Text;
                                    }
                                    break;
                            }
                            // Find data macro
                            if (AvailableMacros.Contains(cellValue.ToUpperCSafe()))
                            {
                                // Create new macro export option
                                ExportOptions_WTE macro = new ExportOptions_WTE(cellValue, cell.CellReference, sheetData, true);
                                offsets.Add(macro);

                                // Add cell to list for later deletion
                                cellsToRemove.Add(cell);

                                // Remove reference to shared string table
                                if (cell.CellValue != null)
                                {
                                    cell.CellValue.Text = "-1";
                                }
                                // Delete possible record in shared string table
                                if ((sharedString != null) && (sharedStringId > -1))
                                {
                                    RemoveSharedStringItem(sharedStringId, workbookPart);
                                }
                            }
                            else
                            {
                                // Resolve macros
                                string resolvedValue = MacroResolver.ResolveMacros(cellValue);

                                if (resolvedValue != cellValue)
                                {
                                    if (ValidationHelper.IsInteger(resolvedValue) || ValidationHelper.IsLong(resolvedValue) || ValidationHelper.IsDouble(resolvedValue))
                                    {
                                        cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                                        cell.CellValue = new CellValue(resolvedValue);
                                    }
                                    else
                                    {
                                        if (UseSharedStringStorage)
                                        {
                                            cell.DataType = CellValues.SharedString;
                                            if (cell.CellValue != null)
                                            {
                                                cell.CellValue.Text = StoreSharedStringItem(resolvedValue).ToString();//InsertSharedStringItem(resolvedValue, sharedStringTablePart).ToString();
                                            }
                                        }
                                        else
                                        {
                                            AddInlineStringToCell(resolvedValue, cell);
                                        }
                                    }

                                    // Delete possible record in shared string table
                                    if ((sharedString != null) && (sharedStringId > -1))
                                    {
                                        RemoveSharedStringItem(sharedStringId, workbookPart);
                                    }
                                }
                            }
                        }
                    }
                    // Remove macro cells and their data
                    foreach (Cell cell in cellsToRemove)
                    {
                        cell.Remove();
                    }
                }
            }
            return offsets;
        }

        /// <summary>
        /// Updates row numbers for each not yet resolved macro according to newly added rows.
        /// </summary>
        /// <param name="startingRowNumber">Number of row from which to start updating</param>
        /// <param name="newRows">Count of newly added rows</param>
        private void UpdateExportOptions_WTE(int startingRowNumber, int newRows)
        {
            if (optionList != null)
            {
                List<ExportOptions_WTE> optionsToUpdate = optionList.Where(o => !o.IsProcessed && (o.RowNumber >= startingRowNumber)).ToList();
                foreach (ExportOptions_WTE optionsObject in optionsToUpdate)
                {
                    // Get new row number
                    string oldCellRef = optionsObject.CellReference;
                    int oldRowNumber = GetRowNumber(oldCellRef);
                    int newRowNumber = oldRowNumber + newRows;

                    // Update row number
                    optionsObject.CellReference = oldCellRef.Replace(oldRowNumber.ToString(), newRowNumber.ToString());
                }
            }
        }

        #endregion "Macro & export options handling"

        #region "Row, column and cell manipulation"

        /// <summary>
        /// Creates row below referenced row.
        /// </summary>
        /// <param name="referencedRow">Row which should be copied under itself</param>
        /// <param name="sheetData">Sheet</param>
        /// <returns>Cloned row</returns>
        private static Row CloneRow(Row referencedRow, SheetData sheetData)
        {
            // Get row index
            uint rowIndex = referencedRow.RowIndex.Value;

            // Clone row
            Row newRow = (Row)referencedRow.Clone();

            // Set new indexes to all following rows
            foreach (Row row in sheetData.Descendants<Row>().Where(r => r.RowIndex.Value > rowIndex))
            {
                // Count new index and set it
                uint newRowIndex = Convert.ToUInt32(row.RowIndex.Value + 1);
                SetNewRowIndexes(row, newRowIndex);
                row.RowIndex = new UInt32Value(newRowIndex);
            }

            // Set new row indexes in cloned row
            SetNewRowIndexes(newRow, rowIndex + 1);

            sheetData.InsertAfter(newRow, referencedRow);
            return newRow;
        }

        /// <summary>
        /// Sets new row indexes to cells in a given row.
        /// </summary>
        /// <param name="row">Row to update indexes in</param>
        /// <param name="newRowIndex">New index to set</param>
        private static void SetNewRowIndexes(Row row, uint newRowIndex)
        {
            IEnumerable<Cell> cells = row.Elements<Cell>();
            if (cells != null)
            {
                foreach (Cell cell in cells)
                {
                    string cellReference = cell.CellReference.Value;
                    cell.CellReference = new StringValue(cellReference.Replace(row.RowIndex.Value.ToString(), newRowIndex.ToString()));
                }
            }
        }

        /// <summary>
        /// Gets existing row by its number.
        /// </summary>
        /// <param name="rowNumber">Number of a row</param>
        /// <param name="sheetData">Sheet to search the row in</param>
        /// <returns>Row if exists otherwise null</returns>
        private Row GetExistingRow(int rowNumber, SheetData sheetData)
        {
            Row row = sheetData.ChildElements.OfType<Row>().FirstOrDefault(r => r.RowIndex == rowNumber);
            if (row != null)
            {
                return row;
            }
            throw new Exception("[DataExportHelper.GetExistingRow]: Row was not found.");
        }

        /// <summary>
        /// Sorts cells in a given row using cell's reference.
        /// </summary>
        /// <param name="row">Row to sort cells in</param>
        private static void SortCellsInRow(Row row)
        {
            if (row != null)
            {
                var cells = row.Elements<Cell>();
                if (cells != null)
                {
                    // Sort by column name
                    var cellList = cells.OrderBy(c => GetColumnNumber(GetColumnName(c.CellReference.Value))).Cast<OpenXmlElement>().ToList();

                    row.RemoveAllChildren();
                    row.Append(cellList);
                }
            }
        }

        /// <summary>
        /// Removes cell specified by cell reference from given row.
        /// </summary>
        /// <param name="row">Row to use for deletion</param>
        /// <param name="cellReference">Reference to a cell to be deleted</param>
        private void DeleteExistingCell(Row row, string cellReference)
        {
            // Extract cell from given row
            Cell cell = row.ChildElements.OfType<Cell>().FirstOrDefault(c => c.CellReference == cellReference);

            // Remove if exists
            if (cell != null)
            {
                cell.Remove();
            }
        }

        /// <summary>
        /// Creates cell based on given value and position.
        /// WTE: modified to add styleindex
        /// </summary>
        /// <param name="cellReference">Cell reference</param>
        /// <param name="cellValue">Value to add to cell</param>
        /// <returns>Created cell with value<returns>
        private Cell CreateCell(string cellReference, object cellValue, UInt32? styleindex = null)
        {
            Cell cell = null;
            if (styleindex.HasValue)
            {
                cell = new Cell() { StyleIndex = styleindex.GetValueOrDefault() };
            }
            else
            {
                cell = new Cell();
            }
            // Store data depending on type
            if ((cellValue is int) || (cellValue is long) || (cellValue is uint) ||
                (cellValue is ulong) || (cellValue is short) || (cellValue is ushort))
            {
                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                cell.CellValue = new CellValue(cellValue.ToString());
            }
            else if ((cellValue is float) || (cellValue is decimal) || (cellValue is double))
            {
                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                cell.CellValue = new CellValue(String.Format(CultureInfo.InvariantCulture, "{0}", cellValue));
            }
            else
            {
                string textValue = ResHelper.LocalizeString(ValidationHelper.GetString(cellValue, string.Empty));
                textValue = TextHelper.LimitLength(textValue, MAX_EXCEL_CELL_SIZE);
                if (UseSharedStringStorage)
                {
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    int index = StoreSharedStringItem(textValue);
                    cell.CellValue = new CellValue(index.ToString());
                }
                else
                {
                    AddInlineStringToCell(textValue, cell);
                }
            }
            // Set cell's column and row index reference
            cell.CellReference = cellReference;
            return cell;
        }

        #endregion "Row, column and cell manipulation"

        #region "Manipulation with inline and shared strings"

        /// <summary>
        /// Ensures shared string table part.
        /// </summary>
        /// <param name="workbookPart">Corresponding workbook part</param>
        /// <returns>New or existing shared string table part</returns>
        private static SharedStringTablePart EnsureSharedStringTablePart(WorkbookPart workbookPart)
        {
            return workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault() ?? workbookPart.AddNewPart<SharedStringTablePart>();
        }

        /// <summary>
        /// Removes shared string item from the shared string table.
        /// </summary>
        /// <param name="sharedStringId">Index of a shared string</param>
        /// <param name="workbookPart">Workbook part</param>
        private void RemoveSharedStringItem(int sharedStringId, WorkbookPart workbookPart)
        {
            SharedStringTablePart sharedStringTablePart = workbookPart.SharedStringTablePart;
            if (sharedStringTablePart != null)
            {
                if (!IsSharedStringReferenced(sharedStringId, workbookPart))
                {
                    SharedStringItem item = sharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(sharedStringId);
                    if (item != null)
                    {
                        // Remove the item
                        item.Remove();

                        // Remove the item from cache
                        RemoveCachedString(item.Text.InnerText);

                        // Refresh all the shared string references
                        foreach (WorksheetPart part in workbookPart.GetPartsOfType<WorksheetPart>())
                        {
                            Worksheet worksheet = part.Worksheet;
                            foreach (Cell cell in worksheet.GetFirstChild<SheetData>().Descendants<Cell>())
                            {
                                if ((cell.DataType != null) && (cell.DataType.Value == CellValues.SharedString))
                                {
                                    int itemIndex = int.Parse(cell.CellValue.Text);
                                    if (itemIndex > sharedStringId)
                                    {
                                        cell.CellValue.Text = (itemIndex - 1).ToString();
                                    }
                                }
                            }
                            worksheet.Save();
                        }

                        workbookPart.SharedStringTablePart.SharedStringTable.Save();
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether string given by identifier is being referenced.
        /// </summary>
        /// <param name="sharedStringId">Identifier of a shared string</param>
        /// <param name="workbookPart">Workbook part</param>
        /// <returns>TRUE if the string is referenced</returns>
        private static bool IsSharedStringReferenced(int sharedStringId, WorkbookPart workbookPart)
        {
            foreach (WorksheetPart part in workbookPart.GetPartsOfType<WorksheetPart>())
            {
                Worksheet worksheet = part.Worksheet;
                foreach (Cell cell in worksheet.GetFirstChild<SheetData>().Descendants<Cell>())
                {
                    // Verify if other cells in the document reference the item
                    if ((cell.DataType != null) && (cell.DataType.Value == CellValues.SharedString) && (cell.CellValue.Text == sharedStringId.ToString()))
                    {
                        // If so, do not remove the item
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Adds text as inline string to given cell.
        /// </summary>
        /// <param name="cellValue">Value to add as a inline string</param>
        /// <param name="cell">Cell to be used for addition</param>
        private static void AddInlineStringToCell(string cellValue, Cell cell)
        {
            // Set data type
            cell.DataType = new EnumValue<CellValues>(CellValues.InlineString);

            // Create text value
            Text text = new Text(cellValue);

            // Append text value to inline string
            InlineString inlineString = new InlineString();
            inlineString.AppendChild(text);

            // Append inline string to cell
            cell.AppendChild(inlineString);
        }

        #region "Shared string caching"

        /// <summary>
        /// Clears collection with cached shared strings.
        /// </summary>
        private void ClearSharedStringsCache()
        {
            sharedStringCache = new Dictionary<string, int>();
        }

        /// <summary>
        /// Removes cached string.
        /// </summary>
        /// <param name="text">Text to remove</param>
        private void RemoveCachedString(string text)
        {
            if (sharedStringCache.ContainsKey(text))
            {
                // Get existing index
                int pivot = sharedStringCache[text];
                Dictionary<string, int> temp = new Dictionary<string, int>();

                // Find values with bigger index
                foreach (KeyValuePair<string, int> pair in sharedStringCache)
                {
                    if (pair.Value > pivot)
                    {
                        temp[pair.Key] = pair.Value - 1;
                    }
                }
                // Replace old values
                foreach (KeyValuePair<string, int> pair in temp)
                {
                    sharedStringCache[pair.Key] = pair.Value;
                }
                // Remove desired string
                sharedStringCache.Remove(text);
            }
        }

        /// <summary>
        /// Caches existing shared string table part.
        /// </summary>
        /// <param name="workbookPart">Workbook part</param>
        private void CacheExistingSharedStrings(WorkbookPart workbookPart)
        {
            SharedStringTablePart sharedStringTablePart = EnsureSharedStringTablePart(workbookPart);
            if (sharedStringTablePart.SharedStringTable != null)
            {
                foreach (SharedStringItem item in sharedStringTablePart.SharedStringTable.Elements<SharedStringItem>())
                {
                    StoreSharedStringItem(item.InnerText);
                }
            }
        }

        /// <summary>
        /// Saves string to string cache to be used later during creation of shared string table part.
        /// </summary>
        /// <param name="text">Text to insert</param>
        /// <returns>Index of shared string item</returns>
        private int StoreSharedStringItem(string text)
        {
            if (sharedStringCache.ContainsKey(text))
            {
                // Get existing index
                return sharedStringCache[text];
            }
            else
            {
                // Create new index
                return (sharedStringCache[text] = sharedStringCache.Count);
            }
        }

        /// <summary>
        /// Fills given string table part with strings cached during sheet creation.
        /// </summary>
        /// <param name="workbookPart">Workbook part</param>
        private void FillStringTable(WorkbookPart workbookPart)
        {
            SharedStringTablePart sharedStringTablePart = EnsureSharedStringTablePart(workbookPart);
            // Create new shared string table
            sharedStringTablePart.SharedStringTable = new SharedStringTable();

            // Sort the shared strings collection
            IOrderedEnumerable<KeyValuePair<string, int>> sortedDic = sharedStringCache.OrderBy(x => x.Value);

            // Convert the collection to table
            foreach (KeyValuePair<string, int> item in sortedDic)
            {
                sharedStringTablePart.SharedStringTable.AppendChild(new SharedStringItem(new Text(item.Key)));
            }

            if (sharedStringTablePart.RootElement == null)
            {
                // Remove shared string table if empty
                workbookPart.DeletePart(sharedStringTablePart);
            }
            else
            {
                // Save shared string table
                sharedStringTablePart.SharedStringTable.Save();
            }
        }

        #endregion "Shared string caching"

        #endregion "Manipulation with inline and shared strings"

        #region "Workbook and sheet manipulation"

        /// <summary>
        /// Inserts new worksheet part to a workbook part.
        /// </summary>
        /// <param name="workbookPart">Workbook part to use</param>
        /// <param name="sheetName">Name of new sheet</param>
        /// <returns>Worksheet part</returns>
        private static WorksheetPart InsertWorksheet(WorkbookPart workbookPart, string sheetName)
        {
            // Add a new worksheet part to the workbook
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique identifier for the new sheet
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Any())
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            sheetName = GetSheetName(sheets, sheetName, sheetId);

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet { Id = relationshipId, SheetId = sheetId, Name = sheetName };

            sheets.Append(sheet);

            workbookPart.Workbook.Save();

            return newWorksheetPart;
        }

        /// <summary>
        /// Gets unique sheet name
        /// </summary>
        /// <param name="sheets">Collection of sheets</param>
        /// <param name="sheetName">Suggested name of the sheet</param>
        /// <param name="sheetId">Sheet identifier</param>
        /// <returns>Unique sheet name</returns>
        private static string GetSheetName(Sheets sheets, string sheetName, uint sheetId)
        {
            string originalSheetName = sheetName;
            while (sheets.Elements<Sheet>().FirstOrDefault(s => s.Name == sheetName) != null)
            {
                sheetName = originalSheetName + " (" + sheetId + ")";
                sheetId++;
            }
            return sheetName;
        }

        #endregion "Workbook and sheet manipulation"

        #region "Publicly visible helper methods for Excel row, column and cell naming and numbering"

        /// <summary>
        /// Returns name of column from given cell reference (ex. "B4" -> "B")
        /// </summary>
        /// <param name="cellReference">Cell reference in LETTERNUMBER format</param>
        /// <returns>Name of a column</returns>
        public static string GetColumnName(string cellReference)
        {
            if (cellReference == null)
            {
                return "A";
            }
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = RegexHelper.GetRegex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        }

        /// <summary>
        /// Returns number of a row from given cell reference (ex. "B4" -> "4")
        /// </summary>
        /// <param name="cellReference">Cell reference in LETTERNUMBER format</param>
        /// <returns>Number of a row</returns>
        public static int GetRowNumber(string cellReference)
        {
            if (cellReference == null)
            {
                return 0;
            }
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = RegexHelper.GetRegex("\\d+");
            Match match = regex.Match(cellReference);
            return int.Parse(match.Value);
        }

        /// <summary>
        /// Creates column reference from column and row numbers.
        /// </summary>
        /// <param name="columnNumber">Number of a column</param>
        /// <param name="rowNumber">Number of a row</param>
        /// <returns>Cell reference in LETTERNUMBER format</returns>
        public static string CreateColumnReference(int columnNumber, int rowNumber)
        {
            return CreateColumnName(columnNumber) + rowNumber;
        }

        /// <summary>
        /// Creates column name from column number ("1" -> "A")
        /// </summary>
        /// <param name="columnNumber">Number of column</param>
        /// <returns>Name of column</returns>
        public static string CreateColumnName(int columnNumber)
        {
            int remaining = columnNumber;
            string columnName = string.Empty;

            while (remaining > 0)
            {
                // Get char offset in ASCII table (starting with 'A')
                int modifier = (remaining - 1) % LETTER_COUNT;

                // Append char to name (based on ASCII number)
                columnName = Convert.ToChar(ASCII_BEGINNING + modifier) + columnName;

                // Calculate new remaining number
                remaining = (remaining - modifier) / LETTER_COUNT;
            }

            return columnName;
        }

        /// <summary>
        /// Gets number of given column name ("A" -> "1")
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <returns>Number of column</returns>
        public static int GetColumnNumber(string columnName)
        {
            int[] digits = new int[columnName.Length];
            for (int i = 0; i < columnName.Length; ++i)
            {
                digits[i] = Convert.ToInt32(columnName[i]) - 64;
            }
            int mul = 1;
            int res = 0;
            for (int pos = digits.Length - 1; pos >= 0; --pos)
            {
                res += digits[pos] * mul;
                mul *= LETTER_COUNT;
            }
            return res;
        }

        #endregion "Publicly visible helper methods for Excel row, column and cell naming and numbering"

        #endregion "XLSX Export"

        #region "CSV Export"

        /// <summary>
        /// Exports first table of given DataSet to CSV format.
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="stream">Stream to write to</param>
        private void ExportToCSV(DataSet dataSet, SystemIO.Stream stream)
        {
            ExportToCSV(dataSet, 0, stream);
        }

        /// <summary>
        /// Exports specified table of given DataSet to CSV format.
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="tableIndex">Index of table to export</param>
        /// <param name="stream">Stream to write to</param>
        public void ExportToCSV(DataSet dataSet, int tableIndex, SystemIO.Stream stream)
        {
            ExportToCSV(dataSet, 0, stream, false);
        }

        /// <summary>
        /// Exports specified table of given DataSet to CSV format.
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="tableIndex">Index of table to export</param>
        /// <param name="stream">Stream to write to</param>
        /// <param name="returnAsArray">If true DataSet is returned in byte array format</param>
        public byte[] ExportToCSV(DataSet dataSet, int tableIndex, SystemIO.Stream stream, bool returnAsArray)
        {
            CMS.IO.StreamWriter csvWriter = null;
            byte[] result = null;

            try
            {
                // Get the exported data
                dataSet = GetExportedData(dataSet, false);

                // Get data view
                DataTable dataTable = dataSet.Tables[tableIndex];

                csvWriter = CMS.IO.StreamWriter.New(stream, Encoding.UTF8);

                if (GenerateHeader)
                {
                    bool first = true;

                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        // Add separator
                        if (!first)
                        {
                            csvWriter.Write(CSVDelimiter);
                        }

                        // Create and append header value
                        string value = PrepareCSVValue(dc.Caption, first);

                        csvWriter.Write(value);

                        first = false;
                    }

                    csvWriter.WriteLine();
                }
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    bool first = true;

                    // Append cells (columns) to the row
                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        // Add separator
                        if (!first)
                        {
                            csvWriter.Write(CSVDelimiter);
                        }

                        // Apply external data bound if set
                        var value = PrepareCSVValue(dataRow[dc.ColumnName], first);

                        // Append value
                        csvWriter.Write(value);

                        first = false;
                    }

                    csvWriter.WriteLine();
                }
            }
            finally
            {
                if (csvWriter != null)
                {
                    csvWriter.Flush();

                    if (returnAsArray)
                    {
                        stream.Position = 0;
                        result = new byte[stream.Length];
                        stream.Read(result, 0, (int)stream.Length);
                    }

                    csvWriter.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Performs necessary steps in order to create valid CSV value.
        /// </summary>
        /// <param name="value">Value to transform</param>
        /// <param name="first">Flag whether the item is the first item in the output</param>
        /// <returns>Valid CSV value</returns>
        private string PrepareCSVValue(object value, bool first)
        {
            // Properly escape the value
            string valueString = ResHelper.LocalizeString(ValidationHelper.GetString(value, string.Empty));

            bool containsNewLine = valueString.IndexOfAny(new[] { '\n', '\r' }) >= 0;
            bool containsQuote = valueString.Contains("\"");

            if (containsQuote)
            {
                // Double-quote quotes
                valueString = valueString.Replace("\"", "\"\"");
            }
            bool hasLeadingOrTrailingWhiteSpaces = (valueString != valueString.Trim());

            // Detect SYLK file format to fix bug in Excel
            bool isSYLK = (first && valueString.StartsWithCSafe("ID"));

            if (hasLeadingOrTrailingWhiteSpaces || containsQuote || containsNewLine || isSYLK || valueString.Contains(CSVDelimiter))
            {
                // Enclose string in quotes
                valueString = "\"" + valueString + "\"";
            }

            if (isSYLK)
            {
                valueString = "\"" + valueString + "\"";
            }

            return valueString;
        }

        #endregion "CSV Export"

        #region "XML Export"

        /// <summary>
        /// Exports given DataSet to XML format.
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="stream">Stream to write to</param>
        private void ExportToXML(DataSet dataSet, SystemIO.Stream stream)
        {
            XmlTextWriter xmlTextWriter = null;
            try
            {
                // Get the exported data
                dataSet = GetExportedData(dataSet, false);

                xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement(dataSet.DataSetName);

                foreach (DataTable dataTable in dataSet.Tables)
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        xmlTextWriter.WriteStartElement(dataTable.TableName);

                        // Append cells (columns) to the row
                        foreach (DataColumn dc in dataTable.Columns)
                        {
                            string startElementName = GetXMLElementName(dc.Caption);

                            xmlTextWriter.WriteStartElement(startElementName);

                            // Apply external data bound if set
                            var value = ResHelper.LocalizeString(ValidationHelper.GetString(dataRow[dc.ColumnName], ""));

                            // Append value
                            xmlTextWriter.WriteString(value);
                            xmlTextWriter.WriteEndElement();
                        }
                        xmlTextWriter.WriteEndElement();
                    }
                }
                xmlTextWriter.WriteEndElement();
            }
            finally
            {
                if (xmlTextWriter != null)
                {
                    xmlTextWriter.Flush();
                    xmlTextWriter.Close();
                }
            }
        }

        /// <summary>
        /// Normalizes header text to be usable as element name.
        /// </summary>
        /// <param name="headerText">Text to normalize</param>
        /// <returns>Valid element name</returns>
        protected string GetXMLElementName(string headerText)
        {
            headerText = TextHelper.RemoveDiacritics(headerText);
            string result = null;

            foreach (string word in headerText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                result += word.Substring(0, 1).ToUpperCSafe() + word.Substring(1);
            }
            result = TextHelper.ReduceWhiteSpaces(result, string.Empty);
            result = ValidationHelper.GetIdentifier(result, string.Empty);

            if (!ValidationHelper.IsIdentifier(result))
            {
                // Try make an valid element name
                result = "_" + result;
            }

            return result;
        }

        #endregion "XML Export"

        #region "Generic helper methods"

        /// <summary>
        /// Gets the DataSet with the formatted exported data
        /// </summary>
        /// <param name="dataSet">DataSet to export</param>
        /// <param name="encode">If true, the values in the DataSet will be encoded</param>
        protected virtual DataSet GetExportedData(DataSet dataSet, bool encode = true)
        {
            DataSet resultDS = new DataSet(dataSet.DataSetName);

            foreach (DataTable dt in dataSet.Tables)
            {
                string tableName = dt.TableName;
                List<string> exportedColumns = GetExportedColumns(tableName);

                // Build output table with all columns as strings
                DataTable resultDT = new DataTable(tableName);

                resultDS.Tables.Add(resultDT);

                foreach (string columnName in exportedColumns)
                {
                    var resultDC = resultDT.Columns.Add(columnName, typeof(string));

                    // Initialize the column caption
                    int columnIndex = GetColumnIndex(tableName, columnName);

                    string columnCaption = GetColumnCaptionText(columnIndex, dt.TableName);
                    if (string.IsNullOrEmpty(columnCaption))
                    {
                        columnCaption = DEFAULT_COLUMN_NAME + columnIndex;
                    }
                    resultDC.Caption = columnCaption;
                }

                var dv = dt.DefaultView;

                // Process all data
                foreach (DataRowView drv in dv)
                {
                    DataRow resultDR = resultDT.NewRow();
                    resultDT.Rows.Add(resultDR);

                    // Prepare the exported data for row
                    object[] items = new object[exportedColumns.Count];
                    int index = 0;

                    foreach (string columnName in exportedColumns)
                    {
                        // Apply external data bound if set
                        var value = GetExportedValue(drv, columnName, resultDR);

                        string valueString = ValidationHelper.GetString(value, string.Empty);
                        if (encode)
                        {
                            valueString = HTMLHelper.HTMLEncode(valueString);
                        }

                        // Save result to exported data
                        items[index++] = valueString;
                    }

                    resultDR.ItemArray = items;
                }
            }

            return resultDS;
        }

        /// <summary>
        /// Gets the exported value for the given
        /// </summary>
        /// <param name="drv">Data row with the source data</param>
        /// <param name="columnName">Column name</param>
        /// <param name="resultDR">DataRow with the result</param>
        protected virtual object GetExportedValue(DataRowView drv, string columnName, DataRow resultDR)
        {
            return (ExternalDataBound != null) ? ExternalDataBound(drv, GetColumnIndex(drv.Row.Table.TableName, columnName)) : drv[columnName];
        }

        /// <summary>
        /// Gets file name with extension.
        /// </summary>
        /// <param name="format">Format of a file</param>
        /// <returns>File name with extension</returns>
        private string GetFileName(DataExportFormatEnum format)
        {
            string fileName;
            if (!string.IsNullOrEmpty(FileNameWithExtension))
            {
                fileName = FileNameWithExtension;
            }
            else
            {
                fileName = FileName + "." + format.ToString().ToLowerCSafe();
            }
            return fileName;
        }

        #endregion "Generic helper methods"

        #endregion "Internal export methods"

        #region "DataExportFormatEnum manipulation"

        /// <summary>
        /// Returns DataExportFormat enum.
        /// </summary>
        /// <param name="dataExportFormat">Data export format</param>
        public static DataExportFormatEnum GetDataExportFormatEnum(string dataExportFormat)
        {
            if (dataExportFormat == null)
            {
                return DataExportFormatEnum.XLSX;
            }

            switch (dataExportFormat.ToLowerCSafe())
            {
                case "xlsx":
                    return DataExportFormatEnum.XLSX;

                case "csv":
                    return DataExportFormatEnum.CSV;

                case "xml":
                    return DataExportFormatEnum.XML;

                default:
                    return DataExportFormatEnum.XLSX;
            }
        }

        /// <summary>
        /// Returns DataExportFormat string.
        /// </summary>
        /// <param name="dataExportFormat">Data export format</param>
        public static string GetDataExportFormatString(DataExportFormatEnum dataExportFormat)
        {
            switch (dataExportFormat)
            {
                case DataExportFormatEnum.XLSX:
                    return "XLSX";

                case DataExportFormatEnum.CSV:
                    return "CSV";

                case DataExportFormatEnum.XML:
                    return "XML";

                default:
                    return "XLSX";
            }
        }

        /// <summary>
        /// Returns content type for given format.
        /// </summary>
        /// <param name="dataExportFormat">Data export format</param>
        public static string GetDataExportFormatContentType(DataExportFormatEnum dataExportFormat)
        {
            switch (dataExportFormat)
            {
                case DataExportFormatEnum.XLSX:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                case DataExportFormatEnum.CSV:
                    return "text/csv";

                case DataExportFormatEnum.XML:
                    return "application/xml";

                default:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
        }

        #endregion "DataExportFormatEnum manipulation"
    }

    #endregion "Customized data export helper"

    /// <summary>
    /// Customize CMS Reporting control (Report table)
    /// </summary>
    public partial class WTE_ReportTable : AbstractReportControl
    {
        #region "Variables"

        private UIGridView mUIGridView;
        private ReportTableInfo mTableInfo;
        private string mParameter = String.Empty;
        private ReportInfo mReportInfo;

        /// <summary>
        /// Indicates whether exception was thrown during data loading
        /// </summary>
        private bool mErrorOccurred;

        #endregion "Variables"

        #region "Properties"

        /// <summary>
        /// Gets the UIGridView object.
        /// </summary>
        protected UIGridView UIGridViewObject
        {
            get
            {
                return mUIGridView ?? (mUIGridView = new UIGridView());
            }
        }

        /// <summary>
        /// Report table connection string
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                String tableConn = (TableInfo == null) ? String.Empty : TableInfo.TableConnectionString;
                if (String.IsNullOrEmpty(tableConn))
                {
                    return (mReportInfo == null) ? String.Empty : mReportInfo.ReportConnectionString;
                }

                return tableConn;
            }
        }

        /// <summary>
        /// Table name - prevent using viewstate (problems with displayreportcontrol and postback).
        /// </summary>
        public override string Parameter
        {
            get
            {
                return mParameter;
            }
            set
            {
                mParameter = value;
            }
        }

        /// <summary>
        /// Direct table info used by preview.
        /// </summary>
        public ReportTableInfo TableInfo
        {
            get
            {
                return mTableInfo ?? (mTableInfo = ReportTableInfoProvider.GetReportTableInfo(Parameter));
            }
            set
            {
                mTableInfo = value;
            }
        }

        /// <summary>
        /// Page size for paged tables
        /// </summary>
        public int PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/disables paging (if null report settings is used)
        /// </summary>
        public bool? EnablePaging
        {
            get;
            set;
        }

        #endregion "Properties"

        #region "Control events"

        /// <summary>
        /// Created grid view based on parameter from report table.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            ItemType = ReportItemType.Table;
        }

        protected override void OnLoad(EventArgs e)
        {
            UIGridViewObject.RowDataBound += UIGridViewObject_RowDataBound;

            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (mReportInfo != null)
            {
                if (TableInfo != null)
                {
                    EnableSubscription = (EnableSubscription && ValidationHelper.GetBoolean(TableInfo.TableSettings["SubscriptionEnabled"], true) && mReportInfo.ReportEnableSubscription);
                    EnableExport = (EnableExport && ValidationHelper.GetBoolean(TableInfo.TableSettings["ExportEnabled"], false));
                    // Register context menu for export - if allowed
                    RegisterSubscriptionScript(TableInfo.TableReportID, "tableid", TableInfo.TableID, menuCont);
                }

                // Export data
                if (!mErrorOccurred)
                {
                    ProcessExport2(ValidationHelper.GetCodeName(mReportInfo.ReportDisplayName));
                }
            }

            base.OnPreRender(e);

            CssRegistration.RegisterBootstrap(Page);
        }

        /// <summary>
        /// Export the selected dataset
        /// </summary>
        public void ProcessExport2(String displayName)
        {
            if (EnableExport && !EmailMode)
            {
                // If export allowed test postback with control unique ID
                string argument = Request[System.Web.UI.Page.postEventArgumentID];
                string target = Request[System.Web.UI.Page.postEventSourceID];
                if (target == UniqueID)
                {
                    // Send dataset to response stream
                    var deh = new DataExportHelper_WTE(ReportDataSet);
                    deh.FileName = displayName;
                    deh.ExportData(DataExportHelper_WTE.GetDataExportFormatEnum(argument), Page.Response);
                }
            }
        }

        /// <summary>
        /// Handles paging on live site.
        /// </summary>
        protected void GridViewObject_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
        }

        protected void UIGridViewObject_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Hide time information for all dates in day interval mode
            if ((SelectedInterval != null) && SelectedInterval.Equals("day", StringComparison.CurrentCultureIgnoreCase))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    DataRow drv = ((DataRowView)e.Row.DataItem).Row;

                    foreach (DataColumn column in drv.Table.Columns)
                    {
                        if (column.DataType.FullName.Equals("system.datetime", StringComparison.CurrentCultureIgnoreCase))
                        {
                            DateTime date = ValidationHelper.GetDateTime(drv[column.Ordinal], DateTime.Now);

                            e.Row.Cells[column.Ordinal].Text = date.Date.ToShortDateString();
                        }
                    }
                }
            }
        }

        #endregion "Control events"

        #region "Public methods"

        /// <summary>
        /// Reload data.
        /// </summary>
        public override void ReloadData(bool forceLoad)
        {
            if ((TableInfo == null) || ((GraphImageWidth != 0) && (ComputedWidth == 0)))
            {
                // Graph width is computed no need to create graph
                return;
            }

            Visible = true;

            EnsureChildControls();

            LoadTable();

            mReportInfo = ReportInfoProvider.GetReportInfo(TableInfo.TableReportID);
            if (mReportInfo == null)
            {
                return;
            }

            // Check security settings
            if (!(CheckReportAccess(mReportInfo) && CheckEmailModeSubscription(mReportInfo, ValidationHelper.GetBoolean(TableInfo.TableSettings["SubscriptionEnabled"], true))))
            {
                Visible = false;
                return;
            }

            // Prepare query attributes
            QueryIsStoredProcedure = TableInfo.TableQueryIsStoredProcedure;
            QueryText = TableInfo.TableQuery;

            // Init parameters
            InitParameters(mReportInfo.ReportParameters);

            // Init macro resolver
            InitResolver();

            mErrorOccurred = false;
            DataSet ds = null;

            // Ensure report item name for caching
            if (String.IsNullOrEmpty(ReportItemName))
            {
                ReportItemName = String.Format("{0};{1}", mReportInfo.ReportName, TableInfo.TableName);
            }

            try
            {
                // Load data
                ds = LoadData();
            }
            catch (Exception ex)
            {
                // Display error message, if data load fail
                lblError.Visible = true;
                lblError.Text = "Error loading the data: " + ex.Message;
                EventLogProvider.LogException("Report table", "E", ex);
                mErrorOccurred = true;
            }

            // If no data load, set empty dataset
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                if (EmailMode && SendOnlyNonEmptyDataSource)
                {
                    Visible = false;
                    return;
                }

                string noRecordText = ValidationHelper.GetString(TableInfo.TableSettings["QueryNoRecordText"], String.Empty);
                if (!String.IsNullOrEmpty(noRecordText))
                {
                    UIGridViewObject.Visible = false;
                    lblInfo.Text = ResolveMacros(noRecordText);
                    lblInfo.Visible = true;
                    EnableExport = false;
                    return;
                }

                if (!EmailMode)
                {
                    Visible = false;
                    return;
                }
            }
            else
            {
                UIGridViewObject.Visible = true;
                // Resolve macros in column names
                int i = 0;
                foreach (DataColumn dc in ds.Tables[0].Columns)
                {
                    if (dc.ColumnName == "Column" + (i + 1))
                    {
                        dc.ColumnName = ResolveMacros(ds.Tables[0].Rows[0][i].ToString());
                    }
                    else
                    {
                        dc.ColumnName = ResolveMacros(dc.ColumnName);
                    }
                    i++;
                }

                // Resolve macros in dataset
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    foreach (DataColumn dc in ds.Tables[0].Columns)
                    {
                        if (dc.DataType.FullName.Equals("system.string", StringComparison.CurrentCultureIgnoreCase))
                        {
                            dr[dc.ColumnName] = ResolveMacros(ValidationHelper.GetString(dr[dc.ColumnName], String.Empty));
                        }
                    }
                }

                if (EmailMode)
                {
                    // For some email formats, export data in csv format
                    EmailFormatEnum format = EmailHelper.GetEmailFormat(ReportSubscriptionSiteID);

                    if ((format == EmailFormatEnum.Both) || (format == EmailFormatEnum.PlainText))
                    {
                        using (var ms = new SystemIO.MemoryStream())
                        {
                            DataExportHelper deh = new DataExportHelper(ds);
                            byte[] data = deh.ExportToCSV(ds, 0, ms, true);
                            ReportSubscriptionSender.AddToRequest(mReportInfo.ReportName, "t" + TableInfo.TableName, data);
                        }
                    }

                    // For plain text email show table only as attachment
                    if (format == EmailFormatEnum.PlainText)
                    {
                        menuCont.Visible = false;
                        ltlEmail.Visible = true;
                        ltlEmail.Text = String.Format(GetString("reportsubscription.attachment"), TableInfo.TableName);
                        return;
                    }

                    GenerateTableForEmail(ds);
                    menuCont.Visible = false;
                    return;
                }
            }

            // Databind to gridview control
            UIGridViewObject.DataSource = ds;
            EnsurePageIndex();
            UIGridViewObject.DataBind();

            if ((TableFirstColumnWidth != Unit.Empty) && (UIGridViewObject.Rows.Count > 0))
            {
                UIGridViewObject.Rows[0].Cells[0].Width = TableFirstColumnWidth;
            }
        }

        /// <summary>
        /// Returns true if graph belongs to report.
        /// </summary>
        /// <param name="report">Report to validate</param>
        public override bool IsValid(ReportInfo report)
        {
            ReportTableInfo rti = TableInfo;

            if ((report != null) && (rti != null) && (report.ReportID == rti.TableReportID))
            {
                return true;
            }

            return false;
        }

        #endregion "Public methods"

        #region "Private methods"

        /// <summary>
        /// Adds GridView to the controls collection.
        /// </summary>
        private void LoadTable()
        {
            if (TableInfo == null)
            {
                return;
            }

            UIGridViewObject.AllowPaging = DisplayPaging();

            if (UIGridViewObject.AllowPaging)
            {
                // Webpart - higher priority
                UIGridViewObject.PageSize = (PageSize > 0) ? PageSize : ValidationHelper.GetInteger(TableInfo.TableSettings["pagesize"], 10);
                UIGridViewObject.PagerSettings.Mode = (PagerButtons)ValidationHelper.GetInteger(TableInfo.TableSettings["pagemode"], (int)PagerButtons.Numeric);
                UIGridViewObject.PageIndexChanging += GridViewObject_PageIndexChanging;
            }
            UIGridViewObject.AllowSorting = false;

            // Get SkinID from reportTable custom data
            string skinId = ValidationHelper.GetString(TableInfo.TableSettings["skinid"], "ReportGridAnalytics");
            if (skinId != String.Empty)
            {
                if (String.IsNullOrEmpty((UIGridViewObject.SkinID)))
                {
                    UIGridViewObject.SkinID = skinId;
                }
            }

            UIGridViewObject.ID = "reportGrid";

            // Add grid view control to the page
            plcGrid.Controls.Clear();
            plcGrid.Controls.Add(UIGridViewObject);

            if (RenderCssClasses && String.IsNullOrEmpty(UIGridViewObject.SkinID))
            {
                //Clear the css styles to eliminate control state
                UIGridViewObject.HeaderStyle.CssClass = String.Empty;
                UIGridViewObject.CssClass = String.Empty;
                UIGridViewObject.RowStyle.CssClass = String.Empty;
                UIGridViewObject.AlternatingRowStyle.CssClass = String.Empty;
            }
        }

        /// <summary>
        /// Indicates if paging should be visible.
        /// </summary>
        private bool DisplayPaging()
        {
            if (EmailMode || (SavedReportID > 0))
            {
                // Hide pager in email mode or for saved reports
                return false;
            }

            // EnablePaging property has higher priority - if not set, use report settings
            return EnablePaging ?? ValidationHelper.GetBoolean(TableInfo.TableSettings["enablepaging"], false);
        }

        /// <summary>
        /// Generates table for email
        /// </summary>
        /// <param name="ds">Dataset with table data</param>
        private void GenerateTableForEmail(DataSet ds)
        {
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            ltlEmail.Visible = true;

            StringBuilder sb = new StringBuilder();

            // Generate header
            sb.Append("<table class=\"table\"><thead><tr class=\"unigrid-head\">");
            DataTable dt = ds.Tables[0];

            foreach (DataColumn dc in dt.Columns)
            {
                sb.Append("<th scope=\"col\">" + dc.ColumnName + "</th>");
            }

            sb.Append("</tr></thead><tbody>");

            int rowNumber = 1;

            // Generate rows
            foreach (DataRow dr in dt.Rows)
            {
                rowNumber++;
                string cssClass = (rowNumber % 2 == 0) ? "even-row" : "odd-row";
                sb.AppendFormat("<tr class=\"{0}\">", cssClass);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.AppendFormat("<td>{0}</td>", dr[i]);
                }

                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");

            ltlEmail.Text = sb.ToString();
        }

        /// <summary>
        /// Ensures the current page index with dependenco on request data du to different contol's life cycle.
        /// </summary>
        private void EnsurePageIndex()
        {
            if ((UIGridViewObject != null) && (UIGridViewObject.AllowPaging))
            {
                // Get current postback target
                string eventTarget = Request.Params[System.Web.UI.Page.postEventSourceID];

                // Handle paging manually because of lifecycle of the control
                if (CMSString.Compare(eventTarget, UIGridViewObject.UniqueID, true) == 0)
                {
                    // Get the current page value
                    string eventArg = ValidationHelper.GetString(Request.Params[System.Web.UI.Page.postEventArgumentID], String.Empty);

                    string[] args = eventArg.Split('$');
                    if ((args.Length == 2) && (CMSString.Compare(args[0], "page", true) == 0))
                    {
                        string pageValue = args[1];
                        int pageIndex = 0;
                        // Switch by page value  0,1.... first,last
                        switch (pageValue.ToLowerInvariant())
                        {
                            // Last item
                            case "last":
                                // Check whether page count is available
                                if (UIGridViewObject.PageCount > 0)
                                {
                                    pageIndex = UIGridViewObject.PageCount - 1;
                                }
                                // if page count is not defined, try compute page count
                                else
                                {
                                    DataSet ds = UIGridViewObject.DataSource as DataSet;
                                    if (!DataHelper.DataSourceIsEmpty(ds))
                                    {
                                        pageIndex = ds.Tables[0].Rows.Count / UIGridViewObject.PageSize;
                                    }
                                }
                                break;

                            case "next":
                                pageIndex = UIGridViewObject.PageIndex + 1;
                                break;

                            case "prev":
                                pageIndex = UIGridViewObject.PageIndex - 1;
                                break;

                            // Page number
                            default:
                                pageIndex = ValidationHelper.GetInteger(pageValue, 1) - 1;
                                break;
                        }

                        UIGridViewObject.PageIndex = pageIndex;
                    }
                }
            }
        }

        #endregion "Private methods"
    }
}