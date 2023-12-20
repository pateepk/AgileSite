namespace CMS.DataEngine
{
    /// <summary>
    /// General interface for the query columns
    /// </summary>
    public interface IQueryColumn : IQueryExpression
    {
        /// <summary>
        /// Returns true if this column represents a single column
        /// </summary>
        bool IsSingleColumn
        {
            get;
        }


        /// <summary>
        /// Converts the column to a column representing its alias
        /// </summary>
        IQueryColumn AsAlias();
    }
}
