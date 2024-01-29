using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Context for macro expression evaluation. Reflects resolver settings and inline macro parameters.
    /// </summary>
    [DebuggerDisplay("Culture: {Culture}; Case-sensitive: {CaseSensitive};")]
    public class EvaluationContext
    {
        #region "Variables"

        private bool mEncapsulateMacroObjects = true;
        private CultureInfo mCultureInfo;

        private List<object> mInnerSources;
        private List<object> mPrioritizedInnerSources;

        private string mOriginalExpressionType;
        private string mOriginalExpression;
        private string mResolverName;
        private string mIdentityName;
        private string mUserName;
        private string mHash;
        private string mConsoleOutput;
        private string mCulture;
        private bool? mAllowOnlySimpleMacros;
        private bool? mEncode;
        private bool? mRecursive;
        private bool? mHandleSQLInjection;

        private bool? mDetailedDebug;
        private bool? mDebug;

        private bool? mCheckSecurity;
        private bool? mIntegrityPassed;
        private bool? mCaseSensitive;
        private int? mEvaluationTimeout;

        private DateTime? mEvaluationStarted;
        private DateTime mMaxEvaluationTimeEnd = DateTime.MinValue;
        private object mRelatedObject;
        private IUserInfo mUser;
        
        #endregion


        #region "Private properties"

        private DateTime MaxEvaluationTimeEnd
        {
            get
            {
                if (mMaxEvaluationTimeEnd == DateTime.MinValue)
                {
                    mMaxEvaluationTimeEnd = EvaluationStarted.AddMilliseconds(EvaluationTimeout);
                }

                return mMaxEvaluationTimeEnd;
            }
        }


        /// <summary>
        /// TimeSpan defined when the evaluation timeout is checked and defined timeout is exceeded. Not set if <see cref="ExternalTimeoutChecker"/> exceeds.
        /// </summary>
        internal TimeSpan TimeoutOverTime
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the parent context (null if the context was created by constructor).
        /// </summary>
        private EvaluationContext ParentContext
        {
            get;
            set;
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Related object allows set object used for resolving in macro method. Used in web part properties.
        /// </summary>
        public object RelatedObject
        {
            get
            {
                if (mRelatedObject == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.RelatedObject;
                    }
                }
                return mRelatedObject;
            }
            set
            {
                mRelatedObject = value;
            }
        }


        /// <summary>
        /// If true, the object will be encapsulated to its macro representations
        /// </summary>
        public bool EncapsulateMacroObjects
        {
            get
            {
                return mEncapsulateMacroObjects;
            }
            set
            {
                mEncapsulateMacroObjects = value;
            }
        }


        /// <summary>
        /// Resolver which is used for the expression evaluation.
        /// </summary>
        public MacroResolver Resolver
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the expression which is being evaluated.
        /// </summary>
        public string OriginalExpression
        {
            get
            {
                if (mOriginalExpression == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.OriginalExpression;
                    }
                }
                return mOriginalExpression;
            }
            set
            {
                mOriginalExpression = value;
            }
        }


        /// <summary>
        /// Gets or sets the type of the expression which is being evaluated (% or $ or ?).
        /// </summary>
        public string OriginalExpressionType
        {
            get
            {
                if (mOriginalExpressionType == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.OriginalExpressionType;
                    }
                }
                return mOriginalExpressionType;
            }
            set
            {
                mOriginalExpressionType = value;
            }
        }


        /// <summary>
        /// Default value of the evaluation. Used when result of the whole macro is empty string.
        /// </summary>
        public string DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the resolver which can resolve this macro (reflects inline |(resolver) parameter).
        /// </summary>
        public string ResolverName
        {
            get
            {
                if (mResolverName == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.ResolverName;
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
        /// Console output - place where the results can be written during macro evaluation using a "print" method.
        /// All child contexts share the output with the parent context (= there is only one console output).
        /// </summary>
        public string ConsoleOutput
        {
            get
            {
                if (ParentContext != null)
                {
                    return ParentContext.ConsoleOutput;
                }
                else
                {
                    return mConsoleOutput;
                }
            }
            set
            {
                if (ParentContext != null)
                {
                    ParentContext.ConsoleOutput = value;
                }
                else
                {
                    mConsoleOutput = value;
                }
            }
        }


        /// <summary>
        /// Determines if the evaluation debugs details
        /// </summary>
        public bool Debug
        {
            get
            {
                if (mDebug == null)
                {
                    mDebug = MacroDebug.DebugCurrentRequest;
                }

                return mDebug.Value;
            }
        }


        /// <summary>
        /// Determines if the evaluation debugs details
        /// </summary>
        public bool DetailedDebug
        {
            get
            {
                if (mDetailedDebug == null)
                {
                    if (ParentContext != null)
                    {
                        // Do not debug details if debug disabled for current request
                        mDetailedDebug = ParentContext.DetailedDebug;
                    }
                    else
                    {
                        mDetailedDebug = Debug && MacroDebug.Detailed;
                    }
                }

                return mDetailedDebug.Value;
            }
            set
            {
                mDetailedDebug = value;
            }
        }


        /// <summary>
        /// Determines whether the result of the expression should be encoded or not.
        /// </summary>
        public bool Encode
        {
            get
            {
                if (mEncode == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.Encode;
                    }
                    mEncode = false;
                }
                return mEncode.Value;
            }
            set
            {
                mEncode = value;
            }
        }


        /// <summary>
        /// Determines whether the macros in the result should be resolved as well.
        /// </summary>
        public bool Recursive
        {
            get
            {
                if (mRecursive == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.Recursive;
                    }
                    mRecursive = false;
                }
                return mRecursive.Value;
            }
            set
            {
                mRecursive = value;
            }
        }


        /// <summary>
        /// Determines whether the apostrophes in the result will be doubled to handle SQL injection.
        /// </summary>
        public bool HandleSQLInjection
        {
            get
            {
                if (mHandleSQLInjection == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.HandleSQLInjection;
                    }
                    mHandleSQLInjection = false;
                }
                return mHandleSQLInjection.Value;
            }
            set
            {
                mHandleSQLInjection = value;
            }
        }


        /// <summary>
        /// Determines whether string comparison and other operations are case sensitive. False by default.
        /// </summary>
        public bool CaseSensitive
        {
            get
            {
                if (mCaseSensitive == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.CaseSensitive;
                    }
                    mCaseSensitive = false;
                }
                return mCaseSensitive.Value;
            }
            set
            {
                mCaseSensitive = value;
            }
        }


        /// <summary>
        /// Culture under which the expression is evaluated. Important for parsing double / datetime from string constants, etc. EN-US by default.
        /// </summary>
        public string Culture
        {
            get
            {
                if (mCulture == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.Culture;
                    }
                    mCulture = MacroStaticSettings.DEFAULT_CULTURE;
                }
                return mCulture;
            }
            set
            {
                mCultureInfo = null;
                mCulture = value;
            }
        }


        /// <summary>
        /// CultureInfo reflecting Culture property.
        /// </summary>
        public CultureInfo CultureInfo
        {
            get
            {
                return mCultureInfo ?? (mCultureInfo = CultureInfo.GetCultureInfo(string.IsNullOrEmpty(Culture) ? MacroStaticSettings.DEFAULT_CULTURE : Culture));
            }
        }


        /// <summary>
        /// Expression evaluation timeout in milliseconds (1000 ms by default). If the evaluation time of the expression exceeds this time, evaluation will be aborted and the result will be null.
        /// The evaluation abortion is then logged into event log.
        /// </summary>
        public int EvaluationTimeout
        {
            get
            {
                if (mEvaluationTimeout == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.EvaluationTimeout;
                    }

                    mEvaluationTimeout = MacroStaticSettings.EvaluationTimeout;
                }
                return mEvaluationTimeout.Value;
            }
            set
            {
                mEvaluationTimeout = value;
            }
        }


        /// <summary>
        /// Time when the evaluation started (needed for timeout evaluation).
        /// </summary>
        public DateTime EvaluationStarted
        {
            get
            {
                if (mEvaluationStarted == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.EvaluationStarted;
                    }
                    mEvaluationStarted = DateTime.Now;
                }

                return mEvaluationStarted.Value;
            }
            set
            {
                mEvaluationStarted = value;
            }
        }


        /// <summary>
        /// Gets or sets the function used as timeout watchdog besides EvaluationTimeout setting. Returns true if timeout has occurred.
        /// </summary>
        public Func<bool> ExternalTimeoutChecker
        {
            get;
            set;
        }


        #region "Security parameters"

        /// <summary>
        /// Main setting determining whether the macro security (signatures) is checked.
        /// </summary>
        public bool CheckSecurity
        {
            get
            {
                if (mCheckSecurity == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.CheckSecurity;
                    }

                    mCheckSecurity = false;
                }
                return mCheckSecurity.Value;
            }
            set
            {
                mCheckSecurity = value;
            }
        }


        /// <summary>
        /// Determines whether the security parameters are consistent with the evaluated macro.
        /// </summary>
        public bool IntegrityPassed
        {
            get
            {
                if (mIntegrityPassed == null)
                {
                    mIntegrityPassed = MacroSecurityProcessor.CheckMacroIntegrity(OriginalExpression, new MacroIdentityOption {IdentityName = IdentityName, UserName = UserName });
                }

                return mIntegrityPassed.Value;
            }
        }


        /// <summary>
        /// Gets the user associated with this context (based on the <see cref="IdentityName"/> and <see cref="UserName"/>)
        /// </summary>
        public IUserInfo User
        {
            get
            {
                if (mUser == null)
                {
                    if ((mIdentityName == null) && (mUserName == null) && (ParentContext != null))
                    {
                        return ParentContext.User;
                    }

                    mUser = new MacroIdentityOption { IdentityName = mIdentityName, UserName = mUserName }.GetEffectiveUser();
                }

                return mUser;
            }
        }


        /// <summary>
        /// Gets or sets the macro identity name against which the security is checked.
        /// </summary>
        public string IdentityName
        {
            get
            {
                if (mIdentityName == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.IdentityName;
                    }
                }

                return mIdentityName;
            }
            set
            {
                mIdentityName = value;
            }
        }


        /// <summary>
        /// Gets or sets the username against which the security is checked.
        /// </summary>
        public string UserName
        {
            get
            {
                if (mUserName == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.UserName;
                    }
                }

                return mUserName;
            }
            set
            {
                mUserName = value;
            }
        }


        /// <summary>
        /// Gets or sets the hash against which the security is checked.
        /// </summary>
        public string Hash
        {
            get
            {
                if (mHash == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.Hash;
                    }
                }
                return mHash;
            }
            set
            {
                mHash = value;
            }
        }


        /// <summary>
        /// Determines whether only simple macros (macros which do not require security check) are allowed.
        /// </summary>
        public bool AllowOnlySimpleMacros
        {
            get
            {
                if (mAllowOnlySimpleMacros == null)
                {
                    if (ParentContext != null)
                    {
                        return ParentContext.AllowOnlySimpleMacros;
                    }
                    mAllowOnlySimpleMacros = false;
                }
                return mAllowOnlySimpleMacros.Value;
            }
            set
            {
                mAllowOnlySimpleMacros = value;
            }
        }
        
        #endregion

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new empty instance of the context.
        /// </summary>
        internal EvaluationContext()
        {
        }


        /// <summary>
        /// Creates new EvaluationContext which takes the default values from resolver. Note that these can be modified during evaluation process via macro parameters.
        /// </summary>
        /// <param name="resolver">Resolver which is used for the evaluation</param>
        /// <param name="originalExpression">Original expression which is being evaluated</param>
        /// <param name="expressionType">Type of the expression being evaluated (% or $ or ?)</param>
        public EvaluationContext(MacroResolver resolver, string originalExpression, string expressionType = "%")
        {
            var settings = resolver.Settings;

            // Get default values from MacroContext
            Encode = settings.EncodeResolvedValues;
            IdentityName = settings.IdentityOption?.IdentityName;
            UserName = settings.IdentityOption?.UserName;
            EvaluationTimeout = settings.EvaluationTimeout;
            CheckSecurity = settings.CheckSecurity;
            Culture = settings.Culture;
            HandleSQLInjection = settings.AvoidInjection;
            Recursive = settings.AllowRecursion;
            EncapsulateMacroObjects = settings.EncapsulateMacroObjects;
            RelatedObject = settings.RelatedObject;
            ExternalTimeoutChecker = settings.ExternalTimeoutChecker;

            // Avoid debug and case sensitive parameter distribution through full recursion.
            DetailedDebug = MacroDebug.Detailed && Debug;
            CaseSensitive = false;

            AllowOnlySimpleMacros = MacroStaticSettings.AllowOnlySimpleMacros;
            CaseSensitive = MacroStaticSettings.CaseSensitiveComparison;

            OriginalExpression = originalExpression;
            OriginalExpressionType = expressionType;
            Resolver = resolver;
        }


        /// <summary>
        /// Creates new EvaluationContext which takes the values from resolver and parameters. Note that these can be modified during evaluation process via macro parameters.
        /// </summary>
        /// <param name="resolver">Resolver which is used for the evaluation</param>
        /// <param name="evaluationParameters">Inherited macro parameters from another macro evaluation. Can be null.</param>
        /// <param name="originalExpression">Original expression which is being evaluated</param>
        /// <param name="expressionType">Type of the expression being evaluated (% or $ or ?)</param>
        internal EvaluationContext(MacroResolver resolver, EvaluationParameters evaluationParameters, string originalExpression, string expressionType = "%")
            : this(resolver, originalExpression, expressionType)
        {
            if (evaluationParameters != null)
            {
                LoadParameters(evaluationParameters);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates child context (clones the parent settings and creates child of the resolver).
        /// </summary>
        public EvaluationContext CreateChildContext()
        {
            return CreateChildContext(true);
        }


        /// <summary>
        /// Creates child context (clones the parent settings and creates child of the resolver).
        /// </summary>
        /// <param name="createChildResolver">If true, the resolver of the child context will be a child resolver of current resolver. If false, the same instance of the resolver will be used.</param>
        public EvaluationContext CreateChildContext(bool createChildResolver)
        {
            var result = new EvaluationContext();
            result.ParentContext = this;
            result.Resolver = createChildResolver ? Resolver.CreateChild() : Resolver;
            return result;
        }


        /// <summary>
        /// Clears all inner sources (does not clear parent inner sources).
        /// </summary>
        public void ClearInnerSources()
        {
            mInnerSources = null;
        }


        /// <summary>
        /// Adds inner source(s) to this instance of the context.
        /// </summary>
        /// <param name="sources">Source(s) to add</param>
        public void AddInnerSource(params object[] sources)
        {
            if (mInnerSources == null)
            {
                mInnerSources = new List<object>();
            }
            mInnerSources.AddRange(sources);
        }


        /// <summary>
        /// Returns list of all aggregated inner sources (from the whole context hierarchy).
        /// </summary>
        public List<object> GetInnerSources()
        {
            if (mInnerSources == null)
            {
                mInnerSources = new List<object>();
            }

            List<object> final = new List<object>();
            final.AddRange(mInnerSources);

            // Copy source data keys from parent context
            if (ParentContext != null)
            {
                final.AddRange(ParentContext.GetInnerSources());
            }

            // Copy source data from resolver
            final.AddRange(Resolver.GetAnonymousSources());

            return final;
        }


        /// <summary>
        /// Clears all prioritized inner sources (does not clear parent inner sources).
        /// </summary>
        public void ClearPrioritizedInnerSources()
        {
            mPrioritizedInnerSources = null;
        }


        /// <summary>
        /// Adds prioritized inner source(s) to this instance of the context.
        /// </summary>
        /// <param name="sources">Source(s) to add</param>
        public void AddPrioritizedInnerSource(params object[] sources)
        {
            if (mPrioritizedInnerSources == null)
            {
                mPrioritizedInnerSources = new List<object>();
            }
            mPrioritizedInnerSources.AddRange(sources);
        }


        /// <summary>
        /// Returns list of all aggregated prioritized inner sources (from the whole context hierarchy).
        /// </summary>
        public List<object> GetPrioritizedInnerSources()
        {
            if (mPrioritizedInnerSources == null)
            {
                mPrioritizedInnerSources = new List<object>();
            }

            List<object> final = new List<object>();
            final.AddRange(mPrioritizedInnerSources);

            // Copy source data keys from parent resolver
            if (ParentContext != null)
            {
                final.AddRange(ParentContext.GetPrioritizedInnerSources());
            }

            return final;
        }


        /// <summary>
        /// Loads values of evaluation parameters.
        /// </summary>
        /// <param name="evaluationParameters"></param>
        internal void LoadParameters(EvaluationParameters evaluationParameters)
        {
            if (evaluationParameters == null)
            {
                throw new ArgumentNullException(nameof(evaluationParameters));
            }

            mCaseSensitive = evaluationParameters.CaseSensitive ?? mCaseSensitive;
            mCulture = evaluationParameters.Culture ?? mCulture;
            DefaultValue = evaluationParameters.DefaultValue ?? DefaultValue;
            mDetailedDebug = evaluationParameters.DetailedDebug ?? mDetailedDebug;
            mEncode = evaluationParameters.Encode ?? mEncode;
            mEvaluationTimeout = evaluationParameters.EvaluationTimeout ?? mEvaluationTimeout;
            mHandleSQLInjection = evaluationParameters.HandleSQLInjection ?? mHandleSQLInjection;
            mHash = evaluationParameters.Hash ?? mHash;
            mRecursive = evaluationParameters.Recursive ?? mRecursive;
            mResolverName = evaluationParameters.ResolverName ?? mResolverName;
            mIdentityName = evaluationParameters.IdentityName ?? mIdentityName;
            mUserName = evaluationParameters.UserName ?? mUserName;
        }


        /// <summary>
        /// Verifies whether evaluation is timeouting. Returns true if timeout has occurred. False if everything is alright.
        /// </summary>
        internal bool CheckForTimeout()
        {
            var maxTimeOut = MaxEvaluationTimeEnd;
            var currentTime = DateTime.Now;

            var currentTimeout = maxTimeOut <= currentTime;
            var parentTimeout = (ParentContext != null) && ParentContext.CheckForTimeout();
            var externalTimeout = (ExternalTimeoutChecker != null) && ExternalTimeoutChecker();

            var resultTimeout = currentTimeout || parentTimeout || externalTimeout;

            // Clear timeout over time
            TimeoutOverTime = TimeSpan.Zero;

            if (resultTimeout)
            {
                if (parentTimeout)
                {
                    // Parent timeout failed
                    TimeoutOverTime = ParentContext.TimeoutOverTime;
                }
                else
                {
                    // Current timeout failed
                    TimeoutOverTime = currentTime - maxTimeOut;
                }
            }
            
            return resultTimeout;
        }

        #endregion
    }
}