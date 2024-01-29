using System;

using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods from System.Math namespace in the MacroEngine.
    /// </summary>
    internal class MathMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if specified value (numerical/timespan/datetime) is in the given range.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if specified value (numerical/datetime/timespan) is in the given range.", 2)]
        [MacroMethodParam(0, "value", typeof(object), "Value to check.")]
        [MacroMethodParam(1, "lowerBound", typeof(object), "Inclusive lower bound of the interval.")]
        [MacroMethodParam(2, "upperBound", typeof(object), "Inclusive upper bound of the interval.")]
        public static object Between(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return CompareValues(context, ">=", parameters[0], parameters[1]);

                case 3:
                    return GetBoolParam(CompareValues(context, ">=", parameters[0], parameters[1])) &&
                           GetBoolParam(CompareValues(context, "<=", parameters[0], parameters[2]));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns modulo of two values.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns modulo of two values.", 2)]
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
        /// Returns maximum from given numbers.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns maximum from given numbers.", 1)]
        [MacroMethodParam(0, "parameters", typeof(double), "List of numbers.")]
        public static object Max(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                double max = GetDoubleParam(parameters[0], context.Culture);
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!ValidationHelper.IsDouble(parameters[i]))
                    {
                        // Wrong type, return null
                        return null;
                    }
                    max = Math.Max(max, GetDoubleParam(parameters[i], context.Culture));
                }
                return max;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns minimum from given numbers.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns minimum from given numbers.", 1)]
        [MacroMethodParam(0, "parameters", typeof(double), "List of numbers.")]
        public static object Min(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                double min = GetDoubleParam(parameters[0], context.Culture);
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!ValidationHelper.IsDouble(parameters[i]))
                    {
                        // Wrong type, return null
                        return null;
                    }
                    min = Math.Min(min, GetDoubleParam(parameters[i], context.Culture));
                }
                return min;
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns the absolute value of a specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the absolute value of a specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Abs(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Abs(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the angle whose cosine is the specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Acos(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Acos(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the angle whose sine is the specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the angle whose sine is the specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Asin(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Asin(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the angle whose tangent is the specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the angle whose tangent is the specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Atan(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Atan(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the smallest whole number greater than or equal to the specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the smallest whole number greater than or equal to the specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Ceiling(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Ceiling(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the cosine of the specified angle.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the cosine of the specified angle.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Cos(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Cos(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the hyperbolic cosine of the specified angle.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the hyperbolic cosine of the specified angle.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Cosh(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Cosh(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns e raised to the specified power.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns e raised to the specified power.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Exp(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Exp(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the largest whole number less than or equal to the specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the largest whole number less than or equal to the specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Floor(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Floor(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the logarithm of a specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the logarithm of a specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Log(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Log(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the base 10 logarithm of a specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the base 10 logarithm of a specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Log10(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Log10(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns a specified number raised to the specified power.", 2)]
        [MacroMethodParam(0, "base", typeof(double), "Base.")]
        [MacroMethodParam(1, "exp", typeof(double), "Exponent.")]
        public static object Pow(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    // Only two parameters are supported
                    return Math.Pow(GetDoubleParam(parameters[0], context.Culture), GetDoubleParam(parameters[1], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the number nearest the specified value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the number nearest the specified value.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        [MacroMethodParam(1, "digits", typeof(int), "The number of fractional digits in the return value.")]
        [MacroMethodParam(2, "mode", typeof(string), "Specification for how to round value if it is midway between two other numbers. Supported variants are AwayFromZero and ToEven")]
        public static object Round(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    return Math.Round(GetDoubleParam(parameters[0], context.Culture));

                case 2:

                    return Math.Round(GetDoubleParam(parameters[0], context.Culture), GetIntParam(parameters[1]));

                case 3:
                {
                    MidpointRounding midpointRounding;

                    switch (GetStringParam(parameters[2]))
                    {
                        case "AwayFromZero":
                            midpointRounding = MidpointRounding.AwayFromZero;
                            break;

                        case "ToEven":
                            midpointRounding = MidpointRounding.ToEven;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    return Math.Round(GetDoubleParam(parameters[0], context.Culture), GetIntParam(parameters[1]), midpointRounding);
                }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a value indicating the sign of a number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns a value indicating the sign of a number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Sign(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Sign(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the sine of the specified angle.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the sine of the specified angle.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Sin(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Sin(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the hyperbolic sine of the specified angle.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the hyperbolic sine of the specified angle.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Sinh(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Sinh(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the square root of a specified number.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Sqrt(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Sqrt(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the tangent of the specified angle.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the tangent of the specified angle.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Tan(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Tan(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns the hyperbolic tangent of the specified angle.", 1)]
        [MacroMethodParam(0, "number", typeof(double), "Number to do the operation on.")]
        public static object Tanh(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:

                    // Only single parameter is supported
                    return Math.Tanh(GetDoubleParam(parameters[0], context.Culture));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if given number(s) is(are) odd.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if given number(s) is(are) odd.", 1)]
        [MacroMethodParam(0, "numbers", typeof(int), "Numbers to do the operation on.")]
        public static object IsOdd(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                if (parameters[0] == null)
                {
                    return false;
                }
                // Only single parameter is supported
                foreach (object num in parameters)
                {
                    if (Math.Abs(GetIntParam(num)) % 2 == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns true if given number(s) is(are) even.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if given number(s) is(are) even.", 1)]
        [MacroMethodParam(0, "numbers", typeof(int), "Numbers to do the operation on.")]
        public static object IsEven(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                if (parameters[0] == null)
                {
                    return false;
                }
                // Only single parameter is supported
                foreach (object num in parameters)
                {
                    if (Math.Abs(GetIntParam(num)) % 2 == 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns positive random integer within a specified range.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(int), "Returns a random integer within a specified range.", 0)]
        [MacroMethodParam(0, "minValue", typeof(int), "The inclusive lower bound of the number.")]
        [MacroMethodParam(1, "maxValue", typeof(int), "The inclusive upper bound of the number.")]
        [MacroMethodParam(2, "seed", typeof(int), "Seed for the pseudorandom generator - default is system time.")]
        public static object GetRandomInt(EvaluationContext context, params object[] parameters)
        {
            int lower = 0;
            int upper = int.MaxValue - 1;

            switch (parameters.Length)
            {
                case 0:
                    break;

                case 1:
                    lower = GetIntParam(parameters[0]);
                    break;

                case 2:
                    lower = GetIntParam(parameters[0]);
                    upper = GetIntParam(parameters[1]);
                    break;

                case 3:
                    lower = GetIntParam(parameters[0]);
                    upper = GetIntParam(parameters[1]);
                    return new Random(GetIntParam(parameters[2])).Next(lower, upper + 1);

                default:
                    throw new NotSupportedException();
            }

            return new Random().Next(lower, upper + 1);
        }


        /// <summary>
        /// Returns positive random double within a specified range.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns a random double within a specified range.", 0)]
        [MacroMethodParam(0, "minValue", typeof(int), "The inclusive lower bound of the number.")]
        [MacroMethodParam(1, "maxValue", typeof(int), "The inclusive upper bound of the number.")]
        [MacroMethodParam(2, "seed", typeof(int), "Seed for the pseudorandom generator - default is system time.")]
        public static object GetRandomDouble(EvaluationContext context, params object[] parameters)
        {
            int lower = 0;
            int upper = 0;
            switch (parameters.Length)
            {
                case 0:
                    return new Random().NextDouble();

                case 1:
                    return GetIntParam(parameters[0]) + new Random().NextDouble();

                case 2:
                    lower = GetIntParam(parameters[0]);
                    upper = GetIntParam(parameters[1]);
                    return lower + (new Random().NextDouble() * (upper - lower));

                case 3:
                    lower = GetIntParam(parameters[0]);
                    upper = GetIntParam(parameters[1]);
                    return lower + (new Random(GetIntParam(parameters[2])).NextDouble() * (upper - lower));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}