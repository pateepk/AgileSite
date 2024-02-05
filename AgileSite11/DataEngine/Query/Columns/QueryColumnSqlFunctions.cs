using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides extension methods with SQL functions applicable to the <see cref="QueryColumn"/> object.
    /// </summary>
    public static class QueryColumnSqlFunctions
    {
        /// <summary>
        /// Applies CAST function to the column.
        /// </summary>
        /// <param name="column">Column used for required function.</param>
        /// <param name="dataType">Data type to which the cast should be done. Use constants defined in <see cref="FieldDataType"/> class.</param>
        /// <param name="size">Size of the type</param>
        /// <param name="precision">Precision of the type</param>
        public static QueryColumn Cast(this QueryColumn column, string dataType, int size = 0, int precision = 0)
        {
            var newResult = column.GetTypedQuery();

            var sqlType = DataTypeManager.GetSqlType(dataType, size, precision);

            newResult.Expression = String.Format("CAST({0} AS {1})", newResult.GetValueExpression(), sqlType);
            newResult.ForceColumnBrackets = false;

            return newResult;
        }


        /// <summary>
        /// Applies ISNULL function to the column.
        /// </summary>
        /// <param name="column">Column used for required function.</param>
        /// <param name="nullReplacement">Replacement expression used instead of null value. Value must be of a type that is implicitly convertible to the type of the column.</param>
        public static QueryColumn IsNull(this QueryColumn column, object nullReplacement)
        {
            var newResult = column.GetTypedQuery();

            var defValue = DataTypeManager.GetSqlValue(nullReplacement);

            newResult.Expression = String.Format("ISNULL({0}, {1})", newResult.GetValueExpression(), defValue);
            newResult.ForceColumnBrackets = false;

            return newResult;
        }
    }
}
