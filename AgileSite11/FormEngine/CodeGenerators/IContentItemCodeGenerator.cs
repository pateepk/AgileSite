using CMS.DataEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Provides classes for content items, such as pages or custom table items, to facilitate their usage in code.
    /// Generated classes contain properties that correspond to associated data class fields and bring the benefits of strongly typed code such as IntelliSense support.
    /// Only page types, products, custom tables and forms are supported.
    /// </summary>
    public interface IContentItemCodeGenerator
    {
        /// <summary>
        /// Gets a value indicating whether this instance can generate content item class for the specified data class.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>True, if this instance can generate content item class for the specified data class; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        bool CanGenerateItemClass(DataClassInfo dataClass);


        /// <summary>
        /// Gets a value indicating whether this instance can generate content item provider class for the specified data class.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>True, if this instance can generate content item provider class for the specified data class; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        bool CanGenerateItemProviderClass(DataClassInfo dataClass);
        

        /// <summary>
        /// Generates content item class for the specified data class and returns it.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>Content item class for the specified data class.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        /// <exception cref="System.ArgumentException">The specified data class is not supported.</exception>
        string GenerateItemClass(DataClassInfo dataClass);
        

        /// <summary>
        /// Generates content item provider class for the specified data class and returns it.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>Content item provider class for the specified data class.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        /// <exception cref="System.ArgumentException">The specified data class is not supported.</exception>
        string GenerateItemProviderClass(DataClassInfo dataClass);
    }
}
