using System;
using System.Collections;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class providing rule helper methods.
    /// </summary>
    public static class RuleHelper
    {
        #region "Methods"

        /// <summary>
        /// Returns attribute condition in XML format based on specific data.
        /// </summary>
        /// <param name="row">Data row container with rule information</param>
        /// <param name="whereCondition">Where condition</param>
        public static string GetAttributeCondition(IDataContainer row, string whereCondition)
        {
            if (DataHelper.DataSourceIsEmpty(row))
            {
                return null;
            }

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.LeadScoring);
            }

            var attInfo = new RuleAttributeItem();

            bool readyToAddParameters = false;
            foreach (string column in row.ColumnNames)
            {
                if (readyToAddParameters)
                {
                    // Set condition parameters
                    attInfo.Parameters.Add(column, GetValue(row, column));
                }
                else
                {
                    // If data container has table property in the first column - skip it
                    if (!column.EqualsCSafe("table", true))
                    {
                        // Set attribute name and value
                        attInfo.Name = column;
                        attInfo.Value = GetValue(row, column).ToString();
                        readyToAddParameters = true;
                    }
                }
            }

            var condition = new RuleCondition(null)
            {
                WhereCondition = whereCondition
            };
            condition.ItemsList.Add(attInfo);


            return condition.GetXmlDefinition();
        }


        /// <summary>
        /// Returns activity condition in XML format based on specific data.
        /// </summary>
        /// <param name="row">Data row container with rule information</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="activityType">Activity type name in case the rule type is activity</param>
        public static string GetActivityCondition(IDataContainer row, string whereCondition, string activityType)
        {
            if (DataHelper.DataSourceIsEmpty(row) || (string.IsNullOrEmpty(activityType)))
            {
                return null;
            }

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.LeadScoring);
            }

            var actInfo = new RuleActivityItem
            {
                // Set activity type name
                ActivityName = activityType
            };

            foreach (string column in row.ColumnNames)
            {
                // Skip table property in the first column
                if (column.EqualsCSafe("table", true))
                {
                    continue;
                }

                // Create field definition
                RuleActivityItem.RuleActivityField field = null;
                if (!column.Contains("."))
                {
                    // Initialize field with its name and value
                    field = new RuleActivityItem.RuleActivityField(column, GetValue(row, column).ToString());

                    // Add field to activity
                    actInfo.Fields.Add(field);
                }
                    // Ensure field definition and add field additional parameters
                else
                {
                    // Get parameter of specific field - parameter name is composed of field name and parameter name
                    string[] paramName = column.Split(new[] { '.' });
                    if (paramName.Length == 2)
                    {
                        foreach (var item in actInfo.Fields)
                        {
                            // Try to find field definition of the processed parameter
                            if (item.Name.EqualsCSafe(paramName[0], true))
                            {
                                field = item;
                                break;
                            }
                        }

                        if (field == null)
                        {
                            // Create new field definition
                            field = new RuleActivityItem.RuleActivityField(paramName[0], null);

                            // Add field to activity
                            actInfo.Fields.Add(field);
                        }

                        // Add field parameter
                        field.Parameters.Add(paramName[1], GetValue(row, column));
                    }
                }
            }

            var condition = new RuleCondition(null)
            {
                WhereCondition = whereCondition
            };
            condition.ItemsList.Add(actInfo);

            return condition.GetXmlDefinition();
        }


        /// <summary>
        /// Returns macro rule condition in XML format based on specific data.
        /// </summary>
        /// <param name="macroValue">Macro rule value</param>
        public static string GetMacroCondition(string macroValue)
        {
            if (string.IsNullOrEmpty(macroValue))
            {
                return null;
            }

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.LeadScoring);
            }

            var macroInfo = new RuleMacroItem(macroValue);

            var condition = new RuleCondition(null);
            condition.ItemsList.Add(macroInfo);

            // Return XML definition
            return condition.GetXmlDefinition();
        }


        /// <summary>
        /// Returns data row container initialized with data from rule condition if the table definition contains column named as rule item name.
        /// If there is no column named as rule item name then empty data row container is returned.
        /// </summary>
        /// <param name="rule">RuleInfo object</param>
        /// <param name="table">Table definition</param>
        /// <param name="activityTypeName">Activity type name is returned via this parameter if it is activity rule</param>
        public static IDataContainer GetDataFromCondition(RuleInfo rule, DataTable table, ref string activityTypeName)
        {
            if (table == null)
            {
                return null;
            }

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.LeadScoring);
            }

            var row = new DataRowContainer(table.NewRow());

            if (rule == null)
            {
                return row;
            }

            // Create new condition object
            var condition = new RuleCondition(rule.RuleCondition);
            foreach (RuleItem item in condition.ItemsList)
            {
                if (item is RuleAttributeItem)
                {
                    RuleAttributeItem attItem = (RuleAttributeItem)item;
                    // Check attribute name column
                    if (table.Columns.Contains(attItem.Name))
                    {
                        // Ensure attribute parameters' columns
                        foreach (DictionaryEntry param in attItem.Parameters)
                        {
                            if (!table.Columns.Contains(param.Key.ToString()))
                            {
                                table.Columns.Add(param.Key.ToString(), typeof (string));
                            }
                        }

                        // Create new data row container
                        row = new DataRowContainer(table.NewRow());

                        // Set attribute value
                        SetValue(row, attItem.Name, attItem.Value);

                        // Set attribute parameters' values
                        foreach (DictionaryEntry param in attItem.Parameters)
                        {
                            row.SetValue(param.Key.ToString(), param.Value);
                        }
                    }
                }
                else if (item is RuleActivityItem)
                {
                    RuleActivityItem actItem = (RuleActivityItem)item;
                    // Set activity type name
                    activityTypeName = actItem.ActivityName;

                    foreach (RuleActivityItem.RuleActivityField field in actItem.Fields)
                    {
                        // Ensure field parameters' columns
                        foreach (DictionaryEntry param in field.Parameters)
                        {
                            // Full parameter name is composed of field name and original parameter name
                            string paramName = field.Name + "." + param.Key;
                            if (!table.Columns.Contains(paramName))
                            {
                                table.Columns.Add(paramName, typeof (string));
                            }
                        }
                    }

                    // Create new data row container
                    row = new DataRowContainer(table.NewRow());

                    foreach (RuleActivityItem.RuleActivityField field in actItem.Fields)
                    {
                        // Check field name column
                        if (table.Columns.Contains(field.Name))
                        {
                            // Set field value
                            SetValue(row, field.Name, field.Value);

                            // Set field parameters' values
                            foreach (DictionaryEntry param in field.Parameters)
                            {
                                // Full parameter name is composed of field name and original parameter name
                                string paramName = field.Name + "." + param.Key;
                                row.SetValue(paramName, param.Value);
                            }
                        }
                    }
                }
            }

            return row;
        }


        /// <summary>
        /// Returns macro condition saved within the given Rule. If there is no corresponding condition in the rule, returns empty string.
        /// </summary>
        /// <param name="rule">Rule containing the macro</param>
        /// <returns>Macro condition of given rule, if available; empty string otherwise</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="rule"/> is null</exception>
        public static string GetMacroConditionFromRule(RuleInfo rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            // Create new condition object
            var condition = new RuleCondition(rule.RuleCondition);

            // Find macro condition
            var macroInfo = condition.ItemsList.OfType<RuleMacroItem>().SingleOrDefault();
            return macroInfo != null ? macroInfo.MacroValue : string.Empty;
        }

        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Returns value in DB culture (en-us).
        /// </summary>
        /// <param name="row">Data source</param>
        /// <param name="columnName">Column name</param>
        private static object GetValue(IDataContainer row, string columnName)
        {
            object value = row.GetValue(columnName);

            return Convert.ToString(value, CultureHelper.EnglishCulture);
        }


        /// <summary>
        /// Sets value (in en-us format) into data container in current culture format.
        /// </summary>
        /// <param name="row">Data container</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        private static void SetValue(DataRowContainer row, string columnName, object value)
        {
            // Convert value into current format
            if (value != null)
            {
                DataColumn column = row.DataRow.Table.Columns[columnName];
                if (column != null)
                {
                    if (column.DataType == typeof(decimal))
                    {
                        value = ValidationHelper.GetDouble(value, 0, "en-us");
                    }

                    if (column.DataType == typeof(DateTime))
                    {
                        value = ValidationHelper.GetDateTime(value, DateTime.MinValue, "en-us");
                    }

                    DataHelper.SetDataRowValue(row.DataRow, column.ColumnName, value);
                }
            }
        }

        #endregion
    }
}