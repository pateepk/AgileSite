using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Load provider event arguments
    /// </summary>
    public class LoadProviderEventArgs : EventArgs
    {
        /// <summary>
        /// Object type for which the provider should be loaded.
        /// </summary>
        public string ObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Loaded provider
        /// </summary>
        public IInfoProvider Provider
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the provider was loaded by the handler.
        /// </summary>
        public bool ProviderLoaded
        {
            get;
            set;
        }
    

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type for which the provider should be loaded</param>
        public LoadProviderEventArgs(string objectType)
        {
            ObjectType = objectType;
        }
    }
}