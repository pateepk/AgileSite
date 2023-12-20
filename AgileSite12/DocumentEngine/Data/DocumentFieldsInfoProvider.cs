using System;
using System.Data;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine.Internal
{
    using TypeInfoDictionary = SafeDictionary<string, DocumentFieldsTypeInfo>;

    /// <summary>
    /// Class for managing document custom fields.
    /// </summary>
    public class DocumentFieldsInfoProvider : AbstractInfoProvider<DocumentFieldsInfo, DocumentFieldsInfoProvider>
    {
        /// <summary>
        /// Document fields prefix for object type.
        /// </summary>
        internal const string DOCUMENT_FIELDS_PREFIX = "documentfields.";

        private static readonly CMSStatic<TypeInfoDictionary> mTypeInfos = new CMSStatic<TypeInfoDictionary>(() => new TypeInfoDictionary());


        #region "Properties"

        /// <summary>
        /// Indicates if the provider instance is up-to-date and can be used to manage object instances.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                return base.IsValid && ValidateClass(TypeInfo.ObjectClassName);
            }
        }


        /// <summary>
        /// Document fields TypeInfo's [className.ToLowerCSafe()] -> [TypeInfo].
        /// </summary>
        private static TypeInfoDictionary TypeInfos
        {
            get
            {
                return mTypeInfos;
            }
        }


        /// <summary>
        /// Class name.
        /// </summary>
        public string ClassName
        {
            get;
            protected set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Creates an instance of <see cref="DocumentFieldsInfoProvider"/>.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public DocumentFieldsInfoProvider()
        {
            // Do not call base constructor
        }


        /// <summary>
        /// Creates an instance of <see cref="DocumentFieldsInfoProvider"/> based on given class name.
        /// </summary>
        /// <param name="className">Class name of the document type.</param>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public DocumentFieldsInfoProvider(string className)
        {
            TypeInfo = GetTypeInfo(className);
            ClassName = className;
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns document fields with specified ID.
        /// </summary>
        /// <param name="fieldsId">Document fields ID.</param>
        /// <param name="className">Class name.</param>
        public static DocumentFieldsInfo GetDocumentFieldsInfo(int fieldsId, string className)
        {
            var provider = GetProviderObject(className);
            return provider.GetInfoById(fieldsId);
        }


        /// <summary>
        /// Returns document fields object query.
        /// </summary>
        public static ObjectQuery<DocumentFieldsInfo> GetDocumentFields(string className)
        {
            var provider = GetProviderObject(className);
            return provider.GetObjectQuery();
        }


        /// <summary>
        /// Deletes specified document fields.
        /// </summary>
        /// <param name="fields">Document fields to be deleted.</param>
        public static void DeleteDocumentFieldsInfo(DocumentFieldsInfo fields)
        {
            var provider = GetProviderObject(fields.ClassName);
            provider.DeleteInfo(fields);
        }


        /// <summary>
        /// Deletes document fields data with specified ID.
        /// </summary>
        /// <param name="fieldsId">Document fields data ID.</param>
        /// <param name="className">Class name</param>
        public static void DeleteDocumentFieldsInfo(int fieldsId, string className)
        {
            DocumentFieldsInfo fields = GetDocumentFieldsInfo(fieldsId, className);
            DeleteDocumentFieldsInfo(fields);
        }


        /// <summary>
        /// Sets (updates or inserts) specified document fields.
        /// </summary>
        /// <param name="fields">Document fields to be set.</param>
        public static void SetDocumentFieldsInfo(DocumentFieldsInfo fields)
        {
            var provider = GetProviderObject(fields.ClassName);
            provider.SetInfo(fields);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Gets the object query for the provider.
        /// </summary>
        protected override ObjectQuery<DocumentFieldsInfo> GetObjectQueryInternal()
        {
            var query = base.GetObjectQueryInternal();
            query.ObjectType = TypeInfo.ObjectType;
            return query;
        }

        #endregion


        #region "Object type methods"

        /// <summary>
        /// Gets document type class name from given object type.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        internal static string GetClassName(string objectType)
        {
            return objectType.ToLowerCSafe().Replace(DOCUMENT_FIELDS_PREFIX, "");
        }


        /// <summary>
        /// Gets document type object type from given class name.
        /// </summary>
        /// <param name="className">Class name.</param>
        internal static string GetObjectType(string className)
        {
            return DOCUMENT_FIELDS_PREFIX + className.ToLowerCSafe();
        }


        /// <summary>
        /// Indicates if given object type represents document type.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        internal static bool IsDocumentFieldsObjectType(string objectType)
        {
            return objectType.StartsWithCSafe(DOCUMENT_FIELDS_PREFIX, true);
        }

        #endregion


        #region "Provider management methods"

        /// <summary>
        /// Gets provider object.
        /// </summary>
        private static DocumentFieldsInfoProvider GetProviderObject(string className)
        {
            return InfoProviderLoader.GetInfoProvider<DocumentFieldsInfoProvider>(GetObjectType(className));
        }


        /// <summary>
        /// Loads document fields provider for given object type.
        /// </summary>
        /// <param name="objectType">Document fields object type.</param>
        internal static IInfoProvider LoadProviderInternal(string objectType)
        {
            var provider = CMSExtensibilitySection.LoadProvider<DocumentFieldsInfoProvider>();

            // Initialize provider
            provider.ClassName = GetClassName(objectType);
            provider.TypeInfo = GetTypeInfo(provider.ClassName);
            provider.InfoObject = DocumentFieldsInfo.New(provider.ClassName);

            provider.InitHashtableSettings(provider.TypeInfo);

            return provider;
        }


        /// <summary>
        /// Invalidates specific provider.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        internal static void InvalidateProvider(string objectType)
        {
            ProviderHelper.InvalidateProvider(objectType);
        }


        /// <summary>
        /// Gets new document fields instance.
        /// </summary>
        /// <param name="dr">Data row with the object data.</param>
        /// <param name="useGenerator">If true, the process allows using the generator to differentiate between particular info types based on data.</param>
        protected override DocumentFieldsInfo CreateInfo(DataRow dr = null, bool useGenerator = true)
        {
            string className = TypeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN ? TypeInfo.ObjectClassName : ClassName;

            return new DocumentFieldsInfo(className, dr);
        }

        #endregion


        #region "Cached TypeInfos methods"

        /// <summary>
        /// Returns the TypeInfo for specified class.
        /// </summary>
        /// <param name="className">Class name.</param>
        internal static ObjectTypeInfo GetTypeInfo(string className)
        {
            // Try to get from hashtable
            className = className.ToLowerCSafe();

            DocumentFieldsTypeInfo existingTypeInfo;
            var exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
            if (!exists || !existingTypeInfo.IsValid)
            {
                lock (TypeInfos.SyncRoot)
                {
                    exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
                    if (!exists || !existingTypeInfo.IsValid)
                    {
                        var newInfo = CreateTypeInfo(className);

                        // Copy events from existing one
                        if (existingTypeInfo != null)
                        {
                            existingTypeInfo.CopyDocumentFieldsTypeInfoEventsTo(newInfo);
                        }

                        TypeInfos[className] = newInfo;
                        return newInfo;
                    }
                }
            }

            return existingTypeInfo;
        }


        /// <summary>
        /// Clear the class info's and properties lists of all object types.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged.</param>
        internal static void Clear(bool logTask)
        {
            TypeInfos.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearDocumentFieldsTypeInfosWebFarmTask ());
            }
        }


        /// <summary>
        /// Invalidates typeinfo specified by class name.
        /// </summary>
        internal static void InvalidateTypeInfo(string className, bool logTask)
        {
            className = className.ToLowerCSafe();
            lock (TypeInfos.SyncRoot)
            {
                var oldInfo = TypeInfos[className];

                if (oldInfo != null)
                {
                    // Invalidate type info
                    oldInfo.IsValid = false;
                }
            }

            if (logTask)
            {
                WebFarmHelper.CreateTask(new InvalidateDocumentFieldsTypeInfoWebFarmTask { ClassName = className });
            }
        }


        /// <summary>
        /// Validates given class name if represents existing document type.
        /// </summary>
        /// <param name="className">Class name to validate.</param>
        private static bool ValidateClass(string className)
        {
            var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(className);
            return (dataClassInfo != null) && dataClassInfo.ClassIsDocumentType;
        }


        /// <summary>
        /// Creates new typeinfo.
        /// </summary>
        private static DocumentFieldsTypeInfo CreateTypeInfo(string className)
        {
            // Create new class info
            var dci = DataClassFactory.NewDataClass(className);
            if (dci == null)
            {
                throw new DocumentTypeNotExistsException("Page type with '" + className + "' class name not found.");
            }

            if (!ValidateClass(className))
            {
                throw new DocumentTypeNotExistsException("The '" + className + "' class name is not a page type.");
            }

            string objectType = GetObjectType(className);

            // Create the type info
            var result = new DocumentFieldsTypeInfo(typeof(DocumentFieldsInfoProvider), objectType, className, dci.IDColumn, null, null, null, null, null, null, null, null)
            {
                CompositeObjectType = TreeNode.OBJECT_TYPE,
                SynchronizationSettings =
                {
                    IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                    LogSynchronization = SynchronizationTypeEnum.None
                },
                LogIntegration = false,
                LogEvents = false,
                TouchCacheDependencies = false,
                SupportsVersioning = false,
                AllowRestore = false,
                ImportExportSettings =
                {
                    IncludeToExportParentDataSet = IncludeToParentEnum.None
                }
            };

            // Initialize new type info object
            ObjectTypeManager.EnsureObjectTypeInfoDynamicList(result);
            return result;
        }

        #endregion
    }
}