using System;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide basic system methods in the MacroEngine.
    /// </summary>
    internal class SystemMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns object ID from code name (GUID respectively) and site name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// Object type of the object;
        /// Code name or GUID of the object;
        /// Site name, if null or empty, global objects are retrieved (optional if the object is not site object);
        /// Allow global objects (if site object is not found, global object with given name is retrieved);
        /// </param>
        [MacroMethod(typeof(int), "Returns object ID from code name (GUID respectively) and site name.", 2)]
        [MacroMethodParam(0, "objectType", typeof(string), "Object type of the object.")]
        [MacroMethodParam(1, "codeNameOrGuid", typeof(string), "Code name or GUID of the object.")]
        [MacroMethodParam(2, "siteName", typeof(string), "Site name, if null or empty, global objects are retrieved (optional if the object is not site object).")]
        [MacroMethodParam(3, "allowGlobal", typeof(bool), "Allow global objects (if site object is not found, global object with given name is retrieved).")]
        public static object GetObjectID(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                string objectType = GetStringParam(parameters[0], context.Culture);

                if (!string.IsNullOrEmpty(objectType))
                {
                    var info = ModuleManager.GetObject(objectType);
                    if (info != null)
                    {
                        string where = null;
                        Guid guid = ValidationHelper.GetGuid(parameters[1], Guid.Empty);

                        var ti = info.TypeInfo;

                        if ((guid != Guid.Empty) && (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                        {
                            where = ti.GUIDColumn + " = '" + guid + "'";
                        }
                        else
                        {
                            string codeName = GetStringParam(parameters[1], context.Culture);
                            if (!string.IsNullOrEmpty(codeName))
                            {
                                where = info.Generalized.CodeNameColumn + " = N'" + SqlHelper.GetSafeQueryString(codeName, false) + "'";
                            }
                        }

                        if ((parameters.Length > 2) && (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                        {
                            string siteWhere = null;
                            string siteName = GetStringParam(parameters[2], context.Culture);

                            if (!string.IsNullOrEmpty(siteName))
                            {
                                int siteId = 0;
                                var site = ProviderHelper.GetInfoByName(PredefinedObjectType.SITE, siteName);
                                if (site != null)
                                {
                                    siteId = site.Generalized.ObjectID;
                                }
                                siteWhere = ti.SiteIDColumn + " = " + siteId;
                            }

                            if (parameters.Length > 3)
                            {
                                // Allow global object if requested
                                if (GetBoolParam(parameters[3]))
                                {
                                    siteWhere = SqlHelper.AddWhereCondition(siteWhere, ti.SiteIDColumn + " IS NULL", "OR");
                                }
                            }

                            where = SqlHelper.AddWhereCondition(where, siteWhere);
                        }

                        info = info.Generalized.GetObject(where);

                        if (info != null)
                        {
                            return info.Generalized.ObjectID;
                        }
                    }
                }

                return null;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// If arguments are numbers than it returns sum of them. Otherwise returns concatenation of string representations.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "If arguments are numbers than it returns sum of them. Otherwise returns concatenation of string representations.", 2, Name = "+")]
        [MacroMethod(typeof(double), "Unary plus.", 1, Name = "u+")]
        [MacroMethodParam(0, "parameters", typeof(double), "List of numbers to multiply.")]
        public static object Add(EvaluationContext context, params object[] parameters)
        {
            // First check if everything is number, if so, than sum it up otherwise concatenate strings
            bool numbers = true;
            foreach (object obj in parameters)
            {
                if ((obj is string) || !ValidationHelper.IsDouble(obj))
                {
                    numbers = false;
                    break;
                }
            }
            if (numbers)
            {
                // Summation
                double result = 0;
                foreach (object obj in parameters)
                {
                    result += GetDoubleParam(obj, context.Culture);
                }
                return result;
            }
            else
            {
                object[] unwrapParams = UnwrapContainer(parameters);

                bool isDateTime = true;

                TimeSpan span = TimeSpan.MinValue;
                DateTime dateTime = DateTime.MinValue;

                // Sum all DateTimes and TimeSpans
                foreach (object param in unwrapParams)
                {
                    if (param is DateTime)
                    {
                        if (dateTime == DateTime.MinValue)
                        {
                            dateTime = (DateTime)param;
                        }
                        else
                        {
                            // Cannot add two date times
                            throw new NotSupportedException();
                        }
                    }
                    else if (param is TimeSpan)
                    {
                        if (span == TimeSpan.MinValue)
                        {
                            span = (TimeSpan)param;
                        }
                        else
                        {
                            span = span.Add((TimeSpan)param);
                        }
                    }
                    else
                    {
                        isDateTime = false;
                    }
                }

                // If some of the values were time, return as time
                if (isDateTime)
                {
                    if ((dateTime != DateTime.MinValue) && (span != TimeSpan.MinValue))
                    {
                        return dateTime.Add(span);
                    }

                    if (dateTime != DateTime.MinValue)
                    {
                        return dateTime;
                    }

                    if (span != TimeSpan.MinValue)
                    {
                        return span;
                    }
                }

                // Default processing - concatenation
                string result = "";

                foreach (object obj in parameters)
                {
                    result += GetStringParam(obj, context.Culture);
                }

                return result;
            }
        }


        /// <summary>
        /// Subtracts two number values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Subtracts two number values.", 2, Name = "-")]
        [MacroMethod(typeof(double), "Unary minus.", 1, Name = "u-")]
        [MacroMethodParam(0, "left", typeof(double), "Left operand.")]
        [MacroMethodParam(0, "right", typeof(double), "Right operand.")]
        public static object Subtract(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Unary minus
                    return -GetDoubleParam(parameters[0], context.Culture);

                case 2:
                    // Check if we are not processing date/time values
                    object[] unwrapParams = UnwrapContainer(parameters);
                    if (unwrapParams[0] is DateTime)
                    {
                        if (unwrapParams[1] is DateTime)
                        {
                            return ((DateTime)unwrapParams[0]).Subtract((DateTime)unwrapParams[1]);
                        }

                        if (unwrapParams[1] is TimeSpan)
                        {
                            return ((DateTime)unwrapParams[0]).Subtract((TimeSpan)unwrapParams[1]);
                        }
                    }
                    else if ((unwrapParams[0] is TimeSpan) && (unwrapParams[1] is TimeSpan))
                    {
                        return ((TimeSpan)unwrapParams[0]).Subtract((TimeSpan)unwrapParams[1]);
                    }

                    // Normal subtraction
                    double number1 = GetDoubleParam(parameters[0], context.Culture);
                    double number2 = GetDoubleParam(parameters[1], context.Culture);

                    return number1 - number2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns product of the parameters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns product of the parameters.", 1, Name = "*")]
        [MacroMethodParam(0, "parameters", typeof(double), "List of numbers to multiply.")]
        public static object Multiply(EvaluationContext context, params object[] parameters)
        {
            // Summation
            double result = 1;
            foreach (object obj in parameters)
            {
                if (!ValidationHelper.IsDouble(obj))
                {
                    // Wrong type, return null
                    return null;
                }
                result *= GetDoubleParam(obj, context.Culture);
            }
            return result;
        }


        /// <summary>
        /// Divides two number values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Divides two number values.", 2, Name = "/")]
        [MacroMethodParam(0, "left", typeof(double), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(double), "Right operand.")]
        public static object Divide(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported
                    double number1 = GetDoubleParam(parameters[0], context.Culture);
                    double number2 = GetDoubleParam(parameters[1], context.Culture);

                    return number1 / number2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if first parameter is greater than second.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if first parameter is greater than second.", 2, Name = ">")]
        [MacroMethodParam(0, "left", typeof(double), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(double), "Right operand.")]
        public static object GreaterThan(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return CompareValues(context, ">", parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if first parameter is lower than second.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if first parameter is lower than second.", 2, Name = "<")]
        [MacroMethodParam(0, "left", typeof(double), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(double), "Right operand.")]
        public static object LowerThan(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported
                    return CompareValues(context, "<", parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if first parameter is greater than or equal to second.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if first parameter is greater than or equal to second.", 2, Name = ">=")]
        [MacroMethodParam(0, "left", typeof(double), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(double), "Right operand.")]
        public static object GreaterThanOrEqual(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    return CompareValues(context, ">=", parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if first parameter is lower than or equal to second.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if first parameter is lower than or equal to second.", 2, Name = "<=")]
        [MacroMethodParam(0, "left", typeof(double), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(double), "Right operand.")]
        public static object LowerThanOrEqual(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    return CompareValues(context, "<=", parameters);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns 0.01 multiple of first argument.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns 0.01 multiple of first argument.", 1, Name = "%")]
        [MacroMethodParam(0, "percent", typeof(double), "Number of percent (number to be multiplied by 0.01).")]
        public static object Percent(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return GetDoubleParam(parameters[0], context.Culture) * 0.01;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns modulo of two values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns modulo of two values.", 2, Name = "mod")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.")]
        public static object Modulo(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    int number1 = GetIntParam(parameters[0]);
                    int number2 = GetIntParam(parameters[1]);

                    return number1 % number2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a bitwise complement.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns a bitwise complement.", 1, Name = "~")]
        [MacroMethodParam(0, "number", typeof(int), "Number to do the operation on.")]
        public static object BitwiseComplement(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return ~GetIntParam(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns logical product of given parameters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns logical product of given parameters.", 2, Name = "and")]
        [MacroMethod(typeof(int), "Returns logical product of given parameters.", 2, Name = "&&")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.", AsExpression = true)]
        public static object And(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                // All parameters must return true
                bool securityPassed;
                return parameters.All(item => GetBoolParam(GetLazyParamValue(item, context, out securityPassed)) && securityPassed);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns logical addition of given parameters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns logical addition of given parameters.", 2, Name = "or")]
        [MacroMethod(typeof(int), "Returns logical addition of given parameters.", 2, Name = "||")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.", AsExpression = true)]
        public static object Or(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                // If any returns true, the whole expression returns true
                bool securityPassed;
                return parameters.Any(item => GetBoolParam(GetLazyParamValue(item, context, out securityPassed)) && securityPassed);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns logical negation of the provided value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns logical negation of given parameter.", 1, Name = "not")]
        [MacroMethod(typeof(int), "Returns logical negation of given parameter.", 1, Name = "!")]
        [MacroMethodParam(0, "value", typeof(int), "Value to negate.")]
        public static object Not(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return !GetBoolParam(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns logical AND of given parameters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns logical AND of given parameters.", 2, Name = "&")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.")]
        public static object LogicalAnd(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported
                    {
                        int arg1 = GetIntParam(parameters[0]);
                        int arg2 = GetIntParam(parameters[1]);

                        return arg1 & arg2;
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns logical OR of given parameters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns logical OR of given parameters.", 2, Name = "|")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.")]
        public static object LogicalOr(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    int arg1 = GetIntParam(parameters[0]);
                    int arg2 = GetIntParam(parameters[1]);

                    return arg1 | arg2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns logical XOR of given parameters.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns logical XOR of given parameters.", 2, Name = "^")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.")]
        public static object LogicalXor(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    int arg1 = GetIntParam(parameters[0]);
                    int arg2 = GetIntParam(parameters[1]);

                    return arg1 ^ arg2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Shifts its first operand left by the number of bits specified by its second operand.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Shifts its first operand left by the number of bits specified by its second operand.", 2, Name = "<<")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.")]
        public static object LeftShift(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    int arg1 = GetIntParam(parameters[0]);
                    int arg2 = GetIntParam(parameters[1]);

                    return arg1 << arg2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Shifts its first operand right by the number of bits specified by its second operand.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Shifts its first operand right by the number of bits specified by its second operand.", 2, Name = ">>")]
        [MacroMethodParam(0, "left", typeof(int), "Left operand.")]
        [MacroMethodParam(1, "right", typeof(int), "Right operand.")]
        public static object RightShift(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    int arg1 = GetIntParam(parameters[0]);
                    int arg2 = GetIntParam(parameters[1]);

                    return arg1 >> arg2;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Formats given object to requested format.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Formats given value to requested format.", 2)]
        [MacroMethodParam(0, "formattedObject", typeof(object), "Object to format.")]
        [MacroMethodParam(1, "format", typeof(string), "Formatting string.")]
        public static object Format(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    // Only two parameters are supported
                    parameters = UnwrapContainer(parameters);
                    return ValidationHelper.GetString(parameters[0], "", context.Culture, GetStringParam(parameters[1], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Logs given items to MacroDebug.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Logs given item(s) to Macro debug.", 1)]
        [MacroMethodParam(0, "object", typeof(object), "Item to log.")]
        public static object LogToDebug(EvaluationContext context, params object[] parameters)
        {
            foreach (object item in parameters)
            {
                object itemToLog = item;

                if (item != null)
                {
                    if (item is BaseInfo)
                    {
                        itemToLog = ((BaseInfo)item).Generalized.ObjectCodeName;
                    }
                    else if (item is GeneralizedInfo)
                    {
                        itemToLog = ((GeneralizedInfo)item).ObjectCodeName;
                    }
                }
                if (itemToLog == null)
                {
                    itemToLog = "null";
                }

                MacroDebug.LogMacroOperation("LogToDebug", itemToLog, MacroDebug.CurrentLogIndent);
            }

            if (parameters.Length == 1)
            {
                return parameters[0];
            }

            return parameters;
        }


        /// <summary>
        /// Processes the custom macro.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Processes the custom macro.", 2, IsHidden = true)]
        [MacroMethodParam(0, "expression", typeof(string), "Expression without parameters.")]
        [MacroMethodParam(1, "parameters", typeof(string), "Parameters of the expression concatenated together.")]
        public static object ProcessCustomMacro(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    EvaluationResult result = context.Resolver.ResolveCustomMacro(GetStringParam(parameters[0], context.Culture), GetStringParam(parameters[0], context.Culture) + GetStringParam(parameters[1], context.Culture));
                    if ((result != null) && result.Match)
                    {
                        return result.Result;
                    }
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Method used in macro rule editor. Contains information to reconstruct the design of the expression. When evaluated, the first parameter evaluation is returned.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Method used in macro rule editor. Contains information to reconstruct the design of the expression. When evaluated, the first parameter is returned.", 2, IsHidden = true)]
        [MacroMethodParam(0, "expression", typeof(object), "Any expression - this will be evaluated and returned as a result.")]
        [MacroMethodParam(1, "xml", typeof(object), "Macro rule designer representation.")]
        public static object Rule(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length == 2)
            {
                var toEval = GetStringParam(parameters[0], context.Culture);

                // If integrity of the outer macro is fine, it means that the user with which the macro is signed
                // is correct and it is the user under which the inner macros in the method should be evaluated
                // So sign the inner macro with same user.
                if (context.IntegrityPassed)
                {
                    if (!string.IsNullOrEmpty(context.IdentityName) || !string.IsNullOrEmpty(context.UserName))
                    {
                        toEval = MacroSecurityProcessor.AddMacroSecurityParams(toEval, new MacroIdentityOption { IdentityName = context.IdentityName, UserName = context.UserName });
                    }
                }
                else
                {
                    // Outer macro is not correctly signed, log the failure to event log
                    MacroDebug.LogSecurityCheckFailure(context.OriginalExpression, context.UserName, context.IdentityName, context.User?.UserName);
                    return null;
                }

                var result = context.Resolver.ResolveMacroExpression(toEval);
                if (result != null)
                {
                    return result.Result;
                }

                return null;
            }
            throw new NotSupportedException();
        }


        /// <summary>
        /// Checks if module is loaded.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Checks if module is loaded.", 1)]
        [MacroMethodParam(0, "moduleName", typeof(string), "Code name of the module.")]
        public static object IsModuleLoaded(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    // Check if module is loaded
                    return ModuleEntryManager.IsModuleLoaded(GetStringParam(parameters[0], context.Culture));

                default:
                    // No other overload is supported
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets object of specified type with specified ID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(BaseInfo), "Gets object of specified type with specified ID.", 2)]
        [MacroMethodParam(0, "objectType", typeof(string), "Object type.")]
        [MacroMethodParam(1, "objectIdentifier", typeof(object), "Object ID, GUID or code name.")]
        [MacroMethodParam(2, "siteId", typeof(int), "Site ID of object (not needed when object is specified by ID).")]
        public static object GetObject(EvaluationContext context, params object[] parameters)
        {
            if ((parameters.Length < 2) || (parameters.Length > 3))
            {
                throw new NotSupportedException();
            }

            // First try identification by ID
            var objectType = GetStringParam(parameters[0], context.Culture);
            var id = GetIntParam(parameters[1]);
            if (id > 0)
            {
                return ProviderHelper.GetInfoById(objectType, id);
            }

            switch (parameters.Length)
            {
                case 2:
                    {
                        // If it's not ID, it could be GUID.
                        var guid = GetGuidParam(parameters[1]);
                        if (guid != Guid.Empty)
                        {
                            return ProviderHelper.GetInfoByGuid(objectType, guid);
                        }

                        // If it's not GUID, it must be code name.
                        return ProviderHelper.GetInfoByName(objectType, GetStringParam(parameters[1]));
                    }

                // Is used when siteId is specified.
                case 3:
                    {
                        var siteId = GetIntParam(parameters[2]);

                        // If it's not ID, it could be GUID.
                        var guid = GetGuidParam(parameters[1]);
                        if (guid != Guid.Empty)
                        {
                            return ProviderHelper.GetInfoByGuid(objectType, guid, siteId);
                        }

                        // If it's not GUID, it must be code name.
                        return ProviderHelper.GetInfoByName(objectType, GetStringParam(parameters[1]), siteId);
                    }
            }

            return null;
        }


        /// <summary>
        /// Gets the nice objext type name for specified type.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(BaseInfo), "Gets the nice objext type name for specified object type.", 1)]
        [MacroMethodParam(0, "objectType", typeof(string), "Object type.")]
        public static object GetObjectTypeName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    string objType = GetStringParam(parameters[0], "");
                    return TypeHelper.GetNiceObjectTypeName(objType);

                default:
                    // No other overload is supported
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Indicates if the system uses forms authentication
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates if the system uses forms authentication.", 0, IsHidden = true)]
        public static object IsFormsAuthentication(EvaluationContext context, params object[] parameters)
        {
            return RequestHelper.IsFormsAuthentication();
        }


        /// <summary>
        /// Indicates if the system uses windows authentication
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates if the system uses windows authentication.", 0, IsHidden = true)]
        public static object IsWindowsAuthentication(EvaluationContext context, params object[] parameters)
        {
            return RequestHelper.IsWindowsAuthentication();
        }


        /// <summary>
        /// Retrieves all object types registered in static type info objects in the system
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IDataQuery), "Retrieves all object types registered in static type info objects in the system.", 0, IsHidden = true)]
        public static object GetRegisteredObjectTypes(EvaluationContext context, params object[] parameters)
        {
            var dataset = new DataSet();

            // Define Object type's table
            var table = new DataTable();
            table.Columns.Add("DisplayName");
            table.Columns.Add("ObjectType");
            table.Columns.Add("IsBinding", typeof(bool));

            dataset.Tables.Add(table);

            // Get all object types with its display name which are not listing type
            var infoObjectTypes = ObjectTypeManager.RegisteredTypes
                                        .Where(t => !ObjectTypeManager.ListObjectTypes.Contains(t.ObjectType))
                                        .Select(t => new
                                        {
                                            DisplayName = TypeHelper.GetNiceObjectTypeName(t.ObjectType),
                                            t.ObjectType,
                                            t.IsBinding
                                        });

            foreach (var infoObjectType in infoObjectTypes)
            {
                var row = table.NewRow();
                row["DisplayName"] = infoObjectType.DisplayName;
                row["ObjectType"] = infoObjectType.ObjectType;
                row["IsBinding"] = infoObjectType.IsBinding;
                table.Rows.Add(row);
            }

            // Create data query for unigrid
            var query = new DataQuery().WithSource(new MemoryDataQuerySource(dataset));
            
            return query;
        }


        /// <summary>
        /// Formats string based on value parameter. If it is greater then zero 
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>        
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "If value (first argument) is greater then zero returns formated 'format' parameter otherwise returns 'lessetThenZeroFormat' parameter.", 3)]
        [MacroMethodParam(0, "value", typeof(string), "Value formating is based on.")]
        [MacroMethodParam(1, "format", typeof(string), "A composite format string. Applied when value is greater then zero.")]
        [MacroMethodParam(2, "lesserThenZeroFormat", typeof(string), "Format string for cases, value is zero or lesser.")]
        public static object FormatId(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3:
                    int value = GetIntParam(parameters[0]);

                    if (value <= 0)
                    {
                        // If value is lesser then zero, returns lesserThenZeroFormat result (if exists)
                        if (parameters.Length > 2)
                        {
                            return String.Format(GetStringParam(parameters[2]), value);
                        }

                        return null;
                    }

                    return string.Format(GetStringParam(parameters[1]), value);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns list of all field types for the given object type
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns list of all field types for the given object type.", 0, IsHidden = true)]
        [MacroMethodParam(0, "objectType", typeof(string), "Object type, if not specified or null, all field types are returned.")]
        [MacroMethodParam(1, "onlyVisible", typeof(bool), "If true, only visible field types are provided.")]
        public static object GetFieldTypes(EvaluationContext context, params object[] parameters)
        {
            var objectType = GetOptionalParam(parameters, 0, (string)null);
            var onlyVisible = GetOptionalParam(parameters, 1, true);

            return DataTypeManager.GetFieldTypes(objectType, onlyVisible);
        }


        /// <summary>
        /// Gets the field groups available in the system
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Gets the field groups available in the system.", 0, IsHidden = true)]
        public static object GetFieldGroups(EvaluationContext context, params object[] parameters)
        {
            return DataTypeManager.GetFieldGroups();
        }
    }
}