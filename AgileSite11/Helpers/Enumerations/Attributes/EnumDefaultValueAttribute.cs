using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Marks the enum value as the default value for the containing enum type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class EnumDefaultValueAttribute : Attribute
    {
    }
}
