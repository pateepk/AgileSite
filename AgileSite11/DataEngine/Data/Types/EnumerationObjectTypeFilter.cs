using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Filter excluding everything except of given type enumeration.
    /// </summary>
    public class EnumerationObjectTypeFilter : IObjectTypeFilter
    {
        /// <summary>
        /// Object types to be included. Everything else will be filtered out.
        /// </summary>
        public IEnumerable<string> ObjectTypes
        {
            get;
            private set;
        } 


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="types">Object types to include. Everything else will be filtered out.</param>
        public EnumerationObjectTypeFilter(IEnumerable<string> types)
        {
            ObjectTypes = types ?? new List<string>();
        }


        /// <summary>
        /// Returns true if the given type info is included in output.
        /// </summary>
        /// <param name="typeInfo">Object type info</param>
        public bool IsIncludedType(ObjectTypeInfo typeInfo)
        {
            return ObjectTypes.Contains(typeInfo.ObjectType);
        }


        /// <summary>
        /// Indicates whether given type info is a child processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="childTypeInfo">Child type info</param>
        public bool IsChildIncludedToParent(ObjectTypeInfo childTypeInfo)
        {
            return false;
        }


        /// <summary>
        /// Indicates whether given type info is a binding processed together with its parent, hence should be ignored in the output.
        /// </summary>
        /// <param name="bindingTypeInfo">Binding type info</param>
        public bool IsBindingIncludedToParent(ObjectTypeInfo bindingTypeInfo)
        {
            return false;
        }
    }
}
