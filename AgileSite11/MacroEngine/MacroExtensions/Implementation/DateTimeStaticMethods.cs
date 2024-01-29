using System;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide static DateTime methods in the MacroEngine.
    /// </summary>
    internal class DateTimeStaticMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns an indication whether the specified year is a leap year.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns an indication whether the specified year is a leap year.", 1)]
        [MacroMethodParam(0, "year", typeof(int), "Year to check.")]
        public static object IsLeapYear(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return DateTime.IsLeapYear(GetIntParam(parameters[0]));
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns the number of days in the specified month and year.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns the number of days in the specified month and year", 2)]
        [MacroMethodParam(0, "year", typeof(int), "Year to check.")]
        [MacroMethodParam(1, "month", typeof(int), "Month to check.")]
        public static object DaysInMonth(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return DateTime.DaysInMonth(GetIntParam(parameters[0]), GetIntParam(parameters[1]));
            }

            throw new NotSupportedException();
        }
    }
}