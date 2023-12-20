using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Ensures that the wrapped API calls are executed against specific database(s).
    /// </summary>
    public class CMSConnectionContext : IDisposable
    {
        /// <summary>
        /// Connection string prefix from the original context
        /// </summary>
        protected string OriginalPrefix
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor, sets up the connection string name prefix for the life time of this object. E.g. for prefix "External" the connection string "ExternalCMSConnectionString"
        /// will be used instead of the default "CMSConnectionString". To revert to default DB, use prefix null or empty string.
        /// </summary>
        /// <param name="connectionStringPrefix">Connection string prefix</param>
        public CMSConnectionContext(string connectionStringPrefix)
        {
            // Get original prefix
            OriginalPrefix = ConnectionHelper.ConnectionStringPrefix;

            // Setup the context
            ConnectionHelper.SetConnectionContext(connectionStringPrefix);
        }


        /// <summary>
        /// Disposes the object and resets the context to the previous one.
        /// </summary>
        public void Dispose()
        {
            // Restore the context to the original prefix
            ConnectionHelper.SetConnectionContext(OriginalPrefix);
        }
    }
}
