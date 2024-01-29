using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for classes which provide where condition for a specific query type
    /// </summary>
    public interface IWhereCondition<TParent> : IWhereCondition, IQueryParameters<TParent>
    {
        /// <summary>
        /// Changes the where operator to AND for subsequent where conditions. Use in combination of methods Where...
        /// </summary>
        TParent And();

        /// <summary>
        /// Adds the given where condition with the AND operator
        /// </summary>
        TParent And(IWhereCondition where);

        /// <summary>
        /// Adds the given where condition with the AND operator. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        TParent And(Action<WhereCondition> where);

        /// <summary>
        /// Changes the where operator to OR for next where conditions. Use in combination of methods Where...
        /// </summary>
        TParent Or();

        /// <summary>
        /// Adds the given where condition with the OR operator
        /// </summary>
        TParent Or(IWhereCondition where);

        /// <summary>
        /// Adds the given where condition with the OR operator. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        TParent Or(Action<WhereCondition> where);

        /// <summary>
        /// Adds the where condition for a not null column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        TParent WhereNotNull(string columnName);

        /// <summary>
        /// Adds the where condition for a null or empty column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        TParent WhereEmpty(string columnName);

        /// <summary>
        /// Adds the where condition for a non empty column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        TParent WhereNotEmpty(string columnName);

        /// <summary>
        /// Adds the where condition for a null column value
        /// </summary>
        /// <param name="columnName">Column name</param>
        TParent WhereNull(string columnName);

        /// <summary>
        /// Adds the where condition for a true column value (boolean column equals true).
        /// </summary>
        /// <param name="columnName">Column name</param>
        TParent WhereTrue(string columnName);

        /// <summary>
        /// Adds the where condition for a false column value (boolean column equals false).
        /// </summary>
        /// <param name="columnName">Column name</param>
        TParent WhereFalse(string columnName);

        /// <summary>
        /// Adds where condition to the nested query, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="nestedQuery">Nested query</param>
        TParent WhereIn(string columnName, IDataQuery nestedQuery);

        /// <summary>
        /// Adds where condition to the nested query, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="nestedQuery">Nested query</param>
        TParent WhereNotIn(string columnName, IDataQuery nestedQuery);

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        TParent WhereIn(string columnName, ICollection<int> values);

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        TParent WhereIn(string columnName, ICollection<string> values);

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        TParent WhereIn(string columnName, ICollection<Guid> values);

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        TParent WhereNotIn(string columnName, ICollection<int> values);

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        TParent WhereNotIn(string columnName, ICollection<string> values);

        /// <summary>
        /// Adds where condition to the list of values, e.g. "columnName NOT IN (...)"
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="values">List of values for the query</param>
        TParent WhereNotIn(string columnName, ICollection<Guid> values);

        /// <summary>
        /// Adds where condition with EXISTS and the nested query "EXISTS (...)"
        /// </summary>
        /// <param name="nestedQuery">Nested query</param>
        TParent WhereExists(IDataQuery nestedQuery);

        /// <summary>
        /// Adds where condition with NOT EXISTS and the nested query "NOT EXISTS (...)"
        /// </summary>
        /// <param name="nestedQuery">Nested query</param>
        TParent WhereNotExists(IDataQuery nestedQuery);
        /// <summary>
        /// Adds the given where conditions to the query
        /// </summary>
        /// <param name="conditions">Nested where conditions</param>
        TParent Where(params IWhereCondition[] conditions);

        /// <summary>
        /// Adds the negation of the given where condition
        /// </summary>
        /// <param name="where">Where condition</param>
        TParent WhereNot(IWhereCondition where);

        /// <summary>
        /// Adds the given where condition to the query. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        /// <param name="condition">Nested where condition</param>
        TParent Where(Action<WhereCondition> condition);

        /// <summary>
        /// Clears the current where condition
        /// </summary>
        TParent NewWhere();

        /// <summary>
        /// Sets the where condition to exclude all data from result
        /// </summary>
        TParent NoResults();

        /// <summary>
        /// Adds the given where condition to the query
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Query parameters</param>
        TParent Where(string where, QueryDataParameters parameters = null);

        /// <summary>
        /// Adds the condition for a string column to contain some substring
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereContains(string columnName, string value);

        /// <summary>
        /// Adds the condition for a string column not to contain some substring
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereNotContains(string columnName, string value);

        /// <summary>
        /// Adds the condition for a string column to start with some prefix
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereStartsWith(string columnName, string value);

        /// <summary>
        /// Adds the condition for a string column not to start with some prefix
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereNotStartsWith(string columnName, string value);

        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        TParent Where(string columnName, QueryOperator op, object value);

        /// <summary>
        /// Adds the given where condition to the query. Matches the column value with an unary operator.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        TParent Where(string columnName, QueryUnaryOperator op);

        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value or null value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereEqualsOrNull(string columnName, object value);

        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereEquals(string columnName, object value);

        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value using LIKE operator.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereLike(string columnName, string value);

        /// <summary>
        /// Adds the given where condition to the query. Matches the column to a given value using NOT LIKE operator.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        TParent WhereNotLike(string columnName, string value);

        /// <summary>
        /// Adds the where condition to match the ID to the query. In case the column name is not provided or unknown, does not generate where condition. If given ID is invalid, adds the condition to match NULL.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="id">ID</param>
        TParent WhereID(string columnName, int id);
    }


    /// <summary>
    /// Interface for classes which provide where condition
    /// </summary>
    public interface IWhereCondition : IQueryParameters, IDisposable
    {
        /// <summary>
        /// Data source identifier that represents the location from which the data are obtained. 
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query. 
        /// </remarks>
        string DataSourceName
        {
            get;
            set;
        }

        /// <summary>
        /// Where condition on the data, e.g. "DocumentName = 'ABC'"
        /// </summary>
        string WhereCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if the given where condition contains compound conditions, e. g. "A > 1 AND B = 5"
        /// </summary>
        bool WhereIsComplex
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if the where condition is empty
        /// </summary>
        bool WhereIsEmpty
        {
            get;
        }

        /// <summary>
        /// Sets the query to return no results. This action is irreversible, once the query is set to return no results it cannot be changed. 
        /// This method is used by data engine to forbid access to data that are not allowed to be accessed (e.g. license limitations), without notifying the process about the fact.
        /// </summary>
        void ReturnNoResults();
    }
}
