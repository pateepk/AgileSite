using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base interface for all query objects
    /// </summary>
    public interface IQueryObject
    {
        /// <summary>
        /// Object name, empty by default
        /// </summary>
        string Name
        {
            get;
        }


        /// <summary>
        /// Query data parameters
        /// </summary>
        QueryDataParameters Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Creates the clone of the object.
        /// </summary>
        IQueryObject CloneObject();


        /// <summary>
        /// Adds the data parameters to the current query parameters
        /// </summary>
        /// <param name="parameters">Parameters to add</param>
        /// <param name="expression">Expression which refers to the parameters</param>
        string IncludeDataParameters(QueryDataParameters parameters, string expression);


        /// <summary>
        /// Marks the object as changed
        /// </summary>
        void Changed();
    }
}
