using System;

namespace CMS.Core
{
    /// <summary>
    /// Represents application startup parameters.
    /// </summary>
    public sealed class AppCoreSetup
    {
        /// <summary>
        /// Gets the path of the directory containing private assemblies.
        /// </summary>
        public string DependenciesFolderPath
        {
            get;
            private set;
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="AppCoreSetup"/> class.
        /// </summary>
        private AppCoreSetup()
        {

        }

        
        /// <summary>
        /// Builds instances of the <see cref="AppCoreSetup"/> class.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// The path of the directory containing private assemblies.
            /// </summary>
            private string mDependenciesFolderPath;

            
            /// <summary>
            /// Configures this builder with the path of the directory containing private assemblies.
            /// </summary>
            /// <param name="dependenciesFolderPath">The path of the directory containing private assemblies.</param>
            /// <returns>A reference to this builder after the path of the directory containing private assemblies has been modified.</returns>
            public Builder WithDependenciesFolderPath(string dependenciesFolderPath)
            {
                mDependenciesFolderPath = dependenciesFolderPath;

                return this;
            }

            
            /// <summary>
            /// Creates a new instance of the <see cref="AppCoreSetup"/> class and returns it.
            /// </summary>
            /// <returns>A new instance of the <see cref="AppCoreSetup"/> class.</returns>
            public AppCoreSetup Build()
            {
                if (string.IsNullOrEmpty(mDependenciesFolderPath))
                {
                    throw new Exception("The path of the directory containing private assemblies is not specified.");
                }

                return new AppCoreSetup
                {
                    DependenciesFolderPath = mDependenciesFolderPath
                };
            }
        }
    }
}