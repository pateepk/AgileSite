namespace CMS.DataEngine
{
    /// <summary>
    /// Base interface for all query objects with value
    /// </summary>
    public interface IQueryObjectWithValue : IQueryObject
    {
        /// <summary>
        /// Gets a string expression representing this object
        /// </summary>
        string GetExpression();


        /// <summary>
        /// Gets a query expression representing this object as a value
        /// </summary>
        IQueryExpression AsValue();
    }
}