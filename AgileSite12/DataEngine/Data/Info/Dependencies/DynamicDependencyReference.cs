using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Holds information about a reference (foreign key) relationship between two object types.
    /// </summary>
    internal sealed class DynamicDependencyReference : IEquatable<DynamicDependencyReference>
    {
        /// <summary>
        /// Object type that is dependent on type stored in <see cref="DependencyObjectTypeColumnName"/> database column.
        /// </summary>
        public string DependentObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the column that belongs to dependent type and stores object type that is a dependency of type defined in <see cref="DependentObjectType"/>.
        /// </summary>
        /// <remarks>This property is used for dynamic dependencies only.</remarks>
        public string DependencyObjectTypeColumnName
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
        /// Constructor
        /// </summary>
        public DynamicDependencyReference(string dependentType, string dependencyTypeColumn, string dependencyColumn, ObjectDependencyEnum integrityType)
        {
            if (dependentType == null)
            {
                throw new ArgumentNullException("dependentType");
            }

            if (dependencyTypeColumn == null)
            {
                throw new ArgumentNullException("dependencyTypeColumn");
            }

            if (dependencyColumn == null)
            {
                throw new ArgumentNullException("dependencyColumn");
            }

            DependentObjectType = dependentType;
            DependencyObjectTypeColumnName = dependencyTypeColumn;
            DependencyColumnName = dependencyColumn;
            IntegrityType = integrityType;
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

            return Equals((DynamicDependencyReference)obj);     
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(DynamicDependencyReference other)
        {
            return String.Equals(DependencyColumnName, other.DependencyColumnName, StringComparison.InvariantCultureIgnoreCase) &&
                   String.Equals(DependencyObjectTypeColumnName, other.DependencyObjectTypeColumnName, StringComparison.InvariantCultureIgnoreCase) &&
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
                hashCode = (hashCode * 397) ^ (DependencyObjectTypeColumnName != null ? DependencyObjectTypeColumnName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DependentObjectType != null ? DependentObjectType.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
