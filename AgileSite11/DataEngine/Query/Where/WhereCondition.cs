using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Where condition builder
    /// </summary>
    public class WhereCondition : WhereConditionBase<WhereCondition>
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// Conditions created using the constructor don't contain a data source identifier.
        /// Source identifiers are used to determine whether sub queries (<see cref="WhereConditionBase{T}.WhereIn(string, IDataQuery)"/>) are materialized or inserted directly into the parent query.
        /// For example, a sub query selecting objects from a separated online marketing database needs to be materialized when the parent query is executed against the main database.
        /// </para>
        /// <para>
        /// If you know the data type that the new condition works with, create the condition using the <see cref="ObjectTypeInfo.CreateWhereCondition"/> method.
        /// You can also set the data source manually through the <see cref="WhereConditionBase{T}.DataSourceName"/> property.
        /// </para>
        /// </remarks>
        public WhereCondition()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <remarks>
        /// <para>
        /// Conditions created using the constructor don't contain a data source identifier.
        /// Source identifiers are used to determine whether sub queries (<see cref="WhereConditionBase{T}.WhereIn(string, IDataQuery)"/>) are materialized or inserted directly into the parent query.
        /// For example, a sub query selecting objects from a separated online marketing database needs to be materialized when the parent query is executed against the main database.
        /// </para>
        /// <para>
        /// If you know the data type that the new condition works with, create the condition using the <see cref="ObjectTypeInfo.CreateWhereCondition"/> method.
        /// You can also set the data source manually through the <see cref="WhereConditionBase{T}.DataSourceName"/> property.
        /// </para>
        /// </remarks>
        public WhereCondition(string whereCondition)
        {
            WhereCondition = whereCondition;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        /// <remarks>
        /// <para>
        /// Conditions created using the constructor don't contain a data source identifier.
        /// Source identifiers are used to determine whether sub queries (<see cref="WhereConditionBase{T}.WhereIn(string, IDataQuery)"/>) are materialized or inserted directly into the parent query.
        /// For example, a sub query selecting objects from a separated online marketing database needs to be materialized when the parent query is executed against the main database.
        /// </para>
        /// <para>
        /// If you know the data type that the new condition works with, create the condition using the <see cref="ObjectTypeInfo.CreateWhereCondition"/> method.
        /// You can also set the data source manually through the <see cref="WhereConditionBase{T}.DataSourceName"/> property.
        /// </para>
        /// </remarks>
        public WhereCondition(string columnName, QueryOperator op, object value)
        {
            Where(columnName, op, value);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="op">Operator</param>
        /// <remarks>
        /// <para>
        /// Conditions created using the constructor don't contain a data source identifier.
        /// Source identifiers are used to determine whether sub queries (<see cref="WhereConditionBase{T}.WhereIn(string, IDataQuery)"/>) are materialized or inserted directly into the parent query.
        /// For example, a sub query selecting objects from a separated online marketing database needs to be materialized when the parent query is executed against the main database.
        /// </para>
        /// <para>
        /// If you know the data type that the new condition works with, create the condition using the <see cref="ObjectTypeInfo.CreateWhereCondition"/> method.
        /// You can also set the data source manually through the <see cref="WhereConditionBase{T}.DataSourceName"/> property.
        /// </para>
        /// </remarks>
        public WhereCondition(string columnName, QueryUnaryOperator op)
        {
            Where(columnName, op);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conditions">Creates a where condition from the given where conditions</param>
        /// <remarks>
        /// <para>
        /// Conditions created using the constructor don't contain a data source identifier.
        /// Source identifiers are used to determine whether sub queries (<see cref="WhereConditionBase{T}.WhereIn(string, IDataQuery)"/>) are materialized or inserted directly into the parent query.
        /// For example, a sub query selecting objects from a separated online marketing database needs to be materialized when the parent query is executed against the main database.
        /// </para>
        /// <para>
        /// If you know the data type that the new condition works with, create the condition using the <see cref="ObjectTypeInfo.CreateWhereCondition"/> method.
        /// You can also set the data source manually through the <see cref="WhereConditionBase{T}.DataSourceName"/> property.
        /// </para>
        /// </remarks>
        public WhereCondition(params IWhereCondition[] conditions)
        {
            // Add all conditions
            foreach (var condition in conditions)
            {
                AddWhereConditionInternal(condition);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a new where condition from the given parameters
        /// </summary>
        /// <param name="where">Where condition parameters</param>
        public static WhereCondition From(Action<WhereCondition> where)
        {
            WhereCondition w = null;
            if (where != null)
            {
                w = new WhereCondition();
                where(w);
            }

            return w;
        }

        #endregion
    }
}
