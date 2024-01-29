namespace CMS.DataEngine
{
    /// <summary>
    /// Query parameters interface.
    /// </summary>
    /// <typeparam name="TParent">Type of Query instance.</typeparam>
    public interface IQueryParameters<TParent> : IQueryParameters
    {
        /// <summary>
        /// Returns specifically typed instance of current Query object to allow user to compose Query in fluent syntax.
        /// </summary>
        TParent GetTypedQuery();
    }


    /// <summary>
    /// Query parameters interface.
    /// </summary>
    public interface IQueryParameters : IQueryObject
    {
        /// <summary>
        /// Applies this where condition to the target object
        /// </summary>
        /// <param name="target">Target object defining parameters</param>
        void ApplyParametersTo(IQueryObject target);


        /// <summary>
        /// Copies the properties to the target query.
        /// </summary>
        /// <param name="target">Target object defining properties</param>
        void CopyPropertiesTo(IQueryObject target);


        /// <summary>
        /// Expands the expression by replacing parameters with their values
        /// </summary>
        /// <param name="expression">Expression to expand</param>
        string Expand(string expression);


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        string ToString(bool expand);


        /// <summary>
        /// Ensures data parameters for the given query
        /// </summary>
        QueryDataParameters EnsureParameters();
    }
}