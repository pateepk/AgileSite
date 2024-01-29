using System;

using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Base;

/// <summary>
/// Example of custom module with custom macro methods registration.
/// </summary>
public class CustomMacroMethods : MacroMethodContainer
{
    #region "Macro methods implementation"

    /// <summary>
    /// Concatenates the given string with " default" string.
    /// </summary>
    /// <param name="param1">String to be concatenated with " default"</param>
    public static string MyMethod(string param1)
    {
        return MyMethod(param1, "default");
    }


    /// <summary>
    /// Concatenates two strings.
    /// </summary>
    /// <param name="param1">First string to concatenate</param>
    /// <param name="param2">Second string to concatenate</param>
    public static string MyMethod(string param1, string param2)
    {
        return param1 + " " + param2;
    }


    // Add your own custom methods here

    #endregion


    #region "MacroResolver wrapper methods"

    /// <summary>
    /// Wrapper method of MyMethod suitable for MacroResolver.
    /// </summary>
    /// <param name="context">Evaluation context with child resolver</param>
    /// <param name="parameters">Parameters of the method</param>
    [MacroMethod(typeof(string), "Returns concatenation of two strings.", 1)]
    [MacroMethodParam(0, "param1", typeof(string), "First string to concatenate.")]
    [MacroMethodParam(1, "param2", typeof(string), "Second string to concatenate.")]
    public static object MyMethod(EvaluationContext context, params object[] parameters)
    {
        switch (parameters.Length)
        {
            case 1:
                // Overload with one parameter
                return MyMethod(ValidationHelper.GetString(parameters[0], ""));

            case 2:
                // Overload with two parameters
                return MyMethod(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""));

            default:
                // No other overload is supported
                throw new NotSupportedException();
        }
    }


    /// <summary>
    /// Compares two strings according to resolver IsCaseSensitiveComparison setting.
    /// </summary>
    /// <param name="context">Evaluation context with child resolver</param>
    /// <param name="parameters">Parameters of the method</param>
    [MacroMethod(typeof(string), "Compares two strings according to resolver IsCaseSensitiveComparison setting.", 2)]
    [MacroMethodParam(0, "param1", typeof(string), "First string to compare.")]
    [MacroMethodParam(1, "param2", typeof(string), "Second string to compare.")]
    public static object MyComparisonMethod(EvaluationContext context, params object[] parameters)
    {
        switch (parameters.Length)
        {
            case 2:
                // Overload with two parameters
                return ValidationHelper.GetString(parameters[0], "").EqualsCSafe(ValidationHelper.GetString(parameters[1], ""), !context.CaseSensitive);

            default:
                // No other overload is supported
                throw new NotSupportedException();
        }
    }


    // Add wrappers for MacroResolver for your own custom methods here
    // The signature of wrapper methods has to be "public static object MyMethodName(EvaluationContext context, params object[] parameters)"

    #endregion
}