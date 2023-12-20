using System;
using System.Linq;
using System.IO;
using System.Web.Hosting;
using System.Web.Optimization;

using Kentico.PageBuilder.Web.Mvc;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Creates a style bundle for given bundle virtual path and list of source folders.
    /// </summary>
    /// <remarks>
    /// CMS.IO functionality is not supported, implementation uses System.IO deliberately.
    /// </remarks>
    internal sealed class ComponentStyleBundler
    {
        internal Func<string, string> mapPath = HostingEnvironment.MapPath;


        /// <summary>
        /// Registers styles bundle to <paramref name="bundles"/>.
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <param name="bundlePath">Virtual path of the bundle.</param>
        /// <param name="styleFolders">List of source folders to include to the bundle.</param>
        /// <remarks>
        /// No bundle is registered if none of the source directories exist or no *.css files are present.
        /// </remarks>
        public void Register(BundleCollection bundles, string bundlePath, string[] styleFolders)
        {
            var includeBundle = false;
            var styleBundle = new StyleBundle(bundlePath)
            {
                Orderer = new KeepSourceOrderOrderer()
            };

            foreach (var sourceVirtualPath in styleFolders)
            {
                var sourceDirectoryPath = mapPath(sourceVirtualPath);
#pragma warning disable BH1014 // Do not use System.IO
                if (!Directory.Exists(sourceDirectoryPath) || !Directory.EnumerateFiles(sourceDirectoryPath, "*.css", SearchOption.AllDirectories).Any())
#pragma warning restore BH1014 // Do not use System.IO
                {
                    continue;
                }

                styleBundle.IncludeDirectory(sourceVirtualPath, "*.css", true);
                includeBundle = true;
            }

            if (includeBundle)
            {
                bundles.Add(styleBundle);
            }
        }
    }
}
