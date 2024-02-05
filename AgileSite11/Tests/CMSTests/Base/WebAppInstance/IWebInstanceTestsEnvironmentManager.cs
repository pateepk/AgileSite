namespace CMS.Tests
{
    /// <summary>
    /// Interface for managing test environment for <see cref="AbstractWebAppInstanceTests"/>.
    /// </summary>
    public interface IWebInstanceTestsEnvironmentManager
    {
        /// <summary>
        /// Name of test web app instance.
        /// </summary>
        string WebAppInstanceName
        {
            get;
        }


        /// <summary>
        /// URL of test web app instance.
        /// </summary>
        string WebAppInstanceUrl
        {
            get;
        }


        /// <summary>
        /// Physical path of test web app instance.
        /// </summary>
        string WebAppInstancePath
        {
            get;
        }


        /// <summary>
        /// Sets up test web app instance. 
        /// </summary>
        void SetUp();


        /// <summary>
        /// Cleans up test web app instance.
        /// </summary>
        void CleanUp();


        /// <summary>
        /// Ensures IIS Express process is running.
        /// </summary>
        void EnsureIISProcess();
    }
}
