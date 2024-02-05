using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class that returns SQL server capabilities based on the connection string and server edition.
    /// </summary>
    public static class SqlServerCapabilitiesFactory
    {
        /// <summary>
        /// Returns SQL server capabilities based on the connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to SQL server</param>
        /// <returns>Instance of SQLServerCapabilities class with data</returns>
        public static ISqlServerCapabilities GetSqlServerCapabilities(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("[SqlServerCapabilitiesFactory:GetSqlServerCapabilities]: Connection string must be defined.");
            }

            var engineEdition = SqlInstallationHelper.GetEngineEdition(connectionString);
            bool runningOnAzureSql = engineEdition == SQLEngineEditionEnum.SQLAzure;

            return new SqlServerCapabilities
            {
                // Azure supports database creation, however it is necessary to specify more options than on the regular SQL server.
                // Now it creates Web(retired) version by default which is not acceptable.
                // More at http://msdn.microsoft.com/en-us/library/dn268335.aspx
                SupportsDatabaseCreation = !runningOnAzureSql,
                
                // Deletion of database on Azure has to be done within the master database, it is not common behavior.
                SupportsDatabaseDeletion = !runningOnAzureSql,

                // OpenQuery is not supported on Azure SQL.
                // More at http://msdn.microsoft.com/en-us/library/azure/ee336253.aspx
                SupportsOpenQueryCommand = !runningOnAzureSql,

                // System stored procedure 'sp_testlinkedserver' is not available on Azure SQL.
                SupportsLinkedServer = !runningOnAzureSql,

                // CONTROL SERVER permission is not available on Azure
                ControlServerPermissionAvailable = !runningOnAzureSql
            };
        }
    }
}