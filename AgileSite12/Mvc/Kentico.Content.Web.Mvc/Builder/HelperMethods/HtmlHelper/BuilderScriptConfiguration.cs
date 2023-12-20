using System.Collections.Generic;

namespace Kentico.Builder.Web.Mvc.Internal
{
    /// <summary>
    /// Encapsulates data sent to client and used by the builder's script.
    /// </summary>
    public class BuilderScriptConfiguration
    {
        /// <summary>
        /// Virtual path of the directory that contains the application hosted in the current application domain.
        /// </summary>
        public string ApplicationPath
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for storing the builder configuration.
        /// </summary>
        public string ConfigurationStoreEndpoint
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for retrieving the builder configuration.
        /// </summary>
        public string ConfigurationLoadEndpoint
        {
            get;
            set;
        }


        /// <summary>
        /// Endpoint for retrieving the builder metadata.
        /// </summary>
        public string MetadataEndpoint
        {
            get;
            set;
        }


        /// <summary>
        /// List of domains that are passed to the client to allow post messages to be retrieved only from these origins.
        /// </summary>
        public IEnumerable<string> AllowedOrigins
        {
            get;
            set;
        }


        /// <summary>
        /// Enables development mode for builder's store.
        /// </summary>
        public bool DevelopmentMode
        {
            get;
            set;
        }
    }
}
