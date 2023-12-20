using System;

namespace CMS.Modules
{
    /// <summary>
    /// Events for installable module package creation process. Allows for customization of the process.
    /// </summary>
    /// <seealso cref="ModulePackageBuilder"/>
    /// <seealso cref="NuSpecBuilder"/>
    public class ModulePackagingEvents
    {
        private static readonly Lazy<ModulePackagingEvents> mInstance = new Lazy<ModulePackagingEvents>(() => new ModulePackagingEvents());


        /// <summary>
        /// Gets the <see cref="ModulePackagingEvents"/> instance.
        /// </summary>
        public static ModulePackagingEvents Instance
        {
            get
            {
                return mInstance.Value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModulePackagingEvents"/> class.
        /// </summary>
        internal ModulePackagingEvents() { }


        /// <summary>
        /// An event raised upon building NuSpec manifest. The before phase allows for modification of <see cref="BuildNuSpecManifestEventArgs.NuSpecBuilder"/>,
        /// the after phase allows for modification of resulting <see cref="BuildNuSpecManifestEventArgs.Manifest"/>.
        /// </summary>
        /// <seealso cref="NuSpecBuilder.BuildToStream"/>
        public readonly BuildNuSpecManifestHandler BuildNuSpecManifest = new BuildNuSpecManifestHandler { Name = $"{nameof(ModulePackagingEvents)}.{nameof(BuildNuSpecManifest)}" };
    }
}
