using System.Collections.Generic;
using System.Web.Optimization;

namespace Kentico.Builder.Web.Mvc
{
    /// <summary>
    /// Class is responsible for ordering files within a bundle.
    /// </summary>
    internal class BundleOrderer : IBundleOrderer
    {
        /// <summary>
        /// File name of inputmask library file.
        /// </summary>
        public const string INPUTMASK_FILE = "inputmask.js";

        /// <summary>
        /// File name of inputmask library dependency file.
        /// </summary>
        public const string INPUTMASK_DEPENDENCY_FILE = "inputmask.dependencyLib.js";


        /// <summary>
        /// Orders bundle files so they are in the right order.
        /// </summary>
        /// <param name="context">Bundle context.</param>
        /// <param name="files">Input enumeration of unordered files.</param>
        /// <returns>Enumeration of ordered files.</returns>
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            BundleFile inputMaskFile = null;
            BundleFile dependencyInputMaskFile = null;

            foreach (var file in files)
            {
                if (file.VirtualFile.Name == INPUTMASK_FILE)
                {
                    inputMaskFile = file;
                }
                else if (file.VirtualFile.Name == INPUTMASK_DEPENDENCY_FILE)
                {
                    dependencyInputMaskFile = file;
                }
                else
                {
                    yield return file;
                }
            }

            if (dependencyInputMaskFile != null)
            {
                yield return dependencyInputMaskFile;
            }
            if (inputMaskFile != null)
            {
                yield return inputMaskFile;
            }
        }
    }
}
