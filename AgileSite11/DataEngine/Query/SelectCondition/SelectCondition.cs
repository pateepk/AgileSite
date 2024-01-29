using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.SqlServer.Server;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides the selection where condition for ABC IN (1, 2, 3, 4, 5, ...) for very large number of items.
    /// </summary>
    public class SelectCondition : IDisposable
    {
        #region "Variables"

        /// <summary>
        /// Constant for Inline limit property defining that all items should be processed inline
        /// </summary>
        public const int ALL_INLINE = -1;


        /// <summary>
        /// Constant for Inline limit property defining that all items should be processed as table-valued parameter.
        /// </summary>
        public const int ALL_TABLE_VALUED_PARAMETER = 0;


        /// <summary>
        /// Limit of the number of items for the inline evaluation.
        /// </summary>
        protected int mInlineLimit = SqlHelper.DefaultSQLInlineLimit;


        /// <summary>
        /// Where condition.
        /// </summary>
        protected string mWhereCondition;


        /// <summary>
        /// Group GUID for the temp table.
        /// </summary>
        protected Guid mGroupGUID = Guid.Empty;


        /// <summary>
        /// Query parameters.
        /// </summary>
        protected QueryDataParameters mParameters;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns the resulting where condition.
        /// </summary>
        public string WhereCondition
        {
            get
            {
                return mWhereCondition;
            }
        }


        /// <summary>
        /// Returns true if the selection is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return (mWhereCondition == SqlHelper.NO_DATA_WHERE);
            }
        }


        /// <summary>
        /// Limit of the number of items for the inline evaluation.
        /// 
        /// If there are more items than this limit, the where condition is returned in format:
        /// ColumnName IN (SELECT * FROM @List)), where @List is of the following SQL user-defined table types: Type_CMS_IntegerTable, Type_CMS_BigIntTable, Type_CMS_StringTable, Type_CMS_GuidTable
        /// 
        /// Otherwise, inline format is used:
        /// ColumnName IN (Value1, Value2, ...)
        /// 
        /// If set to -1, inline format is always used.
        /// </summary>
        /// <remarks>The default value depends on CMSDefaultSQLInlineLimit setting, which has a default value of 1000.</remarks>
        public int InlineLimit
        {
            get
            {
                return mInlineLimit;
            }
            set
            {
                mInlineLimit = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor, prepares empty condition object.
        /// </summary>
        public SelectCondition() 
            : this(null)
        {           
        }


        /// <summary>
        /// Constructor, prepares empty condition object bound to specific query parameters.
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        public SelectCondition(QueryDataParameters parameters)
        {
            mParameters = parameters ?? new QueryDataParameters();
        }


        /// <summary>
        /// Prepares the select condition for specific value types
        /// </summary>
        /// <typeparam name="T">Method will prepare the condition over <see cref="System.Int32"/>, <see cref="System.Int64"/>, <see cref="System.String"/> or <see cref="System.Guid"/>.</typeparam>
        /// <remarks>
        /// Null values are ignored.
        /// </remarks>
        /// <param name="columnName">Column name</param>
        /// <param name="values">Values for the IN expression</param>
        /// <param name="negation">Indicates if the negation should be used in the condition (Column NOT IN (1, 2, 3 ...))</param>
        /// <remarks>If <paramref name="values"></paramref> contains only one item, WhereEquals condition is used</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when unsupported value type is used.</exception>
        public void PrepareCondition<T>(string columnName, ICollection<T> values, bool negation = false)
        {
            if (String.IsNullOrEmpty(columnName))
            {
                return;
            }

            var type = typeof(T);
            if (type != typeof(int) && type != typeof(long) && type != typeof(string) && type != typeof(Guid))
            {
                throw new NotSupportedException("Unsupported value type '" + type.Name + "'. Select condition supports only System.Int, System.String or System.Guid types.");
            }

            if ((values == null) || (values.Count == 0) || (type == typeof(string) && values.All(x => x == null)))
            {
                // No value - always return false
                mWhereCondition = SqlHelper.NO_DATA_WHERE;
                return;
            }

            var distinctValues = values.Distinct().ToList();

            if ((InlineLimit != ALL_INLINE) && (InlineLimit == ALL_TABLE_VALUED_PARAMETER || distinctValues.Count >= InlineLimit))
            {
                IEnumerable<SqlDataRecord> valueTable;

                // Use nested select for evaluation
                if (type == typeof(int))
                {
                    valueTable = SqlHelper.BuildIntTable(distinctValues.Cast<int>());
                }
                else if (type == typeof(string))
                {
                    valueTable = SqlHelper.BuildStringTable(distinctValues.Cast<string>());
                }
                else if (type == typeof(Guid))
                {
                    valueTable = SqlHelper.BuildGuidTable(distinctValues.Cast<Guid>());
                }
                else
                {
                    valueTable = SqlHelper.BuildBigIntTable(distinctValues.Cast<long>());
                }

                mWhereCondition = PrepareWhereCondition<T>(columnName, valueTable, negation);
            }
            else
            {
                // Use standard inline evaluation
                mWhereCondition = SqlHelper.GetWhereInCondition(columnName, distinctValues, negation, false);
            }
        }


        /// <summary>
        /// Prepares a query parameter with unique name.
        /// </summary>
        /// <param name="values">Rows to insert to the table-valued parameter</param>
        internal string PrepareParameter<T>(IEnumerable<SqlDataRecord> values)
        {
            var param = new DataParameter("@List", values);
            param.Type = typeof(IEnumerable<T>);
            var addedParam = mParameters.AddUnique(param, false);

            return addedParam.Name;
        }


        /// <summary>
        /// Disposes the object and removes the selection from the database if it was allocated in the temp table.
        /// </summary>
        public void Dispose()
        {
        }


        /// <summary>
        /// Prepares the where condition for the given list.
        /// </summary>
        /// <param name="columnName">Column name for the where condition</param>
        /// <param name="values">Distinct values for the where condition</param>
        /// <param name="negation">Indicates if the negation should be used in the condition (ABC NOT IN (1, 2, 3 ...))</param>
        private string PrepareWhereCondition<T>(string columnName, IEnumerable<SqlDataRecord> values, bool negation)
        {
            var op = negation ? "NOT IN" : "IN";
            var param = PrepareParameter<T>(values);

            return $"{columnName} {op} (SELECT * FROM {param})";
        }

        #endregion
    }
}