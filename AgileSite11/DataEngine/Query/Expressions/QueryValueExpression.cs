using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query value expression
    /// </summary>
    public class QueryValueExpression : QueryExpressionBase<QueryValueExpression>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public QueryValueExpression()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="name">Value name</param>
        /// <param name="expand">If true, expands the value to a constant</param>
        public QueryValueExpression(object value, string name = null, bool expand = false)
        {
            if (value is DataParameter)
            {
                // Initialize as existing parameter 
                var parameters = EnsureParameters();
                var par = value as DataParameter;
                
                par = parameters.AddUnique(par);
                
                Expression = par.Name;
            }
            else if (expand)
            {
                // Initialize as expanded value
                Expression = SqlHelper.GetSqlValue(value);
            }
            else
            {
                // Initialize as parameter 
                var parameters = EnsureParameters();
                var par = parameters.Add(name ?? "Value", value);

                Expression = par.Name;
            }
        }


        /// <summary>
        /// Changes the name of the expression parameter to the given name
        /// </summary>
        /// <param name="name">Name for the parameter</param>
        public QueryValueExpression As(string name)
        {
            Parameters[0].Name = name;

            return this;
        }
    }
}
