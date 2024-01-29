using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class determining condition which can distinguish between several object types within one Info class.
    /// </summary>
    public class TypeCondition
    {
        #region "Constants"

        private const string NOT_NULL_INTERNAL = "##NOTNULL##";
        private const string NULL_INTERNAL = "##NULL##";
        private const string EXCEPTION_STRING = "The given object violates the type condition of the provider object type. Field '";

        #endregion


        #region "Variables"

        private readonly List<TypeConditionItem> items = new List<TypeConditionItem>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the names of the columns.
        /// </summary>        
        public IEnumerable<string> ConditionColumns
        {
            get
            {
                return items.Select(i => i.Column);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="op">The operator used to compare column value.</param>
        /// <param name="conditionValue">Value of the condition column which determines specified type</param>
        /// <param name="allowNull">if set to true [column value allows null].</param>
        /// <param name="defaultColumnValue">The default column value that would be set into info object in case of type condition application.</param>
        private TypeCondition Add(string conditionColumn, QueryOperator op, object conditionValue, bool allowNull, object defaultColumnValue)
        {
            items.Add(new TypeConditionItem
            {
                Column = conditionColumn,
                Value = conditionValue,
                DefaultColumnValue = defaultColumnValue,
                AllowNull = allowNull,
                Operator = op
            });
            return this;
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="conditionValue">Value of the condition column which determines specified type</param>
        /// <remarks>DefaultColumnValue (The default column value that would be set into info object in case of type condition application) equals to conditionValue.</remarks>
        public TypeCondition WhereEquals(string conditionColumn, object conditionValue)
        {
            return Add(conditionColumn, QueryOperator.Equals, conditionValue, false, conditionValue);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="conditionValue">Value of the condition column which determines specified type</param>
        /// <param name="defaultColumnValue">The default column value that would be set into info object in case of type condition application.</param>
        public TypeCondition WhereEquals(string conditionColumn, object conditionValue, object defaultColumnValue)
        {
            return Add(conditionColumn, QueryOperator.Equals, conditionValue, false, defaultColumnValue);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="conditionValue">Value of the condition column which determines specified type</param>
        /// <param name="defaultColumnValue">The default column value that would be set into info object in case of type condition application.</param>
        public TypeCondition WhereNotEquals(string conditionColumn, object conditionValue, object defaultColumnValue = null)
        {
            return Add(conditionColumn, QueryOperator.NotEquals, conditionValue, false, defaultColumnValue);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="conditionValue">Value of the condition column which determines specified type</param>
        /// <param name="defaultColumnValue">The default column value that would be set into info object in case of type condition application.</param>
        public TypeCondition WhereEqualsOrNull(string conditionColumn, object conditionValue, object defaultColumnValue = null)
        {
            return Add(conditionColumn, QueryOperator.Equals, conditionValue, true, defaultColumnValue);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="conditionValue">Value of the condition column which determines specified type</param>
        /// <param name="defaultColumnValue">The default column value that would be set into info object in case of type condition application.</param>
        public TypeCondition WhereNotEqualsOrNull(string conditionColumn, object conditionValue, object defaultColumnValue = null)
        {
            return Add(conditionColumn, QueryOperator.NotEquals, conditionValue, true, defaultColumnValue);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        public TypeCondition WhereIsNull(string conditionColumn)
        {
            return Add(conditionColumn, QueryOperator.Equals, NULL_INTERNAL, false, null);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        public TypeCondition WhereIsNotNull(string conditionColumn)
        {
            return Add(conditionColumn, QueryOperator.Equals, NOT_NULL_INTERNAL, false, DataHelper.FAKE_ID);
        }


        /// <summary>
        /// Adds another condition into TypeCondition.
        /// </summary>
        /// <param name="conditionColumn">Name of the column the value of which can distinguish between the object types</param>
        /// <param name="defaultColumnValue">The default column value that would be set into info object in case of type condition application.</param>
        public TypeCondition WhereIsNotNull(string conditionColumn, object defaultColumnValue)
        {
            return Add(conditionColumn, QueryOperator.Equals, NOT_NULL_INTERNAL, false, defaultColumnValue);
        }


        /// <summary>
        /// Gets the field value for the Type condition
        /// </summary>
        public object GetFieldValue(string column)
        {
            var item = items.FirstOrDefault(i => i.Column == column);

            return item == null ? null : item.DefaultColumnValue;
        }


        /// <summary>
        /// Gets the actual value for the Type condition column
        /// </summary>
        public object GetValue(string column)
        {
            var item = items.FirstOrDefault(i => i.Column == column);
            return item != null ? item.Value : null;
        }


        /// <summary>
        /// Generates default WHERE condition according to GroupColumn and TypeCondition settings. 
        /// </summary>
        public WhereCondition GetWhereCondition()
        {
            var where = new WhereCondition();

            foreach (var item in items)
            {
                var itemWhere = new WhereCondition();
                var useBrackets = item.AllowNull && (items.Count > 1);

                if (!useBrackets)
                {
                    itemWhere = where;
                }

                var stringVal = item.Value as string;
                switch (stringVal)
                {
                    case NOT_NULL_INTERNAL:
                        itemWhere.WhereNotNull(item.Column);
                        break;
                    case NULL_INTERNAL:
                        itemWhere.WhereNull(item.Column);
                        break;
                    default:
                        itemWhere.Where(item.Column, item.Operator, item.Value);
                        break;
                }

                if (item.AllowNull)
                {
                    itemWhere.Or().WhereNull(item.Column);
                }

                if (useBrackets)
                {
                    where.Where(itemWhere);
                }
            }

            return where;
        }


        /// <summary>
        /// Checks if condition defined in TypeCondition is not violated by the given info.
        /// </summary>
        /// <param name="info">Info to check</param>
        /// <param name="throwException">If true, the exception is thrown in case the object violates the type condition</param>
        internal bool Check(IDataContainer info, bool throwException = true)
        {
            foreach (var item in items)
            {
                var conditionColumn = item.Column;

                var val = info.GetValue(conditionColumn);
                var expectedVal = GetValue(conditionColumn);

                // Special treatment for special values
                var strVal = expectedVal as string;
                if (strVal == NOT_NULL_INTERNAL)
                {
                    if (val == null)
                    {
                        if (throwException)
                        {
                            throw new InvalidOperationException(string.Format("{0}{1}' must not be null.", EXCEPTION_STRING, conditionColumn));
                        }

                        return false;
                    }
                }
                else if (strVal == NULL_INTERNAL)
                {
                    if (val != null)
                    {
                        if (throwException)
                        {
                            throw new InvalidOperationException(string.Format("{0}{1}' must be null.", EXCEPTION_STRING, conditionColumn));
                        }

                        return false;
                    }
                }
                // Treatment for standard values
                else
                {
                    // Condition allows NULL for this column -> OK
                    if ((val == null) && (item.AllowNull))
                    {
                        continue;
                    }

                    switch (item.Operator)
                    {
                        case QueryOperator.Equals:
                            // Both values are null -> OK
                            if ((val == null) && (expectedVal == null))
                            {
                                continue;
                            }
                            // Only val is null or values do not match -> FAIL
                            if ((val == null) || !val.Equals(expectedVal))
                            {
                                if (throwException)
                                {
                                    throw new InvalidOperationException(string.Format("{0}{1}' must have value '{2}'.", EXCEPTION_STRING, conditionColumn, expectedVal));
                                }

                                return false;
                            }
                            break;

                        case QueryOperator.NotEquals:
                            // Only val is null -> OK
                            if ((val == null) && (expectedVal != null))
                            {
                                continue;
                            }
                            // Both values are null or values do match -> FAIL
                            if ((val == null) || val.Equals(expectedVal))
                            {
                                if (throwException)
                                {
                                    throw new InvalidOperationException(string.Format("{0}{1}' must not have value '{2}'.", EXCEPTION_STRING, conditionColumn, expectedVal));
                                }

                                return false;
                            }
                            break;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}