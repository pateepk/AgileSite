using System;
using System.Data;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    using TypeInfoDictionary = SafeDictionary<string, DynamicTreeNodeTypeInfo>;

    /// <summary>
    /// Class providing documents management.
    /// </summary>
    internal class TreeNodeProvider : AbstractInfoProvider<TreeNode, TreeNodeProvider, DocumentQuery>
    {
        #region "Variables"

        private static readonly CMSStatic<TypeInfoDictionary> mTypeInfos = new CMSStatic<TypeInfoDictionary>(() => new TypeInfoDictionary());

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if the provider instance is up-to-date and can be used to manage object instances.
        /// </summary>
        public override bool IsValid => base.IsValid && ValidateClass(TypeInfo.ObjectClassName);


        /// <summary>
        /// Document TypeInfo [className.ToLowerCSafe()] -> [TypeInfo].
        /// </summary>
        private static TypeInfoDictionary TypeInfos => mTypeInfos;


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
        /// Creates an instance of <see cref="TreeNodeProvider"/>.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly.")]
        public TreeNodeProvider()
            : base(false)
        {
            // Do not initialize base provider, it will be initialized in LoadProviderInternal
        }


        /// <summary>
        /// Creates an instance of <see cref="TreeNodeProvider"/> based on given class name.
        /// </summary>
        /// <param name="className">Class name.</param>
        public TreeNodeProvider(string className)
            : base(true)
        {
            TypeInfo = GetTypeInfo(className);
            ClassName = className;
        }

        #endregion


        #region "Standard provider methods"

        /// <summary>
        /// Gets the document.
        /// </summary>
        public static MultiDocumentQuery GetDocument()
        {
            return GetDocuments().TopN(1);
        }


        /// <summary>
        /// Sets specified document.
        /// </summary>
        /// <param name="document">Document to set.</param>
        public static void SetDocument(TreeNode document)
        {
            var type = document?.ClassName;
            var provider = GetProviderObject(type);
            provider.SetDocumentInternal(document);
        }


        /// <summary>
        /// Deletes specified document.
        /// </summary>
        /// <param name="document">Document to delete.</param>
        public static void DeleteDocument(TreeNode document)
        {
            var type = document?.ClassName;
            var provider = GetProviderObject(type);
            provider.DeleteDocumentInternal(document);
        }


        /// <summary>
        /// Gets the query for all documents.
        /// </summary>
        /// <param name="className">Class name representing document type.</param>
        public static DocumentQuery GetDocuments(string className)
        {
            var provider = GetProviderObject(className);
            var query = provider.GetObjectQuery();

            ConfigureQueryProperties(query);

            return query;
        }


        /// <summary>
        /// Gets the query for all documents.
        /// </summary>
        public static DocumentQuery<TDocument> GetDocuments<TDocument>()
            where TDocument : TreeNode, new()
        {
            var provider = GetProviderObject(null);
            var query = provider.GetObjectQuery();

            // Create generic query with correct type and copy properties from the provider query
            var genericQuery = new DocumentQuery<TDocument>();
            query.CopyPropertiesTo(genericQuery);

            // Re-initialize query based on generic type
            genericQuery.InitFromType();

            ConfigureQueryProperties(genericQuery);

            return genericQuery;
        }


        /// <summary>
        /// Gets the query for all documents.
        /// </summary>
        public static MultiDocumentQuery GetDocuments()
        {
            var provider = GetProviderObject(null);
            var query = provider.GetDocumentQuery();

            ConfigureQueryProperties(query);


            return query;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets object query.
        /// </summary>
        protected override DocumentQuery GetObjectQueryInternal()
        {
            return GetQueryInternal(ClassName);
        }


        /// <summary>
        /// Gets document query for system purposes without any additional parametrization.
        /// </summary>
        /// <param name="className">Class name.</param>
        internal static DocumentQuery GetQueryInternal(string className)
        {
            var query = new DocumentQuery(className);

            // Do not ensure extra system columns for low-level API calls, since it can lead to syntactically incorrect queries
            query.Properties.EnsureExtraColumns = false;

            return query;
        }


        /// <summary>
        /// Gets the object query for the provider.
        /// </summary>
        protected MultiDocumentQuery GetDocumentQuery()
        {
            var query = new MultiDocumentQuery();

            // Do not ensure extra system columns for low-level API calls, since it can lead to syntactically incorrect queries
            query.Properties.EnsureExtraColumns = false;

            return query;
        }


        /// <summary>
        /// Sets specified document.
        /// </summary>
        /// <param name="document">Document to set.</param>
        protected virtual void SetDocumentInternal(TreeNode document)
        {
            SetInfo(document);
        }


        /// <summary>
        /// Deletes specified document.
        /// </summary>
        /// <param name="document">Document to delete.</param>
        protected virtual void DeleteDocumentInternal(TreeNode document)
        {
            DeleteInfo(document);
        }


        private static void ConfigureQueryProperties(IDocumentQuery query)
        {
            // Reflect site settings as default parametrization and ensure system columns
            query.Properties.ApplyDefaultSettings = true;
            query.Properties.EnsureExtraColumns = true;
        }

        #endregion


        #region "Object type methods"


        /// <summary>
        /// Gets document class name from given object type.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        public static string GetClassName(string objectType)
        {
            return objectType == TreeNode.OBJECT_TYPE ? null : objectType?.ToLowerInvariant().Replace(DocumentHelper.DOCUMENT_PREFIX, "");
        }


        /// <summary>
        /// Gets document object type from given class name.
        /// </summary>
        /// <param name="className">Class name.</param>
        public static string GetObjectType(string className)
        {
            var isGeneralClassName = String.IsNullOrEmpty(className) || IsGeneralDocumentClass(className);
            return isGeneralClassName ? TreeNode.OBJECT_TYPE : DocumentHelper.DOCUMENT_PREFIX + className.ToLowerInvariant();
        }


        /// <summary>
        /// Indicates if given object type represents document.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        public static bool IsDocumentObjectType(string objectType)
        {
            return objectType.StartsWith(DocumentHelper.DOCUMENT_PREFIX, StringComparison.InvariantCultureIgnoreCase) || IsGeneralDocumentClass(objectType);
        }

        #endregion


        #region "Provider management methods"

        /// <summary>
        /// Gets provider object.
        /// </summary>
        private static TreeNodeProvider GetProviderObject(string className)
        {
            return InfoProviderLoader.GetInfoProvider<TreeNodeProvider>(GetObjectType(className));
        }


        /// <summary>
        /// Loads document info provider for given object type.
        /// </summary>
        /// <param name="objectType">Document object type.</param>
        internal static IInfoProvider LoadProviderInternal(string objectType)
        {
            var provider = CMSExtensibilitySection.LoadProvider<TreeNodeProvider>();

            // Initialize provider
            var className = GetClassName(objectType);

            provider.ClassName = className;
            provider.InfoObject = TreeNode.New(className);
            provider.Init();

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
        /// Creates new document info instance.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        /// <param name="useGenerator">If true, the process allows using the generator to differentiate between particular info types based on data.</param>
        protected override TreeNode CreateInfo(DataRow dr = null, bool useGenerator = true)
        {
            string className = TypeInfo.ObjectClassName != ObjectTypeInfo.VALUE_UNKNOWN ? TypeInfo.ObjectClassName : null;

            return TreeNode.New(className, dr);
        }

        #endregion


        #region "Cached TypeInfos methods"

        /// <summary>
        /// Returns the TypeInfo for specified class.
        /// </summary>
        /// <param name="className">Class name.</param>
        public static ObjectTypeInfo GetTypeInfo(string className)
        {
            if (String.IsNullOrEmpty(className) || IsGeneralDocumentClass(className))
            {
                return TreeNode.TYPEINFO;
            }

            // Try to get from hashtable
            className = className.ToLowerInvariant();


            DynamicTreeNodeTypeInfo existingTypeInfo;
            var exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
            if (!exists || !existingTypeInfo.IsValid)
            {
                lock (TypeInfos.SyncRoot)
                {
                    exists = TypeInfos.TryGetValue(className, out existingTypeInfo);
                    if (!exists || !existingTypeInfo.IsValid)
                    {
                        var newInfo = TreeNodeTypeInfo.CreateDynamicTypeInfo(className);

                        // Copy events from existing one
                        existingTypeInfo?.CopyTreeNodeTypeInfoEventsTo(newInfo);

                        TypeInfos[className] = newInfo;
                        return newInfo;
                    }
                }
            }

            return existingTypeInfo;
        }


        /// <summary>
        /// Clear the class info and properties lists of all object types.
        /// </summary>
        /// <param name="logTask">If true, web farm tasks are logged.</param>
        public static void Clear(bool logTask)
        {
            TypeInfos.Clear();

            if (logTask)
            {
                WebFarmHelper.CreateTask(new ClearDocumentTypeInfosWebFarmTask());
            }
        }


        /// <summary>
        /// Invalidates typeinfo specified by class name.
        /// </summary>
        internal static void InvalidateTypeInfo(string className, bool logTask)
        {
            className = className.ToLowerInvariant();
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
                WebFarmHelper.CreateTask(new InvalidateDocumentTypeInfoWebFarmTask { ClassName = className });
            }
        }


        /// <summary>
        /// Validates given class name if represents existing document type.
        /// </summary>
        /// <param name="className">Class name to validate.</param>
        internal static bool ValidateClass(string className)
        {
            // Provider for generic document is always valid
            if (IsGeneralDocumentClass(className))
            {
                return true;
            }

            var dataClassInfo = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(className);
            var result = dataClassInfo != null && dataClassInfo.ClassIsDocumentType;

            return result;
        }


        /// <summary>
        /// Returns true if the given class name represents a general document.
        /// </summary>
        /// <param name="className">Class name to check.</param>
        internal static bool IsGeneralDocumentClass(string className)
        {
            return string.Equals(className, TreeNode.TYPEINFO.ObjectClassName, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}