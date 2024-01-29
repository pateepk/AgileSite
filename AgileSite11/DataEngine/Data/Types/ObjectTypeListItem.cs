using System;
using System.Diagnostics;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Container for object types list item.
    /// </summary>
    [DebuggerDisplay("ObjectType: {ObjectType}; IsSite: {IsSite};")]
    public class ObjectTypeListItem
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the object is site or not.
        /// </summary>
        public bool IsSite
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the object type has any dynamic dependencies
        /// </summary>
        internal bool HasDynamicDependency
        {
            get;
            set;
        }
    }
}