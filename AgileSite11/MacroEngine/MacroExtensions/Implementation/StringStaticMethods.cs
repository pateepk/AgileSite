using System;
using System.Collections;
using System.Text;

using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide basic string static methods in the MacroEngine.
    /// </summary>
    internal class StringStaticMethods : MacroMethodContainer
    {
        /// <summary>
        /// Indicates whether the specified string is null, Empty or consists only of whitespace characters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates whether the specified string is null, Empty or consists only of whitespace characters.", 1)]
        [MacroMethodParam(0, "value", typeof(string), "The string to test.")]
        public static object IsNullOrWhiteSpace(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                return string.IsNullOrWhiteSpace(GetStringParam(parameters[0], context.Culture));
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Indicates whether the specified string is null or Empty string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates whether the specified string is null or Empty string.", 1)]
        [MacroMethodParam(0, "value", typeof(string), "The string to test.")]
        public static object IsNullOrEmpty(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                return string.IsNullOrEmpty(GetStringParam(parameters[0], context.Culture));
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Concatenates a specified separator String between each element of a specified String array, yielding a single concatenated string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Concatenates a specified separator String between each element of a specified String array (or list of objects), yielding a single concatenated string.", 2)]
        [MacroMethodParam(0, "separator", typeof(string), "Separator string.")]
        [MacroMethodParam(1, "list", typeof(IEnumerable), "IEnumerable to be joined (or list of items to be joined).")]
        public static object Join(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                string separator = GetStringParam(parameters[0], context.Culture);
                StringBuilder sb = new StringBuilder();

                for (int i = 1; i < parameters.Length; i++)
                {
                    if (i > 1)
                    {
                        // Append the separator in front of every item (except for the first one)
                        sb.Append(separator);
                    }

                    object val = parameters[i];
                    if ((val is IEnumerable) && !(val is string))
                    {
                        bool first = true;

                        // For enumerable objects, go inside
                        IEnumerable en = (IEnumerable)val;
                        foreach (object res in en)
                        {
                            if (!first)
                            {
                                sb.Append(separator);
                            }
                            sb.Append(res);
                            first = false;
                        }
                    }
                    else
                    {
                        sb.Append(val);
                    }
                }

                return sb.ToString();
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Formats given string using C# string.Format method.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Formats given value to requested format.", 2)]
        [MacroMethodParam(0, "format", typeof(object), "A composite format string.")]
        [MacroMethodParam(1, "parameters", typeof(string), "Formating parameter(s).")]
        public static object FormatString(EvaluationContext context, params object[] parameters)
        {
            parameters = UnwrapContainer(parameters);
            if (parameters.Length > 0)
            {
                string baseText = GetStringParam(parameters[0], context.Culture);
                object[] args = new object[parameters.Length - 1];
                for (int i = 1; i < parameters.Length; i++)
                {
                    args[i - 1] = parameters[i];
                }

                return string.Format(context.CultureInfo, baseText, args);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Generates Lorem Ipsum text of given length.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Generates Lorem Ipsum text of given length.", 0)]
        [MacroMethodParam(0, "length", typeof(int), "Length of the text.")]
        public static object LoremIpsum(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    // Overload without prameter
                    return LoremIpsumGenerator.GetTextByLength(1000);

                case 1:
                    // Overload with one parameter
                    return LoremIpsumGenerator.GetTextByLength(ValidationHelper.GetInteger(parameters[0], 1000));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}