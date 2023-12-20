using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Wrapper for object types collection filtering logic.
    /// </summary>
    public interface IObjectTypeFilter
    {
        /// <summary>
        /// Indicates whether given object type is included in the output.
        /// </summary>
        /// <param name="objectType">Object type name</param>
        bool IsIncludedType(string objectType);


        /// <summary>
        /// Indicates whether given type info is a child processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="childTypeInfo">Child type info</param>
        bool IsChildIncludedToParent(ObjectTypeInfo childTypeInfo);


        /// <summary>
        /// Indicates whether given type info is a binding processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="bindingTypeInfo">Binding type info</param>
        bool IsBindingIncludedToParent(ObjectTypeInfo bindingTypeInfo);
    }
}
