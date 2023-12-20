using System.Collections.Generic;
using System.Web.Optimization;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Orderer which does not change the order of included files.
    /// </summary>
    internal class KeepSourceOrderOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}
