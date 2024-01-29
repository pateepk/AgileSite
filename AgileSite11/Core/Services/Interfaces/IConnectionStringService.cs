namespace CMS.Core
{
    /// <summary>
    /// ConnectionString service interface
    /// </summary>
    public interface IConnectionStringService
    {
        /// <summary>
        /// Default connection string
        /// </summary>
        string DefaultConnectionString
        {
            get;
        }


        /// <summary>
        /// Gets the specific connection string from the app config
        /// </summary>
        string this[string name]
        {
            get;
        }
    }
}
