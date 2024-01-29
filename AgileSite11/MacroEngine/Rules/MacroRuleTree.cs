using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Represents a structure of of boolean expressions.
    /// </summary>
    [Serializable]
    public class MacroRuleTree
    {
        #region "Variables"

        private List<MacroRuleTree> mChildren;
        private MacroRuleTree mParent;

        private string mRuleText;
        private string mRuleName;
        private string mRuleCondition;
        private string mRuleParameters;

        private StringSafeDictionary<MacroRuleParameter> mParameters;

        private int mPosition;
        private string mOperator = "&&";

        /// <summary>
        /// Regular expression to match the parameters to be resolved
        /// </summary>
        /// Groups:                                                  (1:")(2:nam)(3:par )  (4:")
        private static readonly CMSRegex mParamRegex = new CMSRegex("(\"?){(\\w+)(|[^}]+)?}(\"?)", true);

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the child rules.
        /// </summary>
        public List<MacroRuleTree> Children
        {
            get
            {
                return mChildren ?? (mChildren = new List<MacroRuleTree>());
            }
            set
            {
                mChildren = value;
            }
        }


        /// <summary>
        /// Accepts an action that gets executed on the whole tree structure.
        /// </summary>
        public void Accept(Action<MacroRuleTree> visitorAction)
        {
            visitorAction(this);
            Children.ForEach(visitorAction);
        }


        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public MacroRuleTree Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }


        /// <summary>
        /// Gets or sets the relative position of the rule within the parent group.
        /// </summary>
        public int Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }


        /// <summary>
        /// Gets or sets operator of the rule.
        /// </summary>
        public string Operator
        {
            get
            {
                return mOperator;
            }
            set
            {
                mOperator = value;
            }
        }


        /// <summary>
        /// Gets or sets the associated rule text.
        /// </summary>
        public string RuleText
        {
            get
            {
                return mRuleText;
            }
            set
            {
                mRuleText = value;
            }
        }


        /// <summary>
        /// Gets or sets the associated rule parameters xml definition.
        /// </summary>
        public string RuleParameters
        {
            get
            {
                return mRuleParameters;
            }
            set
            {
                mRuleParameters = value;
            }
        }


        /// <summary>
        /// Gets or sets the associated rule name.
        /// </summary>
        public string RuleName
        {
            get
            {
                return mRuleName;
            }
            set
            {
                mRuleName = value;
            }
        }


        /// <summary>
        /// Gets or sets the associated rule K# condition.
        /// </summary>
        public string RuleCondition
        {
            get
            {
                return mRuleCondition;
            }
            set
            {
                mRuleCondition = value;
            }
        }


        /// <summary>
        /// Gets or sets the parameters of the rule (null for internal nodes).
        /// </summary>
        public StringSafeDictionary<MacroRuleParameter> Parameters
        {
            get
            {
                return mParameters ?? (mParameters = new StringSafeDictionary<MacroRuleParameter>());
            }
            set
            {
                mParameters = value;
            }
        }


        /// <summary>
        /// Returns IDPath of the group.
        /// </summary>
        public string IDPath
        {
            get
            {
                if (Parent == null)
                {
                    return "";
                }

                return (string.IsNullOrEmpty(Parent.IDPath) ? "" : Parent.IDPath + ".") + Position;
            }
        }


        /// <summary>
        /// Returns level of the group.
        /// </summary>
        public int Level
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.Level + 1;
            }
        }


        /// <summary>
        /// Returns true if group is a leaf (expression).
        /// </summary>
        public bool IsLeaf
        {
            get
            {
                return !string.IsNullOrEmpty(RuleText);
            }
        }


        /// <summary>
        /// Returns true if group has a previous sibling node.
        /// </summary>
        public bool HasPreviousSibling
        {
            get
            {
                if (Parent == null)
                {
                    return false;
                }
                if (Position == 0)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion


        #region "Methods"
        
        /// <summary>
        /// Returns the condition in K# representing this rule.
        /// </summary>
        public string GetCondition()
        {
            var sb = new StringBuilder();

            AppendCondition(sb);

            return sb.ToString();
        }

        
        /// <summary>
        /// Appends the whole subtree conditions to given <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">String builder to fill with conditions.</param>
        private void AppendCondition(StringBuilder sb)
        {
            if (IsLeaf)
            {
                if (Level > 0)
                {
                    sb.Append("(");
                }

                string leafCondition = (Parameters == null) ? RuleCondition : mParamRegex.Replace(RuleCondition, ReplaceParameter);

                sb.Append(leafCondition);

                if(Level > 0)
                {
                    sb.Append(")");
                }
            }
            else
            {
                // Print children
                bool wrapChildren = (Level > 0) && (Children.Count > 1);
                if (wrapChildren)
                {
                    sb.Append("(");
                }

                bool op = false;

                foreach (var child in Children)
                {
                    if (op)
                    {
                        sb.Append(" ", child.Operator, " ");
                    }

                    child.AppendCondition(sb);

                    op = true;
                }

                if (wrapChildren)
                {
                    sb.Append(")");
                }
            }
        }


        /// <summary>
        /// Replaces the given parameter with its value.
        /// </summary>
        /// <param name="m">Regex match</param>
        private string ReplaceParameter(Match m)
        {
            var key = m.Groups[2].Value;
            var match = m.Value;

            var p = Parameters[key];
            if (p != null)
            {
                var value = p.Value ?? "";

                // Escape the value automatically if encapsulated within string
                var escape = match.StartsWith("\"", StringComparison.Ordinal) && match.EndsWith("\"", StringComparison.Ordinal);

                // Process the parameters
                var par = m.Groups[3].Value;
                if (!String.IsNullOrEmpty(par) && par.Equals("|(escapeString)", StringComparison.OrdinalIgnoreCase))
                {
                    // Explicitly escape
                    escape = true;
                }

                // Escape the string if necessary
                if (escape)
                {
                    value = MacroElement.EscapeSpecialChars(value);
                }

                // Replace the placeholder with the value, keep potential quotes
                return String.Concat(m.Groups[1].Value, value, m.Groups[4].Value);
            }

            return match;
        }


        /// <summary>
        /// Returns xml of the rule designer (to store the layout and parameters).
        /// </summary>
        public virtual string GetXML()
        {
            return "<rules>" + GetXMLInternal() + "</rules>";
        }


        /// <summary>
        /// Returns xml of the rule designer (to store the layout and parameters).
        /// </summary>
        protected string GetXMLInternal()
        {
            StringBuilder sb = new StringBuilder();
            string pathParams = "pos=\"" + Position + "\"";
            string op = (Operator == "&&" ? "and" : "or");
            if (Parent != null)
            {
                pathParams += " par=\"" + Parent.IDPath + "\"";
            }
            if (IsLeaf)
            {
                sb.Append("<r ", pathParams, " op=\"", XmlHelper.XMLEncode(op), "\" n=\"", XmlHelper.XMLEncode(RuleName), "\" >");
                if (Parameters != null)
                {
                    foreach (string key in Parameters.Keys)
                    {
                        MacroRuleParameter p = Parameters[key];
                        if (p != null)
                        {
                            sb.Append("<p n=\"", XmlHelper.XMLEncode(key), "\"><t>", XmlHelper.XMLEncode(p.Text),
                                "</t><v>", XmlHelper.XMLEncode(p.Value), "</v><r>", p.Required ? "1" : "0",
                                "</r><d>", p.DefaultText, "</d><vt>", p.ValueType, "</vt><tv>", p.ApplyValueTypeConversion ? "1" : "0", "</tv></p>");
                        }
                    }
                }
                sb.Append("</r>");
            }
            else
            {
                if (Parent != null)
                {
                    sb.Append("<r ", pathParams, " op=\"", XmlHelper.XMLEncode(op), "\" />");
                }
                foreach (MacroRuleTree child in Children)
                {
                    sb.Append(child.GetXMLInternal());
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns list of rules which contain required parameters with empty value.
        /// </summary>
        public string ValidateParameters()
        {
            StringBuilder sb = new StringBuilder();

            foreach (MacroRuleParameter p in Parameters.Values)
            {
                if (p.Required && string.IsNullOrEmpty(p.Value))
                {
                    sb.Append(RuleText);
                    break;
                }
            }

            foreach (var child in Children)
            {
                string error = child.ValidateParameters();
                if (!string.IsNullOrEmpty(error))
                {
                    sb.AppendLine(error);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Loads MacroRuleTree from xml definition.
        /// </summary>
        /// <param name="xml">XML to build the designer from</param>
        public virtual void LoadFromXml(string xml)
        {
            var reader = new XmlTextReader(StringReader.New(xml));

            // Read all rules
            while (reader.ReadToFollowing("r"))
            {
                // Rule element
                string parent = reader.GetAttribute("par");
                int position = ValidationHelper.GetInteger(reader.GetAttribute("pos"), 0);
                string op = reader.GetAttribute("op") == "or" ? "||" : "&&";
                string name = reader.GetAttribute("n");

                MacroRuleTree rule;

                // Get the parent node
                var parentNode = GetNodeFromPath(parent);
                if (parentNode == null)
                {
                    throw new InvalidOperationException("Bad XML, cannot find parent node.");
                }

                if (!string.IsNullOrEmpty(name))
                {
                    // Particular macro rule
                    var info = MacroRuleInfoProvider.GetMacroRuleInfo(name);
                    if (info == null)
                    {
                        throw new InvalidOperationException("Unable to find macro rule '" + name + "'.");
                    }

                    var node = parentNode.AddRule(info, position);

                    node.Operator = op;
                    rule = node;
                }
                else
                {
                    // Empty rule without reference to particular rule
                    var node = parentNode.AddRule(new MacroRuleTree(), position, false);

                    node.Operator = op;
                    rule = node;
                }

                if (reader.IsEmptyElement)
                {
                    continue;
                }

                // Read parameters
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && reader.Name.Equals("p", StringComparison.Ordinal))
                    {
                        // Rule parameter
                        string paramName = reader.GetAttribute("n");

                        var p = new MacroRuleParameter();

                        // Read parameter settings
                        while (reader.Read())
                        {
                            // Parameter settings
                            while (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "t":
                                        p.Text = reader.ReadElementContentAsString();
                                        break;

                                    case "v":
                                        p.Value = reader.ReadElementContentAsString();
                                        break;

                                    case "r":
                                        p.Required = ValidationHelper.GetBoolean(reader.ReadElementContentAsString(), false);
                                        break;

                                    case "d":
                                        p.DefaultText = reader.ReadElementContentAsString();
                                        break;

                                    case "vt":
                                        p.ValueType = reader.ReadElementContentAsString();
                                        break;

                                    case "tv":
                                        p.ApplyValueTypeConversion = ValidationHelper.GetBoolean(reader.ReadElementContentAsString(), false);
                                        break;

                                    default:
                                        // Move to next node in case of unknown element
                                        reader.Read();
                                        break;
                                }
                            }

                            if ((reader.NodeType == XmlNodeType.EndElement) && reader.Name.Equals("p", StringComparison.Ordinal))
                            {
                                // End of parameter element
                                break;
                            }
                        }

                        rule.SetParameterValue(paramName, p);
                    }

                    if ((reader.NodeType == XmlNodeType.EndElement) && reader.Name.Equals("r", StringComparison.Ordinal))
                    {
                        // End of rule element
                        break;
                    }

                }
            }
        }


        /// <summary>
        /// Returns rule as a human readable sentence. If the expression is Rule(...) method than it uses XML for the rule, otherwise returns expression as it is.
        /// </summary>
        /// <param name="ruleExpression">Rule expression to render.</param>
        /// <param name="throwOnError">If true, the process throws an exception in case of parsing error.</param>
        public static string GetRuleCondition(string ruleExpression, bool throwOnError = false)
        {
            if (!string.IsNullOrEmpty(ruleExpression))
            {
                string rule = MacroProcessor.RemoveDataMacroBrackets(MacroSecurityProcessor.RemoveSecurityParameters(ruleExpression, false, null));

                try
                {
                    var xml = MacroExpression.ExtractParameter(rule, "rule", 0);

                    if ((xml != null) && (xml.Type == ExpressionType.Value))
                    {
                        return xml.Value.ToString();
                    }
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }

                    return CoreServices.Localization.GetString("macro.syntaxerror");
                }

                return HTMLHelper.HTMLEncode(rule);
            }

            return ruleExpression;
        }


        /// <summary>
        /// Returns rule as a human readable sentence. If the expression is Rule(...) method than it uses XML for the rule, otherwise returns expression as it is.
        /// </summary>
        /// <param name="ruleExpression">Rule expression to render.</param>
        /// <param name="includingMarkup">If true, rule is formatted using HTML markup</param>
        /// <param name="throwOnError">If true, the exception is thrown in case of error in parsing the expression</param>
        /// <param name="valueTransformation">Transformation function which is used to further modify the displayed parameter value (used for example to add TimeZones support for date time parameter values)</param>
        public static string GetRuleText(string ruleExpression, bool includingMarkup = true, bool throwOnError = false, Func<object, object> valueTransformation = null)
        {
            if (!string.IsNullOrEmpty(ruleExpression))
            {
                string rule = MacroProcessor.RemoveDataMacroBrackets(MacroSecurityProcessor.RemoveSecurityParameters(ruleExpression, false, null));

                try
                {
                    var xml = MacroExpression.ExtractParameter(rule, "rule", 1);

                    if ((xml != null) && (xml.Type == ExpressionType.Value))
                    {
                        try
                        {
                            // Try to build from XML
                            MacroRuleTree tree = new MacroRuleTree();
                            tree.LoadFromXml(xml.Value.ToString());
                            return GetRuleText(tree, includingMarkup, valueTransformation);
                        }
                        catch
                        {
                            if (throwOnError)
                            {
                                throw;
                            }

                            // XML is corrupted, extract condition
                            xml = MacroExpression.ExtractParameter(rule, "rule", 0);
                            if (xml != null)
                            {
                                return xml.Value.ToString();
                            }
                        }
                    }
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }

                    return CoreServices.Localization.GetString("macro.syntaxerror");
                }

                return HTMLHelper.HTMLEncode(rule);
            }

            return ruleExpression;
        }


        /// <summary>
        /// Returns rule as a human readable sentence.
        /// </summary>
        /// <param name="rule">Rule to render</param>
        /// <param name="includingMarkup">If true, rule is formatted using HTML markup</param>
        /// <param name="valueTransformation">Transformation function which is used to further modify the displayed parameter value (used for example to add TimeZones support for date time parameter values)</param>
        public static string GetRuleText(MacroRuleTree rule, bool includingMarkup = false, Func<object, object> valueTransformation = null)
        {
            StringBuilder sb = new StringBuilder();

            // Number of indentation
            int n = (includingMarkup ? 15 : 2) * (rule.Level - 1);

            // Append operator
            if (rule.Position > 0)
            {
                bool isAnd = (rule.Operator == "&&");
                if (includingMarkup)
                {
                    sb.Append("<div style=\"padding-left: ", n, "px\" \"><span class=\"ConditionBuilderOperator\">", (isAnd ? "and" : "or"), "</span></div>");
                }
                else
                {
                    sb.Append(GetSpaces(n), (isAnd ? "and" : "or"));
                }
            }

            if (rule.IsLeaf)
            {
                if (includingMarkup)
                {
                    sb.Append("<div id=\"", rule.IDPath, "\" style=\"padding-left: ", n, "px\">");
                }
                else
                {
                    sb.Append(GetSpaces(n));
                }

                string text = (includingMarkup ? HTMLHelper.HTMLEncode(rule.RuleText) : rule.RuleText);

                // Resolve the text of parameters
                if (rule.Parameters != null)
                {
                    foreach (string key in rule.Parameters.Keys)
                    {
                        var p = rule.Parameters[key];

                        string pText = (string.IsNullOrEmpty(p.Text) || p.Text.StartsWith("#", StringComparison.Ordinal) ? "?" : p.Text);

                        var parameterText = GetParameterText(pText, includingMarkup, "ConditionBuilderRuleParam", p.ApplyValueTypeConversion ? p.ValueType : "text", valueTransformation);

                        // Replace the parameter text, escape Regex substitutions (handle text as plain text)
                        text = Regex.Replace(text, "\\{" + key + "\\}", TextHelper.EncodeRegexSubstitutes(parameterText), CMSRegex.IgnoreCase);
                    }
                }

                sb.Append(text);

                if (includingMarkup)
                {
                    sb.Append("</div>");
                }
            }
            else
            {
                // Append the text of child nodes
                foreach (MacroRuleTree child in rule.Children)
                {
                    sb.Append(GetRuleText(child, includingMarkup, valueTransformation));
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Processes the text of parameter (handles multivalue parameters separated with new line).
        /// </summary>
        /// <param name="paramText">Parameter text</param>
        /// <param name="includingMarkup">If true, rule is formatted using HTML markup</param>
        /// <param name="cssClass">Class which will be used (used only when includingMarkup is true), can be null</param>
        /// <param name="valueType">Type of the parameter text value</param>
        /// <param name="valueTransformation">Transformation function which is used to further modify the displayed parameter value (used for example to add TimeZones support for date time parameter values)</param>
        public static string GetParameterText(string paramText, bool includingMarkup, string cssClass = null, string valueType = FieldDataType.Text, Func<object, object> valueTransformation = null)
        {
            const int SHORT_PARAM_TEXT_LIMIT = 3;
            const int SHORT_PARAM_TEXT_MAXDISPLAYITEMS = 1;

            // The parameter value is always in English format
            var defaultCulture = CultureHelper.EnglishCulture;

            if (includingMarkup)
            {
                if (string.IsNullOrEmpty(paramText))
                {
                    return CoreServices.Localization.GetString("macros.macrorule.emptystring");
                }
                if (string.IsNullOrWhiteSpace(paramText))
                {
                    return CoreServices.Localization.GetString("macros.macrorule.whitespaces");
                }

                if (paramText.Contains("\n"))
                {
                    // If parameter text contains new line, it means that it is a multiple value parameter, we need to process it differently - shorten the names
                    string[] paramsTexts = paramText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (paramsTexts.Length > SHORT_PARAM_TEXT_LIMIT)
                    {
                        // Displayed items
                        paramText = string.Join(", ", paramsTexts, 0, SHORT_PARAM_TEXT_MAXDISPLAYITEMS);
                        paramText += "&nbsp;" + string.Format(CoreServices.Localization.GetString("macros.macrorule.moreparameters"), paramsTexts.Length - SHORT_PARAM_TEXT_MAXDISPLAYITEMS);

                        // Add tooltip
                        paramText = "<span title=\"" + HTMLHelper.HTMLEncode(string.Join(", ", paramsTexts)) + "\">" + HTMLHelper.HTMLEncode(paramText) + "</span>";
                    }
                    else
                    {
                        paramText = HTMLHelper.HTMLEncode(string.Join(", ", paramsTexts));
                    }
                }
                else
                {
                    // Convert the value to text representation
                    var value = DataTypeManager.ConvertToSystemType(TypeEnum.Field, valueType, paramText, defaultCulture);
                    if (valueTransformation != null)
                    {
                        paramText = valueTransformation.Invoke(value).ToString();
                    }
                    else
                    {
                        paramText = value.ToString();
                    }

                    // Encode the text
                    paramText = HTMLHelper.HTMLEncode(paramText);
                }

                if (!string.IsNullOrEmpty(cssClass))
                {
                    paramText = "<span class=\"" + cssClass + "\">" + paramText + "</span>";
                }
            }
            else
            {
                var value = DataTypeManager.ConvertToSystemType(TypeEnum.Field, valueType, paramText, defaultCulture);
                if (valueTransformation != null)
                {
                    paramText = valueTransformation.Invoke(value).ToString();
                }
                else
                {
                    paramText = value.ToString();
                }

                paramText = TextHelper.EnsureLineEndings(paramText, ", ");
            }

            return ResHelper.LocalizeString(paramText);
        }


        /// <summary>
        /// Returns string of n spaces.
        /// </summary>
        /// <param name="n">Number of spaces</param>
        private static string GetSpaces(int n)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            for (int i = 0; i < n; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns the root of the rule tree.
        /// </summary>
        public MacroRuleTree GetRoot()
        {
            MacroRuleTree current = this;
            while (current.Parent != null)
            {
                current = current.Parent;
            }
            return current;
        }


        /// <summary>
        /// Sets the parameter value.
        /// </summary>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="parameter">Parameter value</param>
        public void SetParameterValue(string paramName, MacroRuleParameter parameter)
        {
            if (mParameters == null)
            {
                mParameters = new StringSafeDictionary<MacroRuleParameter>();
            }

            mParameters[paramName] = parameter;
        }


        /// <summary>
        /// Adds a new rule as a child of current node. Inserts the rule to the specified position.
        /// </summary>
        /// <param name="ruleInfo">Rule to add</param>
        /// <param name="position">Position of the rule within the group</param>
        public MacroRuleTree AddRule(MacroRuleInfo ruleInfo, int position)
        {
            MacroRuleTree rule = new MacroRuleTree();
            rule.RuleText = ruleInfo.MacroRuleText;
            rule.RuleCondition = ruleInfo.MacroRuleCondition;
            rule.RuleName = ruleInfo.MacroRuleName;
            rule.RuleParameters = ruleInfo.MacroRuleParameters;

            // Load parameters
            if (ruleInfo.MacroRuleParameters != null)
            {
                var fi = new DataDefinition(ruleInfo.MacroRuleParameters);
                var fields = fi.GetFields<FieldInfo>();

                foreach (FieldInfo item in fields)
                {
                    // Get information from the field
                    MacroRuleParameter p = new MacroRuleParameter();

                    p.DefaultText = (string.IsNullOrEmpty(item.Caption) ? "?" : item.Caption);
                    p.Required = !item.AllowEmpty;
                    p.Text = "#" + item.Caption;

                    if (item.DefaultValue != null)
                    {
                        string[] defaultValues = item.DefaultValue.Split(';');
                        if (defaultValues.Length == 2)
                        {
                            p.Value = defaultValues[0];
                            p.Text = defaultValues[1];
                        }
                        else
                        {
                            p.Value = item.DefaultValue;
                        }
                    }

                    rule.SetParameterValue(item.Name, p);
                }
            }

            return AddRule(rule, position, true);
        }


        /// <summary>
        /// Adds a new rule as a child of current node. Inserts the rule to the specified position.
        /// </summary>
        /// <param name="rule">Rule subtree to add</param>
        /// <param name="position">Position of the rule within the group</param>
        /// <param name="setOperator">If true, operator is set according to previous rule</param>
        private MacroRuleTree AddRule(MacroRuleTree rule, int position, bool setOperator)
        {
            if (IsLeaf)
            {
                throw new NotSupportedException("Cannot add a group into the leaf group.");
            }

            rule.Parent = this;

            // Set the position to the correct value if not initially correct
            if ((position < 0) || (position >= Children.Count))
            {
                rule.Position = Children.Count;
                Children.Add(rule);
            }
            else
            {
                rule.Position = position;
                Children.Insert(position, rule);
            }

            // Inherit the operator from previous or next rule or set && if cannot be inherited
            if (setOperator)
            {
                if (position > 0)
                {
                    rule.Operator = Children[position - 1].Operator;
                }
                else
                {
                    // Position is 0, inherit from next sibling
                    if (Children.Count > 1)
                    {
                        rule.Operator = Children[1].Operator;
                    }
                    else
                    {
                        rule.Operator = "&&";
                    }
                }
            }

            ResetPositions();

            return rule;
        }


        /// <summary>
        /// Adds given group as a new child.
        /// </summary>
        /// <param name="position">Relative position within the children collection</param>
        public void RemoveNode(int position)
        {
            RemoveNode(position, true);
        }


        /// <summary>
        /// Adds given group as a new child.
        /// </summary>
        /// <param name="position">Relative position within the children collection</param>
        /// <param name="removeParent">If true, parent group will be removed if we removed last item within the group</param>
        public void RemoveNode(int position, bool removeParent)
        {
            if ((position >= 0) && (Children.Count > position))
            {
                Children.RemoveAt(position);
                ResetPositions();

                // If the rule was last in it's group, delete the group
                if (removeParent && (Children.Count == 0))
                {
                    Parent?.RemoveNode(Position, true);
                }
            }
        }


        /// <summary>
        /// Performs the autoindentation of the expression (according to priority of 'or' and 'and' operators).
        /// </summary>
        public void AutoIndent()
        {
            // Autoindentation makes sense only when at least 3 children are present
            int count = Children.Count;
            if (count > 2)
            {
                bool isAnd = (Children[1].Operator == "&&");
                bool needsProcessing = false;

                // Check if reorganization is needed
                for (int i = 2; i < count; i++)
                {
                    if (isAnd != (Children[i].Operator == "&&"))
                    {
                        needsProcessing = true;
                        break;
                    }
                }

                if (needsProcessing)
                {
                    var children = new List<MacroRuleTree>();
                    children.AddRange(Children);

                    // Indent first expression if the next is and
                    if (children[1].Operator == "&&")
                    {
                        children[0].Indent();
                    }

                    for (int i = 1; i < count; i++)
                    {
                        // Indent the expression if it's and or next expression is and
                        if ((children[i].Operator == "&&") || ((i < count - 1) && (children[i + 1].Operator == "&&")))
                        {
                            children[i].Indent();
                        }
                    }
                }
            }

            // Autoindent child nodes
            for (int i = 0; i < Children.Count; i++)
            {
                if (!Children[i].IsLeaf)
                {
                    Children[i].AutoIndent();
                }
            }
        }


        /// <summary>
        /// Removes brackets which are not needed.
        /// </summary>
        /// <param name="node">Node to process</param>
        public static void RemoveBrackets(MacroRuleTree node)
        {
            if ((node.Parent != null) && (node.Parent.Children.Count == 1) && !node.Parent.Children[0].IsLeaf)
            {
                // Remove brackets around leaf expressions
                node.Parent.Children = node.Children;
                foreach (MacroRuleTree child in node.Children)
                {
                    child.Parent = node.Parent;
                }
                RemoveBrackets(node.Parent);
            }
            else
            {
                if ((node.Parent != null) && (node.Children.Count == 1) && node.Children[0].IsLeaf)
                {
                    // Replace the node with its only children, operator is set from the original node
                    int nodePos = node.Position;
                    node.Children[0].Operator = node.Operator;
                    node.Parent.AddRule(node.Children[0], nodePos, false);
                    node.Parent.RemoveNode(nodePos + 1);
                }
                else
                {
                    foreach (MacroRuleTree child in node.Children.ToArray())
                    {
                        RemoveBrackets(child);
                    }
                }
            }
        }


        /// <summary>
        /// Indents the node with one level.
        /// </summary>
        public void Indent()
        {
            // If the previous sibling is internal node, append it as a child of this sibling
            if (HasPreviousSibling)
            {
                MacroRuleTree sibling = Parent.Children[Position - 1];
                if (!sibling.IsLeaf)
                {
                    // Move to the existing sibling only if the last operator is the same
                    if (sibling.Children.Count > 0)
                    {
                        MacroRuleTree previous = sibling.Children[sibling.Children.Count - 1];
                        if ((previous.Operator == Operator) || (previous.Position == 0))
                        {
                            MoveNode(this, sibling, sibling.Children.Count, false);
                            return;
                        }
                    }
                }
            }

            // If node does not have a parent or it's previous sibling is leaf, create new group
            MacroRuleTree node = new MacroRuleTree();
            node.Operator = Operator;

            Parent.AddRule(node, Position, false);
            MoveNode(this, node, 0, false);
        }


        /// <summary>
        /// Unindents the node with one level.
        /// </summary>
        public void Unindent()
        {
            if (Parent?.Parent != null)
            {
                if (Position == 0)
                {
                    // Set current node's operator before preparing the parent node for remaining children.
                    Operator = Parent.Operator;

                    // If this is not the only child, other children will need a parent node. Use current parent node, but with first remaining node's operator.
                    if (Parent.Children.Count > 1)
                    {
                        Parent.Operator = Parent.Children[1].Operator;
                    }

                    // If we are moving the first item in a group move it up
                    MoveNode(this, Parent.Parent, Parent.Position, false);
                }
                else
                {
                    // Otherwise move it down
                    MoveNode(this, Parent.Parent, Parent.Position + 1, false);
                }
            }
        }


        /// <summary>
        /// Moves the group to given location.
        /// </summary>
        /// <param name="sourcePath">Position path of the source</param>
        /// <param name="targetPath">Position path of the target</param>
        /// <param name="targetPos">Position within the target</param>
        public void MoveNode(string sourcePath, string targetPath, int targetPos)
        {
            MacroRuleTree srcGroup = GetNodeFromPath(sourcePath);
            MacroRuleTree targetGroup = GetNodeFromPath(targetPath);

            MoveNode(srcGroup, targetGroup, targetPos, true);
        }


        /// <summary>
        /// Returns node from it's ID path.
        /// </summary>
        /// <param name="idPath">ID path</param>
        private MacroRuleTree GetNodeFromPath(string idPath)
        {
            string[] source = idPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            MacroRuleTree srcGroup = GetRoot();
            foreach (string posStr in source)
            {
                int pos = ValidationHelper.GetInteger(posStr, 0);
                if (srcGroup.Children.Count > pos)
                {
                    srcGroup = srcGroup.Children[pos];
                }
                else
                {
                    return null;
                }
            }
            return srcGroup;
        }


        /// <summary>
        /// Moves the node within the tree.
        /// </summary>
        /// <param name="srcGroup">Source node</param>
        /// <param name="targetGroup">Target node</param>
        /// <param name="targetPos">Target position within target group</param>
        /// <param name="setOperator">If true, operator is set according to previous rule</param>
        private static void MoveNode(MacroRuleTree srcGroup, MacroRuleTree targetGroup, int targetPos, bool setOperator)
        {
            int originalPos = srcGroup.Position;
            if (targetGroup != srcGroup.Parent)
            {
                MacroRuleTree srcParent = srcGroup.Parent;

                // Refresh the tree after moving node closer to the root (on the same branch), because the position-refresh during the addition executes twice for the moved node
                bool refreshRequired = srcParent.IDPath.StartsWith(targetGroup.IDPath, StringComparison.Ordinal);

                // Move to different groups
                targetGroup.AddRule(srcGroup, targetPos, setOperator);
                srcParent.RemoveNode(originalPos);

                if (refreshRequired)
                {
                    targetGroup.ResetPositions();
                }
            }
            else
            {
                // Move within one group
                targetGroup.AddRule(srcGroup, targetPos, setOperator);
                if (originalPos > targetPos)
                {
                    targetGroup.RemoveNode(originalPos + 1);
                }
                else
                {
                    targetGroup.RemoveNode(originalPos);
                }
            }
        }


        /// <summary>
        /// Sets correct positions according to current state.
        /// </summary>
        public void ResetPositions()
        {
            int i = 0;
            foreach (MacroRuleTree child in Children)
            {
                child.Position = i++;
                child.ResetPositions();
            }
        }

        #endregion
    }
}