using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base for any object participating in the Data query evaluation
    /// </summary>
    public abstract class AbstractQueryObject : DisposableObject, IQueryObject
    {
        /// <summary>
        /// Object name, empty by default
        /// </summary>
        public virtual string Name
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Adds the data parameters to the current query parameters
        /// </summary>
        /// <param name="parameters">Parameters to add</param>
        /// <param name="expression">Expression which refers to the parameters</param>
        public abstract string IncludeDataParameters(QueryDataParameters parameters, string expression);


        /// <summary>
        /// Query parameters
        /// </summary>
        public abstract QueryDataParameters Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Applies this query parameters to the target object
        /// </summary>
        /// <param name="target">Target object defining parameters</param>
        public abstract void ApplyParametersTo(IQueryObject target);


        /// <summary>
        /// Creates the clone of the object.
        /// </summary>
        public abstract IQueryObject CloneObject();


        /// <summary>
        /// Marks the object as changed
        /// </summary>
        public virtual void Changed()
        {
            // Not tracked in the base class
        }


        /// <summary>
        /// Flushes the results but leaves the generated query text unchanged.
        /// After the reset, query can be executed again to obtain new data.
        /// </summary>
        public virtual void Reset()
        {
            // Not tracked in the base class
        }
    }
}
