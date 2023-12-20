using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.FormEngine
{
    /// <summary>
    /// Creates classes for content items, such as pages or custom table items, and saves them to files using naming conventions.
    /// Generated classes contain properties that correspond to associated data class fields and bring the benefits of strongly typed code such as IntelliSense support.
    /// Only page types, products, custom tables and forms are supported.
    /// Files are organized in hiearchical folder structure with one top-level folder for each supported content type (Pages, Products, CustomTableItems and Forms).
    /// Within each top-level folder there are another folders corresponding to the individual namespaces of the respective data class.
    /// </summary>
    public interface IContentItemCodeFileGenerator
    {
        /// <summary>
        /// Creates classes for the specified data classes and saves them to files within the specified folder using naming conventions.
        /// </summary>
        /// <param name="dataClass">An enumerable collection of data class for which code files should be created.</param>
        /// <param name="baseFolderPath">A path to the folder where code files should be created using naming conventions.</param>
        /// <exception cref="System.ArgumentNullException">A collection of data classes or folder path is null.</exception>
        void GenerateFiles(DataClassInfo dataClass, string baseFolderPath);

        
        /// <summary>
        /// Creates classes for the specified data class and saves them to files within the specified folder using naming conventions.
        /// Code files are created only for supported data classes, i.e. page types, products, custom tables and forms.
        /// </summary>
        /// <param name="dataClasses">Data class for which code files should be created.</param>
        /// <param name="baseFolderPath">A path to the folder where code files should be created using naming conventions.</param>
        /// <exception cref="System.ArgumentNullException">Data class or folder path is null.</exception>
        void GenerateFiles(IEnumerable<DataClassInfo> dataClasses, string baseFolderPath);
    }
}