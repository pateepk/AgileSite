namespace CMS.DataEngine
{
    /// <summary>
    /// Query source interface
    /// </summary>
    public interface IQuerySource : IQueryParameters
    {
        /// <summary>
        /// Source expression
        /// </summary>
        string SourceExpression
        {
            get;
            set;
        }
    }
}