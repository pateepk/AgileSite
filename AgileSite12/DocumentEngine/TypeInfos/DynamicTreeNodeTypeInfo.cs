using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Type info for the tree node of specific type
    /// </summary>
    public class DynamicTreeNodeTypeInfo : DynamicObjectTypeInfo
    {
        /// <summary>
        /// Base tree node type info
        /// </summary>
        private static readonly ObjectTypeInfo baseInfo = TreeNode.TYPEINFO;


        /// <summary>
        /// Class structure information combining all participating main classes.
        /// </summary>
        public override ClassStructureInfo ClassStructureInfo
        {
            get
            {
                return baseInfo.ClassStructureInfo;
            }
            set
            {
            }
        }


        /// <summary>
        /// Gets list of child object types.
        /// </summary>
        [RegisterProperty]
        public override List<string> ChildObjectTypes
        {
            get
            {
                if (OriginalTypeInfo != null)
                {
                    return OriginalTypeInfo.ChildObjectTypes;
                }

                return base.ChildObjectTypes;
            }
        }


        /// <summary>
        /// Gets list of binding object types.
        /// </summary>
        [RegisterProperty]
        public override List<string> BindingObjectTypes
        {
            get
            {
                if (OriginalTypeInfo != null)
                {
                    return OriginalTypeInfo.BindingObjectTypes;
                }

                return base.BindingObjectTypes;
            }
        }


        /// <summary>
        /// Gets list of other binding types - binding types where this object participate but is not parent object of those binding types.
        /// </summary>
        [RegisterProperty]
        public override List<string> OtherBindingObjectTypes
        {
            get
            {
                if (OriginalTypeInfo != null)
                {
                    return OriginalTypeInfo.OtherBindingObjectTypes;
                }

                return base.OtherBindingObjectTypes;
            }
        }


        /// <summary>
        /// Defines the list of object types that are part of this wrapper object. If the object is not a wrapper, this property is not initialized.
        /// </summary>
        [RegisterProperty]
        public override ICollection<string> ConsistsOf
        {
            get
            {
                var consists = OriginalTypeInfo != null ? OriginalTypeInfo.ConsistsOf : base.ConsistsOf;
                return new List<string>(consists ?? Enumerable.Empty<string>()) { ObjectType };
            }
            set
            {
                base.ConsistsOf = value;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectType">Object type of the dynamic type</param>
        public DynamicTreeNodeTypeInfo(string objectType)
            : base(baseInfo.ProviderType, objectType, baseInfo.ObjectClassName, baseInfo.IDColumn, baseInfo.TimeStampColumn, baseInfo.GUIDColumn, baseInfo.CodeNameColumn, baseInfo.DisplayNameColumn, baseInfo.BinaryColumn, baseInfo.SiteIDColumn, baseInfo.ParentIDColumn, baseInfo.ParentObjectType)
        {
            OriginalTypeInfo = baseInfo;

            TreeNodeTypeInfo.Initialize(this);
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
    }
}
