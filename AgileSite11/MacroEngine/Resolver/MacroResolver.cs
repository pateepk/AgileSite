using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class to resolve the macros, provides data to the resolving process.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class MacroResolver : IMacroResolver, INotCopyThreadItem
    {
        #region "Events and delegates"

        /// <summary>
        /// Custom macro event.
        /// </summary>
        public static event EventHandler<MacroEventArgs> OnResolveCustomMacro;

        /// <summary>
        /// Handler to get the specific column value from specified object.
        /// </summary>
        public delegate bool GetObjectValueByNameHandler(object obj, string columnName, ref object result);

        /// <summary>
        /// Handler to get the value from the index-th property of specified object.
        /// </summary>
        public delegate bool GetObjectValueByIndexHandler(object obj, int index, ref object result);

        /// <summary>
        /// Handler to get the object value.
        /// </summary>
        public delegate object OnGetValueEventHandler(string name);

        /// <summary>
        /// Gets value event.
        /// </summary>
        public event OnGetValueEventHandler OnGetValue;

        #endregion


        #region "Constants"

        /// <summary>
        /// Represents unresolved keyword.
        /// </summary>
        public static readonly object UNRESOLVED_RETURN_VALUE = DBNull.Value;

        #endregion


        #region "Variables"

        #region "Data sources"

        // Anonymous datasources
        private List<object> mAnonymousSourceData;

        // Main data source
        private Hashtable mNamedSourceData;

        // Determines whether the registered item is prioritized within the AutoCompletion and Object tree.
        private HashSet<string> mNamedSourceDataPriority;

        // Named source data key collection (does not contain neither prioritized items nor hidden items).
        private HashSet<string> mNamedSourceDataKeys;

        // Hidden named source data key collection
        private HashSet<string> mHiddenSourceDataKeys;

        /// Dynamic parameters used to simulate variables during macro evaluation
        private Hashtable mDynamicParameters;

        /// Object with source data it's properties are searched for data when CheckDataSource is called
        private object mSourceObject;

        // Data source aliases (if the key is not found within standard data sources, than the corresponding alias expression is evaluated instead, if exists).
        private StringSafeDictionary<string> mSourceAliases;

        #endregion

        // Global static resolver
        internal bool mIsGlobalResolver;

        // Get object value handlers
        private static List<GetObjectValueByNameHandler> mObjectValueByNameHandlers;
        private static List<GetObjectValueByIndexHandler> mObjectValueByIndexHandlers;

        private static readonly object objectValueByNameLock = new object();
        private static readonly object objectValueByIndexLock = new object();


        // Parent resolver is used to look for the data if it's not explicitly defined in current resolver
        // All the data available in the parent are available also in children (could be overridden, though)
        private MacroResolver mParentResolver;
        private string mResolverName;
        private int mCurrentRecursion;
        private MacroSettings mSettings;
        private ISet<string> mActiveMacros;

        // Collection of resolvers that were skipped during the processing of the macros with this resolver.
        private List<string> mSkippedResolvers;

        #endregion


        #region "Constructors"
        
        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use GetInstance instead.")]
        public MacroResolver()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="MacroResolver"/>
        /// </summary>
        /// <param name="parentResolver">Optional parent resolver instance.</param>
        internal MacroResolver(MacroResolver parentResolver)
        {
            mParentResolver = parentResolver;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Macro resolver settings.
        /// </summary>
        public MacroSettings Settings
        {
            get
            {
                return mSettings ?? (mSettings = GetDefaultSettingsInstance());
            }
            set
            {
                mSettings = value;
            }
        }


        /// <summary>
        /// Resolver name used for context specific resolving.
        /// </summary>
        /// <example>
        /// The macro with resolver parameter e.g. {% CurrentUser.UserName|(resolver)CustomResolverName %} will be resolved only with the same resolver name specified in this property.
        /// <code>
        /// var resolver = MacroResolver.CreateChild();
        /// resolver.ResolverName = "CustomResolverName";
        /// var result = resolver.ResolveMacros("{% CurrentUser.UserName|(resolver)CustomResolverName %}");
        /// </code>
        /// </example>
        /// <seealso cref="EvaluationContext.ResolverName"/>.
        public string ResolverName
        {
            get
            {
                if (mResolverName == null)
                {
                    if (mParentResolver != null)
                    {
                        return mParentResolver.ResolverName;
                    }
                }
                return mResolverName;
            }
            set
            {
                mResolverName = value;
            }
        }


        /// <summary>
        /// Culture for the resolving
        /// </summary>
        public string Culture
        {
            get
            {
                if ((Settings.Culture == null) && (mParentResolver != null))
                {
                    return mParentResolver.Culture;
                }

                return Settings.Culture;
            }
            set
            {
                Settings.Culture = value;
            }
        }


        /// <summary>
        /// Object with source data - To pass parameters to Custom macro function.
        /// </summary>
        public object SourceObject
        {
            get
            {
                // Get from parent if not present
                if ((mSourceObject == null) && (mParentResolver != null))
                {
                    mSourceObject = mParentResolver.SourceObject;
                }
                return mSourceObject;
            }
            set
            {
                mSourceObject = value;
            }
        }


        /// <summary>
        /// If true, only prioritized properties are shown in Macro components (IntelliSense, MacroSelector, ...).
        /// Influences only visual part of the components, resolver will still be able to resolve all the others as well.
        /// </summary>
        public bool ShowOnlyPrioritized
        {
            get;
            set;
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Current stack of active macros (macros that are just being resolved)
        /// </summary>
        private ISet<string> ActiveMacros
        {
            get
            {
                if ((mParentResolver != null) && !mParentResolver.mIsGlobalResolver)
                {
                    return mParentResolver.ActiveMacros;
                }

                // Use local stack of active macros
                return LockHelper.Ensure(ref mActiveMacros, () => { return new SafeHashSet<string>(StringComparer.InvariantCultureIgnoreCase); }, this);
            }
        }


        /// <summary>
        /// Named source data priority. Determines whether the registered item is prioritized within the AutoCompletion and Object tree.
        /// </summary>
        private HashSet<string> NamedSourceDataPriority
        {
            get
            {
                return mNamedSourceDataPriority ?? (mNamedSourceDataPriority = new HashSet<string>());
            }
        }


        /// <summary>
        /// Named source data key collection.
        /// </summary>
        private HashSet<string> NamedSourceDataKeys
        {
            get
            {
                return mNamedSourceDataKeys ?? (mNamedSourceDataKeys = new HashSet<string>());
            }
        }


        /// <summary>
        /// Hidden named source data key collection.
        /// </summary>
        private HashSet<string> HiddenSourceDataKeys
        {
            get
            {
                return mHiddenSourceDataKeys ?? (mHiddenSourceDataKeys = new HashSet<string>());
            }
        }


        /// <summary>
        /// Dynamic parameters [Name.ToLowerCSafe()] -> [Value], new for each resolver instance. The data is available with macro {%Name%}
        /// </summary>
        private Hashtable DynamicParameters
        {
            get
            {
                return mDynamicParameters ?? (mDynamicParameters = new Hashtable());
            }
        }


        /// <summary>
        /// Macro parameters inherited from previous evaluation.
        /// </summary>
        private EvaluationParameters Parameters
        {
            get;
            set;
        }
        

        /// <summary>
        /// Last error produced while evaluating macros
        /// </summary>
        internal string LastError
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        #region "Security methods"

        /// <summary>
        /// Checks whether given identity option has Read permissions for given object type.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="identityOption">Identity option against which to check</param>
        public bool CheckObjectPermissions(BaseInfo obj, MacroIdentityOption identityOption)
        {
            // Get the user
            IUserInfo user = null;
            if (!MacroIdentityOption.IsNullOrEmpty(identityOption))
            {
                user = identityOption.GetEffectiveUser();
            }
            if (user == null)
            {
                if (!MacroIdentityOption.IsNullOrEmpty(Settings.IdentityOption))
                {
                    user = Settings.IdentityOption.GetEffectiveUser();
                }
            }
            if (user == null)
            {
                // If user not found, use permissions of public user
                user = (IUserInfo)ProviderHelper.GetInfoByName(PredefinedObjectType.USER, "public");
            }

            // Get current site name from context
            var currentSite = CMSActionContext.CurrentSite;
            var currentSiteName = (currentSite != null) ? currentSite.SiteName : String.Empty;

            // Check the permissions
            return obj.CheckPermissions(PermissionsEnum.Read, currentSiteName, user);
        }

        #endregion


        #region "Resolving methods"

        /// <summary>
        /// Resolves the given alias path, applies the path segment to the given format string {0} for level 0.
        /// </summary>
        /// <param name="format">Alias path pattern</param>
        /// <param name="escapeSpecChars">Indicates whether special characters in the automatically added part of path should be escaped for valid SQL LIKE query </param>
        public static string ResolveCurrentPath(string format, bool escapeSpecChars = false)
        {
            return new PathMacroContainer(MacroContext.CurrentResolver).ResolvePath(format, escapeSpecChars);
        }


        /// <summary>
        /// Resolves the context macros.
        /// </summary>
        /// <param name="inputText">Text to resolve</param>
        /// <param name="settings">Macro context and settings</param>
        public static string Resolve(string inputText, MacroSettings settings = null)
        {
            // Ensure that empty settings are used when none provided
            settings = settings ?? GetDefaultSettingsInstance();

            return MacroContext.CurrentResolver.ResolveMacros(inputText, settings);
        }


        /// <summary>
        /// Resolves the macros.
        /// </summary>
        /// <param name="text">Input text with the macros</param>
        /// <param name="evaluationContext">Evaluation context of previous (parent) macro evaluation</param>
        public string ResolveMacros(string text, EvaluationContext evaluationContext)
        {
            var oldParameters = Parameters;
            Parameters = evaluationContext != null ? new EvaluationParameters(evaluationContext) : null;

            var result = ResolveMacros(text, Settings);

            Parameters = oldParameters;

            return result;
        }


        /// <summary>
        /// Resolves all supported macro types in the given text within specified resolving context.
        /// </summary>
        /// <param name="text">Input text with macros to be resolved</param>
        /// <param name="settings">Macro context to be used for resolving (if null, context of the resolver is used)</param>
        public virtual string ResolveMacros(string text, MacroSettings settings = null)
        {
            if (!MacroStaticSettings.AllowMacros || String.IsNullOrEmpty(text))
            {
                return text;
            }

            // Set current context for resolving within correct settings and backup original
            MacroSettings oldSettings = Settings;
            Settings = settings ?? Settings;

            // Resolve macros regular way
            string result = ResolveMacrosInternal(text, 0);

            // Restore the original context
            Settings = oldSettings;

            return result;
        }


        /// <summary>
        /// Resolves the macros within given text.
        /// </summary>
        /// <param name="inputText">Input text</param>
        /// <param name="recursionLevel">Recursion level</param>
        private string ResolveMacrosInternal(string inputText, int recursionLevel)
        {
            mCurrentRecursion = recursionLevel;
            string result = inputText;

            // Quick check for macro start
            if (result.Contains("{"))
            {
                // Resolve macros
                result = MacroProcessor.ProcessMacros(result, null, null, ResolveMacroHandler);
            }

            return result;
        }


        /// <summary>
        /// Returns true if resolving of the macro is allowed. Marks the macro within active macros if the resolving is allowed. Reports a recursion error if not.
        /// </summary>
        /// <param name="macro">Macro to check</param>
        /// <param name="activeMacros">Active macros</param>
        private bool AllowResolve(string macro, ISet<string> activeMacros)
        {
            var recursion = activeMacros.Contains(macro);
            if (recursion)
            {
                MacroDebug.LogMacroFailure(macro, "There was a cyclic recursion detected during processing of the macro. This may be caused by collision of local variable name with the source data (context variable, web part property, etc.) Evaluated macro expression: " + macro, "CHECKRECURSION");
                var error = "There was a cyclic recursion detected during processing of the macro. This may be caused by collision of local variable name with the source data (context variable, web part property, etc.) Evaluated macro expression: " + macro;

                LastError = error;
                MacroDebug.LogMacroFailure(macro, error, "CHECKRECURSION");

                return false;
            }

            activeMacros.Add(macro);

            return true;
        }


        /// <summary>
        /// Callback for data macro match. Resolves the macro.
        /// </summary>
        /// <param name="context">Macro processing context</param>
        private string ResolveMacroHandler(MacroProcessingContext context)
        {
            string macro = context.GetWholeMacroExpression();
            string type = context.MacroType;
            string expression = context.Expression;
            string retval = "";

            // Handle recursion
            var activeMacros = ActiveMacros;
            var macroKey = String.Concat(macro, "|", CMSThread.GetCurrentThreadId());

            if (!AllowResolve(macroKey, activeMacros))
            {
                return "";
            }

            try
            {
                DateTime mEvaluationStart = DateTime.Now;

                // Reserve the log item
                DataRow dr = null;
                if (MacroDebug.DebugCurrentRequest)
                {
                    dr = MacroDebug.ReserveMacroLogItem();
                    MacroDebug.CurrentLogIndent++;
                }

                // Reset console output
                RemoveDynamicParameter("out");

                // Validate macro integrity against original macro text with hash
                string macroType;
                string originalMacro = MacroProcessor.RemoveMacroBrackets(context.GetOriginalExpression(), out macroType);

                if (!MacroSecurityProcessor.CheckMacroIntegrity(originalMacro, null))
                {
                    MacroDebug.LogSecurityCheckFailure(originalMacro, "", null, null);
                    return "";
                }

                // Resolve the macro
                var result = ResolveMacroExpression(expression, false, false, type);
                if (result == null)
                {
                    // Not resolved macro (due to an error).
                    return Settings.KeepUnresolvedMacros ? macro : "";
                }

                if (result.Skipped)
                {
                    if (dr != null)
                    {
                        MacroDebug.SetLogItemData(dr, macro, result, --MacroDebug.CurrentLogIndent, DateTime.Now.Subtract(mEvaluationStart).TotalSeconds);
                    }

                    // Macro skipped because of the |(resolver) parameter
                    return macro;
                }

                bool match = result.Match;

                // If unresolved macros are kept, do not resolve
                bool stop = false;
                if (!match && Settings.KeepUnresolvedMacros)
                {
                    retval = macro;
                    stop = true;
                }
                else if (MacroDebug.DebugCurrentRequest)
                {
                    if ((type != "%") || !MacroDebug.Detailed)
                    {
                        // Log this only if detailed debug mode is off
                        MacroDebug.LogMacroOperation(expression, result, MacroDebug.CurrentLogIndent);
                    }
                }

                if (!stop)
                {
                    retval = FinalizeResult(result.Result, true, result.ContextUsed);
                }

                // Log the operation
                if (dr != null)
                {
                    MacroDebug.SetLogItemData(dr, macro, result, --MacroDebug.CurrentLogIndent, DateTime.Now.Subtract(mEvaluationStart).TotalSeconds);
                }
            }
            finally
            {
                // Mark resolving process as finished
                activeMacros.Remove(macroKey);
            }

            // Return the result
            return retval;
        }


        /// <summary>
        /// Resolves the path macro.
        /// </summary>
        /// <param name="format">Path format</param>
        /// <param name="escapeSpecChars">Indicates whether special characters in the automatically added part of path should be escaped for valid SQL LIKE query </param>
        public string ResolvePath(string format, bool escapeSpecChars = false)
        {
            return new PathMacroContainer(this).ResolvePath(format, escapeSpecChars);
        }


        /// <summary>
        /// Resolves the custom macro.
        /// </summary>
        /// <param name="baseExpression">Base expression (without the parameters)</param>
        /// <param name="fullExpression">Full expression (with the parameters)</param>
        public EvaluationResult ResolveCustomMacro(string baseExpression, string fullExpression)
        {
            EvaluationResult result = new EvaluationResult
            {
                Match = false
            };

            // Resolve custom macro
            if (OnResolveCustomMacro != null)
            {
                MacroEventArgs e = new MacroEventArgs
                {
                    Resolver = this,
                    Expression = baseExpression,
                    FullExpression = fullExpression,
                };

                // Call the event handler
                OnResolveCustomMacro(this, e);

                result.Result = e.Result;
                result.Match = e.Match;
            }

            return result;
        }


        /// <summary>
        /// Resolves the data macro expression (expects expression without {% %} brackets).
        /// Use <see cref="MacroProcessor.RemoveDataMacroBrackets"/> method to remove brackets if needed.
        /// </summary>
        /// <param name="expression">Macro expression without {% %} brackets</param>
        /// <param name="keepObjectsAsResult">If true, when the result is InfoObject it is the result, if false, object is resolved as its displayname (for backward compatibility)</param>
        /// <param name="skipSecurityCheck">If true, security check is not performed</param>
        /// <param name="type">Type of the expression (? or $ or %)</param>
        /// <remarks>
        /// Returns null when macro evaluation throws exception
        /// </remarks>
        public EvaluationResult ResolveMacroExpression(string expression, bool keepObjectsAsResult = false, bool skipSecurityCheck = false, string type = "%")
        {
            var settings = new ResolveExpressionSettings(expression)
            {
                KeepObjectsAsResult = keepObjectsAsResult,
                SkipSecurityCheck = skipSecurityCheck,
                Type = type
            };

            return ResolveMacroExpression(settings);
        }


        /// <summary>
        /// Resolves the data macro expression (expects expression without {% %} brackets).
        /// </summary>
        /// <param name="settings">Settings of the resolving process</param>
        /// <remarks>
        /// Returns null when macro evaluation throws exception
        /// </remarks>
        public EvaluationResult ResolveMacroExpression(ResolveExpressionSettings settings)
        {
            EvaluationResult evalResult = null;

            try
            {
                bool anonymous = false;
                if (settings.Expression.EndsWith("@", StringComparison.Ordinal))
                {
                    // Anonymous expression without signature, remove the flag
                    settings.Expression = settings.Expression.Substring(0, settings.Expression.Length - 1);
                    anonymous = true;
                }

                // Special case for empty localization macro
                if (settings.Expression.Trim() == "")
                {
                    if (settings.Type == "?")
                    {
                        // Special case for empty query string macro
                        // It should output the whole query string
                        settings.Expression = "QueryString";
                    }
                }

                if (settings.Type == "$")
                {
                    // Simple localization macros - transform to data macro
                    settings.Type = "%";
                    settings.Expression = "GetResourceString(\"" + MacroElement.EscapeSpecialChars(settings.Expression).Trim() + "\")";
                }

                // Try to get parsed expression from cache
                MacroExpression root = MacroExpression.ParseExpression(settings.Expression, settings.SupressParsingError);

                EvaluationContext evalContext = new EvaluationContext(this, Parameters, settings.Expression, settings.Type);
                evalContext.CheckSecurity = !settings.SkipSecurityCheck && Settings.CheckSecurity;
                if (anonymous)
                {
                    evalContext.UserName = "public";
                }
                if (settings.Type == "?")
                {
                    if (MacroStaticSettings.AllowQueryMacros)
                    {
                        evalContext.AddPrioritizedInnerSource(QueryHelper.Instance);
                    }
                }

                evalResult = new ExpressionEvaluator(root, evalContext).Evaluate();

                if (evalResult.Skipped)
                {
                    // Register required resolver
                    if (mSkippedResolvers == null)
                    {
                        mSkippedResolvers = new List<string>();
                    }
                    if (!mSkippedResolvers.Contains(evalContext.ResolverName.ToLowerCSafe()))
                    {
                        mSkippedResolvers.Add(evalContext.ResolverName.ToLowerCSafe());
                    }
                    return evalResult;
                }

                // For backward compatibility, if the object is InfoObject, resolve it as his DisplayName column (if exists)
                if (evalResult.Result != null)
                {
                    if (!settings.KeepObjectsAsResult)
                    {
                        evalResult.Result = PostProcessResult(evalResult.Result);
                    }
                }

                // Check whether the security checks went ok
                if (Settings.CheckSecurity && !evalResult.SecurityPassed)
                {
                    MacroDebug.LogSecurityCheckFailure(settings.Expression, evalContext.UserName, evalContext.IdentityName, evalContext.User?.UserName);

                    // Reset the value to null, security did not pass
                    evalResult.Result = null;
                }
                else
                {
                    // Security is ignored
                    evalResult.SecurityPassed = true;
                }
            }
            catch (Exception ex)
            {
                // Log error to event log
                string errMessage = "Error while evaluating expression: " + settings.Expression + "\r\n\r\n" + ex;

                LastError = errMessage;
                MacroDebug.LogMacroFailure(settings.Expression, errMessage, "RESOLVEDATAMACRO");
            }

            return evalResult;
        }


        /// <summary>
        /// Returns true if the macro processing skipped particular resolver name
        /// </summary>
        /// <param name="resolverName">Resolver name</param>
        public bool SkippedResolver(string resolverName)
        {
            if (mSkippedResolvers == null)
            {
                return false;
            }

            if (mSkippedResolvers.Contains(resolverName.ToLowerCSafe()))
            {
                return true;
            }

            if (mParentResolver != null)
            {
                return mParentResolver.SkippedResolver(resolverName);
            }

            return false;
        }

        #endregion


        #region "Get data methods"

        /// <summary>
        /// Checks all the data sources for the value and returns true if the data item is registered.
        /// </summary>
        /// <param name="itemName">Name of the data item</param>
        public virtual bool IsDataItemAvailable(string itemName)
        {
            // Disable license error when macro evaluation tries to load objects that are not included in current license
            using (new CMSActionContext { EmptyDataForInvalidLicense = true })
            {
                EvaluationResult result = CheckDataSources(itemName, new EvaluationContext(this, itemName));
                return result.Match;
            }
        }


        /// <summary>
        /// Checks all the data sources for the value. Returns true if given data member was found within supported data sources.
        /// </summary>
        /// <param name="expression">Data member to look for</param>
        /// <param name="context">Evaluation context</param>
        public virtual EvaluationResult CheckDataSources(string expression, EvaluationContext context)
        {
            EvaluationResult result = CheckDataSourcesInternal(expression, context);

            // Handle MacroField type of results
            if (result.Match && (result.Result is MacroField))
            {
                MacroField field = (MacroField)result.Result;
                result.Result = field.GetValue(context);
                result.Match = field.IsFieldAvailable(context);
            }

            return result;
        }


        /// <summary>
        /// Checks all the data sources for the value (at first, it checks if it's not special value).
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="context">Evaluation context</param>
        private EvaluationResult CheckDataSourcesInternal(string expression, EvaluationContext context)
        {
            string exprToLower = expression.ToLowerCSafe();

            // Dynamic parameters (always prioritized, those are variables declared within the macro)
            EvaluationResult result = GetDynamicParameter(exprToLower);
            if (result.Match)
            {
                return result;
            }

            // Check context prioritized sources (prioritized sources on the individual macro level - used to prioritize QueryString for query macros depending on type).
            if (context != null)
            {
                foreach (var item in context.GetPrioritizedInnerSources())
                {
                    result = GetObjectValue(item, exprToLower, context);
                    if (result.Match)
                    {
                        return result;
                    }
                }
            }

            // Check value from handler
            if (OnGetValue != null)
            {
                object value = OnGetValue(exprToLower);
                if (value != null)
                {
                    return new EvaluationResult
                    {
                        Result = value,
                        Match = true
                    };
                }
            }

            // Check named source data (standard data source)
            if ((mNamedSourceData != null) && mNamedSourceData.ContainsKey(exprToLower))
            {
                return new EvaluationResult
                {
                    Result = mNamedSourceData[exprToLower],
                    Match = true
                };
            }

            // Check source data
            if (mAnonymousSourceData != null)
            {
                // Try to find the value in all data rows
                foreach (var obj in mAnonymousSourceData)
                {
                    // Get the object value
                    result = GetObjectValue(obj, exprToLower, context);
                    if (result.Match)
                    {
                        return result;
                    }
                }
            }

            // Check data from parent
            if (mParentResolver != null)
            {
                result = mParentResolver.CheckDataSourcesInternal(expression, context);
                if (result.Match)
                {
                    return result;
                }
            }

            // Try to resolve aliases if the data source was not found in the base data sources
            string alias = GetSourceAlias(exprToLower);
            if (alias != null)
            {
                result = ResolveMacroExpression(alias, true, true);
                if (result.Match)
                {
                    return result;
                }
            }

            // Check static root properties
            var rootProperties = typeof(IMacroRoot).GetStaticProperties<object>();
            if (rootProperties != null)
            {
                var value = rootProperties[expression];
                if (value != null)
                {
                    result.Match = true;
                    result.Result = value.Value;
                    return result;
                }
            }

            // Check the values of default CMSContext
            if (Settings.AllowContextMacros)
            {
                object contextValue = ModuleManager.GetContextProperty("CMSContext", exprToLower);
                if (contextValue != null)
                {
                    return new EvaluationResult
                    {
                        Result = contextValue,
                        Match = true
                    };
                }
            }

            // Not found
            return new EvaluationResult
            {
                Match = false
            };
        }


        /// <summary>
        /// Gets the object value at given index (this is called when indexer [(int)] is used in the expression).
        /// </summary>
        /// <param name="objectToCheck">Source object to get the index-th value from</param>
        /// <param name="index">Index of the item to get</param>
        /// <param name="context">Evaluation context</param>
        public virtual EvaluationResult GetObjectValue(object objectToCheck, int index, EvaluationContext context)
        {
            var result = new EvaluationResult
            {
                Match = false,
            };

            if (objectToCheck == null)
            {
                return result;
            }

            object obj = objectToCheck;
            if (obj is MacroField)
            {
                obj = ((MacroField)obj).GetValue(context);
            }

            // Get by index is always match
            result.Match = true;

            var drc = obj as DataRowContainer;
            if (drc != null)
            {
                if (drc.DataRow.Table.Columns.Count > index)
                {
                    result.Result = drc.DataRow[index];
                }
            }
            else
            {
                var dtc = obj as DataTableContainer;
                if (dtc != null)
                {
                    if (dtc.DataTable.Rows.Count > index)
                    {
                        result.Result = dtc.DataTable.Rows[index];
                    }
                }
                else
                {
                    var dsc = obj as DataSetContainer;
                    if (dsc != null)
                    {
                        if (dsc.DataSet.Tables.Count > index)
                        {
                            result.Result = dsc.DataSet.Tables[index];
                        }
                    }
                    else
                    {
                        var dr = obj as DataRow;
                        if (dr != null)
                        {
                            if (dr.Table.Columns.Count > index)
                            {
                                result.Result = dr[index];
                            }
                        }
                        else
                        {
                            var drv = obj as DataRowView;
                            if (drv != null)
                            {
                                if (drv.Row.Table.Columns.Count > index)
                                {
                                    result.Result = drv[index];
                                }
                            }
                            else
                            {
                                // CharAt function (returns character at given position of the string)
                                var s = obj as string;
                                if (s != null)
                                {
                                    if (s.Length > index)
                                    {
                                        result.Result = s[index];
                                    }
                                }
                                else
                                {
                                    // Collection indexable by int
                                    var indexable = obj as IIndexable;
                                    if (indexable != null)
                                    {
                                        result.Result = indexable[index];
                                    }
                                    else
                                    {
                                        // List indexable by int
                                        var list = obj as IList;
                                        if (list != null)
                                        {
                                            if (list.Count > index)
                                            {
                                                result.Result = list[index];
                                            }
                                        }
                                        else
                                        {
                                            // Enumerable
                                            var en = obj as IEnumerable;
                                            if (en != null)
                                            {
                                                // Index must be zero or positive
                                                if (index < 0)
                                                {
                                                    result.Result = null;
                                                }

                                                int i = 0;

                                                // Enumerate the items until the item is found
                                                foreach (var item in en)
                                                {
                                                    if (i++ == index)
                                                    {
                                                        // Matched index
                                                        result.Result = item;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ((result.Result == null) && (mObjectValueByIndexHandlers != null))
            {
                // Go through extension methods for getting object value
                foreach (var item in mObjectValueByIndexHandlers)
                {
                    object o = null;
                    if (item.Invoke(obj, index, ref o))
                    {
                        result.Result = o;
                        break;
                    }
                }
            }

            EnsureVirtualResult(result, obj);

            // Finalize the value
            FinalizeObjectValue(obj, result);

            return result;
        }


        /// <summary>
        /// Gets the object value of specified name.
        /// </summary>
        /// <param name="objectToCheck">Source object to get the index-th value from</param>
        /// <param name="columnName">Name of the value to get</param>
        /// <param name="context">Evaluation context</param>
        public virtual EvaluationResult GetObjectValue(object objectToCheck, string columnName, EvaluationContext context)
        {
            var result = new EvaluationResult
            {
                Match = false,
            };

            if (objectToCheck == null)
            {
                return result;
            }

            // Do not allow to access Sensitive columns
            if (IsSensitiveColumn(objectToCheck, columnName))
            {
                return result;
            }

            object obj = objectToCheck;

            // Macro field
            var macroField = obj as MacroField;
            if (macroField != null)
            {
                obj = macroField.GetValue(context);
            }

            // DataRow source
            var dr = obj as DataRow;
            if (dr != null)
            {
                // Change locale for cultures that uses different set of upper / lower case characters.
                // Fixes case insensitive comparison to work with English characters.       
                if (CMSString.IsInvariantCompareCulture(dr.Table.Locale))
                {
                    dr.Table.Locale = CultureInfo.InvariantCulture;
                }
                
                if (dr.Table.Columns.Contains(columnName))
                {
                    result.Result = dr[columnName];
                    result.Match = true;
                }
            }
            else
            {
                // DataRowView source
                var drv = obj as DataRowView;
                if (drv != null)
                {
                    // Change locale for cultures that uses different set of upper / lower case characters.
                    // Fixes case insensitive comparison to work with English characters.                   
                    if (CMSString.IsInvariantCompareCulture(drv.DataView.Table.Locale))
                    {
                        drv.DataView.Table.Locale = CultureInfo.InvariantCulture;
                    }

                    if (drv.DataView.Table.Columns.Contains(columnName))
                    {
                        result.Result = drv[columnName];
                        result.Match = true;
                    }
                }
                else
                {
                    // Hierarchical object
                    var virthier = obj as IVirtualHierarchicalObject;
                    if (virthier != null)
                    {
                        object o;
                        result.Match = virthier.TryGetProperty(columnName, out o, context.Resolver.Settings.VirtualMode);
                        result.Result = o;
                    }
                    else
                    {
                        var hier = obj as IHierarchicalObject;
                        if (hier != null)
                        {
                            object o;
                            result.Match = TryGetObjectProperty(hier, columnName, out o);
                            result.Result = o;
                        }
                        else
                        {
                            // Data container source
                            var dc = obj as IDataContainer;
                            if (dc != null)
                            {
                                object o;
                                result.Match = dc.TryGetValue(columnName, out o);
                                result.Result = o;
                            }
                            else
                            {
                                // Simple data container source
                                var sc = obj as ISimpleDataContainer;
                                if (sc != null)
                                {
                                    result.Result = sc.GetValue(columnName);
                                    result.Match = true;
                                }
                                else
                                {
                                    // String
                                    var s = obj as string;
                                    if (s != null)
                                    {
                                        if (columnName.ToLowerCSafe() == "length")
                                        {
                                            result.Result = s.Length;
                                            result.Match = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!result.Match)
            {
                var named = obj as INameIndexable;
                if (named != null)
                {
                    // Indexable object
                    result.Result = named[columnName];
                    if (result.Result != null)
                    {
                        result.Match = true;
                    }
                }
            }

            if (!result.Match && (mObjectValueByNameHandlers != null))
            {
                // Go through extension methods for getting object value
                foreach (var item in mObjectValueByNameHandlers)
                {
                    object o = null;
                    if (item.Invoke(obj, columnName, ref o))
                    {
                        result.Result = o;
                        result.Match = true;
                        break;
                    }
                }
            }

            if (!result.Match)
            {
                // Go through static extension methods
                var field = MacroFieldContainer.GetFieldForObject(obj, columnName);
                if ((field != null) && field.IsFieldAvailable(context))
                {
                    result.Result = field.GetValue(context);
                    result.Match = true;
                }
            }

            EnsureVirtualResult(result, obj);

            // Finalize the value
            FinalizeObjectValue(obj, result);

            return result;
        }


        /// <summary>
        /// Verifies whether columnName is a sensitive column in parentObject.
        /// </summary>
        /// <param name="parentObject">Parent object</param>
        /// <param name="columnName">Name of the column to verify</param>
        private bool IsSensitiveColumn(object parentObject, string columnName)
        {
            // Check the access of the sensitive columns
            if (parentObject is BaseInfo)
            {
                var parentInfo = (BaseInfo)parentObject;
                if (parentInfo.TypeInfo.SensitiveColumns != null)
                {
                    return (parentInfo.TypeInfo.SensitiveColumns.Any(col => col.EqualsCSafe(columnName, true)));
                }
            }

            return false;
        }


        /// <summary>
        /// Ensures the virtual result in case regular result is not available
        /// </summary>
        /// <param name="result">Current result</param>
        /// <param name="obj">Source object</param>
        private void EnsureVirtualResult(EvaluationResult result, object obj)
        {
            // Ensure empty object in case of virtual mode for virtual collection
            if ((result.Result == null) && Settings.VirtualMode)
            {
                var vc = obj as IVirtualTypedCollection;
                if (vc != null)
                {
                    result.Result = vc.GetEmptyObject();
                    result.Match = true;
                }
            }
        }


        /// <summary>
        /// Gets the property of particular hierarchical object
        /// </summary>
        /// <param name="obj">Object to get the value from</param>
        /// <param name="columnName">Name of the column</param>
        /// <param name="result">Returning property value</param>
        private bool TryGetObjectProperty(IHierarchicalObject obj, string columnName, out object result)
        {
            // Handle the virtual mode
            if (Settings.VirtualMode)
            {
                var virt = obj as IVirtualHierarchicalObject;
                if (virt != null)
                {
                    // Advanced hierarchical object in virtual mode
                    return virt.TryGetProperty(columnName, out result, true);
                }
            }

            // Hierarchical object source
            return obj.TryGetProperty(columnName, out result);
        }

        #endregion


        #region "Post-processing methods"

        /// <summary>
        /// If the object is InfoObject, returns its DisplayName column (if exists).
        /// </summary>
        /// <param name="result">Result to process</param>
        /// <param name="allowEnumeration">If true, the process allows enumeration of the result</param>
        protected object PostProcessResult(object result, bool allowEnumeration = true)
        {
            object retval = result;
            if (result != null)
            {
                // Get string result provided by the object
                var o = result as IMacroObject;
                if (o != null)
                {
                    retval = o.ToMacroString();
                }
                else if (allowEnumeration && (result is IEnumerable) && !(result is string))
                {
                    // Enumerate the result in case of enumerable
                    IEnumerator enumerator = ((IEnumerable)result).GetEnumerator();
                    StringBuilder stringVal = new StringBuilder();

                    while (enumerator.MoveNext())
                    {
                        // Do not allow recursive enumeration to prevent flattening of too large repositories such as {% GlobalObjects %}
                        var item = PostProcessResult(enumerator.Current, false);

                        stringVal.AppendLine(ValidationHelper.GetString(item, ""));
                    }

                    retval = stringVal.ToString();

                    // Trim empty line from the end
                    retval = ((string)retval).TrimEnd(Environment.NewLine.ToCharArray());
                }
            }

            return retval;
        }


        /// <summary>
        /// Tracks the dependencies for the given object
        /// </summary>
        /// <param name="obj">Object to track</param>
        private void TrackDependencies(object obj)
        {
            // Register the cache dependencies if tracked
            if (Settings.TrackCacheDependencies &&
                ((obj is BaseInfo) || (obj is GeneralizedInfo)))
            {
                var info = obj as GeneralizedInfo;

                GeneralizedInfo infoObj = info ?? ((BaseInfo)obj).Generalized;

                var dependencies = infoObj.GetCacheDependencies();

                Settings.AddCacheDependencies(dependencies);
            }
        }


        /// <summary>
        /// Finalizes the object value after get
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="result">Current result</param>
        private void FinalizeObjectValue(object obj, EvaluationResult result)
        {
            if ((result == null) || (result.Result == null))
            {
                return;
            }

            if (result.Result is ObjectProperty)
            {
                // Transform object property to its value and keep the object
                ObjectProperty op = (ObjectProperty)result.Result;

                obj = op.Object;
                result.Result = op.Value;
            }

            // Track the dependencies
            TrackDependencies(obj);
        }


        /// <summary>
        /// Finalizes the result (makes the result string).
        /// </summary>
        /// <param name="result">Result object</param>
        /// <param name="processChilden">If true, the children get finalized as well</param>
        /// <param name="context">Evaluation context</param>
        protected string FinalizeResult(object result, bool processChilden, EvaluationContext context)
        {
            if (processChilden && (result is IEnumerable) && !(result is string))
            {
                // If enumerable, process as list (finalize each value and concatenate)
                StringBuilder sb = new StringBuilder();
                IEnumerable en = (IEnumerable)result;
                foreach (object res in en)
                {
                    string nextRes = FinalizeResult(res, false, context);
                    sb.Append(nextRes);
                }

                return sb.ToString();
            }

            string stringResult = ValidationHelper.GetString(result, "", context.Culture);
            
            // Resolve macro in result
            if (context.Recursive && MacroProcessor.ContainsMacro(stringResult) && (mCurrentRecursion < Settings.MaxRecursionlevel))
            {
                int oldRecursion = mCurrentRecursion;
                var oldSettings = Settings;
                Settings = Settings.Clone();
                Settings.Culture = context.Culture;
                Settings.EvaluationTimeout = context.EvaluationTimeout;
                Settings.ExternalTimeoutChecker = context.CheckForTimeout;

                // Resolve macros
                stringResult = ResolveMacrosInternal(stringResult, mCurrentRecursion + 1);

                Settings = oldSettings;

                mCurrentRecursion = oldRecursion;
            }

            // Handle default value
            if ((stringResult == "") && !String.IsNullOrEmpty(context.DefaultValue))
            {
                stringResult = context.DefaultValue;
            }

            // Localize result
            stringResult = ResHelper.LocalizeString(stringResult, Settings.Culture);

            // Avoid injection if configured
            if (context.HandleSQLInjection && (result != null))
            {
                stringResult = SqlHelper.GetSafeQueryString(stringResult, false);
            }

            // HTML encode
            if (context.Encode)
            {
                stringResult = HTMLHelper.HTMLEncode(stringResult);
            }

            return stringResult;
        }

        #endregion


        #region "Named data sources methods"

        /// <summary>
        /// Registers a namespace within this macro resolver
        /// </summary>
        /// <param name="ns">Namespace object</param>
        /// <param name="name">Namespace name. If not set, the name is automatically taken from the type name of the namespace object with removed Namespace suffix.</param>
        /// <param name="allowAnonymous">If true, the namespace members are registered also as anonymous</param>
        /// <param name="hidden">If true, the namespace is hidden and doesn't show up in the Intellisense</param>
        public void RegisterNamespace(IMacroNamespace ns, string name = null, bool allowAnonymous = false, bool hidden = false)
        {
            // Register implicit sources (always included and accessible directly)
            if (allowAnonymous)
            {
                AddAnonymousSourceData(ns);
            }

            name = name ?? GetDefaultNamespaceName(ns);

            // Register default namespaces
            if (hidden)
            {
                SetHiddenNamedSourceData(name, ns);
            }
            else
            {
                SetNamedSourceData(name, ns, false);
            }
        }


        /// <summary>
        /// Gets the default namespace name
        /// </summary>
        /// <param name="ns">Namespace object</param>
        private static string GetDefaultNamespaceName(IMacroNamespace ns)
        {
            var name = ns.GetType().Name;

            if (name.EndsWithCSafe("Namespace", true))
            {
                name = name.Substring(0, name.Length - 9);
            }

            return name;
        }


        /// <summary>
        /// Returns set of registered hidden named data sources (including all hidden named data sources registered in the parent(s) of the resolver, it's an union of keys through the whole hierarchy).
        /// Includes only hidden sources (which are tracked only in DevelopmentMode)
        /// </summary>
        public HashSet<string> GetHiddenRegisteredDataNames()
        {
            var result = new HashSet<string>();

            if (mHiddenSourceDataKeys != null)
            {
                foreach (var item in mHiddenSourceDataKeys)
                {
                    result.Add(item);
                }
            }

            // Copy source data keys from parent resolver
            if (mParentResolver != null)
            {
                var parentSet = mParentResolver.GetHiddenRegisteredDataNames();

                foreach (var item in parentSet)
                {
                    result.Add(item);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns set of registered named data sources (including all named data sources registered in the parent(s) of the resolver, it's an union of keys through the whole hierarchy).
        /// DOES NOT include prioritized keys (call GetPrioritizedDataNames to get those).
        /// </summary>
        public HashSet<string> GetRegisteredDataNames()
        {
            var result = new HashSet<string>();

            if (NamedSourceDataKeys != null)
            {
                foreach (var item in NamedSourceDataKeys)
                {
                    result.Add(item);
                }
            }

            // Copy source data keys from parent resolver
            if (mParentResolver != null)
            {
                var parentSet = mParentResolver.GetRegisteredDataNames();

                foreach (var item in parentSet)
                {
                    result.Add(item);
                }
            }

            return result;
        }


        /// <summary>
        /// Returns set of prioritized named data sources (including all prioritized named data sources registered in the parent(s) of the resolver, it's an union of keys through the whole hierarchy).
        /// DOES NOT include non-prioritized keys (call GetRegisteredDataNames to get those).
        /// </summary>
        public HashSet<string> GetPrioritizedDataNames()
        {
            var result = new HashSet<string>();

            if (NamedSourceDataPriority != null)
            {
                foreach (var item in NamedSourceDataPriority)
                {
                    result.Add(item);
                }
            }

            // Copy source data keys from parent resolver
            if (mParentResolver != null)
            {
                var parentSet = mParentResolver.GetPrioritizedDataNames();

                foreach (var item in parentSet)
                {
                    result.Add(item);
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the named source data by the given selector.
        /// </summary>
        /// <param name="selector">Selector</param>
        public object GetNamedSourceData(string selector)
        {
            if (selector == null)
            {
                return null;
            }

            object result = null;
            if (mNamedSourceData != null)
            {
                result = mNamedSourceData[selector.ToLowerCSafe()];
            }
            if ((result == null) && (mParentResolver != null))
            {
                result = mParentResolver.GetNamedSourceData(selector);
            }
            return result;
        }


        /// <summary>
        /// Returns true if given source data is known (can be null, but the property has to be registered).
        /// </summary>
        /// <param name="selector">Selector</param>
        public bool NamedSourceDataExists(string selector)
        {
            if (mNamedSourceData != null)
            {
                if (mNamedSourceData.ContainsKey(selector.ToLowerCSafe()))
                {
                    return true;
                }
            }
            if (mParentResolver != null)
            {
                return mParentResolver.NamedSourceDataExists(selector);
            }
            return false;
        }


        /// <summary>
        /// Registers given field as the named source data.
        /// </summary>
        /// <param name="field">Field to register</param>
        /// <param name="isPrioritized">If true, this item will appear at the top of AutoCompletion and ObjectTree</param>
        public void SetNamedSourceData(MacroField field, bool isPrioritized = true)
        {
            SetNamedSourceDataInternal(field.Name, field, isPrioritized, false);
        }


        /// <summary>
        /// Registers the named source data. The data can be accessed with macro {%Selector.ColumnName%}. Selector has to be in identifier format.
        /// </summary>
        /// <param name="selector">Data selector</param>
        /// <param name="data">Data</param>
        /// <param name="isPrioritized">If true, this item will appear at the top of AutoCompletion and ObjectTree</param>
        public void SetNamedSourceData(string selector, object data, bool isPrioritized = true)
        {
            SetNamedSourceDataInternal(selector, data, isPrioritized, false);
        }


        /// <summary>
        /// Registers the named source data with late binding. Callback is called whenever the value is needed by MacroResolver.
        /// </summary>
        /// <param name="selector">Data selector</param>
        /// <param name="callback">Callback function which is called to get the data</param>
        /// <param name="isPrioritized">If true, this item will appear at the top of AutoCompletion and ObjectTree</param>
        public void SetNamedSourceDataCallback(string selector, Func<EvaluationContext, object> callback, bool isPrioritized = true)
        {
            SetNamedSourceData(new MacroField(selector, callback), isPrioritized);
        }


        /// <summary>
        /// Registers the named source data with late binding. Callback is called whenever the value is needed by MacroResolver.
        /// </summary>
        /// <param name="selector">Data selector</param>
        /// <param name="data">Data</param>
        public void SetHiddenNamedSourceData(string selector, object data)
        {
            SetNamedSourceDataInternal(selector, data, false, true);
        }


        /// <summary>
        /// Registers the named source data with late binding. Callback is called whenever the value is needed by MacroResolver.
        /// </summary>
        /// <param name="selector">Data selector</param>
        /// <param name="callback">Callback function which is called to get the data</param>
        public void SetHiddenNamedSourceData(string selector, Func<EvaluationContext, object> callback)
        {
            SetNamedSourceDataInternal(selector, new MacroField(selector, callback), false, true);
        }


        /// <summary>
        /// Registers the named source data. The data can be accessed with macro {%Selector.ColumnName%}. Selector has to be in identifier format.
        /// </summary>
        /// <param name="selector">Data selector</param>
        /// <param name="data">Data</param>
        /// <param name="isPrioritized">If true, this item will appear at the top of AutoCompletion and ObjectTree</param>
        /// <param name="hidden">If true, the source will be hidden (will be resolved in the resolving process, but won't be available in IntelliSense or any other macro component)</param>
        private void SetNamedSourceDataInternal(string selector, object data, bool isPrioritized, bool hidden)
        {
            if (selector == null)
            {
                return;
            }

            if (mNamedSourceData == null)
            {
                mNamedSourceData = new Hashtable();
            }

            // Set the data
            mNamedSourceData[selector.ToLowerCSafe()] = data;

            // Save the name according to priority
            if (!hidden)
            {
                if (isPrioritized)
                {
                    NamedSourceDataPriority.Add(selector);
                }
                else
                {
                    // Register the original name (not to lower, for autocompletion purposes)
                    NamedSourceDataKeys.Add(selector);
                }
            }
            else
            {
                // Register hidden values in development mode
                if (SystemContext.DevelopmentMode)
                {
                    HiddenSourceDataKeys.Add(selector);
                }
            }
        }


        /// <summary>
        /// Prioritizes a property (equivalent to using SetNamedSourceData with last parameter true).
        /// </summary>
        /// <param name="propertyName">Name of the property to prioritize</param>
        public void PrioritizeProperty(string propertyName)
        {
            NamedSourceDataPriority.Add(propertyName);

            if (NamedSourceDataKeys.Contains(propertyName))
            {
                NamedSourceDataKeys.Remove(propertyName);
            }
        }


        /// <summary>
        /// Sets hashtable data source to the resolver. Keys of the table are considered names of the data.
        /// </summary>
        /// <param name="data">Data to register to the resolver</param>
        public void SetNamedSourceData(params Hashtable[] data)
        {
            foreach (var table in data)
            {
                foreach (string key in table.Keys)
                {
                    SetNamedSourceData(key, table[key], false);
                }
            }
        }


        /// <summary>
        /// Sets given name-value pairs to the resolver.
        /// </summary>
        /// <param name="data">Data to register to the resolver</param>
        /// <param name="isPrioritized">If true, this item will appear at the top of AutoCompletion and ObjectTree</param>
        public void SetNamedSourceData(IDictionary<string, object> data, bool isPrioritized = true)
        {
            if (data == null)
            {
                return;
            }

            foreach (var pair in data)
            {
                SetNamedSourceData(pair.Key.TrimStart('@'), pair.Value, isPrioritized);
            }
        }

        #endregion


        #region "Dynamic parameters source methods"

        /// <summary>
        /// Removes specified dynamic parameter from the resolver.
        /// </summary>
        /// <param name="name">Parameter name</param>
        internal virtual void RemoveDynamicParameter(string name)
        {
            DynamicParameters.Remove(name.ToLowerCSafe());
        }


        /// <summary>
        /// Adds (sets) the new dynamic parameter to the resolver. The data can be accessed with macro {%name%}.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Value</param>
        internal virtual void SetDynamicParameter(string name, object value)
        {
            DynamicParameters[name.ToLowerCSafe()] = value;
        }


        /// <summary>
        /// Returns the dynamic parameter of specified name.
        /// </summary>
        /// <param name="name">Parameter name</param>
        internal EvaluationResult GetDynamicParameter(string name)
        {
            string nameToLower = name.ToLowerCSafe();
            if (DynamicParameters.ContainsKey(nameToLower))
            {
                return new EvaluationResult
                {
                    Result = DynamicParameters[nameToLower],
                    Match = true
                };
            }

            if (mParentResolver != null)
            {
                return mParentResolver.GetDynamicParameter(name);
            }

            // Dynamic parameter not found
            return new EvaluationResult
            {
                Result = null,
                Match = false
            };
        }

        #endregion


        #region "Anonymous sources methods"

        /// <summary>
        /// Adds new anonymous data source of general type to the resolver.
        /// </summary>
        /// <param name="data">Data to register to the resolver</param>
        public void AddAnonymousSourceData(params object[] data)
        {
            if (mAnonymousSourceData == null)
            {
                mAnonymousSourceData = new List<object>();
            }
            mAnonymousSourceData.AddRange(data);
        }


        /// <summary>
        /// Sets (replaces all existing anonymous sources) specified data source (without name) of general type to the resolver.
        /// </summary>
        /// <param name="data">Data to register to the resolver</param>
        /// <remarks>This method must not be used for <see cref="MacroContext.GlobalResolver"/> instance. Use <see cref="AddAnonymousSourceData"/> instead.</remarks>
        public void SetAnonymousSourceData(params object[] data)
        {
            mAnonymousSourceData = new List<object>(data);
        }


        /// <summary>
        /// Returns list of all aggregated anonymous sources (from the whole resolver hierarchy).
        /// </summary>
        public List<object> GetAnonymousSources()
        {
            if (mAnonymousSourceData == null)
            {
                mAnonymousSourceData = new List<object>();
            }

            List<object> final = new List<object>();
            final.AddRange(mAnonymousSourceData);

            // Copy source data keys from parent resolver
            if (mParentResolver != null)
            {
                final.AddRange(mParentResolver.GetAnonymousSources());
            }

            return final;
        }

        #endregion


        #region "Source aliases methods"

        /// <summary>
        /// Adds alternative source expression.
        /// </summary>
        /// <param name="source">Source for which the alternative expression should be created</param>
        /// <param name="expression">Expression which will be evaluated if the source is not found in base data sources</param>
        public virtual void AddSourceAlias(string source, string expression)
        {
            if (mSourceAliases == null)
            {
                mSourceAliases = new StringSafeDictionary<string>();
            }

            string toLower = source.ToLowerCSafe();
            if (!mSourceAliases.ContainsKey(toLower))
            {
                mSourceAliases.Add(toLower, expression);
            }
        }


        /// <summary>
        /// Gets the named source data by the given selector.
        /// </summary>
        /// <param name="selector">Selector</param>
        public virtual string GetSourceAlias(string selector)
        {
            string toLower = selector.ToLowerCSafe();
            string result = null;
            if ((mSourceAliases != null) && (mSourceAliases.ContainsKey(toLower)))
            {
                result = mSourceAliases[toLower];
            }
            if ((result == null) && (mParentResolver != null))
            {
                result = mParentResolver.GetSourceAlias(toLower);
            }
            return result;
        }

        #endregion


        #region "Object value handler methods"

        /// <summary>
        /// Registers GetObjectValueByName handler(s) to extend possibilities of MacroEngine to dig values from particular object types.
        /// These handlers are internally used when a property is requested out of a given object. First the default object types are checked, then all the handlers (the order of the handler evaluation is not guaranteed).
        /// </summary>
        /// <param name="handlers">Handler(s) to attach</param>
        public static void RegisterObjectValueByNameHandler(params GetObjectValueByNameHandler[] handlers)
        {
            lock (objectValueByNameLock)
            {
                // Create new empty or clone already existing collection of handlers
                List<GetObjectValueByNameHandler> collectionInstance =
                    (mObjectValueByNameHandlers == null)
                    ? new List<GetObjectValueByNameHandler>()
                    : new List<GetObjectValueByNameHandler>(mObjectValueByNameHandlers);

                collectionInstance.AddRange(handlers);

                mObjectValueByNameHandlers = collectionInstance;
            }
        }



        /// <summary>
        /// Registers GetObjectValueByName handler(s) to extend possibilities of MacroEngine to dig values from particular object types.
        /// These handlers are internally used when a property is requested out of a given object. First the default object types are checked, then all the handlers (the order of the handler evaluation is not guaranteed).
        /// </summary>
        /// <param name="handlers">Handler(s) to attach</param>
        public static void RegisterObjectValueByIndexHandler(params GetObjectValueByIndexHandler[] handlers)
        {
            lock (objectValueByIndexLock)
            {
                // Create new empty or clone already existing collection of handlers
                List<GetObjectValueByIndexHandler> collectionInstance =
                    (mObjectValueByIndexHandlers == null)
                    ? new List<GetObjectValueByIndexHandler>()
                    : new List<GetObjectValueByIndexHandler>(mObjectValueByIndexHandlers);

                    collectionInstance.AddRange(handlers);

                    mObjectValueByIndexHandlers = collectionInstance;
            }
        }

        #endregion


        #region "Resolver creation methods"
        
        /// <summary>
        /// Creates new instance of <see cref="MacroResolver"/>.
        /// </summary>
        /// <param name="inheritFromGlobalResolver">If true, the resolver will be a child of the GlobalResolver (recommended if you need full feature resolver).</param>
        public static MacroResolver GetInstance(bool inheritFromGlobalResolver = true)
        {
            return new MacroResolver(inheritFromGlobalResolver ? MacroContext.GlobalResolver : null);
        }


        /// <summary>
        /// Creates new instance of <see cref="MacroResolver"/> as a child resolver that loads the data from the parent resolver by default.
        /// </summary>
        /// <remarks><see cref="Settings"/> are cloned from parent resolver.</remarks>
        public virtual MacroResolver CreateChild()
        {
            MacroResolver newResolver = GetInstance();

            // Clone the settings
            newResolver.Settings = Settings.Clone();

            // Set the parent
            newResolver.mParentResolver = this;

            return newResolver;
        }


        /// <summary>
        /// Creates default settings object
        /// </summary>
        /// <returns>New empty settings object with default values</returns>
        private static MacroSettings GetDefaultSettingsInstance()
        {
            return new MacroSettings
            {
                AvoidInjection = false
            };
        }

        #endregion

        #endregion


        #region "Encapsulation methods"

        /// <summary>
        /// Encapsulates object if needed (DataSet, DataRow, DataTable to their Container equivalent).
        /// </summary>
        /// <param name="obj">Object to be encapsulated</param>
        /// <param name="context">Evaluation context</param>
        public static object EncapsulateObject(object obj, EvaluationContext context = null)
        {
            if (obj != null)
            {
                // Data set
                var ds = obj as DataSet;
                if (ds != null)
                {
                    return new DataSetContainer(ds);
                }
                else
                {
                    // Data table
                    var dt = obj as DataTable;
                    if (dt != null)
                    {
                        return new DataTableContainer(dt);
                    }
                    else
                    {
                        // Data row
                        var dr = obj as DataRow;
                        if (dr != null)
                        {
                            return new DataRowContainer(dr);
                        }
                        else if (obj is TimeSpan)
                        {
                            return new TimeSpanContainer((TimeSpan)obj);
                        }
                        else if (obj is DateTime)
                        {
                            var dateTime = new DateTimeContainer((DateTime)obj);
                            if (context != null)
                            {
                                dateTime.Culture = context.Culture;
                            }

                            return dateTime;
                        }
                    }
                }

                if ((context != null) && context.EncapsulateMacroObjects)
                {
                    var macroObj = obj as IMacroObject;
                    if (macroObj != null)
                    {
                        return macroObj.MacroRepresentation();
                    }
                }
            }

            return obj;
        }

        #endregion
    }
}