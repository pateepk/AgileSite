using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query column extension methods which provide XML querying capabilities.
    /// </summary>
    public static class QueryColumnXmlExtensions
    {
        /// <summary>
        /// Applies an XQuery against an XML column.
        /// </summary>
        /// <param name="column">Query column</param>
        /// <param name="xQuery">XML query</param>
        public static QueryColumn XmlQuery(this QueryColumn column, string xQuery)
        {
            var newResult = column.GetTypedQuery();

            newResult.Expression = String.Format("{0}.query('{1}')", newResult.GetValueExpression(), SqlHelper.EscapeQuotes(xQuery));
            newResult.ForceColumnBrackets = false;

            return newResult;
        }


        /// <summary>
        /// Applies an XML value method against an XML column.
        /// </summary>
        /// <param name="column">Query column</param>
        /// <param name="xQuery">XQuery expression</param>
        /// <param name="dataType">Data type to which the cast should be done. See <see cref="FieldDataType" /></param>
        /// <param name="size">Size of the type</param>
        /// <param name="precision">Precision of the type</param>
        public static QueryColumn XmlValue(this QueryColumn column, string xQuery, string dataType, int size = 0, int precision = 0)
        {
            var newResult = column.GetTypedQuery();

            var sqlType = DataTypeManager.GetSqlType(dataType, size, precision);

            newResult.Expression = String.Format("{0}.value('{1}', '{2}')", newResult.GetValueExpression(), SqlHelper.EscapeQuotes(xQuery), sqlType);
            newResult.ForceColumnBrackets = false;

            return newResult;
        }


        /// <summary>
        /// Applies an XML exists method against an XML column.
        /// </summary>
        /// <param name="column">Query column</param>
        /// <param name="xQuery">XQuery expression</param>
        public static QueryColumn XmlExists(this QueryColumn column, string xQuery)
        {
            var newResult = column.GetTypedQuery();

            newResult.Expression = String.Format("{0}.exist('{1}')", newResult.GetValueExpression(), SqlHelper.EscapeQuotes(xQuery));
            newResult.ForceColumnBrackets = false;

            return newResult;
        }
    }
}
