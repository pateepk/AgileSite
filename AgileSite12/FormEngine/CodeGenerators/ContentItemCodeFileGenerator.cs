using System;
using System.Threading;

namespace CMS.FormEngine
{
    /// <summary>
    /// Creates classes for content items, such as pages or custom table items, and saves them to files using naming conventions.
    /// Generated classes contain properties that correspond to associated data class fields and bring the benefits of strongly typed code such as IntelliSense support.
    /// Only page types, products, custom tables and forms are supported.
    /// Files are organized in hiearchical folder structure with one top-level folder for each supported content type (Pages, Products, CustomTableItems and Forms).
    /// Within each top-level folder there are another folders corresponding to the individual namespaces of the respective data class.
    /// </summary>
    /// <remarks>
    /// This class is thread safe.
    /// </remarks>
    public static class ContentItemCodeFileGenerator
    {
        /// <summary>
        /// The current instance of the code file generator.
        /// </summary>
        private static readonly Lazy<IContentItemCodeFileGenerator> mInstance = new Lazy<IContentItemCodeFileGenerator>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets the current instance of the code file generator.
        /// This is an internal feature that might be removed in the next major version.
        /// </summary>
        public static IContentItemCodeFileGenerator Internal
        {
            get
            {
                return mInstance.Value;
            }
        }


        /// <summary>
        /// Creates a new instance of the code file generator, and returns it.
        /// </summary>
        /// <returns>A new instance of the code file generator.</returns>
        private static IContentItemCodeFileGenerator CreateInstance()
        {
            return new ContentItemCodeFileGeneratorImplementation(ContentItemCodeGenerator.Internal);
        }
    }
}
