using System;
using System.Collections;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(EnumerableMethods), typeof(IEnumerable))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods working with IEnumerable in the MacroEngine.
    /// </summary>
    internal class EnumerableMethods : MacroMethodContainer
    {
        /// <summary>
        /// Converts the given list of enumerable collections to a flat list of their items.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Converts the given list of enumerable collections to a flat list of their items.", 2)]
        [MacroMethodParam(0, "items", typeof(IEnumerable), "List of enumerable collections.", IsParams = true)]
        public static object Flatten(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                return new FlattenEnumerable(parameters);
            }
            else
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets the specified item from the collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets the specified item from the collection.", 2)]
        [MacroMethodParam(0, "collection", typeof(IEnumerable), "Collection.")]
        [MacroMethodParam(1, "index", typeof(int), "Index of the item.")]
        [MacroMethodParam(2, "defaultValue", typeof(object), "Default value which is returned when the requested item is null.")]
        public static object GetItem(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                object result = null;

                if (parameters[0] is IEnumerable)
                {
                    int index = GetIntParam(parameters[1]);
                    int i = 0;

                    // Enumerate the items until the item is found
                    IEnumerator en = ((IEnumerable)parameters[0]).GetEnumerator();
                    while (en.MoveNext())
                    {
                        if (i == index)
                        {
                            // Matched index
                            result = en.Current;
                        }

                        i++;
                    }
                }

                // If default value is defined, return it
                if ((parameters.Length == 3) && (result == null))
                {
                    result = parameters[2];
                }

                return result;
            }
            else
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns specified number of random objects from within the given collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IList), "Randomly selects specified number of objects from within a given collection.", 1)]
        [MacroMethodParam(0, "items", typeof(IEnumerable), "Collection of items from which to select.")]
        [MacroMethodParam(1, "numberOfItems", typeof(int), "Number of items to select.")]
        public static object RandomSelection(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                // Return null - no collection to choose from
                if (parameters[0] == null)
                {
                    return null;
                }

                if (parameters[0] is IEnumerable)
                {
                    IEnumerable enumerable = (IEnumerable)parameters[0];
                    ArrayList items = new ArrayList();

                    // Copy items to a list
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        items.Add(enumerator.Current);
                    }

                    // Number of random numbers to select
                    int capacity = 1;
                    if (parameters.Length > 1)
                    {
                        capacity = Math.Min(ValidationHelper.GetInteger(parameters[1], 1), items.Count);
                    }

                    // Generate the result
                    ArrayList result = new ArrayList(capacity);
                    Random random = new Random();
                    while (capacity > 0)
                    {
                        int itemIndex = random.Next(0, items.Count);

                        result.Add(items[itemIndex]);
                        items.RemoveAt(itemIndex);

                        capacity--;
                    }

                    if (result.Count == 1)
                    {
                        return result[0];
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            throw new NotSupportedException("[MacroMethods.RandomSelection]: Wrong number of parameters passed to the method");
        }


        /// <summary>
        /// Returns specified interval of items from the collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IList), "Selects a specified interval from given collection of items.", 3)]
        [MacroMethodParam(0, "items", typeof(IEnumerable), "Collection of items from which to select.")]
        [MacroMethodParam(1, "lowerBound", typeof(int), "Inclusive lower bound of the interval.")]
        [MacroMethodParam(2, "upperBound", typeof(int), "Inclusive upper bound of the interval.")]
        public static object SelectInterval(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                // Return null - no collection to choose from
                if (parameters[0] == null)
                {
                    return null;
                }

                if (parameters[0] is IEnumerable)
                {
                    // Get the bounds
                    int lowerBound = ValidationHelper.GetInteger(parameters[1], 0);
                    int upperBound = -1;
                    if (parameters.Length > 2)
                    {
                        upperBound = ValidationHelper.GetInteger(parameters[2], 0);
                        if (upperBound < lowerBound)
                        {
                            // Swap the bounds
                            int temp = upperBound;
                            upperBound = lowerBound;
                            lowerBound = temp;
                        }
                    }

                    // Get the result
                    ArrayList result = new ArrayList();
                    int index = 0;
                    IEnumerable col = (IEnumerable)parameters[0];
                    foreach (object item in col)
                    {
                        if ((index >= lowerBound) && ((upperBound == -1) || (index <= upperBound)))
                        {
                            result.Add(item);
                        }
                        index++;
                    }

                    return result;
                }
            }

            throw new NotSupportedException("[MacroMethods.SelectInterval]: Wrong number of parameters passed to the method");
        }


        /// <summary>
        /// Returns true if specified object is within the given collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if specified object exists within the given collection.", 2)]
        [MacroMethodParam(0, "object", typeof(object), "Object which should be checked for existence within the collection.")]
        [MacroMethodParam(1, "collection", typeof(IEnumerable), "Collection of items.")]
        public static object InList(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                object objToCheck = parameters[0];

                for (int i = 1; i < parameters.Length; i++)
                {
                    object o = parameters[i];
                    if ((o is IEnumerable) && !(o is string))
                    {
                        IEnumerable en = (IEnumerable)o;
                        foreach (object item in en)
                        {
                            if (ExpressionEvaluator.IsEqual(item, objToCheck, context))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (ExpressionEvaluator.IsEqual(o, objToCheck, context))
                        {
                            return true;
                        }
                    }
                }

                // No match found
                return false;
            }
            else
            {
                throw new NotSupportedException("[MacroMethods.InList]: Wrong number of parameters passed to the method");
            }
        }


        /// <summary>
        /// Returns true if there is at least one object in the collection which matches given condition. If condition is not defined, returns true if collection is not empty.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if there is at least one object in the collection which matches given condition. If condition is not defined, returns true if collection is not empty.", 1)]
        [MacroMethodParam(0, "collection", typeof(IEnumerable), "Collection of items.")]
        [MacroMethodParam(1, "condition", typeof(MacroExpression), "Filtering macro condition to evaluate for each item.", AsExpression = true)]
        public static object Exists(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    IEnumerable enumerable = parameters[0] as IEnumerable;
                    if (enumerable == null)
                    {
                        return false;
                    }

                    return enumerable.GetEnumerator().MoveNext();

                case 2:
                    if (parameters[0] == null)
                    {
                        return false;
                    }

                    IEnumerable collection = parameters[0] as IEnumerable;
                    if (collection != null)
                    {
                        MacroExpression expr = GetExpressionParam(parameters[1]);

                        // Protect parent context and evaluate expression for each item
                        var child = context.CreateChildContext();
                        foreach (object info in collection)
                        {
                            child.Resolver.SetAnonymousSourceData(info);
                            if (ValidationHelper.GetBoolean(new ExpressionEvaluator(expr, child).Evaluate().Result, false))
                            {
                                return true;
                            }
                        }
                    }
                    return false;

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns collection filtered with given dynamic condition.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IList), "Returns the collection filtered by given condition.", 2)]
        [MacroMethodParam(0, "collection", typeof(IEnumerable), "Collection of items.")]
        [MacroMethodParam(1, "condition", typeof(MacroExpression), "Filtering macro condition to evaluate for each item.", AsExpression = true)]
        public static object Filter(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:

                    if (parameters[0] == null)
                    {
                        return null;
                    }

                    if (parameters[0] is IEnumerable)
                    {
                        IEnumerable collection = (IEnumerable)parameters[0];
                        MacroExpression expr = GetExpressionParam(parameters[1]);

                        // Protect parent context and evaluate expression for each item
                        var child = context.CreateChildContext();
                        ArrayList result = new ArrayList();
                        foreach (var info in collection)
                        {
                            child.Resolver.SetAnonymousSourceData(info);
                            if (ValidationHelper.GetBoolean(new ExpressionEvaluator(expr, child).Evaluate().Result, false))
                            {
                                result.Add(info);
                            }
                        }

                        if (parameters[0] is IInfoObjectCollection)
                        {
                            // If original collection was InfoObjectCollection, keep it this way
                            IInfoObjectCollection resultCollection = new InfoObjectCollection(((IInfoObjectCollection)parameters[0]).ObjectType);
                            resultCollection.Disconnect();
                            resultCollection.Load(result.Cast<BaseInfo>().ToList());

                            return resultCollection;
                        }

                        return result;
                    }
                    else
                    {
                        throw new NotSupportedException("[MacroMethods.Filter]: Wrong type of parameters passed to the method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if there is at least one object in the collection satisfying specified condition. If condition is not defined, returns true if collection is not empty.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if there is at least one object in the collection satisfying specified condition. If condition is not defined, returns true if collection is not empty.", 1)]
        [MacroMethodParam(0, "collection", typeof(IEnumerable), "Collection of items.")]
        [MacroMethodParam(1, "condition", typeof(MacroExpression), "Filtering macro condition to evaluate for each item.", AsExpression = true)]
        public static object Any(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    IEnumerable enumerable = parameters[0] as IEnumerable;
                    if (enumerable != null)
                    {
                        return enumerable.GetEnumerator().MoveNext();
                    }

                    throw new NotSupportedException("[MacroMethods.Any]: Wrong type of parameters passed to the method");

                case 2:
                    if (parameters[0] == null)
                    {
                        return false;
                    }
                    if (parameters[0] is IEnumerable)
                    {
                        IEnumerable collection = (IEnumerable)parameters[0];
                        MacroExpression expr = GetExpressionParam(parameters[1]);

                        // Protect parent context and evaluate expression for each item
                        var child = context.CreateChildContext();
                        foreach (var info in collection)
                        {
                            child.Resolver.SetAnonymousSourceData(info);
                            if (ValidationHelper.GetBoolean(new ExpressionEvaluator(expr, child).Evaluate().Result, false))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    else
                    {
                        throw new NotSupportedException("[MacroMethods.Any]: Wrong type of parameters passed to the method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns true if all the objects in the collection satisfy specified condition (returns true for empty collection).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if all the objects in the collection satisfy specified condition (returns true for empty collection).", 2)]
        [MacroMethodParam(0, "collection", typeof(IEnumerable), "Collection of items.")]
        [MacroMethodParam(1, "condition", typeof(MacroExpression), "Filtering macro condition to evaluate for each item.", AsExpression = true)]
        public static object All(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    if (parameters[0] == null)
                    {
                        return true;
                    }
                    if (parameters[0] is IEnumerable)
                    {
                        IEnumerable collection = (IEnumerable)parameters[0];
                        MacroExpression expr = GetExpressionParam(parameters[1]);

                        // Protect parent context and evaluate expression for each item
                        var child = context.CreateChildContext();
                        foreach (var info in collection)
                        {
                            child.Resolver.SetAnonymousSourceData(info);
                            if (!ValidationHelper.GetBoolean(new ExpressionEvaluator(expr, child).Evaluate().Result, false))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                    else
                    {
                        throw new NotSupportedException("[MacroMethods.All]: Wrong type of parameters passed to the method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        #region "Helper methods"

        /// <summary>
        /// Processes a MacroExpression parameter (filtering condition).
        /// </summary>
        /// <param name="parameter">Parameter to process</param>
        private static MacroExpression GetExpressionParam(object parameter)
        {
            MacroExpression expr;
            if (parameter is MacroExpression)
            {
                expr = (MacroExpression)parameter;
                if (expr.Type == ExpressionType.Value)
                {
                    if (expr.Value is string)
                    {
                        // Consider the string as an expression and parse it
                        expr = MacroExpression.ParseExpression((string)expr.Value);
                    }
                }
            }
            else
            {
                string condition = Convert.ToString(parameter);
                expr = MacroExpression.ParseExpression(condition);
            }
            return expr;
        }

        #endregion
    }
}