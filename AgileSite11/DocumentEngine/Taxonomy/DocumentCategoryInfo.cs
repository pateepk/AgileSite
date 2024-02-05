using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Taxonomy;

[assembly: RegisterObjectType(typeof(DocumentCategoryInfo), DocumentCategoryInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// DocumentCategoryInfo data container class.
    /// </summary>
    public class DocumentCategoryInfo : AbstractInfo<DocumentCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.DOCUMENTCATEGORY;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DocumentCategoryInfoProvider), OBJECT_TYPE, "CMS.DocumentCategory", null, null, null, null, null, null, null, "DocumentID", DocumentCultureDataInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.CONTENT,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("CategoryID", CategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            IsBinding = true,
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Category ID.
        /// </summary>
        public virtual int CategoryID
        {
            get
            {
                return GetIntegerValue("CategoryID", 0);
            }
            set
            {
                SetValue("CategoryID", value);
            }
        }


        /// <summary>
        /// Document ID.
        /// </summary>
        public virtual int DocumentID
        {
            get
            {
                return GetIntegerValue("DocumentID", 0);
            }
            set
            {
                SetValue("DocumentID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentCategoryInfoProvider.DeleteDocumentCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DocumentCategoryInfoProvider.SetDocumentCategoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DocumentCategoryInfo object.
        /// </summary>
        public DocumentCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DocumentCategoryInfo object from the given DataRow.
        /// </summary>
        public DocumentCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}