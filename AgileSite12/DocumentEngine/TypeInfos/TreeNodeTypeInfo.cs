using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.Synchronization;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Type info for the tree node
    /// </summary>
    public class TreeNodeTypeInfo : ObjectTypeInfo
    {
        private ClassStructureInfo mClassStructureInfo;

        /// <summary>
        /// Class structure information combining all participating classes.
        /// </summary>
        public override ClassStructureInfo ClassStructureInfo
        {
            get
            {
                if (mClassStructureInfo != null)
                {
                    return mClassStructureInfo;
                }

                return GetClassStructureInfo();
            }
            set
            {
                mClassStructureInfo = value;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="providerType">Provider type</param>
        /// <param name="objectType">Object type</param>
        /// <param name="objectClassName">Object class name</param>
        /// <param name="idColumn">ID column name</param>
        /// <param name="timeStampColumn">Time stamp column name</param>
        /// <param name="guidColumn">GUID column name</param>
        /// <param name="codeNameColumn">Code name column name</param>
        /// <param name="displayNameColumn">Display name column name</param>
        /// <param name="binaryColumn">Binary column name</param>
        /// <param name="siteIDColumn">Site ID column name</param>
        /// <param name="parentIDColumn">Parent ID column name</param>
        /// <param name="parentObjectType">Parent object type</param>
        public TreeNodeTypeInfo(Type providerType, string objectType, string objectClassName, string idColumn, string timeStampColumn, string guidColumn, string codeNameColumn, string displayNameColumn, string binaryColumn, string siteIDColumn, string parentIDColumn, string parentObjectType)
            : base(providerType, objectType, objectClassName, idColumn, timeStampColumn, guidColumn, codeNameColumn, displayNameColumn, binaryColumn, siteIDColumn, parentIDColumn, parentObjectType)
        {
            Initialize(this);
            InitializeSynchronizationSettings();

            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(IntegrationTaskInfo.OBJECT_TYPE, "TaskDocumentID", ObjectDependencyEnum.Required),
                new ExtraColumn(StagingTaskInfo.OBJECT_TYPE, "TaskDocumentID", ObjectDependencyEnum.Required),
            };

            NestedInfoTypes = new List<string>
            {
                DocumentNodeDataInfo.OBJECT_TYPE,
                DocumentCultureDataInfo.OBJECT_TYPE
            };
        }


        /// <summary>
        /// Gets the nice object type name for this type
        /// </summary>
        public override string GetNiceObjectTypeName()
        {
            return TypeHelper.GetNiceObjectTypeName(TreeNode.OBJECT_TYPE);
        }


        /// <summary>
        /// Copies the event's hooks from current ObjectTypeInfo to specified one.
        /// </summary>
        /// <param name="info">Target.</param>
        internal void CopyTreeNodeTypeInfoEventsTo(ObjectTypeInfo info)
        {
            CopyEventsTo(info);
        }


        /// <summary>
        /// Get combined structure definition
        /// </summary>
        private ClassStructureInfo GetClassStructureInfo()
        {
            return ClassStructureInfo.Combine(SystemViewNames.View_CMS_Tree_Joined, DocumentNodeDataInfo.TYPEINFO.ClassStructureInfo, DocumentCultureDataInfo.TYPEINFO.ClassStructureInfo);
        }


        /// <summary>
        /// Initializes node type info with additional settings
        /// </summary>
        /// <param name="typeInfo">TypeInfo to initialize</param>
        internal static void Initialize(ObjectTypeInfo typeInfo)
        {
            // Compose dependencies from partial parts
            typeInfo.DependsOn =
                DocumentNodeDataInfo.TYPEINFO.DependsOn
                    .Union(DocumentCultureDataInfo.TYPEINFO.DependsOn);
            // The following dependencies are in fact foreign keys, but might potentially cause some recursion problems. 
            // For this reason we don't include them at this point.
            //.Union(new[] {
            //    new ObjectDependency("NodeParentID", DocumentNodeDataInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            //    new ObjectDependency("DocumentNodeID", DocumentNodeDataInfo.OBJECT_TYPE, ObjectDependencyEnum.Required)
            //});
            typeInfo.DependsOnIndirectly =
                DocumentNodeDataInfo.TYPEINFO.DependsOnIndirectly
                    .Union(DocumentCultureDataInfo.TYPEINFO.DependsOnIndirectly);

            typeInfo.SynchronizationSettings.IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None;
            typeInfo.SynchronizationSettings.LogSynchronization = SynchronizationTypeEnum.None;

            typeInfo.DeleteAsDependency = false;
            typeInfo.LogEvents = true;
            typeInfo.TouchCacheDependencies = true;
            typeInfo.SupportsVersioning = false;
            typeInfo.SupportsCloning = false;
            typeInfo.AllowRestore = false;

            var ieSettings = typeInfo.ImportExportSettings;

            ieSettings.IncludeToExportParentDataSet = IncludeToParentEnum.None;
            ieSettings.LogExport = false;
            ieSettings.AllowSingleExport = false;
            ieSettings.IsExportable = true;
            ieSettings.ExcludedDependingType = true;

            typeInfo.OrderColumn = "NodeOrder";
            typeInfo.DefaultOrderBy = "DocumentName";
            typeInfo.ObjectPathColumn = "NodeAliasPath";

            var ciSettings = typeInfo.ContinuousIntegrationSettings;

            ciSettings.Enabled = true;
            ciSettings.FilterColumn = "NodeAliasPath";
            ciSettings.IdentificationField = "NodeGUID";
            ciSettings.ObjectFileNameFields.Add("NodeAliasPath");
            ciSettings.ObjectTypeFolderName = TreeNode.OBJECT_TYPE;
        }


        private void InitializeSynchronizationSettings()
        {
            SynchronizationSettings.ExcludedStagingColumns = new List<string>
            {
                "NodeID",
                "NodeSiteID",
                "NodeParentID",
                "NodeACLID",
                "NodeIsACLOwner",
                "NodeHasChildren",
                "NodeHasLinks",
                "DocumentNodeID",
                "DocumentCheckedOutVersionHistoryID",
                "DocumentPublishedVersionHistoryID",
                "DocumentWorkflowStepID",
                "DocumentCheckedOutByUserID",
                "DocumentCheckedOutWhen",
                "DocumentCheckedOutAutomatically",
                "DocumentID",
                "DocumentForeignKeyValue",
                "DocumentRatingValue",
                "DocumentRatings",
                "DocumentIsArchived",
                "DocumentIsWaitingForTranslation"
            };
        }


        /// <summary>
        /// Creates new typeinfo.
        /// </summary>        
        internal static DynamicTreeNodeTypeInfo CreateDynamicTypeInfo(string className)
        {
            string coupledIdColumn = null;

            // Check if the class exists unless general document
            if (!TreeNodeProvider.IsGeneralDocumentClass(className))
            {
                var dci = DataClassInfoProvider.GetDataClassInfo(className);

                // Class must exist
                if (dci == null)
                {
                    throw new DocumentTypeNotExistsException("Page type with '" + className + "' class name not found.");
                }

                // Class must be a page type
                if (!dci.ClassIsDocumentType)
                {
                    throw new DocumentTypeNotExistsException("The '" + className + "' class name is not a page type.");
                }

                if (dci.ClassIsCoupledClass)
                {
                    // Get information about the coupled ID column in case of coupled class
                    var csi = ClassStructureInfo.GetClassInfo(className);
                    if (csi != null)
                    {
                        coupledIdColumn = csi.IDColumn;
                    }
                }
            }

            string objectType = TreeNodeProvider.GetObjectType(className);

            // Create the type info
            var result = new DynamicTreeNodeTypeInfo(objectType);

            // Include coupled ID column to the serialization settings as excluded so that it is not processed by CI
            // This must be done here and not in the DynamicTreeNodeTypeInfo constructor, because the underlying code has reference to CoreServices and AppReset otherwise reports it might cause uncontrolled app preinit
            if (coupledIdColumn != null)
            {
                result.SerializationSettings.ExcludedFieldNames.Add(coupledIdColumn);
            }

            // Initialize new type info object
            ObjectTypeManager.EnsureObjectTypeInfoDynamicList(result);

            return result;
        }
    }
}
