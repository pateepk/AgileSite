using System;
using System.Runtime.Serialization;

using CMS.Helpers;
using CMS.Base;

namespace CMS.Reporting
{
    /// <summary>
    /// Class to help transformation from old graph custom data to new one.
    /// </summary>
    public class ReportCustomData : ContainerCustomData
    {
        /// <summary>
        /// Overrides indexer to convert data.
        /// </summary>
        /// <param name="key">Key in settings collection</param>
        /// <returns>Value if found null if not</returns>
        public new string this[string key]
        {
            get
            {
                // If value not contains key try get from old keys, or return null
                if (!ContainsColumn(key.ToLowerCSafe ()))
                {
                    return GetOldKeyValue(key);
                }

                // Try to get value from storage
                string value = (string)base[key];
                if (value != null)
                {
                    return value;
                }

                // If not found try to look via old storage keys
                return GetOldKeyValue(key);
            }
            set
            {
                base[key] = value;
            }
        }


        /// <summary>
        /// Search for data stored in old keys
        /// </summary>
        /// <param name="key">Key name</param>
        private String GetOldKeyValue(string key)
        {
            // Test keys which are used also in old data
            switch (key)
            {
                case "ChartAreaGradient":
                    return ConvertGradient("chartGradient");

                case "SeriesGradient":
                    return ConvertGradient("itemGradient");

                case "PlotAreaGradient":
                    return ConvertGradient("graphGradient");

                case "TitleFontNew":
                    return ConvertFont("titleFont");

                case "XAxisFont":
                    return ConvertFont("axisFont");

                case "SeriesSymbols":
                    return ConvertSymbols("symbols");

                case "ChartAreaPrBgColor":
                    return ConvertColors("graphGradient", 0);

                case "ChartAreaSecBgColor":
                    return ConvertColors("graphGradient", 1);

                case "PlotAreaPrBgColor":
                    return ConvertColors("chartGradient", 0);

                case "PlotAreaSecBgColor":
                    return ConvertColors("chartGradient", 1);

                case "SeriesPrBgColor":
                    return ConvertColors("itemGradient", 0);

                case "SeriesSecBgColor":
                    return ConvertSeriesColor("colors");

                case "BarOrientation":
                    return ConvertBarOrientation();
            }

            return null;
        }


        /// <summary>
        /// Convert gradient from angle(set via textbox) to enum (drop down list).
        /// </summary>
        /// <param name="key">Key in old settings collection</param>
        /// <returns>Found value</returns>
        private string ConvertGradient(string key)
        {
            string value = (string)base[key];
            if (value == null)
            {
                return null;
            }

            string[] grad = value.Split(';');
            if (grad.Length != 3)
            {
                return null;
            }

            int val = ValidationHelper.GetInteger(grad[2], 0);
            if (val == 0)
            {
                return "None";
            }
            else if (val <= 45)
            {
                return "LeftRight";
            }
            else if (val <= 90)
            {
                return "DiagonalLeft";
            }
            else if (val <= 135)
            {
                return "TopBottom";
            }
            else if (val <= 180)
            {
                return "DiagonalRight";
            }
            else if (val <= 225)
            {
                return "RightLeft";
            }
            else if (val <= 270)
            {
                return "LeftDiagonal";
            }
            else if (val <= 315)
            {
                return "BottomTop";
            }
            else if (val <= 360)
            {
                return "RightDiagonal";
            }
            return null;
        }


        /// <summary>
        /// Convert font from old graph format (fonttype;size;true;false) to new one.
        /// </summary>
        /// <param name="key">Key in old settings collection</param>
        /// <returns>Found value</returns>
        private string ConvertFont(string key)
        {
            string value = (string)base[key];
            if (value == null)
            {
                return null;
            }

            string[] font = value.Split(';');
            if (font.Length != 4)
            {
                return null;
            }

            //font name
            string ret = font[0] + ";";

            //font type
            if (font[2].ToLowerCSafe() == "true" && font[3].ToLowerCSafe() == "true")
            {
                ret += "bolditalic;";
            }
            else if (font[2].ToLowerCSafe() == "true" && font[3].ToLowerCSafe() == "false")
            {
                ret += "bold;";
            }
            else if (font[2].ToLowerCSafe() == "false" && font[3].ToLowerCSafe() == "true")
            {
                ret += "italic;";
            }
            else
            {
                ret += "regular;";
            }

            //size
            ret += font[1] + ";;";
            return ret;
        }


        /// <summary>
        /// Convert symbols.
        /// </summary>
        /// <param name="key">Key in old settings collection</param>
        /// <returns>Found value</returns>
        private string ConvertSymbols(string key)
        {
            string value = (string)base[key];
            if (value == null)
            {
                return null;
            }

            string[] parts = value.Split(';');
            if (parts.Length > 0)
            {
                value = parts[0];
            }

            switch (value.ToLowerCSafe())
            {
                case "xcross":
                    return "Cross";

                case "plus":
                    return "Star4";

                case "star":
                    return "Star4";

                case "triangledown":
                    return "Triangle";

                case "hdash":
                    return "Square";

                case "vdash":
                    return "Square";

                case "default":
                    return "Square";

                case "none":
                    return "None";

                case "diamond":
                    return "Diamond";

                case "square":
                    return "Square";

                case "triangle":
                    return "Triangle";

                case "circle":
                    return "Circle";

                default:
                    return null;
            }
        }


        /// <summary>
        /// Convert colors (set in string format (primarycolor;secondarycolor;angle) to alone attribute.
        /// </summary>
        /// <param name="key">Key in old settings collection</param>
        /// <param name="index">Index of font string</param>
        /// <returns>Found value</returns>
        private string ConvertColors(string key, int index)
        {
            string value = (string)base[key];
            if (value == null)
            {
                return null;
            }

            string[] clrs = value.Split(';');
            if (clrs.Length == 3 && index < 2)
            {
                return clrs[index];
            }

            return null;
        }


        /// <summary>
        /// Convert first color from series color string (reg;green;blue) to SeriesSecondaryColor.
        /// </summary>
        /// <param name="key">Key in old settings collection</param>
        /// <returns>Found value</returns>
        private string ConvertSeriesColor(string key)
        {
            string value = (string)base[key];
            if (value == null)
            {
                return null;
            }

            string[] parts = value.Split(';');
            if (parts.Length > 0)
            {
                return parts[0];
            }

            return null;
        }


        /// <summary>
        /// Converts orientation of bar or stacked bar.
        /// </summary>
        /// <returns>Found value</returns>
        private string ConvertBarOrientation()
        {
            if (ValidationHelper.GetBoolean(base["VerticalBars"], true))
            {
                return "Vertical";
            }

            return "Horizontal";
        }


        /// <summary>
        /// Constructor - Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="ctxt">Streaming context</param>
        public ReportCustomData(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
        }


        /// <summary>
        /// Constructor - creates empty ReportCustomData object.
        /// Object created by this constructor works only as in-memory storage and does not reflects changes to DataClass and DB. 
        /// </summary>
        public ReportCustomData()
        {
        }


        /// <summary>
        /// Constructor - creates empty CustomData object.
        /// </summary>
        /// <param name="container">Related data container</param>
        /// <param name="columnName">Related column name</param>
        public ReportCustomData(IDataContainer container, string columnName)
            : base(container, columnName)
        {
        }
    }
}