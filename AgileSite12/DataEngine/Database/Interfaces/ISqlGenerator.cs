namespace CMS.DataEngine
{
    /// <summary>
    /// Sql generator interface.
    /// </summary>
    public interface ISqlGenerator
    {
        /// <summary>
        /// View name.
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="indexes">Returns extra code for the initialization of the view</param>
        string GetSystemViewSqlQuery(string viewName, out string indexes);


        /// <summary>
        /// Generates the given type of query for table specified by its className.
        /// </summary>
        /// <param name="className">Class name of the document data</param>
        /// <param name="queryType">Query type</param>
        /// <param name="siteName">CodeName of the site</param>
        string GetSqlQuery(string className, SqlOperationTypeEnum queryType, string siteName);
    }
}