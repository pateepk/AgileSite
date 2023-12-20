using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Class for storing special fields.
    /// </summary>
    public class SpecialFieldsDefinition : List<SpecialField>
    {
        #region "Constants"

        /// <summary>
        /// Used to separate values from text on each line in 'Options' mode.
        /// </summary>
        public const string REPLACED_SEMICOLON = "\x00\x00";

        /// <summary>
        /// Enables users to insert semicolon into 'options' mode values.
        /// </summary>
        public const string SEMICOLON_TO_REPLACE = "\\;";


        /// <summary>
        /// Regular expression for XML with options data.
        ///                                  ( ITEM GROUP           ( VALUE GROUP            ( TEXT GROUP               ( VISIBILITY GROUP
        /// </summary>
        private const string OPTIONS_REGEX = "(?'item'<item value=\"(?'value'[^\"]*)\" text=\"(?'text'[^\"]*)\"( visible=\"(?'visible'[^\"]*)\")* \\/>)+?";


        /// <summary>
        /// Constant string to identify data-hash attribute.
        /// </summary>
        public const string DATA_HASH_ATTRIBUTE = "data-hash";


        /// <summary>
        /// Default resolver name for resolving list item macros.
        /// </summary>
        private const string ITEM_RESOLVER_NAME = "listitem";

        #endregion


        #region "Variables"

        private static SafeDictionary<string, Tuple<int, string>> mMacros;
        private static Regex mOptionsRegex;
        private MacroResolver mMacroResolver;

        #endregion


        #region "Private properties"

        /// <summary>
        /// List of available macros.
        /// </summary>
        private static SafeDictionary<string, Tuple<int, string>> Macros
        {
            get
            {
                if (mMacros == null)
                {
                    mMacros = new SafeDictionary<string, Tuple<int, string>>();

                    // Fill available macros
                    mMacros.Add(SpecialFieldMacro.NONE, new Tuple<int, string>(SpecialFieldValue.NONE, "{0}.empty|general.empty"));
                    mMacros.Add(SpecialFieldMacro.DEFAULT, new Tuple<int, string>(SpecialFieldValue.NONE, "{0}.defaultchoice|general.defaultchoice"));
                    mMacros.Add(SpecialFieldMacro.ALL, new Tuple<int, string>(SpecialFieldValue.ALL, "{0}.selectall|general.selectall"));
                    mMacros.Add(SpecialFieldMacro.GLOBAL, new Tuple<int, string>(SpecialFieldValue.GLOBAL, "{0}.global|general.global"));
                    mMacros.Add(SpecialFieldMacro.GLOBAL_AND_SITE, new Tuple<int, string>(SpecialFieldValue.GLOBAL_AND_SITE, "{0}.globalandsite|general.globalandsite"));
                }

                return mMacros;
            }
        }


        /// <summary>
        /// Regular expression for options parsing
        /// </summary>
        private static Regex OptionsRegex
        {
            get
            {
                return mOptionsRegex ?? (mOptionsRegex = RegexHelper.GetRegex(OPTIONS_REGEX));
            }
        }


        /// <summary>
        /// Macro resolver
        /// </summary>
        protected MacroResolver MacroResolver
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

        #endregion


        #region "Public properties"

        /// <summary>
        /// Resource string prefix to localize names of items.
        /// </summary>
        public string ResourcePrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Form field info
        /// </summary>
        public FormFieldInfo FieldInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if special fields should contain unique IDs
        /// </summary>
        public bool SetUniqueIDs
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the items are sorted before filling a target collection.
        /// </summary>
        public bool SortItems
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether duplicate items should be filtered out when filling a target collection.
        /// </summary>
        public bool AllowDuplicates
        {
            get;
            set;
        }


        /// <summary>
        /// Returns all values.
        /// </summary>
        public IEnumerable<string> Values
        {
            get
            {
                return this.Select(specialField => specialField.Value);
            }
        }


        /// <summary>
        /// Returns all text values.
        /// </summary>
        public IEnumerable<string> Texts
        {
            get
            {
                return this.Select(specialField => specialField.Text);
            }
        }

        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourcePrefix">Resource string prefix for text representation of fields</param>
        /// <param name="fieldInfo">Field info</param>
        /// <param name="resolver">Macro resolver to use</param>
        /// <param name="sortItems">If true, the items are sorted before filling a target collection</param>
        public SpecialFieldsDefinition(string resourcePrefix = null, FormFieldInfo fieldInfo = null, MacroResolver resolver = null, bool sortItems = false)
        {
            ResourcePrefix = resourcePrefix;
            FieldInfo = fieldInfo;
            MacroResolver = resolver;
            SortItems = sortItems;
        }


        #region "Public methods"

        /// <summary>
        /// Loads special fields from source text.
        /// </summary>
        /// <param name="text">Source text with fields definition</param>
        public SpecialFieldsDefinition LoadFromText(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                // Load data from XML
                XmlNode xmlNode = null;
                MatchCollection foundLines = null;

                if (TryLoadXml(text, ref xmlNode))
                {
                    LoadFromXml(xmlNode);
                }
                // Try to load XML using regular expression
                else if (TryLoadRegexOptions(text, ref foundLines))
                {
                    LoadFromRegex(foundLines);
                }
                // Load data from plain text
                else
                {
                    LoadFromPlainText(text);
                }
            }

            return this;
        }


        /// <summary>
        /// Loads special fields from query.
        /// </summary>
        /// <param name="query">Query text</param>
        /// <param name="type">Query type</param>
        /// <param name="valueFormat">Macro format of the value</param>
        /// <param name="textFormat">Macro format of the text</param>
        public SpecialFieldsDefinition LoadFromQuery(string query, QueryTypeEnum type = QueryTypeEnum.SQLQuery, string valueFormat = null, string textFormat = null)
        {
            if (!String.IsNullOrEmpty(query))
            {
                // Resolve macros
                query = MacroResolver.ResolveMacros(query);

                // Load data from query
                DataSet ds = ConnectionHelper.ExecuteQuery(query, null, type);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Insert items one-by one
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        MacroResolver itemResolver = MacroResolver.CreateChild();
                        itemResolver.SetAnonymousSourceData(row);
                        itemResolver.SetNamedSourceData("Item", row);

                        string value = !string.IsNullOrEmpty(valueFormat) ? itemResolver.ResolveMacros(valueFormat) : ((row[0] != DBNull.Value) ? ValidationHelper.GetString(row[0], null) : string.Empty);
                        string text = !string.IsNullOrEmpty(textFormat) ? itemResolver.ResolveMacros(textFormat) : ValidationHelper.GetString(row[1], null);

                        // Prepare field
                        SpecialField field = new SpecialField
                        {
                            Value = value,
                            Text = text
                        };

                        // Add field with nonempty text
                        if (!String.IsNullOrEmpty(field.Text))
                        {
                            Add(field);
                        }
                    }
                }
            }

            return this;
        }


        /// <summary>
        /// Loads special fields from macro data source.
        /// </summary>
        /// <param name="expression">Macro expression to provide a data source</param>
        /// <param name="valueFormat">Macro format of the value</param>
        /// <param name="textFormat">Macro format of the text</param>
        public SpecialFieldsDefinition LoadFromMacro(string expression, string valueFormat = null, string textFormat = null)
        {
            if (!String.IsNullOrEmpty(expression))
            {
                bool isComplexMacro = expression.StartsWithCSafe("{%") && expression.EndsWithCSafe("%}") && (expression.Length >= 4);
                expression = MacroProcessor.RemoveDataMacroBrackets(expression);

                // Resolve macros
                var evalResult = MacroResolver.ResolveMacroExpression(expression, true, !isComplexMacro);
                if ((evalResult != null) && (evalResult.Result != null))
                {
                    IEnumerable source;
                    if ((evalResult.Result is IEnumerable) && !(evalResult.Result is string))
                    {
                        source = (IEnumerable)evalResult.Result;
                    }
                    else
                    {
                        source = new ArrayList { evalResult.Result };
                    }

                    // Insert items one-by one
                    foreach (var item in source)
                    {
                        MacroResolver itemResolver = MacroResolver.CreateChild();
                        itemResolver.SetAnonymousSourceData(item);
                        itemResolver.SetNamedSourceData("Item", item);

                        var line = item.ToString();

                        // Use value from the source if provided
                        var field = GetSpecialField(line);

                        itemResolver.SetNamedSourceData("Value", field.Value);
                        itemResolver.SetNamedSourceData("Text", field.Text);

                        if (!string.IsNullOrEmpty(valueFormat))
                        {
                            field.Value = itemResolver.ResolveMacros(valueFormat);
                        }
                        if (!string.IsNullOrEmpty(textFormat))
                        {
                            field.Text = itemResolver.ResolveMacros(textFormat);
                        }

                        // Add field with nonempty text
                        if (!String.IsNullOrEmpty(field.Text))
                        {
                            Add(field);
                        }
                    }
                }
            }

            return this;
        }


        /// <summary>
        /// Check if input is one of special field macros.
        /// </summary>
        /// <param name="specialMacro">Special field macro</param>
        public static bool IsSpecialFieldMacro(string specialMacro)
        {
            return Macros.ContainsKey(specialMacro);
        }


        /// <summary>
        /// Gets string representation
        /// </summary>
        public override string ToString()
        {
            return String.Join(Environment.NewLine, this);
        }


        /// <summary>
        /// Fills list items.
        /// </summary>
        /// <param name="items">Items collection to be loaded</param>
        /// <param name="securityPurpose">Prevents the control values from being maliciously used in a different control. A suitable value can be the control's <see cref="Control.ClientID"/>.</param>
        public void FillItems(ListItemCollection items, string securityPurpose = null)
        {
            if (SortItems)
            {
                Sort((field, specialField) => String.Compare(field.Text, specialField.Text, StringComparison.Ordinal));
            }
            
            MacroResolver resolver = MacroResolver.CreateChild();
            if (String.IsNullOrEmpty(resolver.ResolverName))
            {
                resolver.ResolverName = ITEM_RESOLVER_NAME;
            }

            // Loop in reverse order
            for (int i = Count - 1; i >= 0; --i)
            {
                // Insert and resolve items
                InsertResolvedItem(this[i], items, resolver, securityPurpose);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks if source text is in XML format and tries to load into XML node.
        /// </summary>
        /// <param name="text">Options in XML format</param>
        /// <param name="xmlNode">Output XML node</param>
        /// <returns>Returns TRUE if loading was successful</returns>
        private bool TryLoadXml(string text, ref XmlNode xmlNode)
        {
            if (text.StartsWithCSafe("<") && text.EndsWithCSafe(">"))
            {
                try
                {
                    // Try to load XML
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml("<xml>" + text + "</xml>");
                    xmlNode = xmlDoc.FirstChild;

                    // Check child nodes
                    if (xmlNode.HasChildNodes)
                    {
                        foreach (XmlNode childNode in xmlNode.ChildNodes)
                        {
                            // Check that each node is in correct format
                            if ((childNode.Attributes == null) || (childNode.Attributes.Count == 0) || (childNode.Name != "item") || (childNode.Attributes["value"] == null) || (childNode.Attributes["text"] == null))
                            {
                                return false;
                            }
                        }

                        // Valid XML 
                        return true;
                    }

                    // Doesn't have child nodes
                    return false;
                }
                catch
                {
                    // Failed to load XML
                    return false;
                }
            }

            // Not XML
            return false;
        }


        /// <summary>
        /// Get lines from regular expression.
        /// </summary>
        /// <param name="text">Text to be parsed</param>
        /// <param name="foundLines">Matched collection</param>
        /// <returns>Returns TRUE if regular expression found any matches</returns>
        private bool TryLoadRegexOptions(string text, ref MatchCollection foundLines)
        {
            foundLines = OptionsRegex.Matches(text);
            return (foundLines.Count > 0);
        }


        /// <summary>
        /// Load fields from plain text.
        /// </summary>
        /// <param name="text">Source text</param>
        private void LoadFromPlainText(string text)
        {
            // Replace semicolons
            text = text.Trim().Replace(SEMICOLON_TO_REPLACE, REPLACED_SEMICOLON);

            // Resolve macros
            text = ResolveMacros(text);

            // Split definition to lines
            string[] lines = text.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                foreach (string line in lines)
                {
                    // Add field
                    Add(GetSpecialField(line));
                }
            }
        }


        /// <summary>
        /// Gets the special field from the given line in format [value] or [value];[text] or [value];[text];[macro]
        /// </summary>
        /// <param name="line">Line to parse</param>
        private static SpecialField GetSpecialField(string line)
        {
            // Get line items
            string[] items = line.Trim().Split(new[] { ';' });

            if (items.Length <= 3)
            {
                // Prepare field
                var field = new SpecialField();

                field.Value = items[0].Replace(REPLACED_SEMICOLON, ";");
                field.Text = (items.Length > 1) ? items[1].Replace(REPLACED_SEMICOLON, ";") : field.Value;

                if (items.Length > 2)
                {
                    field.VisibilityMacro = items[2].Replace(REPLACED_SEMICOLON, ";");
                }

                return field;
            }
            
            throw new FormatException("[SpecialFieldsDefinition.GetSpecialField]: The source text is in wrong format.");
        }


        /// <summary>
        /// Loads fields from old (5.5 and older) XML format.
        /// </summary>
        /// <param name="xmlNode">XML with items</param>
        private void LoadFromXml(XmlNode xmlNode)
        {
            // Loop through all items in XML
            foreach (XmlNode optNode in xmlNode.ChildNodes)
            {
                // Prepare field
                var field = new SpecialField
                    {
                        Value = XmlHelper.GetAttributeValue(optNode, "value"),
                        Text = XmlHelper.GetAttributeValue(optNode, "text"),
                        VisibilityMacro = XmlHelper.GetAttributeValue(optNode, "visibility")
                    };

                // Add field
                Add(field);
            }
        }


        /// <summary>
        /// Loads fields from text using reg-ex groups.
        /// </summary>
        /// <param name="foundLines">Matched reg-ex groups representing lines.</param>
        private void LoadFromRegex(MatchCollection foundLines)
        {
            foreach (Match match in foundLines)
            {
                // Prepare field
                SpecialField field = new SpecialField
                    {
                        Value = ValidationHelper.GetString(match.Groups["value"], null),
                        Text = ValidationHelper.GetString(match.Groups["text"], null),
                        VisibilityMacro = ValidationHelper.GetString(match.Groups["visible"], null)
                    };

                // Add field
                Add(field);
            }
        }


        /// <summary>
        /// Returns FieldDataTypeEnum according to current field info.
        /// </summary>
        /// <returns>Current data type</returns>
        private string GetCurrentDataType()
        {
            if (FieldInfo != null)
            {
                return FieldInfo.DataType;
            }
            else
            {
                return FieldDataType.Unknown;
            }
        }


        /// <summary>
        /// Resolves special macros used in special fields.
        /// </summary>
        /// <param name="text">Source text with field definition</param>
        private string ResolveMacros(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                foreach (string macro in Macros.TypedKeys)
                {
                    var replacement = Macros[macro];
                    text = text.Replace(macro, replacement.Item1 + ";" + ResHelper.GetString(String.Format(replacement.Item2, ResourcePrefix)));
                }
            }

            return text;
        }


        /// <summary>
        /// Inserts item into list collection, resolves macros and formats value field.
        /// </summary>
        /// <param name="field">Special field</param>
        /// <param name="items">List collection</param>
        /// <param name="resolver">Macro resolver</param>
        /// <param name="securityPurpose">Prevents the control values from being maliciously used in a different control. A suitable value can be the control's <see cref="Control.ClientID"/>.</param>
        private void InsertResolvedItem(SpecialField field, ListItemCollection items, MacroResolver resolver, string securityPurpose)
        {
            string text = resolver.ResolveMacros(field.Text);
            string value = ConvertData(resolver.ResolveMacros(field.Value));
            string visibility = field.VisibilityMacro;
            
            // Resolve visibility macro
            bool visible = string.IsNullOrEmpty(visibility) || ValidationHelper.GetBoolean(resolver.ResolveMacros(visibility), false);
            bool insertAllowed = AllowDuplicates || !items.Cast<ListItem>().Any(f => f.Value.EqualsCSafe(value) || f.Text.EqualsCSafe(text));

            // Field is visible and is not already present (field with same text or value)
            if (visible && insertAllowed)
            {
                ListItem item = new ListItem(text, value);
                if (SetUniqueIDs)
                {
                    item.Attributes.Add(DATA_HASH_ATTRIBUTE, ValidationHelper.GetHashString(item.Value, new HashSettings(securityPurpose)));
                }
                items.Insert(0, item);
            }
        }


        /// <summary>
        /// Converts value according to specific data type.
        /// </summary>
        /// <param name="value">Value of the item</param>
        /// <returns>Returns converted data</returns>
        private string ConvertData(string value)
        {
            // Detect empty value
            if (!String.IsNullOrEmpty(value))
            {
                var dataType = GetCurrentDataType();

                // Convert the value to a proper type
                value = ValidationHelper.GetString(DataTypeManager.ConvertToSystemType(TypeEnum.Field, dataType, value, CultureHelper.EnglishCulture), String.Empty);
            }

            return value;
        }

        #endregion
    }
}