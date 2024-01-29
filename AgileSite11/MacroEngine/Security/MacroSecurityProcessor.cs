using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class providing general macro security parameters processing.
    /// </summary>
    public static class MacroSecurityProcessor
    {
        #region "Events"

        /// <summary>
        /// Event which is called whenever the security check on object is requested. It's called after the standard check is processed and the result can be overriden with this handler.
        /// </summary>
        public static event EventHandler<MacroSecurityEventArgs> OnCheckObjectPermissions;

        #endregion


        #region "Variables"

        /// <summary>
        /// If true, the macro integrity is checked when evaluating macro
        /// </summary>
        private static readonly BoolAppSetting CheckIntegrity = new BoolAppSetting("CMSCheckMacroIntegrity", true);


        /// <summary>
        /// Defines a regular expression that detects a complex macro (in case it is matched)
        /// </summary>
        public static CMSRegex RegExpComplexMacro = new CMSRegex("[.[}]");

        #endregion


        #region "Add security parameters methods"

        /// <summary>
        /// Adds security parameters to every macro.
        /// </summary>
        /// <param name="text">Text with macros</param>
        /// <param name="identityOption">Identity option to sign with</param>
        /// <param name="signatures">Dictionary with old identity options indexed by expression</param>
        /// <param name="decodeBeforeSign">If true, expression is decoded before the security params are computed</param>
        /// <param name="saltToUse">Salt to use for hash function</param>
        public static string AddSecurityParameters(string text, MacroIdentityOption identityOption, IDictionary<string, MacroIdentityOption> signatures, bool decodeBeforeSign = false, string saltToUse = null)
        {
            return MacroProcessor.ProcessMacros(text, null, new MacroProcessingParameters(signatures, decodeBeforeSign, newSalt: saltToUse, identityOption: identityOption), AddSecurityParameter, new List<string> { "%", "?" });
        }


        /// <summary>
        /// Callback for removing parameters from macros.
        /// </summary>
        /// <param name="context">Macro processing context</param>
        private static string AddSecurityParameter(MacroProcessingContext context)
        {
            string originalExpr = null;
            if (context.IsOpenExpression)
            {
                originalExpr = context.GetOriginalExpression();
            }

            if (!IsSimpleMacro(context.Expression))
            {
                if (context.Expression.EndsWith("@", StringComparison.Ordinal))
                {
                    // Anonymous expression - Not signed
                    if (context.IsOpenExpression)
                    {
                        return originalExpr;
                    }

                    return context.GetWholeMacroExpression();
                }

                MacroIdentityOption signingIdentityOption = null;
                string saltToUse = context.Parameters.NewSalt;
                IDictionary<string, MacroIdentityOption> signatures = context.Parameters.Signatures;
                bool decode = context.Parameters.Decode;

                // If expressions ends with # do not sign it, keep it like that
                if (context.Expression.EndsWithCSafe("#"))
                {
                    // Remove the hash
                    context.Expression = context.Expression.Substring(0, context.Expression.Length - 1);

                    // Find the original signature in the dictionary
                    if (signatures != null)
                    {
                        signatures.TryGetValue(context.Expression, out signingIdentityOption);
                    }
                }

                // If identity option was not found, sign it with current identity option
                if (signingIdentityOption == null)
                {
                    signingIdentityOption = context.Parameters.IdentityOption;
                }

                // Sign the macro
                if (context.IsOpenExpression)
                {
                    MacroIdentityOption originalIdentityOption;
                    string type;

                    originalExpr = MacroProcessor.RemoveMacroBrackets(originalExpr, out type).TrimEnd('#');
                    originalExpr = RemoveMacroSecurityParams(originalExpr, out originalIdentityOption);

                    return FormatProcessedMacro(context, AddMacroSecurityParams(originalExpr, signingIdentityOption, saltToUse, decode));
                }

                var signed = AddMacroSecurityParams(context.Expression, signingIdentityOption, saltToUse, decode);
                return FormatProcessedMacro(context, signed);
            }

            if (context.IsOpenExpression && (originalExpr != null))
            {
                return originalExpr.Replace("#%}", "%}");
            }

            return FormatProcessedMacro(context, context.Expression.TrimEnd('#'));
        }


        /// <summary>
        /// Adds |(hash) and |(user) security parameters to given expression.
        /// </summary>
        /// <param name="expression">Macro expression to add the parameters to</param>
        /// <param name="identityOption">Identity option to sign with</param>
        /// <param name="saltToUse">Salt to use for hash function</param>
        /// <param name="decode">If true, decodes expression before computing signature</param>
        public static string AddMacroSecurityParams(string expression, MacroIdentityOption identityOption, string saltToUse = null, bool decode = false)
        {
            // Do not sign when the macro is anonymous
            if (expression.EndsWith("@", StringComparison.Ordinal))
            {
                return expression;
            }

            string macro = decode ? XmlHelper.XMLDecode(expression) : expression;

            MacroIdentityOption originalIdentityOption;
            macro = RemoveMacroSecurityParams(macro, out originalIdentityOption);

            string hash = GetMacroHash(macro, identityOption, saltToUse);
            string output = AppendMacroSignature(macro, identityOption, hash);

            if (decode)
            {
                output = XmlHelper.XMLEncode(output);
            }

            return output;
        }


        private static string AppendMacroSignature(string macro, MacroIdentityOption identityOption, string hash)
        {
            return macro + MacroProcessor.BuildMacroParameter(identityOption) + MacroProcessor.BuildMacroParameter(EvaluationParameters.HASH_PARAM_LOWERED, hash);
        }

        #endregion


        #region "Remove security parameters methods"

        /// <summary>
        /// Removes security parameters from all macros.
        /// </summary>
        /// <param name="text">Text with macros</param>
        /// <param name="replaceWithHash">If true, macros which contained security information will be returned with hash at the end</param>
        /// <param name="signatures">Dictionary of identity options indexed by expression to be built while removing security parameters</param>
        public static string RemoveSecurityParameters(string text, bool replaceWithHash, IDictionary<string, MacroIdentityOption> signatures)
        {
            return RemoveSecurityParameters(text, replaceWithHash, signatures, true);
        }


        /// <summary>
        /// Removes security parameters from all macros.
        /// </summary>
        /// <param name="text">Text with macros</param>
        /// <param name="replaceWithHash">If true, macros which contained security information will be returned with hash at the end</param>
        /// <param name="signatures">Dictionary of identity options indexed by expression to be built while removing security parameters</param>
        /// <param name="clearSignatures">If true, <paramref name="signatures"/> dictionary is cleared before text processing</param>
        public static string RemoveSecurityParameters(string text, bool replaceWithHash, IDictionary<string, MacroIdentityOption> signatures, bool clearSignatures)
        {
            string replacement = null;
            if (replaceWithHash)
            {
                replacement = "#";
            }

            if (clearSignatures)
            {
                signatures?.Clear();
            }

            return MacroProcessor.ProcessMacros(text, replacement, new MacroProcessingParameters(signatures), RemoveSecurityParameter, new List<string> { "%", "?" });
        }


        /// <summary>
        /// Marks all macros int the <paramref name="text"/> as anonymous.
        /// </summary>
        /// <param name="text">Text with macros.</param>
        /// <param name="signatures">Dictionary of identity options indexed by expression to be built while removing security parameters</param>
        /// <returns>Text with macros without signatures and marked as anonymous.</returns>
        /// <remarks>This is for internal use only. Do not use it.</remarks>
        public static string RemoveSecurityParametersAndAnonymize(string text, IDictionary<string, MacroIdentityOption> signatures)
        {
            signatures.Clear();

            return MacroProcessor.ProcessMacros(text, "@", new MacroProcessingParameters(signatures), RemoveSecurityParametersAndAddReplacement, new List<string> { "%", "?" });
        }


        /// <summary>
        /// Callback for marking macros for replacement.
        /// </summary>
        /// <param name="context">Macro processing context.</param>
        private static string RemoveSecurityParametersAndAddReplacement(MacroProcessingContext context)
        {
            MacroIdentityOption identityOption;
            string expr;
            
            // Sign the macro
            if (context.IsOpenExpression)
            {
                string _;
                string originalExpr = context.GetOriginalExpression();
                string expressionWithoutBrackets = MacroProcessor.RemoveMacroBrackets(originalExpr, out _);

                expr = RemoveMacroSecurityParams(expressionWithoutBrackets, out identityOption);
            }
            else
            {
                expr = RemoveMacroSecurityParams(context.Expression, out identityOption);
            }

            return FormatProcessedMacro(context, $"{expr.TrimEnd('#')}{context.Replacement}");
        }


        /// <summary>
        /// Callback for removing parameters from macros.
        /// </summary>
        /// <param name="context">Macro processing context</param>
        private static string RemoveSecurityParameter(MacroProcessingContext context)
        {
            string originalExpr = null;
            if (context.IsOpenExpression)
            {
                originalExpr = context.GetOriginalExpression();
            }

            if (context.Expression.EndsWith("@", StringComparison.Ordinal))
            {
                // Anonymous, do not remove anything
                if (context.IsOpenExpression)
                {
                    return originalExpr;
                }

                return context.GetWholeMacroExpression();
            }

            MacroIdentityOption identityOption;
            string expr;

            // Sign the macro
            if (context.IsOpenExpression)
            {
                string type;
                string result = MacroProcessor.RemoveMacroBrackets(originalExpr, out type);

                expr = RemoveMacroSecurityParams(result, out identityOption);
            }
            else
            {
                expr = RemoveMacroSecurityParams(context.Expression, out identityOption).TrimEnd('#');
            }

            if (!IsSimpleMacro(expr))
            {
                string retval = String.Format("{{{0}{1}{2}{3}{1}{0}}}", context.BracketType, context.MacroType, expr, (context.Replacement != null) && !expr.EndsWith("#", StringComparison.Ordinal) ? "#" : String.Empty);

                if (context.Parameters?.Signatures != null)
                {
                    context.Parameters.Signatures[expr] = identityOption;
                }
                return retval;
            }
            
            if (context.IsOpenExpression && (originalExpr != null))
            {
                return originalExpr.Replace("#%}", "%}");
            }

            return FormatProcessedMacro(context, expr);
        }


        /// <summary>
        /// Removes |(hash) and |(user) or |(identity) security parameters from given expression.
        /// </summary>
        /// <param name="expression">Macro expression to remove the parameters from</param>
        /// <param name="identityOption">Value of the removed identity option parameter (either macro identity name or user name)</param>
        public static string RemoveMacroSecurityParams(string expression, out MacroIdentityOption identityOption)
        {
            string hash;

            return RemoveMacroSecurityParams(expression, out identityOption, out hash);
        }


        /// <summary>
        /// Removes |(hash) and |(user) or |(identity) security parameters from given expression.
        /// </summary>
        /// <param name="expression">Macro expression to remove the parameters from</param>
        /// <param name="identityOption">Value of the removed identity option parameter (either macro identity name or user name)</param>
        /// <param name="hash">Value of the removed hash parameter</param>
        public static string RemoveMacroSecurityParams(string expression, out MacroIdentityOption identityOption, out string hash)
        {
            var hashTrimmed = MacroProcessor.RemoveParameter(expression, EvaluationParameters.HASH_PARAM_LOWERED, out hash);

            string macroIdentity;
            var signatureTrimmed = MacroProcessor.RemoveParameter(hashTrimmed, EvaluationParameters.IDENTITY_PARAM_LOWERED, out macroIdentity);

            string user;
            var userTrimmed = MacroProcessor.RemoveParameter(signatureTrimmed, EvaluationParameters.USER_PARAM_LOWERED, out user);

            identityOption = new MacroIdentityOption { IdentityName = macroIdentity, UserName = user };
            return userTrimmed;
        }

        #endregion


        #region "Refresh security parameters methods"

        /// <summary>
        /// Recreates all the macros signatures according to new specified salt in all string columns of each row of the given DataSet (modifies the DataSet).
        /// Keeps the owners of the macros untouched (verifies the integrity against old salt).
        /// </summary>
        /// <param name="ds">DataSet containing macros</param>
        /// <param name="identityOption">Identity option to sign with</param>
        public static void RefreshSecurityParameters(DataSet ds, MacroIdentityOption identityOption)
        {
            TransformAllMacroValues(ds, (oldVal, decode) => AddSecurityParameters(oldVal, identityOption, null, decode));
        }


        /// <summary>
        /// Recreates all the macros signatures according to new specified salt.
        /// Keeps the owners of the macros untouched (verifies the integrity against old salt).
        /// </summary>
        /// <param name="text">Text containing macros</param>
        /// <param name="oldSalt">Old salt used to generate old signatures (to verify integrity of old macros); if null, the integrity is not checked</param>
        /// <param name="newSalt">New salt which should be used to generate new signatures</param>
        /// <param name="decodeBeforeSign">If true, expression is decoded before the security params are computed</param>
        private static string RefreshSecurityParameters(string text, string oldSalt, string newSalt, bool decodeBeforeSign = false)
        {
            return MacroProcessor.ProcessMacros(text, "", new MacroProcessingParameters(decode: decodeBeforeSign, oldSalt: oldSalt, newSalt: newSalt), RefreshSecurityParameters, new List<string> { "%", "?" });
        }


        /// <summary>
        /// Loops through all text columns of given info and signs ALL macros (even already signed) with specified identity option.
        /// </summary>
        /// <param name="info">Info object to process</param>
        /// <param name="identityOption">Identity option to sign with</param>
        /// <param name="saveObject">If set to true, than saves the object to DB if any macro signature has been changed.</param>
        /// <param name="saltToUse">Salt to use for hash function</param>
        public static bool RefreshSecurityParameters(BaseInfo info, MacroIdentityOption identityOption, bool saveObject, string saltToUse = null)
        {
            if (info == null)
            {
                return false;
            }

            bool save = false;
            foreach (string col in info.ColumnNames)
            {
                object val = info.GetValue(col);

                var oldVal = val as string;
                if (oldVal != null)
                {
                    // For efficiency reason call the method only if it contains "{"
                    if (oldVal.Contains("{"))
                    {
                        bool decode = MacroProcessor.IsXMLColumn(col);
                        string newVal = AddSecurityParameters(oldVal, identityOption, null, decode, saltToUse);
                        if (oldVal != newVal)
                        {
                            info.SetValue(col, newVal);
                            save = true;
                        }
                    }
                }
            }

            if (saveObject && save)
            {
                info.Generalized.SetObject();
            }

            return save;
        }


        /// <summary>
        /// Loops through all text columns of given info and recreates all the macros signatures according to new specified salt.
        /// Keeps the owners of the macros untouched (verifies the integrity against old salt).
        /// </summary>
        /// <param name="info">Info object to process</param>
        /// <param name="oldSalt">Old salt used to generate old signatures (to verify integrity of old macros); if null, the integrity is not checked</param>
        /// <param name="newSalt">New salt which should be used to generate new signatures</param>
        /// <param name="saveObject">If set to true, than saves the object to DB if any macro signature has been changed.</param>
        public static bool RefreshSecurityParameters(BaseInfo info, string oldSalt, string newSalt, bool saveObject)
        {
            if (info == null)
            {
                return false;
            }

            bool save = false;

            foreach (string col in info.ColumnNames)
            {
                object val = info.GetValue(col);

                var oldVal = val as string;
                if (oldVal != null)
                {
                    // For efficiency reason call the method only if it contains "{"
                    if (oldVal.Contains("{"))
                    {
                        bool decode = MacroProcessor.IsXMLColumn(col);
                        string newVal = RefreshSecurityParameters(oldVal, oldSalt, newSalt, decode);

                        if (oldVal != newVal)
                        {
                            info.SetValue(col, newVal);
                            save = true;
                        }
                    }
                }
            }

            if (saveObject && save)
            {
                info.Generalized.SetObject();
            }

            return save;
        }


        /// <summary>
        /// Callback for recreating all the macros signatures according to new specified salt.
        /// </summary>
        /// <param name="context">Macro processing context</param>
        internal static string RefreshSecurityParameters(MacroProcessingContext context)
        {
            if (context.Expression.EndsWith("@", StringComparison.Ordinal))
            {
                // Anonymous, do not process
                return context.GetWholeMacroExpression();
            }

            string oldSalt = context.Parameters.OldSalt;
            string newSalt = context.Parameters.NewSalt;

            MacroIdentityOption identityOption;
            string oldHash;
            string expr = context.Parameters.Decode ? XmlHelper.XMLDecode(context.Expression) : context.Expression;
            expr = RemoveMacroSecurityParams(expr, out identityOption, out oldHash).TrimEnd('#');

            if (!IsSimpleMacro(expr))
            {
                bool valid = false;
                if (oldSalt == null)
                {
                    valid = true;
                }
                else
                {
                    if (!String.IsNullOrEmpty(oldHash))
                    {
                        // Verify the old signature
                        valid = VerifyMacroExpression(expr, identityOption, oldHash, oldSalt);
                    }
                    else
                    {
                        // Special case for Rule expressions (old rule macros signed the inner expression)
                        if (expr.StartsWithCSafe("rule(", true))
                        {
                            var ex = MacroExpression.ParseExpression(expr, true);
                            if ((ex.Type == ExpressionType.MethodCall) && (ex.Name.EqualsCSafe("rule", true)) && (ex.Children != null) && (ex.Children.Count == 2))
                            {
                                string innerExpr = ValidationHelper.GetString((ex.Children[0]).Value, "");
                                innerExpr = RemoveMacroSecurityParams(innerExpr, out identityOption, out oldHash).TrimEnd('#');

                                // Verify the signature of the first child expression 
                                valid = VerifyMacroExpression(innerExpr, identityOption, oldHash, oldSalt);
                            }
                        }
                    }
                }

                if (valid)
                {
                    string expression;
                    if (context.IsOpenExpression)
                    {
                        var originalExpression = context.GetOriginalExpression();

                        string type;
                        expression = MacroProcessor.RemoveMacroBrackets(originalExpression, out type).TrimEnd('#');
                    }
                    else
                    {
                        expression = context.Expression;
                    }

                    // Sign with new salt
                    var signed = AddMacroSecurityParams(expression, identityOption, newSalt, context.Parameters.Decode);
                    return FormatProcessedMacro(context, signed);
                }
            }

            return context.GetWholeMacroExpression();
        }


        /// <summary>
        /// Searches all string macro values in given <see cref="DataSet"/> and transforms them using <paramref name="transformValue" />.
        /// </summary>
        /// <param name="ds"><see cref="DataSet"/> containing macro values to be transformed.</param>
        /// <param name="transformValue">Function transforming old macro value into a new one.</param>
        private static void TransformAllMacroValues(DataSet ds, Func<string, bool, string> transformValue)
        {
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Go through all the tables of the dataset and refresh macro signatures of all string fields
                foreach (DataTable table in ds.Tables)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        if (col.DataType == typeof(string))
                        {
                            bool decode = MacroProcessor.IsXMLColumn(col.ColumnName);
                            foreach (DataRow row in table.Rows)
                            {
                                object value = row[col];
                                if (value != DBNull.Value)
                                {
                                    string oldVal = (string)value;
                                    if (oldVal.Contains("{"))
                                    {
                                        row[col] = transformValue(oldVal, decode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Checks the integrity against given user.
        /// </summary>
        /// <param name="originalExpression">Expression to check</param>
        /// <param name="expectedIdentityOption">Identity option to check</param>
        public static bool CheckMacroIntegrity(string originalExpression, MacroIdentityOption expectedIdentityOption)
        {
            if (!CheckIntegrity || "public".EqualsCSafe(expectedIdentityOption?.UserName, true))
            {
                // Integrity does not matter for public user
                return true;
            }

            // Remove the security parameters
            MacroIdentityOption identityOption;
            string hash;
            string expr = RemoveMacroSecurityParams(originalExpression, out identityOption, out hash);

            // If there is no user, integrity of the macro is ok, but the security check might not pass
            // Integrity does not matter for simple macros
            if (MacroIdentityOption.IsNullOrEmpty(identityOption) || IsSimpleMacro(expr))
            {
                return true;
            }

            // Check the integrity of the data
            return VerifyMacroExpression(expr, identityOption, hash);
        }


        /// <summary>
        /// Checks the security of the accessed object.
        /// </summary>
        /// <param name="context">Evaluation context</param>
        /// <param name="obj">Object to check</param>
        /// <param name="onlyCollection">If true, only collections are checked</param>
        public static bool CheckSecurity(EvaluationContext context, object obj, bool onlyCollection)
        {
            if ((obj != null) && context.CheckSecurity)
            {
                BaseInfo objectToCheck = null;

                // Get the correct type info according to type of the object
                if (!onlyCollection)
                {
                    if ((obj is BaseInfo) || (obj is GeneralizedInfo))
                    {
                        objectToCheck = (BaseInfo)obj;
                    }
                }

                if (obj is IInfoObjectCollection)
                {
                    IInfoObjectCollection collection = (IInfoObjectCollection)obj;

                    objectToCheck = collection.ParentObject ?? collection.Object;
                }

                var result = true;

                // Check the object type
                if ((objectToCheck != null) && objectToCheck.TypeInfo.CheckPermissions)
                {
                    if (context.AllowOnlySimpleMacros)
                    {
                        // If only simple macros are allowed and the security check is requested, it should automatically fail
                        return false;
                    }

                    result = context.IntegrityPassed && context.Resolver.CheckObjectPermissions(objectToCheck, new MacroIdentityOption { IdentityName = context.IdentityName, UserName = context.UserName });
                }

                // Run the handler if defined
                if (OnCheckObjectPermissions != null)
                {
                    var args = new MacroSecurityEventArgs(context, result)
                    {
                        ObjectToCheck = obj,
                        OnlyCollections = onlyCollection
                    };

                    OnCheckObjectPermissions(context.Resolver, args);

                    result = args.Result;
                }

                return result;
            }

            return true;
        }


        /// <summary>
        /// Returns true if the macro is simple and does not need signing.
        /// </summary>
        /// <param name="expression">Expression to check</param>
        /// <returns>Returns true if expression does not contain none of those characters: .[}</returns>
        public static bool IsSimpleMacro(string expression)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                return !RegExpComplexMacro.IsMatch(expression);
            }

            return true;
        }


        /// <summary>
        /// Check macro expression integrity by comparing with <paramref name="hash"/>.
        /// </summary>
        /// <param name="macro">Macro expression.</param>
        /// <param name="identityOption">Identity option by which is the macro expression signed.</param>
        /// <param name="hash">The macro expression is valid only if the computed hash equals to this hash.</param>
        /// <param name="salt">Specify to use custom salt when computing hash of given macro expression.</param>
        internal static bool VerifyMacroExpression(string macro, MacroIdentityOption identityOption, string hash, string salt = null)
        {
            var toHash = GetHashPhrase(macro, identityOption);

            // Check the integrity of the data
            var settings = new HashSettings
            {
                Redirect = false, 
                CustomSalt = salt
            };

            return ValidationHelper.ValidateHash(toHash, hash, settings);
        }


        /// <summary>
        /// Compute hash to ensure macro integrity.
        /// </summary>
        /// <param name="macro">Macro expression.</param>
        /// <param name="identityOption">Identity option by which is the macro expression signed.</param>
        /// <param name="salt">Specify to use custom salt when computing hash.</param>
        internal static string GetMacroHash(string macro, MacroIdentityOption identityOption, string salt = null)
        {
            var toHash = GetHashPhrase(macro, identityOption);

            var settings = new HashSettings
            {
                CustomSalt = salt
            };

            return ValidationHelper.GetHashString(toHash, settings);
        }


        /// <summary>
        /// Gets string containing <paramref name="macro"/> and <paramref name="identityOption"/> for hash computation.
        /// </summary>
        private static string GetHashPhrase(string macro, MacroIdentityOption identityOption)
        {
            var toHash = macro + GetSigningString(identityOption);

            // Standardize line endings (necessary for usage in XmlSerializer - example usage is in workflow module - which removes \r characters)
            toHash = TextHelper.EnsureLineEndings(toHash, "\r\n");

            return toHash;
        }


        internal static string GetSigningString(MacroIdentityOption identityOption)
        {
            if (MacroIdentityOption.IsNullOrEmpty(identityOption))
            {
                return null;
            }

            if (!String.IsNullOrEmpty(identityOption.IdentityName))
            {
                return "(identity)" + identityOption.IdentityName;
            }

            return identityOption.UserName;
        }


        /// <summary>
        /// Returns formatted macro expression
        /// </summary>
        /// <param name="context">context having all required values</param>
        /// <param name="macroExpression">macro expression</param>
        private static string FormatProcessedMacro(MacroProcessingContext context, string macroExpression)
        {
            return $"{{{context.BracketType}{context.MacroType}{macroExpression}{context.MacroType}{context.BracketType}}}";
        }

        #endregion
    }
}