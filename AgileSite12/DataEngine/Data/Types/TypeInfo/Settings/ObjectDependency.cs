using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a reference (foreign key relationship) from one object type to another. 
    /// Defines how the system handles dependencies for the reference. For example data integrity during import and staging, or cascading deleting when a referenced object is deleted.
    /// </summary>
    /// <remarks>
    /// Use List collections of ObjectDependency instances to set the <see cref="ObjectTypeInfo.DependsOn"/> property of <see cref="ObjectTypeInfo"/>.
    /// Each ObjectDependency in the collection registers a foreign key field as a reference pointing from the given object type to another.
    /// </remarks>
    public class ObjectDependency : ObjectReference
    {
        #region "Properties"
        
        /// <summary>
        /// Determines how the system handles referential integrity and automatic removal of objects when a referenced object is deleted.
        /// </summary>
        [RegisterColumn]
        public ObjectDependencyEnum DependencyType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the data column that stores the target object type of the reference (for objects without FK database restrictions).
        /// For example ObjectSettings or Metafiles where the object dependency is a combination of MetaFileObjectType and MetaFileObjectID.
        /// </summary>
        [RegisterColumn]
        public string ObjectTypeColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true when dependency is defined using <see cref="ObjectTypeInfo.Extends"/> property.
        /// Returns false when dependency is defined using <see cref="ObjectTypeInfo.DependsOn"/> property.
        /// </summary>
        [RegisterColumn]
        internal bool IsDependencyByExtension
        {
            get;
            private set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new ObjectDependency instance, defining a reference (foreign key relationship) from one object type to another.
        /// </summary>
        /// <param name="dependencyColumn">The name of the foreign key column that stores the IDs of the referenced objects</param>
        /// <param name="dependencyObjectType">The object type that is the target of the reference (null if the target object type is stored in a data column - use the fourth param to specify the field name)</param>
        /// <param name="required">Determines how the system handles referential integrity and automatic removal of objects when a referenced object is deleted (Not required reference by default)</param>
        /// <param name="objectTypeColumn">The name of the column that stores the target object type of the reference (null for standard references)</param>
        public ObjectDependency(string dependencyColumn, string dependencyObjectType, ObjectDependencyEnum required = ObjectDependencyEnum.NotRequired, string objectTypeColumn = null)
            : this(dependencyColumn, dependencyObjectType, false, required, objectTypeColumn)
        {
        }


        /// <summary>
        /// Creates a new ObjectDependency instance, defining a reference (foreign key relationship) from one object type to another.
        /// </summary>
        /// <param name="dependencyColumn">The name of the foreign key column that stores the IDs of the referenced objects</param>
        /// <param name="dependencyObjectType">The object type that is the target of the reference (null if the target object type is stored in a data column - use the fourth param to specify the field name)</param>
        /// <param name="isDependencyByExtension">Determines how is dependency defined, should be true for dependencies defined using <see cref="ObjectTypeInfo.Extends"/> property</param>
        /// <param name="required">Determines how the system handles referential integrity and automatic removal of objects when a referenced object is deleted (Not required reference by default)</param>
        /// <param name="objectTypeColumn">The name of the column that stores the target object type of the reference (null for standard references)</param>
        internal ObjectDependency(string dependencyColumn, string dependencyObjectType, bool isDependencyByExtension, ObjectDependencyEnum required = ObjectDependencyEnum.NotRequired, string objectTypeColumn = null)
            : base(dependencyColumn, dependencyObjectType)
        {
            DependencyType = required;
            ObjectTypeColumn = objectTypeColumn;
            IsDependencyByExtension = isDependencyByExtension;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns true if the dependency is required. Required dependencies are of type Required, RequiredHasDefault or Binding.
        /// </summary>
        internal bool IsRequired()
        {
            switch (DependencyType)
            {
                case ObjectDependencyEnum.Required:
                case ObjectDependencyEnum.RequiredHasDefault:
                case ObjectDependencyEnum.Binding:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Returns the hash code of this object dependency.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var hashCode = (DependencyColumn != null ? DependencyColumn.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (DependencyObjectType != null ? DependencyObjectType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int)DependencyType;
            hashCode = (hashCode * 397) ^ (ObjectTypeColumn != null ? ObjectTypeColumn.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ IsDependencyByExtension.GetHashCode();
            return hashCode;
        }


        /// <summary>
        /// Compares the dependency with another object. Returns true if all fields of the two dependencies match.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ObjectDependency)obj);
        }


        /// <summary>
        /// Compares the dependency with another ObjectDependency object. Returns true if all fields of the two dependencies match.
        /// </summary>
        /// <param name="other">Object to compare</param>
        protected bool Equals(ObjectDependency other)
        {
            return string.Equals(DependencyColumn, other.DependencyColumn, StringComparison.Ordinal)
                && string.Equals(DependencyObjectType, other.DependencyObjectType, StringComparison.Ordinal)
                && (DependencyType == other.DependencyType)
                && string.Equals(ObjectTypeColumn, other.ObjectTypeColumn, StringComparison.Ordinal)
                && other.IsDependencyByExtension == IsDependencyByExtension;
        }


        /// <summary>
        /// Returns true if the dependency is referencing a dynamic object type.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dynamic object type reference lacks any specific <see cref="ObjectReference.DependencyObjectType"/>. It has <see cref="ObjectTypeColumn"/> defined instead.
        /// </para>
        /// <para>
        /// Dynamic object type can reference various object types (e.g MetaFiles have column MetaFileObjectType which can hold different object types).
        /// </para>
        /// </remarks>
        public bool HasDynamicObjectType()
        {
            return (DependencyObjectType == null) && (ObjectTypeColumn != null);
        }

        #endregion
    }
}