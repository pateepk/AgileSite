using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(DocumentTypeScopeClassInfo), DocumentTypeScopeClassInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// DocumentTypeScopeClass data container class.
    /// </summary>
    public class DocumentTypeScopeClassInfo : AbstractInfo<DocumentTypeScopeClassInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.documenttypescopeclass";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DocumentTypeScopeClassInfoProvider), OBJECT_TYPE, "CMS.DocumentTypeScopeClass", null, null, null, null, null, null, null, "ScopeID", DocumentTypeScopeInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ClassID", DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, ObjectDependencyEnum.Binding)
            },
            IsBinding = true,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Document type scope identifier.
        /// </summary>
        public virtual int ScopeID
        {
            get
            {
                return GetIntegerValue("ScopeID", 0);
            }
            set
            {
                SetValue("ScopeID", value);
            }
        }


        /// <summary>
        /// Document type identifier.
        /// </summary>
        public virtual int ClassID
        {
            get
            {
                return GetIntegerValue("ClassID", 0);
            }
            set
            {
                SetValue("ClassID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DocumentTypeScopeClassInfoProvider.DeleteScopeClassInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DocumentTypeScopeClassInfoProvider.SetScopeClassInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty DocumentTypeScopeClass object.
        /// </summary>
        public DocumentTypeScopeClassInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new DocumentTypeScopeClass object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public DocumentTypeScopeClassInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scopeId">Document type scope identifier</param>
        /// <param name="classId">Document type class identifier</param>
        public DocumentTypeScopeClassInfo(int scopeId, int classId)
            : base(TYPEINFO)
        {
            ClassID = classId;
            ScopeID = scopeId;
        }

        #endregion
    }
}
