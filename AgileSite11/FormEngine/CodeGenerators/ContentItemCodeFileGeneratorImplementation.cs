using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.IO;

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
    internal sealed class ContentItemCodeFileGeneratorImplementation : IContentItemCodeFileGenerator
    {
        /// <summary>
        /// The code generator.
        /// </summary>
        private readonly IContentItemCodeGenerator mCodeGenerator;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.FormEngine.ContentItemCodeFileGeneratorImplementation"/> class with the specified content item code generator.
        /// </summary>
        /// <param name="codeGenerator">The content item code generator.</param>
        /// <exception cref="System.ArgumentNullException">The code generator is null.</exception>
        public ContentItemCodeFileGeneratorImplementation(IContentItemCodeGenerator codeGenerator)
        {
            if (codeGenerator == null)
            {
                throw new ArgumentNullException(nameof(codeGenerator));
            }

            mCodeGenerator = codeGenerator;
        }

        
        /// <summary>
        /// Creates classes for the specified data classes and saves them to files within the specified folder using naming conventions.
        /// </summary>
        /// <param name="dataClasses">An enumerable collection of data class for which code files should be created.</param>
        /// <param name="baseFolderPath">A path to the folder where code files should be created using naming conventions.</param>
        /// <exception cref="System.ArgumentNullException">A collection of data classes or folder path is null.</exception>
        public void GenerateFiles(IEnumerable<DataClassInfo> dataClasses, string baseFolderPath)
        {
            if (dataClasses == null)
            {
                throw new ArgumentNullException(nameof(dataClasses));
            }

            if (baseFolderPath == null)
            {
                throw new ArgumentNullException(nameof(baseFolderPath));
            }

            DirectoryHelper.EnsureDiskPath(baseFolderPath, null);
            foreach (var dataClass in dataClasses)
            {
                GenerateCodeFiles(dataClass, baseFolderPath);
            }
        }


        /// <summary>
        /// Creates classes for the specified data class and saves them to files within the specified folder using naming conventions.
        /// Code files are created only for supported data classes, i.e. page types, products, custom tables and forms.
        /// </summary>
        /// <param name="dataClass">Data class for which code files should be created.</param>
        /// <param name="baseFolderPath">A path to the folder where code files should be created using naming conventions.</param>
        /// <exception cref="System.ArgumentNullException">Data class or folder path is null.</exception>
        public void GenerateFiles(DataClassInfo dataClass, string baseFolderPath)
        {
            if (dataClass == null)
            {
                throw new ArgumentNullException(nameof(dataClass));
            }

            if (baseFolderPath == null)
            {
                throw new ArgumentNullException(nameof(baseFolderPath));
            }

            DirectoryHelper.EnsureDiskPath(baseFolderPath, null);
            GenerateCodeFiles(dataClass, baseFolderPath);
        }


        private void GenerateCodeFiles(DataClassInfo dataClass, string baseFolderPath)
        {
            if (!mCodeGenerator.CanGenerateItemClass(dataClass) && !mCodeGenerator.CanGenerateItemProviderClass(dataClass))
            {
                return;
            }

            var className = dataClass.ClassName;
            var separatorIndex = className.LastIndexOf('.');
            var namespaceParts = separatorIndex < 0 ? new string[0] : className.Substring(0, separatorIndex).Split('.').Select(CapitalizeName).ToArray();
            var name = CapitalizeName(separatorIndex < 0 ? className : className.Substring(separatorIndex + 1));
            var relativeFolderPath = GetRelativeFolderPath(dataClass, namespaceParts);

            if (mCodeGenerator.CanGenerateItemClass(dataClass))
            {
                var fileName = $"{name}.generated.cs";
                WriteFileContent(baseFolderPath, relativeFolderPath, fileName, mCodeGenerator.GenerateItemClass(dataClass));
            }

            if (mCodeGenerator.CanGenerateItemProviderClass(dataClass))
            {
                var fileName = $"{name}Provider.generated.cs";
                WriteFileContent(baseFolderPath, relativeFolderPath, fileName, mCodeGenerator.GenerateItemProviderClass(dataClass));
            }
        }

        
        /// <summary>
        /// Saves text content to a file using the specified path fragments.
        /// </summary>
        /// <param name="baseFolderPath">A path to the folder where code files should be created using naming conventions.</param>
        /// <param name="relativeFolderPath">A relative path within the base folder.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="content">Content to save.</param>
        private void WriteFileContent(string baseFolderPath, string relativeFolderPath, string fileName, string content)
        {
            var filePath = Path.Combine(baseFolderPath, relativeFolderPath, fileName);
            DirectoryHelper.EnsureDiskPath(filePath, baseFolderPath);
            using (var writer = File.CreateText(filePath))
            {
                writer.Write(content);
            }
        }


        /// <summary>
        /// Capitalizes the specified name and returns it.
        /// </summary>
        /// <param name="name">The name to capitalize.</param>
        /// <returns>Capitalized name.</returns>
        private string CapitalizeName(string name)
        {
            var builder = new StringBuilder(name);
            builder[0] = Char.ToUpperInvariant(builder[0]);

            return builder.ToString();
        }

        
        /// <summary>
        /// Determines relative folder path for code files using naming conventions.
        /// </summary>
        /// <param name="dataClass">Data class for which code files should be created.</param>
        /// <param name="namespaceParts">A collection of namespace parts for the specified data class.</param>
        /// <returns>Relative folder path for code files using naming conventions.</returns>
        private string GetRelativeFolderPath(DataClassInfo dataClass, string[] namespaceParts)
        {
            var fragments = new List<String>();

            if (dataClass.ClassIsProduct)
            {
                fragments.Add("Products");
            }
            else if (dataClass.ClassIsDocumentType)
            {
                fragments.Add("Pages");
            }
            else if (dataClass.ClassIsCustomTable)
            {
                fragments.Add("CustomTableItems");
            }
            else if (dataClass.ClassIsForm)
            {
                fragments.Add("Forms");
            }
            else
            {
                throw new Exception($"Unknown class ({dataClass.ClassName}).");
            }

            if (namespaceParts.Length > 0)
            {
                fragments.AddRange(namespaceParts);
            }

            return Path.Combine(fragments.ToArray());
        }
    }
}
