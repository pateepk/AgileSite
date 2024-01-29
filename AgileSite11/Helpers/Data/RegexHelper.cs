using System.Text.RegularExpressions;

using CMS.Base;

namespace CMS.Helpers
{
    using ExpressionDictionary = TwoLevelDictionary<RegexOptions, string, Regex>;

    /// <summary>
    /// Regular expression helper.
    /// </summary>
    public static class RegexHelper
    {
        #region "Variables"

        /// <summary>
        /// Default regular expression options.
        /// </summary>
        private static RegexOptions mDefaultOptions = RegexOptions.Compiled;

        /// <summary>
        /// Cached regular expressions
        /// </summary>
        private readonly static ExpressionDictionary mExpressions = new ExpressionDictionary();

        #endregion


        #region "Properties"

        /// <summary>
        /// Default regular expression options.
        /// </summary>
        public static RegexOptions DefaultOptions
        {
            get
            {
                return mDefaultOptions;
            }
            set
            {
                mDefaultOptions = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the regular expression specified by a matching pattern.
        /// </summary>
        /// <param name="pattern">Pattern to match</param>
        public static Regex GetRegex(string pattern)
        {
            return GetRegex(pattern, DefaultOptions);
        }


        /// <summary>
        /// Gets the regular expression specified by a matching pattern, optionally specifying processing options.
        /// </summary>
        /// <param name="pattern">Pattern to match</param>
        /// <param name="options">Processing options</param>
        public static Regex GetRegex(string pattern, RegexOptions options)
        {
            // Do not cache expression that are not compiled, create on-the-fly
            if (!options.HasFlag(RegexOptions.Compiled))
            {
                return CreateRegex(pattern, options);
            }

            // Try to get from cache
            var result = mExpressions[options, pattern];
            if (result == null)
            {
                result = CreateRegex(pattern, options);
                mExpressions[options, pattern] = result;
            }

            return result;
        }


        /// <summary>
        /// Creates a new regular expression
        /// </summary>
        /// <param name="pattern">Pattern to match</param>
        /// <param name="options">Processing options</param>
        private static Regex CreateRegex(string pattern, RegexOptions options)
        {
            return new Regex(pattern, CMSRegex.EnsureCorrectOptions(options));
        }


        /// <summary>
        /// Gets the regular expression specified by a matching pattern, optionally specifying processing options.
        /// </summary>
        /// <param name="pattern">Pattern to match</param>
        /// <param name="ignoreCase">If true, the regular expression is case insensitive</param>
        public static Regex GetRegex(string pattern, bool ignoreCase)
        {
            var options = CMSRegex.GetDefaultOptions(ignoreCase);

            return GetRegex(pattern, options);
        }

        #endregion
    }
}