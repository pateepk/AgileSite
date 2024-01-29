namespace CMS.DataEngine
{
    /// <summary>
    /// Sql server capabilities contract.
    /// </summary>
    public interface ISqlServerCapabilities
    {
        /// <summary>
        /// Indicate if server supports database creation.
        /// </summary>
        bool SupportsDatabaseCreation
        {
            get;
        }


        /// <summary>
        /// Indicate if server supports database deletion.
        /// </summary>
        bool SupportsDatabaseDeletion
        {
            get;
        }


        /// <summary>
        /// Indicate if server supports OPENQUERY commands.
        /// 
        /// OPENQUERY is used to do commands across two or more databases.
        /// http://msdn.microsoft.com/en-us/library/ms188427.aspx
        /// </summary>
        bool SupportsOpenQueryCommand
        {
            get;
        }


        /// <summary>
        /// Indicate if server supports linked server which is used with in OPENQUERY to do commands across two or more databases.
        /// </summary>
        bool SupportsLinkedServer
        {
            get;
        }


        /// <summary>
        /// Indicate if server supports 'CONTROL SERVER' permission for user.
        /// http://msdn.microsoft.com/en-us/library/ms191291.aspx
        /// </summary>
        bool ControlServerPermissionAvailable
        {
            get;
        }
    }
}