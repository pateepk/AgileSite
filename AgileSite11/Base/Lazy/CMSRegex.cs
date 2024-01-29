using System.Text.RegularExpressions;

namespace CMS.Base
{
    /// <summary>
    /// Defined lazy initialized regular expression
    /// </summary>
    public class CMSRegex : CMSLazy<Regex>
    {
        /// <summary>
        /// Defines the options for ignoring case within regular expression
        /// </summary>
        public const RegexOptions IgnoreCase = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pattern">Expression pattern</param>
        /// <param name="options">Options</param>
        public CMSRegex(string pattern, RegexOptions options = RegexOptions.Compiled)
            : base(() => new Regex(pattern, EnsureCorrectOptions(options)))
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pattern">Expression pattern</param>
        /// <param name="ignoreCase">If true, the regular expression is case insensitive</param>
        public CMSRegex(string pattern, bool ignoreCase)
            : base(() => new Regex(pattern, GetDefaultOptions(ignoreCase)))
        {
        }


        /// <summary>
        /// Adds CultureInvariant option when there is ignore case to ensure correct behavior in Turkish culture.
        /// </summary>
        /// <param name="options">Options to be modified</param>
        public static RegexOptions EnsureCorrectOptions(RegexOptions options)
        {
            if (options.HasFlag(RegexOptions.IgnoreCase) && !options.HasFlag(RegexOptions.CultureInvariant))
            {
                // Add CultureInvariant option when there is ignore case to ensure correct behavior in Turkish culture
                options |= RegexOptions.CultureInvariant;
            }
            return options;
        }


        /// <summary>
        /// Gets the default options for a regular expression
        /// </summary>
        /// <param name="ignoreCase">If true, the regular expression is case insensitive</param>
        public static RegexOptions GetDefaultOptions(bool ignoreCase)
        {
            return (ignoreCase ? RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant : RegexOptions.Compiled);
        }


        /// <summary>
        /// Replaces the specified pattern within the given input with the given replacement
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="replacement">Replacement</param>
        /// <param name="allowSubstitutions">If set to false, substitutions are not allowed within the replacement string</param>
        public string Replace(string input, string replacement, bool allowSubstitutions = true)
        {
            return Value.Replace(input, replacement, allowSubstitutions);
        }


        /// <summary>
        /// Replaces the specified pattern within the given input with the given replacement
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="evaluator">Match evaluator</param>
        public string Replace(string input, MatchEvaluator evaluator)
        {
            return Value.Replace(input, evaluator);
        }


        /// <summary>
        /// Returns true if the given pattern is found within the input string
        /// </summary>
        /// <param name="input">Input string</param>
        public bool IsMatch(string input)
        {
            return Value.IsMatch(input);
        }
        

        /// <summary>
        /// Searches the specified input for all recurrences of regular expression
        /// </summary>
        /// <param name="input">Input text</param>
        public MatchCollection Matches(string input)
        {
            return Value.Matches(input);
        }


        /// <summary>
        /// Searches the specified input string for the first occurrence of the regular expression
        /// </summary>
        /// <param name="input">Input text</param>
        public Match Match(string input)
        {
            return Value.Match(input);
        }
    }
}
