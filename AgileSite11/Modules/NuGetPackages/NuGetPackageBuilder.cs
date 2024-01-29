using System;
using System.Linq;
using System.Text;

using NuGet;

using SystemIO = System.IO;

namespace CMS.Modules
{
    /// <summary>
    /// Allows you to create a NuGet package based on NuSpec manifest.
    /// </summary>
    /// <remarks>
    /// If you want to get familiar with how to use <see cref="NuGet.PackageBuilder"/>, see the NuGet source.
    /// </remarks>
    /// <seealso cref="NuSpecBuilder"/>
    internal class NuGetPackageBuilder
    {
        #region "Fields"
        
        private readonly SystemIO.Stream mNuSpec;
        private readonly string mBasePath;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new NuGet package builder.
        /// </summary>
        /// <param name="nuSpecStream">Stream with NuSpec manifest.</param>
        /// <param name="basePath">Base path used for package creation (see <see cref="NuGet.PackageBuilder"/> constructor, where it is being used).</param>
        public NuGetPackageBuilder(SystemIO.Stream nuSpecStream, string basePath)
        {
            mNuSpec = nuSpecStream;
            mBasePath = basePath;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Builds NuGet package to given stream.
        /// </summary>
        /// <param name="stream">Stream to be used.</param>
        public void BuildToStream(SystemIO.Stream stream)
        {
            PackageBuilder packageBuilder = new PackageBuilder(mNuSpec, mBasePath);
            packageBuilder.Save(stream);
        }

        #endregion
    }
}
