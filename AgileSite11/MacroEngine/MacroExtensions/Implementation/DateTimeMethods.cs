using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide DateTime methods in the MacroEngine.
    /// </summary>
    internal class DateTimeMethods : MacroMethodContainer
    {
        /// <summary>
        /// Converts the value of the DateTime to its equivalent short date string representation.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Converts the value of the DateTime to its equivalent short date string representation.", 1)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        public static object ToShortDateString(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).ToString("d", context.CultureInfo);
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Converts the value of the DateTime to its equivalent short date time representation.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Converts the value of the DateTime to its equivalent short time string representation.", 1)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        public static object ToShortTimeString(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).ToString("t", context.CultureInfo);
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of milliseconds
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of milliseconds to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "milliseconds", typeof(int), "Number of milliseconds to add.")]
        public static object AddMilliseconds(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddMilliseconds(GetDoubleParam(parameters[1], context.Culture));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of seconds
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of seconds to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "seconds", typeof(int), "Number of seconds to add.")]
        public static object AddSeconds(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddSeconds(GetDoubleParam(parameters[1], context.Culture));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of minutes
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of minutes to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "minutes", typeof(int), "Number of minutes to add.")]
        public static object AddMinutes(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddMinutes(GetDoubleParam(parameters[1], context.Culture));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of hours
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of hours to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "hours", typeof(int), "Number of hours to add.")]
        public static object AddHours(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddHours(GetDoubleParam(parameters[1], context.Culture));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of days
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of days to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "days", typeof(int), "Number of days to add.")]
        public static object AddDays(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddDays(GetDoubleParam(parameters[1], context.Culture));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of weeks
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of weeks to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "weeks", typeof(int), "Number of weeks to add.")]
        public static object AddWeeks(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddDays(7 * GetDoubleParam(parameters[1], context.Culture));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of months
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of months to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "months", typeof(int), "Number of months to add.")]
        public static object AddMonths(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddMonths(GetIntParam(parameters[1]));
                    }
                    break;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Adds specified number of years
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Adds the specified number of years to the value of the base instance.", 2)]
        [MacroMethodParam(0, "datetime", typeof(DateTime), "Base date time.")]
        [MacroMethodParam(1, "years", typeof(int), "Number of years to add.")]
        public static object AddYears(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    parameters = UnwrapContainer(parameters);
                    if (parameters[0] is DateTime)
                    {
                        return ((DateTime)parameters[0]).AddYears(GetIntParam(parameters[1]));
                    }
                    break;
            }

            throw new NotSupportedException();
        }
    }
}