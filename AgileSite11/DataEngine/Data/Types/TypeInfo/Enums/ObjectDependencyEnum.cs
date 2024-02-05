using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Stores options for handling the referential integrity of references between object types (foreign keys).
    /// </summary>
    /// <remarks>    
    /// Use for the value of the 'required' parameter when creating <see cref="ObjectDependency"/> or <see cref="ExtraColumn"/> objects within type information definitions.
    /// </remarks>
    public enum ObjectDependencyEnum
    {
        /// <summary>
        /// Required reference without a default value. For objects that do not make sense without having the reference set.
        /// If the referenced object is deleted, the system automatically deletes the entire object containing the reference.
        /// If the target object does not exist during import or staging, the operation is cancelled for the given object.
        /// </summary>
        [EnumStringRepresentation("Required")]
        Required,

        /// <summary>        
        /// If the referenced object is deleted or does not exist on the target instance during import or staging, the system automatically assigns a default object.
        /// To set the default object, override the <see cref="BaseInfo.GetDefaultObject()"/> method in the code of the given Info class.
        /// </summary>
        [EnumStringRepresentation("RequiredHasDefault")]
        RequiredHasDefault,

        /// <summary>
        /// Represents an optional reference. Use for objects that make sense without having the reference set.
        /// The reference column must support null values.
        /// If the referenced object is deleted or does not exist on the target instance during import or staging, the system sets a null value for the reference.
        /// </summary>
        [EnumStringRepresentation("NotRequired")]
        NotRequired,

        /// <summary>
        /// Use for the reference fields of dedicated binding classes (classes that only represent relationships between other classes).
        /// Binding references are required. If the referenced object is deleted, the system automatically deletes the entire binding record.
        /// If the target object does not exist during import or staging, the operation is cancelled for the given binding record.
        /// </summary>
        [EnumStringRepresentation("Binding")]
        Binding
    }
}