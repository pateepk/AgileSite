using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Xml;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing helper methods for the Reporting module.
    /// </summary>
    public class ReportHelper
    {
        #region "Constants"

        private const string ROOT_NODE = "Root";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns data row with report parameters from the given parameters string.
        /// </summary>
        /// <param name="report">Report data</param>
        /// <param name="parametersString">String of report parameters from which the data row should be created. 
        /// It is in format [param1 name];[param1 value];[param2 name];[param2 value];....[paramN name];[paramN value].</param>
        /// <param name="replacement">Semicolon replacement in the parameter value</param>
        /// <param name="stringCulture">Culture of the parameters in the input string</param>
        public static DataRow GetReportParameters(ReportInfo report, string parametersString, string replacement, CultureInfo stringCulture)
        {
            // Do not process
            if ((report == null) || (string.IsNullOrEmpty(parametersString)))
            {
                return null;
            }

            // Get parameters array
            string[] parameters = parametersString.Split(';');

            // Do not process - at least one parameter is required
            if (parameters.Length <= 1)
            {
                return null;
            }

            // Get report parameters empty row
            FormInfo fi = new FormInfo(report.ReportParameters);
            DataRow dr = fi.GetDataRow();

            // Put values to the row
            for (int i = 0; i < parameters.Length; i = i + 2)
            {
                var name = parameters[i];

                if (dr.Table.Columns.Contains(name) && (parameters[i + 1] != string.Empty))
                {
                    // Get parameter value
                    var value = parameters[i + 1];
                    if (replacement != null)
                    {
                        value = value.Replace(replacement, ";");
                    }

                    Type type = dr.Table.Columns[name].DataType;

                    // Set parameter value to the row
                    var dataType = DataTypeManager.GetDataType(type);
                    dr[name] = dataType.Convert(value, stringCulture, dataType.ObjectDefaultValue);
                }
            }

            dr.AcceptChanges();

            return dr;
        }


        /// <summary>
        /// Returns report's default connection string
        /// </summary>
        public static String GetDefaultReportConnectionString()
        {
            String def = SettingsKeyInfoProvider.GetValue("CMSDefaultReportConnectionString");

            // For empty settings, return default connection string
            if (String.IsNullOrEmpty(def))
            {
                def = ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME;
            }

            return def;
        }


        /// <summary>
        /// Converts report's parameters datarow to XML string representation.
        /// </summary>
        /// <param name="dr">Data row to convert</param>
        public static String WriteParametersToXml(DataRow dr)
        {
            if (dr == null)
            {
                return String.Empty;
            }

            // Create xml object
            XmlDocument xmlDoc = new XmlDocument();

            // Create child object named 'root'
            XmlElement root = xmlDoc.CreateElement(ROOT_NODE);
            xmlDoc.AppendChild(root);

            var parameters = new Dictionary<string, object>();

            // Loop through all object and create node for every column
            foreach (DataColumn col in dr.Table.Columns)
            {
                parameters.Add(col.ColumnName, DataHelper.GetNull(DataHelper.GetDataRowValue(dr, col.ColumnName)));
            }

            root.AddChildElements(parameters);

            return xmlDoc.ToFormattedXmlString(true);
        }


        /// <summary>
        /// Applies subscription parameters to default report datarow.
        /// </summary>
        /// <param name="rsi">Report subscription object</param>
        /// <param name="dr">Report's default parameters</param>
        /// <param name="resolveMacros">Indicates whether macros should be resolved</param>
        public static void ApplySubscriptionParameters(ReportSubscriptionInfo rsi, DataRow dr, bool resolveMacros)
        {
            // Loop through all nodes in subscription's XML parameters and replace report's value with current subscription's ones.
            if (!String.IsNullOrEmpty(rsi?.ReportSubscriptionParameters))
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(rsi.ReportSubscriptionParameters);

                foreach (DataColumn col in dr.Table.Columns)
                {
                    XmlNode node = xml.SelectSingleNode($"/{ROOT_NODE}/{col.ColumnName}");
                    if (node != null)
                    {
                        // In case of different data types use try catch block
                        try
                        {
                            dr[col.ColumnName] = GetNodeValue(resolveMacros, node, col);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }


        private static object GetNodeValue(bool resolveMacros, XmlNode node, DataColumn column)
        {
            // Set NULL to datetime column for empty subscription parameter
            if (column.DataType == typeof(DateTime) && string.IsNullOrEmpty(node.InnerText))
            {
                return DBNull.Value;
            }

            object value = resolveMacros ? MacroContext.CurrentResolver.ResolveMacros(node.InnerText) : node.InnerText;

            if (column.DataType == typeof(DateTime))
            {
                value = DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldDataType.DateTime, value, CultureHelper.EnglishCulture);
            }

            return value;
        }

        #endregion
    }
}
