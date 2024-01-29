using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.DocumentEngine.Taxonomy;
using CMS.Helpers;
using CMS.Search;
using CMS.Search.Azure;
using CMS.Search.Internal;
using CMS.Taxonomy;

[assembly: RegisterModule(typeof(DocumentEngineModule))]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents the Document Engine module.
    /// </summary>
    public class DocumentEngineModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DocumentEngineModule()
            : base(new DocumentEngineModuleMetadata())
        {
        }


        #region "Module methods"

        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();

            InfoProviderLoader.LoadProvider += AbstractProvider_LoadProvider;

            // Init document type info
            DataClassInfo.ReplaceWith<DocumentTypeInfo>().WhenColumnValue(DataClassInfo.TYPEINFO.ObjectClassName, "ClassIsDocumentType", v => ValidationHelper.GetBoolean(v, false));
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            DocumentHandlers.Init();
            DocumentTypeHandlers.Init();
            AdhocRelationshipNameHandlers.Init();
            DocumentSynchronization.Init();
            ContinuousIntegrationHandlers.Init();
            DocumentMacros.Init();
            SiteDeletion.Init();
            PageInfoHandlers.Init();
            DocumentTagHandlers.Init();
            VersioningHandlers.Init();

            InitExport();

            RegisterContext<DocumentContext>("DocumentContext");
            RegisterContext<CMSContext>("CMSContext");

            RegisterIndexers();

            DocumentFieldDataType.RegisterDataTypes();

            DocumentFieldCreator.Instance.CreatingField.Before += DocumentCategorySearchFieldsProvider.MapAzureSearchDocumentCategories;
            DocumentFieldCreator.Instance.CreatingFields.After += DocumentSearchFieldsProvider.AddNodeAliasPathPrefixesField;

            DocumentCreator.Instance.AddingDocumentValue.Execute += DocumentCategorySearchFieldsProvider.ConvertAzureSearchDocumentCategories;
            DocumentCreator.Instance.CreatingDocument.After += DocumentSearchFieldsProvider.DivideNodeAliasPathIntoNodeAliasPathPrefixes;
        }
        

        /// <summary>
        /// Initializes the Import/Export handlers
        /// </summary>
        private static void InitExport()
        {
            DocumentTypeExport.Init();
            DocumentTypeImport.Init();

            DocumentExport.Init();
            DocumentImport.Init();

            AttachmentExport.Init();
            AttachmentImport.Init();

            AttachmentHistoryExport.Init();
            AttachmentHistoryImport.Init();

            ImportSpecialActions.Init();
            ExportSpecialActions.Init();
        }


        /// <summary>
        /// Registers the search indexers and searchables retrievers for documents
        /// </summary>
        private static void RegisterIndexers()
        {
            SearchIndexers.RegisterIndexer<DocumentSearchIndexer>(TreeNode.OBJECT_TYPE);
            SearchIndexers.RegisterIndexer<DocumentSearchIndexer>(SearchHelper.DOCUMENTS_CRAWLER_INDEX);
            SearchIndexers.RegisterIndexer<DocumentCategorySearchIndexer>(CategoryInfo.OBJECT_TYPE);
            
            SearchablesRetrievers.Register<DocumentSearchablesRetriever>(TreeNode.OBJECT_TYPE);
            SearchablesRetrievers.Register<DocumentSearchablesRetriever>(SearchHelper.DOCUMENTS_CRAWLER_INDEX);
            SearchablesRetrievers.Register<DocumentSearchablesRetriever>(CategoryInfo.OBJECT_TYPE);
        }


        private static void AbstractProvider_LoadProvider(object sender, LoadProviderEventArgs e)
        {
            var objectType = e.ObjectType;
            if (DocumentFieldsInfoProvider.IsDocumentFieldsObjectType(objectType))
            {
                DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(DocumentFieldsInfoProvider.GetClassName(objectType));
                e.Provider = (dataClass != null) ? DocumentFieldsInfoProvider.LoadProviderInternal(objectType) : null;
                e.ProviderLoaded = true;
            }
            else if (TreeNodeProvider.IsDocumentObjectType(objectType))
            {
                if (objectType.EqualsCSafe(TreeNode.OBJECT_TYPE))
                {
                    // General TreeNode provider
                    e.Provider = TreeNodeProvider.LoadProviderInternal(objectType);
                }
                else
                {
                    // Specific document type provider
                    var className = TreeNodeProvider.GetClassName(objectType);
                    var dataClass = DataClassInfoProvider.GetDataClassInfo(className);
                    e.Provider = (dataClass != null) ? TreeNodeProvider.LoadProviderInternal(objectType) : null;
                }

                e.ProviderLoaded = true;
            }
        }

        #endregion


        #region "General methods"

        /// <summary>
        /// Clears the module hash tables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            DocumentTypeHelper.ClearClassNames(logTasks);
        }


        /// <summary>
        /// Gets the object created from the given DataRow.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public override BaseInfo GetObject(string objectType)
        {
            if (objectType != null)
            {
                objectType = objectType.ToLowerCSafe();

                // Page fields object type
                if (DocumentFieldsInfoProvider.IsDocumentFieldsObjectType(objectType))
                {
                    var className = DocumentFieldsInfoProvider.GetClassName(objectType);
                    var dataClass = DataClassInfoProvider.GetDataClassInfo(className);
                    if (dataClass != null)
                    {
                        return DocumentFieldsInfo.New(className);
                    }
                }
                // Page object type
                else if (objectType.StartsWithCSafe(DocumentHelper.DOCUMENT_PREFIX))
                {
                    var className = TreeNodeProvider.GetClassName(objectType);
                    DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(className);
                    if (dataClass != null)
                    {
                        return TreeNode.New(className);
                    }
                }
            }

            return null;
        }

        #endregion
    }
}