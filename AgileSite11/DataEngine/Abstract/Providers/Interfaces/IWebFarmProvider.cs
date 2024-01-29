namespace CMS.DataEngine
{
    /// <summary>
    /// Adds web farm task processing support to info provider.
    /// </summary>
    public interface IWebFarmProvider
    {
        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary data</param>
        void ProcessWebFarmTask(string actionName, string data, byte[] binary);


        /// <summary>
        /// Creates web farm task specific for current object and action name
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        void CreateWebFarmTask(string actionName, string data);


        /// <summary>
        /// Creates web farm task specific for current object and action name
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom task data</param>
        /// <param name="binary">Binary value</param>
        /// <param name="filePath">File path</param>
        void CreateWebFarmTask(string actionName, string data, byte[] binary, string filePath);
    }
}