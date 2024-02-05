using System;

using CMS.DataEngine;

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
    internal sealed class ContentItemCodeGeneratorImplementation : IContentItemCodeGenerator
    {
        /// <summary>
        /// Gets a value indicating whether this instance can generate content item class for the specified data class.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>True, if this instance can generate content item class for the specified data class; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        public bool CanGenerateItemClass(DataClassInfo dataClass)
        {
            if (dataClass == null)
            {
                throw new ArgumentNullException(nameof(dataClass));
            }

            return (dataClass.ClassIsProduct || dataClass.ClassIsDocumentType || dataClass.ClassIsCustomTable || dataClass.ClassIsForm);
        }


        /// <summary>
        /// Gets a value indicating whether this instance can generate content item provider class for the specified data class.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>True, if this instance can generate content item provider class for the specified data class; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        public bool CanGenerateItemProviderClass(DataClassInfo dataClass)
        {
            if (dataClass == null)
            {
                throw new ArgumentNullException(nameof(dataClass));
            }

            return (dataClass.ClassIsDocumentType);
        }

        
        /// <summary>
        /// Generates content item class for the specified data class and returns it.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>Content item class for the specified data class.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        /// <exception cref="System.ArgumentException">The specified data class is not supported.</exception>
        public string GenerateItemClass(DataClassInfo dataClass)
        {
            if (dataClass == null)
            {
                throw new ArgumentNullException(nameof(dataClass));
            }

            var formDefinition = dataClass.ClassFormDefinition;
            var form = new FormInfo(formDefinition);

            // Products are page types so we look for them first
            if (dataClass.ClassIsProduct)
            {
                return ContentItemTemplate.GetCodeForProduct(dataClass.ClassName, form);
            }
            if (dataClass.ClassIsDocumentType)
            {
                return ContentItemTemplate.GetCodeForDocument(dataClass.ClassName, form);
            }
            if (dataClass.ClassIsCustomTable)
            {
                return ContentItemTemplate.GetCodeForCustomTableItem(dataClass.ClassName, form);
            }
            if (dataClass.ClassIsForm)
            {
                return ContentItemTemplate.GetCodeForForm(dataClass.ClassName, form);
            }

            throw new ArgumentException($"Unknown class ({dataClass.ClassName}).");
        }


        /// <summary>
        /// Generates content item provider class for the specified data class and returns it.
        /// </summary>
        /// <param name="dataClass">The data class for which code should be generated.</param>
        /// <returns>Content item provider class for the specified data class.</returns>
        /// <exception cref="System.ArgumentNullException">Data class is null.</exception>
        /// <exception cref="System.ArgumentException">The specified data class is not supported.</exception>
        public string GenerateItemProviderClass(DataClassInfo dataClass)
        {
            if (dataClass == null)
            {
                throw new ArgumentNullException(nameof(dataClass));
            }

            if (dataClass.ClassIsDocumentType)
            {
                return ContentItemProviderTemplate.GetCodeForDocumentProvider(dataClass.ClassName);
            }

            throw new ArgumentException($"Unknown class ({dataClass.ClassName}).");
        }
    }
}