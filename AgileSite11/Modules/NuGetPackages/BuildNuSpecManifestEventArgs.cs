using CMS.Base;

using NuGet;

namespace CMS.Modules
{
    /// <summary>
    /// Arguments of event represented by <see cref="BuildNuSpecManifestHandler"/>.
    /// </summary>
    public class BuildNuSpecManifestEventArgs : CMSEventArgs
    {
        private readonly string mResourceName;


        /// <summary>
        /// Initializes event arguments for event raised during packaging of a module.
        /// </summary>
        public BuildNuSpecManifestEventArgs() { }


        /// <summary>
        /// Initializes event arguments for event raised during packaging of a module with given <paramref name="resourceName"/>.
        /// </summary>
        public BuildNuSpecManifestEventArgs(string resourceName)
        {
            mResourceName = resourceName;
        }


        /// <summary>
        /// Name of the module being packaged.
        /// </summary>
        /// <seealso cref="ModulePackageBuilder(ResourceInfo)"/>
        /// <seealso cref="ModulePackageBuilder.BuildPackage(string)"/>
        /// <seealso cref="ModulePackageBuilder.BuildPackage(System.IO.Stream)"/>
        public string ResourceName => mResourceName;


        /// <summary>
        /// Builder class for NuSpec manifest. Use the builder to adjust the resulting manifest during the before phase of the event.
        /// </summary>
        public NuSpecBuilder NuSpecBuilder { get; set; }


        /// <summary>
        /// NuSpec manifest created by <see cref="NuSpecBuilder"/>. The manifest is available during the after phase of the event.
        /// </summary>
        public Manifest Manifest { get; set; }
    }
}
