using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Contains flags that determine which types of related object types to load for standard objects in vis.js object type graphs.
    /// </summary>
    [Flags]
    internal enum ObjectTypeGraphScopeEnum
    {
        None = 0,

        // Object types set as the parent directly, or via the RegisterAsChildToObjectTypes collection
        ParentObjects = 1,

        // Child object types that are not bindings
        ChildObjects = 2,

        // Bindings that are registered as child object types
        Bindings = 4,

        // Bindings that include the given object type, but are not its child
        OtherBindings = 8,

        All = ParentObjects | ChildObjects | Bindings | OtherBindings
    }
}
