using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(AllowedChildClassInfo), AllowedChildClassInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// AllowedChildClass data container class.
    /// </summary>
    public class AllowedChildClassInfo : AbstractInfo<AllowedChildClassInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.ALLOWEDCHILDCLASS;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AllowedChildClassInfoProvider), OBJECT_TYPE, "CMS.AllowedChildClass", null, null, null, null, null, null, null, "ChildClassID", DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ParentClassID", DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, ObjectDependencyEnum.Binding)
            },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Child class ID.
        /// </summary>
        public virtual int ChildClassID
        {
            get
            {
                return GetIntegerValue("ChildClassID", 0);
            }
            set
            {
                SetValue("ChildClassID", value);
            }
        }


        /// <summary>
        /// Parent class ID.
        /// </summary>
        public virtual int ParentClassID
        {
            get
            {
                return GetIntegerValue("ParentClassID", 0);
            }
            set
            {
                SetValue("ParentClassID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AllowedChildClassInfoProvider.DeleteAllowedChildClassInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AllowedChildClassInfoProvider.SetAllowedChildClassInfo(this);
        }


        /// <summary>
        /// Updates the parent object by saving it (to update the timestamp).
        /// </summary>
        protected override void TouchParent()
        {
            base.TouchParent();

            // Update also other class
            var dci = DataClassInfoProvider.GetDataClassInfo(ParentClassID);
            if (dci != null)
            {
                dci.Generalized.SetObject();
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty AllowedChildClass object.
        /// </summary>
        public AllowedChildClassInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new AllowedChildClass object from the given DataRow.
        /// </summary>
        public AllowedChildClassInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}