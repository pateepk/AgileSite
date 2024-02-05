using System;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(InfoObjectCollectionMethods), typeof(IInfoObjectCollection))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods working with InfoObjectCollection in the MacroEngine.
    /// </summary>
    internal class InfoObjectCollectionMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns a sum of all collection items over specified column.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns a sum of all collection items over specified column.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection of items.")]
        [MacroMethodParam(1, "columnName", typeof(string), "Name of the column.")]
        public static object Sum(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    if ((parameters[0] is IInfoObjectCollection))
                    {
                        IInfoObjectCollection collection = (IInfoObjectCollection)parameters[0];
                        string columnName = Convert.ToString(parameters[1]);

                        double sum = 0;
                        foreach (BaseInfo info in collection)
                        {
                            // Check that the column is of correct type
                            object val = info.GetValue(columnName);
                            if ((val != null) && !ValidationHelper.IsDouble(val))
                            {
                                throw new NotSupportedException("Cannot make a sum of column which doesn't have a numeric type.");
                            }

                            // Compute the average
                            sum += ValidationHelper.GetDouble(val, 0);
                        }
                        return sum;
                    }
                    else
                    {
                        throw new NotSupportedException("Wrong type of parameters passed to the 'Sum' method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns an average of all collection items over specified column.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns an average of all collection items over specified column.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection of items.")]
        [MacroMethodParam(1, "columnName", typeof(string), "Name of the column.")]
        public static object Average(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    if ((parameters[0] is IInfoObjectCollection))
                    {
                        IInfoObjectCollection collection = (IInfoObjectCollection)parameters[0];
                        string columnName = Convert.ToString(parameters[1]);

                        double sum = 0;
                        int count = 0;
                        foreach (BaseInfo info in collection)
                        {
                            // Check that the column is of correct type
                            object val = info.GetValue(columnName);
                            if ((val != null) && !ValidationHelper.IsDouble(val))
                            {
                                throw new NotSupportedException("Cannot make an average of column which doesn't have a numeric type.");
                            }

                            // Compute the average
                            sum += ValidationHelper.GetDouble(val, 0);
                            count++;
                        }
                        if (count > 0)
                        {
                            return sum / count;
                        }
                        return null;
                    }
                    else
                    {
                        throw new NotSupportedException("Wrong type of parameters passed to the 'Average' method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a minimum value of all collection items over specified column.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns a minimum value of all collection items over specified column.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection of items.")]
        [MacroMethodParam(1, "columnName", typeof(string), "Name of the column.")]
        public static object Minimum(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    if ((parameters[0] is IInfoObjectCollection))
                    {
                        IInfoObjectCollection collection = (IInfoObjectCollection)parameters[0];
                        string columnName = Convert.ToString(parameters[1]);

                        double? min = null;
                        foreach (BaseInfo info in collection)
                        {
                            // Check that the column is of correct type
                            object val = info.GetValue(columnName);
                            if ((val != null) && !ValidationHelper.IsDouble(val))
                            {
                                throw new NotSupportedException("Cannot find a minimum of column which doesn't have a numeric type.");
                            }

                            double value = ValidationHelper.GetDouble(val, 0);
                            if (min == null)
                            {
                                min = value;
                            }
                            else
                            {
                                if (value < min)
                                {
                                    min = value;
                                }
                            }
                        }
                        if (min != null)
                        {
                            return min.Value;
                        }
                        return null;
                    }
                    else
                    {
                        throw new NotSupportedException("Wrong type of parameters passed to the 'Minimum' method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a maximum value of all collection items over specified column.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(double), "Returns a maximum value of all collection items over specified column.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection of items.")]
        [MacroMethodParam(1, "columnName", typeof(string), "Name of the column.")]
        public static object Maximum(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    if ((parameters[0] is IInfoObjectCollection))
                    {
                        IInfoObjectCollection collection = (IInfoObjectCollection)parameters[0];
                        string columnName = Convert.ToString(parameters[1]);

                        double? max = null;
                        foreach (BaseInfo info in collection)
                        {
                            // Check that the column is of correct type
                            object val = info.GetValue(columnName);
                            if ((val != null) && !ValidationHelper.IsDouble(val))
                            {
                                throw new NotSupportedException("Cannot find a maximum of column which doesn't have a numeric type.");
                            }

                            double value = ValidationHelper.GetDouble(val, 0);
                            if (max == null)
                            {
                                max = value;
                            }
                            else
                            {
                                if (value > max)
                                {
                                    max = value;
                                }
                            }
                        }
                        if (max != null)
                        {
                            return max.Value;
                        }
                        return null;
                    }
                    else
                    {
                        throw new NotSupportedException("Wrong type of parameters passed to the 'Maximum' method");
                    }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Order by on collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IInfoObjectCollection), "Order by on collection.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection to order.")]
        [MacroMethodParam(1, "orderBy", typeof(string), "ORDER BY column(s).")]
        public static object OrderBy(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                    // Only two parameters are supported
                case 2:
                {
                    // Get collection
                    var collection = parameters[0] as IInfoObjectCollection;
                    if (collection == null)
                    {
                        return null;
                    }

                    // Get ORDER BY expression
                    string orderBy = GetStringParam(parameters[1], context.Culture);
                    if (String.IsNullOrEmpty(orderBy))
                    {
                        return null;
                    }

                    // Only global administrator can fully specify the ORDER BY expression
                    // Users with less privilege have limited options
                    var user = context.User;
                    bool isGlobalAdmin = (user != null) && user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
                    if ((!isGlobalAdmin && !SqlSecurityHelper.OrderByRegex.IsMatch(orderBy)) || !SqlSecurityHelper.CheckQuery(orderBy, QueryScopeEnum.OrderBy))
                    {
                        throw new NotSupportedException("An invalid ORDER BY statement was used.");
                    }

                    // Modify clone without impacting the original collection
                    var clone = collection.CloneCollection();
                    clone.EnforceReadOnlyDataAccess = true;
                    clone.OrderByColumns = orderBy;

                    return clone;
                }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Where on collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IInfoObjectCollection), "Where on collection.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection to filter.")]
        [MacroMethodParam(1, "where", typeof(string), "WHERE condition.")]
        public static object Where(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                    // Only two parameters are supported
                case 2:
                {
                    // Get collection
                    var collection = parameters[0] as IInfoObjectCollection;
                    if (collection == null)
                    {
                        return null;
                    }

                    // Get WHERE condition
                    string where = GetStringParam(parameters[1], context.Culture);
                    if (String.IsNullOrEmpty(@where))
                    {
                        return null;
                    }

                    // Only global administrator can fully specify the WHERE condition
                    // Users with less privilege have limited options
                    var user = context.User;
                    bool isGlobalAdmin = (user != null) && user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
                    if ((!isGlobalAdmin && !SqlSecurityHelper.WhereRegex.IsMatch(@where)) || !SqlSecurityHelper.CheckQuery(@where, QueryScopeEnum.Where))
                    {
                        throw new NotSupportedException("An invalid WHERE condition was used.");
                    }

                    // Modify clone without impacting the original collection
                    var clone = collection.CloneCollection();

                    clone.EnforceReadOnlyDataAccess = true;
                    clone.Where.Where(@where);

                    return clone;
                }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// TopN on collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IInfoObjectCollection), "TopN on collection.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection to filter.")]
        [MacroMethodParam(1, "topn", typeof(int), "TOP N parameter.")]
        public static object TopN(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                    // Only two parameters are supported
                case 2:
                {
                    // Get collection
                    var collection = parameters[0] as IInfoObjectCollection;
                    if (collection == null)
                    {
                        return null;
                    }

                    // Get TOPN number
                    int topN = GetIntParam(parameters[1]);

                    // Modify clone without impacting the original collection
                    var clone = collection.CloneCollection();
                    clone.EnforceReadOnlyDataAccess = true;
                    clone.TopN = topN;

                    return clone;
                }

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Columns on collection.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(IInfoObjectCollection), "Columns on collection.", 2)]
        [MacroMethodParam(0, "collection", typeof(IInfoObjectCollection), "Collection to filter.")]
        [MacroMethodParam(1, "columns", typeof(string), "Columns parameter.")]
        public static object Columns(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                    // Only two parameters are supported
                case 2:
                {
                    // Get collection
                    var collection = parameters[0] as IInfoObjectCollection;
                    if (collection == null)
                    {
                        return null;
                    }

                    // Get columns to limit the collection data
                    string columns = GetStringParam(parameters[1], context.Culture);

                    // Only global administrator can fully specify the selected columns with advanced expressions
                    // Users with less privilege have limited options
                    var user = context.User;
                    bool isGlobalAdmin = (user != null) && user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);
                    if ((!isGlobalAdmin && !SqlSecurityHelper.ColumnsRegex.IsMatch(columns)) || !SqlSecurityHelper.CheckQuery(columns, QueryScopeEnum.Columns))
                    {
                        throw new NotSupportedException("An invalid columns definition was used.");
                    }

                    // Modify clone without impacting the original collection
                    var clone = collection.CloneCollection();
                    clone.EnforceReadOnlyDataAccess = true;
                    clone.Columns = columns;

                    return clone;
                }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}