using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Container for method extensions of an arbitrary object used by MacroEngine.
    /// </summary>
    public class MacroMethodContainer : MacroExtensionContainer<MacroMethodContainer, MacroMethod>
    {
        #region "Properties"

        /// <summary>
        /// Returns enumerable of all methods.
        /// </summary>
        public IEnumerable<MacroMethod> RegisteredMethods
        {
            get
            {
                return RegisteredExtensions;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns list of macro method extension registered for specified object.
        /// Returns null if there is no such extension for given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        public static IEnumerable<MacroMethod> GetMethodsForObject(object obj)
        {
            return GetExtensionsForObject(obj);
        }


        /// <summary>
        /// Returns macro method object of given name if registered for specified object.
        /// It loops through all MacroMethodContainer extensions of given object type.
        /// Returns null if there is no such Extension for given object.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <param name="name">Name of the method</param>
        public static MacroMethod GetMethodForObject(object obj, string name)
        {
            return GetExtensionForObject(obj, name);
        }


        /// <summary>
        /// Returns a method of given name (return null if specified method does not exist).
        /// </summary>
        /// <param name="name">Method name</param>
        public MacroMethod GetMethod(string name)
        {
            return GetExtension(name);
        }


        /// <summary>
        /// Registers the given method.
        /// </summary>
        /// <param name="method">Method to register</param>
        public void RegisterMethod(MacroMethod method)
        {
            RegisterExtension(method);
        }


        /// <summary>
        /// Registers the given methods.
        /// </summary>
        /// <param name="methods">Methods to register</param>
        public void RegisterMethods(params MacroMethod[] methods)
        {
            foreach (var method in methods)
            {
                RegisterMethod(method);
            }
        }


        /// <summary>
        /// Registers all the methods.
        /// </summary>
        protected virtual void RegisterMethods()
        {
            // Do nothing by default
        }


        /// <summary>
        /// Registers all the methods.
        /// </summary>
        protected override void RegisterExtensions()
        {
            base.RegisterExtensions();

            // Register standard way
            RegisterMethods();

            // Get methods registered through attributes
            var methods = GetType().GetMethods();

            foreach (MethodInfo m in methods)
            {
                // Register the methods
                RegisterMethods(GetMacroMethods(m).ToArray());
            }
        }
        

        /// <summary>
        /// Creates a macro method from the given method info. Returns null if the given method does not represent a macro method
        /// </summary>
        /// <param name="m">Method info</param>
        private static IEnumerable<MacroMethod> GetMacroMethods(MethodInfo m)
        {
            if (m != null)
            {
                var methodAttributes = m.GetCustomAttributes(typeof(MacroMethodAttribute), true);
                if (methodAttributes.Length > 0)
                {
                    // Get the parameters of the method, sort it according to their explicit order
                    var methodParams = m.GetCustomAttributes(typeof(MacroMethodParamAttribute), true);
                    var methodParameters = methodParams.Cast<MacroMethodParamAttribute>().ToList();

                    methodParameters.Sort((x, y) => x.Index - y.Index);

                    foreach (MacroMethodAttribute methodAtt in methodAttributes)
                    {
                        // Check type of parameters to know which type of MacroMethod we register
                        var paramaters = m.GetParameters();
                        var isStatic = m.IsStatic;

                        // Get the name of the method
                        var name = m.Name;
                        if (!string.IsNullOrEmpty(methodAtt.Name))
                        {
                            name = methodAtt.Name;
                        }

                        MacroMethod method = null;

                        if (isStatic && IsContextSignature(paramaters))
                        {
                            // Method with signature object(EvaluationContext, object[])
                            var func = (Func<EvaluationContext, object[], object>)Delegate.CreateDelegate(typeof(Func<EvaluationContext, object[], object>), m);

                            method = new MacroMethod(name, func);
                        }
                        else if (isStatic && IsResolverSignature(paramaters))
                        {
                            // Method with signature object(MacroResolver, object[])
                            var func = (Func<MacroResolver, object[], object>)Delegate.CreateDelegate(typeof(Func<MacroResolver, object[], object>), m);

                            method = new MacroMethod(name, func);
                        }
                        else if (isStatic && IsSimpleSignature(paramaters))
                        {
                            // Method with signature object(object[])
                            var func = (Func<object[], object>)Delegate.CreateDelegate(typeof(Func<object[], object>), m);

                            method = new MacroMethod(name, func);
                        }
                        
                        if (method != null)
                        {
                            // Set the rest of the properties from the attribute
                            method.Comment = methodAtt.Comment;
                            method.IsHidden = methodAtt.IsHidden;
                            method.MinimumParameters = methodAtt.MinimumParameters;
                            method.Snippet = methodAtt.Snippet;
                            method.Type = methodAtt.Type;
                            method.SpecialParameters = methodAtt.SpecialParameters;

                            // Add parameters of the method
                            foreach (var macroMethodParam in methodParameters)
                            {
                                method.AddParameter(macroMethodParam.GetMacroParam());
                            }

                            yield return method;
                        }
                    }
                }
            }
        }


        private static bool IsSimpleSignature(ParameterInfo[] parameters)
        {
            // Method with signature object(object[])
            return 
                (parameters.Length == 1) && 
                (parameters[0].ParameterType == typeof(object[]));
        }


        private static bool IsResolverSignature(ParameterInfo[] parameters)
        {
            // Method with signature object(MacroResolver, object[])
            return 
                (parameters.Length == 2) && 
                (parameters[0].ParameterType == typeof(MacroResolver));
        }


        private static bool IsContextSignature(ParameterInfo[] parameters)
        {
            // Method with signature object(EvaluationContext, object[])
            return 
                (parameters.Length == 2) && 
                (parameters[0].ParameterType == typeof(EvaluationContext)) && 
                (parameters[1].ParameterType == typeof(object[]));
        }
        
        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets the optional param of the given type
        /// </summary>
        /// <param name="parameters">List of method parameters</param>
        /// <param name="index">Parameter index</param>
        /// <param name="defaultValue">Default value in case conversion fails or parameter is not available</param>
        protected static T GetOptionalParam<T>(object[] parameters, int index, T defaultValue)
        {
            if (parameters.Length <= index)
            {
                return defaultValue;
            }

            return ValidationHelper.GetValue(parameters[index], defaultValue);
        }


        /// <summary>
        /// Returns the index-th parameter evaluated using given resolver.
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <param name="index">Index of the parameter within the array</param>
        /// <param name="defaultValue">Default value which will be used if the parameter is not present</param>
        protected static T GetParamValue<T>(object[] parameters, int index, T defaultValue)
        {
            T result = defaultValue;

            if ((parameters != null) && (parameters.Length > index) && (!(parameters[index] is CMSCacheDependency)))
            {
                result = ValidationHelper.GetValue<T>(parameters[index]);
            }

            return result;
        }


        /// <summary>
        /// Gets the lazy parameter value by evaluating it
        /// </summary>
        /// <param name="parameter">Parameter to evaluate</param>
        /// <param name="evalContext">Evaluation context</param>
        /// <param name="securityPassed">Indicates whether the security check passed</param>
        protected static object GetLazyParamValue(object parameter, EvaluationContext evalContext, out bool securityPassed)
        {
            var expr = parameter as MacroExpression;
            if (expr != null)
            {
                var evaluator = new ExpressionEvaluator(expr, evalContext);
                var result = evaluator.Evaluate();
                securityPassed = result.SecurityPassed;

                return result.Result;
            }

            securityPassed = true;
            return parameter;
        }


        /// <summary>
        /// Returns the parameter converted to boolean or false if parameter is not boolean.
        /// </summary>
        /// <param name="parameter">Parameter to convert</param>
        protected static bool GetBoolParam(object parameter)
        {
            return ValidationHelper.GetBoolean(parameter, false);
        }


        /// <summary>
        /// Returns the parameter converted to double or zero if parameter is not double.
        /// </summary>
        /// <param name="parameter">Parameter to convert</param>
        /// <param name="culture">Culture to use to convert object to double</param>
        protected static double GetDoubleParam(object parameter, string culture = null)
        {
            return ValidationHelper.GetDouble(parameter, 0, culture);
        }


        /// <summary>
        /// Returns the parameter converted to decimal or zero if parameter is not decimal.
        /// </summary>
        /// <param name="parameter">Parameter to convert</param>
        /// <param name="culture">Culture to use to convert object to double</param>
        protected static decimal GetDecimalParam(object parameter, string culture = null)
        {
            return ValidationHelper.GetDecimal(parameter, 0m, culture);
        }


        /// <summary>
        /// Returns the parameter converted to integer or zero if parameter is not integer.
        /// </summary>
        /// <param name="parameter">Parameter to convert</param>
        protected static int GetIntParam(object parameter)
        {
            return ValidationHelper.GetInteger(parameter, 0);
        }


        /// <summary>
        /// Returns the parameter converted to string or empty string.
        /// </summary>
        /// <param name="parameter">Parameter to convert</param>
        /// <param name="culture">Culture to use to convert object to string</param>
        protected static string GetStringParam(object parameter, string culture = null)
        {
            return ValidationHelper.GetString(parameter, "", culture);
        }


        /// <summary>
        /// Converts the parameter to GUID. Returns Guid.Empty if parameter is not GUID.
        /// </summary>
        /// <param name="parameter">Parameter to convert</param>
        protected static Guid GetGuidParam(object parameter)
        {
            return ValidationHelper.GetGuid(parameter, Guid.Empty);
        }

        #endregion


        #region "Internal registration methods"

        /// <summary>
        /// Registers the given method within the method table.
        /// </summary>
        /// <param name="name">Method name</param>
        /// <param name="method">Method delegate</param>
        /// <param name="type">Return type of the method</param>
        /// <param name="comment">Comment for the method</param>
        /// <param name="minimumParameters">Minimal number of parameters needed to call the method</param>
        /// <param name="parameterDefinition">Parameter definition in format {{name, type, comment}, {name, type, comment}}</param>
        /// <param name="specialParameters">A list of special parameters needed to be supplied by resolver</param>
        protected void RegisterMethodInternal(string name, Func<EvaluationContext, object[], object> method, Type type, string comment, int minimumParameters, object[,] parameterDefinition, string[] specialParameters = null)
        {
            // Create the object
            MacroMethod methodToReg = new MacroMethod(name, method)
            {
                Type = type,
                Comment = comment,
                MinimumParameters = minimumParameters,
                SpecialParameters = specialParameters
            };

            AddParametersAndRegister(parameterDefinition, methodToReg);
        }


        /// <summary>
        /// Adds the parameters from definition and registres the method to the system.
        /// </summary>
        /// <param name="parameterDefinition">Parameter definition</param>
        /// <param name="methodToReg">Method to register</param>
        private void AddParametersAndRegister(object[,] parameterDefinition, MacroMethod methodToReg)
        {
            // Add the parameters
            if (parameterDefinition != null)
            {
                for (int i = 0; i <= parameterDefinition.GetUpperBound(0); i++)
                {
                    string paramName = parameterDefinition[i, 0].ToString();
                    string paramComment = parameterDefinition[i, 2].ToString();
                    Type paramType = (Type)parameterDefinition[i, 1];
                    bool isParams = false;
                    if (parameterDefinition.GetUpperBound(1) == 3)
                    {
                        isParams = (bool)parameterDefinition[i, 3];
                    }

                    methodToReg.AddParameter(paramName, paramType, paramComment, isParams);
                }
            }

            // Register the method
            RegisterMethod(methodToReg);
        }


        /// <summary>
        /// Registers the given method within the method table.
        /// </summary>
        /// <param name="names">Method names to register</param>
        /// <param name="method">Method delegate</param>
        /// <param name="type">Return type of the method</param>
        /// <param name="comment">Comment for the method</param>
        /// <param name="minimumParameters">Minimal number of parameters needed to call the method</param>
        /// <param name="parameterDefinition">Parameter definition in format {{name, type, comment}, {name, type, comment}}</param>
        /// <param name="specialParameters">A list of special parameters needed to be supplied by resolver</param>
        protected void RegisterMethodInternal(string[] names, Func<EvaluationContext, object[], object> method, Type type, string comment, int minimumParameters, object[,] parameterDefinition, string[] specialParameters = null)
        {
            foreach (string name in names)
            {
                RegisterMethodInternal(name, method, type, comment, minimumParameters, parameterDefinition, specialParameters);
            }
        }

        #endregion


        #region "Helper comparison methods"

        /// <summary>
        /// Returns true if first parameter is lower than or equal to second.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parametersToCompare">First two parameters are numbers to compare</param>
        /// <param name="op">Operator</param>
        protected static object CompareValues(EvaluationContext context, string op, params object[] parametersToCompare)
        {
            object[] parameters = UnwrapContainer(parametersToCompare);

            // Keep the compare priority, double comparison must be first
            if (ValidationHelper.IsDouble(parameters[0], context.Culture) && ValidationHelper.IsDouble(parameters[1], context.Culture))
            {
                double number1 = GetDoubleParam(parameters[0], context.Culture);
                double number2 = GetDoubleParam(parameters[1], context.Culture);
                return CompareDouble(number1, number2, op);
            }
            else if (ValidationHelper.IsDateTime(parameters[0]) && ValidationHelper.IsDateTime(parameters[1]))
            {
                DateTime date1 = ValidationHelper.GetDateTime(parameters[0], DateTimeHelper.ZERO_TIME);
                DateTime date2 = ValidationHelper.GetDateTime(parameters[1], DateTimeHelper.ZERO_TIME);
                return CompareDateTime(date1, date2, op);
            }
            else if ((parameters[0] is TimeSpan) && (parameters[1] is TimeSpan))
            {
                TimeSpan span1 = (TimeSpan)parameters[0];
                TimeSpan span2 = (TimeSpan)parameters[1];
                return CompareTimeSpan(span1, span2, op);
            }
            else
            {
                // Compare as strings
                string str1 = GetStringParam(parameters[0], context.Culture);
                string str2 = GetStringParam(parameters[1], context.Culture);
                return CompareString(str1, str2, op, context);
            }

        }


        /// <summary>
        /// Unwraps all DateTime and TimeSpan containers to their original system value.
        /// </summary>
        /// <param name="parameters">Parameters to process</param>
        protected static object[] UnwrapContainer(object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is TimeSpanContainer)
                {
                    parameters[i] = ((TimeSpanContainer)parameters[i]).TimeSpan;
                }
                else if (parameters[i] is DateTimeContainer)
                {
                    parameters[i] = ((DateTimeContainer)parameters[i]).DateTime;
                }
            }
            return parameters;
        }


        /// <summary>
        /// Compares strings (lexicographically).
        /// </summary>
        /// <param name="str1">String to compare</param>
        /// <param name="str2">String to compare</param>
        /// <param name="op">Comparison operator</param>
        /// <param name="context">Evaluation context</param>
        protected static object CompareString(string str1, string str2, string op, EvaluationContext context)
        {
            switch (op.ToLowerCSafe())
            {
                case "<":
                    return String.Compare(str1, str2, !context.CaseSensitive, context.CultureInfo) < 0;
                case ">":
                    return String.Compare(str1, str2, !context.CaseSensitive, context.CultureInfo) > 0;
                case "<=":
                    return String.Compare(str1, str2, !context.CaseSensitive, context.CultureInfo) <= 0;
                case ">=":
                    return String.Compare(str1, str2, !context.CaseSensitive, context.CultureInfo) >= 0;
            }
            return false;
        }


        /// <summary>
        /// Compares double numbers.
        /// </summary>
        /// <param name="number1">Number to compare</param>
        /// <param name="number2">Number to compare</param>
        /// <param name="op">Comparison operator</param>
        protected static object CompareDouble(double number1, double number2, string op)
        {
            switch (op.ToLowerCSafe())
            {
                case "<":
                    return number1 < number2;
                case ">":
                    return number1 > number2;
                case "<=":
                    return number1 <= number2;
                case ">=":
                    return number1 >= number2;
            }
            return false;
        }


        /// <summary>
        /// Compares DateTime values.
        /// </summary>
        /// <param name="date1">DateTime to compare</param>
        /// <param name="date2">DateTime to compare</param>
        /// <param name="op">Comparison operator</param>
        protected static object CompareDateTime(DateTime date1, DateTime date2, string op)
        {
            switch (op.ToLowerCSafe())
            {
                case "<":
                    return DateTime.Compare(date1, date2) < 0;
                case ">":
                    return DateTime.Compare(date1, date2) > 0;
                case "<=":
                    return DateTime.Compare(date1, date2) <= 0;
                case ">=":
                    return DateTime.Compare(date1, date2) >= 0;
            }
            return false;
        }


        /// <summary>
        /// Compares TimeSpan values.
        /// </summary>
        /// <param name="span1">TimeSpan to compare</param>
        /// <param name="span2">TimeSpan to compare</param>
        /// <param name="op">Comparison operator</param>
        protected static object CompareTimeSpan(TimeSpan span1, TimeSpan span2, string op)
        {
            switch (op.ToLowerCSafe())
            {
                case "<":
                    return TimeSpan.Compare(span1, span2) < 0;
                case ">":
                    return TimeSpan.Compare(span1, span2) > 0;
                case "<=":
                    return TimeSpan.Compare(span1, span2) <= 0;
                case ">=":
                    return TimeSpan.Compare(span1, span2) >= 0;
            }
            return false;
        }

        #endregion
    }
}