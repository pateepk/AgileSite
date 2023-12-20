using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Where condition builder - Generic base class
    /// </summary>
    public abstract class WhereConditionBase<TParent> : QueryParametersBase<TParent>, IWhereCondition<TParent>, IEquatable<TParent> 
        where TParent : WhereConditionBase<TParent>, new()
    {
        #region "Variables"

        /// <summary>
        /// Where condition
        /// </summary>
        private string mWhereCondition;


        /// <summary>
        /// Where condition builder
        /// </summary>
        private WhereBuilder mWhereBuilder;


        /// <summary>
        /// Returns true if query doesn't return any results
        /// </summary>
        private bool? mReturnsNoResults;


        private string mDataSourceName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Where condition builder
        /// </summary>
        protected WhereBuilder WhereBuilder
        {
            get
            {
                return mWhereBuilder ?? (mWhereBuilder = new WhereBuilder());
            }
        }


        /// <summary>
        /// Operator used for adding where condition. Default is AND
        /// </summary>
        protected string WhereOperator
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition on the data, e.g. "DocumentName = 'ABC'"
        /// </summary>
        public string WhereCondition
        {
            get
            {
                return mWhereCondition;
            }
            set
            {
                mWhereCondition = value;

                if (String.IsNullOrEmpty(value))
                {
                    WhereIsComplex = false;
                }

                Changed();
            }
        }


        /// <summary>
        /// Returns true if the given where condition contains compound conditions, e. g. "A > 1 AND B = 5"
        /// </summary>
        /// <remarks>
        /// Complex where condition will be surrounded by brackets if added to other where condition.
        /// </remarks>
        public bool WhereIsComplex
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the where condition is empty
        /// </summary>
        public bool WhereIsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(WhereCondition);
            }
        }


        /// <summary>
        /// Returns true if query doesn't return any results
        /// </summary>
        public bool ReturnsNoResults
        {
            get
            {
                return mReturnsNoResults.HasValue ? mReturnsNoResults.Value : CheckReturnsNoResults();
            }
        }


        /// <summary>
        /// Data source identifier that represents the location from which the data are obtained. 
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query. 
        /// </remarks>
        public string DataSourceName
        {
            get
            {
                return mDataSourceName ?? (mDataSourceName = GetDataSourceName());
            }
            set
            {
                mDataSourceName = value;
            }
        }


        /// <summary>
        /// Parent query object
        /// </summary>
        internal IDataQuery ParentQuery
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        protected WhereConditionBase()
        {
            ResetWhereOperator();
        }


        /// <summary>
        /// Applies this where condition to the target object
        /// </summary>
        /// <param name="target">Target object defining parameters</param>
        public override void ApplyParametersTo(IQueryObject target)
        {
            var t = target as IWhereCondition;
            if (t != null)
            {
                // Combine where conditions
                string where = t.IncludeDataParameters(Parameters, WhereCondition);

                t.WhereCondition = WhereBuilder.AddWhereCondition(t.WhereCondition, where, WhereBuilder.OperatorAND);
            }

            base.ApplyParametersTo(target);
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        /// <param name="target">Target class</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IWhereCondition;
            if (t != null)
            {
                t.WhereCondition = WhereCondition;
                t.WhereIsComplex = WhereIsComplex;
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Resets the where operator to the default value
        /// </summary>
        protected void ResetWhereOperator()
        {
            WhereOperator = WhereBuilder.OperatorAND;
        }


        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="leftSide">Left side of the condition</param>
        /// <param name="op">Operator</param>
        /// <param name="rightSide">Right side of the condition</param>
        protected string GetWhere(IQueryObjectWithValue leftSide, QueryOperator op, object rightSide)
        {
            return WhereBuilder.GetWhere(leftSide, op, rightSide, ref mParameters);
        }


        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        protected string GetWhere(string columnName, QueryOperator op, object value)
        {
            return WhereBuilder.GetWhere(columnName, op, value, ref mParameters);
        }
        

        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        private string GetWhere(string columnName, QueryUnaryOperator op)
        {
            return WhereBuilder.GetWhere(columnName, op);
        }


        /// <summary>
        /// Gets the where condition for the given column
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="op">Operator</param>
        private string GetWhere(IQueryObjectWithValue expression, QueryUnaryOperator op)
        {
            return WhereBuilder.GetWhere(expression, op);
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            var result = WhereCondition;

            if (expand)
            {
                result = Expand(result);
            }

            return result ?? string.Empty;
        }


        /// <summary>
        /// Adds the given where condition
        /// </summary>
        /// <param name="condition">Condition to add</param>
        /// <param name="negation">If true, the added where condition is negated</param>
        protected void AddWhereConditionInternal(IWhereCondition condition, bool negation = false)
        {
            if (condition == null)
            {
                return;
            }

            Using(condition);

            string where = condition.WhereCondition;
            QueryDataParameters parameters = condition.Parameters;

            // Include the data parameters to this query
            where = IncludeDataParameters(parameters, where);
            if (condition.WhereIsComplex)
            {
                where = WhereBuilder.GetNestedWhereCondition(where);
            }

            if (negation)
            {
                where = WhereBuilder.GetNegation(where);
            }

            // Add the final where condition
            AddWhereConditionInternal(where);
        }


        /// <summary>
        /// Adds the given where condition to the final where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        private void AddWhereConditionInternal(string where)
        {
            if (!String.IsNullOrEmpty(where))
            {
                var wasEmpty = WhereIsEmpty;

                WhereCondition = WhereBuilder.AddWhereCondition(WhereCondition, where, WhereOperator, false);
                ResetWhereOperator();

                if (!wasEmpty)
                {
                    WhereIsComplex = true;
                }
            }
        }


        /// <summary>
        /// Gets the starts with pattern for the like expression
        /// </summary>
        /// <param name="text">Text to match</param>
        private static string GetStartsWithPattern(string text)
        {
            text = SqlHelper.EscapeLikeQueryPatterns(text) + "%";

            return text;
        }


        /// <summary>
        /// Gets the ends with pattern for the like expression
        /// </summary>
        /// <param name="text">Text to match</param>
        private static string GetEndsWithPattern(string text)
        {
            text = "%" + SqlHelper.EscapeLikeQueryPatterns(text);

            return text;
        }


        /// <summary>
        /// Gets the contains pattern for the like expression
        /// </summary>
        /// <param name="text">Text to match</param>
        protected static string GetContainsPattern(string text)
        {
            text = "%" + SqlHelper.EscapeLikeQueryPatterns(text) + "%";

            return text;
        }


        /// <summary>
        /// Returns true if the object has any settings defined that influence the resulting query
        /// </summary>
        protected virtual bool AnySettingsDefined()
        {
            return !String.IsNullOrEmpty(WhereCondition);
        }


        /// <summary>
        /// Checks if where condition results in no data
        /// </summary>
        protected virtual bool CheckReturnsNoResults()
        {
            return WhereCondition == SqlHelper.NO_DATA_WHERE;
        }


        /// <summary>
        /// Gets data source identifier that represents the location from which the data are obtained. 
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query. 
        /// </remarks>
        protected virtual string GetDataSourceName()
        {
            // Default source
            return DataQuerySource.CMSDATABASE;
        }

        #endregion


        #region "Setup methods"

        /// <summary>
        /// Changes the where operator to AND for subsequent where conditions. Use in combination of methods Where...
        /// </summary>
        public TParent And()
        {
            var result = GetTypedQuery();
            result.WhereOperator = WhereBuilder.OperatorAND;

            return result;
        }


        /// <summary>
        /// Adds the given where condition with the AND operator
        /// </summary>
        public TParent And(IWhereCondition where)
        {
            return And().Where(where);
        }


        /// <summary>
        /// Adds the given where condition with the AND operator. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        public TParent And(Action<WhereCondition> where)
        {
            return And().Where(where);
        }


        /// <summary>
        /// Changes the where operator to OR for next where conditions. Use in combination of methods Where...
        /// </summary>
        public TParent Or()
        {
            var result = GetTypedQuery();
            result.WhereOperator = WhereBuilder.OperatorOR;

            return result;
        }


        /// <summary>
        /// Adds the given where condition with the OR operator
        /// </summary>
        public TParent Or(IWhereCondition where)
        {
            return Or().Where(where);
        }


        /// <summary>
        /// Adds the given where condition with the OR operator. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        public TParent Or(Action<WhereCondition> where)
        {
            return Or().Where(where);
        }


        /// <summary>
        /// Adds the given where condition to the final where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        protected TParent AddWhereCondition(string where)
        {
            var result = GetTypedQuery();

            result.AddWhereConditionInternal(where);

            return result;
        }


        /// <summary>
        /// Adds the where condition for a not null expression value
        /// </summary>
        /// <param name="expression">Expression</param>
        public TParent WhereNotNull(IQueryObjectWithValue expression)
        {
            return Where(expression, QueryOperator.NotEquals, null);
        }


        /// <summary>
        /// Adds the where condition for a not null column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        public TParent WhereNotNull(string columnName)
        {
            return Where(columnName, QueryOperator.NotEquals, null);
        }


        /// <summary>
        /// Adds the where condition for a null or empty column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        public TParent WhereEmpty(string columnName)
        {
            columnName = GetValidColumnName(columnName);
            string equals = GetWhere(columnName, QueryOperator.Equals, "");
            string isnull = WhereBuilder.GetIsNull(columnName, false);

            string where = WhereBuilder.GetNestedWhereCondition(WhereBuilder.AddWhereCondition(equals, isnull, WhereBuilder.OperatorOR, false));

            return AddWhereCondition(where);
        }


        /// <summary>
        /// Adds the where condition for a non empty column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        public TParent WhereNotEmpty(string columnName)
        {
            columnName = GetValidColumnName(columnName);
            string equals = GetWhere(columnName, QueryOperator.NotEquals, "");
            string isnotnull = WhereBuilder.GetIsNull(columnName, true);

            string where = WhereBuilder.GetNestedWhereCondition(WhereBuilder.AddWhereCondition(equals, isnotnull, WhereBuilder.OperatorAND, false));

            return AddWhereCondition(where);
        }


        /// <summary>
        /// Adds the where condition for a null expression value
        /// </summary>
        /// <param name="expression">Expression</param>
        public TParent WhereNull(IQueryObjectWithValue expression)
        {
            return Where(expression, QueryOperator.Equals, null);
        }


        /// <summary>
        /// Adds the where condition for a null column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        public TParent WhereNull(string columnName)
        {
            return Where(columnName, QueryOperator.Equals, null);
        }


        /// <summary>
        /// Adds the where condition for a true expression value (boolean expression equals true).
        /// </summary>
        /// <param name="expression">Column name</param>
        public TParent WhereTrue(IQueryObjectWithValue expression)
        {
            return Where(expression, QueryOperator.Equals, true);
        }


        /// <summary>
        /// Adds the where condition for a true column value (boolean column equals true).
        /// </summary>
        /// <param name="columnName">Column name</param>
        public TParent WhereTrue(string columnName)
        {
            return Where(columnName, QueryOperator.Equals, true);
        }


        /// <summary>
        /// Adds the where condition for a false expression value (boolean expression equals false).
        /// </summary>
        /// <param name="expression">Column name</param>
        public TParent WhereFalse(IQueryObjectWithValue expression)
        {
            return Where(expression, QueryOperator.Equals, false);
        }


        /// <summary>
        /// Adds the where condition for a false column value (boolean column equals false).
        /// </summary>
        /// <param name="columnName">Column name</param>
        public TParent WhereFalse(string columnName)
        {
            return Where(columnName, QueryOperator.Equals, false);
        }


        /// <summary>
        /// Adds where condition to the nested query, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="nestedQuery">Nested query</param>
        public TParent WhereIn(string columnName, IDataQuery nestedQuery)
        {
            return WhereIn(columnName, nestedQuery, false);
        }


        /// <summary>
        /// Adds where condition to the nested query, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="nestedQuery">Nested query</param>
        public TParent WhereNotIn(string columnName, IDataQuery nestedQuery)
        {
            return WhereIn(columnName, nestedQuery, true);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)".
        /// Supported generic type is int, long, string or guid ONLY!
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="nestedQuery">Nested query</param>
        /// <param name="negation">If true, the expression is NOT IN</param>
        protected TParent WhereIn(string columnName, IDataQuery nestedQuery, bool negation)
        {
            if (!HasCompatibleSource(nestedQuery))
            {
                // Check if materialization is allowed
                if (!nestedQuery.AllowMaterialization)
                {
                    CoreServices.EventLog.LogEvent("E", "DataQuery", "MATERIALIZE", String.Format(
@"{0}
Query is being materialized but does not allow materialization. 
If you want to allow materialization on this query, you need to set the AllowMaterialization property to true.

Query text:
{1}

Stack trace:
{2}
",
                        SystemContext.DevelopmentMode ? "NOTE: The development mode is enabled. In this mode all queries do not allow materialization by default to track unwanted materializations.\r\n" : null,
                        nestedQuery,
                        DebugHelper.GetStack()
                    ));
                }

                // For external source, materialize the query and select through result list
                nestedQuery = nestedQuery.AsSingleColumn();

                var ds = nestedQuery.Result;
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    var dt = ds.Tables[0];

                    // Check proper column type
                    var type = dt.Columns[0].DataType;

                    if (type == typeof(int))
                    {
                        return WhereIn(columnName, nestedQuery.GetListResult<int>(), negation);
                    }

                    if (type == typeof(string))
                    {
                        return WhereIn(columnName, nestedQuery.GetListResult<string>(), negation);
                    }

                    if (type == typeof(Guid))
                    {
                        return WhereIn(columnName, nestedQuery.GetListResult<Guid>(), negation);
                    }

                    if (type == typeof(long))
                    {
                        return WhereIn(columnName, nestedQuery.GetListResult<long>(), negation);
                    }

                    throw new NotSupportedException("[WhereConditionBase.WhereIn]: The result column type must be System.String, System.Int32, System.Int64 or System.Guid");
                }

                return WhereIn(columnName, new List<int>(), negation);
            }

            var result = GetTypedQuery();

            // Ensure single column in the nested query
            var subQuery = nestedQuery.AsSubQuery();

            result.Using(subQuery);

            // Include nested query parameters and get sub query text
            var subParameters = subQuery.GetCompleteQueryParameters();
            var subQueryText = subParameters.GetFullQueryText(EnsureParameters());

            // Get the where condition text
            string where = WhereBuilder.GetIn(GetValidColumnName(columnName), negation, subQueryText);

            result.AddWhereCondition(where);

            return result;
        }


        /// <summary>
        /// Returns true if the given query is an external source
        /// </summary>
        /// <param name="query">Nested query</param>
        public virtual bool HasCompatibleSource(IDataQuery query)
        {
            // If parent query is set, check the compatibility of the source against the parent query
            if (ParentQuery != null)
            {
                return ParentQuery.HasCompatibleSource(query);
            }

            return (query.DataSourceName == DataSourceName);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereIn(string columnName, ICollection<int> values)
        {
            return WhereIn(columnName, values, false);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereIn(string columnName, ICollection<string> values)
        {
            return WhereIn(columnName, values, false);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereIn(string columnName, ICollection<long> values)
        {
            return WhereIn(columnName, values, false);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereIn(string columnName, ICollection<Guid> values)
        {
            return WhereIn(columnName, values, false);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereNotIn(string columnName, ICollection<int> values)
        {
            return WhereIn(columnName, values, true);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereNotIn(string columnName, ICollection<string> values)
        {
            return WhereIn(columnName, values, true);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereNotIn(string columnName, ICollection<long> values)
        {
            return WhereIn(columnName, values, true);
        }



        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        public TParent WhereNotIn(string columnName, ICollection<Guid> values)
        {
            return WhereIn(columnName, values, true);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        /// <param name="negation">If true, the expression is NOT IN</param>
        protected TParent WhereIn(string columnName, ICollection<int> values, bool negation)
        {
            return WhereInGeneric(columnName, values, negation);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        /// <param name="negation">If true, the expression is NOT IN</param>
        protected TParent WhereIn(string columnName, ICollection<long> values, bool negation)
        {
            return WhereInGeneric(columnName, values, negation);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        /// <param name="negation">If true, the expression is NOT IN</param>
        protected TParent WhereIn(string columnName, ICollection<string> values, bool negation)
        {
            return WhereInGeneric(columnName, values, negation);
        }
        

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        /// <param name="negation">If true, the expression is NOT IN</param>
        protected TParent WhereIn(string columnName, ICollection<Guid> values, bool negation)
        {
            return WhereInGeneric(columnName, values, negation);
        }


        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"  Supported generic types are Int, String and Guid
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="values">The values.</param>
        /// <param name="negation">if set to true [negation].</param>
        private TParent WhereInGeneric<T>(string columnName, ICollection<T> values, bool negation)
        {
            var result = GetTypedQuery();
            if (negation && ((values == null) || (values.Count == 0)))
            {
                return result;
            }

            var parameters = result.EnsureParameters();
            var cond = new SelectCondition(parameters);

            result.Using(cond);

            cond.PrepareCondition(GetValidColumnName(columnName), values, negation);

            result.AddWhereCondition(cond.WhereCondition);

            return result;
        }


        /// <summary>
        /// Adds where condition with EXISTS and the nested query "EXISTS (...)"
        /// </summary>
        /// <param name="nestedQuery">Nested query</param>
        public TParent WhereExists(IDataQuery nestedQuery)
        {
            return WhereExists(nestedQuery, false);
        }


        /// <summary>
        /// Adds where condition with NOT EXISTS and the nested query "NOT EXISTS (...)"
        /// </summary>
        /// <param name="nestedQuery">Nested query</param>
        public TParent WhereNotExists(IDataQuery nestedQuery)
        {
            return WhereExists(nestedQuery, true);
        }


        /// <summary>
        /// Adds where condition with EXISTS and the nested query "EXISTS (...)"
        /// </summary>
        /// <param name="nestedQuery">Nested query</param>
        /// <param name="negation">If true, the expression is NOT EXISTS</param>
        protected TParent WhereExists(IDataQuery nestedQuery, bool negation)
        {
            var result = GetTypedQuery();

            // Ensure single column in the nested query
            var subQuery = nestedQuery.AsSubQuery();

            result.Using(subQuery);

            // Include nested query parameters and get query text
            var subParameters = subQuery.GetCompleteQueryParameters();
            var subQueryText = subParameters.GetFullQueryText(EnsureParameters());

            string where = WhereBuilder.GetExists(negation, subQueryText);

            result.AddWhereCondition(where);

            return result;
        }


        /// <summary>
        /// Adds the given where conditions to the query
        /// </summary>
        /// <param name="conditions">Nested where conditions</param>
        public TParent Where(params IWhereCondition[] conditions)
        {
            var result = GetTypedQuery();

            // Process all conditions
            foreach (var condition in conditions)
            {
                result.AddWhereConditionInternal(condition);
            }

            return result;
        }


        /// <summary>
        /// Adds the negation of the given where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        public TParent WhereNot(IWhereCondition where)
        {
            var result = GetTypedQuery();

            result.AddWhereConditionInternal(where, true);

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        /// <param name="condition">Nested where condition</param>
        public TParent Where(Action<WhereCondition> condition)
        {
            WhereCondition w = null;
            if (condition != null)
            {
                w = new WhereCondition { ParentQuery = this as IDataQuery };
                condition(w);
            }

            return Where(w);
        }


        /// <summary>
        /// Clears the current where condition
        /// </summary>
        public TParent NewWhere()
        {
            var result = GetTypedQuery();
            result.WhereCondition = null;

            return result;
        }


        /// <summary>
        /// Sets the where condition to exclude all data from result
        /// </summary>
        public TParent NoResults()
        {
            var result = GetTypedQuery();
            result.WhereCondition = SqlHelper.NO_DATA_WHERE;

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Query parameters</param>
        public TParent Where(string where, QueryDataParameters parameters = null)
        {
            var result = GetTypedQuery();

            // Include the parameters
            if (parameters != null)
            {
                where = result.IncludeDataParameters(parameters, where);
            }

            // Ensure brackets for unknown where condition
            where = WhereBuilder.GetNestedWhereCondition(where);

            result.AddWhereConditionInternal(where);

            return result;
        }


        /// <summary>
        /// Adds the condition for a string column to contain some substring
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        public TParent WhereContains(IQueryObjectWithValue expression, string value)
        {
            value = GetContainsPattern(value);

            return Where(expression, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the condition for a string column to contain some substring
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereContains(string columnName, string value)
        {
            value = GetContainsPattern(value);

            return Where(columnName, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the condition for a string column not to contain some substring
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        public TParent WhereNotContains(IQueryObjectWithValue expression, string value)
        {
            value = GetContainsPattern(value);

            return Where(expression, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the condition for a string column not to contain some substring
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereNotContains(string columnName, string value)
        {
            value = GetContainsPattern(value);

            return Where(columnName, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the condition for a string column to start with some prefix
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        public TParent WhereStartsWith(IQueryObjectWithValue expression, string value)
        {
            value = GetStartsWithPattern(value);

            return Where(expression, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the condition for a string column to start with some prefix
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereStartsWith(string columnName, string value)
        {
            value = GetStartsWithPattern(value);

            return Where(columnName, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the condition for a string column not to start with some prefix
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        public TParent WhereNotStartsWith(IQueryObjectWithValue expression, string value)
        {
            value = GetStartsWithPattern(value);

            return Where(expression, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the condition for a string column not to start with some prefix
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereNotStartsWith(string columnName, string value)
        {
            value = GetStartsWithPattern(value);

            return Where(columnName, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the condition for a string expression to end with some prefix
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        public TParent WhereEndsWith(IQueryObjectWithValue expression, string value)
        {
            value = GetEndsWithPattern(value);

            return Where(expression, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the condition for a string column to end with some prefix
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereEndsWith(string columnName, string value)
        {
            value = GetEndsWithPattern(value);

            return Where(columnName, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the condition for a string column not to end with some prefix
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        public TParent WhereNotEndsWith(IQueryObjectWithValue expression, string value)
        {
            value = GetEndsWithPattern(value);

            return Where(expression, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the condition for a string column not to end with some prefix
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereNotEndsWith(string columnName, string value)
        {
            value = GetEndsWithPattern(value);

            return Where(columnName, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value.
        /// </summary>
        /// <param name="leftSide">Column name</param>
        /// <param name="op">Operator</param>
        /// <param name="rightSide">Value</param>
        public TParent Where(IQueryObjectWithValue leftSide, QueryOperator op, object rightSide)
        {
            var result = GetTypedQuery();

            string where = result.GetWhere(leftSide, op, rightSide);

            result.AddWhereCondition(where);

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        public TParent Where(string columnName, QueryOperator op, object value)
        {
            var result = GetTypedQuery();

            string where = result.GetWhere(GetValidColumnName(columnName), op, value);

            result.AddWhereCondition(where);

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column value with an unary operator.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="op">Operator</param>
        public TParent Where(IQueryObjectWithValue expression, QueryUnaryOperator op)
        {
            var result = GetTypedQuery();

            string where = GetWhere(expression, op);

            result.AddWhereCondition(where);

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column value with an unary operator.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        public TParent Where(string columnName, QueryUnaryOperator op)
        {
            var result = GetTypedQuery();

            string where = GetWhere(GetValidColumnName(columnName), op);

            result.AddWhereCondition(where);

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value or null value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereEqualsOrNull(string columnName, object value)
        {
            string where = WhereBuilder.GetEqualsOrNull(GetValidColumnName(columnName), value, ref mParameters);

            return AddWhereCondition(where);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side to the given right side.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereEquals(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.Equals, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereEquals(string columnName, object value)
        {
            return Where(columnName, QueryOperator.Equals, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is not equal to the right side.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereNotEquals(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.NotEquals, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column which is not equal to a given value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereNotEquals(string columnName, object value)
        {
            return Where(columnName, QueryOperator.NotEquals, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is less than the right side.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereLessThan(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.LessThan, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is less or equal than the right side.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereLessOrEquals(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.LessOrEquals, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is greater than the right side.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereGreaterThan(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.LargerThan, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is greater or equal than the right side.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereGreaterOrEquals(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.LargerOrEquals, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is less than the right side.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereLessThan(string columnName, object value)
        {
            return Where(columnName, QueryOperator.LessThan, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is less or equal than the right side.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereLessOrEquals(string columnName, object value)
        {
            return Where(columnName, QueryOperator.LessOrEquals, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is greater than the right side.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereGreaterThan(string columnName, object value)
        {
            return Where(columnName, QueryOperator.GreaterThan, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side which is greater or equal than the right side.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereGreaterOrEquals(string columnName, object value)
        {
            return Where(columnName, QueryOperator.GreaterOrEquals, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side and right side using LIKE operator.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereLike(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.Like, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value using LIKE operator.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereLike(string columnName, string value)
        {
            return Where(columnName, QueryOperator.Like, value);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the left side and right side using NOT LIKE operator.
        /// </summary>
        /// <param name="leftSide">Left side</param>
        /// <param name="rightSide">Right side</param>
        public TParent WhereNotLike(IQueryObjectWithValue leftSide, object rightSide)
        {
            return Where(leftSide, QueryOperator.NotLike, rightSide);
        }


        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value using NOT LIKE operator.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        public TParent WhereNotLike(string columnName, string value)
        {
            return Where(columnName, QueryOperator.NotLike, value);
        }


        /// <summary>
        /// Adds the where condition to match the ID to the query. In case the column name is not provided or unknown, does not generate where condition. If given ID is invalid, adds the condition to match NULL.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="id">ID</param>
        public TParent WhereID(string columnName, int id)
        {
            if ((columnName == null) || (columnName == ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                return GetTypedQuery();
            }

            if (id > 0)
            {
                return Where(columnName, QueryOperator.Equals, id);
            }

            return WhereNull(columnName);
        }


        /// <summary>
        /// Sets the query to return no results. This action is irreversible, once the query is set to return no results it cannot be changed. 
        /// This method is used by data engine to forbid access to data that are not allowed to be accessed (e.g. license limitations), without notifying the process about the fact.
        /// </summary>
        public void ReturnNoResults()
        {
            mReturnsNoResults = true;
        }


        /// <summary>
        /// Returns column name in string format from query column expression (Ensures square brackets)
        /// </summary>
        /// <param name="columnName">Column name</param>
        private static string GetValidColumnName(string columnName)
        {
            if (QueryColumn.IsBasicColumnName(columnName))
            {
                return SqlHelper.AddSquareBrackets(columnName);
            }
            return columnName;
        }

        #endregion


        #region "Equatable methods"

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals((TParent)obj);
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TParent other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var whereEquals = (mWhereCondition == null) ? (other.mWhereCondition == null) : mWhereCondition.EqualsCSafe(other.mWhereCondition, true);
            var parametersEquals = (mParameters == null) ? (other.mParameters == null) : mParameters.Equals(other.mParameters);

            return whereEquals && parametersEquals;
        }


        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ((mWhereCondition != null) ? mWhereCondition.GetHashCode() : 0);
                hash = hash * 23 + ((mParameters != null) ? mParameters.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion
    }
}
