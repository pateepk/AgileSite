using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Class for FormInfo class template generation.
    /// </summary>
    internal partial class ContentItemTemplate
    {
        #region "Variables"
        
        private readonly FormInfo mFormInfo;
        private readonly FormInfo mSkuFormInfo;
        private readonly string mItemClassName;
        private readonly IEnumerable<string> mClassUsings;
        private readonly string mAssemblyRegisterName;
        private readonly Type mBaseClassType;
        private IEnumerable<FormFieldInfo> mFields;
        private IEnumerable<FormFieldInfo> mCustomSKUFields;
        private IEnumerable<FormFieldInfo> mSKUFields;
        private IEnumerable<FormFieldInfo> mSystemSKUFields;

        private readonly Dictionary<FormFieldInfo, string> mPropertyNames;
        private readonly Dictionary<FormFieldInfo, string> mNestedPropertyNames;
        private readonly Dictionary<FormFieldInfo, string> mSKUPropertyNames;
        private readonly Dictionary<FormFieldInfo, string> mSKUNestedPropertyNames;

        #endregion


        #region "Properties"

        /// <summary>
        /// Item type name based on ItemClassName.
        /// </summary>
        private string ItemTypeName
        {
            get;
        }


        /// <summary>
        /// Fields collection.
        /// </summary>
        private IEnumerable<FormFieldInfo> Fields
        {
            get
            {
                if (mFields == null)
                {
                    var fields = mFormInfo.GetFields<FormFieldInfo>()
                                         .Where(x => !x.System);

                    // For custom tables or forms do not create primary key field, as this field is already included in base class.
                    if (mBaseClassType.Name.Equals("CustomTableItem", StringComparison.InvariantCultureIgnoreCase) || mBaseClassType.Name.Equals("BizFormItem", StringComparison.InvariantCultureIgnoreCase))
                    {
                        fields = fields.Where(x => !x.PrimaryKey);
                    }

                    mFields = fields.ToList();
                }

                return mFields;
            }
        }



        /// <summary>
        /// Custom SKU fields collection.
        /// <remarks>Collection is empty if there is no custom field defined in <see cref="mSkuFormInfo"/>.</remarks>
        /// </summary>
        private IEnumerable<FormFieldInfo> CustomSKUFields
        {
            get
            {
                return mCustomSKUFields ?? (mCustomSKUFields = SKUFields.Where(field => !field.System));
            }
        }


        /// <summary>
        /// SKU fields collection.
        /// <remarks>Collection contains custom SKU fields and standard system fields if these are defined in <see cref="mSkuFormInfo"/>.</remarks>
        /// <remarks>Empty collection is returned if <see cref="mSkuFormInfo"/> is <c>null</c>.</remarks>
        /// </summary>
        private IEnumerable<FormFieldInfo> SKUFields
        {
            get
            {
                return mSKUFields ?? (mSKUFields = mSkuFormInfo?.GetFields<FormFieldInfo>().ToList() ?? Enumerable.Empty<FormFieldInfo>());
            }
        }



        /// <summary>
        /// Gets collection of chosen system SKU fields which will be generated in the template.
        /// <remarks>Empty collection is returned if <see cref="mSkuFormInfo"/> is <c>null</c></remarks>
        /// </summary>
        private IEnumerable<FormFieldInfo> SystemSKUFields
        {
            get
            {
                if (mSystemSKUFields == null)
                {
                    var systemSKUFields = new List<string> { "SKUID", "SKUNumber", "SKUWeight", "SKUHeight", "SKUDepth", "SKUWidth" };

                    mSystemSKUFields = SKUFields.Where(field => systemSKUFields.Contains(field.Name));
                }

                return mSystemSKUFields;
            }
        }



        /// <summary>
        /// Indicates whether content type represents product.
        /// </summary>
        private bool IsProduct
        {
            get
            {
                return mBaseClassType.Name.Equals("SKUTreeNode", StringComparison.InvariantCultureIgnoreCase);
            }
        }


        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        private string Namespace
        {
            get;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Initializes a new instance of the FormInfoClassGenerator class.
        /// </summary>
        /// <param name="itemClassName">Base item class name.</param>
        /// <param name="baseClassType">Base class type.</param>
        /// <param name="defaultNamespace">Namespace where class will be generated.</param>
        /// <param name="assemblyRegisterName">Name of register attribute for assembly (f.e. RegisterCustomTable).</param>
        /// <param name="formInfo">FormInfo with fields definition.</param>
        /// <param name="classUsings">List of necessary using namespaces.</param>
        /// <param name="classNameSuffix">Text that is appended to the class name.</param>
        /// <param name="skuFormInfo">FormInfo with SKU fields definition.</param>
        /// <param name="useClassNameNamespace">If true the class namespace will be used as part of the item type defaultNamespace.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any required parameter is empty.</exception>
        private ContentItemTemplate(string itemClassName, Type baseClassType, string defaultNamespace, string assemblyRegisterName, FormInfo formInfo, IEnumerable<string> classUsings = null, string classNameSuffix = null, FormInfo skuFormInfo = null, bool useClassNameNamespace = true)
        {
            mItemClassName = itemClassName;
            ItemTypeName = GetItemTypeName(itemClassName, classNameSuffix);
            Namespace = useClassNameNamespace ? GetFullNamespace(defaultNamespace, itemClassName) : defaultNamespace;
            mFormInfo = formInfo;
            mSkuFormInfo = skuFormInfo;
            mClassUsings = classUsings ?? new string[] { };
            mAssemblyRegisterName = assemblyRegisterName;
            mBaseClassType = baseClassType;

            var reservedFieldNames = new[]
            {
                ItemTypeName,
                ItemTypeName + "Fields", 
                "Fields", 
                "ProductFields", 
                "Product"
            };

            var reservedNestedSKUFields = new[]
            {
                "ProductFields", "Department", "PublicStatus", "Supplier", "Manufacturer", "TaxClass", "Brand", "Collection", "Name", "Description", "ShortDescription"
            };

            // As properties in the generated class are referenced by properties in the nested class, for every field the template needs to know names of both properties.
            // Apart from member names from the base class, there are other names that cannot be used for generated properties, e.g. class name or names of other generated properties.
            mPropertyNames = CreateDictionaryWithPropertyNames(baseClassType, reservedFieldNames, String.Empty);
            mSKUPropertyNames = CreateDictionaryWithSKUPropertyNames(baseClassType, reservedFieldNames, String.Empty);

            mNestedPropertyNames = CreateDictionaryWithPropertyNames(typeof(object), new[] { ItemTypeName + "Fields"}, ItemTypeName);
            mSKUNestedPropertyNames = CreateDictionaryWithSKUPropertyNames(typeof(object), reservedNestedSKUFields, "SKU");
        }

        
        private static string GetItemTypeName(string dataClassName, string classNameSuffix)
        {
            var lastDotIndex = dataClassName.LastIndexOf(".", StringComparison.Ordinal);

            var itemTypeName = lastDotIndex >= 0 ? dataClassName.Substring(lastDotIndex + 1) : dataClassName;
            itemTypeName = ValidationHelper.GetIdentifier(itemTypeName);
            itemTypeName = CapitalizeName(itemTypeName);

            if (!String.IsNullOrEmpty(classNameSuffix))
            {
                itemTypeName += CapitalizeName(classNameSuffix);
            }

            return itemTypeName;
        }
        

        private static string GetFullNamespace(string @namespace, string itemClassName)
        {
            var namespaceSuffix = GetNamespaceFromItemClassName(itemClassName);

            return !string.IsNullOrEmpty(namespaceSuffix) ? $"{@namespace}.{namespaceSuffix}" : @namespace;
        }


        private static string GetNamespaceFromItemClassName(string itemClassName)
        {
            var lastDotIndex = itemClassName.LastIndexOf(".", StringComparison.Ordinal);
            var @namespace = lastDotIndex >= 0 ? itemClassName.Substring(0, lastDotIndex) : string.Empty;

            return CapitalizeName(@namespace);
        }
        

        private static string CapitalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return Char.ToUpperInvariant(name[0]) + name.Substring(1);
        }
        
        #endregion


        #region "Methods"

        private Dictionary<FormFieldInfo, string> CreateDictionaryWithPropertyNames(Type baseType, string[] excludedMemberNames, string fieldNamePrefix)
        {
            var generator = new UniqueMemberNameGenerator(baseType, excludedMemberNames, fieldNamePrefix);

            return Fields.ToDictionary(x => x, x => generator.GetUniqueMemberName(x.Name, true));
        }


        private Dictionary<FormFieldInfo, string> CreateDictionaryWithSKUPropertyNames(Type baseType, string[] excludedMemberNames, string fieldNamePrefix)
        {
            var generator = new UniqueMemberNameGenerator(baseType, excludedMemberNames, fieldNamePrefix);

            return SKUFields.ToDictionary(x => x, x => generator.GetUniqueMemberName(x.Name, !x.Name.Equals("SKUNumber", StringComparison.InvariantCultureIgnoreCase)));
        }


        private string GetPropertyName(FormFieldInfo field)
        {
            return mPropertyNames[field];
        }


        private string GetNestedPropertyName(FormFieldInfo field)
        {
            return mNestedPropertyNames[field];
        }


        private string GetSKUPropertyName(FormFieldInfo field)
        {
            return mSKUPropertyNames[field];
        }


        private string GetSKUNestedPropertyName(FormFieldInfo field)
        {
            return mSKUNestedPropertyNames[field];
        }

        /// <summary>
        /// Returns ValidationHelper Get* method name for specified data type.
        /// </summary>
        private string GetValidationHelperMethodName(string dataType)
        {
            switch (dataType.ToLowerInvariant())
            {
                case FieldDataType.Text:
                case FieldDataType.LongText:
                case FieldDataType.DocAttachments:
                    return "GetString";

                case FieldDataType.Integer:
                    return "GetInteger";

                case FieldDataType.LongInteger:
                    return "GetLong";

                case FieldDataType.Double:
                    return "GetDouble";

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return "GetDateTime";

                case FieldDataType.Boolean:
                    return "GetBoolean";

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return "GetGuid";

                case FieldDataType.Decimal:
                    return "GetDecimal";

                case FieldDataType.TimeSpan:
                    return "GetTimeSpan";

                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), "Specified datatype is not supported.");
            }
        }


        /// <summary>
        /// Returns field description or caption if description is empty.
        /// </summary>
        private string GetSummary(FormFieldInfo fieldInfo, int indentation = 3)
        {
            return CodeGenerator.GetSummary(fieldInfo, indentation);
        }


        /// <summary>
        /// Returns default value for field. If Default value is macro, the default value of data type is used.
        /// </summary>
        private string GetDefaultValue(FormFieldInfo field)
        {
            string dataType = field.DataType;
            bool isMacro;
            string defaultValue = field.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out isMacro);

            bool defaultValueIsEmptyOrMacro = String.IsNullOrEmpty(defaultValue) || isMacro;

            switch (dataType.ToLowerInvariant())
            {
                case FieldDataType.DocAttachments:
                     return $"\"{(defaultValueIsEmptyOrMacro ? string.Empty : defaultValue)}\"";

                case FieldDataType.Text:
                case FieldDataType.LongText:
                    return $"@\"{(defaultValueIsEmptyOrMacro ? string.Empty : defaultValue.Replace("\"", "\"\""))}\"";

                case FieldDataType.Integer:
                case FieldDataType.LongInteger:
                case FieldDataType.Double:
                    return defaultValueIsEmptyOrMacro ? "0" : defaultValue;

                case FieldDataType.Decimal:
                    return defaultValueIsEmptyOrMacro ? "0" : $"ValidationHelper.GetDecimal(\"{defaultValue}\", 0)";

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return defaultValueIsEmptyOrMacro ? "DateTimeHelper.ZERO_TIME" : $"ValidationHelper.GetDateTime(\"{defaultValue}\", DateTimeHelper.ZERO_TIME)";

                case FieldDataType.Boolean:
                    return defaultValueIsEmptyOrMacro ? "false" : defaultValue.ToLowerInvariant();

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return defaultValueIsEmptyOrMacro ? "Guid.Empty" : $"ValidationHelper.GetGuid(\"{defaultValue}\", Guid.Empty)";

                case FieldDataType.TimeSpan:
                    return defaultValueIsEmptyOrMacro ? "TimeSpan.Zero" : $"ValidationHelper.GetTimeSpan(\"{defaultValue}\", TimeSpan.Zero)";

                default:
                    throw new NotSupportedException("Specified datatype is not supported.");
            }
        }


        /// <summary>
        /// Returns proper DataType representation
        /// </summary>
        private string GetDataType(FormFieldInfo field)
        {
            switch (field.DataType.ToLowerInvariant())
            {
                case FieldDataType.Text:
                case FieldDataType.LongText:
                case FieldDataType.DocAttachments:
                    return "string";

                case FieldDataType.Integer:
                    return "int";

                case FieldDataType.LongInteger:
                    return "long";

                case FieldDataType.Double:
                    return "double";

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return "DateTime";

                case FieldDataType.Boolean:
                    return "bool";

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return "Guid";

                case FieldDataType.Decimal:
                    return "decimal";

                case FieldDataType.TimeSpan:
                    return "TimeSpan";

                default:
                    throw new NotSupportedException("Specified datatype is not supported.");
            }
        }

        #endregion


        #region "Static factory methods"

        /// <summary>
        /// Returns generated code for BizFormInfo class.
        /// </summary>
        public static string GetCodeForForm(string className, FormInfo formInfo)
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (className == String.Empty)
            {
                throw new ArgumentException("Value cannot be empty string.", nameof(className));
            }

            if (formInfo == null)
            {
                throw new ArgumentNullException(nameof(formInfo));
            }

            var baseClassType = GetType("CMS.OnlineForms", "CMS.OnlineForms.BizFormItem");

            return new ContentItemTemplate(className, baseClassType, "CMS.OnlineForms.Types", "RegisterBizForm", formInfo, new[] { "CMS.OnlineForms" }, "Item", useClassNameNamespace : false).TransformText();
        }


        /// <summary>
        /// Returns generated code for CustomTableInfo class.
        /// </summary>
        public static string GetCodeForCustomTableItem(string className, FormInfo formInfo)
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (className == String.Empty)
            {
                throw new ArgumentException("Value cannot be empty string.", nameof(className));
            }

            if (formInfo == null)
            {
                throw new ArgumentNullException(nameof(formInfo));
            }

            var baseClassType = GetType("CMS.CustomTables", "CMS.CustomTables.CustomTableItem");

            return new ContentItemTemplate(className, baseClassType, "CMS.CustomTables.Types", "RegisterCustomTable", formInfo, new[] { "CMS.CustomTables" }, "Item").TransformText();
        }


        /// <summary>
        /// Returns generated code for DocumentTypeInfo class.
        /// </summary>
        public static string GetCodeForDocument(string className, FormInfo formInfo)
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (className == String.Empty)
            {
                throw new ArgumentException("Value cannot be empty string.", nameof(className));
            }

            if (formInfo == null)
            {
                throw new ArgumentNullException(nameof(formInfo));
            }

            var baseClassType = GetType("CMS.DocumentEngine", "CMS.DocumentEngine.TreeNode");

            return new ContentItemTemplate(className, baseClassType, "CMS.DocumentEngine.Types", "RegisterDocumentType", formInfo, new[] { "CMS.DocumentEngine" }).TransformText();
        }


        /// <summary>
        /// Returns generated code for DocumentTypeInfo class with SKU related variables.
        /// </summary>
        public static string GetCodeForProduct(string className, FormInfo formInfo, FormInfo skuFormInfo = null)
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (className == String.Empty)
            {
                throw new ArgumentException("Value cannot be empty string.", nameof(className));
            }

            if (formInfo == null)
            {
                throw new ArgumentNullException(nameof(formInfo));
            }

            if (skuFormInfo == null)
            {
                var skuDataClass = DataClassInfoProvider.GetDataClassInfo("ecommerce.sku");
                if (skuDataClass == null)
                {
                    throw new NullReferenceException("Code for product cannot be generated if object with class name 'ecommerce.sku' does not exist. Product data class are missing.");
                }

                skuFormInfo = new FormInfo(skuDataClass.ClassFormDefinition);
            }

            var baseClassType = GetType("CMS.Ecommerce", "CMS.Ecommerce.SKUTreeNode");

            return new ContentItemTemplate(className, baseClassType, "CMS.DocumentEngine.Types", "RegisterDocumentType", formInfo, new[] { "CMS.DocumentEngine", "CMS.Ecommerce" }, skuFormInfo: skuFormInfo).TransformText();
        }


        private static Type GetType(string assemblyName, string fullName)
        {
            // Because of circular references the assembly with this class cannot reference assemblies with base types for generated classes.
            // As the result it is necessary to get base types with knowledge of assembly and type name.
            // If the base type is not available, the code generation will fail. However, all Kentico assemblies should be loaded in every supported scenario.
            // If some of the base types are refactored and are no longer available under original names, there are unit tests that will fail.
            return AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name == assemblyName).GetExportedTypes().Single(x => x != null && x.FullName == fullName);
        }

        #endregion
    }
}