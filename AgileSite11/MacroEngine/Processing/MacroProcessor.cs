using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class providing general macro processing methods.
    /// </summary>
    public static class MacroProcessor
    {
        #region "Variables"

        /// <summary>
        /// List of XML columns needed for signing the macros.
        /// </summary>
        public static readonly HashSet<string> XMLColumns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        { 
            "PageTemplateWebparts", 
            "PersonalizationWebparts", 
            "WebpartProperties", 
            "WidgetProperties", 
            "WebpartDefaultValues",
            "WidgetDefaultValues",
            "ReportParameters", 
            "ClassFormDefinition",
            "ClassXMLSchema",  
            "UserDialogsConfiguration", 
            "SiteInvoiceTemplate",
            "UserLastLogonInfo", 
            "FormDefinition", 
            "UserVisibility", 
            "ClassSearchSettings", 
            "GraphSettings", 
            "TableSettings",
            "TransformationHierarchicalXML",
            "IssueText", 
            "SavedReportParameters", 
            "ActionParameters", 
            "MacroruleParameters", 
            "ElementProperties",
            "UserControlParameters",
            "StepDefinition",
            "StepActionParameters",
            "RuleCondition",
            "DocumentWebParts",
            "NodeXML",
            "VersionXML"
        };


        /// <summary>
        /// Defines a replacement constant for macro processing to not resolve the macro at all
        /// </summary>
        public const string NOT_RESOLVE = "##NOT_RESOLVE##";

        #endregion


        #region "Events"

        /// <summary>
        /// Callback for data macro match.
        /// </summary>
        /// <param name="context">Macro processing context</param>
        public delegate string OnProcessMacro(MacroProcessingContext context);

        #endregion


        #region "General processing methods"

        /// <summary>
        /// Replaces data macros with given replacement or resolves them if replacement is null.
        /// </summary>
        /// <param name="text">Text where the macros will be replaced/resolved</param>
        /// <param name="replacement">Replacement string</param>
        /// <param name="processMacro">Callback to handle the macro resolving</param>
        public static string ProcessDataMacros(string text, string replacement, OnProcessMacro processMacro)
        {
            return ProcessMacros(text, replacement, new MacroProcessingParameters(), processMacro, new List<string> { "%" });
        }


        /// <summary>
        /// Replaces data macros with given replacement or resolves them if replacement is null.
        /// </summary>
        /// <param name="text">Text where the macros will be replaced/resolved</param>
        /// <param name="replacement">Replacement string</param>
        /// <param name="parameters">Custom parameter passed to the callback function</param>
        /// <param name="processMacro">Callback to handle the macro resolving</param>
        internal static string ProcessDataMacros(string text, string replacement, IMacroProcessingParameters parameters, OnProcessMacro processMacro)
        {
            return ProcessMacros(text, replacement, parameters, processMacro, new List<string> { "%" });
        }


        /// <summary>
        /// Replaces macros with given replacement or resolves them if replacement is null.
        /// </summary>
        /// <param name="data">Object to process</param>
        /// <param name="lambda">Lambda expression called on each macro</param>
        /// <param name="type">Type of the macro to resolve (if null, all types are resolved).</param>
        /// <param name="processOpenExpressions">If true, open expressions such as {% if (true) { %} any HTML code {%}%} are processed</param>
        public static bool ProcessMacros(IDataContainer data, Func<MacroProcessingContext, string, string> lambda, List<string> type = null, bool processOpenExpressions = true)
        {
            var someProcessed = false;

            foreach (string col in data.ColumnNames)
            {
                string colName = col;

                var oldVal = data.GetValue(col) as string;
                if (oldVal != null)
                {
                    // For efficiency reason call the method only if it contains "{"
                    if (oldVal.Contains("{"))
                    {
                        bool decode = IsXMLColumn(col);
                        var newVal = ProcessMacros(oldVal, "", new MacroProcessingParameters(decode: decode), context => lambda(context, colName), type, processOpenExpressions);
                        if (oldVal != newVal)
                        {
                            data.SetValue(col, newVal);
                            someProcessed = true;
                        }
                    }
                }
            }

            return someProcessed;
        }


        /// <summary>
        /// Replaces macros with given replacement or resolves them if replacement is null.
        /// </summary>
        /// <param name="text">Text where the macros will be replaced/resolved</param>
        /// <param name="replacement">Replacement string</param>
        /// <param name="parameters">Custom parameter passed to the callback function</param>
        /// <param name="processMacro">Callback to handle the macro resolving</param>
        /// <param name="type">Type of the macro to resolve (if null, all types are resolved).</param>
        /// <param name="processOpenExpressions">If true, open expressions such as {% if (true) { %} any HTML code {%}%} are processed</param>
        internal static string ProcessMacros(string text, string replacement, IMacroProcessingParameters parameters, OnProcessMacro processMacro, List<string> type = null, bool processOpenExpressions = true)
        {
            // Open expression = {% if (true) { %}<html></html>{%}%}
            // Holds last condition statement (if / for / ...) of an open expression
            string openExprCondition = null;
            char openExprType = '%';
            int openExprEnd = 0;
            int openExprStart = 0;
            int openExprCount = 0;

            string result = text;
            if (!String.IsNullOrEmpty(result))
            {
                int index = result.IndexOfCSafe('{');

                while ((index >= 0) && (index < result.Length - 1))
                {
                    // Get the current type and the bracket type (for nested macro support)
                    char currentType = result[index + 1];
                    string bracketType = "";
                    if ((result[index + 1] == '(') && (index < result.Length - 2))
                    {
                        int bracket = result.IndexOfCSafe(')', index + 1);
                        if ((bracket >= 0) && (index < result.Length - 1))
                        {
                            bracketType = result.Substring(index + 1, bracket - index);
                            currentType = result[bracket + 1];
                        }
                    }

                    // Check the validity of macro type
                    bool valid = IsSupportedType(currentType) && (type == null || (type.Contains(currentType.ToString())));
                    int startIndex = index + 2;
                    if (valid)
                    {
                        int len = 2 + bracketType.Length;

                        string endBracket = String.Concat(currentType, bracketType, "}");

                        int endIndex = result.IndexOfCSafe(endBracket, index + len);

                        // Special case for compatibility reasons. This check make sure that obsolete path macros located in xml encoded string will be recognized.
                        // Old path macros need to be detected correctly, because they are being transformed to new ones in import process and upgrade procedure using the MacroCompatibility class.
                        if ((endIndex == -1) && (currentType == '&') && (result.IndexOfCSafe("amp;", startIndex) == startIndex))
                        {
                            endIndex = result.IndexOfCSafe("&amp;}", index);
                            len += "amp;".Length;
                        }

                        if (endIndex >= 0)
                        {
                            // Security params
                            MacroIdentityOption identityOption = null;
                            string hash;

                            // Resolve the data macro
                            string expr = result.Substring(index + len, endIndex - index - len);
                            string exprWithoutParams = (currentType == '%' ? MacroSecurityProcessor.RemoveMacroSecurityParams(expr, out identityOption, out hash) : expr);
                            string exprTrim = exprWithoutParams.Trim();
                            string resolved;

                            if (processOpenExpressions && exprTrim.EndsWithCSafe("{"))
                            {
                                bool isElseExpr = exprTrim.StartsWithCSafe("}") && (currentType == openExprType) && exprTrim.Contains("else");
                                if (isElseExpr)
                                {
                                    // For expressions like "} else {" we can decrease the bracket as well
                                    openExprCount--;
                                }

                                // Open expression
                                if (openExprCount == 0)
                                {
                                    if (isElseExpr)
                                    {
                                        // Whole true case of the if statement will be an open expression "if () { ResolveMacros() } else {"
                                        string innerMacro = result.Substring(openExprEnd, index - openExprEnd);
                                        string innerText = GetInnerMacroExpression(innerMacro);

                                        openExprCondition = openExprCondition + innerText + exprWithoutParams;
                                    }
                                    else
                                    {
                                        // Save opening only when outer most open expression - to support open expressions within open expressions
                                        openExprCondition = exprWithoutParams;
                                    }
                                    // Save other info
                                    openExprType = currentType;
                                    openExprEnd = endIndex + len;

                                    if (!isElseExpr)
                                    {
                                        openExprStart = index;
                                    }
                                }
                                if (currentType == openExprType)
                                {
                                    openExprCount++;

                                    // Set the start of next macro search to the end of this macro
                                    startIndex = endIndex;
                                }
                            }
                            else if (processOpenExpressions && exprTrim.StartsWithCSafe("}") && (currentType == openExprType))
                            {
                                // End of an open expression
                                openExprCount--;
                                if (openExprCount == 0)
                                {
                                    // Escape all the characters to make valid string constant and recursively resolve all the macros within the inner text.
                                    string innerMacro = result.Substring(openExprEnd, index - openExprEnd);
                                    string innerText = GetInnerMacroExpression(innerMacro);

                                    // Build the whole expression
                                    string newExpr = openExprCondition + innerText + exprWithoutParams;

                                    string salt = parameters?.OldSalt;
                                    bool decode = parameters != null && parameters.Decode;

                                    if (MacroIdentityOption.IsNullOrEmpty(identityOption))
                                    {
                                        identityOption = parameters?.IdentityOption;
                                    }

                                    // Sign the whole expression before we process it (we sign the expression with the user of the last expression, integrity was ok, therefore we can do it)
                                    newExpr = MacroSecurityProcessor.AddMacroSecurityParams(newExpr, identityOption, salt, decode);

                                    var context = new MacroProcessingContext
                                    {
                                        MacroEnd = endIndex + len,
                                        MacroStart = openExprStart,
                                        Parameters = parameters,
                                        Replacement = replacement,
                                        SourceText = result,
                                        MacroType = currentType.ToString(),
                                        BracketType = "",
                                        Expression = newExpr,
                                        IsOpenExpression = true,
                                    };

                                    resolved = processMacro(context);

                                    if (replacement != NOT_RESOLVE)
                                    {
                                        result = String.Concat(result.Substring(0, openExprStart), resolved, result.Substring(endIndex + len));
                                        startIndex = openExprStart + resolved.Length;
                                    }
                                    else
                                    {
                                        startIndex = endIndex + len;
                                    }

                                    openExprCondition = null;
                                }
                            }
                            else
                            {
                                // Normal epxression
                                if (openExprCount == 0)
                                {
                                    var context = new MacroProcessingContext
                                    {
                                        MacroEnd = endIndex + len,
                                        MacroStart = index,
                                        Parameters = parameters,
                                        Replacement = replacement,
                                        SourceText = result,
                                        MacroType = currentType.ToString(),
                                        BracketType = bracketType,
                                        Expression = expr,
                                        IsOpenExpression = false,
                                    };

                                    resolved = processMacro(context);

                                    if (replacement != NOT_RESOLVE)
                                    {
                                        result = String.Concat(result.Substring(0, context.MacroStart), resolved, result.Substring(context.MacroEnd));
                                        startIndex = context.MacroStart + resolved.Length;
                                    }
                                    else
                                    {
                                        startIndex = context.MacroEnd;
                                    }
                                }
                            }
                        }
                    }

                    // Find the next macro
                    if (startIndex < result.Length - 1)
                    {
                        index = result.IndexOfCSafe('{', startIndex);
                    }
                    else
                    {
                        index = -1;
                    }
                }
            }

            return result;
        }


        private static string GetInnerMacroExpression(string innerMacro)
        {
            // Open expression result always gets to output
            return $"print(ResolveMacros(\"{MacroElement.EscapeSpecialChars(innerMacro)}\"))";
        }


        /// <summary>
        /// Returns true if the macro type is supported by MacroResolver.
        /// </summary>
        /// <param name="type">Type to check</param>
        internal static bool IsSupportedType(char type)
        {
            switch (type)
            {
                case '%':
                case '@':
                case '?':
                case '#':
                case '&':
                case '$':
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// HTML decodes macro definitions for given text.
        /// </summary>
        /// <param name="inputText">Input text</param>
        public static string DecodeMacros(string inputText)
        {
            if (!String.IsNullOrEmpty(inputText))
            {
                inputText = ProcessMacros(inputText, null, null, DecodeMacrosEvaluator, new List<string> { "%", "?", "#", "$" }, false);
            }

            return inputText;
        }


        /// <summary>
        /// HTML decode regex evaluator.
        /// </summary>
        /// <param name="context">Macro processing context</param>
        private static string DecodeMacrosEvaluator(MacroProcessingContext context)
        {
            return HTMLHelper.HTMLDecode(context.GetWholeMacroExpression());
        }


        /// <summary>
        /// Removes macros from input text. Macros are replaced by specified replacement.
        /// </summary>
        /// <param name="inputText">Input text</param>
        /// <param name="replacement">Replacement string</param>
        public static string RemoveMacros(string inputText, string replacement = "")
        {
            if (!String.IsNullOrEmpty(inputText) && inputText.Contains("{"))
            {
                return ProcessDataMacros(inputText, replacement, null, (x => replacement));
            }

            // No macros inside, return input text as is
            return inputText;
        }


        /// <summary>
        /// Gets the list of macros in the given text.
        /// </summary>
        /// <param name="originalText">Text to analyze for macros</param>
        /// <param name="onlySimpleMacros">If true, only simple macros are returned</param>
        public static string GetMacros(string originalText, bool onlySimpleMacros = false)
        {
            if (originalText != null)
            {
                StringBuilder sb = new StringBuilder();
                if (onlySimpleMacros)
                {
                    ProcessDataMacros(originalText, null, new MacroProcessingParameters(builder: sb), GetSimpleMacrosHandler);
                }
                else
                {
                    ProcessDataMacros(originalText, null, new MacroProcessingParameters(builder: sb), GetMacrosHandler);
                }

                return sb.ToString().Trim(';');
            }

            return null;
        }


        /// <summary>
        /// Handler for GetMacro method (appends macro to the StringBuilder passed in parameters). Appends only simple macros.
        /// </summary>
        /// <param name="context">Processing context</param>
        private static string GetSimpleMacrosHandler(MacroProcessingContext context)
        {
            if (MacroSecurityProcessor.IsSimpleMacro(context.Expression))
            {
                return GetMacrosHandler(context);
            }
            return "";
        }


        /// <summary>
        /// Handler for GetMacro method (appends macro to the StringBuilder passed in parameters).
        /// </summary>
        /// <param name="context">Processing context</param>
        private static string GetMacrosHandler(MacroProcessingContext context)
        {
            StringBuilder sb = context.Parameters.Builder;

            sb.Append(";", context.Expression);

            return "";
        }

        #endregion


        #region "Parameter processing"

        /// <summary>
        /// Un escapes the parameter value.
        /// </summary>
        /// <param name="value">Value to unescape</param>
        public static string UnescapeParameterValue(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Replace("\\|", "|").Replace("\\n", "\n").Replace("%\\}", "%}");
            }
            return value;
        }


        /// <summary>
        /// Escapes the parameter value (ensures nested macros within the parameter values are escaped).
        /// </summary>
        /// <param name="value">Value to escape</param>
        public static string EscapeParameterValue(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Replace("|", "\\|").Replace("\\\\|(", "\\|").Replace("\n", "\\n").Replace("%}", "%\\}");
            }
            return value;
        }


        /// <summary>
        /// Builds the macro parameter.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public static string BuildMacroParameter(string name, string value)
        {
            return String.Concat("|(", name, ")", EscapeParameterValue(value));
        }


        /// <summary>
        /// Builds macro parameter from identity option.
        /// </summary>
        /// <param name="identityOption">Identity option for which to build the parameter.</param>
        /// <returns>Macro parameter string.</returns>
        public static string BuildMacroParameter(MacroIdentityOption identityOption)
        {
            if (!String.IsNullOrEmpty(identityOption?.IdentityName))
            {
                return BuildMacroParameter(EvaluationParameters.IDENTITY_PARAM_LOWERED, identityOption.IdentityName);
            }
            return BuildMacroParameter(EvaluationParameters.USER_PARAM_LOWERED, identityOption?.UserName);
        }


        /// <summary>
        /// Removes parameter with given name from macro expression (expression should be without brackets {%%}).
        /// </summary>
        /// <param name="expression">Macro expression without the type brackets</param>
        /// <param name="name">Name of the parameter to remove</param>
        public static string RemoveParameter(string expression, string name)
        {
            string val;

            return RemoveParameter(expression, name, out val);
        }


        /// <summary>
        /// Removes parameter with given name from macro expression (expression should be without brackets {%%}).
        /// </summary>
        /// <param name="expression">Macro expression without the type brackets</param>
        /// <param name="name">Name of the parameter to remove</param>
        /// <param name="value">Value of the removed parameter will be passed here</param>
        public static string RemoveParameter(string expression, string name, out string value)
        {
            if (!String.IsNullOrEmpty(expression))
            {
                int indexOfQuotes = expression.LastIndexOfCSafe('"');
                string param = String.Concat("|(", name, ")");
                int index = expression.LastIndexOfCSafe(param);

                bool escaped = false;
                if ((index > 0) && (expression[index - 1] == '\\'))
                {
                    index--;
                    escaped = true;
                }

                if (index > indexOfQuotes)
                {
                    // Process the parameter only if it's not part of a string constant
                    string result = expression;
                    value = null;
                    if (index >= 0)
                    {
                        var endIndex = expression.IndexOfCSafe(escaped ? "\\|" : "|", index + 1);

                        int padding = (escaped ? 1 : 0);

                        if (endIndex >= 0)
                        {
                            value = expression.Substring(index + param.Length + padding, endIndex - index - param.Length - padding);
                        }
                        else
                        {
                            value = expression.Substring(index + param.Length + padding);
                        }

                        result = result.Substring(0, index) + (endIndex >= 0 ? result.Substring(endIndex) : "");
                    }
                    return result;
                }
            }

            value = null;
            return expression;
        }

        #endregion


        #region "Macro string operations"

        /// <summary>
        /// Removes data macro brackets {% %} from given text.
        /// </summary>
        /// <param name="value">Value to modify</param>
        public static string RemoveDataMacroBrackets(string value)
        {
            return RemoveMacroBrackets(value, "%");
        }


        /// <summary>
        /// Removes localization macro brackets {$ $} from given text.
        /// </summary>
        /// <param name="value">Value to modify</param>
        public static string RemoveLocalizationMacroBrackets(string value)
        {
            return RemoveMacroBrackets(value, "$");
        }


        /// <summary>
        /// Removes query macro brackets {? ?} from given text.
        /// </summary>
        /// <param name="value">Value to modify</param>
        public static string RemoveQueryMacroBrackets(string value)
        {
            return RemoveMacroBrackets(value, "?");
        }


        /// <summary>
        /// Removes all types macro brackets from given text.
        /// </summary>
        /// <param name="value">Value to modify</param>
        /// <param name="type">Type of macro which was removed</param>
        public static string RemoveMacroBrackets(string value, out string type)
        {
            string[] types = new[] { "%", "?", "$" };
            foreach (var t in types)
            {
                string newVal = RemoveMacroBrackets(value, t);
                if (value != newVal)
                {
                    type = t;
                    return newVal;
                }
            }
            type = null;
            return value;
        }


        /// <summary>
        /// Removes macro brackets from given text. Macro type is specified in macroChar parameter, e.g. '%' for data macros, '$' for localization macros...
        /// </summary>
        /// <param name="value">Value to modify</param>
        /// <param name="macroChar">Character that specifies macro type</param>
        private static string RemoveMacroBrackets(string value, string macroChar)
        {
            if (value.EndsWithCSafe(String.Format("{0}}}", macroChar)))
            {
                value = value.Substring(0, value.Length - 2);
            }
            if (value.StartsWithCSafe(String.Format("{{{0}", macroChar)))
            {
                value = value.Substring(2);
            }
            return value;
        }


        /// <summary>
        /// Encodes macro to prevent its resolving.
        /// </summary>
        /// <param name="macro">Macro definition</param>
        public static string EncodeMacro(string macro)
        {
            if (!String.IsNullOrEmpty(macro) && ContainsMacro(macro))
            {
                // Data macro
                macro = macro.Replace("{%", "{ %").Replace("%}", "% }");

                // Localization macro
                macro = macro.Replace("{$", "{ $").Replace("$}", "$ }");

                // Query string macro
                macro = macro.Replace("{?", "{ ?").Replace("?}", "? }");
            }

            return macro;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns true if the specified text contains macro.
        /// </summary>
        /// <param name="inputText">Text to check</param>
        public static bool ContainsMacro(string inputText)
        {
            // Quick check for macro start
            if ((inputText == null) || !inputText.Contains("{"))
            {
                return false;
            }

            // Check all data macros (ony data, query and localization macros are supported since v8)
            char[] types = new[] { '%', '?', '$' };
            return types.Any(type => ContainsMacroType(inputText, type));
        }


        /// <summary>
        /// Checks whether given text contains specified macro type
        /// </summary>
        /// <param name="inputText">Text to check</param>
        /// <param name="type">Type of the macro (%, $, ?)</param>
        private static bool ContainsMacroType(string inputText, char type)
        {
            int index = inputText.IndexOfCSafe("{" + type);
            if (index >= 0)
            {
                return inputText.IndexOfCSafe(type + "}", index + 2) >= 0;
            }
            return false;
        }


        /// <summary>
        /// Returns true if given text is in {$xxx$} format.
        /// Note that this method returns true only if the whole text is localization macro, it is NOT a contains method.
        /// </summary>
        /// <param name="text">Text to check</param>
        public static bool IsLocalizationMacro(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return false;
            }

            string keyTrimmed = text.Trim();

            if (keyTrimmed.StartsWithCSafe("{$") && keyTrimmed.EndsWithCSafe("$}") && (keyTrimmed.Length > 4))
            {
                var innerMacro = keyTrimmed.Substring(2, keyTrimmed.Length - 4);

                if (!innerMacro.Contains("{$") && !innerMacro.Contains("$}"))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true, if the given column is a XML column
        /// </summary>
        /// <param name="columnName">Column name</param>
        public static bool IsXMLColumn(string columnName)
        {
            return XMLColumns.Contains(columnName);
        }

        #endregion
    }
}