using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Taxonomy;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(DocumentTagInfo), DocumentTagInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// DocumentTagInfo data container class.
    /// </summary>
    public class DocumentTagInfo : AbstractInfo<DocumentTagInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.documenttag";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DocumentTagInfoProvider), OBJECT_TYPE, "CMS.DocumentTag", null, null, null, null, null, null, null, "DocumentID", PredefinedObjectType.DOCUMENTLOCALIZATION)
                                              {
                                                  ModuleName = ModuleName.CONTENT,

                                                  DependsOn = new List<ObjectDependency>
                                                  {
                                                    new ObjectDependency("TagID", TagInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
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
                                                  ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
                                              };

        #endregion


        #region "Properties"

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


        /// <summary>
        /// Tag ID.
        /// </summary>
        public virtual int TagID
        {
            get
            {
                return GetIntegerValue("TagID", 0);
            }
            set
            {
                SetValue("TagID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentTagInfoProvider.DeleteDocumentTagInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DocumentTagInfoProvider.SetDocumentTagInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DocumentTagInfo object.
        /// </summary>
        public DocumentTagInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DocumentTagInfo object from the given DataRow.
        /// </summary>
        public DocumentTagInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}