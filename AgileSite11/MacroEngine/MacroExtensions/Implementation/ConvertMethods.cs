using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide conversion methods in the MacroEngine.
    /// </summary>
    internal class ConvertMethods : MacroMethodContainer
    {
        /// <summary>
        /// Converts value to int, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Converts value to int, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(int), "Default value.")]
        public static object ToInt(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return 0;

                case 1:
                    return GetIntParam(parameters[0]);

                case 2:
                    return ValidationHelper.GetInteger(parameters[0], GetIntParam(parameters[1]));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to bool, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Converts value to bool, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(bool), "Default value.")]
        public static object ToBool(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return false;

                case 1:
                    return GetBoolParam(parameters[0]);

                case 2:
                    return ValidationHelper.GetBoolean(parameters[0], GetBoolParam(parameters[1]));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to BaseInfo, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(BaseInfo), "Converts value to BaseInfo, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(BaseInfo), "Default value.")]
        public static object ToBaseInfo(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length == 0)
            {
                return null;
            }
            else if (parameters.Length > 0)
            {
                BaseInfo result = parameters[0] as BaseInfo;

                if ((result == null) && (parameters.Length == 2))
                {
                    return parameters[1] as BaseInfo;
                }

                return result;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Converts value to double, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Converts value to double, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(double), "Default value.")]
        [MacroMethodParam(2, "culture", typeof(string), "Culture to use.")]
        public static object ToDouble(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return 0;

                case 1:
                    return ValidationHelper.GetDouble(parameters[0], 0, context.Culture);

                case 2:
                    return ValidationHelper.GetDouble(parameters[0], GetDoubleParam(parameters[1], context.Culture), context.Culture);

                case 3:
                    return ValidationHelper.GetDouble(parameters[0], GetDoubleParam(parameters[1], context.Culture), GetStringParam(parameters[2], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to decimal, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(decimal), "Converts value to decimal, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(decimal), "Default value.")]
        [MacroMethodParam(2, "culture", typeof(string), "Culture to use.")]
        public static object ToDecimal(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return 0;

                case 1:
                    return ValidationHelper.GetDecimal(parameters[0], 0m, context.Culture);

                case 2:
                    return ValidationHelper.GetDecimal(parameters[0], GetDecimalParam(parameters[1], context.Culture), context.Culture);

                case 3:
                    return ValidationHelper.GetDecimal(parameters[0], GetDecimalParam(parameters[1], context.Culture), GetStringParam(parameters[2], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to string, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Converts value to string, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(string), "Default value.")]
        [MacroMethodParam(2, "culture", typeof(string), "Culture to use.")]
        [MacroMethodParam(3, "format", typeof(string), "Format string.")]
        public static object ToString(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return "";

                case 1:
                    return ValidationHelper.GetString(parameters[0], "", context.Culture);

                case 2:
                    return ValidationHelper.GetString(parameters[0], GetStringParam(parameters[1], context.Culture), context.Culture);

                case 3:
                    return ValidationHelper.GetString(parameters[0], GetStringParam(parameters[1], context.Culture), GetStringParam(parameters[2], context.Culture));

                case 4:
                    return ValidationHelper.GetString(parameters[0], GetStringParam(parameters[1], context.Culture), GetStringParam(parameters[2], context.Culture), GetStringParam(parameters[3], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to Guid, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(Guid), "Converts value to Guid, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(Guid), "Default value.")]
        public static object ToGuid(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return Guid.Empty;

                case 1:
                    return ValidationHelper.GetGuid(parameters[0], Guid.Empty);

                case 2:
                    return ValidationHelper.GetGuid(parameters[0], ValidationHelper.GetGuid(parameters[1], Guid.Empty));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to DateTime, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Converts value to DateTime, if it is not possible, returns default value.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(DateTime), "Default value.")]
        [MacroMethodParam(2, "culture", typeof(string), "Culture to use.")]
        public static object ToDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return DateTimeHelper.ZERO_TIME;

                case 1:
                    return ValidationHelper.GetDateTime(parameters[0], DateTimeHelper.ZERO_TIME, context.Culture);

                case 2:
                    return ValidationHelper.GetDateTime(parameters[0], ValidationHelper.GetDateTime(parameters[1], DateTimeHelper.ZERO_TIME), context.Culture);

                case 3:
                    return ValidationHelper.GetDateTime(parameters[0], ValidationHelper.GetDateTime(parameters[1], DateTimeHelper.ZERO_TIME), GetStringParam(parameters[2], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to DateTime string representation in EN culture.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Converts value to DateTime in EN culture.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        [MacroMethodParam(1, "defaultValue", typeof(DateTime), "Default value.")]
        public static object ToSystemDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return DateTimeHelper.ZERO_TIME;

                case 1:
                    return ValidationHelper.GetDateTime(parameters[0], DateTimeHelper.ZERO_TIME, CultureHelper.EnglishCulture);

                case 2:
                    return ValidationHelper.GetDateTime(parameters[0], ValidationHelper.GetDateTime(parameters[1], DateTimeHelper.ZERO_TIME), CultureHelper.EnglishCulture);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts value to TimeSpan, if it is not possible, returns default value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(TimeSpan), "Converts value to TimeSpan, if it is not possible, returns null.", 1)]
        [MacroMethodParam(0, "value", typeof(object), "Object to convert.")]
        public static object ToTimeSpan(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    return TimeSpan.Zero;

                case 1:
                    try
                    {
                        return TimeSpan.Parse(GetStringParam(parameters[0], context.Culture));
                    }
                    catch { }
                    return null;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Converts double representation of DateTime (OLE Automation Date) to DateTime.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Converts double representation of DateTime (OLE Automation Date) to DateTime.", 1)]
        [MacroMethodParam(0, "value", typeof(double), "OADate double representation to convert.")]
        public static object FromOADate(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return DateTime.FromOADate(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}