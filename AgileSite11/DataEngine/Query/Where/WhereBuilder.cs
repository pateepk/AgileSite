using System;
using System.Linq.Expressions;

using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Where condition builder
    /// </summary>
    public class WhereBuilder
    {
        #region "Properties
        
        /// <summary>
        /// Default where builder
        /// </summary>
        public static WhereBuilder Default
        {
            get
            {
                return ObjectFactory<WhereBuilder>.StaticSingleton();
            }
        }

        #endregion


        #region "Constants"

        /// <summary>
        /// SQL "LIKE" operator constant.
        /// </summary>
        public const string LIKE = "LIKE";

        
        /// <summary>
        /// SQL "NOT LIKE" operator constant.
        /// </summary>
        public const string NOT_LIKE = "NOT LIKE";

        
        /// <summary>
        /// SQL "=" operator constant.
        /// </summary>
        public const string EQUAL = "=";


        /// <summary>
        /// SQL "&lt;&gt;" operator constant.
        /// </summary>
        public const string NOT_EQUAL = "<>";


        /// <summary>
        /// Returns the OR operator
        /// </summary>
        public virtual string OperatorOR
        {
            get
            {
                return "OR";
            }
        }


        /// <summary>
        /// Returns the AND operator
        /// </summary>
        public virtual string OperatorAND
        {
            get
            {
                return "AND";
            }
        }


        /// <summary>
        /// Returns the NULL constant
        /// </summary>
        public virtual string NULL
        {
            get
            {
                return "NULL";
            }
        }

        #endregion


        #region "SQL query generation methods"

        /// <summary>
        /// Gets the negation of the given where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        public string GetNegation(string where)
        {
            return "NOT " + where;
        }


        /// <summary>
        /// Gets the IS NULL expression
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="negation">If true, negates the expression</param>
        public virtual string GetIsNull(string columnName, bool negation)
        {
            return String.Format("{0} IS {1}{2}", columnName, (negation ? "NOT " : null), NULL);
        }


        /// <summary>
        /// Gets the IN expression
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="negation">If true, produces NOT IN</param>
        /// <param name="nested">Nested expression</param>
        public virtual string GetIn(string columnName, bool negation, string nested)
        {
            string where = String.Format(
@"{0} {1} (
    {2}
)", columnName, (negation ? "NOT IN" : "IN"), nested);

            return where;
        }


        /// <summary>
        /// Gets the EXISTS expression
        /// </summary>
        /// <param name="negation">If true, produces NOT EXISTS</param>
        /// <param name="nested">Nested expression</param>
        public virtual string GetExists(bool negation, string nested)
        {
            string where = String.Format(
@"{0} (
    {1}
)", (negation ? "NOT EXISTS" : "EXISTS"), nested);

            return where;
        }


        /// <summary>
        /// Gets the binary operator based on the expression type
        /// </summary>
        /// <param name="ex">Expression type</param>
        /// <param name="rightExpression">Expression on the right side of binary operator (second expression)</param>
        public virtual string GetBinaryOperator(ExpressionType ex, Expression rightExpression = null)
        {
            var nullComparison = false;

            // Check whether value of right expression isn't null.
            // Obtain value from constant and field member expressions.
            // Properties are translated to database columns, their values are not accessed.
            if (rightExpression != null)
            {
                if (rightExpression.NodeType == ExpressionType.Constant)
                {
                    nullComparison = ((ConstantExpression)rightExpression).Value == null;
                }
                else if (rightExpression.NodeType == ExpressionType.MemberAccess)
                {
                    var memberExpression = (MemberExpression)rightExpression;
                    if (memberExpression.Member is System.Reflection.FieldInfo)
                    {
                        // Get value
                        var objectMember = Expression.Convert(memberExpression, typeof (object));
                        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                        var getter = getterLambda.Compile();

                        nullComparison = getter() == null;
                    }
                }
            }


            string op;

            switch (ex)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    op = "AND";
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    op = "OR";
                    break;

                case ExpressionType.Equal:
                    op = nullComparison ? "IS" : "=";
                    break;

                case ExpressionType.NotEqual:
                    op = nullComparison ? "IS NOT" : "<>";
                    break;

                case ExpressionType.LessThan:
                    op = "<";
                    break;

                case ExpressionType.LessThanOrEqual:
                    op = "<=";
                    break;

                case ExpressionType.GreaterThan:
                    op = ">";
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    op = ">=";
                    break;

                default:
                    throw new NotSupportedException("[ObjectCollection.ProcessBinary]: Binary operator '" + ex + "' is not supported.");
            }

            return op;
        }


        /// <summary>
        /// Gets the operator string
        /// </summary>
        /// <param name="op">Operator to convert</param>
        private object GetOperator(QueryUnaryOperator op)
        {
            switch (op)
            {
                case QueryUnaryOperator.IsNull:
                    return "IS NULL";

                case QueryUnaryOperator.IsNotNull:
                    return "IS NOT NULL";
            }

            throw new NotSupportedException("Unknown query unary operator.");
        }
        

        /// <summary>
        /// Gets the operator string
        /// </summary>
        /// <param name="op">Operator to convert</param>
        public virtual string GetOperator(QueryOperator op)
        {
            switch (op)
            {
                case QueryOperator.Equals:
                    return "=";

                case QueryOperator.Like:
                    return "LIKE";

                case QueryOperator.NotLike:
                    return "NOT LIKE";

                case QueryOperator.NotEquals:
                    return "<>";

                case QueryOperator.GreaterThan:
                    return ">";

                case QueryOperator.LessThan:
                    return "<";

                case QueryOperator.LessOrEquals:
                    return "<=";

                case QueryOperator.GreaterOrEquals:
                    return ">=";
            }

            throw new NotSupportedException("Unknown query operator.");
        }


        /// <summary>
        /// Gets a new parameter
        /// </summary>
        /// <param name="name">Value name</param>
        /// <param name="value">Value</param>
        /// <param name="parameters">Collection of data parameters</param>
        public virtual string GetParameter(string name, object value, ref QueryDataParameters parameters)
        {
            // Add parameter value
            parameters = parameters ?? new QueryDataParameters();

            var param = parameters.AddUnique(name, value);

            return param.Name;
        }



        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="op">Operator</param>
        public virtual string GetWhere(IQueryObjectWithValue expression, QueryUnaryOperator op)
        {
            return String.Format("{0} {1}", expression.GetExpression(), GetOperator(op));
        }


        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        public virtual string GetWhere(string columnName, QueryUnaryOperator op)
        {
            return String.Format("{0} {1}", columnName, GetOperator(op));
        }


        /// <summary>
        /// Gets the where condition for the given expressions. Note that in this variant string value is always represented as a value. To pass in column name, you need to use new QueryColumn("Name")
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="op">Operator</param>
        /// <param name="rightSide">Right side</param>
        /// <param name="parameters">Collection of data parameters</param>
        public virtual string GetWhere(IQueryObjectWithValue leftSide, QueryOperator op, object rightSide, ref QueryDataParameters parameters)
        {
            // Get left-side column name if available
            IQueryColumn colmn = leftSide as IQueryColumn;
            var valueName = (colmn == null) ? "Value" : leftSide.Name;

            // Include the right side value
            var leftExpression = QueryDataParameters.IncludeValue(valueName, leftSide, ref parameters);

            // Add value as parameter
            if (RepresentsNull(rightSide))
            {
                return GetNullComparison(leftExpression, op);
            }

            // Get right-side column name if available
            colmn = rightSide as IQueryColumn;
            valueName = (colmn == null) ? valueName : colmn.Name;

            // Include the right side value
            var rightExpression = QueryDataParameters.IncludeValue(valueName, rightSide, ref parameters);

            return String.Format("{0} {1} {2}", leftExpression, GetOperator(op), rightExpression);
        }


        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        /// <param name="parameters">Collection of data parameters</param>
        public virtual string GetWhere(string columnName, QueryOperator op, object value, ref QueryDataParameters parameters)
        {
            // Add value as parameter
            if (RepresentsNull(value))
            {
                return GetNullComparison(columnName, op);
            }

            // Include the right side value
            var rightSide = QueryDataParameters.IncludeValue(columnName, value, ref parameters);

            return String.Format("{0} {1} {2}", columnName, GetOperator(op), rightSide);
        }


        /// <summary>
        /// Gets the null comparison for the given expression in format "[expression] IS NULL"
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="op">Operator</param>
        private string GetNullComparison(string expression, QueryOperator op)
        {
            // Add null comparison
            switch (op)
            {
                case QueryOperator.Equals:
                    return GetIsNull(expression, false);

                case QueryOperator.NotEquals:
                    return GetIsNull(expression, true);

                default:
                    throw new NotSupportedException("Only Equals and NotEquals operator are supported for null values.");
            }
        }
        

        /// <summary>
        /// Adds the where condition to the existing one
        /// </summary>
        /// <param name="where">Where condition to add</param>
        public virtual string GetNestedWhereCondition(string where)
        {
            if (String.IsNullOrEmpty(where))
            {
                return where;
            }

            return String.Format("({0})", where);
        }


        /// <summary>
        /// Adds the where condition to the existing one
        /// </summary>
        /// <param name="where">Where condition to add</param>
        /// <param name="condition">Condition to add</param>
        /// <param name="op">Operator</param>
        /// <param name="nested">If true, the where condition is added as nested</param>
        public virtual string AddWhereCondition(string where, string condition, string op = null, bool nested = true)
        {
            // If condition present, add
            if (!String.IsNullOrEmpty(condition))
            {
                // Add and if previous condition not empty
                if (!String.IsNullOrEmpty(where))
                {
                    if (where != condition)
                    {
                        if (op == null)
                        {
                            op = OperatorAND;
                        }

                        // Ensure brackets
                        if (nested)
                        {
                            where = GetNestedWhereCondition(where);
                            condition = GetNestedWhereCondition(condition);
                        }

                        where = String.Format("{0} {1} {2}", where, op, condition);
                    }
                }
                else
                {
                    where = condition;
                }
            }

            return where;
        }


        /// <summary>
        /// Gets the equals or empty expression
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        /// <param name="parameters">Query parameters</param>
        public string GetEqualsOrNull(string columnName, object value, ref QueryDataParameters parameters)
        {
            var representsNull = RepresentsNull(value);
            var equals = representsNull ? null : GetWhere(columnName, QueryOperator.Equals, value, ref parameters);

            var isnull = GetIsNull(columnName, false);

            var where = AddWhereCondition(equals, isnull, OperatorOR, false);
            if (!representsNull)
            {
                where = GetNestedWhereCondition(where);
            }

            return where;
        }


        /// <summary>
        /// Returns true if the given value represents database NULL
        /// </summary>
        /// <param name="value">Value to check</param>
        public static bool RepresentsNull(object value)
        {
            return (value == null) || (value == DBNull.Value);
        }

        #endregion
    }
}
