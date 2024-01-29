using System;
using System.Data.Entity.Design.PluralizationServices;

using CMS.Helpers;


namespace CMS.FormEngine
{
    /// <summary>
    /// This class provides parameters for related T4 template.
    /// </summary>
    internal partial class ContentItemProviderTemplate
    {
        private string mItemNamePlural;


        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        private string Namespace
        {
            get;
            set;
        }


        /// <summary>
        /// Class identifier based on data class name.
        /// </summary>
        private string ProviderClassName
        {
            get
            {
                return ItemTypeName + "Provider";
            }
        }


        /// <summary>
        /// Gets item type name, it's derived from data class name.
        /// </summary>
        private string ItemTypeName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets item name, i.e. name of the generated provider class.
        /// </summary>
        private string ItemName
        {
            get
            {
                return ItemTypeName;
            }
        }


        /// <summary>
        /// Gets item type name in plural.
        /// </summary>
        private string ItemNamePlural
        {
            get
            {
                if (mItemNamePlural == null)
                {
                    var service = PluralizationService.CreateService(CultureHelper.EnglishCulture);
                    mItemNamePlural = service.Pluralize(ItemName);
                }

                return mItemNamePlural;
            }
        }


        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="itemClassName">Item class name.</param>
        /// <param name="defaultNamespace">Default defaultNamespace for the generated class.</param>
        private ContentItemProviderTemplate(string itemClassName, string defaultNamespace)
        {
            Namespace = GetFullNamespace(defaultNamespace, itemClassName);
            ItemTypeName = GetItemType(itemClassName);
        }


        /// <summary>
        /// Compiles the template and return the code of generated provider.
        /// </summary>
        /// <param name="itemDataClass">Item class name.</param>
        public static string GetCodeForDocumentProvider(string itemDataClass)
        {
            if (String.IsNullOrEmpty(itemDataClass))
            {
                throw new ArgumentNullException(nameof(itemDataClass));
            }

            return new ContentItemProviderTemplate(itemDataClass, "CMS.DocumentEngine.Types").TransformText();
        }


        private static string GetFullNamespace(string defaultNamespace, string itemClassName)
        {
            var namespaceSuffix = GetNamespaceFromItemClassName(itemClassName);

            return $"{defaultNamespace}.{namespaceSuffix}";
        }


        private static string GetNamespaceFromItemClassName(string itemClassName)
        {
            var lastDotIndex = itemClassName.LastIndexOf(".", StringComparison.Ordinal);
            var @namespace = lastDotIndex >= 0 ? itemClassName.Substring(0, lastDotIndex) : string.Empty;

            return CapitalizeName(@namespace);
        }


        private static string GetItemType(string itemClassName)
        {
            var lastDotIndex = itemClassName.LastIndexOf(".", StringComparison.Ordinal);
            var itemType = lastDotIndex >= 0 ? itemClassName.Substring(lastDotIndex + 1) : itemClassName;
            itemType = ValidationHelper.GetIdentifier(itemType);

            return CapitalizeName(itemType);
        }


        private static string CapitalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return Char.ToUpperInvariant(name[0]) + name.Substring(1);
        }
    }
}
