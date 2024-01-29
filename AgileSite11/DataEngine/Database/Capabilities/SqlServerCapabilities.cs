namespace CMS.DataEngine
{
    /// <summary>
    /// Class that holds information about SQL server capabilities.
    /// 
    /// These are features of the SQL server itself.
    /// It doesn't depend on the user and his permissions.
    /// </summary>
    internal class SqlServerCapabilities : ISqlServerCapabilities
    {
        /// <summary>
        /// Indicate if server supports database creation.
        /// </summary>
        public bool SupportsDatabaseCreation
        {
            get;
            set;
        }


        /// <summary>
        /// Indicate if server supports database deletion.
        /// </summary>
        public bool SupportsDatabaseDeletion
        {
            get;
            set;
        }


        /// <summary>
        /// Indicate if server supports OPENQUERY commands.
        /// 
        /// OPENQUERY is used to do command across two or more databases.
        /// http://msdn.microsoft.com/en-us/library/ms188427.aspx
        /// </summary>
        public bool SupportsOpenQueryCommand
        {
            get;
            set;
        }


        /// <summary>
        /// Indicate if server supports linked server which is used with in OPENQUERY to do commands across two or more databases.
        /// </summary>
        public bool SupportsLinkedServer
        {
            get;
            set;
        }


        /// <summary>
        /// Indicate if server supports 'CONTROL SERVER' permission for user.
        /// http://msdn.microsoft.com/en-us/library/ms191291.aspx
        /// </summary>
        public bool ControlServerPermissionAvailable
        {
            get;
            set;
        }
    }
}
