using System;

namespace CMS
{
    /// <summary>
    /// Marks the assembly as discoverable (containing modules or implementations)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class AssemblyDiscoverableAttribute : Attribute
    {
    }
}
