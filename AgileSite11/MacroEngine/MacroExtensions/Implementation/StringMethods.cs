using System;
using System.Linq;
using System.Text.RegularExpressions;

using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(StringMethods), typeof(char))]
[assembly: RegisterExtension(typeof(StringMethods), typeof(string))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide string methods in the MacroEngine.
    /// </summary>
    internal class StringMethods : MacroMethodContainer
    {
        /// <summary>
        /// Converts the string to lower.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Converts the string to lower.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to convert.")]
        public static object ToLower(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return GetStringParam(parameters[0], context.Culture).ToLowerCSafe();

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts the string to upper.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Converts the string to upper.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to convert.")]
        public static object ToUpper(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Only single parameter is supported
                    return GetStringParam(parameters[0], context.Culture).ToUpperCSafe();

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Retrieves a substring from this instance.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Retrieves a substring from this instance.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "index", typeof(int), "The index of the start of the substring as the second.")]
        [MacroMethodParam(2, "length", typeof(int), "The number of characters in the substring as the third.")]
        public static object Substring(EvaluationContext context, params object[] parameters)
        {
            int index, length;
            switch (parameters.Length)
            {
                case 2:
                    // Overload for two parameters
                    index = GetIntParam(parameters[1]);
                    return GetStringParam(parameters[0], context.Culture).Substring(index);

                case 3:
                    // Overload for three parameters
                    string text = GetStringParam(parameters[0], context.Culture);
                    index = GetIntParam(parameters[1]);
                    length = GetIntParam(parameters[2]);
                    if (index + length <= text.Length)
                    {
                        return text.Substring(index, length);
                    }
                    else
                    {
                        return text.Substring(index);
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Identifies the substrings in this instance that are delimited by one or more characters specified in an array, then places the substrings into a String array.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string[]), "Identifies the substrings in this instance that are delimited by one or more characters specified in an array, then places the substrings into a String array.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "String to split.")]
        [MacroMethodParam(1, "delimiters", typeof(string), "Delimiters string. Each character of this string will be taken as a delimiter.")]
        [MacroMethodParam(2, "removeEmpty", typeof(bool), "If true, removes empty entries from the split results.")]
        public static object Split(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3:
                    // Overload for two parameters
                    {
                        var text = GetStringParam(parameters[0], context.Culture);
                        var delimiters = GetStringParam(parameters[1], context.Culture).ToCharArray();

                        var removeEmpty = GetOptionalParam(parameters, 2, false);
                        var options = removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;

                        return text.Split(delimiters, options);
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Removes all occurrences of white space characters from the beginning and end of this instance.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Removes all occurrences of white space characters from the beginning and end of this instance.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be trimmed as a first parameter.")]
        [MacroMethodParam(1, "charsToTrim", typeof(string), "String divided to individual characters which are then trimmed.")]
        public static object Trim(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                // First overload of Trim method
                case 1:
                    return GetStringParam(parameters[0], context.Culture).Trim();

                // Overload with characters to be trimmed
                case 2:
                    char[] trimChars;
                    if (parameters[1] is char[])
                    {
                        trimChars = (char[])parameters[1];
                    }
                    else
                    {
                        trimChars = GetStringParam(parameters[1], context.Culture).ToCharArray();
                    }
                    return GetStringParam(parameters[0], context.Culture).Trim(trimChars);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Removes all occurrences of a set of characters specified in an array from the end of this instance.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Removes all occurrences of a set of characters specified in an array from the end of this instance.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be trimmed.")]
        [MacroMethodParam(1, "charsToTrim", typeof(string), "String divided to individual characters which are then trimmed.")]
        public static object TrimEnd(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return GetStringParam(parameters[0], context.Culture).TrimEnd();

                case 2:
                    char[] trimChars;
                    if (parameters[1] is char[])
                    {
                        trimChars = (char[])parameters[1];
                    }
                    else
                    {
                        trimChars = GetStringParam(parameters[1], context.Culture).ToCharArray();
                    }
                    return GetStringParam(parameters[0], context.Culture).TrimEnd(trimChars);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Removes all occurrences of a set of characters specified in an array from the beginning of this instance.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Removes all occurrences of a set of characters specified in an array from the beginning of this instance.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be trimmed.")]
        [MacroMethodParam(1, "charsToTrim", typeof(string), "String divided to individual characters which are then trimmed.")]
        public static object TrimStart(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return GetStringParam(parameters[0], context.Culture).TrimStart();

                case 2:
                    char[] trimChars;
                    if (parameters[1] is char[])
                    {
                        trimChars = (char[])parameters[1];
                    }
                    else
                    {
                        trimChars = GetStringParam(parameters[1], context.Culture).ToCharArray();
                    }
                    return GetStringParam(parameters[0], context.Culture).TrimStart(trimChars);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Replaces all occurrences of a specified Unicode character or String in this instance, with another specified Unicode character or String.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Replaces all occurrences of a specified Unicode character or String in this instance, with another specified Unicode character or String.", 3)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "replace", typeof(string), "Text to be replaced.")]
        [MacroMethodParam(2, "replacement", typeof(string), "Replacement text.")]
        public static object Replace(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                // Only three parameters are supported
                case 3:
                    return GetStringParam(parameters[0], context.Culture).Replace(GetStringParam(parameters[1], context.Culture), GetStringParam(parameters[2], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Deletes a specified number of characters from this instance beginning at a specified position.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Deletes a specified number of characters from this instance beginning at a specified position.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "position", typeof(int), "The position in this instance to begin deleting characters as the second.")]
        [MacroMethodParam(2, "length", typeof(int), "The number of characters to delete as the third.")]
        public static object Remove(EvaluationContext context, params object[] parameters)
        {
            string text;
            int index;
            switch (parameters.Length)
            {
                case 2:
                    text = GetStringParam(parameters[0], context.Culture);
                    index = Math.Max(Math.Min(GetIntParam(parameters[1]), text.Length), 0);
                    return (index < text.Length ? text.Remove(index) : text);

                case 3:
                    text = GetStringParam(parameters[0], context.Culture);
                    index = Math.Max(Math.Min(GetIntParam(parameters[1]), text.Length), 0);
                    int count = Math.Max(Math.Min(GetIntParam(parameters[2]), text.Length - index), 0);
                    return (index < text.Length ? text.Remove(index, count) : text);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Left-aligns the characters in this string, padding on the right with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Left-aligns the characters in this string, padding on the right with a specified Unicode character, for a specified total length.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "length", typeof(int), "The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.")]
        [MacroMethodParam(2, "paddingString", typeof(string), "A Unicode padding character (if not specified, space is used).")]
        public static object PadLeft(EvaluationContext context, params object[] parameters)
        {
            int width;
            switch (parameters.Length)
            {
                case 2:
                    // Overload with two parameters
                    width = GetIntParam(parameters[1]);
                    return GetStringParam(parameters[0], context.Culture).PadLeft(width);

                case 3:
                    // Overload with three parameters
                    width = GetIntParam(parameters[1]);
                    char padChar = GetStringParam(parameters[2], context.Culture).ToCharArray()[0];
                    return GetStringParam(parameters[0], context.Culture).PadLeft(width, padChar);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Right-aligns the characters in this string, padding on the left with a specified Unicode character, for a specified total length.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Right-aligns the characters in this string, padding on the left with a specified Unicode character, for a specified total length.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "length", typeof(int), "The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.")]
        [MacroMethodParam(2, "paddingString", typeof(string), "A Unicode padding character (if not specified, space is used).")]
        public static object PadRight(EvaluationContext context, params object[] parameters)
        {
            int width;
            switch (parameters.Length)
            {
                case 2:
                    // Overload with two parameters
                    width = GetIntParam(parameters[1]);
                    return GetStringParam(parameters[0], context.Culture).PadRight(width);

                case 3:
                    // Overload with three parameters
                    width = GetIntParam(parameters[1]);
                    char padChar = GetStringParam(parameters[2], context.Culture).ToCharArray()[0];
                    return GetStringParam(parameters[0], context.Culture).PadRight(width, padChar);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Determines whether the end of this instance matches at least one of the specified string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Determines whether the end of this instance matches at least one of the specified strings.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to check.", false)]
        [MacroMethodParam(1, "findText", typeof(string), "Text(s) to find.", IsParams = true)]
        public static object EndsWith(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                string baseText = GetStringParam(parameters[0], context.Culture);
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (baseText.EndsWithCSafe(GetStringParam(parameters[i], context.Culture), !context.CaseSensitive))
                    {
                        return true;
                    }
                }

                return false;
            }
            throw new NotSupportedException();
        }


        /// <summary>
        /// Determines whether the beginning of this instance matches at least one of the specified string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Determines whether the beginning of this instance matches at least one of the specified strings.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to check.", false)]
        [MacroMethodParam(1, "findText", typeof(string), "Text(s) to find.", IsParams = true)]
        public static object StartsWith(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                string baseText = GetStringParam(parameters[0], context.Culture);
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (baseText.StartsWithCSafe(GetStringParam(parameters[i], context.Culture), !context.CaseSensitive))
                    {
                        return true;
                    }
                }

                return false;
            }
            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns a value indicating whether the specified string object(s) do(es) not occur within this string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns a value indicating whether the specified string(s) do(es) not occur within this string (if more than one string is specified, none of them can occur within a base string).", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to search in.", false)]
        [MacroMethodParam(1, "search", typeof(string), "Text(s) to search.", IsParams = true)]
        public static object NotContains(EvaluationContext context, params object[] parameters)
        {
            return ContainsInternal(context, parameters, true);
        }


        /// <summary>
        /// Returns a value indicating whether the specified string object(s) occur(s) within this string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns a value indicating whether the specified string(s) occurs within this string (if more than one string is specified, all of them have to occur within a base string).", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to search in.", false)]
        [MacroMethodParam(1, "search", typeof(string), "Text(s) to search.", IsParams = true)]
        public static object Contains(EvaluationContext context, params object[] parameters)
        {
            return ContainsInternal(context, parameters, false);
        }


        /// <summary>
        /// Returns a value indicating whether the specified string object(s) do(es) not occur within this string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        /// <param name="notContains">Depends whether notcontains or contains is resolved</param>
        private static object ContainsInternal(EvaluationContext context, object[] parameters, bool notContains)
        {
            if (parameters.Length > 1)
            {
                string baseText = GetStringParam(parameters[0], context.Culture);
                for (int i = 1; i < parameters.Length; i++)
                {
                    bool result;
                    if (context.CaseSensitive)
                    {
                        result = baseText.Contains(GetStringParam(parameters[i], context.Culture));
                    }
                    else
                    {
                        result = baseText.ToLowerCSafe().Contains(GetStringParam(parameters[i], context.Culture).ToLowerCSafe());
                    }
                    if (result == notContains)
                    {
                        return false;
                    }
                }

                return true;
            }
            throw new NotSupportedException();
        }


        /// <summary>
        /// Reports the index of the first occurrence of a String, or one or more characters, within this instance.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Reports the index of the first occurrence of a String, or one or more characters, within this instance.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "searchFor", typeof(string), "The string to seek as the second.")]
        public static object IndexOf(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return GetStringParam(parameters[0], context.Culture).IndexOfCSafe(GetStringParam(parameters[1], context.Culture), !context.CaseSensitive);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Reports the index position of the last occurrence of a specified Unicode character or String within this instance.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Reports the index position of the last occurrence of a specified Unicode character or String within this instance.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Base text.")]
        [MacroMethodParam(1, "searchFor", typeof(string), "The string to seek as the second.")]
        public static object LastIndexOf(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return GetStringParam(parameters[0], context.Culture).LastIndexOfCSafe(GetStringParam(parameters[1], context.Culture), !context.CaseSensitive);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Replaces the string using regular expressions.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Replaces the string using regular expressions.", 3)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        [MacroMethodParam(1, "regex", typeof(string), "Regular expression.")]
        [MacroMethodParam(2, "replacement", typeof(string), "Replacement string.")]
        public static object RegexReplace(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    // Only three parameters are supported
                    string stringResult = GetStringParam(parameters[0], context.Culture);
                    string exp = GetStringParam(parameters[1], context.Culture);
                    string dest = GetStringParam(parameters[2], context.Culture);

                    try
                    {
                        // Create the regex
                        Regex re = RegexHelper.GetRegex(exp);
                        return re.Replace(stringResult, dest);
                    }
                    catch
                    {
                    }

                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Matches the value to the regular expression and returns the match.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Matches the value to the regular expression and returns the match.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        [MacroMethodParam(1, "regex", typeof(string), "Regular expression.")]
        public static object GetMatch(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported
                    try
                    {
                        // Create the regex
                        Regex re = RegexHelper.GetRegex(GetStringParam(parameters[1], context.Culture));
                        Match m = re.Match(GetStringParam(parameters[0], context.Culture));
                        return m.ToString();
                    }
                    catch
                    {
                    }

                    return "";

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Matches the value to the regular expression.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Matches the value to the regular expression.", 2)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be processed.")]
        [MacroMethodParam(1, "regex", typeof(string), "Regular expression.")]
        public static object Matches(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported
                    try
                    {
                        // Create the regex
                        Regex re = RegexHelper.GetRegex(GetStringParam(parameters[1], context.Culture));
                        return re.IsMatch(GetStringParam(parameters[0], context.Culture));
                    }
                    catch
                    {
                    }

                    return false;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Replaces each format item in a specified string with the text equivalent of a corresponding object's value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Replaces each format item in a specified string with the text equivalent of a corresponding object's value.", 2)]
        [MacroMethodParam(0, "format", typeof(string), "A composite format string.")]
        [MacroMethodParam(1, "items", typeof(string), "An object array that contains zero or more objects to format.", IsParams = true)]
        public static object Format(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length <= 1)
            {
                throw new NotSupportedException();
            }

            string format = GetStringParam(parameters[0]);

            // Remove first 'format' value and use rest of array
            var pars = parameters.Skip(1).ToArray();

            return string.Format(format, pars);
        }


        /// <summary>
        /// Formats the given value using the given string in case the value is not empty.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Formats the given value using the given string in case the value is not empty.", 2)]
        [MacroMethodParam(0, "value", typeof(string), "Value to format.")]
        [MacroMethodParam(1, "format", typeof(string), "A composite format string.")]
        [MacroMethodParam(1, "emptyResult", typeof(string), "Result in case the value is empty.")]
        public static object FormatNotEmpty(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3:
                    // Only 2 or 3 parameters are supported
                    {
                        string value = GetStringParam(parameters[0]);
                        if (String.IsNullOrEmpty(value))
                        {
                            // If value is empty, returns the empty result if available
                            if (parameters.Length > 2)
                            {
                                return GetStringParam(parameters[2]);
                            }

                            return null;
                        }

                        string format = GetStringParam(parameters[1]);

                        return string.Format(format, value);
                    }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}