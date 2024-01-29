namespace CMS.DataEngine
{
    /// <summary>
    /// General interface for the query expression
    /// </summary>
    public interface IQueryExpression : IQueryParameters, IQueryObjectWithValue
    {
        /// <summary>
        /// Expression
        /// </summary>
        string Expression
        {
            get;
            set;
        }
    }
}