using System.Web.UI.DataVisualization.Charting;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Class containing the default report graph settings
    /// </summary>
    public static class ReportGraphDefaults
    {
        #region "General settings"
        /// <summary>
        /// Indicates whether the graph export is enabled.
        /// </summary>
        public const bool ExportEnable = true;

        /// <summary>
        /// Indicates whether the graph subscription is enabled.
        /// </summary>
        public const bool Subscription = true;

        /// <summary>
        /// Default connection string value
        /// </summary>
        public const string SelectConnectionString = null;

        #endregion


        #region "Border style"

        /// <summary>
        /// The series border style
        /// </summary>
        public const string SeriesBorderStyle = "NotSet";

        /// <summary>
        /// The series line border style
        /// </summary>
        public const string SeriesLineBorderStyle = "Solid";

        /// <summary>
        /// The legend border style
        /// </summary>
        public const string LegendBorderStyle = "NotSet";

        /// <summary>
        /// The chart area border style
        /// </summary>
        public const string ChartAreaBorderStyle = "NotSet";

        /// <summary>
        /// The plot area border style
        /// </summary>
        public const string PlotAreaBorderStyle = "NotSet";

        /// <summary>
        /// The border skin style
        /// </summary>
        public const string BorderSkinStyle = "None";

        #endregion


        #region "Border size"

        /// <summary>
        /// The series line border size
        /// </summary>
        public const int SeriesLineBorderSize = 4;

        /// <summary>
        /// The series border size
        /// </summary>
        public const int SeriesBorderSize = 0;

        /// <summary>
        /// The chart area border size
        /// </summary>
        public const int ChartAreaBorderSize = 0;

        /// <summary>
        /// The plot area border size
        /// </summary>
        public const int PlotAreaBorderSize = 0;

        /// <summary>
        /// The legend border size
        /// </summary>
        public const int LegendBorderSize = 0;

        #endregion


        #region "Border color"

        /// <summary>
        /// The plot area border color
        /// </summary>
        public const string PlotAreaBorderColor = null;

        /// <summary>
        /// The chart area border color
        /// </summary>
        public const string ChartAreaBorderColor = null;

        /// <summary>
        /// The legend border color
        /// </summary>
        public const string LegendBorderColor = null;

        /// <summary>
        /// The series border color
        /// </summary>
        public const string SeriesBorderColor = null;

        #endregion


        #region "Other values"

        // Other values
        /// <summary>
        /// The chart area primary background color
        /// </summary>
        public const string ChartAreaPrBgColor = null;

        /// <summary>
        /// The chart area sec background color
        /// </summary>
        public const string ChartAreaSecBgColor = null;

        /// <summary>
        /// The chart area gradient
        /// </summary>
        public const string ChartAreaGradient = "None";

        /// <summary>
        /// The title color
        /// </summary>
        public const string TitleColor = null;

        /// <summary>
        /// The chart width
        /// </summary>
        public static readonly string ChartWidth = ReportGraph.DEFAULT_WIDTH.ToString();

        /// <summary>
        /// The chart height
        /// </summary>
        public static readonly string ChartHeight = ReportGraph.DEFAULT_HEIGHT.ToString();

        /// <summary>
        /// Show the main grid
        /// </summary>
        public const bool ShowGrid = true;

        /// <summary>
        /// The y axis use x settings
        /// </summary>
        public const bool YAxisUseXSettings = true;

        /// <summary>
        /// The x axis title color
        /// </summary>
        public const string XAxisTitleColor = null;

        /// <summary>
        /// The y axis title color
        /// </summary>
        public const string YAxisTitleColor = null;

        /// <summary>
        /// The series symbols
        /// </summary>
        public const string SeriesSymbols = "Circle";

        /// <summary>
        /// The bar drawing style
        /// </summary>
        public const string BarDrawingStyle = "Bar";

        /// <summary>
        /// The series display item value
        /// </summary>
        public const bool SeriesDisplayItemValue = true;

        /// <summary>
        /// The x axis interval
        /// </summary>
        public const int XAxisInterval = 1;

        /// <summary>
        /// The x axis sort
        /// </summary>
        public const bool XAxisSort = true;

        /// <summary>
        /// The pie label style
        /// </summary>
        public const string PieLabelStyle = "Outside";

        /// <summary>
        /// The pie doughnut radius
        /// </summary>
        public const string PieDoughnutRadius = "70";

        /// <summary>
        /// The pie drawing style
        /// </summary>
        public const string PieDrawingStyle = "Doughnut";

        /// <summary>
        /// The pie drawing design
        /// </summary>
        public const string PieDrawingDesign = "Default";

        /// <summary>
        /// The stacked bar drawing style
        /// </summary>
        public const string StackedBarDrawingStyle = "Bar";

        /// <summary>
        /// The graph legend position
        /// </summary>
        public const int GraphLegendPosition = 100;

        #endregion


        #region "Major grid"

        /// <summary>
        /// The major grid line color
        /// </summary>
        public const string MajorGridLineColor = "#d6d9d6";

        /// <summary>
        /// The major grid line dash style
        /// </summary>
        public const ChartDashStyle MajorGridLineDashStyle = ChartDashStyle.Dash;

        #endregion
    }
}
