namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration for database engine edition of the instance of SQL Server installed on the server.
    /// http://msdn.microsoft.com/en-us/library/ms174396.aspx, EngineEdition property.
    /// </summary>
    public enum SQLEngineEditionEnum
    {
        /// <summary>
        /// Unknown engine edition.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Not available in SQL Server 2005 and later versions.
        /// </summary>
        PersonalOrDesktop = 1,

        /// <summary>
        /// This is returned for Standard, Web, and Business Intelligence.
        /// </summary>
        Standard = 2,

        /// <summary>
        /// This is returned for Evaluation, Developer, and both Enterprise editions.
        /// </summary>
        Enterprise = 3,

        /// <summary>
        /// This is returned for Express, Express with Tools and Express with Advanced Services
        /// </summary>
        Express = 4,

        /// <summary>
        /// This is returned for SQL Azure server or sometimes for any other SQL Database.
        /// It is not safe to use this property to decide whether is SQL server running on Azure or not.
        /// </summary>
        SQLAzure = 5
    }
}
