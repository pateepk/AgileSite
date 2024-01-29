using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines a column of another object type as a reference (foreign key relationship) pointing to the current object type.
    /// Determines how the system handles dependencies for the reference. For example data integrity during import and staging, or cascading deleting when a referenced object is deleted.
    /// </summary>
    /// <remarks>
    /// Use List collections of ExtraColumn instances to set the <see cref="ObjectTypeInfo.Extends"/> property of <see cref="ObjectTypeInfo"/>.
    /// Each ExtraColumn in the collection registers a foreign key column of another object type as a reference pointing to the current object type.
    /// </remarks>
    public class ExtraColumn : AbstractDataContainer<ExtraColumn>
    {
        /// <summary>
        /// Gets or sets the name of the foreign key column in the object type that is the source of the reference.
        /// </summary>
        [RegisterColumn]
        public string ColumnName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the object type that is the source of the reference.
        /// </summary>
        [RegisterColumn]
        public string ExtendedObjectType
        {
            get;
            set;
        }


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
        /// Creates a new ExtraColumn instance, defining a reference (foreign key relationship) from another object type to the current object type.
        /// </summary>
        /// <param name="extendedObjectType">The object type that is the source of the reference</param>
        /// <param name="columnName">The name of the foreign key column of the source object type that stores the IDs of the referenced objects</param>
        /// <param name="required">Determines how the system handles referential integrity and automatic removal of objects when a referenced object is deleted (Not required reference by default)</param>
        public ExtraColumn(string extendedObjectType, string columnName, ObjectDependencyEnum required = ObjectDependencyEnum.NotRequired)
        {
            ColumnName = columnName;
            ExtendedObjectType = extendedObjectType;
            DependencyType = required;
        }
    }
}