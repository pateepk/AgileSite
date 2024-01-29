using System;
using System.Threading;

namespace CMS.FormEngine
{
    /// <summary>
    /// Provides classes for content items, such as pages or custom table items, to facilitate their usage in code.
    /// Generated classes contain properties that correspond to associated data class fields and bring the benefits of strongly typed code such as IntelliSense support.
    /// Only page types, products, custom tables and forms are supported.
    /// </summary>
    /// <remarks>
    /// This class is thread safe.
    /// </remarks>
    public static class ContentItemCodeGenerator
    {
        /// <summary>
        /// The current instance of the code generator.
        /// </summary>
        private static readonly Lazy<IContentItemCodeGenerator> mInstance = new Lazy<IContentItemCodeGenerator>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        
        /// <summary>
        /// Gets the current instance of the code generator.
        /// This is an internal feature that might be removed in the next major version.
        /// </summary>
        public static IContentItemCodeGenerator Internal
        {
            get
            {
                return mInstance.Value;
            }
        }


        /// <summary>
        /// Creates a new instance of the code generator, and returns it.
        /// </summary>
        /// <returns>A new instance of the code generator.</returns>
        private static IContentItemCodeGenerator CreateInstance()
        {
            return new ContentItemCodeGeneratorImplementation();
        }
    }
}