using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Container for configuration data required for database installation.
    /// </summary>
    public class DatabaseInstallationSettings
    {
        /// <summary>
        /// Gets or sets the connection string to the database.
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the folder with the database scripts.
        /// </summary>
        public string ScriptsFolder
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the error message used when creation of DB objects fails.
        /// </summary>
        public string DatabaseObjectInstallationErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the error message used when data creation fails.
        /// </summary>
        public string DataInstallationErrorMessage
        {
            get;
            set;
        }



        /// <summary>
        /// Gets or sets the logger used during database installation.
        /// </summary>
        public Action<string, MessageTypeEnum> Logger
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the information determining hotfix application during database installation.
        /// </summary>
        public bool ApplyHotfix
        {
            get;
            set;
        }
    }
}
