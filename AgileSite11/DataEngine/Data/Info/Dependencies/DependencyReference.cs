using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Holds information about a reference (foreign key) relationship between two object types. 
    /// </summary>
    internal sealed class DependencyReference : IEquatable<DependencyReference>
    {
        /// <summary>
        /// Object type that is dependent on type defined by <see cref="DependencyObjectType"/>.
        /// </summary>
        public string DependentObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Object type that is a dependency of type defined in <see cref="DependentObjectType"/>.
        /// </summary>
        public string DependencyObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the column that belongs to dependent type and stores identifier that belongs to instance of the dependency type.
        /// </summary>
        public string DependencyColumnName
        {
            get;
            private set;
        }


        /// <summary>
        /// Type of referential integrity between object types.
        /// </summary>
        public ObjectDependencyEnum IntegrityType
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns true when dependency is defined using <see cref="ObjectTypeInfo.Extends"/> property.
        /// Returns false when dependency is defined using <see cref="ObjectTypeInfo.DependsOn"/> property.
        /// </summary>
        internal bool IsDependencyByExtension
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public DependencyReference(string dependentType, string dependencyType, string dependencyColumn, ObjectDependencyEnum integrityType, bool isDependencyByExtension = false)
        {
            if (dependentType == null)
            {
                throw new ArgumentNullException("dependentType");
            }

            if (dependencyType == null)
            {
                throw new ArgumentNullException("dependencyType");
            }

            if (dependencyColumn == null)
            {
                throw new ArgumentNullException("dependencyColumn");
            }

            DependentObjectType = dependentType;
            DependencyObjectType = dependencyType;
            DependencyColumnName = dependencyColumn;
            IntegrityType = integrityType;
            IsDependencyByExtension = isDependencyByExtension;
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
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

            return Equals((DependencyReference)obj);       
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(DependencyReference other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return String.Equals(DependencyColumnName, other.DependencyColumnName, StringComparison.InvariantCultureIgnoreCase) &&
                   String.Equals(DependencyObjectType, other.DependencyObjectType, StringComparison.InvariantCultureIgnoreCase) &&
                   other.IsDependencyByExtension == IsDependencyByExtension &&
                   String.Equals(DependentObjectType, other.DependentObjectType, StringComparison.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Gets the object hash code
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (DependencyColumnName != null ? DependencyColumnName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DependencyObjectType != null ? DependencyObjectType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DependentObjectType != null ? DependentObjectType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsDependencyByExtension.GetHashCode();
                return hashCode;
            }
        }
    }
}
