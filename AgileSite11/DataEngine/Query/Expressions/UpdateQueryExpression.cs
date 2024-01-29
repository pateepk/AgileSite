using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides the values expression for the update query built from the given pairs of column names and respective values (or expressions)
    /// </summary>
    public class UpdateQueryExpression : QueryExpressionBase<UpdateQueryExpression>
    {
        /// <summary>
        /// Creates a new instance of <see cref="UpdateQueryExpression"/>.
        /// </summary>
        public UpdateQueryExpression()
        {
        }


        /// <summary>
        /// Creates a new instance of <see cref="UpdateQueryExpression"/> built from pairs of column / values.
        /// </summary>
        /// <param name="values">List of column-value pairs to update</param>
        public UpdateQueryExpression(IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            var sb = new StringBuilder();

            var parameters = EnsureParameters();

            // Add values and parameters
            foreach (var value in values)
            {
                var name = value.Key;

                // Include the value, it may be a more complex value such as QueryExpression
                var valueString = QueryDataParameters.IncludeValue(name, value.Value, ref parameters);

                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.AppendFormat(SqlHelper.UPDATE_FORMAT, name, valueString);
            }

            // Update the string expression
            Expression = sb.ToString();
        }
    }
}
