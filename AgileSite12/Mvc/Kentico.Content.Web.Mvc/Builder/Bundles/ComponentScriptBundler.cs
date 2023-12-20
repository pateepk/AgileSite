using System;
using System.Linq;
using System.IO;
using System.Web.Hosting;
using System.Web.Optimization;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Creates a script bundle for given bundle virtual path and list of source folders.
    /// </summary>
    /// <remarks>
    /// CMS.IO functionality is not supported, implementation uses System.IO deliberately.
    /// </remarks>
     internal sealed class ComponentScriptBundler
    {
        internal Func<string, string> mapPath = HostingEnvironment.MapPath;


        /// <summary>
        /// Registers scripts bundle to <paramref name="bundles"/>.
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <param name="bundlePath">Virtual path of the bundle.</param>
        /// <param name="sourceFolders">List of source folders to include to the bundle.</param>
        /// <remarks>
        /// No bundle is registered if none of the source directories exist or no *.js files are present.
        /// </remarks>
        public void Register(BundleCollection bundles, string bundlePath, string[] sourceFolders)
        {
            var bundle = new ScriptBundle(bundlePath);
            bool includeBundle = false;

            foreach (var sourcePath in sourceFolders)
            {
                var sourceDirectoryPath = mapPath(sourcePath);
#pragma warning disable BH1014 // Do not use System.IO
                if (!Directory.Exists(sourceDirectoryPath) || !Directory.EnumerateFiles(sourceDirectoryPath, "*.js", SearchOption.AllDirectories).Any())
#pragma warning restore BH1014 // Do not use System.IO
                {
                    continue;
                }

                bundle.IncludeDirectory(sourcePath, "*.js", true);
                includeBundle = true;
            }

            bundle.Orderer = new BundleOrderer();

            if (includeBundle)
            {
                bundles.Add(bundle);
            }
        }
    }
}
