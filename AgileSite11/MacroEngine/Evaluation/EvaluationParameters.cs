using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Contains values of macro parameters
    /// </summary>
    internal class EvaluationParameters
    {
        internal const string HASH_PARAM_LOWERED = "hash";
        internal const string USER_PARAM_LOWERED = "user";
        internal const string IDENTITY_PARAM_LOWERED = "identity";
        internal const string CULTURE_PARAM_LOWERED = "culture";
        internal const string DEBUG_PARAM_LOWERED = "debug";
        internal const string CASE_SENSITIVE_PARAM_LOWERED = "casesensitive";
        internal const string TIMEOUT_PARAM_LOWERED = "timeout";
        internal const string RESOLVER_PARAM_LOWERED = "resolver";
        internal const string ENCODE_PARAM_LOWERED = "encode";
        internal const string SQL_INJECTION_PARAM_LOWERED = "handlesqlinjection";
        internal const string RECURSIVE_PARAM_LOWERED = "recursive";
        internal const string DEFAULT_PARAM_LOWERED = "default";

        // Backward compatibility - 'notrecursive' parameter replaced with 'recursive' parameter
        internal const string LOWERED_NOT_RECURSIVE_PARAMETER_NAME = "notrecursive";

        #region "Properties"

        /// <summary>
        /// Gets or sets the hash against which the security is checked.
        /// </summary>
        [MacroParameterName(HASH_PARAM_LOWERED)]
        public string Hash
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the identity name against which the security is checked.
        /// </summary>
        [MacroParameterName(IDENTITY_PARAM_LOWERED)]
        public string IdentityName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the username against which the security is checked.
        /// </summary>
        [MacroParameterName(USER_PARAM_LOWERED)]
        public string UserName
        {
            get;
            set;
        }


        /// <summary>
        /// Culture under which the expression is evaluated. Important for parsing double / datetime from string constants, etc. EN-US by default.
        /// </summary>
        [MacroParameterName(CULTURE_PARAM_LOWERED)]
        public string Culture
        {
            get;
            set;
        }


        /// <summary>
        /// Determines if the evaluation debugs details
        /// </summary>
        [MacroParameterName(DEBUG_PARAM_LOWERED)]
        public bool? DetailedDebug
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether string comparison and other operations are case sensitive. False by default.
        /// </summary>
        [MacroParameterName(CASE_SENSITIVE_PARAM_LOWERED)]
        public bool? CaseSensitive
        {
            get;
            set;
        }


        /// <summary>
        /// Expression evaluation timeout in milliseconds (1000 ms by default). If the evaluation time of the expression exceeds this time, evaluation will be aborted and the result will be null.
        /// The evaluation abortion is then logged into event log.
        /// </summary>
        [MacroParameterName(TIMEOUT_PARAM_LOWERED)]
        public int? EvaluationTimeout
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the resolver which can resolve this macro (reflects inline |(resolver) parameter).
        /// </summary>
        [MacroParameterName(RESOLVER_PARAM_LOWERED)]
        public string ResolverName
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the result of the expression should be encoded or not.
        /// </summary>
        [MacroParameterName(ENCODE_PARAM_LOWERED)]
        public bool? Encode
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the apostrophes in the result will be doubled to handle SQL injection.
        /// </summary>
        [MacroParameterName(SQL_INJECTION_PARAM_LOWERED)]
        public bool? HandleSQLInjection
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the macros in the result should be resolved as well.
        /// </summary>
        [MacroParameterName(RECURSIVE_PARAM_LOWERED)]
        public bool? Recursive
        {
            get;
            set;
        }


        /// <summary>
        /// Default value of the evaluation. Used when result of the whole macro is empty string.
        /// </summary>
        [MacroParameterName(DEFAULT_PARAM_LOWERED)]
        public string DefaultValue
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor
        /// </summary>
        public EvaluationParameters()
        {
        }


        /// <summary>
        /// Initializes parameters from <see cref="EvaluationContext"/>. Copies inheritable macro parameters from another macro evaluation.
        /// </summary>
        public EvaluationParameters(EvaluationContext evaluationContext)
        {
            if (evaluationContext == null)
            {
                throw new ArgumentNullException(nameof(evaluationContext));
            }

            // Macro identity name / user name and hash parameters are expression specific. They are not inheritable.

            CaseSensitive = evaluationContext.CaseSensitive;
            Culture = evaluationContext.Culture;
            DefaultValue = evaluationContext.DefaultValue;
            DetailedDebug = evaluationContext.DetailedDebug;
            Encode = evaluationContext.Encode;
            EvaluationTimeout = evaluationContext.EvaluationTimeout;
            HandleSQLInjection = evaluationContext.HandleSQLInjection;
            Recursive = evaluationContext.Recursive;
            ResolverName = evaluationContext.ResolverName;
        }


        /// <summary>
        /// Initializes parameters from <see cref="MacroExpression.Parameters"/>. Parses the parameters in expression.
        /// </summary>
        public EvaluationParameters(IEnumerable<MacroExpression> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (var parameter in parameters)
            {
                if (!SetParameter(parameter.Name, parameter.Value))
                {
                    string errMessage = "Error while evaluating expression: " + parameter.ToString(true) + "\r\n\r\n" + "Invalid parameter";
                    MacroDebug.LogMacroFailure(parameter.ToString(true), errMessage, "RESOLVEDATAMACRO");
                }
            }
        }

        #endregion


        #region

        /// <summary>
        /// Parse given macro parameter and it's value and stores it inside this data container.
        /// </summary>
        /// <param name="name">Macro parameter name</param>
        /// <param name="value">Macro parameter value</param>
        /// <returns>True when parameter was recognized and it's value was stored.</returns>
        public bool SetParameter(string name, object value)
        {
            string paramValue = value != null ? value.ToString().Trim() : null;

            switch (name?.ToLowerInvariant())
            {
                case HASH_PARAM_LOWERED:
                    Hash = paramValue;
                    break;

                case IDENTITY_PARAM_LOWERED:
                    IdentityName = paramValue;
                    break;

                case USER_PARAM_LOWERED:
                    UserName = paramValue;
                    break;

                case CULTURE_PARAM_LOWERED:
                    Culture = paramValue;
                    break;

                case DEBUG_PARAM_LOWERED:
                    DetailedDebug = ValidationHelper.GetBoolean(paramValue, true);
                    break;

                case CASE_SENSITIVE_PARAM_LOWERED:
                    CaseSensitive = ValidationHelper.GetBoolean(paramValue, true);
                    break;

                case TIMEOUT_PARAM_LOWERED:
                    EvaluationTimeout = ValidationHelper.GetInteger(paramValue, 1000);
                    break;

                case RESOLVER_PARAM_LOWERED:
                    ResolverName = paramValue;
                    break;

                case ENCODE_PARAM_LOWERED:
                    Encode = ValidationHelper.GetBoolean(paramValue, true);
                    break;

                case SQL_INJECTION_PARAM_LOWERED:
                    HandleSQLInjection = ValidationHelper.GetBoolean(paramValue, true);
                    break;

                case RECURSIVE_PARAM_LOWERED:
                    Recursive = ValidationHelper.GetBoolean(paramValue, true);
                    break;

                case LOWERED_NOT_RECURSIVE_PARAMETER_NAME:
                    Recursive = !ValidationHelper.GetBoolean(paramValue, true);
                    break;

                case DEFAULT_PARAM_LOWERED:
                    DefaultValue = ValidationHelper.GetString(paramValue, "");
                    break;
                default:
                    // Parameter was not recognized
                    return false;
            }

            return true;
        }

        #endregion
    }
}
