using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI.DataVisualization.Charting;

using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;

using SystemIO = System.IO;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Summary description for ReportGraph.
    /// </summary>
    public class ReportGraph
    {
        #region "Variables"

        /// <summary>
        /// Default graph width.
        /// </summary>
        public const int DEFAULT_WIDTH = 600;

        /// <summary>
        /// Default graph height.
        /// </summary>
        public const int DEFAULT_HEIGHT = 400;

        /// <summary>
        /// Number of displayed records. 
        /// </summary>
        public const int RECORDS_DISPLAYED = 200;

        /// <summary>
        /// MS Chart.
        /// </summary>
        private Chart mChart;

        /// <summary>
        /// MacroResolver.
        /// </summary>
        private MacroResolver mResolver;

        /// <summary>
        /// Graph's data set.
        /// </summary>
        private DataSet mGraphDataSet;

        /// <summary>
        /// If true values of one point are show as percent (all get total 100 %).
        /// </summary>
        private bool mShowValueAsPercent;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Chart property.
        /// </summary>
        public Chart ChartControl
        {
            get
            {
                return mChart ?? (mChart = new Chart());
            }
            set
            {
                mChart = value;
            }
        }


        /// <summary>
        /// Correct graph width (if percent width is required).
        /// </summary>
        public int CorrectWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Colors assigned to series.
        /// </summary>
        public Dictionary<string, Color> Colors
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates chart.
        /// </summary>
        /// <param name="graphInfo">Chart's settings</param>
        /// <param name="dsGraphData">Data set with chart data</param>
        /// <param name="resolver">Helper for resolve macros</param>
        /// <param name="returnByteData">If true code returns byte data</param>
        public byte[] CreateChart(ReportGraphInfo graphInfo, DataSet dsGraphData, MacroResolver resolver, bool returnByteData = false)
        {
            if (DataHelper.DataSourceIsEmpty(dsGraphData))
            {
                return CreateInvalidDataGraph(graphInfo, "rep.nodata", returnByteData);
            }

            if (!ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSWebAnalyticsShowFullData"], false))
            {
                // Reduce the displayed data (display only points).
                int graphInterval = DataHelper.GetItemsCount(dsGraphData) / RECORDS_DISPLAYED;
                // Calculate average value for the time intervals.
                dsGraphData = DataHelper.ReduceDataSetData(dsGraphData, graphInterval, true, null);
            }

            var table = dsGraphData.Tables[0];
            var macros = new List<string[]>();

            // Replace macros in column names and string fields
            foreach (DataColumn column in table.Columns)
            {
                string colname = column.ColumnName;

                // Check whether macro delimiters are present
                if (colname.StartsWithCSafe("#@") && colname.Contains("@#"))
                {
                    // Get ending delimiter index
                    int endmacroIndex = colname.IndexOfCSafe("@#");

                    // Remove macro starting delimiter
                    string specMacrosStr = colname.Substring(2, endmacroIndex);

                    // Get column name
                    colname = colname.Substring(endmacroIndex + 2);

                    // Remove macro ending delimiter
                    specMacrosStr = specMacrosStr.Substring(0, specMacrosStr.Length - 2);
                    macros.Add(specMacrosStr.Split(';'));
                }
                else
                {
                    macros.Add(null);
                }

                // Resolve macros and trim the column name
                // There can be no spaces after or before the column name
                column.ColumnName = resolver.ResolveMacros(colname).Trim();

                if (column.DataType == typeof(string))
                {
                    foreach (DataRow dr in table.Rows)
                    {
                        dr[column] = resolver.ResolveMacros(DataHelper.GetStringValue(dr, column.ColumnName));
                    }
                }
            }

            Font defaultFont = new Font("Arial", 11, FontStyle.Bold);

            Chart chart = ChartControl;
            chart.Customize += mChart_Customize;
            mResolver = resolver;
            mGraphDataSet = dsGraphData;

            // Basic settings
            SetChartDimensions(chart, graphInfo);

            // Set chart custom palette
            chart.Palette = ChartColorPalette.None;
            chart.PaletteCustomColors = ReportGraphSeriesPalette.Colors;

            // Axis Tittles
            chart.ChartAreas.Add("MainChartArea");
            ChartArea mainArea = chart.ChartAreas["MainChartArea"];

            var axisX = mainArea.AxisX;
            var axisY = mainArea.AxisY;

            if (CultureHelper.IsUICultureRTL())
            {
                chart.RightToLeft = RightToLeft.Yes;
                axisX.IsReversed = true;
            }

            // Load data
            ReportCustomData settings = graphInfo.GraphSettings;

            mainArea.Area3DStyle.Enable3D = ValidationHelper.GetBoolean(settings["ShowAs3D"], false);
            var rotateY = settings["RotateY"];
            if (rotateY != null)
            {
                mainArea.Area3DStyle.Rotation = ValidationHelper.GetInteger(rotateY, 15);
            }

            var rotateX = settings["RotateX"];
            if (rotateX != null)
            {
                mainArea.Area3DStyle.Inclination = ValidationHelper.GetInteger(rotateX, 15);
            }

            // Loop through series. First series is labels, so start with index 1
            chart.DataSource = table;

            var firstColumn = table.Columns[0];
            var firstColumnName = firstColumn.ColumnName;
            var firstColumnType = firstColumn.DataType;

            for (int i = 1; i < table.Columns.Count; i++)
            {
                Series series = chart.Series.Add("Series" + i);

                // Set chart type and set specific attributes based on this type
                SetSerie(series, mainArea, graphInfo, macros[i]);

                series.XValueMember = firstColumnName;

                // Replacing ',' by ',,' allows us to display comma in column name because 
                // Chart treats single comma as a separator between multiple column names 
                series.YValueMembers = table.Columns[i].ColumnName.Replace(",", ",,");
                series.Name = table.Columns[i].ColumnName;
                if ((Colors != null) && (Colors.ContainsKey(series.Name)))
                {
                    series.Color = Colors[series.Name];
                }

                // Set item link
                string itemLink = settings["SeriesItemLink"];
                if (!String.IsNullOrEmpty(itemLink))
                {
                    series.Url = resolver.ResolveMacros(itemLink);
                }

                if (ValidationHelper.GetBoolean(settings["xaxissort"], false))
                {
                    series.IsXValueIndexed = true;
                    chart.DataManipulator.Sort(PointSortOrder.Ascending, "X", series);
                }

                //Set tooltip
                string toolTip = settings["SeriesItemToolTip"];
                if (!String.IsNullOrEmpty(toolTip))
                {
                    series.ToolTip = toolTip;
                }

                // Tooltip fixation
                if (firstColumnType == typeof(DateTime))
                {
                    series.XValueType = ChartValueType.DateTime;
                }
                else if (firstColumnType == typeof(String))
                {
                    series.XValueType = ChartValueType.String;
                }

                if (graphInfo.GraphType.ToLowerCSafe() == "pie")
                {
                    // Allow only one data serie at a time in pie charts
                    break;
                }
            }

            // Hide axis X end labels
            axisX.LabelStyle.IsEndLabelVisible = true;

            // Label formats
            axisX.LabelStyle.Format = mResolver.ResolveMacros(settings["XAxisFormat"]);
            axisY.LabelStyle.Format = mResolver.ResolveMacros(settings["YAxisFormat"]);

            // If set show the grid in chart
            if (ValidationHelper.GetBoolean(settings["ShowMajorGrid"], false))
            {
                SetDefaultGrid(axisX, true);
                SetDefaultGrid(axisY, true);
            }
            else
            {
                axisX.MajorGrid.Enabled = axisY.MajorGrid.Enabled = false;
            }

            // Set minor grid for y Axis            
            axisY.MinorTickMark.Enabled = true;

            // Title
            Title mainTitle = chart.Titles.Add(resolver.ResolveMacros(graphInfo.GraphTitle));
            Font ft = SetFont(settings["TitleFontNew"]);
            mainTitle.Font = ft ?? new Font("Arial", 14, FontStyle.Bold);

            Color titleColor = HTMLHelper.GetSafeColorFromHtml(settings["TitleColor"], Color.Empty);
            if (titleColor != Color.Empty)
            {
                mainTitle.ForeColor = titleColor;
            }

            mainTitle.Alignment = ConvertAlignment(ValidationHelper.GetString(settings["TitlePosition"], "Center"));

            // Legend
            Legend legend = chart.Legends.Add("Legend1");

            legend.BackColor = HTMLHelper.GetSafeColorFromHtml(settings["LegendBgColor"], Color.Empty);
            if (legend.BackColor == Color.Empty)
            {
                legend.BackColor = Color.Transparent;
            }

            string legendTitle = resolver.ResolveMacros(settings["LegendTitle"]);
            if (!String.IsNullOrEmpty(legendTitle))
            {
                legend.Title = legendTitle;
                legend.TitleSeparator = LegendSeparatorStyle.GradientLine;
                legend.TitleFont = new Font("Arial", 8, FontStyle.Regular);
            }

            legend.BorderColor = SetBorderColor("LegendBorderColor", settings);
            legend.BorderWidth = ValidationHelper.GetInteger(settings["LegendBorderSize"], ReportGraphDefaults.LegendBorderSize);
            legend.BorderDashStyle = (ChartDashStyle)Enum.Parse(typeof(ChartDashStyle), ValidationHelper.GetString(settings["LegendBorderStyle"], ReportGraphDefaults.LegendBorderStyle));

            SetLegendPosition(legend, settings["LegendPosition"]);
            legend.IsDockedInsideChartArea = ValidationHelper.GetBoolean(settings["LegendInside"], false);

            // Try to convert from old values
            ConvertLegendPosition(graphInfo, legend);
            if (settings["LegendPosition"] == "None")
            {
                legend.Enabled = false;
            }

            // XAxis
            axisX.Title = ValidationHelper.GetString(resolver.ResolveMacros(graphInfo.GraphXAxisTitle), String.Empty);
            Font xFont = SetFont(settings["XAxisFont"]);
            axisX.TitleFont = xFont ?? defaultFont;

            axisX.TitleForeColor = HTMLHelper.GetSafeColorFromHtml(settings["XAxisTitleColor"], Color.Black);

            // Label style font
            Font xLabelFont = SetFont(settings["XAxisLabelFont"]);
            if (xLabelFont != null)
            {
                axisX.LabelStyle.Font = xLabelFont;
            }

            axisX.TitleAlignment = (StringAlignment)Enum.Parse(typeof(StringAlignment), ValidationHelper.GetString(settings["XAxisTitlePosition"], "Center"));

            int interval = ValidationHelper.GetInteger(settings["XAxisInterval"], 0);
            if (interval != 0)
            {
                axisX.Interval = interval;
            }

            // YAxis
            axisY.Title = ValidationHelper.GetString(resolver.ResolveMacros(graphInfo.GraphYAxisTitle), String.Empty);
            axisY.TitleForeColor = HTMLHelper.GetSafeColorFromHtml(settings["YAxisTitleColor"], Color.Black);

            // If true load same settings as x axis has
            if (ValidationHelper.GetBoolean(settings["YAxisUseXAxisSettings"], true))
            {
                axisY.TitleFont = xFont ?? defaultFont;

                if (xLabelFont != null)
                {
                    axisY.LabelStyle.Font = xLabelFont;
                }
                axisY.TitleAlignment = axisX.TitleAlignment;
            }
            else
            {
                Font yFont = SetFont(settings["YAxisFont"]);
                axisY.TitleFont = yFont ?? defaultFont;
                Font yLabelFont = SetFont(settings["YAxisLabelFont"]);
                if (yLabelFont != null)
                {
                    axisY.LabelStyle.Font = yLabelFont;
                }

                axisY.TitleAlignment = (StringAlignment)Enum.Parse(typeof(StringAlignment), ValidationHelper.GetString(settings["YAxisTitlePosition"], "Center"));
            }

            // Chart Area
            Color prColor = HTMLHelper.GetSafeColorFromHtml(settings["ChartAreaPrBgColor"], Color.Empty);
            Color scColor = HTMLHelper.GetSafeColorFromHtml(settings["ChartAreaSecBgColor"], Color.Empty);
            chart.BorderlineColor = SetBorderColor("ChartAreaBorderColor", settings);
            chart.BackGradientStyle = SetGradient(ValidationHelper.GetString(settings["ChartAreaGradient"], "None"), ref prColor, ref scColor);
            chart.BackColor = prColor;
            chart.BackSecondaryColor = scColor;

            chart.BorderlineWidth = ValidationHelper.GetInteger(settings["ChartAreaBorderSize"], ReportGraphDefaults.ChartAreaBorderSize);
            chart.BorderlineDashStyle = (ChartDashStyle)Enum.Parse(typeof(ChartDashStyle), ValidationHelper.GetString(settings["ChartAreaBorderStyle"], ReportGraphDefaults.ChartAreaBorderStyle));
            chart.BorderSkin.SkinStyle = (BorderSkinStyle)Enum.Parse(typeof(BorderSkinStyle), ValidationHelper.GetString(settings["BorderSkinStyle"], "None"));

            // Angle of labels rotation in both axes
            axisX.LabelStyle.Angle = ValidationHelper.GetInteger(settings["XAxisAngle"], 0);
            axisY.LabelStyle.Angle = ValidationHelper.GetInteger(settings["YAxisAngle"], 0);

            // Y axis settings
            SetAxisScale(axisY, settings);
            axisY.IsReversed = ValidationHelper.GetBoolean(settings["ReverseYAxis"], false);

            // Plot area
            prColor = HTMLHelper.GetSafeColorFromHtml(settings["PlotAreaPrBgColor"], Color.Empty);
            scColor = HTMLHelper.GetSafeColorFromHtml(settings["PlotAreaSecBgColor"], Color.Empty);
            mainArea.BackColor = prColor == Color.Empty ? Color.Transparent : prColor;

            mainArea.BorderColor = SetBorderColor("PlotAreaBorderColor", settings);
            mainArea.BackGradientStyle = SetGradient(ValidationHelper.GetString(settings["PlotAreaGradient"], "None"), ref prColor, ref scColor);
            mainArea.BackSecondaryColor = scColor;
            mainArea.BorderWidth = ValidationHelper.GetInteger(settings["PlotAreaBorderSize"], ReportGraphDefaults.PlotAreaBorderSize);
            mainArea.BorderDashStyle = (ChartDashStyle)Enum.Parse(typeof(ChartDashStyle), ValidationHelper.GetString(settings["PlotAreaBorderStyle"], ReportGraphDefaults.PlotAreaBorderStyle));

            mShowValueAsPercent = ValidationHelper.GetBoolean(settings["ValuesAsPercent"], false);

            if (returnByteData)
            {
                using (var stream = new SystemIO.MemoryStream())
                {
                    chart.SaveImage(stream, ChartImageFormat.Png);

                    return stream.ToArray();
                }
            }
            return null;
        }


        /// <summary>
        /// If given data set is null return this graph.
        /// </summary>
        /// <param name="graphInfo">Graph info</param>
        /// <param name="titleStr">Text to show</param>
        /// <param name="returnByteData">Return byte array representing chart if true</param>
        public byte[] CreateInvalidDataGraph(ReportGraphInfo graphInfo, string titleStr, bool returnByteData)
        {
            Chart chart = ChartControl;

            SetChartDimensions(chart, graphInfo);

            // Title
            Title title = chart.Titles.Add("title");
            title.Text = ResHelper.GetString(titleStr);
            title.Font = new Font("Arial", 16, FontStyle.Bold);
            title.ForeColor = Color.Red;

            // BackGround color and skin
            if (graphInfo != null)
            {
                ReportCustomData settings = graphInfo.GraphSettings;
                Color prColor = HTMLHelper.GetSafeColorFromHtml(settings["ChartAreaPrBgColor"], Color.Empty);
                Color scColor = HTMLHelper.GetSafeColorFromHtml(settings["ChartAreaSecBgColor"], Color.Empty);
                chart.BackGradientStyle = SetGradient(ValidationHelper.GetString(settings["ChartAreaGradient"], ReportGraphDefaults.ChartAreaGradient), ref prColor, ref scColor);
                chart.BackColor = prColor;
                chart.BackSecondaryColor = scColor;
                chart.BorderSkin.SkinStyle = ValidationHelper.GetString(settings["BorderSkinStyle"], ReportGraphDefaults.BorderSkinStyle).ToEnum<BorderSkinStyle>();
            }

            if (returnByteData)
            {
                using (var stream = new SystemIO.MemoryStream())
                {
                    chart.SaveImage(stream, ChartImageFormat.Png);

                    return stream.ToArray();
                }
            }
            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Converts Alignment for axis titles to MS Chart enum.
        /// </summary>
        /// <param name="value">Settings enum</param>
        private static ContentAlignment ConvertAlignment(string value)
        {
            switch (value)
            {
                case "Center":
                    return ContentAlignment.MiddleCenter;

                case "Left":
                    return ContentAlignment.MiddleLeft;

                case "Right":
                    return ContentAlignment.MiddleRight;

                default:
                    return ContentAlignment.MiddleCenter;
            }
        }


        /// <summary>
        /// Converts font style to ms chart enum.
        /// </summary>
        /// <param name="data">Settings font string (name;type;size;underline;strikethrought)</param>
        private static Font SetFont(string data)
        {
            if (!String.IsNullOrEmpty(data))
            {
                string[] parts = data.Split(new[] { ';' });
                if (parts.Length == 5)
                {
                    FontStyle fs = FontStyle.Regular;
                    // Font type 
                    switch (parts[1].ToLowerCSafe())
                    {
                        case "bold":
                            fs = FontStyle.Bold;
                            break;

                        case "italic":
                            fs = FontStyle.Italic;
                            break;

                        case "bolditalic":
                            fs = FontStyle.Bold | FontStyle.Italic;
                            break;

                        case "regular":
                            fs = FontStyle.Regular;
                            break;
                    }

                    // Underline
                    if (parts[3].EqualsCSafe("underline", true))
                    {
                        fs |= FontStyle.Underline;
                    }

                    // Strike through
                    if (parts[4].EqualsCSafe("strikethrought", true))
                    {
                        fs |= FontStyle.Strikeout;
                    }

                    return new Font(parts[0], ValidationHelper.GetInteger(parts[2], 10), fs);
                }
            }
            return null;
        }


        /// <summary>
        /// Convert old legend format to new one.
        /// </summary>
        /// <param name="graphInfo">Graph info data</param>
        /// <param name="legend">Legend structure for holding legend position info</param>
        private static void ConvertLegendPosition(ReportGraphInfo graphInfo, Legend legend)
        {
            // 100 is random picked value to find out if there is any graph legend position set
            int value = ValidationHelper.GetInteger(graphInfo.GraphLegendPosition, ReportGraphDefaults.GraphLegendPosition);
            if (value == ReportGraphDefaults.GraphLegendPosition)
            {
                return;
            }

            switch (value)
            {
                case 0:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Top;
                    break;

                case 1:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Top;
                    break;

                case 2:
                    legend.Alignment = StringAlignment.Far;
                    legend.Docking = Docking.Top;
                    break;

                case 3:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Bottom;
                    break;

                case 4:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Top;
                    legend.IsDockedInsideChartArea = true;
                    break;

                case 5:
                    legend.Alignment = StringAlignment.Far;
                    legend.Docking = Docking.Top;
                    legend.IsDockedInsideChartArea = true;
                    break;

                case 6:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Bottom;
                    legend.IsDockedInsideChartArea = true;
                    break;

                case 7:
                    legend.Alignment = StringAlignment.Far;
                    legend.Docking = Docking.Bottom;
                    legend.IsDockedInsideChartArea = true;
                    break;

                case 8:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Top;
                    break;

                case 9:
                    legend.Alignment = StringAlignment.Center;
                    legend.Docking = Docking.Top;
                    break;

                case 10:
                    legend.Alignment = StringAlignment.Center;
                    legend.Docking = Docking.Bottom;
                    break;

                case 11:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Top;
                    break;

                case 12:
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Bottom;
                    break;

                case -1:
                    legend.Enabled = false;
                    break;

                default:
                    legend.Alignment = StringAlignment.Far;
                    legend.Docking = Docking.Bottom;
                    break;
            }
        }


        /// <summary>
        /// Sets legend position.
        /// </summary>
        /// <param name="legend">Legend position info</param>
        /// <param name="position">Settings enum for legend position</param>
        private static void SetLegendPosition(Legend legend, string position)
        {
            if (String.IsNullOrEmpty(position))
            {
                return;
            }

            switch (position.ToLowerCSafe())
            {
                case "none":
                    break;

                case "top":
                    legend.Alignment = StringAlignment.Center;
                    legend.Docking = Docking.Top;
                    break;

                case "left":
                    legend.Alignment = StringAlignment.Center;
                    legend.Docking = Docking.Left;
                    break;

                case "right":
                    legend.Alignment = StringAlignment.Center;
                    legend.Docking = Docking.Right;
                    break;

                case "bottom":
                    legend.Alignment = StringAlignment.Center;
                    legend.Docking = Docking.Bottom;
                    break;

                case "topleft":
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Top;
                    break;

                case "topright":
                    legend.Alignment = StringAlignment.Far;
                    legend.Docking = Docking.Top;
                    break;

                case "bottomleft":
                    legend.Alignment = StringAlignment.Near;
                    legend.Docking = Docking.Bottom;
                    break;

                case "bottomright":
                    legend.Alignment = StringAlignment.Far;
                    legend.Docking = Docking.Bottom;
                    break;
            }
        }


        /// <summary>
        /// Swap colors.
        /// </summary>
        /// <param name="clr1">First color</param>
        /// <param name="clr2">Second color</param>
        private static void SwapColors(ref Color clr1, ref Color clr2)
        {
            Color tmp = clr2;
            clr2 = clr1;
            clr1 = tmp;
        }


        /// <summary>
        /// For historical reasons border color is black if not set. Deny border display by set border style to not set.
        /// </summary>
        /// <param name="key">Key name in settings collection</param>
        /// <param name="settings">Settings collection</param>
        private static Color SetBorderColor(string key, ReportCustomData settings)
        {
            // If border color is null (old version) create black color
            if ((settings == null) || (settings[key] == null))
            {
                return Color.Black;
            }
            return HTMLHelper.GetSafeColorFromHtml(settings[key], Color.Empty);
        }


        /// <summary>
        /// Convert gradient to ms chart enum.
        /// </summary>
        /// <param name="gradient">Settings gradient string</param>
        /// <param name="prColor">Primary color (chart, serie, plot)</param>
        /// <param name="secColor">Secondary color (chart, serie, plot)</param>        
        private static GradientStyle SetGradient(string gradient, ref Color prColor, ref Color secColor)
        {
            if (prColor == null || secColor == null)
            {
                return GradientStyle.None;
            }

            // Select gradient
            switch (gradient.ToLowerCSafe())
            {
                case "leftright":
                    return GradientStyle.LeftRight;

                case "diagonalleft":
                    return GradientStyle.DiagonalLeft;

                case "topbottom":
                    return GradientStyle.TopBottom;

                case "diagonalright":
                    return GradientStyle.DiagonalRight;

                case "rightleft":
                    SwapColors(ref prColor, ref secColor);
                    return GradientStyle.LeftRight;

                case "leftdiagonal":
                    SwapColors(ref prColor, ref secColor);
                    return GradientStyle.DiagonalLeft;

                case "bottomtop":
                    SwapColors(ref prColor, ref secColor);
                    return GradientStyle.TopBottom;

                case "rightdiagonal":
                    SwapColors(ref prColor, ref secColor);
                    return GradientStyle.DiagonalRight;

                default:
                    return GradientStyle.None;
            }
        }


        /// <summary>
        /// Sets grid color,style and width for axis.
        /// </summary>
        private static void SetDefaultGrid(Axis axis, bool enable)
        {
            if (enable)
            {
                axis.MajorGrid.LineColor = ColorTranslator.FromHtml(ReportGraphDefaults.MajorGridLineColor);
                axis.MajorGrid.LineDashStyle = ReportGraphDefaults.MajorGridLineDashStyle;
                axis.LineWidth = 1;
            }

            axis.MajorGrid.Enabled = enable;
        }


        /// <summary>
        /// Sets scale and logarithm usage for set axis.
        /// </summary>
        /// <param name="axis">Char's axis</param>
        /// <param name="settings">Settings from GUI</param>
        private static void SetAxisScale(Axis axis, ReportCustomData settings)
        {
            // Use ten powers (logarithmic scale of y axis)
            axis.IsLogarithmic = ValidationHelper.GetBoolean(settings["TenPowers"], false);
            var scaleMin = settings["ScaleMin"];
            if (!String.IsNullOrEmpty(scaleMin))
            {
                axis.Minimum = ValidationHelper.GetInteger(scaleMin, 0);
            }
            var scaleMax = settings["ScaleMax"];
            if (!String.IsNullOrEmpty(scaleMax))
            {
                axis.Maximum = ValidationHelper.GetInteger(scaleMax, 0);
            }
        }


        /// <summary>
        /// Sets chart type and its settings for each serie.
        /// </summary>
        /// <param name="serie">Chart's series</param>
        /// <param name="mainArea">Main chart's area</param>
        /// <param name="graphInfo">Chart's data</param>
        /// <param name="columnMacros">Column name macro</param>
        private void SetSerie(Series serie, ChartArea mainArea, ReportGraphInfo graphInfo, IEnumerable<string> columnMacros)
        {
            ReportCustomData settings = graphInfo.GraphSettings;
            string orientation = ValidationHelper.GetString(settings["BarOrientation"], String.Empty);
            string chartType = graphInfo.GraphType.ToLowerCSafe();
            string style;

            switch (chartType)
            {
                case "pie":
                {
                    // Drawing style - doughnut, area
                    style = ValidationHelper.GetString(settings["PieDrawingStyle"], ReportGraphDefaults.PieDrawingStyle);

                    serie.ChartType = style.EqualsCSafe("doughnut", true) ? SeriesChartType.Doughnut : SeriesChartType.Pie;

                    // Drawing style - softedge, concave, none
                    serie["PieDrawingStyle"] = ValidationHelper.GetString(settings["PieDrawingDesign"], ReportGraphDefaults.PieDrawingDesign);

                    // Label style - inside, outside, none
                    serie["PieLabelStyle"] = ValidationHelper.GetString(settings["PieLabelStyle"], ReportGraphDefaults.PieLabelStyle);

                    // This value may not be null
                    serie["DoughnutRadius"] = ValidationHelper.GetString(settings["PieDoughnutRadius"], ReportGraphDefaults.PieDoughnutRadius);

                    // Collect pieces
                    string otherValue = ValidationHelper.GetString(settings["PieOtherValue"], String.Empty);
                    if (otherValue != String.Empty)
                    {
                        // Set the threshold under which all points will be collected
                        serie["CollectedThreshold"] = otherValue;

                        // Set label and legend text of the collected pie slice
                        serie["CollectedLabel"] = serie["CollectedLegendText"] = ResHelper.GetString("graph.pie.others");
                    }
                }
                    break;

                case "baroverlay":
                case "bar":
                {
                    // Overlay - series show in a line one behind other
                    mainArea.Area3DStyle.IsClustered = !ValidationHelper.GetBoolean(settings["BarOverlay"], false);

                    // Drawing style - cylinder,bar
                    style = ValidationHelper.GetString(settings["BarDrawingStyle"], ReportGraphDefaults.BarDrawingStyle);

                    serie["DrawingStyle"] = style.EqualsCSafe("cylinder", true) ? "cylinder" : "default";

                    // bar orientation - horizontal,vertical
                    serie.ChartType = orientation.EqualsCSafe("horizontal", true) ? SeriesChartType.Bar : SeriesChartType.Column;

                    // Conversion clustered
                    if (chartType.EqualsCSafe("baroverlay", true))
                    {
                        mainArea.Area3DStyle.IsClustered = false;
                        mainArea.Area3DStyle.Enable3D = true;
                    }
                }
                    break;

                case "barpercentage":
                {
                    // fillcurves old Convert
                    bool fillCurves = ValidationHelper.GetBoolean(settings["FillCurves"], false);
                    serie.ChartType = fillCurves ? SeriesChartType.Area : SeriesChartType.StackedBar100;

                    if (orientation.EqualsCSafe("vertical", true))
                    {
                        serie.ChartType = SeriesChartType.StackedColumn100;
                    }
                }
                    break;

                case "barstacked":
                {
                    serie.ChartType = SeriesChartType.StackedBar;

                    bool fillCurves = ValidationHelper.GetBoolean(settings["FillCurves"], false);
                    style = ValidationHelper.GetString(settings["StackedBarDrawingStyle"], ReportGraphDefaults.StackedBarDrawingStyle);
                    bool maxStacked = ValidationHelper.GetBoolean(settings["StackedBarMaxStacked"], false);

                    // Drawing style - bar, area
                    if (style.EqualsCSafe("area", true) || (fillCurves))
                    {
                        serie.ChartType = maxStacked ? SeriesChartType.StackedArea100 : SeriesChartType.StackedArea;
                    }
                    else
                    {
                        if (style.EqualsCSafe("cylinder", true))
                        {
                            serie["DrawingStyle"] = "cylinder";
                        }

                        // Vertical oriented stacked bar - > stacked column
                        if (orientation.EqualsCSafe("vertical", true))
                        {
                            serie.ChartType = maxStacked ? SeriesChartType.StackedColumn100 : SeriesChartType.StackedColumn;
                        }
                        else
                        {
                            serie.ChartType = maxStacked ? SeriesChartType.StackedBar100 : SeriesChartType.StackedBar;
                        }
                    }
                }
                    break;

                case "line":
                {
                    // Line drawing style - line,spline
                    style = ValidationHelper.GetString(settings["LineDrawinStyle"], "Line");
                    serie.ChartType = style.EqualsCSafe("line", true) ? SeriesChartType.Line : SeriesChartType.Spline;

                    // Conversion from old smoothcurves
                    if (ValidationHelper.GetBoolean(settings["SmoothCurves"], false))
                    {
                        serie.ChartType = SeriesChartType.Spline;
                    }

                    // Marker style
                    MarkerStyle ms = (MarkerStyle)Enum.Parse(typeof (MarkerStyle), ValidationHelper.GetString(settings["SeriesSymbols"], ReportGraphDefaults.SeriesSymbols));
                    if (ms != MarkerStyle.None)
                    {
                        serie["ShowMarkerLines"] = "true";
                        serie.MarkerStyle = (MarkerStyle)Enum.Parse(typeof (MarkerStyle), ValidationHelper.GetString(settings["SeriesSymbols"], ReportGraphDefaults.SeriesSymbols));
                        serie.MarkerSize = 6;
                        serie.MarkerColor = Color.White;
                    }
                }
                    break;

                default:
                    serie.ChartType = SeriesChartType.Column;
                    break;
            }

            // Default Series Settings
            Color prColor = HTMLHelper.GetSafeColorFromHtml(settings["SeriesPrBgColor"], Color.Empty);
            Color scColor = HTMLHelper.GetSafeColorFromHtml(settings["SeriesSecBgColor"], Color.Empty);

            // series settings - series color,gradient and border color, size and style.
            serie.BorderColor = SetBorderColor("SeriesBorderColor", settings);
            serie.BackGradientStyle = SetGradient(ValidationHelper.GetString(settings["SeriesGradient"], "None"), ref prColor, ref scColor);
            serie.Color = prColor;
            serie.BackSecondaryColor = scColor;

            bool isLineChart = (chartType.EqualsCSafe("line", true));
            serie.BorderWidth = ValidationHelper.GetInteger(settings["SeriesBorderSize"], (isLineChart ? ReportGraphDefaults.SeriesLineBorderSize : ReportGraphDefaults.SeriesBorderSize));
            serie.BorderDashStyle = (ChartDashStyle)Enum.Parse(typeof(ChartDashStyle), ValidationHelper.GetString(settings["SeriesBorderStyle"], (isLineChart ? ReportGraphDefaults.SeriesLineBorderStyle : ReportGraphDefaults.SeriesBorderStyle)));

            // Empty point data
            serie["EmptyPointValue"] = "Zero";

            var emptyPointStyle = serie.EmptyPointStyle;
            emptyPointStyle.BorderWidth = serie.BorderWidth;
            emptyPointStyle.BorderDashStyle = serie.BorderDashStyle;
            emptyPointStyle.IsValueShownAsLabel = false;

            // Set same markers as for default line
            if (serie.MarkerStyle != MarkerStyle.None)
            {
                emptyPointStyle.MarkerSize = serie.MarkerSize;
                emptyPointStyle.MarkerStyle = serie.MarkerStyle;
                emptyPointStyle.MarkerColor = serie.MarkerColor;
            }

            // Label format
            serie.Label = ValidationHelper.GetString(settings["ItemValueFormat"], String.Empty);

            // DisplayItemValue
            var displayItemValue = settings["DisplayItemValue"];
            if (displayItemValue != null)
            {
                serie.IsValueShownAsLabel = ValidationHelper.GetBoolean(displayItemValue, false);
            }
            else
            {
                // Conversion - if old graph was bar type set this value to true otherwise set it to false
                switch (chartType)
                {
                    case "bar":
                    case "barstacked":
                    case "barpercentage":
                    case "baroverlay":
                        serie.IsValueShownAsLabel = true;
                        break;

                    default:
                        serie.IsValueShownAsLabel = false;
                        break;
                }
            }

            // Check whether macro is defined in column name
            if (columnMacros != null)
            {
                foreach (string macro in columnMacros)
                {
                    // No symbols
                    if (macro.EqualsCSafe("NS", true))
                    {
                        serie.MarkerStyle = MarkerStyle.None;
                    }
                    // Line color
                    else if (macro.StartsWithCSafe("LC", true))
                    {
                        string hexColor = "#" + macro.Substring(2);
                        serie.Color = HTMLHelper.GetSafeColorFromHtml(hexColor, serie.Color);
                    }
                }
            }
        }


        /// <summary>
        /// Sets width and height to the given <paramref name="chart"/> based on <paramref name="graphInfo"/> or default settings.
        /// </summary>
        /// <param name="chart">Chart object</param>
        /// <param name="graphInfo">Report graph object</param>
        private void SetChartDimensions(Chart chart, ReportGraphInfo graphInfo)
        {
            if (graphInfo != null)
            {
                // Basic settings
                chart.Width = CorrectWidth != 0 ? CorrectWidth : graphInfo.GraphWidth;
                chart.Height = graphInfo.GraphHeight;
            }

            // If width and height aren't specified use default values
            if (chart.Width == 0)
            {
                chart.Width = DEFAULT_WIDTH;
            }
            if (chart.Height == 0)
            {
                chart.Height = DEFAULT_HEIGHT;
            }
        }


        /// <summary>
        /// Event for label macro customization.
        /// </summary>
        private void mChart_Customize(object sender, EventArgs e)
        {
            // Resolve macros stored in DataTable (label - column)
            Chart chart = ChartControl;
            if ((chart == null) || (mResolver == null))
            {
                return;
            }

            // Check dataset
            if ((mGraphDataSet != null) && (mGraphDataSet.Tables.Count > 0))
            {
                // Pie charts need special resolve routine
                foreach (Series serie in chart.Series)
                {
                    if (serie.MarkerStyle != MarkerStyle.None)
                    {
                        // Make the marker the same color as the lines
                        serie.MarkerBorderColor = serie.Color;
                        serie.EmptyPointStyle.MarkerBorderColor = serie.Color;
                    }

                    serie.EmptyPointStyle.Color = serie.Color;

                    if ((serie.ChartType == SeriesChartType.Pie) || (serie.ChartType == SeriesChartType.Doughnut))
                    {
                        bool isLegendVisible = ((mChart.Legends.Count > 0) && (mChart.Legends[0].Enabled));
                        double aggregatingThreshold = ValidationHelper.GetDouble(serie["CollectedThreshold"], 0d);

                        // 'Others' slice is used when dataset rows count is different from number of points in series
                        bool aggregatedToOthers = ((aggregatingThreshold > 0) && (chart.Series[0].Points.Count != mGraphDataSet.Tables[0].Rows.Count));
                        var dataPoints = chart.Series[0].Points.ToList();
                        var dataSet = mGraphDataSet.Tables[0].AsEnumerable();

                        if (aggregatedToOthers)
                        {
                            // Skip the last data point from the collection (last data point is the aggregated one, therefore the label is set up in another way).
                            dataPoints = dataPoints.Take(dataPoints.Count() - 1).ToList();
                            var minDisplayedValue = dataPoints.Min(c => c.YValues[0]);

                            // Get all data rows which are not aggregated to the 'Others' slice. 
                            dataSet = dataSet.Where(row => row.Field<int>(1) >= minDisplayedValue);

                            // Set data point properties for the aggregated 'Others' piece
                            var others = serie.Points.Last();
                            others.Label = serie.Label;
                            others.ToolTip = others.ToolTip.Replace("#VALX", ResHelper.GetString("graph.pie.others"));
                        }

                        for (int i = 0; i < dataPoints.Count; i++)
                        {
                            if (isLegendVisible)
                            {
                                // Set legend text according to the values in data source
                                var dataPoint = dataPoints[i];
                                dataPoint.LegendText = mResolver.ResolveMacros(dataSet.ElementAt(i).Field<string>(0));

                                if (dataPoint.LegendText == String.Empty)
                                {
                                    dataPoint.LegendText = " ";
                                }
                            }
                        }
                    }
                    else
                    {
                        // Resolve actual values 
                        foreach (DataPoint dp in serie.Points)
                        {
                            mResolver.SetNamedSourceData("yval", dp.YValues[0]);
                            mResolver.SetNamedSourceData("xval", dp.XValue);
                            dp.ToolTip = mResolver.ResolveMacros(dp.ToolTip);

                            if (serie.IsValueShownAsLabel)
                            {
                                dp.Label = mResolver.ResolveMacros(dp.Label);
                            }
                        }
                    }
                }
            }

            // Show percentage of all points in one row (count through all series)
            if (mShowValueAsPercent)
            {
                if (chart.Series.Count > 0)
                {
                    for (int k = 0; k < chart.Series[0].Points.Count; k++)
                    {
                        // Count sum through all series
                        double sum = 0;
                        for (int i = 0; i < chart.Series.Count; i++)
                        {
                            DataPoint dp = chart.Series[i].Points[k];
                            sum += dp.YValues[0];
                        }

                        // Fix every point in serie
                        for (int i = 0; i < chart.Series.Count; i++)
                        {
                            DataPoint dp = chart.Series[i].Points[k];
                            dp.Label = dp.Label.Replace("#POINTVALUE", dp.YValues[0].ToString());
                            if (sum > 0)
                            {
                                dp.YValues[0] = (dp.YValues[0] / sum) * 100;
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}