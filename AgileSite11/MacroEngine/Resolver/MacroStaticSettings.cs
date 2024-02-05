using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class providing static settings of MacroEngine.
    /// </summary>
    public static class MacroStaticSettings
    {
        #region "Constants"

        /// <summary>
        /// Default expression evaluation timeout in milliseconds.
        /// </summary>
        private const int DEFAULT_EVALUATION_TIMEOUT = 1000;


        /// <summary>
        /// Default value for maximal recursion level in expression evaluation.
        /// </summary>
        internal const int DEFAULT_MAX_RECURSION_LEVEL = 9;


        /// <summary>
        /// Default culture under which the expression is evaluated.
        /// </summary>
        internal const string DEFAULT_CULTURE = "en-us";

        #endregion


        #region "Variables"

        private static bool mAllowContextMacros = true;
        private static bool mAllowMacros = true;
        private static bool mAllowCookieMacros = true;
        private static bool mAllowQueryMacros = true;
        private static bool mAllowDataMacros = true;
        private static bool mAllowLocalizationMacros = true;
        private static bool mAllowCustomMacros = true;
        private static bool mAllowSpecialMacros = true;
        private static bool? mDisableParameters;
        private static bool? mAllowOnlySimpleMacros;
        private static bool? mCaseSensitiveComparison;
        private static int? mMaxMacroNodes;
        private static int? mEvaluationTimeout;

        #endregion


        #region "Properties"


        /// <summary>
        /// Gets or sets default macro evaluation timeout
        /// </summary>
        internal static int EvaluationTimeout
        {
            get
            {
                if (mEvaluationTimeout == null)
                {
                    mEvaluationTimeout = DEFAULT_EVALUATION_TIMEOUT;
                }
                return mEvaluationTimeout.Value;
            }
            set
            {
                mEvaluationTimeout = value;
            }
        }


        /// <summary>
        /// Determines how many properties can be displayed in AutoCompletion and MacroTree controls. Reflects global setting CMSMaxMacroNodes from web.config.
        /// </summary>
        public static int MaxMacroNodes
        {
            get
            {
                if (mMaxMacroNodes == null)
                {
                    mMaxMacroNodes = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSMaxMacroNodes"], 250);
                }
                return mMaxMacroNodes.Value;
            }
            set
            {
                mMaxMacroNodes = value;
            }
        }


        /// <summary>
        /// Determines whether the string comparison in macros is case sensitive (unless the |(casesensitive) parameter is used.). Reflects global setting CMSMacrosCaseSensitiveComparison from web.config.
        /// </summary>
        public static bool CaseSensitiveComparison
        {
            get
            {
                if (mCaseSensitiveComparison == null)
                {
                    mCaseSensitiveComparison = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSMacrosCaseSensitiveComparison"], false);
                }
                return mCaseSensitiveComparison.Value;
            }
            set
            {
                mCaseSensitiveComparison = value;
            }
        }


        /// <summary>
        /// If true, macro parameters are ignored and not processed (CMSDisableMacroParameters web.config key is reflected), false by default.
        /// </summary>
        public static bool DisableParameters
        {
            get
            {
                if (mDisableParameters == null)
                {
                    mDisableParameters = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDisableMacroParameters"], false);
                }

                return mDisableParameters.Value;
            }
            set
            {
                mDisableParameters = value;
            }
        }


        /// <summary>
        /// If true, only simple macros (those which don't need security check) are allowed, all others won't be resolved. If true, CMSTextBox does not add security parameters to macros.
        /// </summary>
        public static bool AllowOnlySimpleMacros
        {
            get
            {
                if (mAllowOnlySimpleMacros == null)
                {
                    mAllowOnlySimpleMacros = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSAllowOnlySimpleMacros"], false);
                }

                return mAllowOnlySimpleMacros.Value;
            }
            set
            {
                mAllowOnlySimpleMacros = value;
            }
        }


        /// <summary>
        /// Allow macro resolving?
        /// </summary>
        public static bool AllowMacros
        {
            get
            {
                return mAllowMacros;
            }
            set
            {
                mAllowMacros = value;
            }
        }


        /// <summary>
        /// Allow query macro resolving?
        /// </summary>
        public static bool AllowQueryMacros
        {
            get
            {
                return mAllowQueryMacros;
            }
            set
            {
                mAllowQueryMacros = value;
            }
        }


        /// <summary>
        /// Allow context macro resolving?
        /// </summary>
        public static bool AllowContextMacros
        {
            get
            {
                return mAllowContextMacros;
            }
            set
            {
                mAllowContextMacros = value;
            }
        }


        /// <summary>
        /// Allow cookie macro resolving?
        /// </summary>
        public static bool AllowCookieMacros
        {
            get
            {
                return mAllowCookieMacros;
            }
            set
            {
                mAllowCookieMacros = value;
            }
        }


        /// <summary>
        /// Allow data macro resolving?
        /// </summary>
        public static bool AllowDataMacros
        {
            get
            {
                return mAllowDataMacros;
            }
            set
            {
                mAllowDataMacros = value;
            }
        }


        /// <summary>
        /// Allow localization macro resolving?
        /// </summary>
        public static bool AllowLocalizationMacros
        {
            get
            {
                return mAllowLocalizationMacros;
            }
            set
            {
                mAllowLocalizationMacros = value;
            }
        }


        /// <summary>
        /// Allow custom macro resolving?
        /// </summary>
        public static bool AllowCustomMacros
        {
            get
            {
                return mAllowCustomMacros;
            }
            set
            {
                mAllowCustomMacros = value;
            }
        }


        /// <summary>
        /// Allow special macro resolving?
        /// </summary>
        public static bool AllowSpecialMacros
        {
            get
            {
                return mAllowSpecialMacros;
            }
            set
            {
                mAllowSpecialMacros = value;
            }
        }

        #endregion
    }
}