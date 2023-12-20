using System;
using System.Linq;
using System.Text;

namespace CMS.Modules.NuGetPackages
{
    /// <summary>
    /// Provides module's metadata.
    /// </summary>
    public class ModulePackageMetadata
    {
        /// <summary>
        /// Gets or sets identifier of the package.
        /// </summary>
        public string Id
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets title of the package.
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets version of the package. Format like "1.1.1"
        /// </summary>
        public string Version
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets authors of the package, comma-separated.
        /// </summary>
        public string Authors
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets description of the package.
        /// </summary>
        public string Description
        {
            get;
            set;
        }
    }
}
